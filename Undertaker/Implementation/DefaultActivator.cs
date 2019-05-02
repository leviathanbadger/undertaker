using System;
using System.Reflection;
using System.Threading;

namespace Undertaker.Implementation
{
    public class DefaultActivator : IActivator
    {
        public object Activate(Type type)
        {
            var ctor = type.GetConstructor(Array.Empty<Type>());
            if (ctor == null) throw new InvalidOperationException($"The type \"{type.FullName}\" cannot be activated by the default activator. There is no public parameterless constructor.");
            return ctor.Invoke(BindingFlags.CreateInstance, null, Array.Empty<object>(), Thread.CurrentThread.CurrentCulture);
        }
    }
}
