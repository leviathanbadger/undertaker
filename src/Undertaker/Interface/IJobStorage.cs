using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Undertaker
{
    public interface IJobStorage : IDisposable
    {
        [CanBeNull]
        Task<IJob> PollForNextJobAsync();

        Task<IJob> CreateJobAsync(JobDefinition jobDefinition);
        Task UpdateJobStatusAsync(IJob job, JobStatus status);
    }
}
