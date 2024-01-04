#include "pch.h"
#include "LogConfigurator.h"

#include <boost/log/trivial.hpp>
#include <boost/log/utility/setup.hpp>
#include <windows.h>

static const std::string DEFAULT_FILE_EXTENSION = ".log";

using namespace boost::log;

namespace
{
trivial::severity_level ToSeverityLevel(LogLevel level)
{
    switch (level)
    {
        case LogLevel::Fatal:
            return trivial::fatal;
        case LogLevel::Error:
            return trivial::error;
        case LogLevel::Warning:
            return trivial::warning;
        case LogLevel::Info:
            return trivial::info;
        case LogLevel::Debug:
            return trivial::debug;
        case LogLevel::Trace:
            return trivial::trace;
        default:
            assert(false);
    }
    throw;
}

std::string GetTempDirectoryPathImpl()
{
    std::string tmp_prefix;
    char char_path[MAX_PATH];
    if (auto s = GetTempPathA(MAX_PATH, char_path))
    {
        tmp_prefix = std::string(char_path, std::size_t(s - 1));
    }
    std::replace(tmp_prefix.begin(), tmp_prefix.end(), '\\', '/');
    return tmp_prefix;
}
} // namespace

void LogConfigurator::Setup(LogLevel level, OutputType output_type,
                         const std::string& profile_name)
{
    if (profile_name.empty())
        throw std::runtime_error("Invalid profile name");

    static const std::string COMMON_FMT("[%TimeStamp%] [%ThreadID%] [%Severity%] [%ClassName%]:  %Message%");

    register_simple_formatter_factory<trivial::severity_level, char>("Severity");

    if ((output_type & OutputType::Console) == OutputType::Console)
    {
        // Output message to console
        add_console_log(
            std::cout,
            keywords::format = COMMON_FMT,
            keywords::auto_flush = true);
    }

    if ((output_type & OutputType::File) == OutputType::File)
    {
        std::string file_name = profile_name;
         file_name += "_%Y%m%d_%2N";
        file_name += DEFAULT_FILE_EXTENSION;
        
        std::string file_path = GetTempDirectoryPathImpl() + "\\" + profile_name + "\\" + file_name;

        // Output message to file, rotates when file reached 1mb or at midnight every day. Each log file
        // is capped at 1mb and total is 20mb
        add_file_log(
            keywords::file_name = file_path,
            keywords::rotation_size = 1 * 1024 * 1024,
            keywords::max_size = 20 * 1024 * 1024,
            keywords::time_based_rotation = sinks::file::rotation_at_time_point(0, 0, 0),
            keywords::format = COMMON_FMT,
            keywords::auto_flush = true);
    }

    add_common_attributes();

    core::get()->set_filter(trivial::severity >= ToSeverityLevel(level));
}
