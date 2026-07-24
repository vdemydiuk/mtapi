// This is the main DLL file.

#include "Stdafx.h"

#include <array>
#include <functional>
#include <string>
#include <codecvt>
#include "Mt5Handler.h"
#include "MtService.h"

// Capacities (in wchar_t, including the null terminator) of the caller-allocated
// MQL string buffers. They must match the StringInit calls in the expert advisor:
// StringInit(_error, 1000, 0) and StringInit(payload, 5000, 0).
static const size_t ERROR_BUFFER_CAPACITY = 1000;
static const size_t PAYLOAD_BUFFER_CAPACITY = 5000;

static std::wstring convertToWString(const std::string& src)
{
    using convert_typeX = std::codecvt_utf8<wchar_t>;
    std::wstring_convert<convert_typeX, wchar_t> converterX;
    return converterX.from_bytes(src);
}

static void convertSystemString(wchar_t* dest, const std::string& src, size_t capacity)
{
    auto wstr = convertToWString(src);
    size_t len = wstr.length();
    if (len >= capacity)
        len = capacity - 1;
    memcpy(dest, wstr.c_str(), len * sizeof(wchar_t));
    dest[len] = L'\0';
}

static std::string convertWString(const wchar_t* src)
{
    std::wstring string_to_convert(src);
    using convert_typeX = std::codecvt_utf8<wchar_t>;
    std::wstring_convert<convert_typeX, wchar_t> converterX;
    return converterX.to_bytes(string_to_convert);
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
        convertSystemString(err, e.what(), ERROR_BUFFER_CAPACITY);
        MtService::GetInstance().LogError(e.what());
    }
    return result;
}

_DLLAPI int _stdcall initExpert(int expertHandle, int port, wchar_t* err)
{
    return Execute<int>([&expertHandle, &port]() {
        auto mt_handler = std::make_unique<MT5Handler>(expertHandle);
        MtService::GetInstance().InitExpert(port, expertHandle, std::move(mt_handler));
        return 1;
        }, err, 0);
}

_DLLAPI int _stdcall deinitExpert(int expertHandle, wchar_t* err)
{
    return Execute<int>([&expertHandle]() {
        MtService::GetInstance().DeinitExpert(expertHandle);
        return 1;
        }, err, 0);
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
        convertSystemString(res, MtService::GetInstance().GetCommandPayload(expertHandle), PAYLOAD_BUFFER_CAPACITY);
        return 1;
        }, err, 0);
}

// Size-aware replacement for getPayload: copies at most (capacity - 1) characters
// into res (always null-terminated) and returns the full payload length in wchar_t
// (excluding the null terminator), or -1 on error. If the returned length is
// >= capacity, the caller should re-allocate its buffer and call again.
_DLLAPI int _stdcall getPayload2(int expertHandle, wchar_t* res, int capacity, wchar_t* err)
{
    return Execute<int>([&expertHandle, res, &capacity]() {
        auto wstr = convertToWString(MtService::GetInstance().GetCommandPayload(expertHandle));
        int required = static_cast<int>(wstr.length());
        if (capacity > 0)
        {
            int len = required < capacity ? required : capacity - 1;
            memcpy(res, wstr.c_str(), static_cast<size_t>(len) * sizeof(wchar_t));
            res[len] = L'\0';
        }
        return required;
        }, err, -1);
}