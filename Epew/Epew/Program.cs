using GRYLibrary.Core.Miscellaneous;

namespace Epew.Epew.Core
{
    internal static class Program
    {
        internal static int Main(string[] commandlineArguments)
        {
            var gryConsoleApplication = new GRYConsoleApplication<EpewOptions>(MainI);
            return gryConsoleApplication.Main(commandlineArguments);
        }
        internal static int MainI(string[] arguments)
        {
            return new ProgramExecutor().Main(arguments);
        }
    }
}
