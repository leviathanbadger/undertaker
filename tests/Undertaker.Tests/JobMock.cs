using JetBrains.Annotations;

namespace Undertaker
{
    public class JobMock : IJob
    {
        public JobMock(string discriminator)
        {
            Discriminator = discriminator;
        }

        public IJobStorage Storage
        {
            get
            {
                return null;
            }
        }

        public string Name
        {
            get
            {
                return "JobMock";
            }
        }
        public string Description
        {
            get
            {
                return "A mock job.";
            }
        }

        public JobStatus Status
        {
            get
            {
                return JobStatus.Scheduled;
            }
        }

        [UsedImplicitly]
        public string Discriminator { get; }
    }
}
