using CommandLine;
using GRYLibrary.Core;
using GRYLibrary.Core.Log.ConcreteLogTargets;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ExternalProgramExecutionWrapper
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

        [Option('r', nameof(RunAsAdministrator), Required = false, HelpText = "Run program as administrator", Default = false)]
        public bool RunAsAdministrator { get; set; }

        [Option('h', nameof(AddLogOverhead), Required = false, HelpText = "Add log overhead", Default = true)]
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

    }
    public enum Verbosity
    {
        Quiet = 0,
        Normal = 1,
        Verbose = 2,
    }

    internal class Program
    {
        internal static int Main(string[] args)
        {
            int exitCode = -1;
            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                string line = "--------------------------------------------------------------------";
                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                Guid executionId = Guid.NewGuid();
                GRYLibrary.Core.Log.GRYLog log = GRYLibrary.Core.Log.GRYLog.Create();
                log.Configuration.ResetToDefaultValues();
                ExternalProgramExecutor externalProgramExecutor = null;
                try
                {
                    string argument;
                    if (options.ArgumentIsBase64Encoded)
                    {
                        argument = new UTF8Encoding(false).GetString(Convert.FromBase64String(options.Argument));
                    }
                    else
                    {
                        argument = options.Argument;
                    }
                    string commandLineExecutionAsString = $"'{options.Workingdirectory}>{options.Program} {argument}'";
                    string title;
                    string shortTitle;
                    if (string.IsNullOrWhiteSpace(options.Title))
                    {
                        title = nameof(ExternalProgramExecutor) + ": " + commandLineExecutionAsString;
                        shortTitle = string.Empty;
                    }
                    else
                    {
                        title = options.Title;
                        shortTitle = title;
                    }
                    TrySetTitle(title);
                    log.Configuration.Name = title;
                    if (options.LogFile != null)
                    {
                        log.Configuration.GetLogTarget<LogFile>().Enabled = true;
                        log.Configuration.GetLogTarget<LogFile>().File = options.LogFile;
                    }
                    if (options.AddLogOverhead)
                    {
                        log.Configuration.Format = GRYLibrary.Core.Log.GRYLogLogFormat.GRYLogFormat;
                    }
                    else
                    {
                        log.Configuration.Format = GRYLibrary.Core.Log.GRYLogLogFormat.OnlyMessage;
                    }
                    log.Configuration.SetEnabledOfAllLogTargets(options.Verbosity != Verbosity.Quiet);
                    if (options.Verbosity == Verbosity.Verbose)
                    {
                        foreach (GRYLibrary.Core.Log.GRYLogTarget target in log.Configuration.LogTargets)
                        {
                            log.Configuration.GetLogTarget<GRYLibrary.Core.Log.ConcreteLogTargets.Console>().LogLevels.Add(Microsoft.Extensions.Logging.LogLevel.Debug);
                            log.Configuration.GetLogTarget<GRYLibrary.Core.Log.ConcreteLogTargets.Console>().LogLevels.Add(Microsoft.Extensions.Logging.LogLevel.Debug);
                        }
                    }
                    string commandLineArguments = Utilities.GetCommandLineArguments();
                    log.Log(line, Microsoft.Extensions.Logging.LogLevel.Debug);
                    DateTime startTime = DateTime.Now;
                    string startTimeAsString = startTime.ToString(log.Configuration.DateFormat);
                    log.Log($"{nameof(ExternalProgramExecutor)} v{version} started at " + startTimeAsString, Microsoft.Extensions.Logging.LogLevel.Debug);
                    log.Log($"Execution-Id: {executionId}", Microsoft.Extensions.Logging.LogLevel.Debug);
                    log.Log($"Argument: '{commandLineArguments}'", Microsoft.Extensions.Logging.LogLevel.Debug);
                    log.Log($"Start executing {commandLineExecutionAsString}", Microsoft.Extensions.Logging.LogLevel.Debug);
                    externalProgramExecutor = ExternalProgramExecutor.CreateByGRYLog(options.Program, argument, log, options.Workingdirectory, shortTitle, options.PrintErrorsAsInformation, options.TimeoutInMilliseconds);
                    externalProgramExecutor.RunAsAdministrator = options.RunAsAdministrator;
                    externalProgramExecutor.ThrowErrorIfExitCodeIsNotZero = false;
                    exitCode = externalProgramExecutor.StartConsoleApplicationInCurrentConsoleWindow();
                    WriteToFile(options.StdOutFile, externalProgramExecutor.AllStdOutLines);
                    WriteToFile(options.StdErrFile, externalProgramExecutor.AllStdErrLines);

                    List<string> exitCodeFileContent = new List<string>();
                    if (options.Verbosity == Verbosity.Verbose)
                    {
                        exitCodeFileContent.Add($"{startTimeAsString}: Started {commandLineExecutionAsString} with exitcode");
                    }
                    exitCodeFileContent.Add(externalProgramExecutor.ExitCode.ToString());
                    WriteToFile(options.ExitCodeFile, exitCodeFileContent.ToArray());
                }
                catch (Exception exception)
                {
                    log.Log("Error in " + nameof(ExternalProgramExecutor), exception);
                }
                if (externalProgramExecutor != null)
                {
                    log.Log(nameof(ExternalProgramExecutor) + " finished", Microsoft.Extensions.Logging.LogLevel.Debug);
                    log.Log($"Execution-Id: {executionId}", Microsoft.Extensions.Logging.LogLevel.Debug);
                    if (externalProgramExecutor.ExecutionState == ExecutionState.Terminated)
                    {
                        log.Log($"Exit-code: {exitCode}", Microsoft.Extensions.Logging.LogLevel.Debug);
                        log.Log($"Duration: {Utilities.DurationToUserFriendlyString(externalProgramExecutor.ExecutionDuration)}", Microsoft.Extensions.Logging.LogLevel.Debug);
                    }
                    log.Log(line, Microsoft.Extensions.Logging.LogLevel.Debug);
                }
            });
            return exitCode;
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
                System.IO.File.AppendAllLines(file, lines, new UTF8Encoding(false));
            }
        }
    }
}
