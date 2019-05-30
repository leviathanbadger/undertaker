using FluentAssertions;
using JetBrains.Annotations;
using System;
using Xunit;

namespace Undertaker
{
    public class DefaultActivatorFacts
    {
        [Fact]
        public void Activate_WhenTypeHasNoParameterlessConstructor_ShouldFailFast()
        {
            //Arrange
            var type = typeof(NoDefaultCtorClass);
            var activator = new DefaultActivator();

            //Act
            Action act = () => activator.Activate(type);

            //Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("*no public parameterless constructor*");
        }

        [Fact]
        public void Activate_WhenTypeHasPrivateParameterlessConstructor_ShouldFailFast()
        {
            //Arrange
            var type = typeof(PrivateDefaultCtorClass);
            var activator = new DefaultActivator();

            //Act
            Action act = () => activator.Activate(type);

            //Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("*no public parameterless constructor*");
        }

        [Fact]
        public void Activate_WhenTypeHasPublicParameterlessConstructor_ShouldCreateInstanceOfType()
        {
            //Arrange
            var type = typeof(DefaultCtorClass);
            var activator = new DefaultActivator();

            //Act
            var result = activator.Activate(type);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType(type);
        }

        private class NoDefaultCtorClass
        {
            // ReSharper disable once UnusedParameter.Local
            public NoDefaultCtorClass(string arbitrary)
            {
            }
        }

        private class PrivateDefaultCtorClass
        {
            private PrivateDefaultCtorClass()
            {
            }
        }

        private class DefaultCtorClass
        {
            [UsedImplicitly]
            public DefaultCtorClass()
            {
            }
        }
    }
}
