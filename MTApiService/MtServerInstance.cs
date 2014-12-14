using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Net;
using System.Diagnostics;

namespace MTApiService
{
    public class MtServerInstance
    {
        #region Init_Instance

        static readonly MtServerInstance mInstance = new MtServerInstance();

        private MtServerInstance()
        {            
        }
        
        static MtServerInstance() 
        { 
        }  

        public static MtServerInstance GetInstance()
        {
            return mInstance;
        }

        #endregion


        #region Public Methods
        public void InitExpert(int expertHandle, int port, string symbol, double bid, double ask, IMetaTraderHandler mtHandler)
        {
            Debug.WriteLine("MtApiServerInstance::InitExpert: symbol = {0}, expertHandle = {1}, port = {2}", symbol, expertHandle, port);

            MtServer server = null;
            lock (mServersDictionary)
            {
                if (mServersDictionary.ContainsKey(port))
                {
                    server = mServersDictionary[port];
                }
                else
                {
                    server = new MtServer(port);
                    server.Stopped += new EventHandler(server_Stopped);
                    mServersDictionary[port] = server;

                    server.Start();
                }
            }

            var expert = new MtExpert(expertHandle, new MtQuote(symbol, bid, ask), mtHandler);

            lock (mExpertsDictionary)
            {
                mExpertsDictionary[expert.Handle] = expert;
            }

            server.AddExpert(expert);
        }

        public void DeinitExpert(int expertHandle)
        {
            Debug.WriteLine("MtApiServerInstance::DeinitExpert: expertHandle = {0}", expertHandle);

            MtExpert expert = null;

            lock (mExpertsDictionary)
            {
                if (mExpertsDictionary.ContainsKey(expertHandle) == true)
                {
                    expert = mExpertsDictionary[expertHandle];
                    mExpertsDictionary.Remove(expertHandle);
                }
            }

            if (expert != null)
            {
                expert.Deinit();
            }            
        }

        public void SendQuote(int expertHandle, string symbol, double bid, double ask)
        {
            Debug.WriteLine("MtApiServerInstance::SendQuote: enter. symbol = {0}, bid = {1}, ask = {2}", symbol, bid, ask);

            MtExpert expert = null;
            lock (mExpertsDictionary)
            {
                expert = mExpertsDictionary[expertHandle];
            }

            if (expert != null)
            {
                expert.Quote = new MtQuote(symbol, bid, ask);
            }

            Debug.WriteLine("MtApiServerInstance::SendQuote: finish.");
        }

        public void SendResponse(int expertHandle, MtResponse response)
        {
            Debug.WriteLine("MtApiServerInstance::SendResponse: id = {0}, response = {1}", expertHandle, response);

            MtExpert expert = null;
            lock (mExpertsDictionary)
            {
                expert = mExpertsDictionary[expertHandle];
            }

            if (expert != null)
            {
                expert.SendResponse(response);
            }

            Debug.WriteLine("MtApiServerInstance::SendResponse: finish");
        }

        public int GetCommandType(int expertHandle)
        {
            Debug.WriteLine("MtApiServerInstance::GetCommandType: expertHandle = {0}", expertHandle);

            MtExpert expert = null;
            lock (mExpertsDictionary)
            {
                expert = mExpertsDictionary[expertHandle];
            }

            return (expert != null) ? expert.GetCommandType() : 0;
        }

        public object GetCommandParameter(int expertHandle, int index)
        {
            Debug.WriteLine("MtApiServerInstance::GetCommandParameter: expertHandle = {0}, index = {1}", expertHandle, index);

            MtExpert expert = null;
            lock (mExpertsDictionary)
            {
                expert = mExpertsDictionary[expertHandle];
            }

            return (expert != null) ? expert.GetCommandParameter(index) : null;
        }
        #endregion

        #region Private Methods
        private void server_Stopped(object sender, EventArgs e)
        {
            MtServer server = (MtServer)sender;
            server.Stopped -= server_Stopped;

            var port = server.Port;
            lock (mServersDictionary)
            {
                if (mServersDictionary.ContainsKey(port))
                {
                    mServersDictionary.Remove(port);
                }
            }
        }
        #endregion

        #region Fields
        private readonly MtRegistryManager mConnectionManager = new MtRegistryManager();
        private readonly Dictionary<int, MtServer> mServersDictionary = new Dictionary<int, MtServer>();
        private readonly Dictionary<int, MtExpert> mExpertsDictionary = new Dictionary<int, MtExpert>();
        #endregion
    }
}
