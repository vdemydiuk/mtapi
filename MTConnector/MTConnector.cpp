// This is the main DLL file.

#include "Stdafx.h"

#include "MT4Handler.h"
#include "MtService.h"
#include "Windows.h"
#include <algorithm>
#include <functional>
#include <mutex>
#include <string>

// Buffer sizes (in characters) allocated by the MQL expert via StringInit().
// Keep in sync with MtApi*.mq*: payload = 5000, error = 1000.
static const size_t PAYLOAD_BUFFER_SIZE = 5000;
static const size_t ERROR_BUFFER_SIZE = 1000;

// Converts UTF-8 to UTF-16 and copies it into a buffer provided by MQL.
// Copies at most dest_size - 1 characters and always writes a terminating null.
static void convertSystemString(wchar_t* dest, size_t dest_size, const std::string& src)
{
    if (dest == nullptr || dest_size == 0)
        return;

    dest[0] = L'\0';
    if (src.empty())
        return;

    const int src_len = static_cast<int>(src.size());
    const int len = MultiByteToWideChar(CP_UTF8, 0, src.data(), src_len, nullptr, 0);
    if (len <= 0)
        return;

    std::wstring wstr(len, L'\0');
    MultiByteToWideChar(CP_UTF8, 0, src.data(), src_len, &wstr[0], len);
    const size_t count = (std::min)(wcsnlen(wstr.c_str(), wstr.size()), dest_size - 1);
    memcpy(dest, wstr.c_str(), count * sizeof(wchar_t));
    dest[count] = L'\0';
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
        convertSystemString(err, ERROR_BUFFER_SIZE, e.what());
        MtService::GetInstance().LogError(e.what());
    }
    return result;
}

// MtService runs an io thread that is joined by the destructor of its static instance.
// If the terminal unloads this DLL (MT4 does it when a backtest ends and no chart uses the expert),
// that destructor runs in DLL_PROCESS_DETACH under the loader lock, where joining a thread deadlocks
// and the io thread may still run code of the unloaded DLL. Pinning keeps the DLL loaded until
// the process exits, so the service is reused by the next expert.
static void pinModule()
{
    static std::once_flag flag;
    std::call_once(flag, []() {
        HMODULE module = nullptr;
        if (!GetModuleHandleExW(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_PIN,
            reinterpret_cast<LPCWSTR>(&pinModule), &module))
        {
            MtService::GetInstance().LogError("pinModule: GetModuleHandleExW failed, error = " + std::to_string(GetLastError()));
        }
    });
}

_DLLAPI bool _stdcall initExpert(int expertHandle, int port, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &port]() {
        auto mt_handler = std::make_unique<MT4Handler>(expertHandle);
        pinModule();
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
        convertSystemString(res, PAYLOAD_BUFFER_SIZE, MtService::GetInstance().GetCommandPayload(expertHandle));
        return 1;
        }, err, 0);
}