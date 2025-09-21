using Epew.Core.Helper;
using Epew.Core.Verbs;
using GRYLibrary.Core.ExecutePrograms;
using System;
using System.Diagnostics;
using System.IO;

namespace Epew.Core.Runner
{
    internal class RunWithArgumentsFromFile :RunBase
    {
        private readonly RunFile _Options;

        public RunWithArgumentsFromFile(ProgramStarter programStarter, RunFile options) : base(programStarter)
        {
            this._Options = options;
        }

        protected override int RunImplementation()
        {
            string argumentsFile = _Options.File;
            string currentFolder = Directory.GetCurrentDirectory();
            if(GRYLibrary.Core.Misc.Utilities.IsRelativeLocalFilePath(argumentsFile))
            {
                argumentsFile = GRYLibrary.Core.Misc.Utilities.ResolveToFullPath(argumentsFile, currentFolder);
            }
            string argumentAsString = File.ReadAllText(argumentsFile).Replace("\r", string.Empty).Replace("\n", string.Empty);
            ExternalProgramExecutor externalProgramExecutor = new ExternalProgramExecutor(new ExternalProgramExecutorConfiguration()
            {
                Program = "epew",
                Argument = argumentAsString,
                WorkingDirectory = currentFolder,
            });
            externalProgramExecutor.Run();
            return externalProgramExecutor.ExitCode;
        }
    }
}
