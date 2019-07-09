using FluentAssertions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        [Fact]
        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public void Ctor_WhenOriginalParameterArrayIsModified_ShouldNotReflectArrayChange()
        {
            //Arrange
            var runAfter = new IJob[]
            {
                new JobMock("job1"),
                new JobMock("job2"),
                new JobMock("job3")
            };

            //Act
            var result = new JobDefinition("fish", "fishy", "Undertaker.Fish; some assembly required", "Swim", true, new ParameterDefinition[0], DateTime.UtcNow.AddHours(2), runAfter);
            runAfter[1] = new JobMock("job4");

            //Assert
            var job = (JobMock)result.RunAfterJobs.ElementAt(1);
            job.Discriminator.Should().Be("job2");
        }

        [Fact]
        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public void Ctor_WhenOriginalRunAfterArrayIsModified_ShouldNotReflectArrayChange()
        {
            //Arrange
            var parameters = new[]
            {
                new ParameterDefinition("type1", "25"),
                new ParameterDefinition("type2", "'c'"),
                new ParameterDefinition("type3", "\"str\"")
            };

            //Act
            var result = new JobDefinition("fish", "fishy", "Undertaker.Fish; some assembly required", "Swim", true, parameters, DateTime.UtcNow.AddHours(2), new IJob[0]);
            parameters[1] = new ParameterDefinition("type4", "[]");

            //Assert
            result.Parameters.ElementAt(1).FullyQualifiedTypeName.Should().Be("type2");
        }
    }
}
