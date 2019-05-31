using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;
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

        [Fact]
        public void Run_WithStaticExpression_ShouldWork()
        {
            //Arrange
            var scheduler = Substitute.For<IJobScheduler>();
            JobDefinition jobDefinition = null;
            scheduler.ScheduleJob(Arg.Do<JobDefinition>(jobDef => jobDefinition = jobDef));
            var builder = (IJobBuilder)new JobBuilder(scheduler);

            //Act
            builder.Run(() => TestJob.RunStatic());

            //Assert
            scheduler.ScheduleJob(Arg.Any<JobDefinition>()).Received(1);
            jobDefinition.Should().NotBeNull();
            jobDefinition.IsMethodStatic.Should().BeTrue();
            jobDefinition.MethodName.Should().Be(nameof(TestJob.RunStatic));
            jobDefinition.FullyQualifiedTypeName.Should().Be(typeof(TestJob).AssemblyQualifiedName);
        }

        [Fact]
        public void Run_WithMemberExpression_ShouldWork()
        {
            //Arrange
            var scheduler = Substitute.For<IJobScheduler>();
            JobDefinition jobDefinition = null;
            scheduler.ScheduleJob(Arg.Do<JobDefinition>(jobDef => jobDefinition = jobDef));
            var builder = (IJobBuilder)new JobBuilder(scheduler);

            //Act
            builder.Run<TestJob>(job => job.Run());

            //Assert
            scheduler.ScheduleJob(Arg.Any<JobDefinition>()).Received(1);
            jobDefinition.Should().NotBeNull();
            jobDefinition.IsMethodStatic.Should().BeFalse();
            jobDefinition.MethodName.Should().Be(nameof(TestJob.Run));
            jobDefinition.FullyQualifiedTypeName.Should().Be(typeof(TestJob).AssemblyQualifiedName);
        }

        [Fact]
        public void Run_WithStaticAsyncExpression_ShouldWork()
        {
            //Arrange
            var scheduler = Substitute.For<IJobScheduler>();
            JobDefinition jobDefinition = null;
            scheduler.ScheduleJob(Arg.Do<JobDefinition>(jobDef => jobDefinition = jobDef));
            var builder = (IJobBuilder)new JobBuilder(scheduler);

            //Act
            builder.Run(() => TestJob.RunStaticAsync());

            //Assert
            scheduler.ScheduleJob(Arg.Any<JobDefinition>()).Received(1);
            jobDefinition.Should().NotBeNull();
            jobDefinition.IsMethodStatic.Should().BeTrue();
            jobDefinition.MethodName.Should().Be(nameof(TestJob.RunStaticAsync));
            jobDefinition.FullyQualifiedTypeName.Should().Be(typeof(TestJob).AssemblyQualifiedName);
        }

        [Fact]
        public void Run_WithMemberAsyncExpression_ShouldWork()
        {
            //Arrange
            var scheduler = Substitute.For<IJobScheduler>();
            JobDefinition jobDefinition = null;
            scheduler.ScheduleJob(Arg.Do<JobDefinition>(jobDef => jobDefinition = jobDef));
            var builder = (IJobBuilder)new JobBuilder(scheduler);

            //Act
            builder.Run<TestJob>(job => job.RunAsync());

            //Assert
            scheduler.ScheduleJob(Arg.Any<JobDefinition>()).Received(1);
            jobDefinition.Should().NotBeNull();
            jobDefinition.IsMethodStatic.Should().BeFalse();
            jobDefinition.MethodName.Should().Be(nameof(TestJob.RunAsync));
            jobDefinition.FullyQualifiedTypeName.Should().Be(typeof(TestJob).AssemblyQualifiedName);
        }

        [Fact]
        public void Run_WithNonMethodCallExpression_ShouldFailFast()
        {
            //Arrange
            var scheduler = Substitute.For<IJobScheduler>();
            var builder = (IJobBuilder)new JobBuilder(scheduler);

            //Act
            Action act = () => builder.Run<TestJob>(job => Task.CompletedTask);

            //Assert
            act.Should()
               .Throw<ArgumentException>()
               .WithMessage("*must be a method call expression*");
        }

        [Fact]
        public void Run_WhenMemberMethodObjectIsNotParameter_ShouldFailFast()
        {
            //Arrange
            var scheduler = Substitute.For<IJobScheduler>();
            var builder = (IJobBuilder)new JobBuilder(scheduler);
            var otherJob = new TestJob();

            //Act
            Action act = () => builder.Run<TestJob>(job => otherJob.Run());

            //Assert
            act.Should()
               .Throw<ArgumentException>()
               .WithMessage("An activation class was selected, but was not used as the left-hand side of the method call expression.");
        }

        [Fact]
        public void Run_WithoutParameter_WhenExpressionHasMethodObject_ShouldFailFast()
        {
            //Arrange
            var scheduler = Substitute.For<IJobScheduler>();
            var builder = (IJobBuilder)new JobBuilder(scheduler);
            var otherJob = new TestJob();

            //Act
            Action act = () => builder.Run(() => otherJob.Run());

            //Assert
            act.Should()
               .Throw<ArgumentException>()
               .WithMessage("No activation class selected, so the method call must be static.*");
        }

        [UsedImplicitly]
        private class TestJob
        {
            public static void RunStatic()
            {
            }

            public static Task RunStaticAsync()
            {
                return Task.CompletedTask;
            }

            public void Run()
            {
            }

            public Task RunAsync()
            {
                return Task.CompletedTask;
            }
        }
    }
}
