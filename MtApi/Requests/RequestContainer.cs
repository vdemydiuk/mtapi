using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MtApi.Requests
{
    public class RequestContainer
    {
        private static readonly Dictionary<Type, RequestType> RequestTypes = new Dictionary<Type, RequestType>();

        static RequestContainer()
        {
            RequestTypes[typeof (GetOrderRequest)] = RequestType.GetOrder;
            RequestTypes[typeof(GetOrdersRequest)] = RequestType.GetOrders;
        }

        public RequestType RequestType { get; set; }
        public RequestBase Request { get; set; }

        public virtual string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static RequestContainer CreateNew<T>(T request) where T: RequestBase
        {
            if (RequestTypes.ContainsKey(request.GetType()) == false)
                throw new ArgumentException("Unknown request type");

            return new RequestContainer { RequestType = RequestTypes[request.GetType()], Request = request };
        }
    }
}