using System.Threading.Tasks;

namespace Undertaker
{
    public class JobScheduler : IJobScheduler
    {
        private readonly IJobStorage _storage;

        public JobScheduler(IJobStorage storage)
        {
            _storage = storage;
        }

        public IJobBuilder BuildJob()
        {
            return new JobBuilder(this);
        }

        public async Task<IJob> ScheduleJobAsync(JobDefinition jobDefinition)
        {
            return await _storage.CreateJobAsync(jobDefinition);
        }
    }
}
