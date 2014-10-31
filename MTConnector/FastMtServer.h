// FastMtServer.h

#pragma once

using namespace MetaTraderApi;

public class FastMtServer
{
public:
	static FastMtServer& const Instance()
    {
            static FastMtServer theSingleInstance;
            return theSingleInstance;
    }

	public void Init(string user, int accountId, int connectionAttemt);
	public void Start(string symbol, double bid, double ask);
	public void Stop();
	public void SendQuote(string symbol, double bid, double ask);

private:
	FastMtServer(){}
	FastMtServer(OnlyOne& root){}


        public void Init(string user, int accountId, int connectionAttemt)
        {
            Logger.Instance.Write("FastMtQuoteServer::Init");

            if (_mtServer == null)
            {
                var initServerName = user + accountId.ToString();
                _mtServer = new QuoteServer(initServerName, connectionAttemt);
                _mtServer.ConnectionStatusUpdated +=new ConnectionStatusEventHandler(_mtServer_ConnectionStatusUpdated);
            }
        }

        public void Start(string symbol, double bid, double ask)
        {
            Logger.Instance.Write("FastMtQuoteServer::Start");

            _startSymbol = symbol;
            _startBid = bid;
            _startAsk = ask;

            _mtServer.Start();
        }

        public void Stop()
        {
            Logger.Instance.Write("FastMtQuoteServer::Init");

            _mtServer.Stop();
        }

        public void SendQuote(string symbol, double bid, double ask)
        {
            Logger.Instance.Write("FastMtQuoteServer::SendQuote");

            _mtServer.SendNewQuote(symbol, bid, ask);
        }

        void  _mtServer_ConnectionStatusUpdated(object sender, ConnectionStatusType connectionStatus)
        {
            Logger.Instance.Write("FastMtQuoteServer::_mtServer_ConnectionStatusUpdated: status = " + connectionStatus.ToString());

            if (connectionStatus == ConnectionStatusType.Connected && _mtServer != null)
            {
                try
                {
                    SendQuote(_startSymbol, _startBid, _startAsk);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Write("FastMtQuoteServer::_mtServer_ConnectionStatusUpdated: Error = " + ex.Message);
                }
            }
        }

        private QuoteServer _mtServer;
        private string _startSymbol = string.Empty;
        private double _startBid;
        private double _startAsk;
}