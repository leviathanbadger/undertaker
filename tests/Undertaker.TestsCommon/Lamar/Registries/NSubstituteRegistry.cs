using Lamar;
using Undertaker.TestsCommon.Lamar.Conventions;

namespace Undertaker.TestsCommon.Lamar.Registries
{
    public class NSubstituteRegistry : ServiceRegistry
    {
        public NSubstituteRegistry()
        {
            Initialize();
        }

        private void Initialize()
        {
            Scan(scanner =>
            {
                scanner.AssembliesFromApplicationBaseDirectory(assembly => assembly.FullName.StartsWith("Undertaker"));
                scanner.Convention<NSubstituteConvention>();
            });
        }
    }
}
