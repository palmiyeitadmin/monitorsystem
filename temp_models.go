package api

import "time"

type HeartbeatRequest struct {
	SystemInfo  SystemInfo     `json:"system"`
	Disks       []DiskInfo     `json:"disks"`
	Services    []ServiceInfo  `json:"services"`
	NetworkInfo *NetworkInfo   `json:"network,omitempty"`
	Timestamp   time.Time      `json:"timestamp"`
	AgentInfo   *AgentMetadata `json:"agent,omitempty"`
}

type SystemInfo struct {
	Hostname      string  `json:"hostname"`
	OSType        string  `json:"osType"`
	OSVersion     string  `json:"osVersion"`
	CPUPercent    float64 `json:"cpuPercent"`
	RAMPercent    float64 `json:"ramPercent"`
	RAMUsedMB     uint64  `json:"ramUsedMb"`
	RAMTotalMB    uint64  `json:"ramTotalMb"`
	UptimeSeconds uint64  `json:"uptimeSeconds"`
	ProcessCount  int     `json:"processCount"`
}

type DiskInfo struct {
	Name        string  `json:"name"`
	MountPoint  string  `json:"mountPoint"`
	FileSystem  string  `json:"fileSystem"`
	TotalGB     float64 `json:"totalGb"`
	UsedGB      float64 `json:"usedGb"`
	UsedPercent float64 `json:"usedPercent"`
}

type ServiceInfo struct {
	Name        string                 `json:"name"`
	DisplayName string                 `json:"displayName"`
	Type        string                 `json:"type"`
	Status      string                 `json:"status"`
	Config      map[string]interface{} `json:"config,omitempty"`
}

type NetworkInfo struct {
	PrimaryIP string `json:"primaryIp"`
	PublicIP  string `json:"publicIp,omitempty"`
	InBytes   uint64 `json:"inBytes"`
	OutBytes  uint64 `json:"outBytes"`
}

type AgentMetadata struct {
	Version   string `json:"version"`
	BuildHash string `json:"buildHash"`
	Platform  string `json:"platform"`
}

type HeartbeatResponse struct {
	Success     bool      `json:"success"`
	HostID      string    `json:"host_id"`
	NextCheckIn int       `json:"next_check_in"`
	Commands    []Command `json:"commands,omitempty"`
	Message     string    `json:"message,omitempty"`
}

type Command struct {
	ID     string                 `json:"id"`
	Type   string                 `json:"type"`
	Params map[string]interface{} `json:"params,omitempty"`
}
