//go:build windows
// +build windows

package eventlog

import (
	"fmt"
	"strings"
	"syscall"
	"time"
	"unsafe"
)

var (
	wevtapi                    = syscall.NewLazyDLL("wevtapi.dll")
	procEvtQuery               = wevtapi.NewProc("EvtQuery")
	procEvtNext                = wevtapi.NewProc("EvtNext")
	procEvtClose               = wevtapi.NewProc("EvtClose")
	procEvtRender              = wevtapi.NewProc("EvtRender")
	procEvtFormatMessage       = wevtapi.NewProc("EvtFormatMessage")
	procEvtCreateRenderContext = wevtapi.NewProc("EvtCreateRenderContext")
)

const (
	EvtQueryChannelPath      = 0x1
	EvtQueryReverseDirection = 0x200
	EvtRenderEventValues     = 1
	EvtRenderEventXml        = 2
	EvtSystemTimeCreated     = 8
	EvtSystemLevel           = 2
	EvtSystemEventID         = 10
	EvtSystemProviderName    = 1
)

// EventInfo represents a single event log entry
type EventInfo struct {
	LogName     string    `json:"logName"`
	EventID     int       `json:"eventId"`
	Level       string    `json:"level"`
	Source      string    `json:"source"`
	Message     string    `json:"message"`
	TimeCreated time.Time `json:"timeCreated"`
	Category    string    `json:"category"`
}

// Collector collects Windows Event Logs
type Collector struct {
	enabled bool
}

// NewCollector creates a new event log collector
func NewCollector(enabled bool) *Collector {
	return &Collector{
		enabled: enabled,
	}
}

// Collect retrieves critical event logs from Windows
func (c *Collector) Collect() ([]EventInfo, error) {
	if !c.enabled {
		return nil, nil
	}

	var allEvents []EventInfo

	// Define critical event sources
	criticalEvents := []struct {
		log      string
		category string
		eventIDs []int
	}{
		{"System", "System", []int{6008, 41, 1074, 7023, 7024, 7031}},
		{"System", "Disk", []int{7, 11, 51, 52, 153, 154, 129}},
		{"Application", "SQL", []int{17063, 17065, 9002, 823, 824, 825}},
		{"Application", "System", []int{1000, 1001, 1002}},
		{"Security", "Security", []int{4625, 4740, 4720}},
	}

	// Collect from each source
	for _, source := range criticalEvents {
		events, err := c.queryEvents(source.log, source.category, source.eventIDs, 100)
		if err != nil {
			continue // Skip errors, continue with other sources
		}
		allEvents = append(allEvents, events...)
	}

	// Limit total events
	if len(allEvents) > 1000 {
		allEvents = allEvents[:1000]
	}

	return allEvents, nil
}

func (c *Collector) queryEvents(channelPath, category string, eventIDs []int, maxEvents int) ([]EventInfo, error) {
	// Build XPath query for specific event IDs
	var eventIDQuery string
	for i, id := range eventIDs {
		if i > 0 {
			eventIDQuery += " or "
		}
		eventIDQuery += fmt.Sprintf("EventID=%d", id)
	}

	// Query last 24 hours only
	query := fmt.Sprintf("*[System[(%s) and TimeCreated[timediff(@SystemTime) <= 86400000]]]", eventIDQuery)

	channelPathPtr, _ := syscall.UTF16PtrFromString(channelPath)
	queryPtr, _ := syscall.UTF16PtrFromString(query)

	// EvtQuery
	hResults, _, _ := procEvtQuery.Call(
		0, // Session
		uintptr(unsafe.Pointer(channelPathPtr)),
		uintptr(unsafe.Pointer(queryPtr)),
		EvtQueryChannelPath|EvtQueryReverseDirection,
	)

	if hResults == 0 {
		return nil, fmt.Errorf("EvtQuery failed for %s", channelPath)
	}
	defer procEvtClose.Call(hResults)

	var events []EventInfo
	eventHandles := make([]uintptr, 10)
	var returned uint32

	for len(events) < maxEvents {
		// EvtNext - Get next batch of events
		ret, _, _ := procEvtNext.Call(
			hResults,
			uintptr(len(eventHandles)),
			uintptr(unsafe.Pointer(&eventHandles[0])),
			0, // Timeout (0 = return immediately)
			0, // Flags
			uintptr(unsafe.Pointer(&returned)),
		)

		if ret == 0 || returned == 0 {
			break
		}

		// Process each event
		for i := uint32(0); i < returned && len(events) < maxEvents; i++ {
			eventInfo := c.extractEventInfo(eventHandles[i], channelPath, category)
			if eventInfo != nil {
				events = append(events, *eventInfo)
			}
			procEvtClose.Call(eventHandles[i])
		}
	}

	return events, nil
}

func (c *Collector) extractEventInfo(hEvent uintptr, logName, category string) *EventInfo {
	// Simplified extraction - in production, use EvtRender to get all fields
	// For now, return a basic structure

	event := &EventInfo{
		LogName:     logName,
		Category:    category,
		TimeCreated: time.Now(),        // Should extract from event
		Level:       "Error",           // Should extract from event
		EventID:     0,                 // Should extract from event
		Source:      "Unknown",         // Should extract from event
		Message:     "Event log entry", // Should extract from event
	}

	return event
}

// mapEventLevel maps Windows event level to string
func mapEventLevel(level uint32) string {
	switch level {
	case 1:
		return "Critical"
	case 2:
		return "Error"
	case 3:
		return "Warning"
	case 4:
		return "Information"
	default:
		return "Unknown"
	}
}

// DetermineCategoryFromSource determines category from source and event ID
func DetermineCategoryFromSource(source string, eventID int) string {
	sourceLower := strings.ToLower(source)

	if strings.Contains(sourceLower, "mssql") || strings.Contains(sourceLower, "sql") {
		return "SQL"
	}
	if strings.Contains(sourceLower, "iis") || strings.Contains(sourceLower, "w3svc") {
		return "IIS"
	}
	if strings.Contains(sourceLower, "veeam") {
		return "Veeam"
	}
	if strings.Contains(sourceLower, "docker") {
		return "Docker"
	}
	if strings.Contains(sourceLower, "disk") || strings.Contains(sourceLower, "ntfs") {
		return "Disk&Storage"
	}
	if strings.Contains(sourceLower, "tcpip") || strings.Contains(sourceLower, "network") {
		return "Network"
	}
	if eventID >= 4000 && eventID < 5000 {
		return "Security"
	}

	return "System"
}
