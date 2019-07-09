using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public async Task CreateJobAsync_WhenStorageIsDisposed_ShouldFailFast()
        {
            //Arrange
            var jobName = "BillyBobJoe";
            var jobDesc = "Description goes here.";
            var parameters = Array.Empty<ParameterDefinition>();
            var runAt = DateTime.UtcNow.AddMinutes(5);
            var runAfter = Array.Empty<IJob>();

            var jobDefinition = new JobDefinition(jobName, jobDesc, "", "", false, parameters, runAt, runAfter);

            var jobStorage = _container.Get<InMemoryJobStorage>();
            jobStorage.Dispose();

            //Act
            Func<Task> act = () => jobStorage.CreateJobAsync(jobDefinition);

            //Assert
            var exception = await act.Should().ThrowAsync<ObjectDisposedException>();
            exception.Which.ObjectName.Should().Be(nameof(InMemoryJobStorage));
        }

        [Fact]
        public async Task CreateJobAsync_WhenRunAfterJobIsNotFromStorage_ShouldFailFast()
        {
            //Arrange
            var jobName = "BillyBobJoe";
            var jobDesc = "Description goes here.";
            var parameters = Array.Empty<ParameterDefinition>();
            var runAt = DateTime.UtcNow.AddMinutes(5);
            var runAfter = new IJob[] { new JobMock("one") };

            var jobDefinition = new JobDefinition(jobName, jobDesc, "", "", false, parameters, runAt, runAfter);

            var jobStorage = _container.Get<InMemoryJobStorage>();

            //Act
            Func<Task> act = () => jobStorage.CreateJobAsync(jobDefinition);

            //Assert
            var exception = await act.Should().ThrowAsync<KeyNotFoundException>();
            exception.WithMessage("*Job* was created using a different job scheduler or storage*");
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
            job.Status.Should().Be(JobStatus.Scheduled);
        }

        [Fact]
        public async Task PollForNextJobAsync_WhenStorageIsDisposed_ShouldFailFast()
        {
            //Arrange
            var jobStorage = _container.Get<InMemoryJobStorage>();
            jobStorage.Dispose();

            //Act
            Func<Task> act = () => jobStorage.PollForNextJobAsync();

            //Assert
            var exception = await act.Should().ThrowAsync<ObjectDisposedException>();
            exception.Which.ObjectName.Should().Be(nameof(InMemoryJobStorage));
        }

        [Fact]
        public async Task PollForNextJobAsync_WhenThereAreNoScheduledJobs_ShouldReturnNull()
        {
            //Arrange
            var jobStorage = _container.Get<InMemoryJobStorage>();

            //Act
            var nextJob = await jobStorage.PollForNextJobAsync();

            //Assert
            nextJob.Should().BeNull();
        }

        [Fact]
        public async Task PollForNextJobAsync_WhenScheduledJobsAreAllInFuture_ShouldReturnNull()
        {
            //Arrange
            var jobName = "BillyBobJoe";
            var jobDesc = "Description goes here.";
            var parameters = Array.Empty<ParameterDefinition>();
            var runAt = DateTime.UtcNow.AddMinutes(5);
            var runAfter = Array.Empty<IJob>();

            var jobDefinition = new JobDefinition(jobName, jobDesc, "", "", false, parameters, runAt, runAfter);

            var jobStorage = _container.Get<InMemoryJobStorage>();

            await jobStorage.CreateJobAsync(jobDefinition);

            //Act
            var nextJob = await jobStorage.PollForNextJobAsync();

            //Assert
            nextJob.Should().BeNull();
        }

        [Fact]
        public async Task PollForNextJobAsync_WhenScheduledJobIsInPast_ShouldReturnJob()
        {
            //Arrange
            var jobName = "BillyBobJoe";
            var jobDesc = "Description goes here.";
            var parameters = Array.Empty<ParameterDefinition>();
            var runAt = DateTime.UtcNow.AddMinutes(-5);
            var runAfter = Array.Empty<IJob>();

            var jobDefinition = new JobDefinition(jobName, jobDesc, "", "", false, parameters, runAt, runAfter);

            var jobStorage = _container.Get<InMemoryJobStorage>();

            var expectedJob = await jobStorage.CreateJobAsync(jobDefinition);

            //Act
            var nextJob = await jobStorage.PollForNextJobAsync();

            //Assert
            nextJob.Should().NotBeNull();
            nextJob.Should().Be(expectedJob);
            Debug.Assert(nextJob != null, nameof(nextJob) + " != null");
            nextJob.Status.Should().Be(JobStatus.Processing);

            //Act
            nextJob = await jobStorage.PollForNextJobAsync();

            //Assert
            nextJob.Should().BeNull();
        }
    }
}
