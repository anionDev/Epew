using CommandLine;
using CommandLine.Text;
using ExternalProgramExecutionWrapper.Overhead;
using GRYLibrary.Core;
using GRYLibrary.Core.Log.ConcreteLogTargets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ExternalProgramExecutionWrapper
{
    public static class Program
    {
        public const string ProgramShortName = "epew";
        public const string ProjectLink = "https://github.com/anionDev/externalProgramExecutionWrapper";
        public const string LicenseLink = "https://raw.githubusercontent.com/anionDev/externalProgramExecutionWrapper/master/License.txt";
        public const string LicenseName = "MIT";

        public const int ExitCodeNoProgramExecuted = 2147393801;
        public const int ExitCodeFatalErroroccurred = 2147393802;
        public const int ExitCodeTimeout = 2147393803;

        internal static int Main(string[] arguments)
        {
            int result = ExitCodeNoProgramExecuted;
            try
            {
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string argument = Utilities.GetCommandLineArguments();
                ParserResult<Options> argumentParserResult = new Parser(settings => settings.CaseInsensitiveEnumValues = true).ParseArguments<Options>(arguments);
                if (string.IsNullOrWhiteSpace(argument))
                {
                    System.Console.WriteLine($"{ProgramShortName} v{version}");
                    System.Console.WriteLine($"Try \"{ProgramShortName} help\" to get information about the usage.");
                }
                else if (IsHelpCommand(argument))
                {
                    WriteHelp(argumentParserResult);
                }
                else
                {
                    argumentParserResult.WithParsed((Action<Options>)(options =>
                    {
                        Guid executionId = Guid.NewGuid();
                        GRYLibrary.Core.Log.GRYLog log = GRYLibrary.Core.Log.GRYLog.Create();
                        log.Configuration.ResetToDefaultValues();
                        ExternalProgramExecutor externalProgramExecutor = null;
                        try
                        {
                            string argumentForExecution;
                            if (options.ArgumentIsBase64Encoded)
                            {
                                argumentForExecution = new UTF8Encoding(false).GetString(Convert.FromBase64String(options.Argument));
                            }
                            else
                            {
                                argumentForExecution = options.Argument;
                            }
                            string commandLineExecutionAsString = $"'{options.Workingdirectory}>{options.Program} {argumentForExecution}'";
                            string title;
                            string shortTitle;
                            if (string.IsNullOrWhiteSpace(options.Title))
                            {
                                title = $"{ProgramShortName}: {commandLineExecutionAsString}";
                                shortTitle = string.Empty;
                            }
                            else
                            {
                                title = options.Title;
                                shortTitle = title;
                            }
                            TrySetTitle(title);
                            log.Configuration.Name = shortTitle;
                            if (options.LogFile != null)
                            {
                                log.Configuration.GetLogTarget<LogFile>().Enabled = true;
                                log.Configuration.GetLogTarget<LogFile>().File = options.LogFile;
                            }
                            GRYLibrary.Core.Log.GRYLogLogFormat format = default;
                            if (options.AddLogOverhead)
                            {
                                format = GRYLibrary.Core.Log.GRYLogLogFormat.GRYLogFormat;
                            }
                            else
                            {
                                format = GRYLibrary.Core.Log.GRYLogLogFormat.OnlyMessage;
                            }
                            foreach (GRYLibrary.Core.Log.GRYLogTarget target in log.Configuration.LogTargets)
                            {
                                target.Format = format;
                            }
                            log.Configuration.SetEnabledOfAllLogTargets(options.Verbosity != Verbosity.Quiet);
                            if (options.Verbosity == Verbosity.Verbose)
                            {
                                foreach (GRYLibrary.Core.Log.GRYLogTarget target in log.Configuration.LogTargets)
                                {
                                    target.LogLevels.Add(Microsoft.Extensions.Logging.LogLevel.Debug);
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
                                    throw new ArgumentException($"The specified working-directory '{options.Workingdirectory}' does not exist.");
                                }
                            }
                            string commandLineArguments = Utilities.GetCommandLineArguments();
                            DateTime startTime = DateTime.Now;
                            string startTimeAsString = startTime.ToString(log.Configuration.DateFormat);
                            log.Log($"{ProgramShortName} v{version} started at {startTimeAsString}", Microsoft.Extensions.Logging.LogLevel.Debug);
                            log.Log($"Execution-id: {executionId}", Microsoft.Extensions.Logging.LogLevel.Debug);
                            log.Log($"Argument: '{commandLineArguments}'", Microsoft.Extensions.Logging.LogLevel.Debug);
                            log.Log($"Start executing {commandLineExecutionAsString}", Microsoft.Extensions.Logging.LogLevel.Debug);
                            externalProgramExecutor = ExternalProgramExecutor.CreateByGRYLog(options.Program, argumentForExecution, log, workingDirectory, shortTitle, options.PrintErrorsAsInformation, options.TimeoutInMilliseconds);
                            externalProgramExecutor.RunSynchronously = !options.NotSynchronous;
                            externalProgramExecutor.ThrowErrorIfExitCodeIsNotZero = false;
                            int externalProgramExecutorStartReturnValue = externalProgramExecutor.Start();
                            void programExecutionResultHandler()
                            {
                                if (externalProgramExecutor.ProcessWasAbortedDueToTimeout)
                                {
                                    log.Log($"Execution with id {executionId} was aborted due to a timeout. (The timeout was set to {Utilities.DurationToUserFriendlyString(TimeSpan.FromMilliseconds(externalProgramExecutor.TimeoutInMilliseconds.Value))}).", Microsoft.Extensions.Logging.LogLevel.Warning);
                                    result = ExitCodeTimeout;
                                }
                                WriteToFile(options.StdOutFile, externalProgramExecutor.AllStdOutLines);
                                WriteToFile(options.StdErrFile, externalProgramExecutor.AllStdErrLines);
                                List<string> exitCodeFileContent = new List<string>();
                                if (options.Verbosity == Verbosity.Verbose)
                                {
                                    exitCodeFileContent.Add($"{startTimeAsString}: Executed {commandLineExecutionAsString} with execution-id {executionId} with exitcode");
                                }
                                exitCodeFileContent.Add(externalProgramExecutor.ExitCode.ToString());
                                WriteToFile(options.ExitCodeFile, exitCodeFileContent.ToArray());
                            }
                            if (!options.NotSynchronous)
                            {
                                programExecutionResultHandler();
                                result = externalProgramExecutor.ExitCode;
                            }
                            else
                            {
                                Task task = new Task(programExecutionResultHandler);
                                task.Start();
                                task.Wait();
                                result = externalProgramExecutorStartReturnValue;
                            }
                        }
                        catch (Exception exception)
                        {
                            log.Log($"Error in {ProgramShortName}.", exception);
                        }
                        if (externalProgramExecutor != null)
                        {
                            log.Log($"{ProgramShortName} finished.", Microsoft.Extensions.Logging.LogLevel.Debug);
                            log.Log($"Execution-id: {executionId}", Microsoft.Extensions.Logging.LogLevel.Debug);
                            if (externalProgramExecutor.ExecutionState == ExecutionState.Terminated)
                            {
                                log.Log($"Exitcode: {result}", Microsoft.Extensions.Logging.LogLevel.Debug);
                                log.Log($"Duration: {Utilities.DurationToUserFriendlyString(externalProgramExecutor.ExecutionDuration)}", Microsoft.Extensions.Logging.LogLevel.Debug);
                            }
                        }
                    }));
                }
            }
            catch (Exception exception)
            {
                System.Console.Error.WriteLine($"Fatal error occurred: {exception}");
                result = ExitCodeFatalErroroccurred;
            }
            return result;
        }

        private static void WriteHelp(ParserResult<Options> argumentParserResult)
        {
            System.Console.Out.WriteLine(HelpText.AutoBuild(argumentParserResult).ToString());
            System.Console.Out.WriteLine();
            System.Console.Out.WriteLine($"{ProgramShortName} ({nameof(ExternalProgramExecutionWrapper)}) is a tool to wrap program-calls with some useful functions like getting stdout, stderr, exitcode and the ability to set a timeout.");
            System.Console.Out.WriteLine();
            System.Console.Out.WriteLine($"For more information see the website of the {ProgramShortName}-project: {ProjectLink}");
            System.Console.Out.WriteLine($"{ProgramShortName} is mainly licensed under the terms of {LicenseName}. For the concrete license-text see {LicenseLink}");
            System.Console.Out.WriteLine();
            System.Console.Out.WriteLine($"Exitcodes:");
            System.Console.Out.WriteLine($"{ExitCodeNoProgramExecuted}: If no program was executed");
            System.Console.Out.WriteLine($"{ExitCodeFatalErroroccurred}: If a fatal error occurred");
            System.Console.Out.WriteLine($"{ExitCodeTimeout}: If the executed program was aborted due to the given timeout");
            System.Console.Out.WriteLine($"2147393881: If executed on MacOS (applies only to the pip-package)");
            System.Console.Out.WriteLine($"2147393882: If executed on an unknown OS (applies only to the pip-package)");
            System.Console.Out.WriteLine($"2147393883: If an (unexpected) exception occurred (applies only to the pip-package)");
            System.Console.Out.WriteLine($"If the executed program terminated then its exitcode will be set as exitcode of {ProgramShortName}.");
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

        private static void TrySetTitle(string title)
        {
            try
            {
                System.Console.Title = title;
            }
            catch
            {
                Utilities.NoOperation();
            }
        }

        private static void WriteToFile(string file, string[] lines)
        {
            if (!string.IsNullOrEmpty(file))
            {
                file = Utilities.ResolveToFullPath(file);
                Utilities.EnsureFileExists(file);
                File.AppendAllLines(file, lines, new UTF8Encoding(false));
            }
        }
    }
}
