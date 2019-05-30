using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using System;
using System.Linq;
using Xunit;

namespace Undertaker
{
    public class JobBuilderFacts
    {
        [Fact]
        public void AfterDateTime_WhenRunAtHasNotBeenSelected_ShouldSetRunAtTime()
        {
            //Arrange
            var scheduler = Substitute.For<IJobScheduler>();
            JobDefinition jobDefinition = null;
            scheduler.ScheduleJob(Arg.Do<JobDefinition>(jobDef => jobDefinition = jobDef));
            var builder = new JobBuilder(scheduler);

            var runAt = DateTime.UtcNow.AddHours(1);

            //Act
            builder.After(runAt).Run<TestJob>(job => job.Run());

            //Assert
            scheduler.ScheduleJob(Arg.Any<JobDefinition>()).Received(1);
            jobDefinition.Should().NotBeNull();
            jobDefinition.RunAtTime.Should().Be(runAt);
            jobDefinition.RunAfterJobs.Should().BeEmpty();
        }

        [Fact]
        public void AfterDateTime_WhenRunAtHasBeenSelected_ShouldFailFast()
        {
            //Arrange
            var scheduler = Substitute.For<IJobScheduler>();
            var builder = new JobBuilder(scheduler).After(DateTime.UtcNow.AddHours(1));

            //Act
            Action act = () => builder.After(DateTime.UtcNow.AddHours(2));

            //Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("*run-after time has already been specified*");
        }

        [Fact]
        public void AfterJob_ShouldAddJobAsPrerequisite()
        {
            //Arrange
            var scheduler = Substitute.For<IJobScheduler>();
            JobDefinition jobDefinition = null;
            scheduler.ScheduleJob(Arg.Do<JobDefinition>(jobDef => jobDefinition = jobDef));
            var builder = new JobBuilder(scheduler);

            var afterJob = Substitute.For<IJob>();

            //Act
            builder.After(afterJob).Run<TestJob>(job => job.Run());

            //Assert
            scheduler.ScheduleJob(Arg.Any<JobDefinition>()).Received(1);
            jobDefinition.Should().NotBeNull();
            jobDefinition.RunAtTime.Should().Be(null);
            jobDefinition.RunAfterJobs.Count.Should().Be(1);
            jobDefinition.RunAfterJobs.First().Should().Be(afterJob);
        }

        [Fact]
        public void AfterJob_WhenInvokedMultipleTimes_ShouldAddMultipleJobsAsPrerequisites()
        {
            //Arrange
            var scheduler = Substitute.For<IJobScheduler>();
            JobDefinition jobDefinition = null;
            scheduler.ScheduleJob(Arg.Do<JobDefinition>(jobDef => jobDefinition = jobDef));
            var builder = (IJobBuilder)new JobBuilder(scheduler);

            var afterJobs = Enumerable.Range(0, 5)
                                      .Select(x => Substitute.For<IJob>())
                                      .ToList();

            //Act
            foreach (var afterJob in afterJobs)
            {
                builder = builder.After(afterJob);
            }
            builder.Run<TestJob>(job => job.Run());

            //Assert
            scheduler.ScheduleJob(Arg.Any<JobDefinition>()).Received(1);
            jobDefinition.Should().NotBeNull();
            jobDefinition.RunAtTime.Should().Be(null);
            jobDefinition.RunAfterJobs.Should().BeEquivalentTo(afterJobs);
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
