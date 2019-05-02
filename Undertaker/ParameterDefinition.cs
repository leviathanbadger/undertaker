namespace Undertaker
{
    public class ParameterDefinition
    {
        public ParameterDefinition(
            string fullyQualifiedTypeName,
            string serializedValue)
        {
            FullyQualifiedTypeName = fullyQualifiedTypeName;
            SerializedValue = serializedValue;
        }

        public string FullyQualifiedTypeName { get; }
        public string SerializedValue { get; }
    }
}
