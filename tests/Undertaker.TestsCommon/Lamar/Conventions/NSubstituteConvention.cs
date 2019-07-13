using Lamar;
using Lamar.Scanning;
using Lamar.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;

namespace Undertaker.TestsCommon.Lamar.Conventions
{
    public class NSubstituteConvention : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, ServiceRegistry services)
        {
            foreach (var type in types.FindTypes(TypeClassification.Interfaces))
            {
                services.AddSingleton(type, ctx => Substitute.For(new[] { type }, Array.Empty<object>()));
            }
        }
    }
}
