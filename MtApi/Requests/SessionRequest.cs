using System;

namespace MtApi.Requests
{
    public class SessionRequest : RequestBase
    {
        public string Symbol { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int SessionIndex { get; set; }
        public SessionType SessionType { get; set; }

        public override RequestType RequestType
        {
            get { return RequestType.Session; }
        }
    }
}