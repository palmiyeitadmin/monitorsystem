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
	RAMUsedMB     uint64  `json:"ram_used_mb"`
	RAMTotalMB    uint64  `json:"ram_total_mb"`
	UptimeSeconds uint64  `json:"uptime_seconds"`
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
