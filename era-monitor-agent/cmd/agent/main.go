package main

import (
	"context"
	"flag"
	"os"
	"os/signal"
	"syscall"

	"github.com/eracloud/era-monitor-agent/internal/agent"
	"github.com/eracloud/era-monitor-agent/internal/config"
	"github.com/eracloud/era-monitor-agent/internal/logger"
	"github.com/spf13/viper"
	"go.uber.org/zap"
)

func main() {
	configFile := flag.String("config", "config.yaml", "Path to configuration file")
	flag.Parse()

	// Load Configuration
	viper.SetConfigFile(*configFile)
	viper.SetConfigType("yaml")
	
	// Set Defaults
	defaultCfg := config.GetDefaultConfig()
	// (Viper defaults setting omitted for brevity, relying on struct defaults for now)

	if err := viper.ReadInConfig(); err != nil {
		// If config file doesn't exist, we'll use defaults but warn
		if _, ok := err.(viper.ConfigFileNotFoundError); !ok {
			panic("Fatal error config file: " + err.Error())
		}
	}

	var cfg config.Config
	if err := viper.Unmarshal(&cfg); err != nil {
		panic("Unable to decode into struct: " + err.Error())
	}
	
	// If config file was missing, use defaults populated manually if needed, 
	// but for now we assume the struct zero values or manual override.
	// A better approach is to merge defaults.
	if cfg.Server.APIEndpoint == "" {
		cfg = *defaultCfg
	}

	// Initialize Logger
	log := logger.NewLogger(cfg.Logging.LogPath, cfg.Logging.Level)
	defer log.Sync()

	log.Info("Agent initializing...")

	// Create Agent
	agt := agent.NewAgent(&cfg, log)

	// Context with Cancellation
	ctx, cancel := context.WithCancel(context.Background())
	defer cancel()

	// Handle Signals
	sigChan := make(chan os.Signal, 1)
	signal.Notify(sigChan, syscall.SIGINT, syscall.SIGTERM)

	go func() {
		sig := <-sigChan
		log.Info("Received signal, shutting down...", zap.String("signal", sig.String()))
		cancel()
	}()

	// Run Agent
	if err := agt.Run(ctx); err != nil {
		log.Fatal("Agent stopped with error", zap.Error(err))
	}

	log.Info("Agent shutdown complete")
}
