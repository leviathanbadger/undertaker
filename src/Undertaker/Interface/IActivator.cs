using System;

namespace Undertaker
{
    public interface IActivator
    {
        object Activate(Type type);
    }
}
