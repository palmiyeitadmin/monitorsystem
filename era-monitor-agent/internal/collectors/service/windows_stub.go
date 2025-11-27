//go:build !windows

package service

import (
	"fmt"

	"github.com/eracloud/era-monitor-agent/internal/api"
)

type WindowsMonitor struct{}

func NewWindowsMonitor(services []string) (*WindowsMonitor, error) {
	return nil, fmt.Errorf("windows monitor not supported on this platform")
}

func (m *WindowsMonitor) Name() string {
	return "windows"
}

func (m *WindowsMonitor) GetServices() ([]api.ServiceInfo, error) {
	return nil, nil
}
