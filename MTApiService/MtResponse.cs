using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Collections;

namespace MTApiService
{
    [DataContract]
    [KnownType("GetKnownTypes")]
    public abstract class MtResponse
    {
        static IEnumerable<Type> GetKnownTypes()
        {
            return new Type[] { typeof(MtResponseInt), typeof(MtResponseDouble),
                                typeof(MtResponseString), typeof(MtResponseBool),
                                typeof(MtResponseLong), typeof(MtResponseULong),
                                typeof(MtResponseDoubleArray), typeof(MtResponseIntArray),
                                typeof(MtResponseLongArray), typeof(MtResponseMqlTick),
                                typeof(MtResponseArrayList), typeof(MtResponseMqlRatesArray),
                                typeof(MtResponseMqlBookInfoArray)};
        }
    }

    [DataContract]
    public class MtResponseInt: MtResponse
    {
        public MtResponseInt(int value)
        {
            Value = value;
        }

        [DataMember]
        public int Value { get; private set; }
    }

    [DataContract]
    public class MtResponseLong : MtResponse
    {
        public MtResponseLong(long value)
        {
            Value = value;
        }

        [DataMember]
        public long Value { get; private set; }
    }

    [DataContract]
    public class MtResponseULong : MtResponse
    {
        public MtResponseULong(ulong value)
        {
            Value = value;
        }

        [DataMember]
        public ulong Value { get; private set; }
    }

    [DataContract]
    public class MtResponseDouble : MtResponse
    {
        public MtResponseDouble(double value)
        {
            Value = value;
        }

        [DataMember]
        public double Value { get; private set; }
    }

    [DataContract]
    public class MtResponseString : MtResponse
    {
        public MtResponseString(string value)
        {
            Value = value;
        }

        [DataMember]
        public string Value { get; private set; }
    }

    [DataContract]
    public class MtResponseBool : MtResponse
    {
        public MtResponseBool(bool value)
        {
            Value = value;
        }

        [DataMember]
        public bool Value { get; private set; }
    }

    [DataContract]
    public class MtResponseDoubleArray : MtResponse
    {
        public MtResponseDoubleArray(double[] value)
        {
            Value = value;
        }

        [DataMember]
        public double[] Value { get; private set; }
    }

    [DataContract]
    public class MtResponseIntArray : MtResponse
    {
        public MtResponseIntArray(int[] value)
        {
            Value = value;
        }

        [DataMember]
        public int[] Value { get; private set; }
    }

    [DataContract]
    public class MtResponseLongArray : MtResponse
    {
        public MtResponseLongArray(long[] value)
        {
            Value = value;
        }

        [DataMember]
        public long[] Value { get; private set; }
    }

    [DataContract]
    public class MtResponseArrayList : MtResponse
    {
        public MtResponseArrayList(ArrayList value)
        {
            Value = value;
        }

        [DataMember]
        public ArrayList Value { get; private set; }
    }

    [DataContract]
    public class MtResponseMqlRatesArray : MtResponse
    {
        public MtResponseMqlRatesArray(MtMqlRates[] value)
        {
            Value = value;
        }

        [DataMember]
        public MtMqlRates[] Value { get; private set; }
    }

    [DataContract]
    public class MtResponseMqlTick : MtResponse
    {
        public MtResponseMqlTick(MtMqlTick value)
        {
            Value = value;
        }

        [DataMember]
        public MtMqlTick Value { get; private set; }
    }

    [DataContract]
    public class MtResponseMqlBookInfoArray : MtResponse
    {
        public MtResponseMqlBookInfoArray(MtMqlBookInfo[] value)
        {
            Value = value;
        }

        [DataMember]
        public MtMqlBookInfo[] Value { get; private set; }
    }
    
}
