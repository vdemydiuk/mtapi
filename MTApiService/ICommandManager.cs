namespace MTApiService
{
    internal interface ICommandManager
    {
        MtCommandTask SendCommand(MtCommand task);
    }
}
