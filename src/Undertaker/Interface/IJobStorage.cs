using JetBrains.Annotations;

namespace Undertaker
{
    public interface IJobStorage
    {
        [CanBeNull]
        IJob PollForNextJob();

        IJob CreateJob(JobDefinition jobDefinition);
        void UpdateJobStatus(IJob job, JobStatus status);
    }
}
