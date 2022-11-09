using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using log4net;

namespace MTApiService
{
    [ServiceContract(CallbackContract = typeof(IMtApiCallback), SessionMode = SessionMode.Required)]
    public interface IMtApi
    {
        [OperationContract]
        bool Connect();

        [OperationContract(IsOneWay = true)]
        void Disconnect();

        [OperationContract]
        MtResponse SendCommand(MtCommand command);

        [OperationContract]
        List<MtQuote> GetQuotes();
    }

    [ServiceContract]
    public interface IMtApiCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnQuoteUpdate(MtQuote quote);

        [OperationContract(IsOneWay = true)]
        void OnServerStopped();

        [OperationContract(IsOneWay = true)]
        void OnQuoteAdded(MtQuote quote);

        [OperationContract(IsOneWay = true)]
        void OnQuoteRemoved(MtQuote quote);

        [OperationContract(IsOneWay = true)]
        void OnMtEvent(MtEvent ntEvent);
    }
}
