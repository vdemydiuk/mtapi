// This is the main DLL file.

#include "Stdafx.h"

#include "MT5Connector.h"
#include "MT5Handler.h"

#include "Windows.h"
#include <vcclr.h>
#include <functional>

using namespace System;
using namespace MTApiService;
using namespace System::Runtime::InteropServices;
using namespace System::Reflection;
using namespace System::Text;
using namespace System::Collections::Generic;
using namespace System::Diagnostics;
using namespace System::Security::Cryptography; 
using namespace System::Security;

#pragma pack(push, 1)
public struct CMqlRates
{
    __int64    time;         // Period start time
    double  open;         // Open price
    double  high;         // The highest price of the period
    double  low;          // The lowest price of the period
    double  close;        // Close price
    __int64 tick_volume;  // Tick volume
    int     spread;       // Spread
    __int64 real_volume;  // Trade volume
};
#pragma pack(pop)

void convertSystemString(wchar_t* dest, String^ src)
{
    if (src != nullptr) {
        pin_ptr<const wchar_t> wch = PtrToStringChars(src);
        memcpy(dest, wch, wcsnlen(wch, 1000) * sizeof(wchar_t));
        dest[wcsnlen(wch, 1000)] = L'\0';
    }
    else
    {
        dest[0] = L'\0';
    }
}

#define _DLLAPI extern "C" __declspec(dllexport)

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
        MtAdapter::GetInstance()->LogError(e->Message);
    }
    return result;
}

_DLLAPI int _stdcall initExpert(int expertHandle, int port, wchar_t* symbol, double bid, double ask, int isTestMode, wchar_t* err)
{
    return Execute<int>([&expertHandle, &port, symbol, &bid, &ask, &isTestMode]() {
        bool isTesting = (isTestMode != 0) ? true : false;
        auto expert = gcnew Mt5Expert(expertHandle, gcnew String(symbol), bid, ask, gcnew MT5Handler(), isTesting);
        MtAdapter::GetInstance()->AddExpert(port, expert);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall deinitExpert(int expertHandle, wchar_t* err)
{
    return Execute<int>([&expertHandle]() {
        MtAdapter::GetInstance()->RemoveExpert(expertHandle);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall updateQuote(int expertHandle, wchar_t* symbol, double bid, double ask, wchar_t* err)
{
    return Execute<int>([&expertHandle, symbol, &bid, &ask]() {
        MtAdapter::GetInstance()->SendQuote(expertHandle, gcnew String(symbol), bid, ask);
        return 1;
    }, err, 0);
}

_DLLAPI bool _stdcall sendEvent(int expertHandle, int eventType, wchar_t* payload, wchar_t* err)
{
    return Execute<bool>([&expertHandle, &eventType, payload]() {
        MtAdapter::GetInstance()->SendEvent(expertHandle, eventType, gcnew String(payload));
        return true;
    }, err, false);
}

_DLLAPI int _stdcall sendIntResponse(int expertHandle, int response, wchar_t* err)
{
    return Execute<int>([&expertHandle, &response]() {
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseInt(response));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendLongResponse(int expertHandle, __int64 response, wchar_t* err)
{
    return Execute<int>([&expertHandle, &response]() {
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseLong(response));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendULongResponse(int expertHandle, unsigned __int64 response, wchar_t* err)
{
    return Execute<int>([&expertHandle, &response]() {
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseULong(response));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendBooleanResponse(int expertHandle, int response, wchar_t* err)
{
    return Execute<int>([&expertHandle, &response]() {
        bool value = (response != 0) ? true : false;
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseBool(value));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendDoubleResponse(int expertHandle, double response, wchar_t* err)
{
    return Execute<int>([&expertHandle, &response]() {
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseDouble(response));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendStringResponse(int expertHandle, wchar_t* response, wchar_t* err)
{
    return Execute<int>([&expertHandle, response]() {
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseString(gcnew String(response)));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendVoidResponse(int expertHandle, wchar_t* err)
{
    return Execute<int>([&expertHandle]() {
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseObject(nullptr));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendDoubleArrayResponse(int expertHandle, double* values, int size, wchar_t* err)
{
    return Execute<int>([&expertHandle, values, &size]() {
        array<double>^ list = gcnew array<double>(size);
        for (int i = 0; i < size; i++)
            list[i] = values[i];
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseDoubleArray(list));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendIntArrayResponse(int expertHandle, int* values, int size, wchar_t* err)
{
    return Execute<int>([&expertHandle, values, &size]() {
        array<int>^ list = gcnew array<int>(size);
        for (int i = 0; i < size; i++)
            list[i] = values[i];
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseIntArray(list));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendLongArrayResponse(int expertHandle, __int64* values, int size, wchar_t* err)
{
    return Execute<int>([&expertHandle, values, &size]() {
        array<System::Int64>^ list = gcnew array<System::Int64>(size);
        for (int i = 0; i < size; i++)
            list[i] = values[i];
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseLongArray(list));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendMqlRatesArrayResponse(int expertHandle, CMqlRates values[], int size, wchar_t* err)
{
    return Execute<int>([&expertHandle, values, &size]() {
        array<MtMqlRates^>^ list = gcnew array<MtMqlRates^>(size);
        for (int i = 0; i < size; i++)
        {
            MtMqlRates^ rates = gcnew MtMqlRates();
            rates->time = values[i].time;
            rates->open = values[i].open;
            rates->high = values[i].high;
            rates->low = values[i].low;
            rates->close = values[i].close;
            rates->tick_volume = values[i].tick_volume;
            rates->spread = values[i].spread;
            rates->real_volume = values[i].real_volume;
            list[i] = rates;
        }
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseMqlRatesArray(list));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendErrorResponse(int expertHandle, int code, wchar_t* message, wchar_t* err)
{
    return Execute<int>([&expertHandle, &code, message]() {
        MtResponseString^ res = gcnew MtResponseString(gcnew String(message));
        res->ErrorCode = code;
        MtAdapter::GetInstance()->SendResponse(expertHandle, res);
        return true;
    }, err, 0);
}

//----------- get values -------------------------------

_DLLAPI int _stdcall getCommandType(int expertHandle, int& res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &res]() {
        res = MtAdapter::GetInstance()->GetCommandType(expertHandle);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getIntValue(int expertHandle, int paramIndex, int& res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, &res]() {
        res = MtAdapter::GetInstance()->GetCommandParameter<int>(expertHandle, paramIndex);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getDoubleValue(int expertHandle, int paramIndex, double& res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, &res]() {
        res = MtAdapter::GetInstance()->GetCommandParameter<double>(expertHandle, paramIndex);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getStringValue(int expertHandle, int paramIndex, wchar_t* res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, res]() {
        convertSystemString(res, MtAdapter::GetInstance()->GetCommandParameter<String^>(expertHandle, paramIndex));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getULongValue(int expertHandle, int paramIndex, unsigned __int64& res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, &res]() {
        res = MtAdapter::GetInstance()->GetCommandParameter<unsigned __int64>(expertHandle, paramIndex);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getLongValue(int expertHandle, int paramIndex, __int64& res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, &res]() {
        res = MtAdapter::GetInstance()->GetCommandParameter<__int64>(expertHandle, paramIndex);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getBooleanValue(int expertHandle, int paramIndex, int& res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, &res]() {
        bool val = MtAdapter::GetInstance()->GetCommandParameter<bool>(expertHandle, paramIndex);
        res = val == true ? 1 : 0;
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getUIntValue(int expertHandle, int paramIndex, unsigned int& res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, &res]() {
        res = MtAdapter::GetInstance()->GetCommandParameter<unsigned int>(expertHandle, paramIndex);
        return 1;
    }, err, 0);
}
