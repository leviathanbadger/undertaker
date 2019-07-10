using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Undertaker
{
    public interface IJobScheduler
    {
        [MustUseReturnValue]
        IJobBuilder BuildJob();

        Task<IJob> ScheduleJobAsync(JobDefinition jobDefinition);
    }
}
