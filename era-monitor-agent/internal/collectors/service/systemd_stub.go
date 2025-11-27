//go:build !linux

package service

import (
	"fmt"

	"github.com/eracloud/era-monitor-agent/internal/api"
)

type SystemdMonitor struct{}

func NewSystemdMonitor(units []string) (*SystemdMonitor, error) {
	return nil, fmt.Errorf("systemd monitor not supported on this platform")
}

func (m *SystemdMonitor) Name() string {
	return "systemd"
}

func (m *SystemdMonitor) GetServices() ([]api.ServiceInfo, error) {
	return nil, nil
}
