using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using GRYLibrary.Core.Misc;
using System.IO;
using System.Reflection;
using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.ExecutePrograms.WaitingStates;
using Microsoft.Extensions.Logging;
using GRYLibrary.Core.Misc.FilePath;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Logging.GRYLogger.ConcreteLogTargets;

namespace Epew.Core
{
    internal class ProgramExecutor
    {
        internal const string ProgramName = "Epew";
        internal const string ProjectLink = "https://github.com/anionDev/Epew";
        internal const string LicenseName = "MIT";

        internal const int ExitCodeNoProgramExecuted = 2147393801;
        internal const int ExitCodeFatalErroroccurred = 2147393802;
        internal const int ExitCodeTimeout = 2147393803;

        private GRYLog _Log = null;
        private SentenceBuilder _SentenceBuilder = null;
        private string _Title = null;
        internal ExternalProgramExecutor _ExternalProgramExecutor = null;
        internal string Version { get; private set; }
        internal string LicenseLink { get; private set; }

        public int Main(string[] arguments)
        {
            int result = ExitCodeNoProgramExecuted;
            try
            {
                this.Version = GetVersion();
                this.LicenseLink = $"https://raw.githubusercontent.com/anionDev/Epew/v{this.Version}/License.txt";
                this._SentenceBuilder = SentenceBuilder.Create();
                if(arguments == null)
                {
                    throw Utilities.CreateNullReferenceExceptionDueToParameter(nameof(arguments));
                }
                string argumentsAsString = string.Join(' ', arguments);
                this._Log = GRYLog.Create();
                this._Log.Configuration.Initliaze();
                string workingDirectory = Directory.GetCurrentDirectory();
                try
                {
                    if(arguments.Length == 0)
                    {
                        this._Log.Log($"{ProgramName} v{this.Version}");
                        this._Log.Log($"Run '{ProgramName} --help' to get help about the usage.");
                    }
                    else
                    {
                        ParserResult<EpewOptions> parserResult = new Parser(settings => settings.CaseInsensitiveEnumValues = true).ParseArguments<EpewOptions>(arguments);
                        if(IsHelpCommand(arguments))
                        {
                            this.WriteHelp(parserResult);
                        }
                        else
                        {
                            parserResult.WithParsed(options => result = this.HandleSuccessfullyParsedArguments(options))
                            .WithNotParsed(errors => this.HandleParsingErrors(argumentsAsString, errors));
                        }
                    }
                }
                catch(Exception exception)
                {
                    this._Log.Log($"Fatal error occurred while processing argument '{workingDirectory}> epew {argumentsAsString}", exception);
                }
            }
            catch(Exception exception)
            {
                this._Log.Log($"Fatal error occurred", exception);
                result = ExitCodeFatalErroroccurred;
            }
            this._Log.Log($"Finished Epew", LogLevel.Debug);
            return result;
        }

        private void HandleParsingErrors(string argumentsAsString, IEnumerable<Error> errors)
        {
            int amountOfErrors = errors.Count();
            this._Log.Log($"Argument '{argumentsAsString}' could not be parsed successfully.", LogLevel.Error);
            if(0 < amountOfErrors)
            {
                this._Log.Log($"The following error{(amountOfErrors == 1 ? string.Empty : "s")} occurred:", LogLevel.Error);
                foreach(Error error in errors)
                {
                    this._Log.Log($"{error.Tag}: {this._SentenceBuilder.FormatError(error)}", LogLevel.Error);
                }
            }
        }

        private int HandleSuccessfullyParsedArguments(EpewOptions options)
        {
            if(options.HideConsoleWindow)
            {
                ConsoleExtensions.HideConsoleWindow();
            }
            if(options.Verbosity == Verbosity.Verbose)
            {
                foreach(GRYLogTarget logtarget in this._Log.Configuration.LogTargets)
                {
                    logtarget.LogLevels.Add(LogLevel.Debug);
                }
            }
            Guid executionId = Guid.NewGuid();
            int result = ExitCodeNoProgramExecuted;
            try
            {
                RemoveQuotes(options);
                string argumentForExecution;
                if(options.ArgumentIsBase64Encoded)
                {
                    argumentForExecution = new UTF8Encoding(false).GetString(Convert.FromBase64String(options.Argument));
                }
                else
                {
                    if(string.IsNullOrWhiteSpace(options.Argument))
                    {
                        argumentForExecution = string.Empty;
                    }
                    else
                    {
                        argumentForExecution = options.Argument;
                    }
                }
                string workingDirectory;
                if(string.IsNullOrWhiteSpace(options.Workingdirectory))
                {
                    workingDirectory = Directory.GetCurrentDirectory();
                }
                else
                {
                    if(Directory.Exists(options.Workingdirectory))
                    {
                        workingDirectory = options.Workingdirectory;
                    }
                    else
                    {
                        throw new ArgumentException($"The specified workingdirectory '{options.Workingdirectory}' does not exist.");
                    }
                }
                if(string.IsNullOrWhiteSpace(options.Program))
                {
                    throw new ArgumentException($"No program to execute specified.");
                }

                string commandLineExecutionAsString = $"'{workingDirectory}>{options.Program} {argumentForExecution}'";
                if(string.IsNullOrWhiteSpace(options.Title))
                {
                    this._Title = $"{ProgramName}: {commandLineExecutionAsString}";
                }
                else
                {
                    this._Title = options.Title;
                }
                if(!string.IsNullOrWhiteSpace(options.LogFile))
                {
                    foreach(var target in this._Log.Configuration.LogTargets)
                    {
                        if(target is LogFile logFile)
                        {
                            logFile.Enabled = true;
                            logFile.File = AbstractFilePath.FromString(options.LogFile);
                            break;
                        }
                    }
                }

                foreach(GRYLogTarget target in this._Log.Configuration.LogTargets)
                {
                    target.Format = options.AddLogOverhead ? GRYLogLogFormat.GRYLogFormat : GRYLogLogFormat.OnlyMessage;
                }
                string commandLineArguments = Utilities.GetCommandLineArguments();
                ExternalProgramExecutorConfiguration externalProgramExecutorConfiguration = new ExternalProgramExecutorConfiguration()
                {
                    Program = options.Program,
                    Argument = argumentForExecution,
                    WorkingDirectory = workingDirectory,
                    Verbosity = options.Verbosity,
                    User = options.User,
                    Password = options.Password,
                    CreateWindow = !options.HideConsoleWindow,
                    TimeoutInMilliseconds = options.TimeoutInMilliseconds,
                };
                if(options.NotSynchronous)
                {
                    externalProgramExecutorConfiguration.WaitingState = new RunAsynchronously();
                }
                else
                {
                    externalProgramExecutorConfiguration.WaitingState = new RunSynchronously();
                }
                this._ExternalProgramExecutor = new ExternalProgramExecutor(externalProgramExecutorConfiguration)
                {
                    LogObject = this._Log
                };

                using(IDisposable subNamespace = this._Log.UseSubNamespace(options.LogNamespace))
                {
                    this._ExternalProgramExecutor.Run();
                }

                WriteNumberToFile(options.Verbosity, executionId, this._Title, commandLineExecutionAsString, this._ExternalProgramExecutor.ProcessId, "process-id", options.ProcessIdFile);
                if(options.NotSynchronous)
                {
                    return 0;
                }
                else
                {
                    new Task(() =>
                    {
                        this._ExternalProgramExecutor.WaitUntilTerminated();
                        this.ProgramExecutionResultHandler(this._ExternalProgramExecutor, options, executionId, commandLineExecutionAsString);
                    }).Start();
                    result = this._ExternalProgramExecutor.ExitCode;
                }
            }
            catch(Exception exception)
            {
                this._Log.Log($"Error in {ProgramName}.", exception);
            }
            return result;
        }

        private static void RemoveQuotes(EpewOptions options)
        {
            options.Argument = TrimQuotes(options.Argument);
            options.Program = TrimQuotes(options.Program);
            options.Workingdirectory = TrimQuotes(options.Workingdirectory);
            options.LogFile = TrimQuotes(options.LogFile);
            options.ExitCodeFile = TrimQuotes(options.ExitCodeFile);
            options.ProcessIdFile = TrimQuotes(options.ProcessIdFile);
            options.StdOutFile = TrimQuotes(options.StdOutFile);
            options.StdErrFile = TrimQuotes(options.StdErrFile);
            options.Title = TrimQuotes(options.Title);
            options.LogNamespace = TrimQuotes(options.LogNamespace);
        }

        private static string TrimQuotes(string argument)
        {
            if(argument == null)
            {
                return string.Empty;
            }
            else
            {
                return Utilities.EnsurePathHasNoLeadingOrTrailingQuotes(argument.Trim()).Trim();
            }
        }

        private static void WriteNumberToFile(Verbosity verbosity, Guid executionId, string title, string commandLineExecutionAsString, int value, string nameOfValue, string file)
        {
            List<string> fileContent = new()
            {
                value.ToString()
            };
            if(verbosity == Verbosity.Verbose)
            {
                fileContent.Add($"Execution '{title}' ('{commandLineExecutionAsString}') with execution-id {executionId} has {nameOfValue}");
            }
            WriteToFile(file, fileContent.ToArray());
        }

        private int ProgramExecutionResultHandler(ExternalProgramExecutor externalProgramExecutor, EpewOptions options, Guid executionId, string commandLineExecutionAsString)
        {
            try
            {
                int result;
                if(externalProgramExecutor.ProcessWasAbortedDueToTimeout)
                {
                    result = ExitCodeTimeout;
                }
                else
                {
                    result = externalProgramExecutor.ExitCode;
                }
                WriteToFile(options.StdOutFile, externalProgramExecutor.AllStdOutLines);
                WriteToFile(options.StdErrFile, externalProgramExecutor.AllStdErrLines);
                WriteNumberToFile(options.Verbosity, executionId, this._Title, commandLineExecutionAsString, result, "exit-code", options.ExitCodeFile);
                return result;
            }
            finally
            {
                externalProgramExecutor.Dispose();
            }
        }

        private void WriteHelp(ParserResult<EpewOptions> argumentParserResult)
        {
            this._Log.Log(HelpText.AutoBuild(argumentParserResult).ToString());
            this._Log.Log(string.Empty);
            this._Log.Log($"{ProgramName} is a tool to wrap program-calls with some useful functions like getting stdout, stderr, exitcode and the ability to set a timeout and so on.");
            this._Log.Log(string.Empty);
            this._Log.Log($"Current version: v{this.Version}");
            this._Log.Log($"For more information see the website of the {ProgramName}-project: {ProjectLink}");
            this._Log.Log($"{ProgramName} is licensed under the terms of {LicenseName}. For the concrete license-text see {this.LicenseLink}");
            this._Log.Log(string.Empty);
            this._Log.Log($"Exitcodes:");
            this._Log.Log($"{ExitCodeNoProgramExecuted}: If no program was executed");
            this._Log.Log($"{ExitCodeFatalErroroccurred}: If a fatal error occurred");
            this._Log.Log($"{ExitCodeTimeout}: If the executed program was aborted due to the given timeout");
            this._Log.Log($"If running synchronously then the exitcode of the executed program will be set as exitcode of {ProgramName}.");
            this._Log.Log($"If running asynchronously then the process-id of the executed program will be set as exitcode of {ProgramName}.");
        }

        private static bool IsHelpCommand(string[] arguments)
        {
            foreach(string argument in arguments)
            {
                string argumentLower = argument.ToLower();
                if(argumentLower.Equals("--help")
                    || argumentLower.Equals("/help")
                    || argumentLower.Equals("/h"))
                {
                    return true;
                }
            }
            return false;
        }

        private static void WriteToFile(string file, string[] lines)
        {
            if(!string.IsNullOrEmpty(file))
            {
                file = Utilities.ResolveToFullPath(file.Trim());
                Utilities.EnsureFileExists(file);
                File.AppendAllLines(file, lines, new UTF8Encoding(false));
            }
        }

        private static string GetVersion()
        {
            System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
