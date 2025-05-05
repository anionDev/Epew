using System.Collections.Generic;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using GRYLibrary.Core.Misc;
using System.Reflection;
using Microsoft.Extensions.Logging;
using GRYLibrary.Core.Logging.GRYLogger;
using Epew.Core.Verbs;
using Error = CommandLine.Error;
using Epew.Core.Runner;

namespace Epew.Core.Helper
{
    public class ProgramStarter
    {
        internal const string ProgramName = "Epew";
        internal const string ProjectLink = "https://github.com/anionDev/Epew";
        internal const int ExitCodeFatalErrorOccurred = 2147393801;
        internal const int ExitCodeParsingError = 2147393802;
        internal readonly IGRYLog _Log;
        internal string Version { get; private set; }
        internal string LicenseLink { get; private set; }
        internal IList<string> BasicHelp { get; private set; }
        internal RunBase? Result;
        public ProgramStarter()
        {
            this._Log = GRYLog.Create();
            this.Version = GetVersion();
            this.LicenseLink = $"https://raw.githubusercontent.com/anionDev/Epew/v{this.Version}/License.txt";
            this.BasicHelp = new List<string>() {
                $"{ProgramName} v{GetVersion()}",
                $"Run '{ProgramName} --help' to get help about the usage.",
                $"License: {this.LicenseLink}",
                $"See {ProjectLink} for further information.",
            };

        }
        public int Main(string[] arguments)
        {
            try
            {
                if(arguments.Length == 0)
                {
                    this.PrintBasicHelp();
                    return 0;
                }
                else
                {
                    ParserResult<object> parserResult = Parser.Default.ParseArguments<RunCLI, RunFile>(arguments);
                    return parserResult.MapResult(
                        (RunCLI parsedArgument) => parsedArgument.Accept(new VerbVisitor(this)),
                        (RunFile parsedArgument) => parsedArgument.Accept(new VerbVisitor(this)),
                        (IEnumerable<Error> errors) => this.HandleErrors(errors, parserResult)
                    );
                }
            }
            catch
            {
                return ExitCodeFatalErrorOccurred;
            }
        }

        private void PrintBasicHelp()
        {
            foreach(string line in this.BasicHelp)
            {
                this._Log.Log(line);
            }
        }

        private int HandleErrors(IEnumerable<Error> errors, ParserResult<object> parserResult)
        {
            // Optional: Fehler auswerten
            if(errors.IsHelp())
            {
                HelpText helpText = HelpText.AutoBuild(parserResult, h =>
                {
                    h.Heading = $"{ProgramName} v{this.Version}";
                    h.AdditionalNewLineAfterOption = false;
                    h.AddDashesToOption = true;
                    h.AutoVersion = false;
                    return h;
                }, e => e);
                this._Log.Log(helpText);
                this._Log.Log(string.Empty);
                this._Log.Log($"{ProgramName} is a tool to wrap program-calls with some useful functions like getting stdout, stderr, exitcode, the ability to set a timeout and so on.");
                this._Log.Log($"See {ProjectLink} for further information.");
                this._Log.Log(string.Empty);
                this._Log.Log($"Exitcodes:");
                this._Log.Log($"{ExitCodeFatalErrorOccurred}: If a fatal error occurred");
                this._Log.Log($"{ExitCodeParsingError}: If the arguments can not be parsed");
                this._Log.Log($"{RunWithArgumentsFromCLI.ExitCodeNoProgramExecuted}: If no program was executed");
                this._Log.Log($"{RunWithArgumentsFromCLI.ExitCodeTimeout}: If the executed program was aborted due to the given timeout");
                this._Log.Log($"If running synchronously then the exitcode of the executed program will be set as exitcode of {ProgramName}.");
                this._Log.Log($"If running asynchronously then the process-id of the executed program will be set as exitcode of {ProgramName}.");
                return 0;
            }
            if(errors.IsVersion())
            {
                this._Log.Log($"v{this.Version}");
                return 0;
            }

            Utilities.AssertCondition(errors.Any(), "No errors occurred.");
            this._Log.Log($"The following errors occurred:", LogLevel.Error);
            foreach(Error error in errors)
            {
                this._Log.Log(Utilities.GetValue(error.Tag.ToString()), LogLevel.Error);
            }
            return ExitCodeParsingError;
        }
        private static string GetVersion()
        {
            System.Version version = Utilities.GetValue(Utilities.GetValue(Assembly.GetExecutingAssembly().GetName()).Version);
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
