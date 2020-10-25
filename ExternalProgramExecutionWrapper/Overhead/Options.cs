using CommandLine;

namespace ExternalProgramExecutionWrapper.Overhead
{
    public class Options
    {
        [Option('p', nameof(Program), Required = true, HelpText = "Program which should be executed")]
        public string Program { get; set; }

        [Option('a', nameof(Argument), Required = false, HelpText = "Argument for the program which should be executed")]
        public string Argument { get; set; }

        [Option('b', nameof(ArgumentIsBase64Encoded), Required = false, HelpText = "Specifiy whether " + nameof(Argument) + " is base64-encoded", Default = false)]
        public bool ArgumentIsBase64Encoded { get; set; }

        [Option('w', nameof(Workingdirectory), Required = false, HelpText = "Workingdirectory for the program which should be executed")]
        public string Workingdirectory { get; set; }

        [Option('v', nameof(Verbosity), Required = false, HelpText = "Verbosity of " + nameof(ExternalProgramExecutionWrapper), Default = Verbosity.Normal)]
        public Verbosity Verbosity { get; set; }

        [Option('i', nameof(PrintErrorsAsInformation), Required = false, HelpText = "Treat errors as information", Default = false)]
        public bool PrintErrorsAsInformation { get; set; }

        [Option('h', nameof(AddLogOverhead), Required = false, HelpText = "Add log overhead", Default = false)]
        public bool AddLogOverhead { get; set; }

        [Option('l', nameof(LogFile), Required = false, HelpText = "Logfile for " + nameof(ExternalProgramExecutionWrapper))]
        public string LogFile { get; set; }

        [Option('o', nameof(StdOutFile), Required = false, HelpText = "File for the stdout of the executed program")]
        public string StdOutFile { get; set; }

        [Option('e', nameof(StdErrFile), Required = false, HelpText = "File for the stderr of the executed program")]
        public string StdErrFile { get; set; }

        [Option('x', nameof(ExitCodeFile), Required = false, HelpText = "File for the exitcode of the executed program")]
        public string ExitCodeFile { get; set; }

        [Option('d', nameof(TimeoutInMilliseconds), Required = false, HelpText = "Maximal duration of the execution process before it will by aborted by " + nameof(ExternalProgramExecutionWrapper), Default = int.MaxValue)]
        public int TimeoutInMilliseconds { get; set; }

        [Option('t', nameof(Title), Required = false, HelpText = "Title for the execution-process")]
        public string Title { get; set; }

        [Option('n', nameof(NotSynchronous), Required = false, HelpText = "Run the program asynchronously", Default = false)]
        public bool NotSynchronous { get; set; }
    }
}
