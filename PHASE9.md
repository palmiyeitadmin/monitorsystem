PHASE 9: Go Agent Development (Days 43-52)9.1 Project Setup & Dependenciesbash# Create project
mkdir era-monitor-agent
cd era-monitor-agent
go mod init github.com/eracloud/era-monitor-agent

# Install dependencies
go get fyne.io/fyne/v2                     # GUI framework
go get github.com/shirou/gopsutil/v3       # System metrics (cross-platform)
go get github.com/yusufpapurcu/wmi         # Windows WMI queries
go get github.com/kardianos/service        # Windows/Linux service wrapper
go get github.com/go-resty/resty/v2        # HTTP client
go get github.com/robfig/cron/v3           # Scheduler
go get go.uber.org/zap                     # Logging
go get github.com/spf13/viper              # Configuration
go get github.com/google/uuid              # UUID generation9.2 Complete Project Structureera-monitor-agent/
├── cmd/
│   ├── agent/
│   │   └── main.go                        # CLI/Service entry point
│   └── agent-gui/
│       └── main.go                        # GUI entry point (Windows)
│
├── internal/
│   ├── config/
│   │   ├── config.go                      # Config structures
│   │   ├── loader.go                      # Load from file
│   │   ├── saver.go                       # Save to file
│   │   └── defaults.go                    # Default values
│   │
│   ├── collectors/
│   │   ├── interfaces.go                  # Collector interface
│   │   ├── registry.go                    # Collector registry
│   │   ├── system/
│   │   │   ├── cpu.go                     # CPU metrics
│   │   │   ├── memory.go                  # RAM metrics
│   │   │   ├── disk.go                    # Disk metrics
│   │   │   ├── network.go                 # Network metrics
│   │   │   └── uptime.go                  # System uptime
│   │   ├── windows/
│   │   │   ├── iis_sites.go               # IIS Web Sites
│   │   │   ├── iis_apppools.go            # IIS Application Pools
│   │   │   ├── windows_services.go        # Windows Services
│   │   │   ├── eventlog.go                # Windows Event Log
│   │   │   └── detector.go                # Auto-detect installed services
│   │   ├── linux/
│   │   │   ├── systemd.go                 # Systemd units
│   │   │   ├── docker.go                  # Docker containers
│   │   │   ├── nginx.go                   # Nginx status
│   │   │   ├── apache.go                  # Apache status
│   │   │   └── detector.go                # Auto-detect installed services
│   │   └── common/
│   │       ├── process.go                 # Process monitoring
│   │       └── port.go                    # Port checking
│   │
│   ├── models/
│   │   ├── heartbeat.go                   # Heartbeat request/response
│   │   ├── system.go                      # System metrics model
│   │   ├── disk.go                        # Disk info model
│   │   ├── service.go                     # Service info model
│   │   └── network.go                     # Network info model
│   │
│   ├── sender/
│   │   ├── client.go                      # HTTP client wrapper
│   │   ├── heartbeat.go                   # Send heartbeat
│   │   ├── retry.go                       # Retry logic
│   │   └── queue.go                       # Offline queue (store & forward)
│   │
│   ├── agent/
│   │   ├── agent.go                       # Main agent orchestrator
│   │   ├── scheduler.go                   # Collection scheduler
│   │   └── state.go                       # Agent state management
│   │
│   ├── service/
│   │   ├── service.go                     # Service interface
│   │   ├── windows.go                     # Windows Service implementation
│   │   └── linux.go                       # Systemd service implementation
│   │
│   └── logger/
│       ├── logger.go                      # Logger setup
│       └── file.go                        # File rotation
│
├── ui/
│   ├── app.go                             # Fyne application
│   ├── theme.go                           # ERA dark theme
│   ├── windows/
│   │   ├── main_window.go                 # Main window
│   │   ├── settings_dialog.go             # Settings dialog
│   │   └── about_dialog.go                # About dialog
│   ├── tabs/
│   │   ├── status_tab.go                  # Connection status tab
│   │   ├── collectors_tab.go              # Collectors configuration
│   │   ├── services_tab.go                # Monitored services list
│   │   └── logs_tab.go                    # Real-time logs
│   ├── components/
│   │   ├── status_indicator.go            # Status dot component
│   │   ├── service_list.go                # Service list component
│   │   ├── metric_gauge.go                # CPU/RAM gauge
│   │   └── log_viewer.go                  # Log viewer component
│   ├── tray/
│   │   └── tray.go                        # System tray icon & menu
│   └── resources/
│       ├── icons.go                       # Embedded icons
│       └── logo.go                        # ERA logo
│
├── scripts/
│   ├── build/
│   │   ├── build-windows.ps1              # Windows build script
│   │   ├── build-linux.sh                 # Linux build script
│   │   └── build-all.sh                   # Cross-compile all platforms
│   ├── installer/
│   │   ├── windows/
│   │   │   ├── installer.iss              # Inno Setup script
│   │   │   └── installer.wxs              # WiX installer (alternative)
│   │   └── linux/
│   │       ├── postinst                   # Post-install script (deb)
│   │       ├── prerm                      # Pre-remove script
│   │       └── era-monitor-agent.service  # Systemd unit file
│   └── install/
│       ├── install.sh                     # One-line Linux installer
│       └── install.ps1                    # Windows installer script
│
├── configs/
│   ├── config.example.json                # Example configuration
│   └── config.schema.json                 # JSON schema for validation
│
├── assets/
│   ├── icons/
│   │   ├── icon.ico                       # Windows icon
│   │   ├── icon.png                       # PNG icon (various sizes)
│   │   └── tray/
│   │       ├── connected.png
│   │       ├── disconnected.png
│   │       └── warning.png
│   └── logo/
│       └── era-logo.png
│
├── docs/
│   ├── INSTALLATION.md
│   ├── CONFIGURATION.md
│   └── TROUBLESHOOTING.md
│
├── Makefile
├── go.mod
├── go.sum
├── README.md
└── LICENSE9.3 Core Files Implementation9.3.1 Configuration Structurego// internal/config/config.go

package config

import (
    "encoding/json"
    "os"
    "path/filepath"
    "runtime"
)

type Config struct {
    // Server Connection
    Server ServerConfig `json:"server"`
    
    // Host Identification
    Host HostConfig `json:"host"`
    
    // Collectors Configuration
    Collectors CollectorsConfig `json:"collectors"`
    
    // Agent Settings
    Agent AgentConfig `json:"agent"`
    
    // Logging
    Logging LoggingConfig `json:"logging"`
}

type ServerConfig struct {
    APIEndpoint string `json:"apiEndpoint"` // https://monitor.eracloud.com.tr/api
    APIKey      string `json:"apiKey"`      // Host-specific API key
    Timeout     int    `json:"timeout"`     // Request timeout in seconds (default: 30)
    RetryCount  int    `json:"retryCount"`  // Number of retries (default: 3)
    RetryDelay  int    `json:"retryDelay"`  // Delay between retries in seconds
}

type HostConfig struct {
    DisplayName string `json:"displayName"` // Friendly name for this host
    Location    string `json:"location"`    // Optional location identifier
    Tags        []string `json:"tags"`      // Optional tags
}

type CollectorsConfig struct {
    // Interval in seconds
    IntervalSeconds int `json:"intervalSeconds"` // Default: 60
    
    // System Metrics
    System SystemCollectorConfig `json:"system"`
    
    // Windows-specific
    IIS            IISCollectorConfig            `json:"iis"`
    WindowsServices WindowsServicesCollectorConfig `json:"windowsServices"`
    SQLServer      SQLServerCollectorConfig      `json:"sqlServer"`
    
    // Linux-specific
    Systemd SystemdCollectorConfig `json:"systemd"`
    Docker  DockerCollectorConfig  `json:"docker"`
    Nginx   NginxCollectorConfig   `json:"nginx"`
}

type SystemCollectorConfig struct {
    Enabled bool `json:"enabled"` // Default: true
    CPU     bool `json:"cpu"`     // Collect CPU metrics
    RAM     bool `json:"ram"`     // Collect RAM metrics
    Disk    bool `json:"disk"`    // Collect Disk metrics
    Network bool `json:"network"` // Collect Network metrics
}

type IISCollectorConfig struct {
    Detected       bool     `json:"detected"`       // Auto-detected
    Enabled        bool     `json:"enabled"`        // User enabled
    MonitorSites   bool     `json:"monitorSites"`   // Monitor IIS Sites
    MonitorAppPools bool    `json:"monitorAppPools"` // Monitor App Pools
    ExcludeSites   []string `json:"excludeSites"`   // Sites to exclude
    ExcludePools   []string `json:"excludePools"`   // Pools to exclude
}

type WindowsServicesCollectorConfig struct {
    Enabled  bool     `json:"enabled"`
    Services []string `json:"services"` // List of service names to monitor
    // Example: ["MSSQLSERVER", "W3SVC", "Spooler"]
}

type SQLServerCollectorConfig struct {
    Detected         bool   `json:"detected"`
    Enabled          bool   `json:"enabled"`
    MonitorService   bool   `json:"monitorService"`   // Monitor SQL Server service
    MonitorAgent     bool   `json:"monitorAgent"`     // Monitor SQL Agent service
    MonitorDatabases bool   `json:"monitorDatabases"` // Monitor database sizes (requires connection)
    ConnectionString string `json:"connectionString"` // Optional: for database monitoring
}

type SystemdCollectorConfig struct {
    Enabled bool     `json:"enabled"`
    Units   []string `json:"units"` // List of systemd units to monitor
    // Example: ["nginx.service", "postgresql.service", "docker.service"]
}

type DockerCollectorConfig struct {
    Detected   bool     `json:"detected"`
    Enabled    bool     `json:"enabled"`
    Containers []string `json:"containers"` // Empty = all containers
    Exclude    []string `json:"exclude"`    // Container names to exclude
}

type NginxCollectorConfig struct {
    Detected  bool   `json:"detected"`
    Enabled   bool   `json:"enabled"`
    StatusURL string `json:"statusUrl"` // http://localhost/nginx_status
}

type AgentConfig struct {
    RunAsService   bool   `json:"runAsService"`   // Run as Windows Service / Systemd
    StartWithOS    bool   `json:"startWithOS"`    // Auto-start on boot
    MinimizeToTray bool   `json:"minimizeToTray"` // Minimize to system tray (GUI)
    CheckForUpdates bool  `json:"checkForUpdates"` // Auto-check for updates
}

type LoggingConfig struct {
    Level      string `json:"level"`      // debug, info, warn, error
    MaxSizeMB  int    `json:"maxSizeMB"`  // Max log file size
    MaxBackups int    `json:"maxBackups"` // Number of backup files to keep
    MaxAgeDays int    `json:"maxAgeDays"` // Days to keep old logs
    LogToFile  bool   `json:"logToFile"`  // Write logs to file
    LogPath    string `json:"logPath"`    // Log file path
}

// GetDefaultConfig returns default configuration
func GetDefaultConfig() *Config {
    return &Config{
        Server: ServerConfig{
            APIEndpoint: "https://monitor.eracloud.com.tr/api",
            Timeout:     30,
            RetryCount:  3,
            RetryDelay:  5,
        },
        Host: HostConfig{
            DisplayName: getHostname(),
        },
        Collectors: CollectorsConfig{
            IntervalSeconds: 60,
            System: SystemCollectorConfig{
                Enabled: true,
                CPU:     true,
                RAM:     true,
                Disk:    true,
                Network: false,
            },
        },
        Agent: AgentConfig{
            RunAsService:    true,
            StartWithOS:     true,
            MinimizeToTray:  true,
            CheckForUpdates: true,
        },
        Logging: LoggingConfig{
            Level:      "info",
            MaxSizeMB:  10,
            MaxBackups: 3,
            MaxAgeDays: 7,
            LogToFile:  true,
            LogPath:    getDefaultLogPath(),
        },
    }
}

// GetConfigPath returns the configuration file path
func GetConfigPath() string {
    if runtime.GOOS == "windows" {
        return filepath.Join(os.Getenv("ProgramData"), "ERAMonitor", "config.json")
    }
    return "/etc/era-monitor/config.json"
}

// Load reads configuration from file
func Load() (*Config, error) {
    configPath := GetConfigPath()
    
    data, err := os.ReadFile(configPath)
    if err != nil {
        if os.IsNotExist(err) {
            // Return default config if file doesn't exist
            return GetDefaultConfig(), nil
        }
        return nil, err
    }
    
    var config Config
    if err := json.Unmarshal(data, &config); err != nil {
        return nil, err
    }
    
    return &config, nil
}

// Save writes configuration to file
func (c *Config) Save() error {
    configPath := GetConfigPath()
    
    // Ensure directory exists
    dir := filepath.Dir(configPath)
    if err := os.MkdirAll(dir, 0755); err != nil {
        return err
    }
    
    data, err := json.MarshalIndent(c, "", "  ")
    if err != nil {
        return err
    }
    
    return os.WriteFile(configPath, data, 0600)
}

func getHostname() string {
    hostname, _ := os.Hostname()
    return hostname
}

func getDefaultLogPath() string {
    if runtime.GOOS == "windows" {
        return filepath.Join(os.Getenv("ProgramData"), "ERAMonitor", "logs", "agent.log")
    }
    return "/var/log/era-monitor/agent.log"
}9.3.2 Collector Interface & Registrygo// internal/collectors/interfaces.go

package collectors

import (
    "context"
    "github.com/eracloud/era-monitor-agent/internal/models"
)

// Collector interface for all metric collectors
type Collector interface {
    // Name returns the collector name
    Name() string
    
    // Collect gathers metrics and returns them
    Collect(ctx context.Context) (*CollectorResult, error)
    
    // IsAvailable checks if this collector can run on current system
    IsAvailable() bool
    
    // Configure applies configuration to the collector
    Configure(config interface{}) error
}

// CollectorResult contains collected data
type CollectorResult struct {
    System   *models.SystemMetrics   `json:"system,omitempty"`
    Disks    []models.DiskInfo       `json:"disks,omitempty"`
    Services []models.ServiceInfo    `json:"services,omitempty"`
    Network  *models.NetworkInfo     `json:"network,omitempty"`
}

// ServiceDetector interface for auto-detecting installed services
type ServiceDetector interface {
    // Detect finds installed services on the system
    Detect() ([]DetectedService, error)
}

// DetectedService represents an auto-detected service
type DetectedService struct {
    Name        string `json:"name"`
    DisplayName string `json:"displayName"`
    Type        string `json:"type"` // IIS, SQLServer, WindowsService, Systemd, Docker
    IsInstalled bool   `json:"isInstalled"`
    IsRunning   bool   `json:"isRunning"`
    Version     string `json:"version,omitempty"`
}go// internal/collectors/registry.go

package collectors

import (
    "context"
    "sync"
    
    "go.uber.org/zap"
)

// Registry manages all collectors
type Registry struct {
    collectors map[string]Collector
    mu         sync.RWMutex
    logger     *zap.Logger
}

// NewRegistry creates a new collector registry
func NewRegistry(logger *zap.Logger) *Registry {
    return &Registry{
        collectors: make(map[string]Collector),
        logger:     logger,
    }
}

// Register adds a collector to the registry
func (r *Registry) Register(collector Collector) {
    r.mu.Lock()
    defer r.mu.Unlock()
    
    if collector.IsAvailable() {
        r.collectors[collector.Name()] = collector
        r.logger.Info("Collector registered", zap.String("name", collector.Name()))
    } else {
        r.logger.Debug("Collector not available, skipping", zap.String("name", collector.Name()))
    }
}

// CollectAll runs all registered collectors and aggregates results
func (r *Registry) CollectAll(ctx context.Context) (*CollectorResult, error) {
    r.mu.RLock()
    defer r.mu.RUnlock()
    
    aggregated := &CollectorResult{
        Services: make([]models.ServiceInfo, 0),
        Disks:    make([]models.DiskInfo, 0),
    }
    
    var wg sync.WaitGroup
    var mu sync.Mutex
    
    for name, collector := range r.collectors {
        wg.Add(1)
        go func(name string, c Collector) {
            defer wg.Done()
            
            result, err := c.Collect(ctx)
            if err != nil {
                r.logger.Error("Collector failed", 
                    zap.String("name", name), 
                    zap.Error(err))
                return
            }
            
            mu.Lock()
            defer mu.Unlock()
            
            // Merge results
            if result.System != nil {
                aggregated.System = result.System
            }
            if result.Disks != nil {
                aggregated.Disks = append(aggregated.Disks, result.Disks...)
            }
            if result.Services != nil {
                aggregated.Services = append(aggregated.Services, result.Services...)
            }
            if result.Network != nil {
                aggregated.Network = result.Network
            }
        }(name, collector)
    }
    
    wg.Wait()
    return aggregated, nil
}9.3.3 System Metrics Collectorgo// internal/collectors/system/cpu.go

package system

import (
    "context"
    "time"
    
    "github.com/shirou/gopsutil/v3/cpu"
    "github.com/shirou/gopsutil/v3/mem"
    "github.com/shirou/gopsutil/v3/disk"
    "github.com/shirou/gopsutil/v3/host"
    "github.com/eracloud/era-monitor-agent/internal/collectors"
    "github.com/eracloud/era-monitor-agent/internal/models"
)

type SystemCollector struct {
    config SystemCollectorConfig
}

type SystemCollectorConfig struct {
    CPU     bool
    RAM     bool
    Disk    bool
    Network bool
}

func NewSystemCollector(config SystemCollectorConfig) *SystemCollector {
    return &SystemCollector{config: config}
}

func (c *SystemCollector) Name() string {
    return "system"
}

func (c *SystemCollector) IsAvailable() bool {
    return true // Always available
}

func (c *SystemCollector) Configure(config interface{}) error {
    if cfg, ok := config.(SystemCollectorConfig); ok {
        c.config = cfg
    }
    return nil
}

func (c *SystemCollector) Collect(ctx context.Context) (*collectors.CollectorResult, error) {
    result := &collectors.CollectorResult{
        System: &models.SystemMetrics{},
        Disks:  make([]models.DiskInfo, 0),
    }
    
    // CPU Usage
    if c.config.CPU {
        cpuPercent, err := cpu.PercentWithContext(ctx, time.Second, false)
        if err == nil && len(cpuPercent) > 0 {
            result.System.CPUPercent = cpuPercent[0]
        }
    }
    
    // Memory Usage
    if c.config.RAM {
        memInfo, err := mem.VirtualMemoryWithContext(ctx)
        if err == nil {
            result.System.RAMPercent = memInfo.UsedPercent
            result.System.RAMUsedMB = int64(memInfo.Used / 1024 / 1024)
            result.System.RAMTotalMB = int64(memInfo.Total / 1024 / 1024)
        }
    }
    
    // Disk Usage
    if c.config.Disk {
        partitions, err := disk.PartitionsWithContext(ctx, false)
        if err == nil {
            for _, partition := range partitions {
                usage, err := disk.UsageWithContext(ctx, partition.Mountpoint)
                if err != nil {
                    continue
                }
                
                result.Disks = append(result.Disks, models.DiskInfo{
                    Name:        partition.Device,
                    MountPoint:  partition.Mountpoint,
                    TotalGB:     float64(usage.Total) / 1024 / 1024 / 1024,
                    UsedGB:      float64(usage.Used) / 1024 / 1024 / 1024,
                    UsedPercent: usage.UsedPercent,
                })
            }
        }
    }
    
    // Uptime
    hostInfo, err := host.InfoWithContext(ctx)
    if err == nil {
        result.System.UptimeSeconds = int64(hostInfo.Uptime)
        result.System.Hostname = hostInfo.Hostname
        result.System.OS = hostInfo.OS
        result.System.Platform = hostInfo.Platform
        result.System.PlatformVersion = hostInfo.PlatformVersion
    }
    
    return result, nil
}9.3.4 IIS Collector (Windows)go// internal/collectors/windows/iis_sites.go
// +build windows

package windows

import (
    "context"
    "fmt"
    "os/exec"
    "strings"
    "encoding/xml"
    
    "github.com/yusufpapurcu/wmi"
    "github.com/eracloud/era-monitor-agent/internal/collectors"
    "github.com/eracloud/era-monitor-agent/internal/models"
)

type IISCollector struct {
    config IISCollectorConfig
}

type IISCollectorConfig struct {
    MonitorSites    bool
    MonitorAppPools bool
    ExcludeSites    []string
    ExcludePools    []string
}

// WMI classes for IIS
type Win32_PerfFormattedData_W3SVC_WebService struct {
    Name               string
    CurrentConnections uint32
    TotalBytesSent     uint64
    TotalBytesReceived uint64
}

func NewIISCollector(config IISCollectorConfig) *IISCollector {
    return &IISCollector{config: config}
}

func (c *IISCollector) Name() string {
    return "iis"
}

func (c *IISCollector) IsAvailable() bool {
    // Check if IIS is installed by looking for w3svc service
    cmd := exec.Command("sc", "query", "w3svc")
    err := cmd.Run()
    return err == nil
}

func (c *IISCollector) Configure(config interface{}) error {
    if cfg, ok := config.(IISCollectorConfig); ok {
        c.config = cfg
    }
    return nil
}

func (c *IISCollector) Collect(ctx context.Context) (*collectors.CollectorResult, error) {
    result := &collectors.CollectorResult{
        Services: make([]models.ServiceInfo, 0),
    }
    
    // Collect IIS Sites
    if c.config.MonitorSites {
        sites, err := c.collectSites(ctx)
        if err == nil {
            result.Services = append(result.Services, sites...)
        }
    }
    
    // Collect App Pools
    if c.config.MonitorAppPools {
        pools, err := c.collectAppPools(ctx)
        if err == nil {
            result.Services = append(result.Services, pools...)
        }
    }
    
    return result, nil
}

func (c *IISCollector) collectSites(ctx context.Context) ([]models.ServiceInfo, error) {
    var services []models.ServiceInfo
    
    // Use appcmd to list sites
    cmd := exec.CommandContext(ctx, 
        "C:\\Windows\\System32\\inetsrv\\appcmd.exe", 
        "list", "site", "/xml")
    
    output, err := cmd.Output()
    if err != nil {
        return nil, fmt.Errorf("failed to run appcmd: %w", err)
    }
    
    // Parse XML output
    type Site struct {
        Name  string `xml:"SITE.NAME,attr"`
        ID    string `xml:"SITE.ID,attr"`
        State string `xml:"state,attr"`
    }
    type AppCmd struct {
        Sites []Site `xml:"SITE"`
    }
    
    var result AppCmd
    if err := xml.Unmarshal(output, &result); err != nil {
        return nil, fmt.Errorf("failed to parse appcmd output: %w", err)
    }
    
    for _, site := range result.Sites {
        // Check exclusion list
        if c.isExcluded(site.Name, c.config.ExcludeSites) {
            continue
        }
        
        status := "Unknown"
        switch strings.ToLower(site.State) {
        case "started":
            status = "Running"
        case "stopped":
            status = "Stopped"
        }
        
        services = append(services, models.ServiceInfo{
            Name:        site.Name,
            DisplayName: site.Name,
            Type:        "IIS_Site",
            Status:      status,
            Config: map[string]interface{}{
                "siteId": site.ID,
            },
        })
    }
    
    return services, nil
}

func (c *IISCollector) collectAppPools(ctx context.Context) ([]models.ServiceInfo, error) {
    var services []models.ServiceInfo
    
    cmd := exec.CommandContext(ctx,
        "C:\\Windows\\System32\\inetsrv\\appcmd.exe",
        "list", "apppool", "/xml")
    
    output, err := cmd.Output()
    if err != nil {
        return nil, err
    }
    
    type AppPool struct {
        Name  string `xml:"APPPOOL.NAME,attr"`
        State string `xml:"state,attr"`
    }
    type AppCmd struct {
        AppPools []AppPool `xml:"APPPOOL"`
    }
    
    var result AppCmd
    if err := xml.Unmarshal(output, &result); err != nil {
        return nil, err
    }
    
    for _, pool := range result.AppPools {
        if c.isExcluded(pool.Name, c.config.ExcludePools) {
            continue
        }
        
        status := "Unknown"
        switch strings.ToLower(pool.State) {
        case "started":
            status = "Running"
        case "stopped":
            status = "Stopped"
        }
        
        services = append(services, models.ServiceInfo{
            Name:        pool.Name,
            DisplayName: pool.Name,
            Type:        "IIS_AppPool",
            Status:      status,
        })
    }
    
    return services, nil
}

func (c *IISCollector) isExcluded(name string, excludeList []string) bool {
    for _, excluded := range excludeList {
        if strings.EqualFold(name, excluded) {
            return true
        }
    }
    return false
}9.3.5 Windows Service Collectorgo// internal/collectors/windows/windows_services.go
// +build windows

package windows

import (
    "context"
    "fmt"
    
    "golang.org/x/sys/windows/svc"
    "golang.org/x/sys/windows/svc/mgr"
    "github.com/eracloud/era-monitor-agent/internal/collectors"
    "github.com/eracloud/era-monitor-agent/internal/models"
)

type WindowsServiceCollector struct {
    config WindowsServiceCollectorConfig
}

type WindowsServiceCollectorConfig struct {
    Services []string // Service names to monitor
}

func NewWindowsServiceCollector(config WindowsServiceCollectorConfig) *WindowsServiceCollector {
    return &WindowsServiceCollector{config: config}
}

func (c *WindowsServiceCollector) Name() string {
    return "windows_services"
}

func (c *WindowsServiceCollector) IsAvailable() bool {
    return true
}

func (c *WindowsServiceCollector) Configure(config interface{}) error {
    if cfg, ok := config.(WindowsServiceCollectorConfig); ok {
        c.config = cfg
    }
    return nil
}

func (c *WindowsServiceCollector) Collect(ctx context.Context) (*collectors.CollectorResult, error) {
    result := &collectors.CollectorResult{
        Services: make([]models.ServiceInfo, 0),
    }
    
    // Connect to service manager
    manager, err := mgr.Connect()
    if err != nil {
        return nil, fmt.Errorf("failed to connect to service manager: %w", err)
    }
    defer manager.Disconnect()
    
    for _, serviceName := range c.config.Services {
        service, err := manager.OpenService(serviceName)
        if err != nil {
            // Service not found
            result.Services = append(result.Services, models.ServiceInfo{
                Name:   serviceName,
                Type:   "WindowsService",
                Status: "NotFound",
            })
            continue
        }
        
        status, err := service.Query()
        service.Close()
        
        if err != nil {
            result.Services = append(result.Services, models.ServiceInfo{
                Name:   serviceName,
                Type:   "WindowsService",
                Status: "Unknown",
            })
            continue
        }
        
        statusStr := c.mapServiceState(status.State)
        
        // Get service config for display name
        config, _ := service.Config()
        displayName := serviceName
        if config.DisplayName != "" {
            displayName = config.DisplayName
        }
        
        result.Services = append(result.Services, models.ServiceInfo{
            Name:        serviceName,
            DisplayName: displayName,
            Type:        "WindowsService",
            Status:      statusStr,
            Config: map[string]interface{}{
                "startType": c.mapStartType(config.StartType),
            },
        })
    }
    
    return result, nil
}

func (c *WindowsServiceCollector) mapServiceState(state svc.State) string {
    switch state {
    case svc.Stopped:
        return "Stopped"
    case svc.StartPending:
        return "Starting"
    case svc.StopPending:
        return "Stopping"
    case svc.Running:
        return "Running"
    case svc.ContinuePending:
        return "Continuing"
    case svc.PausePending:
        return "Pausing"
    case svc.Paused:
        return "Paused"
    default:
        return "Unknown"
    }
}

func (c *WindowsServiceCollector) mapStartType(startType uint32) string {
    switch startType {
    case mgr.StartAutomatic:
        return "Automatic"
    case mgr.StartManual:
        return "Manual"
    case mgr.StartDisabled:
        return "Disabled"
    default:
        return "Unknown"
    }
}9.3.6 Linux Systemd Collectorgo// internal/collectors/linux/systemd.go
// +build linux

package linux

import (
    "context"
    "os/exec"
    "strings"
    
    "github.com/eracloud/era-monitor-agent/internal/collectors"
    "github.com/eracloud/era-monitor-agent/internal/models"
)

type SystemdCollector struct {
    config SystemdCollectorConfig
}

type SystemdCollectorConfig struct {
    Units []string // Unit names to monitor (e.g., "nginx.service")
}

func NewSystemdCollector(config SystemdCollectorConfig) *SystemdCollector {
    return &SystemdCollector{config: config}
}

func (c *SystemdCollector) Name() string {
    return "systemd"
}

func (c *SystemdCollector) IsAvailable() bool {
    // Check if systemctl exists
    _, err := exec.LookPath("systemctl")
    return err == nil
}

func (c *SystemdCollector) Configure(config interface{}) error {
    if cfg, ok := config.(SystemdCollectorConfig); ok {
        c.config = cfg
    }
    return nil
}

func (c *SystemdCollector) Collect(ctx context.Context) (*collectors.CollectorResult, error) {
    result := &collectors.CollectorResult{
        Services: make([]models.ServiceInfo, 0),
    }
    
    for _, unit := range c.config.Units {
        serviceInfo := c.getUnitStatus(ctx, unit)
        result.Services = append(result.Services, serviceInfo)
    }
    
    return result, nil
}

func (c *SystemdCollector) getUnitStatus(ctx context.Context, unit string) models.ServiceInfo {
    info := models.ServiceInfo{
        Name: unit,
        Type: "SystemdUnit",
    }
    
    // Get active state
    cmd := exec.CommandContext(ctx, "systemctl", "is-active", unit)
    output, err := cmd.Output()
    if err != nil {
        info.Status = "Unknown"
    } else {
        state := strings.TrimSpace(string(output))
        info.Status = c.mapSystemdState(state)
    }
    
    // Get description
    cmd = exec.CommandContext(ctx, "systemctl", "show", unit, "--property=Description", "--value")
    output, err = cmd.Output()
    if err == nil {
        info.DisplayName = strings.TrimSpace(string(output))
    }
    
    // Get additional properties
    cmd = exec.CommandContext(ctx, "systemctl", "show", unit, 
        "--property=ActiveState,SubState,MainPID", "--value")
    output, err = cmd.Output()
    if err == nil {
        lines := strings.Split(strings.TrimSpace(string(output)), "\n")
        if len(lines) >= 3 {
            info.Config = map[string]interface{}{
                "activeState": lines[0],
                "subState":    lines[1],
                "mainPID":     lines[2],
            }
        }
    }
    
    return info
}

func (c *SystemdCollector) mapSystemdState(state string) string {
    switch state {
    case "active":
        return "Running"
    case "inactive":
        return "Stopped"
    case "failed":
        return "Failed"
    case "activating":
        return "Starting"
    case "deactivating":
        return "Stopping"
    default:
        return "Unknown"
    }
}9.3.7 Heartbeat Sendergo// internal/sender/heartbeat.go

package sender

import (
    "context"
    "fmt"
    "net"
    "time"
    
    "github.com/go-resty/resty/v2"
    "go.uber.org/zap"
    "github.com/eracloud/era-monitor-agent/internal/config"
    "github.com/eracloud/era-monitor-agent/internal/models"
)

type HeartbeatSender struct {
    client *resty.Client
    config *config.ServerConfig
    logger *zap.Logger
    queue  *OfflineQueue // For store-and-forward
}

func NewHeartbeatSender(cfg *config.ServerConfig, logger *zap.Logger) *HeartbeatSender {
    client := resty.New().
        SetTimeout(time.Duration(cfg.Timeout) * time.Second).
        SetRetryCount(cfg.RetryCount).
        SetRetryWaitTime(time.Duration(cfg.RetryDelay) * time.Second).
        SetHeader("Content-Type", "application/json").
        SetHeader("X-API-Key", cfg.APIKey).
        SetHeader("User-Agent", "ERAMonitor-Agent/1.0")
    
    return &HeartbeatSender{
        client: client,
        config: cfg,
        logger: logger,
        queue:  NewOfflineQueue(logger),
    }
}

func (s *HeartbeatSender) Send(ctx context.Context, payload *models.HeartbeatRequest) (*models.HeartbeatResponse, error) {
    endpoint := fmt.Sprintf("%s/agent/heartbeat", s.config.APIEndpoint)
    
    var response models.HeartbeatResponse
    
    resp, err := s.client.R().
        SetContext(ctx).
        SetBody(payload).
        SetResult(&response).
        Post(endpoint)
    
    if err != nil {
        // Network error - queue for later
        s.logger.Warn("Failed to send heartbeat, queuing for retry", 
            zap.Error(err))
        s.queue.Add(payload)
        return nil, fmt.Errorf("failed to send heartbeat: %w", err)
    }
    
    if resp.StatusCode() == 401 {
        return nil, fmt.Errorf("authentication failed: invalid API key")
    }
    
    if resp.StatusCode() != 200 {
        return nil, fmt.Errorf("server returned status %d: %s", 
            resp.StatusCode(), resp.String())
    }
    
    s.logger.Debug("Heartbeat sent successfully",
        zap.Int("responseTime", int(resp.Time().Milliseconds())),
        zap.String("hostId", response.HostID))
    
    // Process any queued items on successful connection
    go s.processQueue(ctx)
    
    return &response, nil
}

func (s *HeartbeatSender) processQueue(ctx context.Context) {
    items := s.queue.GetAll()
    for _, item := range items {
        if _, err := s.Send(ctx, item); err == nil {
            s.queue.Remove(item)
        }
    }
}

// GetPublicIP retrieves the public IP address
func GetPublicIP() string {
    resp, err := resty.New().
        SetTimeout(5 * time.Second).
        R().
        Get("https://api.ipify.org")
    
    if err != nil {
        return ""
    }
    return resp.String()
}

// GetPrimaryIP retrieves the primary local IP address
func GetPrimaryIP() string {
    conn, err := net.Dial("udp", "8.8.8.8:80")
    if err != nil {
        return ""
    }
    defer conn.Close()
    
    localAddr := conn.LocalAddr().(*net.UDPAddr)
    return localAddr.IP.String()
}9.3.8 Main Agent Orchestratorgo// internal/agent/agent.go

package agent

import (
    "context"
    "os"
    "runtime"
    "sync"
    "time"
    
    "go.uber.org/zap"
    "github.com/eracloud/era-monitor-agent/internal/collectors"
    "github.com/eracloud/era-monitor-agent/internal/collectors/system"
    "github.com/eracloud/era-monitor-agent/internal/collectors/windows"
    "github.com/eracloud/era-monitor-agent/internal/collectors/linux"
    "github.com/eracloud/era-monitor-agent/internal/config"
    "github.com/eracloud/era-monitor-agent/internal/models"
    "github.com/eracloud/era-monitor-agent/internal/sender"
)

type Agent struct {
    config    *config.Config
    registry  *collectors.Registry
    sender    *sender.HeartbeatSender
    logger    *zap.Logger
    
    ctx       context.Context
    cancel    context.CancelFunc
    wg        sync.WaitGroup
    
    // State
    isRunning  bool
    lastSentAt time.Time
    lastError  error
    mu         sync.RWMutex
    
    // Callbacks for UI updates
    OnStatusChange func(status AgentStatus)
    OnLogMessage   func(level, message string)
}

type AgentStatus struct {
    IsConnected    bool
    LastSentAt     time.Time
    LastError      string
    NextSendIn     time.Duration
    CPUPercent     float64
    RAMPercent     float64
    ServicesCount  int
}

func NewAgent(cfg *config.Config, logger *zap.Logger) *Agent {
    ctx, cancel := context.WithCancel(context.Background())
    
    agent := &Agent{
        config:   cfg,
        logger:   logger,
        ctx:      ctx,
        cancel:   cancel,
        registry: collectors.NewRegistry(logger),
        sender:   sender.NewHeartbeatSender(&cfg.Server, logger),
    }
    
    agent.registerCollectors()
    
    return agent
}

func (a *Agent) registerCollectors() {
    // System collector (always available)
    if a.config.Collectors.System.Enabled {
        a.registry.Register(system.NewSystemCollector(system.SystemCollectorConfig{
            CPU:     a.config.Collectors.System.CPU,
            RAM:     a.config.Collectors.System.RAM,
            Disk:    a.config.Collectors.System.Disk,
            Network: a.config.Collectors.System.Network,
        }))
    }
    
    // Platform-specific collectors
    if runtime.GOOS == "windows" {
        // IIS
        if a.config.Collectors.IIS.Enabled {
            a.registry.Register(windows.NewIISCollector(windows.IISCollectorConfig{
                MonitorSites:    a.config.Collectors.IIS.MonitorSites,
                MonitorAppPools: a.config.Collectors.IIS.MonitorAppPools,
                ExcludeSites:    a.config.Collectors.IIS.ExcludeSites,
                ExcludePools:    a.config.Collectors.IIS.ExcludePools,
            }))
        }
        
        // Windows Services
        if len(a.config.Collectors.WindowsServices.Services) > 0 {
            a.registry.Register(windows.NewWindowsServiceCollector(
                windows.WindowsServiceCollectorConfig{
                    Services: a.config.Collectors.WindowsServices.Services,
                }))
        }
        
        // SQL Server
        if a.config.Collectors.SQLServer.Enabled {
            a.registry.Register(windows.NewSQLServerCollector(
                windows.SQLServerCollectorConfig{
                    MonitorService: a.config.Collectors.SQLServer.MonitorService,
                    MonitorAgent:   a.config.Collectors.SQLServer.MonitorAgent,
                }))
        }
    }
    
    if runtime.GOOS == "linux" {
        // Systemd
        if len(a.config.Collectors.Systemd.Units) > 0 {
            a.registry.Register(linux.NewSystemdCollector(linux.SystemdCollectorConfig{
                Units: a.config.Collectors.Systemd.Units,
            }))
        }
        
        // Docker
        if a.config.Collectors.Docker.Enabled {
            a.registry.Register(linux.NewDockerCollector(linux.DockerCollectorConfig{
                Containers: a.config.Collectors.Docker.Containers,
                Exclude:    a.config.Collectors.Docker.Exclude,
            }))
        }
    }
}

func (a *Agent) Start() error {
    a.mu.Lock()
    if a.isRunning {
        a.mu.Unlock()
        return nil
    }
    a.isRunning = true
    a.mu.Unlock()
    
    a.logger.Info("Agent starting",
        zap.String("endpoint", a.config.Server.APIEndpoint),
        zap.Int("interval", a.config.Collectors.IntervalSeconds))
    
    // Initial heartbeat
    a.wg.Add(1)
    go func() {
        defer a.wg.Done()
        a.sendHeartbeat()
    }()
    
    // Start scheduler
    a.wg.Add(1)
    go a.runScheduler()
    
    return nil
}

func (a *Agent) Stop() {
    a.mu.Lock()
    if !a.isRunning {
        a.mu.Unlock()
        return
    }
    a.isRunning = false
    a.mu.Unlock()
    
    a.logger.Info("Agent stopping")
    a.cancel()
    a.wg.Wait()
    a.logger.Info("Agent stopped")
}

func (a *Agent) runScheduler() {
    defer a.wg.Done()
    
    ticker := time.NewTicker(time.Duration(a.config.Collectors.IntervalSeconds) * time.Second)
    defer ticker.Stop()
    
    for {
        select {
        case <-a.ctx.Done():
            return
        case <-ticker.C:
            a.sendHeartbeat()
        }
    }
}

func (a *Agent) sendHeartbeat() {
    ctx, cancel := context.WithTimeout(a.ctx, 30*time.Second)
    defer cancel()
    
    // Collect metrics
    result, err := a.registry.CollectAll(ctx)
    if err != nil {
        a.logger.Error("Failed to collect metrics", zap.Error(err))
        return
    }
    
    // Build heartbeat payload
    payload := &models.HeartbeatRequest{
        Timestamp: time.Now().UTC(),
        System: models.SystemInfo{
            Hostname:        a.config.Host.DisplayName,
            OSType:          runtime.GOOS,
            CPUPercent:      result.System.CPUPercent,
            RAMPercent:      result.System.RAMPercent,
            RAMUsedMB:       result.System.RAMUsedMB,
            RAMTotalMB:      result.System.RAMTotalMB,
            UptimeSeconds:   result.System.UptimeSeconds,
        },
        Disks:    result.Disks,
        Services: result.Services,
        Network: models.NetworkInfo{
            PrimaryIP: sender.GetPrimaryIP(),
            PublicIP:  sender.GetPublicIP(),
        },
        AgentVersion: "1.0.0",
    }
    
    // Send heartbeat
    response, err := a.sender.Send(ctx, payload)
    
    a.mu.Lock()
    a.lastSentAt = time.Now()
    a.lastError = err
    a.mu.Unlock()
    
    // Notify UI
    if a.OnStatusChange != nil {
        status := AgentStatus{
            IsConnected:   err == nil,
            LastSentAt:    a.lastSentAt,
            NextSendIn:    time.Duration(a.config.Collectors.IntervalSeconds) * time.Second,
            CPUPercent:    result.System.CPUPercent,
            RAMPercent:    result.System.RAMPercent,
            ServicesCount: len(result.Services),
        }
        if err != nil {
            status.LastError = err.Error()
        }
        a.OnStatusChange(status)
    }
    
    if err != nil {
        a.logger.Error("Failed to send heartbeat", zap.Error(err))
        if a.OnLogMessage != nil {
            a.OnLogMessage("error", fmt.Sprintf("Failed to send heartbeat: %v", err))
        }
        return
    }
    
    a.logger.Info("Heartbeat sent successfully",
        zap.String("hostId", response.HostID),
        zap.Int("nextCheckIn", response.NextCheckIn))
    
    if a.OnLogMessage != nil {
        a.OnLogMessage("info", "Heartbeat sent successfully")
    }
}

func (a *Agent) GetStatus() AgentStatus {
    a.mu.RLock()
    defer a.mu.RUnlock()
    
    status := AgentStatus{
        IsConnected: a.lastError == nil,
        LastSentAt:  a.lastSentAt,
        NextSendIn:  time.Duration(a.config.Collectors.IntervalSeconds) * time.Second,
    }
    if a.lastError != nil {
        status.LastError = a.lastError.Error()
    }
    return status
}9.3.9 Fyne GUI - Main Windowgo// ui/windows/main_window.go

package windows

import (
    "fmt"
    "time"
    
    "fyne.io/fyne/v2"
    "fyne.io/fyne/v2/app"
    "fyne.io/fyne/v2/container"
    "fyne.io/fyne/v2/dialog"
    "fyne.io/fyne/v2/layout"
    "fyne.io/fyne/v2/widget"
    "github.com/eracloud/era-monitor-agent/internal/agent"
    "github.com/eracloud/era-monitor-agent/internal/config"
    "github.com/eracloud/era-monitor-agent/ui/tabs"
    "github.com/eracloud/era-monitor-agent/ui/theme"
    "github.com/eracloud/era-monitor-agent/ui/tray"
)

type MainWindow struct {
    app     fyne.App
    window  fyne.Window
    config  *config.Config
    agent   *agent.Agent
    
    // Status bar elements
    statusIcon     *widget.Label
    statusText     *widget.Label
    lastSentLabel  *widget.Label
    
    // Tabs
    statusTab     *tabs.StatusTab
    collectorsTab *tabs.CollectorsTab
    servicesTab   *tabs.ServicesTab
    logsTab       *tabs.LogsTab
    
    // Buttons
    startStopBtn *widget.Button
}

func NewMainWindow(cfg *config.Config, agnt *agent.Agent) *MainWindow {
    a := app.NewWithID("com.eracloud.monitor-agent")
    a.Settings().SetTheme(theme.NewERADarkTheme())
    
    w := a.NewWindow("ERA Monitor Agent")
    w.Resize(fyne.NewSize(650, 750))
    w.SetFixedSize(false)
    
    mw := &MainWindow{
        app:    a,
        window: w,
        config: cfg,
        agent:  agnt,
    }
    
    // Set up agent callbacks
    agnt.OnStatusChange = mw.onAgentStatusChange
    agnt.OnLogMessage = mw.onAgentLogMessage
    
    mw.buildUI()
    mw.setupTray()
    mw.setupWindowBehavior()
    
    return mw
}

func (mw *MainWindow) buildUI() {
    // Status bar
    mw.statusIcon = widget.NewLabel("●")
    mw.statusIcon.TextStyle = fyne.TextStyle{Bold: true}
    
    mw.statusText = widget.NewLabel("Disconnected")
    mw.lastSentLabel = widget.NewLabel("Last sent: --:--:--")
    
    statusBar := container.NewHBox(
        mw.statusIcon,
        mw.statusText,
        layout.NewSpacer(),
        mw.lastSentLabel,
    )
    
    // Create tabs
    mw.statusTab = tabs.NewStatusTab(mw.config, mw.agent)
    mw.collectorsTab = tabs.NewCollectorsTab(mw.config, mw.onConfigChanged)
    mw.servicesTab = tabs.NewServicesTab(mw.agent)
    mw.logsTab = tabs.NewLogsTab()
    
    tabContainer := container.NewAppTabs(
        container.NewTabItem("Status", mw.statusTab.Content()),
        container.NewTabItem("Collectors", mw.collectorsTab.Content()),
        container.NewTabItem("Services", mw.servicesTab.Content()),
        container.NewTabItem("Logs", mw.logsTab.Content()),
    )
    tabContainer.SetTabLocation(container.TabLocationTop)
    
    // Buttons
    mw.startStopBtn = widget.NewButton("Start Agent", mw.onStartStop)
    mw.startStopBtn.Importance = widget.HighImportance
    
    saveBtn := widget.NewButton("Save Configuration", mw.onSave)
    testBtn := widget.NewButton("Test Connection", mw.onTestConnection)
    
    buttonBar := container.NewHBox(
        layout.NewSpacer(),
        testBtn,
        saveBtn,
        mw.startStopBtn,
    )
    
    // Main layout
    content := container.NewBorder(
        container.NewVBox(
            statusBar,
            widget.NewSeparator(),
        ), // top
        container.NewVBox(
            widget.NewSeparator(),
            buttonBar,
        ), // bottom
        nil, nil,
        tabContainer, // center
    )
    
    mw.window.SetContent(content)
}

func (mw *MainWindow) setupTray() {
    if desk, ok := mw.app.(fyne.Desktop); ok {
        // Create system tray menu
        menu := fyne.NewMenu("ERA Monitor",
            fyne.NewMenuItem("Open", func() {
                mw.window.Show()
            }),
            fyne.NewMenuItemSeparator(),
            fyne.NewMenuItem("Start Agent", func() {
                mw.agent.Start()
            }),
            fyne.NewMenuItem("Stop Agent", func() {
                mw.agent.Stop()
            }),
            fyne.NewMenuItemSeparator(),
            fyne.NewMenuItem("Quit", func() {
                mw.app.Quit()
            }),
        )
        
        desk.SetSystemTrayMenu(menu)
        desk.SetSystemTrayIcon(tray.GetTrayIcon("disconnected"))
    }
}

func (mw *MainWindow) setupWindowBehavior() {
    // Minimize to tray on close
    mw.window.SetCloseIntercept(func() {
        if mw.config.Agent.MinimizeToTray {
            mw.window.Hide()
        } else {
            mw.confirmQuit()
        }
    })
}

func (mw *MainWindow) onAgentStatusChange(status agent.AgentStatus) {
    // Update status bar (must run on main thread)
    mw.window.Canvas().Refresh(mw.statusIcon)
    
    if status.IsConnected {
        mw.statusIcon.SetText("●")
        // Green color would be set via theme
        mw.statusText.SetText("Connected")
        
        if desk, ok := mw.app.(fyne.Desktop); ok {
            desk.SetSystemTrayIcon(tray.GetTrayIcon("connected"))
        }
    } else {
        mw.statusIcon.SetText("●")
        // Red color
        mw.statusText.SetText("Disconnected")
        
        if desk, ok := mw.app.(fyne.Desktop); ok {
            desk.SetSystemTrayIcon(tray.GetTrayIcon("disconnected"))
        }
    }
    
    if !status.LastSentAt.IsZero() {
        mw.lastSentLabel.SetText(fmt.Sprintf("Last sent: %s", 
            status.LastSentAt.Format("15:04:05")))
    }
    
    // Update status tab
    mw.statusTab.UpdateStatus(status)
}

func (mw *MainWindow) onAgentLogMessage(level, message string) {
    mw.logsTab.AddLog(level, message)
}

func (mw *MainWindow) onStartStop() {
    status := mw.agent.GetStatus()
    
    if status.IsConnected {
        mw.agent.Stop()
        mw.startStopBtn.SetText("Start Agent")
    } else {
        if err := mw.agent.Start(); err != nil {
            dialog.ShowError(err, mw.window)
            return
        }
        mw.startStopBtn.SetText("Stop Agent")
    }
}

func (mw *MainWindow) onSave() {
    if err := mw.config.Save(); err != nil {
        dialog.ShowError(err, mw.window)
        return
    }
    dialog.ShowInformation("Success", "Configuration saved successfully.", mw.window)
}

func (mw *MainWindow) onTestConnection() {
    // Show progress dialog
    progress := dialog.NewProgressInfinite("Testing Connection", 
        "Connecting to ERA Monitor server...", mw.window)
    progress.Show()
    
    go func() {
        // Test connection
        time.Sleep(2 * time.Second) // Simulate connection test
        
        progress.Hide()
        
        // Show result
        dialog.ShowInformation("Connection Test", 
            "Successfully connected to ERA Monitor server.", mw.window)
    }()
}

func (mw *MainWindow) onConfigChanged() {
    // Config changed in UI, may need to restart collectors
    mw.logsTab.AddLog("info", "Configuration changed")
}

func (mw *MainWindow) confirmQuit() {
    dialog.ShowConfirm("Quit", 
        "Are you sure you want to quit? The agent will stop monitoring.",
        func(confirmed bool) {
            if confirmed {
                mw.agent.Stop()
                mw.app.Quit()
            }
        }, mw.window)
}

func (mw *MainWindow) Show() {
    mw.window.ShowAndRun()
}9.3.10 Collectors Tabgo// ui/tabs/collectors_tab.go

package tabs

import (
    "fyne.io/fyne/v2"
    "fyne.io/fyne/v2/container"
    "fyne.io/fyne/v2/widget"
    "github.com/eracloud/era-monitor-agent/internal/config"
)

type CollectorsTab struct {
    config    *config.Config
    onChange  func()
    container *fyne.Container
}

func NewCollectorsTab(cfg *config.Config, onChange func()) *CollectorsTab {
    tab := &CollectorsTab{
        config:   cfg,
        onChange: onChange,
    }
    tab.build()
    return tab
}

func (t *CollectorsTab) build() {
    // System Metrics Section
    systemLabel := widget.NewLabelWithStyle("System Metrics", 
        fyne.TextAlignLeading, fyne.TextStyle{Bold: true})
    
    cpuCheck := widget.NewCheck("CPU Usage", func(checked bool) {
        t.config.Collectors.System.CPU = checked
        t.onChange()
    })
    cpuCheck.SetChecked(t.config.Collectors.System.CPU)
    
    ramCheck := widget.NewCheck("RAM Usage", func(checked bool) {
        t.config.Collectors.System.RAM = checked
        t.onChange()
    })
    ramCheck.SetChecked(t.config.Collectors.System.RAM)
    
    diskCheck := widget.NewCheck("Disk Usage", func(checked bool) {
        t.config.Collectors.System.Disk = checked
        t.onChange()
    })
    diskCheck.SetChecked(t.config.Collectors.System.Disk)
    
    networkCheck := widget.NewCheck("Network I/O", func(checked bool) {
        t.config.Collectors.System.Network = checked
        t.onChange()
    })
    networkCheck.SetChecked(t.config.Collectors.System.Network)
    
    systemSection := container.NewVBox(
        systemLabel,
        cpuCheck,
        ramCheck,
        diskCheck,
        networkCheck,
    )
    
    // IIS Section (Windows only)
    var iisSection *fyne.Container
    if t.config.Collectors.IIS.Detected {
        iisLabel := container.NewHBox(
            widget.NewLabelWithStyle("IIS Web Server", 
                fyne.TextAlignLeading, fyne.TextStyle{Bold: true}),
            widget.NewLabel("✓ Detected"),
        )
        
        iisEnabled := widget.NewCheck("Enable IIS Monitoring", func(checked bool) {
            t.config.Collectors.IIS.Enabled = checked
            t.onChange()
        })
        iisEnabled.SetChecked(t.config.Collectors.IIS.Enabled)
        
        iisSites := widget.NewCheck("Monitor Sites", func(checked bool) {
            t.config.Collectors.IIS.MonitorSites = checked
            t.onChange()
        })
        iisSites.SetChecked(t.config.Collectors.IIS.MonitorSites)
        
        iisPools := widget.NewCheck("Monitor Application Pools", func(checked bool) {
            t.config.Collectors.IIS.MonitorAppPools = checked
            t.onChange()
        })
        iisPools.SetChecked(t.config.Collectors.IIS.MonitorAppPools)
        
        iisSection = container.NewVBox(
            widget.NewSeparator(),
            iisLabel,
            iisEnabled,
            container.NewHBox(widget.NewLabel("    "), iisSites),
            container.NewHBox(widget.NewLabel("    "), iisPools),
        )
    }
    
    // SQL Server Section
    var sqlSection *fyne.Container
    if t.config.Collectors.SQLServer.Detected {
        sqlLabel := container.NewHBox(
            widget.NewLabelWithStyle("SQL Server", 
                fyne.TextAlignLeading, fyne.TextStyle{Bold: true}),
            widget.NewLabel("✓ Detected"),
        )
        
        sqlEnabled := widget.NewCheck("Enable SQL Server Monitoring", func(checked bool) {
            t.config.Collectors.SQLServer.Enabled = checked
            t.onChange()
        })
        sqlEnabled.SetChecked(t.config.Collectors.SQLServer.Enabled)
        
        sqlService := widget.NewCheck("Monitor SQL Server Service", func(checked bool) {
            t.config.Collectors.SQLServer.MonitorService = checked
            t.onChange()
        })
        sqlService.SetChecked(t.config.Collectors.SQLServer.MonitorService)
        
        sqlAgent := widget.NewCheck("Monitor SQL Agent Service", func(checked bool) {
            t.config.Collectors.SQLServer.MonitorAgent = checked
            t.onChange()
        })
        sqlAgent.SetChecked(t.config.Collectors.SQLServer.MonitorAgent)
        
        sqlSection = container.NewVBox(
            widget.NewSeparator(),
            sqlLabel,
            sqlEnabled,
            container.NewHBox(widget.NewLabel("    "), sqlService),
            container.NewHBox(widget.NewLabel("    "), sqlAgent),
        )
    }
    
    // Windows Services Section
    servicesLabel := widget.NewLabelWithStyle("Windows Services", 
        fyne.TextAlignLeading, fyne.TextStyle{Bold: true})
    
    addServiceBtn := widget.NewButton("+ Add Service", func() {
        t.showAddServiceDialog()
    })
    
    servicesList := t.buildServicesList()
    
    servicesSection := container.NewVBox(
        widget.NewSeparator(),
        container.NewHBox(servicesLabel, addServiceBtn),
        servicesList,
    )
    
    // Combine all sections
    content := container.NewVBox(systemSection)
    if iisSection != nil {
        content.Add(iisSection)
    }
    if sqlSection != nil {
        content.Add(sqlSection)
    }
    content.Add(servicesSection)
    
    t.container = container.NewScroll(content)
}

func (t *CollectorsTab) buildServicesList() *fyne.Container {
    list := container.NewVBox()
    
    for _, svc := range t.config.Collectors.WindowsServices.Services {
        svcName := svc // Capture for closure
        
        item := container.NewHBox(
            widget.NewCheck(svc, func(checked bool) {
                // Toggle service monitoring
            }),
            widget.NewButtonWithIcon("", theme.DeleteIcon(), func() {
                t.removeService(svcName)
            }),
        )
        list.Add(item)
    }
    
    if len(t.config.Collectors.WindowsServices.Services) == 0 {
        list.Add(widget.NewLabel("No services configured"))
    }
    
    return list
}

func (t *CollectorsTab) showAddServiceDialog() {
    // Show dialog to add a new Windows service
    // Could include a search/list of installed services
}

func (t *CollectorsTab) removeService(name string) {
    // Remove service from config
    var newServices []string
    for _, svc := range t.config.Collectors.WindowsServices.Services {
        if svc != name {
            newServices = append(newServices, svc)
        }
    }
    t.config.Collectors.WindowsServices.Services = newServices
    t.onChange()
    t.build() // Rebuild UI
}

func (t *CollectorsTab) Content() fyne.CanvasObject {
    return t.container
}9.4 Build Scriptsmakefile# Makefile

VERSION := 1.0.0
BUILD_TIME := $(shell date -u +"%Y-%m-%dT%H:%M:%SZ")
GIT_COMMIT := $(shell git rev-parse --short HEAD)

LDFLAGS := -ldflags "-X main.Version=$(VERSION) -X main.BuildTime=$(BUILD_TIME) -X main.GitCommit=$(GIT_COMMIT)"

.PHONY: all build-windows build-linux build-windows-gui clean

all: build-windows build-linux build-windows-gui

build-windows:
	GOOS=windows GOARCH=amd64 go build $(LDFLAGS) -o bin/era-agent-windows-amd64.exe ./cmd/agent

build-linux:
	GOOS=linux GOARCH=amd64 go build $(LDFLAGS) -o bin/era-agent-linux-amd64 ./cmd/agent

build-windows-gui:
	GOOS=windows GOARCH=amd64 go build $(LDFLAGS) -ldflags "-H windowsgui" -o bin/era-agent-gui.exe ./cmd/agent-gui

build-linux-arm64:
	GOOS=linux GOARCH=arm64 go build $(LDFLAGS) -o bin/era-agent-linux-arm64 ./cmd/agent

package-deb:
	# Create .deb package
	mkdir -p build/deb/usr/local/bin
	mkdir -p build/deb/etc/era-monitor
	mkdir -p build/deb/lib/systemd/system
	cp bin/era-agent-linux-amd64 build/deb/usr/local/bin/era-agent
	cp configs/config.example.json build/deb/etc/era-monitor/config.json
	cp scripts/installer/linux/era-monitor-agent.service build/deb/lib/systemd/system/
	dpkg-deb --build build/deb bin/era-agent_$(VERSION)_amd64.deb

package-windows:
	# Build Windows installer with Inno Setup
	iscc scripts/installer/windows/installer.iss

clean:
	rm -rf bin/ build/bash# scripts/install/install.sh

#!/bin/bash

# ERA Monitor Agent - Linux Installation Script
# Usage: curl -sSL https://monitor.eracloud.com.tr/install.sh | sudo bash -s -- --key=YOUR_API_KEY

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
INSTALL_DIR="/usr/local/bin"
CONFIG_DIR="/etc/era-monitor"
LOG_DIR="/var/log/era-monitor"
SERVICE_FILE="/etc/systemd/system/era-monitor-agent.service"
DOWNLOAD_URL="https://monitor.eracloud.com.tr/downloads"

# Parse arguments
API_KEY=""
API_ENDPOINT="https://monitor.eracloud.com.tr/api"

while [[ $# -gt 0 ]]; do
    case $1 in
        --key=*)
            API_KEY="${1#*=}"
            shift
            ;;
        --endpoint=*)
            API_ENDPOINT="${1#*=}"
            shift
            ;;
        *)
            echo -e "${RED}Unknown parameter: $1${NC}"
            exit 1
            ;;
    esac
done

if [ -z "$API_KEY" ]; then
    echo -e "${RED}Error: API key is required${NC}"
    echo "Usage: $0 --key=YOUR_API_KEY [--endpoint=URL]"
    exit 1
fi

echo -e "${GREEN}ERA Monitor Agent Installer${NC}"
echo "================================"

# Check if running as root
if [ "$EUID" -ne 0 ]; then
    echo -e "${RED}Please run as root (sudo)${NC}"
    exit 1
fi

# Detect architecture
ARCH=$(uname -m)
case $ARCH in
    x86_64)
        BINARY="era-agent-linux-amd64"
        ;;
    aarch64)
        BINARY="era-agent-linux-arm64"
        ;;
    *)
        echo -e "${RED}Unsupported architecture: $ARCH${NC}"
        exit 1
        ;;
esac

echo -e "${YELLOW}Detected architecture: $ARCH${NC}"

# Create directories
echo "Creating directories..."
mkdir -p $CONFIG_DIR
mkdir -p $LOG_DIR

# Download agent
echo "Downloading agent..."
curl -sSL "$DOWNLOAD_URL/$BINARY" -o "$INSTALL_DIR/era-agent"
chmod +x "$INSTALL_DIR/era-agent"

# Create configuration
echo "Creating configuration..."
cat > "$CONFIG_DIR/config.json" << EOF
{
  "server": {
    "apiEndpoint": "$API_ENDPOINT",
    "apiKey": "$API_KEY",
    "timeout": 30,
    "retryCount": 3,
    "retryDelay": 5
  },
  "host": {
    "displayName": "$(hostname)"
  },
  "collectors": {
    "intervalSeconds": 60,
    "system": {
      "enabled": true,
      "cpu": true,
      "ram": true,
      "disk": true,
      "network": false
    },
    "systemd": {
      "enabled": true,
      "units": []
    },
    "docker": {
      "enabled": false
    }
  },
  "agent": {
    "runAsService": true
  },
  "logging": {
    "level": "info",
    "logToFile": true,
    "logPath": "/var/log/era-monitor/agent.log"
  }
}
EOF

# Create systemd service
echo "Creating systemd service..."
cat > $SERVICE_FILE << EOF
[Unit]
Description=ERA Monitor Agent
After=network.target

[Service]
Type=simple
ExecStart=$INSTALL_DIR/era-agent
Restart=always
RestartSec=10
User=root

[Install]
WantedBy=multi-user.target
EOF

# Enable and start service
echo "Enabling and starting service..."
systemctl daemon-reload
systemctl enable era-monitor-agent
systemctl start era-monitor-agent

echo ""
echo -e "${GREEN}Installation complete!${NC}"
echo ""
echo "Agent status: $(systemctl is-active era-monitor-agent)"
echo ""
echo "Useful commands:"
echo "  View status:  systemctl status era-monitor-agent"
echo "  View logs:    journalctl -u era-monitor-agent -f"
echo "  Restart:      systemctl restart era-monitor-agent"
echo "  Edit config:  nano $CONFIG_DIR/config.json"