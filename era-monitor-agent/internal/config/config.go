package config

import (
	"os"
	"path/filepath"
	"runtime"

	"github.com/spf13/viper"
)

type Config struct {
	Server     ServerConfig     `mapstructure:"server"`
	Host       HostConfig       `mapstructure:"host"`
	Collectors CollectorsConfig `mapstructure:"collectors"`
	Services   ServicesConfig   `mapstructure:"services"`
	GUI        GUIConfig        `mapstructure:"gui"`
	Agent      AgentConfig      `mapstructure:"agent"`
	Logging    LoggingConfig    `mapstructure:"logging"`
}

type ServerConfig struct {
	APIEndpoint string `mapstructure:"apiEndpoint"`
	APIKey      string `mapstructure:"apiKey"`
	Timeout     int    `mapstructure:"timeout"`
	RetryCount  int    `mapstructure:"retryCount"`
	RetryDelay  int    `mapstructure:"retryDelay"`
}

type HostConfig struct {
	DisplayName string   `mapstructure:"displayName"`
	Location    string   `mapstructure:"location"`
	Tags        []string `mapstructure:"tags"`
}

type CollectorsConfig struct {
	IntervalSeconds int                   `mapstructure:"intervalSeconds"`
	System          SystemCollectorConfig `mapstructure:"system"`
}

type SystemCollectorConfig struct {
	Enabled bool `mapstructure:"enabled"`
	CPU     bool `mapstructure:"cpu"`
	RAM     bool `mapstructure:"ram"`
	Disk    bool `mapstructure:"disk"`
	Network bool `mapstructure:"network"`
}

type ServicesConfig struct {
	Windows WindowsServicesConfig `mapstructure:"windows"`
	Systemd SystemdServicesConfig `mapstructure:"systemd"`
	Docker  DockerServicesConfig  `mapstructure:"docker"`
	IIS     IISServicesConfig     `mapstructure:"iis"`
}

type WindowsServicesConfig struct {
	Enabled  bool     `mapstructure:"enabled"`
	Services []string `mapstructure:"services"`
}

type SystemdServicesConfig struct {
	Enabled bool     `mapstructure:"enabled"`
	Units   []string `mapstructure:"units"`
}

type DockerServicesConfig struct {
	Enabled    bool     `mapstructure:"enabled"`
	Containers []string `mapstructure:"containers"`
}

type IISServicesConfig struct {
	Enabled  bool     `mapstructure:"enabled"`
	Sites    []string `mapstructure:"sites"`
	AppPools []string `mapstructure:"appPools"`
}

type GUIConfig struct {
	Enabled           bool `mapstructure:"enabled"`
	StartMinimized    bool `mapstructure:"startMinimized"`
	ShowNotifications bool `mapstructure:"showNotifications"`
}

type AgentConfig struct {
	RunAsService    bool `mapstructure:"runAsService"`
	StartWithOS     bool `mapstructure:"startWithOS"`
	MinimizeToTray  bool `mapstructure:"minimizeToTray"`
	CheckForUpdates bool `mapstructure:"checkForUpdates"`
}

type LoggingConfig struct {
	Level      string `mapstructure:"level"`
	MaxSizeMB  int    `mapstructure:"maxSizeMB"`
	MaxBackups int    `mapstructure:"maxBackups"`
	MaxAgeDays int    `mapstructure:"maxAgeDays"`
	LogToFile  bool   `mapstructure:"logToFile"`
	LogPath    string `mapstructure:"logPath"`
}

func GetDefaultConfig() *Config {
	return &Config{
		Server: ServerConfig{
			APIEndpoint: "http://localhost:5000/api",
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
				Network: true,
			},
		},
		Services: ServicesConfig{
			Windows: WindowsServicesConfig{Enabled: runtime.GOOS == "windows"},
			Systemd: SystemdServicesConfig{Enabled: runtime.GOOS == "linux"},
			Docker:  DockerServicesConfig{Enabled: true},
			IIS:     IISServicesConfig{Enabled: runtime.GOOS == "windows"},
		},
		GUI: GUIConfig{
			Enabled:           runtime.GOOS == "windows",
			StartMinimized:    true,
			ShowNotifications: true,
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

func getHostname() string {
	hostname, _ := os.Hostname()
	return hostname
}

func getDefaultLogPath() string {
	if runtime.GOOS == "windows" {
		return filepath.Join(os.Getenv("ProgramData"), "ERAMonitor", "logs", "agent.log")
	}
	return "/var/log/era-monitor/agent.log"
}

func Load(path string) (*Config, error) {
	v := viper.New()
	v.SetConfigFile(path)
	v.SetConfigType("yaml")

	// Set Defaults
	defaultCfg := GetDefaultConfig()
	// We can't easily iterate struct to set defaults in viper without reflection or mapstructure.
	// For now, we'll unmarshal and if fields are missing, they might be zero-valued.
	// Better approach: Unmarshal into defaultCfg to overwrite defaults with file values.

	if err := v.ReadInConfig(); err != nil {
		if _, ok := err.(viper.ConfigFileNotFoundError); !ok {
			return nil, err
		}
		// If file not found, return default config
		return defaultCfg, nil
	}

	if err := v.Unmarshal(defaultCfg); err != nil {
		return nil, err
	}

	return defaultCfg, nil
}

func (c *Config) Save(path string) error {
	v := viper.New()
	v.SetConfigFile(path)
	v.SetConfigType("yaml")

	// Set values from struct
	v.Set("server.apiEndpoint", c.Server.APIEndpoint)
	v.Set("server.apiKey", c.Server.APIKey)
	v.Set("server.timeout", c.Server.Timeout)
	v.Set("server.retryCount", c.Server.RetryCount)
	v.Set("server.retryDelay", c.Server.RetryDelay)

	v.Set("host.displayName", c.Host.DisplayName)
	v.Set("host.location", c.Host.Location)
	v.Set("host.tags", c.Host.Tags)

	v.Set("collectors.intervalSeconds", c.Collectors.IntervalSeconds)
	v.Set("collectors.system.enabled", c.Collectors.System.Enabled)
	v.Set("collectors.system.cpu", c.Collectors.System.CPU)
	v.Set("collectors.system.ram", c.Collectors.System.RAM)
	v.Set("collectors.system.disk", c.Collectors.System.Disk)
	v.Set("collectors.system.network", c.Collectors.System.Network)

	v.Set("services.windows.enabled", c.Services.Windows.Enabled)
	v.Set("services.windows.services", c.Services.Windows.Services)
	v.Set("services.systemd.enabled", c.Services.Systemd.Enabled)
	v.Set("services.systemd.units", c.Services.Systemd.Units)
	v.Set("services.docker.enabled", c.Services.Docker.Enabled)
	v.Set("services.docker.containers", c.Services.Docker.Containers)
	v.Set("services.iis.enabled", c.Services.IIS.Enabled)
	v.Set("services.iis.sites", c.Services.IIS.Sites)
	v.Set("services.iis.appPools", c.Services.IIS.AppPools)

	v.Set("gui.enabled", c.GUI.Enabled)
	v.Set("gui.startMinimized", c.GUI.StartMinimized)
	v.Set("gui.showNotifications", c.GUI.ShowNotifications)

	v.Set("agent.runAsService", c.Agent.RunAsService)
	v.Set("agent.startWithOS", c.Agent.StartWithOS)
	v.Set("agent.minimizeToTray", c.Agent.MinimizeToTray)
	v.Set("agent.checkForUpdates", c.Agent.CheckForUpdates)

	v.Set("logging.level", c.Logging.Level)
	v.Set("logging.maxSizeMB", c.Logging.MaxSizeMB)
	v.Set("logging.maxBackups", c.Logging.MaxBackups)
	v.Set("logging.maxAgeDays", c.Logging.MaxAgeDays)
	v.Set("logging.logToFile", c.Logging.LogToFile)
	v.Set("logging.logPath", c.Logging.LogPath)

	return v.WriteConfig()
}
