PHASE 7: Go Agent Development (Days 31-40)
7.1 Overview
Phase 7 focuses on implementing the Go-based monitoring agent:

Cross-platform agent (Windows, Linux, macOS)
System metrics collection (CPU, RAM, Disk, Network)
Service monitoring (Windows Services, Systemd, Docker, IIS)
Heartbeat communication with API
Auto-update mechanism
Fyne GUI for Windows (system tray)
Configuration management
Secure API key storage
Logging and diagnostics


7.2 Project Structure
era-agent/
├── cmd/
│   ├── agent/
│   │   └── main.go              # CLI agent entry point
│   └── agent-gui/
│       └── main.go              # GUI agent entry point (Windows)
├── internal/
│   ├── agent/
│   │   ├── agent.go             # Main agent logic
│   │   ├── config.go            # Configuration management
│   │   └── heartbeat.go         # Heartbeat sender
│   ├── collector/
│   │   ├── collector.go         # Collector interface
│   │   ├── cpu.go               # CPU metrics
│   │   ├── memory.go            # Memory metrics
│   │   ├── disk.go              # Disk metrics
│   │   ├── network.go           # Network metrics
│   │   └── system.go            # System info
│   ├── service/
│   │   ├── service.go           # Service monitor interface
│   │   ├── windows.go           # Windows services
│   │   ├── systemd.go           # Systemd units
│   │   ├── docker.go            # Docker containers
│   │   └── iis.go               # IIS sites/app pools
│   ├── api/
│   │   ├── client.go            # API client
│   │   └── models.go            # API models
│   ├── updater/
│   │   └── updater.go           # Auto-update logic
│   └── gui/
│       ├── app.go               # Fyne GUI application
│       ├── tray.go              # System tray
│       └── views/
│           ├── main.go          # Main window
│           ├── settings.go      # Settings view
│           └── logs.go          # Logs view
├── pkg/
│   └── logger/
│       └── logger.go            # Logging utilities
├── configs/
│   └── config.example.yaml      # Example configuration
├── scripts/
│   ├── build.sh                 # Build script
│   ├── install-windows.ps1      # Windows installer
│   └── install-linux.sh         # Linux installer
├── go.mod
├── go.sum
└── README.md

7.3 Configuration
yaml# configs/config.example.yaml

# ERA Monitor Agent Configuration
server:
  url: "https://api.eramonitor.com"
  api_key: "your-api-key-here"
  timeout: 30s
  retry_count: 3
  retry_delay: 5s

agent:
  name: ""  # Auto-detected if empty
  check_interval: 60s
  log_level: "info"  # debug, info, warn, error
  log_file: ""  # Empty for stdout, or path to file

collectors:
  cpu:
    enabled: true
    interval: 60s
  memory:
    enabled: true
    interval: 60s
  disk:
    enabled: true
    interval: 60s
    mount_points: []  # Empty for all, or specific paths
    exclude_fs_types:
      - "tmpfs"
      - "devtmpfs"
      - "squashfs"
  network:
    enabled: true
    interval: 60s
    interfaces: []  # Empty for all

services:
  windows:
    enabled: true
    services: []  # Empty for auto-detect, or specific service names
  systemd:
    enabled: true
    units: []  # Empty for auto-detect, or specific unit names
  docker:
    enabled: true
    containers: []  # Empty for all, or specific container names/IDs
  iis:
    enabled: true
    sites: []  # Empty for all
    app_pools: []  # Empty for all

updater:
  enabled: true
  check_interval: 1h
  auto_update: false  # Require manual approval

gui:
  enabled: true
  start_minimized: true
  show_notifications: true

7.4 Core Agent Implementation
Main Entry Point
go// cmd/agent/main.go

package main

import (
	"context"
	"flag"
	"fmt"
	"os"
	"os/signal"
	"syscall"

	"github.com/eracloud/era-agent/internal/agent"
	"github.com/eracloud/era-agent/pkg/logger"
)

var (
	version   = "dev"
	buildTime = "unknown"
	commit    = "unknown"
)

func main() {
	// Parse flags
	configPath := flag.String("config", "", "Path to configuration file")
	apiKey := flag.String("api-key", "", "API key (overrides config)")
	serverURL := flag.String("server", "", "Server URL (overrides config)")
	showVersion := flag.Bool("version", false, "Show version and exit")
	flag.Parse()

	if *showVersion {
		fmt.Printf("ERA Monitor Agent v%s\nBuild: %s\nCommit: %s\n", version, buildTime, commit)
		os.Exit(0)
	}

	// Initialize logger
	log := logger.New("info")
	log.Info("ERA Monitor Agent starting", "version", version)

	// Load configuration
	cfg, err := agent.LoadConfig(*configPath)
	if err != nil {
		log.Error("Failed to load configuration", "error", err)
		os.Exit(1)
	}

	// Override with flags
	if *apiKey != "" {
		cfg.Server.APIKey = *apiKey
	}
	if *serverURL != "" {
		cfg.Server.URL = *serverURL
	}

	// Validate configuration
	if cfg.Server.APIKey == "" {
		log.Error("API key is required")
		os.Exit(1)
	}

	// Create agent
	a, err := agent.New(cfg, log)
	if err != nil {
		log.Error("Failed to create agent", "error", err)
		os.Exit(1)
	}

	// Setup graceful shutdown
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	sigChan := make(chan os.Signal, 1)
	signal.Notify(sigChan, syscall.SIGINT, syscall.SIGTERM)

	go func() {
		sig := <-sigChan
		log.Info("Received signal, shutting down", "signal", sig)
		cancel()
	}()

	// Start agent
	if err := a.Run(ctx); err != nil {
		log.Error("Agent error", "error", err)
		os.Exit(1)
	}

	log.Info("Agent stopped")
}
Configuration Management
go// internal/agent/config.go

package agent

import (
	"os"
	"path/filepath"
	"runtime"
	"time"

	"gopkg.in/yaml.v3"
)

type Config struct {
	Server     ServerConfig     `yaml:"server"`
	Agent      AgentConfig      `yaml:"agent"`
	Collectors CollectorsConfig `yaml:"collectors"`
	Services   ServicesConfig   `yaml:"services"`
	Updater    UpdaterConfig    `yaml:"updater"`
	GUI        GUIConfig        `yaml:"gui"`
}

type ServerConfig struct {
	URL        string        `yaml:"url"`
	APIKey     string        `yaml:"api_key"`
	Timeout    time.Duration `yaml:"timeout"`
	RetryCount int           `yaml:"retry_count"`
	RetryDelay time.Duration `yaml:"retry_delay"`
}

type AgentConfig struct {
	Name          string        `yaml:"name"`
	CheckInterval time.Duration `yaml:"check_interval"`
	LogLevel      string        `yaml:"log_level"`
	LogFile       string        `yaml:"log_file"`
}

type CollectorsConfig struct {
	CPU     CPUCollectorConfig     `yaml:"cpu"`
	Memory  MemoryCollectorConfig  `yaml:"memory"`
	Disk    DiskCollectorConfig    `yaml:"disk"`
	Network NetworkCollectorConfig `yaml:"network"`
}

type CPUCollectorConfig struct {
	Enabled  bool          `yaml:"enabled"`
	Interval time.Duration `yaml:"interval"`
}

type MemoryCollectorConfig struct {
	Enabled  bool          `yaml:"enabled"`
	Interval time.Duration `yaml:"interval"`
}

type DiskCollectorConfig struct {
	Enabled        bool          `yaml:"enabled"`
	Interval       time.Duration `yaml:"interval"`
	MountPoints    []string      `yaml:"mount_points"`
	ExcludeFSTypes []string      `yaml:"exclude_fs_types"`
}

type NetworkCollectorConfig struct {
	Enabled    bool          `yaml:"enabled"`
	Interval   time.Duration `yaml:"interval"`
	Interfaces []string      `yaml:"interfaces"`
}

type ServicesConfig struct {
	Windows WindowsServicesConfig `yaml:"windows"`
	Systemd SystemdServicesConfig `yaml:"systemd"`
	Docker  DockerServicesConfig  `yaml:"docker"`
	IIS     IISServicesConfig     `yaml:"iis"`
}

type WindowsServicesConfig struct {
	Enabled  bool     `yaml:"enabled"`
	Services []string `yaml:"services"`
}

type SystemdServicesConfig struct {
	Enabled bool     `yaml:"enabled"`
	Units   []string `yaml:"units"`
}

type DockerServicesConfig struct {
	Enabled    bool     `yaml:"enabled"`
	Containers []string `yaml:"containers"`
}

type IISServicesConfig struct {
	Enabled  bool     `yaml:"enabled"`
	Sites    []string `yaml:"sites"`
	AppPools []string `yaml:"app_pools"`
}

type UpdaterConfig struct {
	Enabled       bool          `yaml:"enabled"`
	CheckInterval time.Duration `yaml:"check_interval"`
	AutoUpdate    bool          `yaml:"auto_update"`
}

type GUIConfig struct {
	Enabled           bool `yaml:"enabled"`
	StartMinimized    bool `yaml:"start_minimized"`
	ShowNotifications bool `yaml:"show_notifications"`
}

func DefaultConfig() *Config {
	return &Config{
		Server: ServerConfig{
			URL:        "https://api.eramonitor.com",
			Timeout:    30 * time.Second,
			RetryCount: 3,
			RetryDelay: 5 * time.Second,
		},
		Agent: AgentConfig{
			CheckInterval: 60 * time.Second,
			LogLevel:      "info",
		},
		Collectors: CollectorsConfig{
			CPU:    CPUCollectorConfig{Enabled: true, Interval: 60 * time.Second},
			Memory: MemoryCollectorConfig{Enabled: true, Interval: 60 * time.Second},
			Disk: DiskCollectorConfig{
				Enabled:        true,
				Interval:       60 * time.Second,
				ExcludeFSTypes: []string{"tmpfs", "devtmpfs", "squashfs"},
			},
			Network: NetworkCollectorConfig{Enabled: true, Interval: 60 * time.Second},
		},
		Services: ServicesConfig{
			Windows: WindowsServicesConfig{Enabled: runtime.GOOS == "windows"},
			Systemd: SystemdServicesConfig{Enabled: runtime.GOOS == "linux"},
			Docker:  DockerServicesConfig{Enabled: true},
			IIS:     IISServicesConfig{Enabled: runtime.GOOS == "windows"},
		},
		Updater: UpdaterConfig{
			Enabled:       true,
			CheckInterval: 1 * time.Hour,
			AutoUpdate:    false,
		},
		GUI: GUIConfig{
			Enabled:           runtime.GOOS == "windows",
			StartMinimized:    true,
			ShowNotifications: true,
		},
	}
}

func LoadConfig(path string) (*Config, error) {
	cfg := DefaultConfig()

	// Try to find config file
	if path == "" {
		path = findConfigFile()
	}

	if path != "" {
		data, err := os.ReadFile(path)
		if err != nil {
			return nil, err
		}

		if err := yaml.Unmarshal(data, cfg); err != nil {
			return nil, err
		}
	}

	// Check environment variables
	if apiKey := os.Getenv("ERA_API_KEY"); apiKey != "" {
		cfg.Server.APIKey = apiKey
	}
	if serverURL := os.Getenv("ERA_SERVER_URL"); serverURL != "" {
		cfg.Server.URL = serverURL
	}

	return cfg, nil
}

func findConfigFile() string {
	// Check common locations
	locations := []string{
		"config.yaml",
		"config.yml",
		"/etc/era-agent/config.yaml",
		"/etc/era-agent/config.yml",
	}

	if runtime.GOOS == "windows" {
		programData := os.Getenv("ProgramData")
		if programData != "" {
			locations = append(locations,
				filepath.Join(programData, "ERA Monitor", "config.yaml"),
				filepath.Join(programData, "ERA Monitor", "config.yml"),
			)
		}
	}

	// Check user home
	if home, err := os.UserHomeDir(); err == nil {
		locations = append(locations,
			filepath.Join(home, ".era-agent", "config.yaml"),
			filepath.Join(home, ".era-agent", "config.yml"),
		)
	}

	for _, loc := range locations {
		if _, err := os.Stat(loc); err == nil {
			return loc
		}
	}

	return ""
}

func (c *Config) Save(path string) error {
	data, err := yaml.Marshal(c)
	if err != nil {
		return err
	}

	dir := filepath.Dir(path)
	if err := os.MkdirAll(dir, 0755); err != nil {
		return err
	}

	return os.WriteFile(path, data, 0600)
}
Agent Core
go// internal/agent/agent.go

package agent

import (
	"context"
	"sync"
	"time"

	"github.com/eracloud/era-agent/internal/api"
	"github.com/eracloud/era-agent/internal/collector"
	"github.com/eracloud/era-agent/internal/service"
	"github.com/eracloud/era-agent/pkg/logger"
)

type Agent struct {
	config     *Config
	log        *logger.Logger
	apiClient  *api.Client
	collectors []collector.Collector
	services   []service.Monitor
	
	// State
	mu          sync.RWMutex
	isRunning   bool
	lastMetrics *api.HeartbeatRequest
	lastError   error
	lastSentAt  time.Time
}

func New(cfg *Config, log *logger.Logger) (*Agent, error) {
	// Create API client
	apiClient := api.NewClient(api.ClientConfig{
		BaseURL:    cfg.Server.URL,
		APIKey:     cfg.Server.APIKey,
		Timeout:    cfg.Server.Timeout,
		RetryCount: cfg.Server.RetryCount,
		RetryDelay: cfg.Server.RetryDelay,
	})

	a := &Agent{
		config:    cfg,
		log:       log,
		apiClient: apiClient,
	}

	// Initialize collectors
	if err := a.initCollectors(); err != nil {
		return nil, err
	}

	// Initialize service monitors
	if err := a.initServiceMonitors(); err != nil {
		return nil, err
	}

	return a, nil
}

func (a *Agent) initCollectors() error {
	if a.config.Collectors.CPU.Enabled {
		a.collectors = append(a.collectors, collector.NewCPUCollector())
	}

	if a.config.Collectors.Memory.Enabled {
		a.collectors = append(a.collectors, collector.NewMemoryCollector())
	}

	if a.config.Collectors.Disk.Enabled {
		a.collectors = append(a.collectors, collector.NewDiskCollector(
			a.config.Collectors.Disk.MountPoints,
			a.config.Collectors.Disk.ExcludeFSTypes,
		))
	}

	if a.config.Collectors.Network.Enabled {
		a.collectors = append(a.collectors, collector.NewNetworkCollector(
			a.config.Collectors.Network.Interfaces,
		))
	}

	a.log.Info("Initialized collectors", "count", len(a.collectors))
	return nil
}

func (a *Agent) initServiceMonitors() error {
	if a.config.Services.Windows.Enabled {
		if mon, err := service.NewWindowsMonitor(a.config.Services.Windows.Services); err == nil {
			a.services = append(a.services, mon)
		} else {
			a.log.Warn("Failed to initialize Windows service monitor", "error", err)
		}
	}

	if a.config.Services.Systemd.Enabled {
		if mon, err := service.NewSystemdMonitor(a.config.Services.Systemd.Units); err == nil {
			a.services = append(a.services, mon)
		} else {
			a.log.Warn("Failed to initialize Systemd monitor", "error", err)
		}
	}

	if a.config.Services.Docker.Enabled {
		if mon, err := service.NewDockerMonitor(a.config.Services.Docker.Containers); err == nil {
			a.services = append(a.services, mon)
		} else {
			a.log.Warn("Failed to initialize Docker monitor", "error", err)
		}
	}

	if a.config.Services.IIS.Enabled {
		if mon, err := service.NewIISMonitor(a.config.Services.IIS.Sites, a.config.Services.IIS.AppPools); err == nil {
			a.services = append(a.services, mon)
		} else {
			a.log.Warn("Failed to initialize IIS monitor", "error", err)
		}
	}

	a.log.Info("Initialized service monitors", "count", len(a.services))
	return nil
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

	a.log.Info("Agent started", "interval", a.config.Agent.CheckInterval)

	// Initial heartbeat
	a.sendHeartbeat()

	ticker := time.NewTicker(a.config.Agent.CheckInterval)
	defer ticker.Stop()

	for {
		select {
		case <-ctx.Done():
			a.log.Info("Agent stopping...")
			return ctx.Err()
		case <-ticker.C:
			a.sendHeartbeat()
		}
	}
}

func (a *Agent) sendHeartbeat() {
	a.log.Debug("Collecting metrics...")

	// Collect system info
	systemInfo, err := collector.GetSystemInfo()
	if err != nil {
		a.log.Error("Failed to get system info", "error", err)
		return
	}

	// Collect metrics
	var cpuPercent, ramPercent float64
	var ramUsedMB, ramTotalMB int64
	var uptimeSeconds int64
	var processCount int

	for _, c := range a.collectors {
		metrics, err := c.Collect()
		if err != nil {
			a.log.Warn("Collector error", "collector", c.Name(), "error", err)
			continue
		}

		switch c.Name() {
		case "cpu":
			if v, ok := metrics["percent"].(float64); ok {
				cpuPercent = v
			}
		case "memory":
			if v, ok := metrics["percent"].(float64); ok {
				ramPercent = v
			}
			if v, ok := metrics["used_mb"].(int64); ok {
				ramUsedMB = v
			}
			if v, ok := metrics["total_mb"].(int64); ok {
				ramTotalMB = v
			}
		}
	}

	// Collect disk info
	diskCollector := collector.NewDiskCollector(
		a.config.Collectors.Disk.MountPoints,
		a.config.Collectors.Disk.ExcludeFSTypes,
	)
	diskInfo, _ := diskCollector.CollectDisks()

	// Collect service status
	var services []api.ServiceInfo
	for _, mon := range a.services {
		svcList, err := mon.GetServices()
		if err != nil {
			a.log.Warn("Service monitor error", "monitor", mon.Name(), "error", err)
			continue
		}
		services = append(services, svcList...)
	}

	// Get network info
	networkInfo := a.getNetworkInfo()

	// Build heartbeat request
	request := &api.HeartbeatRequest{
		SystemInfo: api.SystemInfo{
			Hostname:     systemInfo.Hostname,
			OSType:       systemInfo.OSType,
			OSVersion:    systemInfo.OSVersion,
			CPUPercent:   cpuPercent,
			RAMPercent:   ramPercent,
			RAMUsedMB:    ramUsedMB,
			RAMTotalMB:   ramTotalMB,
			UptimeSeconds: uptimeSeconds,
			ProcessCount: processCount,
		},
		Disks:       diskInfo,
		Services:    services,
		NetworkInfo: networkInfo,
	}

	a.mu.Lock()
	a.lastMetrics = request
	a.mu.Unlock()

	// Send heartbeat
	a.log.Debug("Sending heartbeat...")
	response, err := a.apiClient.SendHeartbeat(request)
	if err != nil {
		a.mu.Lock()
		a.lastError = err
		a.mu.Unlock()
		a.log.Error("Failed to send heartbeat", "error", err)
		return
	}

	a.mu.Lock()
	a.lastError = nil
	a.lastSentAt = time.Now()
	a.mu.Unlock()

	a.log.Info("Heartbeat sent successfully",
		"cpu", cpuPercent,
		"ram", ramPercent,
		"disks", len(diskInfo),
		"services", len(services),
		"next_check", response.NextCheckIn,
	)

	// Process commands from server
	if len(response.Commands) > 0 {
		a.processCommands(response.Commands)
	}
}

func (a *Agent) getNetworkInfo() api.NetworkInfo {
	netCollector := collector.NewNetworkCollector(a.config.Collectors.Network.Interfaces)
	info, err := netCollector.CollectNetworkInfo()
	if err != nil {
		a.log.Warn("Failed to collect network info", "error", err)
		return api.NetworkInfo{}
	}
	return info
}

func (a *Agent) processCommands(commands []api.Command) {
	for _, cmd := range commands {
		a.log.Info("Received command", "type", cmd.Type, "id", cmd.ID)

		switch cmd.Type {
		case "restart_service":
			// Handle service restart
		case "update_config":
			// Handle config update
		case "update_agent":
			// Handle agent update
		default:
			a.log.Warn("Unknown command type", "type", cmd.Type)
		}
	}
}

// Status returns the current agent status
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

type AgentStatus struct {
	IsRunning   bool
	LastSentAt  time.Time
	LastError   error
	LastMetrics *api.HeartbeatRequest
}

7.5 Collectors
CPU Collector
go// internal/collector/cpu.go

package collector

import (
	"time"

	"github.com/shirou/gopsutil/v3/cpu"
)

type CPUCollector struct{}

func NewCPUCollector() *CPUCollector {
	return &CPUCollector{}
}

func (c *CPUCollector) Name() string {
	return "cpu"
}

func (c *CPUCollector) Collect() (map[string]interface{}, error) {
	// Get CPU percentage (with 1 second interval for accuracy)
	percentages, err := cpu.Percent(time.Second, false)
	if err != nil {
		return nil, err
	}

	var totalPercent float64
	if len(percentages) > 0 {
		totalPercent = percentages[0]
	}

	// Get CPU info
	cpuInfo, err := cpu.Info()
	if err != nil {
		return nil, err
	}

	// Get per-core percentages
	corePercentages, _ := cpu.Percent(0, true)

	result := map[string]interface{}{
		"percent":      totalPercent,
		"core_count":   len(cpuInfo),
		"cores":        corePercentages,
	}

	if len(cpuInfo) > 0 {
		result["model"] = cpuInfo[0].ModelName
		result["mhz"] = cpuInfo[0].Mhz
	}

	return result, nil
}
Memory Collector
go// internal/collector/memory.go

package collector

import (
	"github.com/shirou/gopsutil/v3/mem"
)

type MemoryCollector struct{}

func NewMemoryCollector() *MemoryCollector {
	return &MemoryCollector{}
}

func (c *MemoryCollector) Name() string {
	return "memory"
}

func (c *MemoryCollector) Collect() (map[string]interface{}, error) {
	v, err := mem.VirtualMemory()
	if err != nil {
		return nil, err
	}

	result := map[string]interface{}{
		"percent":     v.UsedPercent,
		"total_mb":    int64(v.Total / 1024 / 1024),
		"used_mb":     int64(v.Used / 1024 / 1024),
		"free_mb":     int64(v.Free / 1024 / 1024),
		"available_mb": int64(v.Available / 1024 / 1024),
		"cached_mb":   int64(v.Cached / 1024 / 1024),
		"buffers_mb":  int64(v.Buffers / 1024 / 1024),
	}

	// Get swap info
	swap, err := mem.SwapMemory()
	if err == nil {
		result["swap_total_mb"] = int64(swap.Total / 1024 / 1024)
		result["swap_used_mb"] = int64(swap.Used / 1024 / 1024)
		result["swap_percent"] = swap.UsedPercent
	}

	return result, nil
}
Disk Collector
go// internal/collector/disk.go

package collector

import (
	"strings"

	"github.com/eracloud/era-agent/internal/api"
	"github.com/shirou/gopsutil/v3/disk"
)

type DiskCollector struct {
	mountPoints    []string
	excludeFSTypes []string
}

func NewDiskCollector(mountPoints, excludeFSTypes []string) *DiskCollector {
	return &DiskCollector{
		mountPoints:    mountPoints,
		excludeFSTypes: excludeFSTypes,
	}
}

func (c *DiskCollector) Name() string {
	return "disk"
}

func (c *DiskCollector) Collect() (map[string]interface{}, error) {
	disks, err := c.CollectDisks()
	if err != nil {
		return nil, err
	}

	var totalGB, usedGB float64
	for _, d := range disks {
		totalGB += d.TotalGB
		usedGB += d.UsedGB
	}

	var usedPercent float64
	if totalGB > 0 {
		usedPercent = (usedGB / totalGB) * 100
	}

	return map[string]interface{}{
		"total_gb":     totalGB,
		"used_gb":      usedGB,
		"free_gb":      totalGB - usedGB,
		"used_percent": usedPercent,
		"disks":        disks,
	}, nil
}

func (c *DiskCollector) CollectDisks() ([]api.DiskInfo, error) {
	partitions, err := disk.Partitions(false)
	if err != nil {
		return nil, err
	}

	var disks []api.DiskInfo

	for _, p := range partitions {
		// Check if we should skip this filesystem type
		if c.shouldExclude(p.Fstype) {
			continue
		}

		// Check if we should include this mount point
		if len(c.mountPoints) > 0 && !c.containsMountPoint(p.Mountpoint) {
			continue
		}

		usage, err := disk.Usage(p.Mountpoint)
		if err != nil {
			continue
		}

		disks = append(disks, api.DiskInfo{
			Name:        p.Device,
			MountPoint:  p.Mountpoint,
			FileSystem:  p.Fstype,
			TotalGB:     float64(usage.Total) / 1024 / 1024 / 1024,
			UsedGB:      float64(usage.Used) / 1024 / 1024 / 1024,
			UsedPercent: usage.UsedPercent,
		})
	}

	return disks, nil
}

func (c *DiskCollector) shouldExclude(fstype string) bool {
	for _, excluded := range c.excludeFSTypes {
		if strings.EqualFold(fstype, excluded) {
			return true
		}
	}
	return false
}

func (c *DiskCollector) containsMountPoint(mountPoint string) bool {
	for _, mp := range c.mountPoints {
		if strings.EqualFold(mountPoint, mp) {
			return true
		}
	}
	return false
}
Network Collector
go// internal/collector/network.go

package collector

import (
	"net"
	"strings"

	"github.com/eracloud/era-agent/internal/api"
	"github.com/shirou/gopsutil/v3/net"
)

type NetworkCollector struct {
	interfaces []string
}

func NewNetworkCollector(interfaces []string) *NetworkCollector {
	return &NetworkCollector{
		interfaces: interfaces,
	}
}

func (c *NetworkCollector) Name() string {
	return "network"
}

func (c *NetworkCollector) Collect() (map[string]interface{}, error) {
	counters, err := psnet.IOCounters(true)
	if err != nil {
		return nil, err
	}

	var totalBytesIn, totalBytesOut uint64
	interfaceStats := make([]map[string]interface{}, 0)

	for _, counter := range counters {
		if len(c.interfaces) > 0 && !c.containsInterface(counter.Name) {
			continue
		}

		// Skip loopback
		if strings.HasPrefix(strings.ToLower(counter.Name), "lo") {
			continue
		}

		totalBytesIn += counter.BytesRecv
		totalBytesOut += counter.BytesSent

		interfaceStats = append(interfaceStats, map[string]interface{}{
			"name":       counter.Name,
			"bytes_in":  counter.BytesRecv,
			"bytes_out": counter.BytesSent,
			"packets_in":  counter.PacketsRecv,
			"packets_out": counter.PacketsSent,
			"errors_in":   counter.Errin,
			"errors_out":  counter.Errout,
		})
	}

	return map[string]interface{}{
		"total_bytes_in":  totalBytesIn,
		"total_bytes_out": totalBytesOut,
		"interfaces":      interfaceStats,
	}, nil
}

func (c *NetworkCollector) CollectNetworkInfo() (api.NetworkInfo, error) {
	info := api.NetworkInfo{}

	// Get primary IP
	addrs, err := net.InterfaceAddrs()
	if err == nil {
		for _, addr := range addrs {
			if ipnet, ok := addr.(*net.IPNet); ok && !ipnet.IP.IsLoopback() {
				if ipnet.IP.To4() != nil {
					info.PrimaryIP = ipnet.IP.String()
					break
				}
			}
		}
	}

	// Get IO counters
	counters, err := psnet.IOCounters(false)
	if err == nil && len(counters) > 0 {
		info.InBytes = counters[0].BytesRecv
		info.OutBytes = counters[0].BytesSent
	}

	return info, nil
}

func (c *NetworkCollector) containsInterface(name string) bool {
	for _, iface := range c.interfaces {
		if strings.EqualFold(name, iface) {
			return true
		}
	}
	return false
}

// Alias for psnet to avoid naming conflict
var psnet = net
System Info Collector
go// internal/collector/system.go

package collector

import (
	"os"
	"runtime"
	"time"

	"github.com/shirou/gopsutil/v3/host"
	"github.com/shirou/gopsutil/v3/process"
)

type SystemInfo struct {
	Hostname      string
	OSType        string
	OSVersion     string
	Platform      string
	KernelVersion string
	Uptime        time.Duration
	ProcessCount  int
}

func GetSystemInfo() (*SystemInfo, error) {
	info := &SystemInfo{
		OSType: runtime.GOOS,
	}

	// Get hostname
	hostname, err := os.Hostname()
	if err == nil {
		info.Hostname = hostname
	}

	// Get host info
	hostInfo, err := host.Info()
	if err == nil {
		info.Platform = hostInfo.Platform
		info.OSVersion = hostInfo.PlatformVersion
		info.KernelVersion = hostInfo.KernelVersion
		info.Uptime = time.Duration(hostInfo.Uptime) * time.Second
	}

	// Get process count
	processes, err := process.Processes()
	if err == nil {
		info.ProcessCount = len(processes)
	}

	return info, nil
}

type Collector interface {
	Name() string
	Collect() (map[string]interface{}, error)
}

7.6 Service Monitors
Service Monitor Interface
go// internal/service/service.go

package service

import (
	"github.com/eracloud/era-agent/internal/api"
)

type Monitor interface {
	Name() string
	GetServices() ([]api.ServiceInfo, error)
}
Windows Service Monitor
go// internal/service/windows.go

//go:build windows

package service

import (
	"strings"

	"github.com/eracloud/era-agent/internal/api"
	"golang.org/x/sys/windows/svc"
	"golang.org/x/sys/windows/svc/mgr"
)

type WindowsMonitor struct {
	services []string
}

func NewWindowsMonitor(services []string) (*WindowsMonitor, error) {
	return &WindowsMonitor{
		services: services,
	}, nil
}

func (m *WindowsMonitor) Name() string {
	return "windows"
}

func (m *WindowsMonitor) GetServices() ([]api.ServiceInfo, error) {
	// Connect to service manager
	manager, err := mgr.Connect()
	if err != nil {
		return nil, err
	}
	defer manager.Disconnect()

	var result []api.ServiceInfo

	// If specific services are configured, only check those
	if len(m.services) > 0 {
		for _, name := range m.services {
			info, err := m.getServiceInfo(manager, name)
			if err != nil {
				continue
			}
			result = append(result, info)
		}
	} else {
		// Get all services
		services, err := manager.ListServices()
		if err != nil {
			return nil, err
		}

		for _, name := range services {
			info, err := m.getServiceInfo(manager, name)
			if err != nil {
				continue
			}
			result = append(result, info)
		}
	}

	return result, nil
}

func (m *WindowsMonitor) getServiceInfo(manager *mgr.Mgr, name string) (api.ServiceInfo, error) {
	service, err := manager.OpenService(name)
	if err != nil {
		return api.ServiceInfo{}, err
	}
	defer service.Close()

	// Get service status
	status, err := service.Query()
	if err != nil {
		return api.ServiceInfo{}, err
	}

	// Get service config
	config, err := service.Config()
	if err != nil {
		return api.ServiceInfo{}, err
	}

	return api.ServiceInfo{
		Name:        name,
		DisplayName: config.DisplayName,
		Type:        "WindowsService",
		Status:      mapWindowsState(status.State),
		Config: map[string]interface{}{
			"start_type":  config.StartType,
			"binary_path": config.BinaryPathName,
			"description": config.Description,
		},
	}, nil
}

func mapWindowsState(state svc.State) string {
	switch state {
	case svc.Running:
		return "running"
	case svc.Stopped:
		return "stopped"
	case svc.StartPending:
		return "starting"
	case svc.StopPending:
		return "stopping"
	case svc.Paused:
		return "paused"
	default:
		return "unknown"
	}
}
Systemd Monitor
go// internal/service/systemd.go

//go:build linux

package service

import (
	"context"
	"strings"

	"github.com/coreos/go-systemd/v22/dbus"
	"github.com/eracloud/era-agent/internal/api"
)

type SystemdMonitor struct {
	units []string
}

func NewSystemdMonitor(units []string) (*SystemdMonitor, error) {
	// Test connection
	conn, err := dbus.NewWithContext(context.Background())
	if err != nil {
		return nil, err
	}
	conn.Close()

	return &SystemdMonitor{
		units: units,
	}, nil
}

func (m *SystemdMonitor) Name() string {
	return "systemd"
}

func (m *SystemdMonitor) GetServices() ([]api.ServiceInfo, error) {
	conn, err := dbus.NewWithContext(context.Background())
	if err != nil {
		return nil, err
	}
	defer conn.Close()

	var result []api.ServiceInfo

	if len(m.units) > 0 {
		// Get specific units
		for _, unitName := range m.units {
			info, err := m.getUnitInfo(conn, unitName)
			if err != nil {
				continue
			}
			result = append(result, info)
		}
	} else {
		// Get all service units
		units, err := conn.ListUnitsContext(context.Background())
		if err != nil {
			return nil, err
		}

		for _, unit := range units {
			if !strings.HasSuffix(unit.Name, ".service") {
				continue
			}

			result = append(result, api.ServiceInfo{
				Name:        unit.Name,
				DisplayName: unit.Description,
				Type:        "SystemdUnit",
				Status:      mapSystemdState(unit.ActiveState, unit.SubState),
				Config: map[string]interface{}{
					"load_state": unit.LoadState,
					"sub_state":  unit.SubState,
				},
			})
		}
	}

	return result, nil
}

func (m *SystemdMonitor) getUnitInfo(conn *dbus.Conn, name string) (api.ServiceInfo, error) {
	props, err := conn.GetAllPropertiesContext(context.Background(), name)
	if err != nil {
		return api.ServiceInfo{}, err
	}

	activeState := ""
	subState := ""
	description := ""

	if v, ok := props["ActiveState"].(string); ok {
		activeState = v
	}
	if v, ok := props["SubState"].(string); ok {
		subState = v
	}
	if v, ok := props["Description"].(string); ok {
		description = v
	}

	return api.ServiceInfo{
		Name:        name,
		DisplayName: description,
		Type:        "SystemdUnit",
		Status:      mapSystemdState(activeState, subState),
		Config: map[string]interface{}{
			"active_state": activeState,
			"sub_state":    subState,
		},
	}, nil
}

func mapSystemdState(activeState, subState string) string {
	switch activeState {
	case "active":
		if subState == "running" {
			return "running"
		}
		return "active"
	case "inactive":
		return "stopped"
	case "failed":
		return "failed"
	case "activating":
		return "starting"
	case "deactivating":
		return "stopping"
	default:
		return "unknown"
	}
}
Docker Monitor
go// internal/service/docker.go

package service

import (
	"context"
	"strings"

	"github.com/docker/docker/api/types"
	"github.com/docker/docker/client"
	"github.com/eracloud/era-agent/internal/api"
)

type DockerMonitor struct {
	containers []string
	client     *client.Client
}

func NewDockerMonitor(containers []string) (*DockerMonitor, error) {
	cli, err := client.NewClientWithOpts(client.FromEnv, client.WithAPIVersionNegotiation())
	if err != nil {
		return nil, err
	}

	// Test connection
	_, err = cli.Ping(context.Background())
	if err != nil {
		cli.Close()
		return nil, err
	}

	return &DockerMonitor{
		containers: containers,
		client:     cli,
	}, nil
}

func (m *DockerMonitor) Name() string {
	return "docker"
}

func (m *DockerMonitor) GetServices() ([]api.ServiceInfo, error) {
	containers, err := m.client.ContainerList(context.Background(), types.ContainerListOptions{
		All: true,
	})
	if err != nil {
		return nil, err
	}

	var result []api.ServiceInfo

	for _, container := range containers {
		// Filter by name if configured
		if len(m.containers) > 0 {
			match := false
			for _, name := range m.containers {
				for _, cname := range container.Names {
					if strings.Contains(cname, name) || container.ID[:12] == name {
						match = true
						break
					}
				}
				if match {
					break
				}
			}
			if !match {
				continue
			}
		}

		name := ""
		if len(container.Names) > 0 {
			name = strings.TrimPrefix(container.Names[0], "/")
		} else {
			name = container.ID[:12]
		}

		result = append(result, api.ServiceInfo{
			Name:        name,
			DisplayName: container.Image,
			Type:        "DockerContainer",
			Status:      mapDockerState(container.State),
			Config: map[string]interface{}{
				"container_id": container.ID[:12],
				"image":        container.Image,
				"status":       container.Status,
				"ports":        container.Ports,
			},
		})
	}

	return result, nil
}

func mapDockerState(state string) string {
	switch strings.ToLower(state) {
	case "running":
		return "running"
	case "exited", "dead":
		return "stopped"
	case "created", "restarting":
		return "starting"
	case "paused":
		return "paused"
	case "removing":
		return "stopping"
	default:
		return "unknown"
	}
}

7.7 API Client
go// internal/api/client.go

package api

import (
	"bytes"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"time"
)

type Client struct {
	config     ClientConfig
	httpClient *http.Client
}

type ClientConfig struct {
	BaseURL    string
	APIKey     string
	Timeout    time.Duration
	RetryCount int
	RetryDelay time.Duration
}

func NewClient(config ClientConfig) *Client {
	return &Client{
		config: config,
		httpClient: &http.Client{
			Timeout: config.Timeout,
		},
	}
}

func (c *Client) SendHeartbeat(request *HeartbeatRequest) (*HeartbeatResponse, error) {
	var lastErr error

	for attempt := 0; attempt <= c.config.RetryCount; attempt++ {
		if attempt > 0 {
			time.Sleep(c.config.RetryDelay)
		}

		response, err := c.doHeartbeat(request)
		if err == nil {
			return response, nil
		}

		lastErr = err
	}

	return nil, fmt.Errorf("all retry attempts failed: %w", lastErr)
}

func (c *Client) doHeartbeat(request *HeartbeatRequest) (*HeartbeatResponse, error) {
	body, err := json.Marshal(request)
	if err != nil {
		return nil, fmt.Errorf("failed to marshal request: %w", err)
	}

	req, err := http.NewRequest("POST", c.config.BaseURL+"/api/agent/heartbeat", bytes.NewReader(body))
	if err != nil {
		return nil, fmt.Errorf("failed to create request: %w", err)
	}

	req.Header.Set("Content-Type", "application/json")
	req.Header.Set("X-API-Key", c.config.APIKey)
	req.Header.Set("User-Agent", "ERA-Agent/1.0")

	resp, err := c.httpClient.Do(req)
	if err != nil {
		return nil, fmt.Errorf("request failed: %w", err)
	}
	defer resp.Body.Close()

	respBody, err := io.ReadAll(resp.Body)
	if err != nil {
		return nil, fmt.Errorf("failed to read response: %w", err)
	}

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("server returned %d: %s", resp.StatusCode, string(respBody))
	}

	var response HeartbeatResponse
	if err := json.Unmarshal(respBody, &response); err != nil {
		return nil, fmt.Errorf("failed to parse response: %w", err)
	}

	return &response, nil
}

func (c *Client) GetConfig() (*AgentConfigResponse, error) {
	req, err := http.NewRequest("GET", c.config.BaseURL+"/api/agent/config", nil)
	if err != nil {
		return nil, err
	}

	req.Header.Set("X-API-Key", c.config.APIKey)

	resp, err := c.httpClient.Do(req)
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return nil, fmt.Errorf("server returned %d", resp.StatusCode)
	}

	var config AgentConfigResponse
	if err := json.NewDecoder(resp.Body).Decode(&config); err != nil {
		return nil, err
	}

	return &config, nil
}
API Models
go// internal/api/models.go

package api

type HeartbeatRequest struct {
	SystemInfo  SystemInfo    `json:"system_info"`
	Disks       []DiskInfo    `json:"disks"`
	Services    []ServiceInfo `json:"services"`
	NetworkInfo NetworkInfo   `json:"network_info"`
}

type SystemInfo struct {
	Hostname      string  `json:"hostname"`
	OSType        string  `json:"os_type"`
	OSVersion     string  `json:"os_version"`
	CPUPercent    float64 `json:"cpu_percent"`
	RAMPercent    float64 `json:"ram_percent"`
	RAMUsedMB     int64   `json:"ram_used_mb"`
	RAMTotalMB    int64   `json:"ram_total_mb"`
	UptimeSeconds int64   `json:"uptime_seconds"`
	ProcessCount  int     `json:"process_count"`
}

type DiskInfo struct {
	Name        string  `json:"name"`
	MountPoint  string  `json:"mount_point"`
	FileSystem  string  `json:"file_system"`
	TotalGB     float64 `json:"total_gb"`
	UsedGB      float64 `json:"used_gb"`
	UsedPercent float64 `json:"used_percent"`
}

type ServiceInfo struct {
	Name        string                 `json:"name"`
	DisplayName string                 `json:"display_name"`
	Type        string                 `json:"type"`
	Status      string                 `json:"status"`
	Config      map[string]interface{} `json:"config,omitempty"`
}

type NetworkInfo struct {
	PrimaryIP string `json:"primary_ip"`
	PublicIP  string `json:"public_ip,omitempty"`
	InBytes   uint64 `json:"in_bytes"`
	OutBytes  uint64 `json:"out_bytes"`
}

type HeartbeatResponse struct {
	Success    bool      `json:"success"`
	HostID     string    `json:"host_id"`
	NextCheckIn int       `json:"next_check_in"`
	Commands   []Command `json:"commands,omitempty"`
	Message    string    `json:"message,omitempty"`
}

type Command struct {
	ID     string                 `json:"id"`
	Type   string                 `json:"type"`
	Params map[string]interface{} `json:"params,omitempty"`
}

type AgentConfigResponse struct {
	CheckIntervalSeconds int      `json:"check_interval_seconds"`
	CollectCPU           bool     `json:"collect_cpu"`
	CollectMemory        bool     `json:"collect_memory"`
	CollectDisk          bool     `json:"collect_disk"`
	CollectNetwork       bool     `json:"collect_network"`
	CollectServices      bool     `json:"collect_services"`
	ServicesToMonitor    []string `json:"services_to_monitor,omitempty"`
}

7.8 Fyne GUI (Windows)
go// internal/gui/app.go

package gui

import (
	"time"

	"fyne.io/fyne/v2"
	"fyne.io/fyne/v2/app"
	"fyne.io/fyne/v2/container"
	"fyne.io/fyne/v2/dialog"
	"fyne.io/fyne/v2/driver/desktop"
	"fyne.io/fyne/v2/theme"
	"fyne.io/fyne/v2/widget"
	"github.com/eracloud/era-agent/internal/agent"
)

type App struct {
	fyneApp    fyne.App
	mainWindow fyne.Window
	agent      *agent.Agent
	config     *agent.Config
	
	// UI elements
	statusLabel     *widget.Label
	cpuLabel        *widget.Label
	ramLabel        *widget.Label
	lastSentLabel   *widget.Label
	servicesLabel   *widget.Label
}

func NewApp(agentInstance *agent.Agent, config *agent.Config) *App {
	fyneApp := app.NewWithID("com.eracloud.era-agent")
	fyneApp.SetIcon(resourceIconPng)
	
	return &App{
		fyneApp: fyneApp,
		agent:   agentInstance,
		config:  config,
	}
}

func (a *App) Run() {
	// Create main window
	a.mainWindow = a.fyneApp.NewWindow("ERA Monitor Agent")
	a.mainWindow.Resize(fyne.NewSize(400, 500))
	
	// Setup system tray if supported
	if desk, ok := a.fyneApp.(desktop.App); ok {
		a.setupSystemTray(desk)
	}
	
	// Build UI
	a.buildUI()
	
	// Start status updater
	go a.updateStatusLoop()
	
	// Hide window if start minimized
	if a.config.GUI.StartMinimized {
		a.mainWindow.Hide()
	} else {
		a.mainWindow.Show()
	}
	
	// Handle close - minimize to tray instead
	a.mainWindow.SetCloseIntercept(func() {
		a.mainWindow.Hide()
	})
	
	a.fyneApp.Run()
}

func (a *App) buildUI() {
	// Header
	header := container.NewVBox(
		widget.NewLabelWithStyle("ERA Monitor Agent", fyne.TextAlignCenter, fyne.TextStyle{Bold: true}),
		widget.NewSeparator(),
	)
	
	// Status section
	a.statusLabel = widget.NewLabel("Status: Starting...")
	a.cpuLabel = widget.NewLabel("CPU: --")
	a.ramLabel = widget.NewLabel("RAM: --")
	a.lastSentLabel = widget.NewLabel("Last Heartbeat: --")
	a.servicesLabel = widget.NewLabel("Services: --")
	
	statusCard := widget.NewCard("Status", "",
		container.NewVBox(
			a.statusLabel,
			a.cpuLabel,
			a.ramLabel,
			a.lastSentLabel,
			a.servicesLabel,
		),
	)
	
	// Server info
	serverInfo := widget.NewCard("Server", "",
		container.NewVBox(
			widget.NewLabel("URL: "+a.config.Server.URL),
			widget.NewLabel("API Key: "+maskAPIKey(a.config.Server.APIKey)),
		),
	)
	
	// Actions
	refreshBtn := widget.NewButtonWithIcon("Send Heartbeat", theme.ViewRefreshIcon(), func() {
		// Trigger immediate heartbeat
	})
	
	settingsBtn := widget.NewButtonWithIcon("Settings", theme.SettingsIcon(), func() {
		a.showSettings()
	})
	
	logsBtn := widget.NewButtonWithIcon("View Logs", theme.DocumentIcon(), func() {
		a.showLogs()
	})
	
	actions := container.NewHBox(
		refreshBtn,
		settingsBtn,
		logsBtn,
	)
	
	// Main layout
	content := container.NewVBox(
		header,
		statusCard,
		serverInfo,
		widget.NewSeparator(),
		actions,
	)
	
	a.mainWindow.SetContent(content)
}

func (a *App) setupSystemTray(desk desktop.App) {
	menu := fyne.NewMenu("ERA Agent",
		fyne.NewMenuItem("Show", func() {
			a.mainWindow.Show()
		}),
		fyne.NewMenuItemSeparator(),
		fyne.NewMenuItem("Send Heartbeat", func() {
			// Trigger heartbeat
		}),
		fyne.NewMenuItemSeparator(),
		fyne.NewMenuItem("Quit", func() {
			a.fyneApp.Quit()
		}),
	)
	
	desk.SetSystemTrayMenu(menu)
	desk.SetSystemTrayIcon(resourceIconPng)
}

func (a *App) updateStatusLoop() {
	ticker := time.NewTicker(2 * time.Second)
	defer ticker.Stop()
	
	for range ticker.C {
		status := a.agent.Status()
		
		// Update UI (must be done on main thread)
		a.fyneApp.SendNotification(nil) // Trigger UI update
		
		if status.IsRunning {
			a.statusLabel.SetText("Status: Running ✓")
		} else {
			a.statusLabel.SetText("Status: Stopped")
		}
		
		if status.LastMetrics != nil {
			a.cpuLabel.SetText(fmt.Sprintf("CPU: %.1f%%", status.LastMetrics.SystemInfo.CPUPercent))
			a.ramLabel.SetText(fmt.Sprintf("RAM: %.1f%%", status.LastMetrics.SystemInfo.RAMPercent))
			a.servicesLabel.SetText(fmt.Sprintf("Services: %d", len(status.LastMetrics.Services)))
		}
		
		if !status.LastSentAt.IsZero() {
			a.lastSentLabel.SetText(fmt.Sprintf("Last Heartbeat: %s", status.LastSentAt.Format("15:04:05")))
		}
		
		if status.LastError != nil {
			a.statusLabel.SetText("Status: Error - " + status.LastError.Error())
		}
	}
}

func (a *App) showSettings() {
	settingsWindow := a.fyneApp.NewWindow("Settings")
	settingsWindow.Resize(fyne.NewSize(400, 300))
	
	serverURL := widget.NewEntry()
	serverURL.SetText(a.config.Server.URL)
	
	apiKey := widget.NewPasswordEntry()
	apiKey.SetText(a.config.Server.APIKey)
	
	interval := widget.NewEntry()
	interval.SetText(a.config.Agent.CheckInterval.String())
	
	form := &widget.Form{
		Items: []*widget.FormItem{
			{Text: "Server URL", Widget: serverURL},
			{Text: "API Key", Widget: apiKey},
			{Text: "Check Interval", Widget: interval},
		},
		OnSubmit: func() {
			// Save settings
			a.config.Server.URL = serverURL.Text
			a.config.Server.APIKey = apiKey.Text
			// Parse interval...
			
			dialog.ShowInformation("Settings", "Settings saved. Restart agent to apply.", settingsWindow)
		},
	}
	
	settingsWindow.SetContent(container.NewPadded(form))
	settingsWindow.Show()
}

func (a *App) showLogs() {
	logsWindow := a.fyneApp.NewWindow("Logs")
	logsWindow.Resize(fyne.NewSize(600, 400))
	
	logText := widget.NewMultiLineEntry()
	logText.SetText("Log entries will appear here...")
	logText.Disable()
	
	logsWindow.SetContent(container.NewScroll(logText))
	logsWindow.Show()
}

func maskAPIKey(key string) string {
	if len(key) <= 8 {
		return "****"
	}
	return key[:4] + "..." + key[len(key)-4:]
}

7.9 Build Scripts
bash#!/bin/bash
# scripts/build.sh

VERSION=${1:-"1.0.0"}
BUILD_TIME=$(date -u +"%Y-%m-%dT%H:%M:%SZ")
COMMIT=$(git rev-parse --short HEAD 2>/dev/null || echo "unknown")

LDFLAGS="-X main.version=${VERSION} -X main.buildTime=${BUILD_TIME} -X main.commit=${COMMIT}"

echo "Building ERA Agent v${VERSION}..."

# Create output directory
mkdir -p dist

# Build for Linux AMD64
echo "Building for Linux AMD64..."
GOOS=linux GOARCH=amd64 go build -ldflags "${LDFLAGS}" -o dist/era-agent-linux-amd64 ./cmd/agent

# Build for Linux ARM64
echo "Building for Linux ARM64..."
GOOS=linux GOARCH=arm64 go build -ldflags "${LDFLAGS}" -o dist/era-agent-linux-arm64 ./cmd/agent

# Build for Windows AMD64 (CLI)
echo "Building for Windows AMD64 (CLI)..."
GOOS=windows GOARCH=amd64 go build -ldflags "${LDFLAGS}" -o dist/era-agent-windows-amd64.exe ./cmd/agent

# Build for Windows AMD64 (GUI)
echo "Building for Windows AMD64 (GUI)..."
GOOS=windows GOARCH=amd64 go build -ldflags "${LDFLAGS} -H=windowsgui" -o dist/era-agent-gui-windows-amd64.exe ./cmd/agent-gui

# Build for macOS AMD64
echo "Building for macOS AMD64..."
GOOS=darwin GOARCH=amd64 go build -ldflags "${LDFLAGS}" -o dist/era-agent-darwin-amd64 ./cmd/agent

# Build for macOS ARM64 (M1/M2)
echo "Building for macOS ARM64..."
GOOS=darwin GOARCH=arm64 go build -ldflags "${LDFLAGS}" -o dist/era-agent-darwin-arm64 ./cmd/agent

echo "Build complete!"
ls -la dist/
powershell# scripts/install-windows.ps1

param(
    [string]$ApiKey,
    [string]$ServerUrl = "https://api.eramonitor.com"
)

$ErrorActionPreference = "Stop"

$ServiceName = "ERAMonitorAgent"
$InstallDir = "$env:ProgramFiles\ERA Monitor"
$ConfigDir = "$env:ProgramData\ERA Monitor"

Write-Host "Installing ERA Monitor Agent..." -ForegroundColor Green

# Create directories
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
New-Item -ItemType Directory -Force -Path $ConfigDir | Out-Null

# Copy files
Copy-Item "era-agent.exe" -Destination "$InstallDir\era-agent.exe" -Force

# Create configuration
$Config = @"
server:
  url: "$ServerUrl"
  api_key: "$ApiKey"
  timeout: 30s
  retry_count: 3
  retry_delay: 5s

agent:
  check_interval: 60s
  log_level: info

collectors:
  cpu:
    enabled: true
  memory:
    enabled: true
  disk:
    enabled: true
  network:
    enabled: true

services:
  windows:
    enabled: true
  iis:
    enabled: true
  docker:
    enabled: true
"@

$Config | Out-File -FilePath "$ConfigDir\config.yaml" -Encoding utf8

# Install as Windows Service
Write-Host "Installing Windows Service..." -ForegroundColor Yellow

$servicePath = "$InstallDir\era-agent.exe"
$serviceArgs = "--config `"$ConfigDir\config.yaml`""

# Check if service exists
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Stopping existing service..."
    Stop-Service -Name $ServiceName -Force
    sc.exe delete $ServiceName
    Start-Sleep -Seconds 2
}

# Create service
New-Service -Name $ServiceName `
    -BinaryPathName "$servicePath $serviceArgs" `
    -DisplayName "ERA Monitor Agent" `
    -Description "Monitoring agent for ERA Monitor" `
    -StartupType Automatic

# Start service
Start-Service -Name $ServiceName

Write-Host "Installation complete!" -ForegroundColor Green
Write-Host "Service Status: $((Get-Service -Name $ServiceName).Status)"

7.10 Phase 7 Checklist
markdown# Phase 7 Completion Checklist

## Project Structure
- [ ] cmd/agent/main.go
- [ ] cmd/agent-gui/main.go
- [ ] internal/agent/agent.go
- [ ] internal/agent/config.go
- [ ] internal/collector/
- [ ] internal/service/
- [ ] internal/api/
- [ ] internal/gui/

## Collectors
- [ ] CPU collector (percent, cores)
- [ ] Memory collector (percent, used, total, swap)
- [ ] Disk collector (mounts, usage, filesystem)
- [ ] Network collector (bytes in/out, interfaces)
- [ ] System info collector (hostname, OS, uptime)

## Service Monitors
- [ ] Windows Services monitor
- [ ] Systemd units monitor
- [ ] Docker containers monitor
- [ ] IIS Sites/AppPools monitor

## API Client
- [ ] Heartbeat sender with retry
- [ ] Config fetcher
- [ ] Command processor
- [ ] Error handling

## Configuration
- [ ] YAML config file support
- [ ] Environment variable override
- [ ] Command-line flags
- [ ] Default values

## GUI (Windows)
- [ ] Fyne application setup
- [ ] System tray integration
- [ ] Status display
- [ ] Settings window
- [ ] Logs viewer

## Build & Deploy
- [ ] Multi-platform build script
- [ ] Windows installer (PowerShell)
- [ ] Linux installer (Bash)
- [ ] Windows Service support
- [ ] Systemd service file

## Features
- [ ] Automatic hostname detection
- [ ] Secure API key storage
- [ ] Retry logic with backoff
- [ ] Graceful shutdown
- [ ] Logging with levels
- [ ] Auto-discovery of services

## Testing
- [ ] Collector unit tests
- [ ] API client tests
- [ ] Service monitor tests
- [ ] Integration tests

## Documentation
- [ ] README.md
- [ ] Installation guide
- [ ] Configuration reference
- [ ] Troubleshooting guide
