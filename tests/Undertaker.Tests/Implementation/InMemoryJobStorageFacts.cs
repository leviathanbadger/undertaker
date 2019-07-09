using FluentAssertions;
using System;
using System.Threading.Tasks;
using Undertaker.TestsCommon.Containers;
using Xunit;

namespace Undertaker
{
    public class InMemoryJobStorageFacts
    {
        private readonly UndertakerContainer _container = new UndertakerContainer();

        [Fact]
        public async Task CreateJob_ShouldWork()
        {
            //Arrange
            var jobName = "BillyBobJoe";
            var jobDesc = "Description goes here.";
            var parameters = Array.Empty<ParameterDefinition>();
            var runAt = DateTime.UtcNow.AddMinutes(5);
            var runAfter = Array.Empty<IJob>();

            var jobDefinition = new JobDefinition(jobName, jobDesc, "", "", false, parameters, runAt, runAfter);

            var jobStorage = _container.Get<InMemoryJobStorage>();

            //Act
            var job = await jobStorage.CreateJobAsync(jobDefinition);

            //Assert
            job.Should().NotBeNull();
            job.Storage.Should().Be(jobStorage);
            job.Name.Should().Be(jobName);
            job.Description.Should().Be(jobDesc);
        }
    }
}
