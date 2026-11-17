#pragma once

#include <atomic>
#include <string>
#include <memory>

#include "LogLevel.h"

class Logger
{
public:
    Logger(const char* class_name);
    ~Logger();

    void Fatal(const char* fmt, ...);
    void Error(const char* fmt, ...);
    void Warning(const char* fmt, ...);
    void Info(const char* fmt, ...);
    void Debug(const char* fmt, ...);
    void Trace(const char* fmt, ...);

    // Returns true if messages of the given level are written to the log.
    // Logging methods already skip formatting of disabled levels, but their arguments are
    // still evaluated: guard expensive arguments (e.g. ToHex) with this check.
    static bool IsEnabled(LogLevel level)
    {
        return static_cast<int>(level) <= level_.load(std::memory_order_relaxed);
    }

    // Sets the most verbose level that is written to the log (called by LogConfigurator).
    static void SetLevel(LogLevel level);

private:
    class LoggerImpl;
    std::unique_ptr<LoggerImpl> logger_impl_;

    static std::atomic<int> level_;
};
