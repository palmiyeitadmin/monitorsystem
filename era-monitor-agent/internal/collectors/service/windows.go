//go:build windows

package service

import (
	"github.com/eracloud/era-monitor-agent/internal/api"
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
