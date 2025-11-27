package system

import (
	"context"
	"errors"
	"io"
	"net"
	"net/http"
	"strings"
	"time"

	"github.com/eracloud/era-monitor-agent/internal/config"
	"github.com/shirou/gopsutil/v3/cpu"
	"github.com/shirou/gopsutil/v3/disk"
	"github.com/shirou/gopsutil/v3/host"
	"github.com/shirou/gopsutil/v3/mem"
	netstats "github.com/shirou/gopsutil/v3/net"
	"github.com/shirou/gopsutil/v3/process"
)

type SystemMetrics struct {
	CPUPercent      float64
	RAMPercent      float64
	RAMUsedMB       uint64
	RAMTotalMB      uint64
	UptimeSeconds   uint64
	Hostname        string
	OS              string
	Platform        string
	PlatformVersion string
	ProcessCount    int
}

type DiskInfo struct {
	Name        string
	MountPoint  string
	FileSystem  string
	TotalGB     float64
	UsedGB      float64
	UsedPercent float64
}

type CollectorResult struct {
	System  *SystemMetrics
	Disks   []DiskInfo
	Network *NetworkMetrics
}

type SystemCollector struct {
	config config.SystemCollectorConfig
}

type NetworkMetrics struct {
	PrimaryIP string
	PublicIP  string
	InBytes   uint64
	OutBytes  uint64
}

func NewSystemCollector(cfg config.SystemCollectorConfig) *SystemCollector {
	return &SystemCollector{config: cfg}
}

func (c *SystemCollector) Collect(ctx context.Context) (*CollectorResult, error) {
	result := &CollectorResult{
		System: &SystemMetrics{},
		Disks:  make([]DiskInfo, 0),
	}

	if c.config.Network {
		if netInfo, err := collectNetwork(ctx); err == nil {
			result.Network = netInfo
		} else {
			result.Network = &NetworkMetrics{}
		}
	}

	// CPU
	if c.config.CPU {
		percent, err := cpu.PercentWithContext(ctx, time.Second, false)
		if err == nil && len(percent) > 0 {
			result.System.CPUPercent = percent[0]
		}
	}

	// Memory
	if c.config.RAM {
		v, err := mem.VirtualMemoryWithContext(ctx)
		if err == nil {
			result.System.RAMPercent = v.UsedPercent
			result.System.RAMUsedMB = v.Used / 1024 / 1024
			result.System.RAMTotalMB = v.Total / 1024 / 1024
		}
	}

	// Disk
	if c.config.Disk {
		partitions, err := disk.PartitionsWithContext(ctx, false)
		if err == nil {
			for _, p := range partitions {
				usage, err := disk.UsageWithContext(ctx, p.Mountpoint)
				if err == nil {
					result.Disks = append(result.Disks, DiskInfo{
						Name:        p.Device,
						MountPoint:  p.Mountpoint,
						FileSystem:  p.Fstype,
						TotalGB:     float64(usage.Total) / 1024 / 1024 / 1024,
						UsedGB:      float64(usage.Used) / 1024 / 1024 / 1024,
						UsedPercent: usage.UsedPercent,
					})
				}
			}
		}
	}

	// Host Info
	info, err := host.InfoWithContext(ctx)
	if err == nil {
		result.System.UptimeSeconds = info.Uptime
		result.System.Hostname = info.Hostname
		result.System.OS = info.OS
		result.System.Platform = info.Platform
		result.System.PlatformVersion = info.PlatformVersion
		if info.Procs > 0 {
			result.System.ProcessCount = int(info.Procs)
		}
	}

	if result.System.ProcessCount == 0 {
		if pids, err := process.PidsWithContext(ctx); err == nil {
			result.System.ProcessCount = len(pids)
		}
	}

	return result, nil
}

func collectNetwork(ctx context.Context) (*NetworkMetrics, error) {
	netInfo := &NetworkMetrics{}

	if primaryIP := detectPrimaryIP(); primaryIP != "" {
		netInfo.PrimaryIP = primaryIP
	}

	if stats, err := netstats.IOCountersWithContext(ctx, false); err == nil && len(stats) > 0 {
		netInfo.InBytes = stats[0].BytesRecv
		netInfo.OutBytes = stats[0].BytesSent
	}

	publicIP, err := fetchPublicIP(ctx)
	if err == nil && publicIP != "" {
		netInfo.PublicIP = publicIP
	}

	return netInfo, nil
}

func detectPrimaryIP() string {
	ifaces, err := net.Interfaces()
	if err != nil {
		return ""
	}

	for _, iface := range ifaces {
		if iface.Flags&net.FlagUp == 0 || iface.Flags&net.FlagLoopback != 0 {
			continue
		}

		addrs, err := iface.Addrs()
		if err != nil {
			continue
		}

		for _, addr := range addrs {
			var ip net.IP
			switch v := addr.(type) {
			case *net.IPNet:
				ip = v.IP
			case *net.IPAddr:
				ip = v.IP
			}

			if ip == nil || ip.IsLoopback() {
				continue
			}

			if ipv4 := ip.To4(); ipv4 != nil {
				return ipv4.String()
			}
		}
	}

	return ""
}

func fetchPublicIP(ctx context.Context) (string, error) {
	req, err := http.NewRequestWithContext(ctx, http.MethodGet, "https://api.ipify.org", nil)
	if err != nil {
		return "", err
	}

	resp, err := http.DefaultClient.Do(req)
	if err != nil {
		return "", err
	}
	defer resp.Body.Close()

	if resp.StatusCode != http.StatusOK {
		return "", errors.New("failed to fetch public IP")
	}

	body, err := io.ReadAll(resp.Body)
	if err != nil {
		return "", err
	}

	return strings.TrimSpace(string(body)), nil
}
