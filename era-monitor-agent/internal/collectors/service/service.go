package service

import (
	"github.com/eracloud/era-monitor-agent/internal/api"
)

type Monitor interface {
	Name() string
	GetServices() ([]api.ServiceInfo, error)
}
