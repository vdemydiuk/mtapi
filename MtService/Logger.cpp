#include "pch.h"
#include "Logger.h"
#include "LogLevel.h"
#include "boost/log/common.hpp"
#include "boost/log/trivial.hpp"
#include "boost/log/sources/severity_logger.hpp"
#include <stdarg.h>

using namespace boost::log;

class Logger::LoggerImpl
{
public:
    LoggerImpl(const char* class_name)
    {
        boost_logger_.add_attribute("ClassName", attributes::constant<std::string>(class_name));
    }

    void LogSev(trivial::severity_level level, const char* msg)
    {
        BOOST_LOG_SEV(boost_logger_, level) << msg;
    }

    void LogSev(trivial::severity_level level, const char* fmt, va_list va)
    {
        char buf[8192];
        vsnprintf(buf, sizeof(buf), fmt, va);
        LogSev(level, buf);
    }

private:
    sources::severity_logger<trivial::severity_level> boost_logger_;
};

Logger::Logger(const char* class_name)
    : logger_impl_(new Logger::LoggerImpl(class_name))
{
}

Logger::~Logger()
{
}

void Logger::Fatal(const char* fmt, ...)
{
    va_list va;
    va_start(va, fmt);
    logger_impl_->LogSev(trivial::fatal, fmt, va);
    va_end(va);
}

void Logger::Error(const char* fmt, ...)
{
    va_list va;
    va_start(va, fmt);
    logger_impl_->LogSev(trivial::error, fmt, va);
    va_end(va);
}

void Logger::Warning(const char* fmt, ...)
{
    va_list va;
    va_start(va, fmt);
    logger_impl_->LogSev(trivial::warning, fmt, va);
    va_end(va);
}

void Logger::Info(const char* fmt, ...)
{
    va_list va;
    va_start(va, fmt);
    logger_impl_->LogSev(trivial::info, fmt, va);
    va_end(va);
}

void Logger::Debug(const char* fmt, ...)
{
    va_list va;
    va_start(va, fmt);
    logger_impl_->LogSev(trivial::debug, fmt, va);
    va_end(va);
}

void Logger::Trace(const char* fmt, ...)
{
    va_list va;
    va_start(va, fmt);
    logger_impl_->LogSev(trivial::trace, fmt, va);
    va_end(va);
}
