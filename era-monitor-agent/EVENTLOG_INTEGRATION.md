# Event Log Collector - Agent Integration Guide

## ✅ Completed
1. **models.go** - Added `EventLogInfo` struct and `EventLogs []EventLogInfo` field to `HeartbeatRequest`
2. **eventlog/windows.go** - Full Windows Event Log collector with syscalls
3. **config.go** - Added `EventLog bool` to `SystemCollectorConfig`
4. **config.yaml** - Added `eventLog: true` to system collectors

## 📝 Manual Integration Required (agent.go is gitignored)

### Step 1: Import the event log collector
Add to imports in `internal/agent/agent.go`:
```go
import (
    // ... existing imports
    "era-monitor-agent/internal/collectors/eventlog"
)
```

### Step 2: Initialize collector in Agent struct
Add field to Agent struct:
```go
type Agent struct {
    // ... existing fields
    eventLogCollector *eventlog.Collector
}
```

### Step 3: Initialize in New() or Start()
```go
func (a *Agent) Start() error {
    // ... existing code
    
    // Initialize event log collector (Windows only)
    if runtime.GOOS == "windows" {
        a.eventLogCollector = eventlog.NewCollector(a.config.Collectors.System.EventLog)
    }
    
    // ... rest of code
}
```

### Step 4: Collect event logs in collectData()
In the `collectData()` function where you build the HeartbeatRequest:
```go
func (a *Agent) collectData() (*api.HeartbeatRequest, error) {
    // ... existing system/disk/service collection code
    
    // Collect event logs (Windows only)
    var eventLogs []api.EventLogInfo
    if a.eventLogCollector != nil {
        logs, err := a.eventLogCollector.Collect()
        if err == nil && len(logs) > 0 {
            // Convert eventlog.EventInfo to api.EventLogInfo
            eventLogs = make([]api.EventLogInfo, len(logs))
            for i, log := range logs {
                eventLogs[i] = api.EventLogInfo{
                    LogName:     log.LogName,
                    EventID:     log.EventID,
                    Level:       log.Level,
                    Source:      log.Source,
                    Message:     log.Message,
                    TimeCreated: log.TimeCreated,
                    Category:    log.Category,
                }
            }
        }
    }
    
    return &api.HeartbeatRequest{
        SystemInfo:  systemInfo,
        Disks:       disks,
        Services:    services,
        NetworkInfo: networkInfo,
        EventLogs:   eventLogs,  // ADD THIS LINE
        Timestamp:   time.Now(),
        AgentInfo:   agentInfo,
    }, nil
}
```

## Testing
1. Build agent: `go build -o era-monitor-agent.exe cmd/agent/main.go`
2. Run as Administrator (required for Event Log access)
3. Check API logs to verify event logs are received
4. Query: `GET /api/hosts/{id}/eventlogs`

## Notes
- Event Log collection requires Administrator privileges
- Collects last 24 hours of critical events
- Limited to 1000 events total
- Categories: System, SQL, IIS, Veeam, Docker, Security, Disk, Network
- 30-day retention policy on backend
