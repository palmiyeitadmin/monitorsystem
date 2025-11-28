//go:build windows
// +build windows

package eventlog

import (
	"encoding/json"
	"fmt"
	"os/exec"
	"strconv"
	"strings"
	"time"
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

// PowerShell event structure
type psEvent struct {
	LogName     string `json:"LogName"`
	Id          int    `json:"Id"`
	LevelText   string `json:"LevelDisplayName"`
	Source      string `json:"ProviderName"`
	Message     string `json:"Message"`
	TimeCreated string `json:"TimeCreated"`
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
	// Build PowerShell command to get events
	eventIDFilter := ""
	for i, id := range eventIDs {
		if i > 0 {
			eventIDFilter += ","
		}
		eventIDFilter += strconv.Itoa(id)
	}

	// PowerShell command to get events as JSON
	psCmd := fmt.Sprintf(`
		[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
		$events = Get-WinEvent -FilterHashtable @{
			LogName='%s'
			ID=%s
		} -MaxEvents %d -ErrorAction SilentlyContinue | Select-Object -First %d
		
		$events | ForEach-Object {
			[PSCustomObject]@{
				LogName = $_.LogName
				Id = $_.Id
				LevelDisplayName = $_.LevelDisplayName
				ProviderName = $_.ProviderName
				Message = $_.Message
				TimeCreated = $_.TimeCreated.ToString('o')
			}
		} | ConvertTo-Json -Depth 2
	`, channelPath, eventIDFilter, maxEvents, maxEvents)

	// Execute PowerShell
	cmd := exec.Command("powershell", "-NoProfile", "-NonInteractive", "-Command", psCmd)
	output, err := cmd.Output()
	if err != nil {
		// No events found is not an error
		return []EventInfo{}, nil
	}

	// Parse JSON output
	var psEvents []psEvent
	if err := json.Unmarshal(output, &psEvents); err != nil {
		// Try single event (PowerShell returns object instead of array for single result)
		var singleEvent psEvent
		if err2 := json.Unmarshal(output, &singleEvent); err2 == nil {
			psEvents = []psEvent{singleEvent}
		} else {
			return nil, fmt.Errorf("failed to parse PowerShell output: %v", err)
		}
	}

	// Convert to EventInfo
	var events []EventInfo
	for _, pse := range psEvents {
		timeCreated, _ := time.Parse(time.RFC3339, pse.TimeCreated)

		events = append(events, EventInfo{
			LogName:     pse.LogName,
			EventID:     pse.Id,
			Level:       pse.LevelText,
			Source:      pse.Source,
			Message:     pse.Message,
			TimeCreated: timeCreated,
			Category:    category,
		})
	}

	fmt.Printf("Collected %d events from %s\n", len(events), channelPath)
	return events, nil
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
