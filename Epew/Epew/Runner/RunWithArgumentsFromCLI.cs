using Epew.Core.Helper;
using Epew.Core.Verbs;
using GRYLibrary.Core.ExecutePrograms;
using GRYLibrary.Core.ExecutePrograms.WaitingStates;
using GRYLibrary.Core.Logging.GRYLogger;
using GRYLibrary.Core.Logging.GRYLogger.ConcreteLogTargets;
using GRYLibrary.Core.Misc;
using GRYLibrary.Core.Misc.FilePath;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Epew.Core.Runner
{
    internal class RunWithArgumentsFromCLI :RunBase
    {

        internal const int ExitCodeNoProgramExecuted = 2147393803;
        internal const int ExitCodeTimeout = 2147393804;
        internal const string ProgramName = "Epew";
        private readonly RunCLI _Options;
        private string _Title = null;
        internal ExternalProgramExecutor _ExternalProgramExecutor = null;
        public RunWithArgumentsFromCLI(ProgramStarter programStarter, RunCLI options) : base(programStarter)
        {
            this._Options = options;
        }

        protected override int RunImplementation()
        {
            if(this._Options.HideConsoleWindow)
            {
                ConsoleExtensions.HideConsoleWindow();
            }
            if(this._Options.Verbosity == Verbosity.Verbose)
            {
                foreach(GRYLogTarget logtarget in _ProgramStarter._Log.Configuration.LogTargets)
                {
                    logtarget.LogLevels.Add(LogLevel.Debug);
                }
            }
            Guid executionId = Guid.NewGuid();
            int result = ExitCodeNoProgramExecuted;
            try
            {
                RemoveQuotes(this._Options);
                string argumentForExecution;
                if(this._Options.ArgumentIsBase64Encoded)
                {
                    argumentForExecution = new UTF8Encoding(false).GetString(Convert.FromBase64String(this._Options.Argument));
                }
                else
                {
                    if(string.IsNullOrWhiteSpace(this._Options.Argument))
                    {
                        argumentForExecution = string.Empty;
                    }
                    else
                    {
                        argumentForExecution = this._Options.Argument;
                    }
                }
                string workingDirectory;
                if(string.IsNullOrWhiteSpace(this._Options.Workingdirectory))
                {
                    workingDirectory = Directory.GetCurrentDirectory();
                }
                else
                {
                    if(Directory.Exists(this._Options.Workingdirectory))
                    {
                        workingDirectory = this._Options.Workingdirectory;
                    }
                    else
                    {
                        throw new ArgumentException($"The specified workingdirectory '{this._Options.Workingdirectory}' does not exist.");
                    }
                }
                if(string.IsNullOrWhiteSpace(this._Options.Program))
                {
                    throw new ArgumentException($"No program to execute specified.");
                }

                string commandLineExecutionAsString = $"'{workingDirectory}>{this._Options.Program} {argumentForExecution}'";
                if(string.IsNullOrWhiteSpace(this._Options.Title))
                {
                    this._Title = $"{ProgramName}: {commandLineExecutionAsString}";
                }
                else
                {
                    this._Title = this._Options.Title;
                }
                if(!string.IsNullOrWhiteSpace(this._Options.LogFile))
                {
                    foreach(GRYLogTarget target in this._ProgramStarter._Log.Configuration.LogTargets)
                    {
                        if(target is LogFile logFile)
                        {
                            logFile.Enabled = true;
                            logFile.File = AbstractFilePath.FromString(this._Options.LogFile);
                            break;
                        }
                    }
                }

                foreach(GRYLogTarget target in _ProgramStarter._Log.Configuration.LogTargets)
                {
                    target.Format = this._Options.AddLogOverhead ? GRYLogLogFormat.GRYLogFormat : GRYLogLogFormat.OnlyMessage;
                }
                string commandLineArguments = Utilities.GetCommandLineArguments();
                ExternalProgramExecutorConfiguration externalProgramExecutorConfiguration = new ExternalProgramExecutorConfiguration()
                {
                    Program = this._Options.Program,
                    Argument = argumentForExecution,
                    WorkingDirectory = workingDirectory,
                    Verbosity = this._Options.Verbosity,
                    User = this._Options.User,
                    Password = this._Options.Password,
                    CreateWindow = !this._Options.HideConsoleWindow,
                    RedirectStandardInput = true,
                    TimeoutInMilliseconds = this._Options.TimeoutInMilliseconds,
                };
                if(this._Options.NotSynchronous)
                {
                    externalProgramExecutorConfiguration.WaitingState = new RunAsynchronously();
                }
                else
                {
                    externalProgramExecutorConfiguration.WaitingState = new RunSynchronously();
                }
                this._ExternalProgramExecutor = new ExternalProgramExecutor(externalProgramExecutorConfiguration)
                {
                    LogObject = _ProgramStarter._Log
                };
                using(IDisposable subNamespace = _ProgramStarter._Log.UseSubNamespace(this._Options.LogNamespace))
                {
                    this._ExternalProgramExecutor.Run();
                }

                WriteNumberToFile(this._Options.Verbosity, executionId, this._Title, commandLineExecutionAsString, this._ExternalProgramExecutor.ProcessId, "process-id", this._Options.ProcessIdFile);
                if(this._Options.NotSynchronous)
                {
                    return 0;
                }
                else
                {
                    new Task(() =>
                    {
                        this._ExternalProgramExecutor.WaitUntilTerminated();
                        this.ProgramExecutionResultHandler(this._ExternalProgramExecutor, this._Options, executionId, commandLineExecutionAsString);
                    }).Start();
                    result = this._ExternalProgramExecutor.ExitCode;
                }
            }
            catch(Exception exception)
            {
                _ProgramStarter._Log.Log($"Error in {ProgramStarter.ProgramName}.", exception);
            }
            return result;
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
        private static void WriteToFile(string file, string[] lines)
        {
            if(!string.IsNullOrEmpty(file))
            {
                file = file.Trim().ResolveToFullPath();
                Utilities.EnsureFileExists(file);
                File.AppendAllLines(file, lines, new UTF8Encoding(false));
            }
        }
        private int ProgramExecutionResultHandler(ExternalProgramExecutor externalProgramExecutor, RunCLI options, Guid executionId, string commandLineExecutionAsString)
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

        private static void RemoveQuotes(RunCLI options)
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
    }
}
