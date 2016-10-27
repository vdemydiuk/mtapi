using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MTApiService
{
    internal class MtApiProxy : DuplexClientBase<IMtApi>, IMtApi, IDisposable
    {
        public MtApiProxy(InstanceContext callbackContext, Binding binding, EndpointAddress remoteAddress)
            : base(callbackContext, binding, remoteAddress)
        {
            InnerDuplexChannel.Faulted += InnerDuplexChannel_Faulted;
            InnerDuplexChannel.Open();
        }

        #region IMtApi Members

        public bool Connect()
        {
            return Channel.Connect();
        }

        public void Disconnect()
        {
            Channel.Disconnect();
        }

        public MtResponse SendCommand(MtCommand command)
        {
            return Channel.SendCommand(command);
        }

        public List<MtQuote> GetQuotes()
        {
            return Channel.GetQuotes();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch (CommunicationException)
            {
                Abort();
            }
            catch (TimeoutException)
            {
                Abort();
            }
            catch (Exception)
            {
                Abort();
            }
        }

        #endregion

        #region Private Methods
        private void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        {
            Faulted?.Invoke(this, e);
        }

        #endregion

        #region Events
        public event EventHandler Faulted;
        #endregion
    }
}
