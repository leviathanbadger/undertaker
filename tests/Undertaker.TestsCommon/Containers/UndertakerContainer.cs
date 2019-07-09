using AutoFixture;
using AutoFixture.Dsl;
using Lamar;
using System;
using Undertaker.TestsCommon.Registries;

namespace Undertaker.TestsCommon.Containers
{
    public class UndertakerContainer
    {
        private readonly IContainer _container;

        public UndertakerContainer(Action<ServiceRegistry> afterRegisterFn = null)
        {
            _container = new Container(services =>
            {
                services.IncludeRegistry<AutoFixtureRegistry>();

                afterRegisterFn?.Invoke(services);
            });
        }

        public T Get<T>()
        {
            return _container.GetInstance<T>();
        }

        public T Fake<T>()
        {
            return Get<Fixture>().Create<T>();
        }

        public ICustomizationComposer<T> FakeBuild<T>()
        {
            return Get<Fixture>().Build<T>();
        }
    }
}
