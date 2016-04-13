namespace MTApiService
{
    public interface ICommandManager
    {
        void EnqueueCommandTask(MtCommandTask task);
        MtCommandTask DequeueCommandTask();
    }
}
