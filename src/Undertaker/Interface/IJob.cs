namespace Undertaker
{
    public interface IJob
    {
        IJobStorage Storage { get; }

        string Name { get; }
        string Description { get; }
    }
}
