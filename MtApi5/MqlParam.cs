namespace MtApi5
{
    public class MqlParam
    {
        public ENUM_DATATYPE DataType { get; set; }
        public long IntegerValue { get; set; } = 0;
        public double DoubleValue { get; set; } = 0.0;
        public string StringValue { get; set; } = String.Empty;
    }
}
