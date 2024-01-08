// This is the main DLL file.

#include "Stdafx.h"

#include <array>
#include <functional>
#include <string>
#include <codecvt>
#include "Mt5Handler.h"
#include "MtService.h"

static void convertSystemString(wchar_t* dest, const std::string& src)
{
    using convert_typeX = std::codecvt_utf8<wchar_t>;
    std::wstring_convert<convert_typeX, wchar_t> converterX;
    auto wstr = converterX.from_bytes(src);
    memcpy(dest, wstr.c_str(), wcsnlen(wstr.c_str(), 1000) * sizeof(wchar_t));
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
        convertSystemString(err, e.what());
        MtService::GetInstance().LogError(e.what());
    }
    return result;
}

_DLLAPI int _stdcall initExpert(int expertHandle, int port, int isTestMode, wchar_t* err)
{
    return Execute<int>([&expertHandle, &port, &isTestMode]() {
        bool isTesting = (isTestMode != 0) ? true : false;
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
        convertSystemString(res, MtService::GetInstance().GetCommandPayload(expertHandle));
        return 1;
        }, err, 0);
}