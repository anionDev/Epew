using CommandLine;
using CommandLine.Text;
using GRYLibrary.Core.LogObject;
using GRYLibrary.Core.LogObject.ConcreteLogTargets;
using GRYLibrary.Core.Miscellaneous;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Epew
{
    public static class Program
    {
        internal const string ProgramName = "Epew";
        internal const string ProjectLink = "https://github.com/anionDev/Epew";
        internal const string LicenseName = "MIT";
        internal static readonly string Version = GetVersion();
        internal static readonly string LicenseLink = $"https://raw.githubusercontent.com/anionDev/Epew/v{Version}/License.txt";

        internal const int ExitCodeNoProgramExecuted = 2147393801;
        internal const int ExitCodeFatalErroroccurred = 2147393802;
        internal const int ExitCodeTimeout = 2147393803;
        internal static ExternalProgramExecutor _ExternalProgramExecutor = null;
        internal static GRYLog _Log = GRYLog.Create();
        private static readonly SentenceBuilder _SentenceBuilder = SentenceBuilder.Create();

        private static string _Title;

        internal static int Main(string[] arguments)
        {
            int result = ExitCodeNoProgramExecuted;
            string argument = Utilities.GetCommandLineArguments();
            string workingDirectory = Directory.GetCurrentDirectory();
            try
            {
                ParserResult<Options> argumentParserResult = new Parser(settings => settings.CaseInsensitiveEnumValues = true).ParseArguments<Options>(arguments);
                if (string.IsNullOrEmpty(argument))
                {
                    System.Console.WriteLine($"{ProgramName} v{Version}");
                    System.Console.WriteLine($"Run '{ProgramName} --help' to get help about the usage.");
                }
                else if (IsHelpCommand(argument))
                {
                    WriteHelp(argumentParserResult);
                }
                else
                {
                    if (argumentParserResult is Parsed<Options>)
                    {
                        argumentParserResult.WithParsed(options =>
                        {
                            result = ProcessArguments(options, result);
                        });
                    }
                    else if (argumentParserResult is NotParsed<Options> notParsed)
                    {
                        throw new ArgumentException($"Argument '{argument}' could not be parsed successfully. Errors: " + string.Join(",", notParsed.Errors.Select(error => $"\"{error.Tag}: {_SentenceBuilder.FormatError(error)}\"")));
                    }
                    else
                    {
                        throw new ArgumentException($"Argument '{argument}' resulted in a undefined parser-result.");
                    }
                }
            }
            catch (Exception exception)
            {
                _Log.Log($"Fatal error occurred while processing argument '{workingDirectory}> epew {argument}", exception);
                result = ExitCodeFatalErroroccurred;
            }
            return result;
        }
        private static int ProcessArguments(Options options, int initialExitCodeValue)
        {
            Guid executionId = Guid.NewGuid();
            int result = initialExitCodeValue;
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
                if (!string.IsNullOrWhiteSpace(options.LogFile))
                {
                    _Log.Configuration.GetLogTarget<LogFile>().Enabled = true;
                    _Log.Configuration.GetLogTarget<LogFile>().File = options.LogFile;
                }
                foreach (GRYLogTarget target in _Log.Configuration.LogTargets)
                {
                    target.Format = options.AddLogOverhead ? GRYLogLogFormat.GRYLogFormat : GRYLogLogFormat.OnlyMessage;
                }
                string commandLineArguments = Utilities.GetCommandLineArguments();
                _ExternalProgramExecutor = new ExternalProgramExecutor(options.Program, argumentForExecution, workingDirectory)
                {
                    LogObject = _Log,
                    LogNamespace = options.LogNamespace,
                    PrintErrorsAsInformation = options.PrintErrorsAsInformation,
                    TimeoutInMilliseconds = options.TimeoutInMilliseconds,
                    ThrowErrorIfExitCodeIsNotZero = false,
                    Verbosity = options.Verbosity
                };


                if (options.NotSynchronous)
                {
                    _ExternalProgramExecutor.StartAsynchronously();
                    new Task(() =>
                    {
                        _ExternalProgramExecutor.WaitUntilTerminated();
                        ProgramExecutionResultHandler(_ExternalProgramExecutor, options, executionId, commandLineExecutionAsString);
                    }).Start();
                    result = _ExternalProgramExecutor.ProcessId;
                }
                else
                {
                    _ExternalProgramExecutor.StartSynchronously();
                    result = ProgramExecutionResultHandler(_ExternalProgramExecutor, options, executionId, commandLineExecutionAsString);
                }
                WriteNumberToFile(options.Verbosity, executionId, _Title, commandLineExecutionAsString, _ExternalProgramExecutor.ProcessId, "process-id", options.ProcessIdFile);
            }
            catch (Exception exception)
            {
                _Log.Log($"Error in {ProgramName}.", exception);
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

        private static void WriteNumberToFile(Verbosity verbosity, Guid executionId, string title, string commandLineExecutionAsString, int value, string nameOfValue, string file)
        {
            List<string> fileContent = new();
            fileContent.Add(value.ToString());
            if (verbosity == Verbosity.Verbose)
            {
                fileContent.Add($"Execution '{title}' ('{commandLineExecutionAsString}') with execution-id {executionId} has {nameOfValue}");
            }
            WriteToFile(file, fileContent.ToArray());
        }

        private static int ProgramExecutionResultHandler(ExternalProgramExecutor externalProgramExecutor, Options options, Guid executionId, string commandLineExecutionAsString)
        {
            try
            {
                int result;
                if (externalProgramExecutor.ProcessWasAbortedDueToTimeout)
                {
                    result = ExitCodeTimeout;
                }
                else
                {
                    result = externalProgramExecutor.ExitCode;
                }
                WriteToFile(options.StdOutFile, externalProgramExecutor.AllStdOutLines);
                WriteToFile(options.StdErrFile, externalProgramExecutor.AllStdErrLines);
                WriteNumberToFile(options.Verbosity, executionId, _Title, commandLineExecutionAsString, result, "exit-code", options.ExitCodeFile);
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
            System.Console.Out.WriteLine($"{ProgramName} is a tool to wrap program-calls with some useful functions like getting stdout, stderr, exitcode and the ability to set a timeout and so on.");
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
