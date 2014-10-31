using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MTApiService
{
    class MtApiProxy : DuplexClientBase<IMtApi>, IMtApi, IDisposable
    {
        public MtApiProxy(InstanceContext callbackContext, Binding binding,
                                                   EndpointAddress remoteAddress)
            : base(callbackContext, binding, remoteAddress)
        {
            base.InnerDuplexChannel.Faulted += new EventHandler(InnerDuplexChannel_Faulted);
            base.InnerDuplexChannel.Open();
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

        public IEnumerable<MtQuote> GetQuotes()
        {
            return Channel.GetQuotes();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                this.Close();
            }
            catch (CommunicationException)
            {
                this.Abort();
            }
            catch (TimeoutException)
            {
                this.Abort();
            }
            catch (Exception)
            {
                this.Abort();
            }
        }

        #endregion

        #region Private Methods
        private void InnerDuplexChannel_Faulted(object sender, EventArgs e)
        {
            if (Faulted != null)
                Faulted(this, e);
        }
        #endregion

        #region Events
        public event EventHandler Faulted;
        #endregion
    }
}
