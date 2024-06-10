using System.Runtime.CompilerServices;

namespace GitOps.Updater.Tests
{
    public static class ModuleInitializer
    {

        [ModuleInitializer]
        public static void Initialize() =>
            VerifyDiffPlex.Initialize();
    }
}
