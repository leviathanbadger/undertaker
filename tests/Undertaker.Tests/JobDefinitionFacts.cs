using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Xunit;

namespace Undertaker
{
    public class JobDefinitionFacts
    {
        [Fact]
        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public void Ctor_ShouldSetProperties()
        {
            //Arrange
            const string name = "fish";
            const string desc = "fishy";
            const string fullyQualifiedTypeName = "Undertaker.Fish; some assembly required";
            const string methodName = "Swim";
            const bool isStatic = true;
            var parameters = new[]
            {
                new ParameterDefinition("type1", "25"),
                new ParameterDefinition("type2", "'c'"),
                new ParameterDefinition("type3", "\"str\"")
            };
            DateTime? runAt = DateTime.UtcNow.AddHours(2);
            var runAfter = new IJob[]
            {
                new JobMock("job1"),
                new JobMock("job2"),
                new JobMock("job3")
            };

            //Act
            var result = new JobDefinition(name, desc, fullyQualifiedTypeName, methodName, isStatic, parameters, runAt, runAfter);

            //Assert
            result.Name.Should().Be(name);
            result.Description.Should().Be(desc);
            result.FullyQualifiedTypeName.Should().Be(fullyQualifiedTypeName);
            result.MethodName.Should().Be(methodName);
            result.IsMethodStatic.Should().Be(isStatic);
            result.Parameters.Should().BeEquivalentTo(parameters);
            result.RunAtTime.Should().Be(runAt);
            result.RunAfterJobs.Should().BeEquivalentTo(runAfter);
        }

        private class JobMock : IJob
        {
            public JobMock(string discriminator)
            {
                Discriminator = discriminator;
            }

            [UsedImplicitly]
            public string Discriminator { get; }
        }
    }
}
