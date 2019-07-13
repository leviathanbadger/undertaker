using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using System.Threading.Tasks;
using Undertaker.TestsCommon.Containers;
using Xunit;

namespace Undertaker
{
    public class JobSchedulerFacts
    {
        private readonly UndertakerContainer _container = new UndertakerContainer();

        [Fact]
        public async Task BuildJob_ShouldCreateJob()
        {
            //Arrange
            var storage = _container.Get<IJobStorage>();
            var scheduler = new JobScheduler(storage);

            //Act
            var job = await scheduler.BuildJob()
                                     .WithName("RunTestJob")
                                     .EnqueueAsync<TestJob>(j => j.Run());

            //Assert
            job.Should().NotBeNull();
            await storage.Received().CreateJobAsync(Arg.Any<JobDefinition>());
        }

        [UsedImplicitly]
        private class TestJob
        {
            public void Run()
            {
            }
        }
    }
}
