#pragma once

#include "LogLevel.h"
#include <type_traits>
#include <string>

enum class OutputType : int
{
    Console = 0x1,
    File = 0x2
};

inline OutputType operator|(OutputType lhs, OutputType rhs)
{
    using T = std::underlying_type_t<OutputType>;
    return static_cast<OutputType>(static_cast<T>(lhs) | static_cast<T>(rhs));
}

inline OutputType& operator|=(OutputType& lhs, OutputType rhs)
{
    lhs = lhs | rhs;
    return lhs;
}

inline OutputType operator&(OutputType lhs, OutputType rhs)
{
    using T = std::underlying_type_t<OutputType>;
    return static_cast<OutputType>(static_cast<T>(lhs) & static_cast<T>(rhs));
}


class LogConfigurator
{
public:
    static void Setup(LogLevel level, OutputType output_type,
                      const std::string& profile_name);
};