using System;
using System.Collections.Generic;
using log4net;

namespace MTApiService
{
    public class MtServerInstance
    {
        #region Fields
        private const string LogProfileName = "MtApiService";

        private static readonly ILog Log = LogManager.GetLogger(typeof(MtServerInstance));
        private static readonly MtServerInstance Instance = new MtServerInstance();

        private readonly Dictionary<int, MtServer> _servers = new Dictionary<int, MtServer>();
        private readonly Dictionary<int, MtExpert> _experts = new Dictionary<int, MtExpert>();
        #endregion

        #region Init Instance

        private MtServerInstance()
        {
            LogConfigurator.Setup(LogProfileName);
        }
        
        static MtServerInstance() 
        {
        }

        public static MtServerInstance GetInstance()
        {
            return Instance;
        }

        #endregion


        #region Public Methods
        public void InitExpert(int expertHandle, int port, string symbol, double bid, double ask, IMetaTraderHandler mtHandler)
        {
            if (mtHandler == null)
                throw new ArgumentNullException(nameof(mtHandler));

            Log.InfoFormat("InitExpert: begin. symbol = {0}, expertHandle = {1}, port = {2}", symbol, expertHandle, port);

            MtServer server;
            lock (_servers)
            {
                if (_servers.ContainsKey(port))
                {
                    server = _servers[port];
                }
                else
                {
                    server = new MtServer(port);
                    server.Stopped += server_Stopped;
                    _servers[port] = server;

                    server.Start();
                }
            }

            var expert = new MtExpert(expertHandle, new MtQuote(symbol, bid, ask), mtHandler);

            lock (_experts)
            {
                _experts[expert.Handle] = expert;
            }

            server.AddExpert(expert);

            Log.Info("InitExpert: end");
        }

        public void DeinitExpert(int expertHandle)
        {
            Log.InfoFormat("DeinitExpert: begin. expertHandle = {0}", expertHandle);

            MtExpert expert = null;

            lock (_experts)
            {
                if (_experts.ContainsKey(expertHandle))
                {
                    expert = _experts[expertHandle];
                    _experts.Remove(expertHandle);
                }
            }

            if (expert != null)
            {
                expert.Deinit();
            }
            else
            {
                Log.WarnFormat("DeinitExpert: expert with id {0} has not been found.", expertHandle);
            }

            Log.Info("DeinitExpert: end");
        }

        public void SendQuote(int expertHandle, string symbol, double bid, double ask)
        {
            Log.DebugFormat("SendQuote: begin. symbol = {0}, bid = {1}, ask = {2}", symbol, bid, ask);

            MtExpert expert;
            lock (_experts)
            {
                expert = _experts[expertHandle];
            }

            if (expert != null)
            {
                expert.Quote = new MtQuote(symbol, bid, ask);
            }
            else
            {
                Log.WarnFormat("SendQuote: expert with id {0} has not been found.", expertHandle);
            }

            Log.Debug("SendQuote: end");
        }

        public void SendEvent(int expertHandle, int eventType, string payload)
        {
            Log.DebugFormat("SendEvent: begin. eventType = {0}, payload = {1}", eventType, payload);

            MtExpert expert;
            lock (_experts)
            {
                expert = _experts[expertHandle];
            }

            if (expert != null)
            {
                expert.SendEvent(new MtEvent(eventType, payload));
            }
            else
            {
                Log.WarnFormat("SendEvent: expert with id {0} has not been found.", expertHandle);
            }

            Log.Debug("SendEvent: end");
        }

        public void SendResponse(int expertHandle, MtResponse response)
        {
            Log.DebugFormat("SendResponse: begin. id = {0}, response = {1}", expertHandle, response);

            MtExpert expert;
            lock (_experts)
            {
                expert = _experts[expertHandle];
            }

            if (expert != null)
            {
                expert.SendResponse(response);
            }
            else
            {
                Log.WarnFormat("SendResponse: expert with id {0} has not been found.", expertHandle);
            }

            Log.Debug("SendResponse: end");
        }

        public int GetCommandType(int expertHandle)
        {
            Log.DebugFormat("GetCommandType: begin. expertHandle = {0}", expertHandle);

            MtExpert expert;
            lock (_experts)
            {
                expert = _experts[expertHandle];
            }

            if (expert == null)
            {
                Log.WarnFormat("GetCommandType: expert with id {0} has not been found.", expertHandle);
            }

            var retval = expert?.GetCommandType() ?? 0;

            Log.DebugFormat("GetCommandType: end. retval = {0}", retval);

            return retval;
        }

        public object GetCommandParameter(int expertHandle, int index)
        {
            Log.DebugFormat("GetCommandParameter: begin. expertHandle = {0}, index = {1}", expertHandle, index);

            MtExpert expert;
            lock (_experts)
            {
                expert = _experts[expertHandle];
            }

            if (expert == null)
            {
                Log.WarnFormat("GetCommandParameter: expert with id {0} has not been found.", expertHandle);
            }

            var retval = expert?.GetCommandParameter(index);

            Log.DebugFormat("GetCommandParameter: end. retval = {0}", retval);

            return retval;
        }
        #endregion

        #region Private Methods
        private void server_Stopped(object sender, EventArgs e)
        {
            var server = (MtServer)sender;
            server.Stopped -= server_Stopped;

            var port = server.Port;

            Log.InfoFormat("server_Stopped: port = {0}", port);

            lock (_servers)
            {
                if (_servers.ContainsKey(port))
                {
                    _servers.Remove(port);
                }
            }
        }
        #endregion
    }
}
