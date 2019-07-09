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
        public async Task CreateJobAsync_WhenJobDefinitionIsNull_ShouldFailFast()
        {
            //Arrange
            JobDefinition jobDefinition = (dynamic)null;

            var jobStorage = _container.Get<InMemoryJobStorage>();

            //Act
            Func<Task> act = () => jobStorage.CreateJobAsync(jobDefinition);

            //Assert
            var exception = await act.Should().ThrowAsync<ArgumentNullException>();
            exception.Which.ParamName.Should().Be(nameof(jobDefinition));
        }

        [Fact]
        public async Task CreateJobAsync_ShouldWork()
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
