using GRYLibrary.Core;
using GRYLibrary.Core.Log.ConcreteLogTargets;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ExternalProgramExecutorWrapper
{
    internal class Program
    {
        /// <summary>
        /// Executes a program based on the given commandline arguments
        /// </summary>
        /// <remarks>
        /// Usage: Commandline-arguments=Base64("ProgramPathAndFile;~Arguments;~Title;~WorkingDirectory;~PrintErrorsAsInformation;~LogFile;~TimeoutInMilliseconds;~Verbose;~AddLogOverhead;~outputFileForStdOut;~outputFileForStdErr")
        /// The arguments PrintErrorsAsInformation and verbose are boolean values. Pass '1' to set them to true or anything else to set them to false.
        /// </remarks>
        /// <return>
        /// Returns the exitcode of the executed program. If an unexpected error occurred so that no program will be executed then the returncode is -1.
        /// </return>
        internal static int Main()
        {
            int exitCode = -1;
            string line = "----------------------------------";
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Guid executionId = Guid.NewGuid();
            GRYLibrary.Core.Log.GRYLog log = GRYLibrary.Core.Log.GRYLog.Create();
            ExternalProgramExecutor externalProgramExecutor = null;
            try
            {
                string commandLineArguments = string.Join(" ", System.Environment.GetCommandLineArgs().Skip(1)).Trim();
                if (commandLineArguments.Equals(string.Empty)
                    || commandLineArguments.Equals("help")
                    || commandLineArguments.Equals("--help")
                    || commandLineArguments.Equals("-h")
                    || commandLineArguments.Equals("/help")
                    || commandLineArguments.Equals("/h"))
                {
                    System.Console.WriteLine($"ExternalProgramExecutorWrapper v{version}");
                    System.Console.WriteLine("Usage: Commandline-arguments=Base64(\"ProgramPathAndFile;~Arguments;~WorkingDirectory;~Title;~PrintErrorsAsInformation;~LogFile;~TimeoutInMilliseconds;~Verbose;~AddLogOverhead;~outputFileForStdOut;~outputFileForStdErr\")");
                    return exitCode;
                }
                string decodedString = new UTF8Encoding(false).GetString(Convert.FromBase64String(commandLineArguments));
                string[] argumentsSplitted;
                if (decodedString.Contains(";~"))
                {
                    argumentsSplitted = decodedString.Split(new string[] { ";~" }, StringSplitOptions.None);
                }
                else
                {
                    argumentsSplitted = new string[] { decodedString };
                }

                string programPathAndFile = argumentsSplitted[0].Trim();

                string arguments;
                if (argumentsSplitted.Length >= 2)
                {
                    arguments = argumentsSplitted[1].Trim();
                }
                else
                {
                    arguments = string.Empty;
                }

                string workingDirectory;
                if (argumentsSplitted.Length >= 3)
                {
                    workingDirectory = argumentsSplitted[2].Trim();
                }
                else
                {
                    workingDirectory = System.IO.Directory.GetCurrentDirectory();
                }

                string titleOfExecution;
                if (argumentsSplitted.Length >= 4)
                {
                    titleOfExecution = argumentsSplitted[3].Trim();
                }
                else
                {
                    titleOfExecution = string.Empty;
                }

                bool printErrorsAsInformation;
                if (argumentsSplitted.Length >= 5)
                {
                    printErrorsAsInformation = Utilities.StringToBoolean(argumentsSplitted[4]);
                }
                else
                {
                    printErrorsAsInformation = false;
                }

                string logFile = null;
                if (argumentsSplitted.Length >= 6)
                {
                    string trimmedArgument = argumentsSplitted[5].Trim();
                    if (!string.IsNullOrEmpty(trimmedArgument))
                    {
                        logFile = trimmedArgument;
                    }
                }

                int? timeoutInMilliseconds;
                if (argumentsSplitted.Length >= 7)
                {
                    timeoutInMilliseconds = null;
                    string timeoutAsString = argumentsSplitted[6].Trim();
                    if (!string.IsNullOrEmpty(timeoutAsString))
                    {
                        timeoutInMilliseconds = int.Parse(timeoutAsString);
                    }
                }
                else
                {
                    timeoutInMilliseconds = int.MaxValue;
                }

                bool verbose;
                if (argumentsSplitted.Length >= 8)
                {
                    verbose = Utilities.StringToBoolean(argumentsSplitted[7]);
                }
                else
                {
                    verbose = true;
                }

                bool addLogOverhead;
                if (argumentsSplitted.Length >= 9)
                {
                    addLogOverhead = Utilities.StringToBoolean(argumentsSplitted[8]);
                }
                else
                {
                    addLogOverhead = true;
                }

                string outputFileForStdOut;
                if (argumentsSplitted.Length >= 10)
                {
                    outputFileForStdOut = argumentsSplitted[9];
                }
                else
                {
                    outputFileForStdOut = null;
                }

                string outputFileForStdErr;
                if (argumentsSplitted.Length >= 11)
                {
                    outputFileForStdErr = argumentsSplitted[10];
                }
                else
                {
                    outputFileForStdErr = null;
                }
                if (!string.IsNullOrWhiteSpace(titleOfExecution))
                {
                    try
                    {
                        System.Console.Title = titleOfExecution;
                    }
                    catch
                    {
                        Utilities.NoOperation();
                    }
                    log.Configuration.Name = titleOfExecution;
                }
                if (logFile != null)
                {
                    log.Configuration.GetLogTarget<LogFile>().Enabled = true;
                    log.Configuration.GetLogTarget<LogFile>().File = logFile;
                }
                if (addLogOverhead)
                {
                    log.Configuration.Format = GRYLibrary.Core.Log.GRYLogLogFormat.GRYLogFormat;
                }
                else
                {
                    log.Configuration.Format = GRYLibrary.Core.Log.GRYLogLogFormat.OnlyMessage;
                }
                if (verbose)
                {
                    foreach (GRYLibrary.Core.Log.GRYLogTarget target in log.Configuration.LogTargets)
                    {
                        log.Configuration.GetLogTarget<GRYLibrary.Core.Log.ConcreteLogTargets.Console>().LogLevels.Add(Microsoft.Extensions.Logging.LogLevel.Debug);
                        log.Configuration.GetLogTarget<GRYLibrary.Core.Log.ConcreteLogTargets.Console>().LogLevels.Add(Microsoft.Extensions.Logging.LogLevel.Debug);
                    }
                }
                log.Log(line, Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log($"ExternalProgramExecutorWrapper v{version} started", Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log("Raw input: " + commandLineArguments, Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log("Decoded input: " + decodedString, Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log($"Execution-Id: {executionId}", Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log($"ExternalProgramExecutorWrapper-original-argument is '{commandLineArguments}'", Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log($"Start executing '{workingDirectory}>{programPathAndFile} {arguments}'", Microsoft.Extensions.Logging.LogLevel.Debug);
                externalProgramExecutor = ExternalProgramExecutor.CreateByGRYLog(programPathAndFile, arguments, log, workingDirectory, titleOfExecution, printErrorsAsInformation, timeoutInMilliseconds);
                exitCode = externalProgramExecutor.StartConsoleApplicationInCurrentConsoleWindow();
                WriteToFile(outputFileForStdOut, externalProgramExecutor.AllStdOutLines);
                WriteToFile(outputFileForStdErr, externalProgramExecutor.AllStdErrLines);
            }
            catch (Exception exception)
            {
                log.Log("Error in ExternalProgramExecutionWrapper", exception);
            }
            if (externalProgramExecutor != null)
            {
                log.Log("ExternalProgramExecutorWrapper finished", Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log($"Execution-Id: {executionId}", Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log($"Exit-code: {exitCode}", Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log($"Duration: {Utilities.DurationToUserFriendlyString(externalProgramExecutor.ExecutionDuration)}", Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log(line, Microsoft.Extensions.Logging.LogLevel.Debug);
            }
            return exitCode;
        }

        private static void WriteToFile(string file, string[] lines)
        {
            file = Utilities.ResolveToFullPath(file);
            Utilities.EnsureFileExists(file);
            System.IO.File.WriteAllLines(file, lines, new UTF8Encoding(false));
        }
    }
}
