using System;
using System.Collections.Generic;

namespace Undertaker
{
    public class JobDefinition
    {
        public JobDefinition(
            string name,
            string desc,
            string fullyQualifiedTypeName,
            string methodName,
            bool isStatic,
            ParameterDefinition[] parameters,
            DateTime? runAt,
            IJob[] runAfter)
        {
            Name = name;
            Description = desc;
            FullyQualifiedTypeName = fullyQualifiedTypeName;
            MethodName = methodName;
            IsMethodStatic = isStatic;
            Parameters = Array.AsReadOnly(parameters ?? new ParameterDefinition[0]);
            RunAtTime = runAt;
            RunAfterJobs = Array.AsReadOnly(runAfter ?? new IJob[0]);
        }

        public string Name { get; }
        public string Description { get; }
        public string FullyQualifiedTypeName { get; }
        public string MethodName { get; }
        public bool IsMethodStatic { get; }
        public IReadOnlyCollection<ParameterDefinition> Parameters { get; }
        public DateTime? RunAtTime { get; }
        public IReadOnlyCollection<IJob> RunAfterJobs { get; }
    }
}
