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

public struct CMqlTick
{
    __int64                time;          // Time of the last prices update
    double                bid;           // Current Bid price
    double                ask;           // Current Ask price
    double                last;          // Price of the last deal (Last)
    unsigned __int64    volume;        // Volume for the current Last price
};

public struct CMqlBookInfo
{
    int        type;       // Order type from ENUM_BOOK_TYPE enumeration
    double    price;      // Price
    __int64    volume;     // Volume
};
#pragma pack(pop)

void convertSystemString(wchar_t* dest, String^ src)
{
    pin_ptr<const wchar_t> wch = PtrToStringChars(src);
    memcpy(dest, wch, wcsnlen(wch, 1000) * sizeof(wchar_t));
    dest[wcsnlen(wch, 1000)] = L'\0';
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
        MtAdapter::GetInstance()->SendResponse(expertHandle, nullptr);
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

_DLLAPI int _stdcall sendMqlTickResponse(int expertHandle, CMqlTick* response, wchar_t* err)
{
    return Execute<int>([&expertHandle, response]() {
        MtMqlTick^ mtResponse = gcnew MtMqlTick();
        mtResponse->time = response->time;
        mtResponse->bid = response->bid;
        mtResponse->ask = response->ask;
        mtResponse->last = response->last;
        mtResponse->volume = response->volume;
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseMqlTick(mtResponse));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall sendMqlBookInfoArrayResponse(int expertHandle, CMqlBookInfo values[], int size, wchar_t* err)
{
    return Execute<int>([&expertHandle, values, &size]() {
        array<MtMqlBookInfo^>^ list = gcnew array<MtMqlBookInfo^>(size);
        for (int i = 0; i < size; i++)
        {
            MtMqlBookInfo^ info = gcnew MtMqlBookInfo();
            info->type = values[i].type;
            info->price = values[i].price;
            info->volume = values[i].volume;
            list[i] = info;
        }
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseMqlBookInfoArray(list));
        return 1;
    }, err, 0);
}

//----------- get values -------------------------------

_DLLAPI int _stdcall getCommandType(int expertHandle, int* res, wchar_t* err)
{
    return Execute<int>([&expertHandle, res]() {
        *res = MtAdapter::GetInstance()->GetCommandType(expertHandle);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getIntValue(int expertHandle, int paramIndex, int* res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, res]() {
        *res = (int)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getDoubleValue(int expertHandle, int paramIndex, double* res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, res]() {
        *res = (double)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getStringValue(int expertHandle, int paramIndex, wchar_t* res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, res]() {
        convertSystemString(res, (String^)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex));
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getULongValue(int expertHandle, int paramIndex, unsigned __int64* res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, res]() {
        *res = (unsigned __int64)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getLongValue(int expertHandle, int paramIndex, __int64* res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, res]() {
        *res = (__int64)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getBooleanValue(int expertHandle, int paramIndex, int* res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, res]() {
        bool val = (bool)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        *res = val == true ? 1 : 0;
        return 1;
    }, err, 0);
}

_DLLAPI int _stdcall getUIntValue(int expertHandle, int paramIndex, unsigned int* res, wchar_t* err)
{
    return Execute<int>([&expertHandle, &paramIndex, res]() {
        *res = (unsigned int)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        return 1;
    }, err, 0);
}
