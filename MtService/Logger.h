#pragma once

#include <string>
#include <memory>

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

private:
    class LoggerImpl;
    std::unique_ptr<LoggerImpl> logger_impl_;
};
