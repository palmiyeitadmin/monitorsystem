package service

import (
	"context"
	"strings"

	"github.com/docker/docker/api/types/container"
	"github.com/docker/docker/client"
	"github.com/eracloud/era-monitor-agent/internal/api"
)

type DockerMonitor struct {
	containers []string
	client     *client.Client
}

func NewDockerMonitor(containers []string) (*DockerMonitor, error) {
	cli, err := client.NewClientWithOpts(client.FromEnv, client.WithAPIVersionNegotiation())
	if err != nil {
		return nil, err
	}

	// Test connection
	_, err = cli.Ping(context.Background())
	if err != nil {
		cli.Close()
		return nil, err
	}

	return &DockerMonitor{
		containers: containers,
		client:     cli,
	}, nil
}

func (m *DockerMonitor) Name() string {
	return "docker"
}

func (m *DockerMonitor) GetServices() ([]api.ServiceInfo, error) {
	containers, err := m.client.ContainerList(context.Background(), container.ListOptions{
		All: true,
	})
	if err != nil {
		return nil, err
	}

	var result []api.ServiceInfo

	for _, container := range containers {
		// Filter by name if configured
		if len(m.containers) > 0 {
			match := false
			for _, name := range m.containers {
				for _, cname := range container.Names {
					if strings.Contains(cname, name) || container.ID[:12] == name {
						match = true
						break
					}
				}
				if match {
					break
				}
			}
			if !match {
				continue
			}
		}

		name := ""
		if len(container.Names) > 0 {
			name = strings.TrimPrefix(container.Names[0], "/")
		} else {
			name = container.ID[:12]
		}

		result = append(result, api.ServiceInfo{
			Name:        name,
			DisplayName: container.Image,
			Type:        "DockerContainer",
			Status:      mapDockerState(container.State),
			Config: map[string]interface{}{
				"container_id": container.ID[:12],
				"image":        container.Image,
				"status":       container.Status,
				"ports":        container.Ports,
			},
		})
	}

	return result, nil
}

func mapDockerState(state string) string {
	switch strings.ToLower(state) {
	case "running":
		return "running"
	case "exited", "dead":
		return "stopped"
	case "created", "restarting":
		return "starting"
	case "paused":
		return "paused"
	case "removing":
		return "stopping"
	default:
		return "unknown"
	}
}
