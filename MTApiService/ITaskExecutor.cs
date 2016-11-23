namespace MTApiService
{
    internal interface ITaskExecutor
    {
        void Execute(MtCommandTask task);
        
        int Handle { get; }
    }
}
