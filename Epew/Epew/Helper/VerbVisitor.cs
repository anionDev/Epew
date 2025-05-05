using Epew.Core.Runner;
using Epew.Core.Verbs;

namespace Epew.Core.Helper
{
    internal class VerbVisitor :IVerbBaseVisitor<int>
    {
        private readonly ProgramStarter _ProgramExecutor;

        public VerbVisitor(ProgramStarter programExecutor)
        {
            this._ProgramExecutor = programExecutor;
        }

        public int Handle(RunCLI runWithOptionsFromCLI)
        {
            return new RunWithArgumentsFromCLI(this._ProgramExecutor,runWithOptionsFromCLI).Run();
        }

        public int Handle(RunFile runWithOptionsFromFile)
        {
            return new RunWithArgumentsFromFile(this._ProgramExecutor,runWithOptionsFromFile).Run();
        }
    }
}
