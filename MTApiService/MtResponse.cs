using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections;
using System.Globalization;

namespace MTApiService
{
    [DataContract]
    [KnownType("GetKnownTypes")]
    public abstract class MtResponse
    {
        static IEnumerable<Type> GetKnownTypes()
        {
            return new Type[] { typeof(MtResponseObject),
                                typeof(MtResponseInt), typeof(MtResponseDouble),
                                typeof(MtResponseString), typeof(MtResponseBool),
                                typeof(MtResponseLong), typeof(MtResponseULong),
                                typeof(MtResponseDoubleArray), typeof(MtResponseIntArray),
                                typeof(MtResponseLongArray), typeof(MtResponseMqlTick),
                                typeof(MtResponseArrayList), typeof(MtResponseMqlRatesArray),
                                typeof(MtResponseMqlBookInfoArray)};
        }

        public abstract object GetValue();

        [DataMember]
        public int ErrorCode { get; set; }
    }

    [DataContract]
    public class MtResponseObject : MtResponse
    {
        public MtResponseObject(object value)
        {
            Value = value;
        }

        [DataMember]
        public object Value { get; private set; }

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString();
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString();
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString();
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.CurrentCulture);
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value;
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString();
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString();
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString();
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString();
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString();
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString();
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString();
        }
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

        public override object GetValue() { return Value; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
    
}
