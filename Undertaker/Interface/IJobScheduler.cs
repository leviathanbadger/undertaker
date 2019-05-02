namespace Undertaker
{
    public interface IJobScheduler
    {
        IJobBuilder BuildJob();
    }
}
