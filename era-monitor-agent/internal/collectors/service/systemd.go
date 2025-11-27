//go:build linux

package service

import (
	"context"
	"strings"

	"github.com/coreos/go-systemd/v22/dbus"
	"github.com/eracloud/era-monitor-agent/internal/api"
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
