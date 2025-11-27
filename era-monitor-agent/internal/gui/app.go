package gui

import (
	"context"
	"encoding/json"
	"fmt"
	"os"
	"time"

	"fyne.io/fyne/v2"
	"fyne.io/fyne/v2/app"
	"fyne.io/fyne/v2/container"
	"fyne.io/fyne/v2/dialog"
	"fyne.io/fyne/v2/driver/desktop"
	"fyne.io/fyne/v2/theme"
	"fyne.io/fyne/v2/widget"
	"github.com/eracloud/era-monitor-agent/internal/agent"
	"github.com/eracloud/era-monitor-agent/internal/config"
	"github.com/go-resty/resty/v2"
)

type App struct {
	fyneApp    fyne.App
	mainWindow fyne.Window
	agent      *agent.Agent
	config     *config.Config
	configPath string

	// Context management for agent restart
	ctx       context.Context
	cancel    context.CancelFunc
	restartCh chan bool

	// UI elements
	statusLabel   *widget.Label
	cpuLabel      *widget.Label
	ramLabel      *widget.Label
	lastSentLabel *widget.Label
	servicesLabel *widget.Label
}

func NewApp(agentInstance *agent.Agent, cfg *config.Config, configPath string, ctx context.Context, cancel context.CancelFunc) *App {
	fyneApp := app.NewWithID("com.eracloud.era-agent")

	return &App{
		fyneApp:    fyneApp,
		agent:      agentInstance,
		config:     cfg,
		configPath: configPath,
		ctx:        ctx,
		cancel:     cancel,
		restartCh:  make(chan bool, 1),
	}
}

func (a *App) Run() {
	a.mainWindow = a.fyneApp.NewWindow("ERA Monitor Agent")
	a.mainWindow.Resize(fyne.NewSize(400, 500))

	if desk, ok := a.fyneApp.(desktop.App); ok {
		a.setupSystemTray(desk)
	}

	a.buildUI()
	go a.updateStatusLoop()

	if a.config.GUI.StartMinimized {
		a.mainWindow.Hide()
	} else {
		a.mainWindow.Show()
	}

	a.mainWindow.SetCloseIntercept(func() {
		a.mainWindow.Hide()
	})

	a.fyneApp.Run()
}

func (a *App) buildUI() {
	header := container.NewVBox(
		widget.NewLabelWithStyle("ERA Monitor Agent", fyne.TextAlignCenter, fyne.TextStyle{Bold: true}),
		widget.NewSeparator(),
	)

	a.statusLabel = widget.NewLabel("Status: Starting...")
	a.cpuLabel = widget.NewLabel("CPU: --")
	a.ramLabel = widget.NewLabel("RAM: --")
	a.lastSentLabel = widget.NewLabel("Last Heartbeat: --")
	a.servicesLabel = widget.NewLabel("Services: --")

	statusCard := widget.NewCard("Status", "",
		container.NewVBox(
			a.statusLabel,
			a.cpuLabel,
			a.ramLabel,
			a.lastSentLabel,
			a.servicesLabel,
		),
	)

	serverInfo := widget.NewCard("Server", "",
		container.NewVBox(
			widget.NewLabel("URL: "+a.config.Server.APIEndpoint),
			widget.NewLabel("API Key: "+maskAPIKey(a.config.Server.APIKey)),
		),
	)

	refreshBtn := widget.NewButtonWithIcon("Send Heartbeat", theme.ViewRefreshIcon(), func() {
		go func() {
			if err := a.agent.ForceHeartbeat(context.Background()); err != nil {
				dialog.ShowError(err, a.mainWindow)
			} else {
				dialog.ShowInformation("Success", "Heartbeat sent successfully", a.mainWindow)
			}
		}()
	})

	settingsBtn := widget.NewButtonWithIcon("Settings", theme.SettingsIcon(), func() {
		a.showSettings()
	})

	logsBtn := widget.NewButtonWithIcon("View Logs", theme.DocumentIcon(), func() {
		a.showLogs()
	})

	restartBtn := widget.NewButtonWithIcon("Restart Agent", theme.MediaReplayIcon(), func() {
		dialog.ShowInformation("Restart Required", "Please close and restart the application for changes to take effect.", a.mainWindow)
	})

	actions := container.NewHBox(
		refreshBtn,
		settingsBtn,
		logsBtn,
		restartBtn,
	)

	content := container.NewVBox(
		header,
		statusCard,
		serverInfo,
		widget.NewSeparator(),
		actions,
	)

	a.mainWindow.SetContent(content)
}

func (a *App) setupSystemTray(desk desktop.App) {
	menu := fyne.NewMenu("ERA Agent",
		fyne.NewMenuItem("Show", func() {
			a.mainWindow.Show()
		}),
		fyne.NewMenuItemSeparator(),
		fyne.NewMenuItem("Quit", func() {
			a.fyneApp.Quit()
		}),
	)

	desk.SetSystemTrayMenu(menu)
}

func (a *App) updateStatusLoop() {
	ticker := time.NewTicker(2 * time.Second)
	defer ticker.Stop()

	for range ticker.C {
		status := a.agent.Status()

		if status.IsRunning {
			a.statusLabel.SetText("Status: Running ✓")
		} else {
			a.statusLabel.SetText("Status: Stopped")
		}

		if status.LastMetrics != nil {
			a.cpuLabel.SetText(fmt.Sprintf("CPU: %.1f%%", status.LastMetrics.SystemInfo.CPUPercent))
			a.ramLabel.SetText(fmt.Sprintf("RAM: %.1f%%", status.LastMetrics.SystemInfo.RAMPercent))
			a.servicesLabel.SetText(fmt.Sprintf("Services: %d", len(status.LastMetrics.Services)))
		}

		if !status.LastSentAt.IsZero() {
			a.lastSentLabel.SetText(fmt.Sprintf("Last Heartbeat: %s", status.LastSentAt.Format("15:04:05")))
		}

		if status.LastError != nil {
			a.statusLabel.SetText("Status: Error - " + status.LastError.Error())
		}
	}
}

func maskAPIKey(key string) string {
	if len(key) <= 8 {
		return "****"
	}
	return key[:4] + "..." + key[len(key)-4:]
}

func (a *App) showSettings() {
	usernameEntry := widget.NewEntry()
	usernameEntry.SetPlaceHolder("admin@example.com")

	passwordEntry := widget.NewPasswordEntry()
	passwordEntry.SetPlaceHolder("Enter password")

	apiKeyEntry := widget.NewEntry()
	apiKeyEntry.SetText(a.config.Server.APIKey)
	apiKeyEntry.SetPlaceHolder("API Key will be auto-filled after login")

	apiEndpointEntry := widget.NewEntry()
	apiEndpointEntry.SetText(a.config.Server.APIEndpoint)
	apiEndpointEntry.SetPlaceHolder("http://localhost:5000/api")

	loginStatusLabel := widget.NewLabel("")

	loginBtn := widget.NewButton("Login & Get API Key", func() {
		if usernameEntry.Text == "" || passwordEntry.Text == "" {
			loginStatusLabel.SetText("❌ Please enter username and password")
			return
		}

		loginStatusLabel.SetText("🔄 Logging in...")

		go func() {
			loginData := map[string]string{
				"email":    usernameEntry.Text,
				"password": passwordEntry.Text,
			}

			client := resty.New()
			resp, err := client.R().
				SetBody(loginData).
				Post(apiEndpointEntry.Text + "/auth/login")

			if err != nil {
				loginStatusLabel.SetText("❌ Connection failed: " + err.Error())
				return
			}

			if resp.IsError() {
				loginStatusLabel.SetText("❌ Login failed: " + resp.Status())
				return
			}

			var result map[string]interface{}
			if err := json.Unmarshal(resp.Body(), &result); err != nil {
				loginStatusLabel.SetText("❌ Failed to parse response")
				return
			}

			if accessToken, ok := result["accessToken"].(string); ok {
				apiKeyEntry.SetText(accessToken)
				loginStatusLabel.SetText("✅ Login successful! API Key retrieved.")
			} else {
				loginStatusLabel.SetText("❌ No access token in response")
			}
		}()
	})

	loginTab := container.NewVBox(
		widget.NewLabel("Login to automatically retrieve API Key:"),
		widget.NewSeparator(),
		widget.NewForm(
			widget.NewFormItem("API Endpoint", apiEndpointEntry),
			widget.NewFormItem("Username/Email", usernameEntry),
			widget.NewFormItem("Password", passwordEntry),
		),
		loginBtn,
		loginStatusLabel,
		widget.NewSeparator(),
		widget.NewLabel("Or manually enter API Key:"),
		widget.NewForm(
			widget.NewFormItem("API Key", apiKeyEntry),
		),
	)

	hostnameEntry := widget.NewEntry()
	hostnameEntry.SetText(a.config.Host.DisplayName)

	locationEntry := widget.NewEntry()
	locationEntry.SetText(a.config.Host.Location)

	autoFillBtn := widget.NewButton("Auto-fill System Info", func() {
		hostname, err := os.Hostname()
		if err == nil {
			hostnameEntry.SetText(hostname)
		}
	})

	hostTab := container.NewVBox(
		widget.NewForm(
			widget.NewFormItem("Hostname", hostnameEntry),
			widget.NewFormItem("Location", locationEntry),
		),
		autoFillBtn,
	)

	tabs := container.NewAppTabs(
		container.NewTabItem("Connection", loginTab),
		container.NewTabItem("Host Info", hostTab),
	)

	settingsDialog := dialog.NewCustomConfirm("Settings", "Save", "Cancel", tabs, func(save bool) {
		if save {
			a.config.Server.APIEndpoint = apiEndpointEntry.Text
			a.config.Server.APIKey = apiKeyEntry.Text
			a.config.Host.DisplayName = hostnameEntry.Text
			a.config.Host.Location = locationEntry.Text

			if err := a.config.Save(a.configPath); err != nil {
				dialog.ShowError(fmt.Errorf("Failed to save config: %w", err), a.mainWindow)
			} else {
				dialog.ShowInformation("Success", "Settings saved. Please restart the agent for changes to take effect.", a.mainWindow)
			}
		}
	}, a.mainWindow)

	settingsDialog.Resize(fyne.NewSize(600, 500))
	settingsDialog.Show()
}

func (a *App) showLogs() {
	logPath := a.config.Logging.LogPath

	data, err := os.ReadFile(logPath)
	if err != nil {
		dialog.ShowError(fmt.Errorf("Failed to read log file: %w", err), a.mainWindow)
		return
	}

	logContent := widget.NewMultiLineEntry()
	logContent.SetText(string(data))
	logContent.Wrapping = fyne.TextWrapWord

	logDialog := dialog.NewCustom("Agent Logs", "Close", container.NewScroll(logContent), a.mainWindow)
	logDialog.Resize(fyne.NewSize(800, 600))
	logDialog.Show()
}
