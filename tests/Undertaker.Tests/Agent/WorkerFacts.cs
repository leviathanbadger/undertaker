using FluentAssertions;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Undertaker.TestsCommon.Containers;
using Xunit;

namespace Undertaker.Agent
{
    public class WorkerFacts
    {
        private readonly UndertakerContainer _container = new UndertakerContainer();

        [Fact]
        public void Ctor_WhenStorageIsNull_ShouldFailFast()
        {
            //Arrange
            var activator = _container.Get<IActivator>();

            //Act
            Func<object> act = () => new Worker(null, activator);

            //Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .Which.ParamName.Should().Be("storage");
        }

        [Fact]
        public void Ctor_WhenActivatorIsNull_ShouldFailFast()
        {
            //Arrange
            var storage = _container.Get<IJobStorage>();

            //Act
            Func<object> act = () => new Worker(storage, null);

            //Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .Which.ParamName.Should().Be("activator");
        }

        [Fact]
        public async Task Start_ShouldBeginPollingForJobs()
        {
            //Arrange
            var storage = _container.Get<IJobStorage>();
            var activator = _container.Get<IActivator>();
            storage.PollForNextJobAsync().Returns(Task.FromResult<IJob>(null));
            var worker = new Worker(storage, activator);

            //Act
            worker.Start();
            await Task.Delay(200);
            worker.Stop(true);

            //Assert
            await storage.Received().PollForNextJobAsync();
        }

        [Fact]
        public void Start_WhenWorkerIsAlreadyRunning_ShouldFailFast()
        {
            //Arrange
            var storage = _container.Get<IJobStorage>();
            var activator = _container.Get<IActivator>();
            storage.PollForNextJobAsync().Returns(Task.FromResult<IJob>(null));
            var worker = new Worker(storage, activator);

            try
            {
                //Act
                worker.Start();
                Action act = () => worker.Start();

                //Assert
                act.Should()
                   .Throw<InvalidOperationException>()
                   .WithMessage("*already running*");
            }
            finally
            {
                //Cleanup
                worker.Stop(true);
            }
        }

        [Fact]
        public void Start_WhenWorkerWasStopped_ShouldFailFast()
        {
            //Arrange
            var storage = _container.Get<IJobStorage>();
            var activator = _container.Get<IActivator>();
            storage.PollForNextJobAsync().Returns(Task.FromResult<IJob>(null));
            var worker = new Worker(storage, activator);

            //Act
            worker.Start();
            worker.Stop(true);
            Action act = () => worker.Start();

            //Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("*already* started and stopped once* create a new worker*");
        }
    }
}
