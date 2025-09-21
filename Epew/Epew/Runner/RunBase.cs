using Epew.Core.Helper;

namespace Epew.Core.Runner
{
    public abstract class RunBase
    {
        protected readonly ProgramStarter _ProgramStarter;

        protected RunBase(ProgramStarter programStarter)
        {
            this._ProgramStarter = programStarter;
        }

        public int Run() {
            _ProgramStarter.Result = this;
          return this.RunImplementation();

        }
        protected abstract int RunImplementation();
    }
}
