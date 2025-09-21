using CommandLine;
using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.Misc;
using Epew.Core.Helper;

namespace Epew.Core.Verbs
{
    [Verb(nameof(RunCLI), isDefault: true, HelpText = "Runs a program using the arguments specified by commandline-arguments.")]
    public class RunCLI:VerbBase
    {
        internal ExternalProgramExecutor _ExternalProgramExecutor = null;

        [Option('p', nameof(Program), Required = true, HelpText = "Program which should be executed")]
        public string Program { get; set; }

        [Option('a', nameof(Argument), Required = false, HelpText = "Argument for the program which should be executed", Default = Utilities.EmptyString)]
        public string Argument { get; set; }

        [Option('b', nameof(ArgumentIsBase64Encoded), Required = false, HelpText = "Specifies whether " + nameof(Argument) + " is base64-encoded", Default = false)]
        public bool ArgumentIsBase64Encoded { get; set; }

        [Option('w', nameof(Workingdirectory), Required = false, HelpText = "Workingdirectory for the program which should be executed")]
        public string Workingdirectory { get; set; }

        [Option('v', nameof(Verbosity), Required = false, HelpText = "Verbosity of " + ProgramStarter.ProgramName + ". The concrete available values are documentated at https://aniondev.github.io/GRYLibraryReference/api/GRYLibrary.Core.Miscellaneous.Verbosity.html", Default = Verbosity.Full)]//TODO fix this link
        public Verbosity Verbosity { get; set; }

        [Option('i', nameof(PrintErrorsAsInformation), Required = false, HelpText = "Treat errors as information", Default = false)]
        public bool PrintErrorsAsInformation { get; set; }

        [Option('g', nameof(AddLogOverhead), Required = false, HelpText = "Add log overhead", Default = false)]
        public bool AddLogOverhead { get; set; }

        [Option('f', nameof(LogFile), Required = false, HelpText = "Logfile for " + ProgramStarter.ProgramName)]
        public string LogFile { get; set; }

        [Option('o', nameof(StdOutFile), Required = false, HelpText = "File for the stdout of the executed program")]
        public string StdOutFile { get; set; }

        [Option('e', nameof(StdErrFile), Required = false, HelpText = "File for the stderr of the executed program")]
        public string StdErrFile { get; set; }

        [Option('x', nameof(ExitCodeFile), Required = false, HelpText = "File for the exitcode of the executed program")]
        public string ExitCodeFile { get; set; }

        [Option('r', nameof(ProcessIdFile), Required = false, HelpText = "File for the process-id of the executed program")]
        public string ProcessIdFile { get; set; }

        [Option('d', nameof(TimeoutInMilliseconds), Required = false, HelpText = "Maximal duration of the execution process before it will by aborted by " + ProgramStarter.ProgramName, Default = int.MaxValue)]
        public int TimeoutInMilliseconds { get; set; }

        [Option('t', nameof(Title), Required = false, HelpText = "Title for the execution-process")]
        public string Title { get; set; }

        [Option('n', nameof(NotSynchronous), Required = false, HelpText = "Run the program asynchronously", Default = false)]
        public bool NotSynchronous { get; set; }

        [Option('l', nameof(LogNamespace), Required = false, HelpText = "Namespace for log", Default = "")]
        public string LogNamespace { get; set; }

        [Option('u', nameof(User), Required = false, HelpText = "Run the program as the given user", Default = null)]
        public string User { get; set; }
        [Option('c', nameof(Password), Required = false, HelpText = "Password of the user", Default = null)]
        public string Password { get; set; }

        [Option('h', nameof(HideConsoleWindow), Required = false, HelpText = "Hide console window", Default = false)]
        public bool HideConsoleWindow { get; set; }

        public override void Accept(IVerbBaseVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IVerbBaseVisitor<T> visitor)
        {
           return visitor.Handle(this);
        }
    }
}
