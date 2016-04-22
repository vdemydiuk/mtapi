using System;

namespace MtApi
{
    public class MtConnectionException: Exception        
    {
        public MtConnectionException()
            : this(null, null)
        {            
        }

        public MtConnectionException(string message)
            : this(message, null)
        {            
        }

        public MtConnectionException(string message, Exception exception)
            : base(message, exception)
        {
        }

    }
}