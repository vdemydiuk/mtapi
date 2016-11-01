// This is the main DLL file.

#include "stdafx.h"

#include "MT4Handler.h"
#include "Windows.h"
#include < vcclr.h >

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

void convertSystemString(wchar_t* dest, String^ src)
{
    pin_ptr<const wchar_t> wch = PtrToStringChars(src);
    memcpy(dest, wch, wcslen(wch) * sizeof(wchar_t));
    dest[wcslen(wch)] = '\0';
}

int _stdcall initExpert(int expertHandle, int port, wchar_t* symbol, double bid, double ask, wchar_t* err)
{
    try
    {
        MT4Handler^ mtHandler = gcnew MT4Handler();

        MtServerInstance::GetInstance()->InitExpert(expertHandle, port, gcnew String(symbol), bid, ask, mtHandler);
    }
    catch (Exception^ e)
    {
        convertSystemString(err, e->Message);
        Debug::WriteLine("[ERROR] MTConnector:initExpert(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall deinitExpert(int expertHandle, wchar_t* err)
{
    try
    {
        MtServerInstance::GetInstance()->DeinitExpert(expertHandle);
    }    
    catch (Exception^ e)
    {
        convertSystemString(err, e->Message);
        Debug::WriteLine("[ERROR] MTConnector:deinitExpert(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall updateQuote(int expertHandle, wchar_t* symbol, double bid, double ask, wchar_t* err)
{
    try
    {
        MtServerInstance::GetInstance()->SendQuote(expertHandle, gcnew String(symbol), bid, ask);
    }
    catch (Exception^ e)
    {
        convertSystemString(err, e->Message);
        Debug::WriteLine("[ERROR] MTConnector:updateQuote(): " + e->Message);
        return 0;
    }
    return 1;
}


int _stdcall sendEvent(int expertHandle, int eventType, wchar_t* payload, wchar_t* err)
{
    try
    {
        MtServerInstance::GetInstance()->SendEvent(expertHandle, eventType, gcnew String(payload));
    }
    catch (Exception^ e)
    {
        convertSystemString(err, e->Message);
        Debug::WriteLine("[ERROR] MTConnector:updateTimeBar(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendIntResponse(int expertHandle, int response)
{
    try
    {
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseInt(response));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:sendIntResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendBooleanResponse(int expertHandle, int response)
{
    try
    {
        bool value = (response != 0) ? true : false;

        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseBool(value));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:sendBooleanResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendDoubleResponse(int expertHandle, double response)
{
    try
    {
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseDouble(response));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:sendDoubleResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendStringResponse(int expertHandle, wchar_t* response)
{
    try
    {
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseString(gcnew String(response)));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:sendStringResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendVoidResponse(int expertHandle)
{
    try
    {        
        MtServerInstance::GetInstance()->SendResponse(expertHandle, nullptr);
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:sendVoidResponse(): " + e->Message);
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

        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseDoubleArray(list));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:sendDoubleArrayResponse(): " + e->Message);
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

        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseIntArray(list));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:sendIntArrayResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall sendLongResponse(int expertHandle, __int64 response)
{
    try
    {
        MtServerInstance::GetInstance()->SendResponse(expertHandle, gcnew MtResponseLong(response));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:sendLongResponse(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getCommandType(int expertHandle, int* res)
{
    try
    {
        *res = MtServerInstance::GetInstance()->GetCommandType(expertHandle);
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:getCommandType(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getIntValue(int expertHandle, int paramIndex, int* res)
{
    try
    {
        *res = (int)MtServerInstance::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:getIntValue(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getDoubleValue(int expertHandle, int paramIndex, double* res)
{
    try
    {
        *res = (double)MtServerInstance::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:getDoubleValue(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getStringValue(int expertHandle, int paramIndex, wchar_t* res)
{
    try
    {
        convertSystemString(res, (String^)MtServerInstance::GetInstance()->GetCommandParameter(expertHandle, paramIndex));
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MTConnector:getStringValue(): " + e->Message);
        return 0;
    }
    return 1;
}

int _stdcall getBooleanValue(int expertHandle, int paramIndex, int* res)
{
    try
    {
        bool val = (bool)MtServerInstance::GetInstance()->GetCommandParameter(expertHandle, paramIndex);
        *res = val == true ? 1 : 0;
    }
    catch (Exception^ e)
    {
        Debug::WriteLine("[ERROR] MT5Connector:getBooleanValue(): " + e->Message);
        return 0;
    }
    return 1;
}