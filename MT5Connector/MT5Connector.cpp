// This is the main DLL file.

#include "Stdafx.h"

#include "MT5Connector.h"
#include "MT5Handler.h"

#include "Windows.h"
#include < vcclr.h >

using namespace System;
using namespace MTApiService;
using namespace System::Runtime::InteropServices;
using namespace System::Reflection;
using namespace System::Text;
using namespace  System::Collections::Generic;
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
    memcpy(dest, wch, wcslen(wch) * sizeof(wchar_t));
    dest[wcslen(wch)] = '\0';
}

bool VerifySignature(System::String^ inputData, System::String^ signature, System::String^ publicKey)
{
    bool verifyResult = false;

    try
    {
        DSACryptoServiceProvider^ dsa = gcnew DSACryptoServiceProvider();
        dsa->FromXmlString(publicKey);
        array<System::Byte>^ data = UTF8Encoding::ASCII->GetBytes(inputData);
        array<System::Byte>^ signatureData = Convert::FromBase64String(signature);
        verifyResult = dsa->VerifyData(data, signatureData);
    }
    catch(Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:VerifySignature(): failed. " + e->Message);
        verifyResult = false;
    }

    return verifyResult;
}

bool g_IsVerified = true;

void _stdcall verify(int isDemo, wchar_t* accountName, long accountNumber)
{
    if (isDemo != 0)
    {
        g_IsVerified = true;
        return;
    }

    System::String^ signature = MtRegistryManager::ReadSignatureKey(gcnew String(accountName), accountNumber.ToString());
    Resources::ResourceManager^ rm = gcnew Resources::ResourceManager(L"MT5Connector.cl", Assembly::GetExecutingAssembly());
    System::String^ inputData = gcnew System::String(accountName);
    inputData += accountNumber.ToString();
    System::String^ publicKey = rm->GetString(L"cl");
    g_IsVerified = VerifySignature(inputData, gcnew System::String(signature), publicKey);
}

int _stdcall initExpert(int expertHandle, int port, wchar_t* symbol, double bid, double ask, wchar_t* err)
{
    if (g_IsVerified == false)
    {
        System::String^ errorVerified = "Verification is failed!\nPlease contact with support.";
        convertSystemString(err, errorVerified);
        Debug::WriteLine("[ERROR] MT5Connector:initExpert(): not verified");
        return 0;
    }

    try
    {
        MT5Handler^ mtHander = gcnew MT5Handler();
        MtAdapter::GetInstance()->InitExpert(expertHandle, port, gcnew String(symbol), bid, ask, mtHander);
    }
    catch (Exception^ e)
    {
        convertSystemString(err, e->Message);
        Debug::WriteLine("[ERROR] MT5Connector:initExpert(): " + e->Message);

        return 0;
    }
    return 1;
}

int _stdcall deinitExpert(int expertHandle, wchar_t* err)
{
    try
    {
        MtAdapter::GetInstance()->DeinitExpert(expertHandle);
    }    
    catch (Exception^ e)
    {
        convertSystemString(err, e->Message);
        Debug::WriteLine("[ERROR] MT5Connector:deinitExpert(): " + e->Message);

        return 0;
    }
    return 1;
}

int _stdcall updateQuote(int expertHandle, wchar_t* symbol, double bid, double ask, wchar_t* err)
{
    try
    {
        MtAdapter::GetInstance()->SendQuote(expertHandle, gcnew String(symbol), bid, ask);
    }
    catch (Exception^ e)
    {
        convertSystemString(err, e->Message);
        Debug::WriteLine("[ERROR] MT5Connector:updateQuote(): " + e->Message);

        return 0;
    }
    return 1;
}

int _stdcall sendIntResponse(int expertHandle, int response)
{
    try
    {
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseInt(response));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendIntResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendLongResponse(int expertHandle, __int64 response)
{
    try
    {
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseLong(response));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendLongResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendULongResponse(int expertHandle, unsigned __int64 response)
{
    try
    {
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseULong(response));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendLongResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendBooleanResponse(int expertHandle, int response)
{
    try
    {
        bool value = (response != 0) ? true : false;

        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseBool(value));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendBooleanResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendDoubleResponse(int expertHandle, double response)
{
    try
    {
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseDouble(response));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendDoubleResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendStringResponse(int expertHandle, wchar_t* response)
{
    try
    {
        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseString(gcnew String(response)));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendStringResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendVoidResponse(int expertHandle)
{
    try
    {        
        MtAdapter::GetInstance()->SendResponse(expertHandle, nullptr);
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendVoidResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendDoubleArrayResponse(int expertHandle, double* values, int size)
{
    try
    {
        array<double>^ list = gcnew array<double>(size);

        for(int i = 0; i < size; i++)
        {
            list[i] = values[i];
        }

        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseDoubleArray(list));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendDoubleArrayResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendIntArrayResponse(int expertHandle, int* values, int size)
{
    try
    {
        array<int>^ list = gcnew array<int>(size);

        for(int i = 0; i < size; i++)
        {
            list[i] = values[i];
        }

        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseIntArray(list));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendIntArrayResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendLongArrayResponse(int expertHandle, __int64* values, int size)
{
    try
    {
        array<System::Int64>^ list = gcnew array<System::Int64>(size);

        for(int i = 0; i < size; i++)
        {
            list[i] = values[i];
        }

        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseLongArray(list));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendLongArrayResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendMqlRatesArrayResponse(int expertHandle, CMqlRates values[], int size)
{
    try
    {
        array<MtMqlRates^>^ list = gcnew array<MtMqlRates^>(size);

        for(int i = 0; i < size; i++)
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
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendMqlRatesArrayResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendMqlTickResponse(int expertHandle, CMqlTick* response, int size)
{
    try
    {
        MtMqlTick^ mtResponse = gcnew MtMqlTick();

        mtResponse->time = response->time;
        mtResponse->bid = response->bid;
        mtResponse->ask = response->ask;
        mtResponse->last = response->last;
        mtResponse->volume = response->volume;

        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseMqlTick(mtResponse));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendMqlTickResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendMqlBookInfoArrayResponse(int expertHandle, CMqlBookInfo values[], int size)
{
    try
    {
        array<MtMqlBookInfo^>^ list = gcnew array<MtMqlBookInfo^>(size);

        for(int i = 0; i < size; i++)
        {
            MtMqlBookInfo^ info = gcnew MtMqlBookInfo();

            info->type = values[i].type;
            info->price = values[i].price;
            info->volume = values[i].volume;

            list[i] = info;
        }

        MtAdapter::GetInstance()->SendResponse(expertHandle, gcnew MtResponseMqlBookInfoArray(list));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:sendMqlRatesArrayResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

//----------- get values -------------------------------

int _stdcall getCommandType(int expertHandle, int* res)
{
    try
    {
        *res = MtAdapter::GetInstance()->GetCommandType(expertHandle);
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:getCommandType(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getIntValue(int expertHandle, int paramIndex, int* res)
{
    try
    {
        *res = (int)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:getIntValue(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getDoubleValue(int expertHandle, int paramIndex, double* res)
{
    try
    {
        *res = (double)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:getDoubleValue(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getStringValue(int expertHandle, int paramIndex, wchar_t* res)
{
    try
    {
        convertSystemString(res, (String^)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:getStringValue(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getULongValue(int expertHandle, int paramIndex, unsigned __int64* res)
{
    try
    {
        *res = (unsigned long)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:getULongValue(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getLongValue(int expertHandle, int paramIndex, __int64* res)
{
    try
    {
        *res = (long)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:getLongValue(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getBooleanValue(int expertHandle, int paramIndex, int* res)
{
    try
    {
        bool val = (bool)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        *res = val == true ? 1 : 0;
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:getBooleanValue(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getUIntValue(int expertHandle, int paramIndex, unsigned int* res)
{
    try
    {
        *res = (unsigned int)MtAdapter::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:getUIntValue(): " + e->Message);
        return 0;
    }
    return 1;
}