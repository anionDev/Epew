using CommandLine;
using CommandLine.Text;
using Epew.Overhead;
using GRYLibrary.Core;
using GRYLibrary.Core.Log;
using GRYLibrary.Core.Log.ConcreteLogTargets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Epew
{
    public static class Program
    {
        public const string ProgramName = "epew";
        public const string ProjectLink = "https://github.com/anionDev/Epew";
        public const string LicenseName = "MIT";
        public static readonly string Version = GetVersion();
        public static readonly string LicenseLink = $"https://raw.githubusercontent.com/anionDev/Epew/v{Version}/License.txt";

        public const int ExitCodeNoProgramExecuted = 2147393801;
        public const int ExitCodeFatalErroroccurred = 2147393802;
        public const int ExitCodeTimeout = 2147393803;

        private static string _Title;

        internal static int Main(string[] arguments)
        {
            int result = ExitCodeNoProgramExecuted;
            try
            {
                string argument = Utilities.GetCommandLineArguments();
                ParserResult<Options> argumentParserResult = new Parser(settings => settings.CaseInsensitiveEnumValues = true).ParseArguments<Options>(arguments);
                if (string.IsNullOrWhiteSpace(argument))
                {
                    System.Console.WriteLine($"{ProgramName} v{Version}");
                    System.Console.WriteLine($"Try \"{ProgramName} --help\" to get information about the usage.");
                }
                else if (IsHelpCommand(argument))
                {
                    WriteHelp(argumentParserResult);
                }
                else
                {
                    argumentParserResult.WithParsed(options =>
                    {
                        ExternalProgramExecutor externalProgramExecutor = null;
                        Guid executionId = Guid.NewGuid();
                        GRYLog log = GRYLog.Create();
                        log.Configuration.ResetToDefaultValues();
                        log.Configuration.WriteExceptionStackTraceOfExceptionInLogEntry = true;
                        try
                        {
                            RemoveQuotes(options);
                            string argumentForExecution;
                            if (options.ArgumentIsBase64Encoded)
                            {
                                argumentForExecution = new UTF8Encoding(false).GetString(Convert.FromBase64String(options.Argument));
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(options.Argument))
                                {
                                    argumentForExecution = string.Empty;
                                }
                                else
                                {
                                    argumentForExecution = options.Argument;
                                }
                            }
                            string workingDirectory;
                            if (string.IsNullOrWhiteSpace(options.Workingdirectory))
                            {
                                workingDirectory = Directory.GetCurrentDirectory();
                            }
                            else
                            {
                                if (Directory.Exists(options.Workingdirectory))
                                {
                                    workingDirectory = options.Workingdirectory;
                                }
                                else
                                {
                                    throw new ArgumentException($"The specified workingdirectory '{options.Workingdirectory}' does not exist.");
                                }
                            }
                            if (string.IsNullOrWhiteSpace(options.Program))
                            {
                                throw new ArgumentException($"No program to execute specified.");
                            }

                            string commandLineExecutionAsString = $"'{workingDirectory}>{options.Program} {argumentForExecution}'";
                            if (string.IsNullOrWhiteSpace(options.Title))
                            {
                                _Title = $"{ProgramName}: {commandLineExecutionAsString}";
                            }
                            else
                            {
                                _Title = options.Title;
                            }
                            if (options.LogFile != null)
                            {
                                log.Configuration.GetLogTarget<LogFile>().Enabled = true;
                                log.Configuration.GetLogTarget<LogFile>().File = options.LogFile;
                            }
                            log.Configuration.GetLogTarget<GRYLibrary.Core.Log.ConcreteLogTargets.Console>().Enabled = options.WriteOutputToConsole;
                            foreach (GRYLogTarget target in log.Configuration.LogTargets)
                            {
                                target.Format = options.AddLogOverhead ? GRYLogLogFormat.GRYLogFormat : GRYLogLogFormat.OnlyMessage;
                            }
                            log.Configuration.SetEnabledOfAllLogTargets(options.Verbosity != Verbosity.Quiet);
                            if (options.Verbosity == Verbosity.Verbose)
                            {
                                foreach (GRYLogTarget target in log.Configuration.LogTargets)
                                {
                                    target.LogLevels.Add(Microsoft.Extensions.Logging.LogLevel.Debug);
                                }
                            }
                            string commandLineArguments = Utilities.GetCommandLineArguments();
                            string astring;
                            if (options.NotSynchronous)
                            {
                                astring = "asynchronous";
                            }
                            else
                            {
                                astring = "synchronous";
                            }
                            DateTime startTime = DateTime.Now;
                            string startTimeAsString = startTime.ToString(log.Configuration.DateFormat);
                            log.Log($"{ProgramName} v{Version} started at {startTimeAsString}", Microsoft.Extensions.Logging.LogLevel.Debug);
                            log.Log($"Execution-id: {executionId}", Microsoft.Extensions.Logging.LogLevel.Debug);
                            log.Log($"Plain argument: '{commandLineArguments}'", Microsoft.Extensions.Logging.LogLevel.Debug);
                            log.Log($"Start executing {commandLineExecutionAsString} ({astring})", Microsoft.Extensions.Logging.LogLevel.Debug);
                            externalProgramExecutor = new ExternalProgramExecutor(options.Program, argumentForExecution, workingDirectory)
                            {
                                LogObject = log,
                                LogNamespace = options.LogNamespace,
                                PrintErrorsAsInformation = options.PrintErrorsAsInformation,
                                TimeoutInMilliseconds = options.TimeoutInMilliseconds,
                                ThrowErrorIfExitCodeIsNotZero = false
                            };


                            if (options.NotSynchronous)
                            {
                                externalProgramExecutor.StartAsynchronously();
                                new Task(() =>
                                {
                                    externalProgramExecutor.WaitUntilTerminated();
                                    ProgramExecutionResultHandler(externalProgramExecutor, options, log, executionId, startTimeAsString, commandLineExecutionAsString);
                                }).Start();
                                result = externalProgramExecutor.ProcessId;
                            }
                            else
                            {
                                externalProgramExecutor.StartSynchronously();
                                result = ProgramExecutionResultHandler(externalProgramExecutor, options, log, executionId, startTimeAsString, commandLineExecutionAsString);
                            }
                            WriteNumberToFile(options.Verbosity, executionId, _Title, commandLineExecutionAsString, startTimeAsString, externalProgramExecutor.ProcessId, "process-id", options.ProcessIdFile);
                        }
                        catch (Exception exception)
                        {
                            log.Log($"Error in {ProgramName}.", exception);
                        }
                        if (externalProgramExecutor != null)
                        {
                            log.Log($"{ProgramName} finished.", Microsoft.Extensions.Logging.LogLevel.Debug);
                            log.Log($"Execution-id: {executionId}", Microsoft.Extensions.Logging.LogLevel.Debug);
                            log.Log($"Process-id: {externalProgramExecutor.ProcessId}", Microsoft.Extensions.Logging.LogLevel.Debug);
                            if (externalProgramExecutor.CurrentExecutionState == ExecutionState.Terminated)
                            {
                                log.Log($"Exitcode: {result}", Microsoft.Extensions.Logging.LogLevel.Debug);
                                log.Log($"Duration: {Utilities.DurationToUserFriendlyString(externalProgramExecutor.ExecutionDuration)}", Microsoft.Extensions.Logging.LogLevel.Debug);
                            }
                        }
                    });
                }
            }
            catch (Exception exception)
            {
                System.Console.Error.WriteLine($"Fatal error occurred: {exception}");
                result = ExitCodeFatalErroroccurred;
            }
            return result;
        }

        private static void RemoveQuotes(Options options)
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
            if (argument == null)
            {
                return string.Empty;
            }
            else
            {
                return Utilities.EnsurePathHasNoLeadingOrTrailingQuotes(argument.Trim()).Trim();
            }
        }

        private static void WriteNumberToFile(Verbosity verbosity, Guid executionId, string title, string commandLineExecutionAsString, string startTimeAsString, int value, string nameOfValue, string file)
        {
            List<string> fileContent = new List<string>();
            if (verbosity == Verbosity.Verbose)
            {
                fileContent.Add($"{startTimeAsString}: Execution '{title}' ('{commandLineExecutionAsString}') with execution-id {executionId} has {nameOfValue}");
            }
            fileContent.Add(value.ToString());
            WriteToFile(file, fileContent.ToArray());
        }

        private static int ProgramExecutionResultHandler(ExternalProgramExecutor externalProgramExecutor, Options options, GRYLog log, Guid executionId, string startTimeAsString, string commandLineExecutionAsString)
        {
            try
            {
                int result;
                if (externalProgramExecutor.ProcessWasAbortedDueToTimeout)
                {
                    log.Log($"Execution with id {executionId} was aborted due to a timeout. (The timeout was set to {Utilities.DurationToUserFriendlyString(TimeSpan.FromMilliseconds(externalProgramExecutor.TimeoutInMilliseconds.Value))}).", Microsoft.Extensions.Logging.LogLevel.Warning);
                    result = ExitCodeTimeout;
                }
                else
                {
                    result = externalProgramExecutor.ExitCode;
                }
                WriteToFile(options.StdOutFile, externalProgramExecutor.AllStdOutLines);
                WriteToFile(options.StdErrFile, externalProgramExecutor.AllStdErrLines);
                WriteNumberToFile(options.Verbosity, executionId, _Title, commandLineExecutionAsString, startTimeAsString, result, "exit-code", options.ExitCodeFile);
                return result;
            }
            finally
            {
                externalProgramExecutor.Dispose();
            }
        }

        private static void WriteHelp(ParserResult<Options> argumentParserResult)
        {
            System.Console.Out.WriteLine(HelpText.AutoBuild(argumentParserResult).ToString());
            System.Console.Out.WriteLine();
            System.Console.Out.WriteLine($"{ProgramName} is a tool to wrap program-calls with some useful functions like getting stdout, stderr, exitcode and the ability to set a timeout.");
            System.Console.Out.WriteLine();
            System.Console.Out.WriteLine($"Current version: v{Version}");
            System.Console.Out.WriteLine($"For more information see the website of the {ProgramName}-project: {ProjectLink}");
            System.Console.Out.WriteLine($"{ProgramName} is licensed under the terms of {LicenseName}. For the concrete license-text see {LicenseLink}");
            System.Console.Out.WriteLine();
            System.Console.Out.WriteLine($"Exitcodes:");
            System.Console.Out.WriteLine($"{ExitCodeNoProgramExecuted}: If no program was executed");
            System.Console.Out.WriteLine($"{ExitCodeFatalErroroccurred}: If a fatal error occurred");
            System.Console.Out.WriteLine($"{ExitCodeTimeout}: If the executed program was aborted due to the given timeout");
            System.Console.Out.WriteLine($"If running synchronously then the exitcode of the executed program will be set as exitcode of {ProgramName}.");
            System.Console.Out.WriteLine($"If running asynchronously then the process-id of the executed program will be set as exitcode of {ProgramName}.");
        }

        private static bool IsHelpCommand(string argument)
        {
            argument = argument.ToLower().Trim();
            return argument.Equals("help")
                || argument.Equals("--help")
                || argument.Equals("-h")
                || argument.Equals("/help")
                || argument.Equals("/h");
        }

        private static void WriteToFile(string file, string[] lines)
        {
            if (!string.IsNullOrEmpty(file))
            {
                file = Utilities.ResolveToFullPath(file.Trim());
                Utilities.EnsureFileExists(file);
                File.AppendAllLines(file, lines, new UTF8Encoding(false));
            }
        }

        private static string GetVersion()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }
}
