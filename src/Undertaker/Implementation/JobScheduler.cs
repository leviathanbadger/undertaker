using System;

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
            throw new NotImplementedException();
        }

        public IJob ScheduleJob(JobDefinition jobDefinition)
        {
            throw new NotImplementedException();
        }
    }
}
