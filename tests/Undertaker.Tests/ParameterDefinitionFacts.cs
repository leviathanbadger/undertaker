using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Undertaker
{
    public class ParameterDefinitionFacts
    {
        [Fact]
        [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
        public void Ctor_ShouldSetProperties()
        {
            //Arrange
            const string fullyQualifiedTypeName = "Undertaker.Fish; some assembly required";
            const string serializedValue = "fishy";

            //Act
            var result = new ParameterDefinition(fullyQualifiedTypeName, serializedValue);

            //Assert
            result.FullyQualifiedTypeName.Should().Be(fullyQualifiedTypeName);
            result.SerializedValue.Should().Be(serializedValue);
        }
    }
}
