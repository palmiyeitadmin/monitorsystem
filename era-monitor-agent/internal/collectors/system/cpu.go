package system

import (
	"context"
	"time"

	"github.com/eracloud/era-monitor-agent/internal/config"
	"github.com/shirou/gopsutil/v3/cpu"
	"github.com/shirou/gopsutil/v3/disk"
	"github.com/shirou/gopsutil/v3/host"
	"github.com/shirou/gopsutil/v3/mem"
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
}

type DiskInfo struct {
	Name        string
	MountPoint  string
	TotalGB     float64
	UsedGB      float64
	UsedPercent float64
}

type CollectorResult struct {
	System *SystemMetrics
	Disks  []DiskInfo
}

type SystemCollector struct {
	config config.SystemCollectorConfig
}

func NewSystemCollector(cfg config.SystemCollectorConfig) *SystemCollector {
	return &SystemCollector{config: cfg}
}

func (c *SystemCollector) Collect(ctx context.Context) (*CollectorResult, error) {
	result := &CollectorResult{
		System: &SystemMetrics{},
		Disks:  make([]DiskInfo, 0),
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
	}

	return result, nil
}
