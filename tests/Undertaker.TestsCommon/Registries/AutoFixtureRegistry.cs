using AutoFixture;
using Lamar;

namespace Undertaker.TestsCommon.Registries
{
    public class AutoFixtureRegistry : ServiceRegistry
    {
        public AutoFixtureRegistry()
        {
            Initialize();
        }

        private void Initialize()
        {
            this.For<Fixture>()
                .Use(ctx => new Fixture())
                .Singleton();
        }
    }
}
