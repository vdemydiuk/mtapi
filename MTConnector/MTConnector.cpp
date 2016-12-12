// This is the main DLL file.

#include "stdafx.h"

#include "MT4Handler.h"
#include "Windows.h"
#include <vcclr.h>
#include <functional>

using namespace System;
using namespace MTApiService;
using namespace System::Runtime::InteropServices;
using namespace System::Reflection;
using namespace System::Security::Cryptography; 
using namespace System::Security;
using namespace System::Text;
using namespace System::IO;
using namespace  System::Collections::Generic;
using namespace System::Diagnostics;

#define _DLLAPI extern "C" __declspec(dllexport)

void convertSystemString(wchar_t* dest, String^ src)
{
    pin_ptr<const wchar_t> wch = PtrToStringChars(src);
    memcpy(dest, wch, wcslen(wch) * sizeof(wchar_t));
    dest[wcslen(wch)] = '\0';
}

template <typename T> T Execute(std::function<T()> func, wchar_t* err, T default_value)
{
    T result = default_value;
    try
    {
        result = func();
    }
    catch (Exception^ e)
    {
        convertSystemString(err, e->Message);
    }
    return result;
}

_DLLAPI bool _stdcall initExpert(int expertHandle, int port, wchar_t* symbol, double bid, double ask, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &port, symbol, &bid, &ask]() {
        MT4Handler^ mtHandler = gcnew MT4Handler();
        MtServerInstance::GetInstance()->InitExpert(expertHandle, port, gcnew String(symbol), bid, ask, mtHandler);
        return true;
    }, err, false);

}

_DLLAPI bool _stdcall deinitExpert(int expertHandle, wchar_t* err)
{
    return Execute<bool>([&expertHandle]() {
        MtServerInstance::GetInstance()->DeinitExpert(expertHandle);
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall updateQuote(int expertHandle, wchar_t* symbol, double bid, double ask, wchar_t* err)
{
    return Execute<bool>([&expertHandle, symbol, &bid, &ask]() {
        MtServerInstance::GetInstance()->SendQuote(expertHandle, gcnew String(symbol), bid, ask);
        return true;
    }, err, false);
}


_DLLAPI bool _stdcall sendEvent(int expertHandle, int eventType, wchar_t* payload, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &eventType, payload]() {
        MtServerInstance::GetInstance()->SendEvent(expertHandle, eventType, gcnew String(payload));
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall sendIntResponse(int expertHandle, int response, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &response]() {
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseInt(response));
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall sendBooleanResponse(int expertHandle, bool response, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &response]() {
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseBool(response));
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall sendDoubleResponse(int expertHandle, double response, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &response]() {
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseDouble(response));
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall sendStringResponse(int expertHandle, wchar_t* response, wchar_t* err)
{
    return Execute<bool>([&expertHandle, response]() {
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseString(gcnew String(response)));
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall sendErrorResponse(int expertHandle, int code, wchar_t* message, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &code, message]() {
        MtResponseString^ res = gcnew MtResponseString(gcnew String(message));
        res->ErrorCode = code;
        MtServerInstance::GetInstance()->SendResponse(expertHandle, res);
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall sendVoidResponse(int expertHandle, wchar_t* err)
{
    return Execute<bool>([&expertHandle]() {
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseObject(nullptr));
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall sendDoubleArrayResponse(int expertHandle, double* values, int size, wchar_t* err)
{
    return Execute<bool>([&expertHandle, values, &size]() {
        array<double>^ list = gcnew array<double>(size);
        for (int i = 0; i < size; i++)
        {
            list[i] = values[i];
        }
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseDoubleArray(list));
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall sendIntArrayResponse(int expertHandle, int* values, int size, wchar_t* err)
{
    return Execute<bool>([&expertHandle, values, &size]() {
        array<int>^ list = gcnew array<int>(size);
        for (int i = 0; i < size; i++)
        {
            list[i] = values[i];
        }
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseIntArray(list));
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall sendLongResponse(int expertHandle, __int64 response, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &response]() {
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseLong(response));
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall getCommandType(int expertHandle, int* res, wchar_t* err)
{
    return Execute<bool>([&expertHandle, res]() {
        *res = MtServerInstance::GetInstance()->GetCommandType(expertHandle);
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall getIntValue(int expertHandle, int paramIndex, int* res, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &paramIndex, res]() {
        *res = (int)MtServerInstance::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall getDoubleValue(int expertHandle, int paramIndex, double* res, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &paramIndex, res]() {
        *res = (double)MtServerInstance::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall getStringValue(int expertHandle, int paramIndex, wchar_t* res, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &paramIndex, res]() {
        convertSystemString(res, (String^)MtServerInstance::GetInstance()->GetCommandParameter(expertHandle, paramIndex));
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall getBooleanValue(int expertHandle, int paramIndex, int* res, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &paramIndex, res]() {
        *res = (bool)MtServerInstance::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        return true;
    }, err, false);
}

_DLLAPI bool _stdcall getLongValue(int expertHandle, int paramIndex, __int64* res, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &paramIndex, res]() {
        *res = (__int64)MtServerInstance::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        return true;
    }, err, false);
}