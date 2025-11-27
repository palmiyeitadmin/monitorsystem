package main

import (
	"context"
	"flag"

	"github.com/eracloud/era-monitor-agent/internal/agent"
	"github.com/eracloud/era-monitor-agent/internal/config"
	"github.com/eracloud/era-monitor-agent/internal/gui"
	"github.com/eracloud/era-monitor-agent/internal/logger"
	"go.uber.org/zap"
)

func main() {
	configFile := flag.String("config", "config.yaml", "Path to configuration file")
	flag.Parse()

	// Load Configuration
	// Load Configuration
	cfg, err := config.Load(*configFile)
	if err != nil {
		// If loading failed (and not just missing file which returns default), log error
		println("Error loading config: " + err.Error())
		// We continue with what we have (nil or partial) or default if Load handles it.
		// Load returns default if file missing.
		if cfg == nil {
			cfg = config.GetDefaultConfig()
		}
	}

	// Initialize Logger
	log := logger.NewLogger(cfg.Logging.LogPath, cfg.Logging.Level)
	defer log.Sync()

	log.Info("Agent GUI initializing...")

	// Create Agent
	agt := agent.NewAgent(cfg, log)

	// Context with Cancellation
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	// Start Agent in background
	go func() {
		if err := agt.Run(ctx); err != nil {
			log.Error("Agent stopped with error", zap.Error(err))
		}
	}()

	// Create and Run GUI
	// Note: Fyne app.Run() must be called on the main thread (which this is)
	app := gui.NewApp(agt, cfg, *configFile, ctx, cancel)
	app.Run()

	log.Info("GUI closed, shutting down agent...")
}
