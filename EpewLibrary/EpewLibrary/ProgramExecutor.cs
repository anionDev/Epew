﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using GRYLibrary.Core.Log;
using GRYLibrary.Core.Log.ConcreteLogTargets;
using GRYLibrary.Core.Miscellaneous;
using GRYLibrary.Core.Miscellaneous.ExecutePrograms;
using GRYLibrary.Core.Miscellaneous.ExecutePrograms.WaitingStates;
using System.IO;
using System.Reflection;


namespace Epew.EpewLibrary.Core
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
        private ExternalProgramExecutor _ExternalProgramExecutor = null;
        internal string Version { get; private set; }
        internal string LicenseLink { get; private set; }

        public int Main(string[] arguments)
        {
            int result = ExitCodeNoProgramExecuted;
            try
            {
                Version = GetVersion();
                LicenseLink = $"https://raw.githubusercontent.com/anionDev/Epew/v{Version}/License.txt";
                _SentenceBuilder = SentenceBuilder.Create();
                if (arguments == null)
                {
                    throw Utilities.CreateNullReferenceExceptionDueToParameter(nameof(arguments));
                }
                string argumentsAsString = String.Join(' ', arguments);
                _Log = GRYLog.Create();
                string workingDirectory = Directory.GetCurrentDirectory();
                try
                {
                    if (arguments.Length == 0)
                    {
                        _Log.Log($"{ProgramName} v{Version}");
                        _Log.Log($"Run '{ProgramName} --help' to get help about the usage.");
                    }
                    else
                    {
                        ParserResult<EpewOptions> parserResult = new Parser(settings => settings.CaseInsensitiveEnumValues = true).ParseArguments<EpewOptions>(arguments);
                        if (IsHelpCommand(arguments))
                        {
                            WriteHelp(parserResult);
                        }
                        else
                        {
                            parserResult.WithParsed(options =>
                            {
                                result = HandleSuccessfullyParsedArguments(options);
                            })
                            .WithNotParsed(errors =>
                            {
                                HandleParsingErrors(argumentsAsString, errors);
                            });
                            return result;
                        }
                    }
                }
                catch (Exception exception)
                {
                    _Log.Log($"Fatal error occurred while processing argument '{workingDirectory}> epew {argumentsAsString}", exception);
                }

            }
            catch (Exception exception)
            {
                System.Console.Error.WriteLine($"Fatal error occurred", exception.ToString());
                result = ExitCodeFatalErroroccurred;
            }
            return result;
        }

        private void HandleParsingErrors(string argumentsAsString, IEnumerable<Error> errors)
        {
            var amountOfErrors = errors.Count();
            _Log.Log($"Argument '{argumentsAsString}' could not be parsed successfully.", Microsoft.Extensions.Logging.LogLevel.Error);
            if (0 < amountOfErrors)
            {
                _Log.Log($"The following error{(amountOfErrors == 1 ? string.Empty : "s")} occurred:", Microsoft.Extensions.Logging.LogLevel.Error);
                foreach (var error in errors)
                {
                    _Log.Log($"{error.Tag}: {_SentenceBuilder.FormatError(error)}", Microsoft.Extensions.Logging.LogLevel.Error);
                }
            }
        }

        private int HandleSuccessfullyParsedArguments(EpewOptions options)
        {
            bool elevate = options.ElevatePrivileges && !Utilities.IsAdministrator();
            Guid executionId = Guid.NewGuid();
            int result = ExitCodeNoProgramExecuted;
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


                if (elevate)
                {
                    _ExternalProgramExecutor = new ExternalProgramExecutor("EpewAdmin",
                        ExternalProgramExecutor.CreateEpewArgumentString(options.Program, options.Argument, options.Workingdirectory,
                        options.PrintErrorsAsInformation, options.TimeoutInMilliseconds, options.Verbosity,
                        options.AddLogOverhead, options.LogFile, options.Title, GetWaitingState(options), options.LogNamespace, options.ElevatePrivileges),
                        options.Workingdirectory);
                }
                else
                {
                    _ExternalProgramExecutor = new ExternalProgramExecutor(new ExternalProgramExecutorConfiguration()
                    {
                        Program = options.Program,
                    });
                }

                _ExternalProgramExecutor.Run();

                WriteNumberToFile(options.Verbosity, executionId, _Title, commandLineExecutionAsString, _ExternalProgramExecutor.ProcessId, "process-id", options.ProcessIdFile);
                if (options.NotSynchronous)
                {
                    return 2147393804;
                }
                else
                {
                    new Task(() =>
                    {
                        _ExternalProgramExecutor.WaitUntilTerminated();
                        ProgramExecutionResultHandler(_ExternalProgramExecutor, options, executionId, commandLineExecutionAsString);
                    }).Start();
                    result = _ExternalProgramExecutor.ExitCode;
                }
            }
            catch (Exception exception)
            {
                _Log.Log($"Error in {ProgramName}.", exception);
            }
            return result;
        }

        private static WaitingState GetWaitingState(EpewOptions options)
        {
            WaitingState result;
            if (options.NotSynchronous)
            {
                result = new RunAsynchronously();
            }
            else
            {
                result = new RunSynchronously()
                {
                    ThrowErrorIfExitCodeIsNotZero = false
                };
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

        private int ProgramExecutionResultHandler(ExternalProgramExecutor externalProgramExecutor, EpewOptions options, Guid executionId, string commandLineExecutionAsString)
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

        private void WriteHelp(ParserResult<EpewOptions> argumentParserResult)
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

        private static bool IsHelpCommand(string[] arguments)
        {
            foreach (var argument in arguments)
            {
                string argumentLower = argument.ToLower();
                if (argumentLower.Equals("--help")
                    || argumentLower.Equals("-h")
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