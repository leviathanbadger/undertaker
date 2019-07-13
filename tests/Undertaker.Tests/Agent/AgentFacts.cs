using FluentAssertions;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Undertaker.TestsCommon.Containers;
using Xunit;

namespace Undertaker.Agent
{
    public class AgentFacts
    {
        private readonly UndertakerContainer _container = new UndertakerContainer();

        [Fact]
        public void UseStorage_WhenStorageIsNull_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            IJobStorage storage = (dynamic)null;

            //Act
            Action act = () => agent.UseStorage(storage);

            //Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .Which.ParamName.Should().Be(nameof(storage));
        }

        [Fact]
        public void UseStorage_WhenAgentIsDisposed_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            IJobStorage storage = _container.Get<IJobStorage>();
            agent.Dispose();

            //Act
            Action act = () => agent.UseStorage(storage);

            //Assert
            act.Should()
               .Throw<ObjectDisposedException>()
               .Which.ObjectName.Should().Be(nameof(Agent));
        }

        [Fact]
        public void UseStorage_WhenStorageWasAlreadySpecified_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            IJobStorage storage = _container.Get<IJobStorage>();

            //Act
            agent.UseStorage(storage);
            Action act = () => agent.UseStorage(storage);

            //Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("*storage* already* specified*");
        }

        [Fact]
        public void UseActivator_WhenStorageIsNull_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            IActivator activator = (dynamic)null;

            //Act
            Action act = () => agent.UseActivator(activator);

            //Assert
            act.Should()
               .Throw<ArgumentNullException>()
               .Which.ParamName.Should().Be(nameof(activator));
        }

        [Fact]
        public void UseActivator_WhenAgentIsDisposed_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            IActivator activator = _container.Get<IActivator>();
            agent.Dispose();

            //Act
            Action act = () => agent.UseActivator(activator);

            //Assert
            act.Should()
               .Throw<ObjectDisposedException>()
               .Which.ObjectName.Should().Be(nameof(Agent));
        }

        [Fact]
        public void UseActivator_WhenStorageWasAlreadySpecified_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            IActivator activator = _container.Get<IActivator>();

            //Act
            agent.UseActivator(activator);
            Action act = () => agent.UseActivator(activator);

            //Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("*activator* already* specified*");
        }

        [Fact]
        public void UseConcurrentWorkers_WhenCountIsZero_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();

            //Act
            Action act = () => agent.UseConcurrentWorkers(0);

            //Assert
            act.Should()
               .Throw<ArgumentException>()
               .WithMessage("*at least one concurrent worker*");
        }

        [Fact]
        public void UseConcurrentWorkers_WhenCountIsGreaterThanThirty_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();

            //Act
            Action act = () => agent.UseConcurrentWorkers(31);

            //Assert
            act.Should()
               .Throw<ArgumentException>()
               .WithMessage("*cannot have more than thirty concurrent workers*");
        }

        [Fact]
        public void UseConcurrentWorkers_WhenAgentIsDisposed_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            agent.Dispose();

            //Act
            Action act = () => agent.UseConcurrentWorkers(5);

            //Assert
            act.Should()
               .Throw<ObjectDisposedException>()
               .Which.ObjectName.Should().Be(nameof(Agent));
        }

        [Fact]
        public void UseConcurrentWorkers_WhenStorageWasAlreadySpecified_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();

            //Act
            agent.UseConcurrentWorkers(5);
            Action act = () => agent.UseConcurrentWorkers(5);

            //Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("*concurrent worker count* already* specified*");
        }

        [Fact]
        public void Start_WhenAgentIsDisposed_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            agent.UseStorage(_container.Get<IJobStorage>())
                 .UseActivator(_container.Get<IActivator>())
                 .Dispose();

            //Act
            Action act = () => agent.Start();

            //Assert
            act.Should()
               .Throw<ObjectDisposedException>()
               .Which.ObjectName.Should().Be(nameof(Agent));
        }

        [Fact]
        public void Start_WhenStorageIsNull_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            agent.UseActivator(_container.Get<IActivator>());

            //Act
            Action act = () => agent.Start();

            //Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("*must specify* storage* UseStorage*");
        }

        [Fact]
        public void Start_WhenActivatorIsNull_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            agent.UseStorage(_container.Get<IJobStorage>());

            //Act
            Action act = () => agent.Start();

            //Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("*must specify* activator* UseActivator*");
        }

        [Fact]
        public void Start_ShouldStartTheAgent()
        {
            //Arrange
            var agent = new Agent();
            var storage = _container.Get<IJobStorage>();
            agent.UseStorage(storage)
                 .UseActivator(_container.Get<IActivator>());
            storage.PollForNextJobAsync().Returns(Task.FromResult<IJob>(null));

            using (agent)
            {
                //Act
                agent.Start();

                //Assert
                agent.IsRunning.Should().BeTrue();

                //Act
                agent.Stop(true);

                //Assert
                agent.IsRunning.Should().BeFalse();
            }
        }

        [Fact]
        public void UseConcurrentWorkers_WhenAgentIsStarted_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            var storage = _container.Get<IJobStorage>();
            agent.UseStorage(storage)
                 .UseActivator(_container.Get<IActivator>());
            storage.PollForNextJobAsync().Returns(Task.FromResult<IJob>(null));

            using (agent)
            {
                //Act
                agent.Start();
                Action act = () => agent.UseConcurrentWorkers(5);

                //Assert
                act.Should()
                   .Throw<NotSupportedException>()
                   .WithMessage("*number of concurrent workers cannot be changed* agent is running*");
            }
        }

        [Fact]
        public void Start_WhenAgentIsAlreadyStarted_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            var storage = _container.Get<IJobStorage>();
            agent.UseStorage(storage)
                 .UseActivator(_container.Get<IActivator>());
            storage.PollForNextJobAsync().Returns(Task.FromResult<IJob>(null));

            using (agent)
            {
                //Act
                agent.Start();
                Action act = () => agent.Start();

                //Assert
                act.Should()
                   .Throw<InvalidOperationException>()
                   .WithMessage("*agent* already running*");
            }
        }

        [Fact]
        public void Stop_WhenAgentIsDisposed_ShouldFailFast()
        {
            //Arrange
            var agent = new Agent();
            agent.Dispose();

            //Act
            Action act = () => agent.Stop(true);

            //Assert
            act.Should()
               .Throw<ObjectDisposedException>()
               .Which.ObjectName.Should().Be(nameof(Agent));
        }

        [Fact]
        public void Dispose_WhenAgentIsDisposed_ShouldDoNothing()
        {
            //Arrange
            var agent = new Agent();
            agent.Dispose();

            //Act
            Action act = () => agent.Dispose();

            //Assert
            act.Should().NotThrow();
        }
    }
}
