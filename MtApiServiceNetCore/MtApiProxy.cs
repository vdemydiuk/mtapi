using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace MTApiService
{
    internal class MtApiProxy : IMtApi, IDisposable
    {
        private IMtApi InnerChannel;

        public CommunicationState State => ((ICommunicationObject)InnerChannel).State;

        public MtApiProxy(InstanceContext callbackContext, Binding binding, EndpointAddress remoteAddress)
        {
            var channel = new DuplexChannelFactory<IMtApi>(callbackContext, binding, remoteAddress);
            channel.Faulted += InnerDuplexChannel_Faulted;

            // configure endpoint programmatically instead via an attribute which will lead to a PlatformNotSupportedException
            (channel.Endpoint.EndpointBehaviors.Single(b => b is CallbackBehaviorAttribute) as CallbackBehaviorAttribute).UseSynchronizationContext = false;

            InnerChannel = channel.CreateChannel();
        }

        #region IMtApi Members

        public bool Connect()
        {
            return InnerChannel.Connect();
        }

        public void Disconnect()
        {
            InnerChannel.Disconnect();
        }

        public MtResponse SendCommand(MtCommand command)
        {
            return InnerChannel.SendCommand(command);
        }

        public List<MtQuote> GetQuotes()
        {
            return InnerChannel.GetQuotes();
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                Disconnect();
            }
            catch (Exception)
            {

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
