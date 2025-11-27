package logger

import (
	"os"
	"path/filepath"

	"go.uber.org/zap"
	"go.uber.org/zap/zapcore"
)

func NewLogger(logPath string, level string) *zap.Logger {
	// Ensure log directory exists
	if logPath != "" {
		os.MkdirAll(filepath.Dir(logPath), 0755)
	}

	config := zap.NewProductionConfig()
	
	// Set Level
	var zapLevel zapcore.Level
	if err := zapLevel.UnmarshalText([]byte(level)); err != nil {
		zapLevel = zap.InfoLevel
	}
	config.Level = zap.NewAtomicLevelAt(zapLevel)

	// Set Output
	if logPath != "" {
		config.OutputPaths = []string{"stdout", logPath}
	} else {
		config.OutputPaths = []string{"stdout"}
	}

	config.EncoderConfig.EncodeTime = zapcore.ISO8601TimeEncoder

	logger, _ := config.Build()
	return logger
}
