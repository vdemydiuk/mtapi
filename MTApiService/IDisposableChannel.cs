using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace MTApiService
{
    sealed class DisposableChannel<T> : IDisposable
    {
        T proxy;
        bool disposed;

        public DisposableChannel(T proxy)
        {
            if (!(proxy is ICommunicationObject)) throw new ArgumentException("object of type ICommunicationObject expected", "proxy");

            this.proxy = proxy;
        }

        public T Service
        {
            get
            {
                if (disposed) throw new ObjectDisposedException("DisposableProxy");

                return proxy;
            }
        }

        public void Dispose()
        {
            if (!disposed)
            {
                Dispose(true);
            }

            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (proxy != null)
                {
                    ICommunicationObject ico = null;

                    if (proxy is ICommunicationObject)
                        ico = (ICommunicationObject)proxy;

                    // This state may change after the test and there's no known way to synchronize
                    // so that's why we just give it our best shot
                    if (ico.State == CommunicationState.Faulted)
                        ico.Abort(); // Known to be faulted
                    else
                        try
                        {
                            ico.Close(); // Attempt to close, this is the nice way and we ought to be nice
                        }
                        catch
                        {
                            ico.Abort(); // Sometimes being nice isn't an option
                        }

                    proxy = default(T);
                }
            }

            disposed = true;
        }
    }
}
