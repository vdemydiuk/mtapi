// This is the main DLL file.

#include "Stdafx.h"

#include "MT4Handler.h"
#include "MtService.h"
#include "Windows.h"
#include <functional>
#include <string>

// Converts UTF-8 to UTF-16 and copies it into a buffer provided by MQL.
// At most 1000 characters are copied and no terminating null is written:
// MQL initializes the buffer with zeros (StringInit) before the call.
static void convertSystemString(wchar_t* dest, const std::string& src)
{
    if (src.empty())
        return;

    const int src_len = static_cast<int>(src.size());
    const int len = MultiByteToWideChar(CP_UTF8, 0, src.data(), src_len, nullptr, 0);
    if (len <= 0)
        return;

    std::wstring wstr(len, L'\0');
    MultiByteToWideChar(CP_UTF8, 0, src.data(), src_len, &wstr[0], len);
    memcpy(dest, wstr.c_str(), wcsnlen(wstr.c_str(), 1000) * sizeof(wchar_t));
}

// Converts a null-terminated UTF-16 string from MQL to UTF-8.
static std::string convertWString(const wchar_t* src)
{
    if (src == nullptr)
        return std::string();

    const int src_len = static_cast<int>(wcslen(src));
    if (src_len == 0)
        return std::string();

    const int len = WideCharToMultiByte(CP_UTF8, 0, src, src_len, nullptr, 0, nullptr, nullptr);
    if (len <= 0)
        return std::string();

    std::string result(len, '\0');
    WideCharToMultiByte(CP_UTF8, 0, src, src_len, &result[0], len, nullptr, nullptr);
    return result;
}

template <typename T> T Execute(std::function<T()> func, wchar_t* err, T default_value)
{
    T result = default_value;
    try
    {
        result = func();
    }
    catch (std::exception& e)
    {
        convertSystemString(err, e.what());
        MtService::GetInstance().LogError(e.what());
    }
    return result;
}

_DLLAPI bool _stdcall initExpert(int expertHandle, int port, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &port]() {
        auto mt_handler = std::make_unique<MT4Handler>(expertHandle);
        MtService::GetInstance().InitExpert(port, expertHandle, std::move(mt_handler));
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall deinitExpert(int expertHandle, wchar_t* err)
{
    return Execute<bool>([&expertHandle]() {
        MtService::GetInstance().DeinitExpert(expertHandle);
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall sendEvent(int expertHandle, int event_type, const wchar_t* payload, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &event_type, payload]() {
        MtService::GetInstance().SendEvent(expertHandle, event_type, convertWString(payload));
        return true;
        }, err, false);
}

_DLLAPI int _stdcall sendResponse(int expertHandle, const wchar_t* response, wchar_t* err)
{
    return Execute<int>([&expertHandle, response]() {
        MtService::GetInstance().SendResponse(expertHandle, convertWString(response));
        return 1;
        }, err, 0);
}

_DLLAPI int _stdcall getCommandType(int expertHandle, int& res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &res]() {
        res = MtService::GetInstance().GetCommandType(expertHandle);
        return 1;
        }, err, 0);
}

_DLLAPI int _stdcall getPayload(int expertHandle, wchar_t* res, wchar_t* err)
{
    return Execute<int>([&expertHandle, res]() {
        convertSystemString(res, MtService::GetInstance().GetCommandPayload(expertHandle));
        return 1;
        }, err, 0);
}