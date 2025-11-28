package agent

import (
	"context"
	"encoding/json"
	"fmt"
	"os"
	"runtime"
	"sync"
	"time"

	"github.com/eracloud/era-monitor-agent/internal/api"
	"github.com/eracloud/era-monitor-agent/internal/collectors/eventlog"
	"github.com/eracloud/era-monitor-agent/internal/collectors/service"
	"github.com/eracloud/era-monitor-agent/internal/collectors/system"
	"github.com/eracloud/era-monitor-agent/internal/config"
	"github.com/go-resty/resty/v2"
	"go.uber.org/zap"
)

type Agent struct {
	cfg               *config.Config
	logger            *zap.Logger
	systemCollector   *system.SystemCollector
	eventLogCollector *eventlog.Collector
	serviceMonitors   []service.Monitor
	client            *resty.Client

	// State
	mu          sync.RWMutex
	isRunning   bool
	lastMetrics *api.HeartbeatRequest
	lastError   error
	lastSentAt  time.Time
}

var (
	agentVersion = getEnv("ERA_AGENT_VERSION", "0.1.0")
	agentBuild   = os.Getenv("ERA_AGENT_BUILD")
)

type AgentStatus struct {
	IsRunning   bool
	LastSentAt  time.Time
	LastError   error
	LastMetrics *api.HeartbeatRequest
}

func NewAgent(cfg *config.Config, logger *zap.Logger) *Agent {
	client := resty.New()
	client.SetBaseURL(cfg.Server.APIEndpoint)
	client.SetTimeout(time.Duration(cfg.Server.Timeout) * time.Second)
	client.SetRetryCount(cfg.Server.RetryCount)
	client.SetRetryWaitTime(time.Duration(cfg.Server.RetryDelay) * time.Second)

	a := &Agent{
		cfg:             cfg,
		logger:          logger,
		systemCollector: system.NewSystemCollector(cfg.Collectors.System),
		client:          client,
	}

	// Initialize Event Log Collector (Windows only)
	if runtime.GOOS == "windows" {
		a.eventLogCollector = eventlog.NewCollector(cfg.Collectors.System.EventLog)
	}

	// Initialize Service Monitors
	a.initServiceMonitors()

	return a
}

func (a *Agent) initServiceMonitors() {
	if a.cfg.Services.Windows.Enabled {
		if mon, err := service.NewWindowsMonitor(a.cfg.Services.Windows.Services); err == nil {
			a.serviceMonitors = append(a.serviceMonitors, mon)
		} else {
			a.logger.Warn("Failed to initialize Windows service monitor", zap.Error(err))
		}
	}

	if a.cfg.Services.Systemd.Enabled {
		if mon, err := service.NewSystemdMonitor(a.cfg.Services.Systemd.Units); err == nil {
			a.serviceMonitors = append(a.serviceMonitors, mon)
		} else {
			a.logger.Warn("Failed to initialize Systemd monitor", zap.Error(err))
		}
	}

	if a.cfg.Services.Docker.Enabled {
		if mon, err := service.NewDockerMonitor(a.cfg.Services.Docker.Containers); err == nil {
			a.serviceMonitors = append(a.serviceMonitors, mon)
		} else {
			a.logger.Warn("Failed to initialize Docker monitor", zap.Error(err))
		}
	}
}

func (a *Agent) Run(ctx context.Context) error {
	a.mu.Lock()
	a.isRunning = true
	a.mu.Unlock()

	defer func() {
		a.mu.Lock()
		a.isRunning = false
		a.mu.Unlock()
	}()

	a.logger.Info("Starting ERA Monitor Agent",
		zap.String("hostname", a.cfg.Host.DisplayName),
		zap.String("server", a.cfg.Server.APIEndpoint),
	)

	ticker := time.NewTicker(time.Duration(a.cfg.Collectors.IntervalSeconds) * time.Second)
	defer ticker.Stop()

	// Initial collection
	if err := a.collectAndSend(ctx); err != nil {
		a.logger.Error("Initial collection failed", zap.Error(err))
	}

	for {
		select {
		case <-ctx.Done():
			a.logger.Info("Agent stopping...")
			return nil
		case <-ticker.C:
			if err := a.collectAndSend(ctx); err != nil {
				a.logger.Error("Collection cycle failed", zap.Error(err))
			}
		}
	}
}

func (a *Agent) collectAndSend(ctx context.Context) error {
	a.logger.Debug("Starting collection cycle")

	// Collect System Metrics
	sysResult, err := a.systemCollector.Collect(ctx)
	if err != nil {
		a.setError(err)
		return fmt.Errorf("failed to collect system metrics: %w", err)
	}

	// Collect Service Metrics
	var services []api.ServiceInfo
	for _, mon := range a.serviceMonitors {
		svcList, err := mon.GetServices()
		if err != nil {
			a.logger.Warn("Service monitor error", zap.String("monitor", mon.Name()), zap.Error(err))
			continue
		}
		services = append(services, svcList...)
	}

	// Map System Metrics to API Model
	systemInfo := api.SystemInfo{
		Hostname:      sysResult.System.Hostname,
		OSType:        sysResult.System.OS,
		OSVersion:     sysResult.System.PlatformVersion,
		CPUPercent:    sysResult.System.CPUPercent,
		RAMPercent:    sysResult.System.RAMPercent,
		RAMUsedMB:     sysResult.System.RAMUsedMB,
		RAMTotalMB:    sysResult.System.RAMTotalMB,
		UptimeSeconds: sysResult.System.UptimeSeconds,
		ProcessCount:  sysResult.System.ProcessCount,
	}

	var disks []api.DiskInfo
	for _, d := range sysResult.Disks {
		disks = append(disks, api.DiskInfo{
			Name:        d.Name,
			MountPoint:  d.MountPoint,
			FileSystem:  d.FileSystem,
			TotalGB:     d.TotalGB,
			UsedGB:      d.UsedGB,
			UsedPercent: d.UsedPercent,
		})
	}

	var metadata *api.AgentMetadata
	if agentVersion != "" || agentBuild != "" {
		metadata = &api.AgentMetadata{
			Version:   agentVersion,
			BuildHash: agentBuild,
			Platform:  runtime.GOOS,
		}
	}

	// Prepare Request
	request := &api.HeartbeatRequest{
		SystemInfo: systemInfo,
		Disks:      disks,
		Services:   services,
		Timestamp:  time.Now().UTC(),
		AgentInfo:  metadata,
	}

	if sysResult.Network != nil {
		request.NetworkInfo = &api.NetworkInfo{
			PrimaryIP: sysResult.Network.PrimaryIP,
			PublicIP:  sysResult.Network.PublicIP,
			InBytes:   sysResult.Network.InBytes,
			OutBytes:  sysResult.Network.OutBytes,
		}
	}

	// Collect Event Logs (Windows only)
	if a.eventLogCollector != nil {
		eventLogs, err := a.eventLogCollector.Collect()
		if err == nil && len(eventLogs) > 0 {
			// Convert eventlog.EventInfo to api.EventLogInfo
			apiEventLogs := make([]api.EventLogInfo, len(eventLogs))
			for i, log := range eventLogs {
				apiEventLogs[i] = api.EventLogInfo{
					LogName:     log.LogName,
					EventID:     log.EventID,
					Level:       log.Level,
					Source:      log.Source,
					Message:     log.Message,
					TimeCreated: log.TimeCreated,
					Category:    log.Category,
				}
			}
			request.EventLogs = apiEventLogs
			a.logger.Debug("Collected event logs", zap.Int("count", len(eventLogs)))
		} else if err != nil {
			a.logger.Warn("Failed to collect event logs", zap.Error(err))
		}
	}

	a.mu.Lock()
	a.lastMetrics = request
	a.mu.Unlock()

	// Log Payload for Debugging
	payloadBytes, _ := json.MarshalIndent(request, "", "  ")
	a.logger.Info("Sending Heartbeat Payload", zap.String("payload", string(payloadBytes)))

	// Send to API
	resp, err := a.client.R().
		SetContext(ctx).
		SetHeader("X-API-Key", a.cfg.Server.APIKey).
		SetHeader("Content-Type", "application/json").
		SetBody(request).
		Post("/agent/heartbeat")

	if err != nil {
		a.setError(err)
		return fmt.Errorf("failed to send heartbeat: %w", err)
	}

	if resp.IsError() {
		err := fmt.Errorf("server returned error: %s. Body: %s", resp.Status(), resp.String())
		a.setError(err)
		return err
	}

	a.mu.Lock()
	a.lastError = nil
	a.lastSentAt = time.Now()
	a.mu.Unlock()

	a.logger.Info("Heartbeat sent successfully",
		zap.Float64("cpu", systemInfo.CPUPercent),
		zap.Float64("ram", systemInfo.RAMPercent),
		zap.Int("services", len(services)),
	)

	return nil
}

func (a *Agent) setError(err error) {
	a.mu.Lock()
	a.lastError = err
	a.mu.Unlock()
}

func (a *Agent) Status() AgentStatus {
	a.mu.RLock()
	defer a.mu.RUnlock()

	return AgentStatus{
		IsRunning:   a.isRunning,
		LastSentAt:  a.lastSentAt,
		LastError:   a.lastError,
		LastMetrics: a.lastMetrics,
	}
}

// ForceHeartbeat triggers an immediate heartbeat collection and send
func (a *Agent) ForceHeartbeat(ctx context.Context) error {
	return a.collectAndSend(ctx)
}

func getEnv(key, fallback string) string {
	if value := os.Getenv(key); value != "" {
		return value
	}
	return fallback
}
