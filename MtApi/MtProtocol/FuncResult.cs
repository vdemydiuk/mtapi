namespace MtApi.MtProtocol
{
    internal class FuncResult<T>
    {
        public bool RetVal { get; set; }
        public T? Result { get; set; }
    }
}
