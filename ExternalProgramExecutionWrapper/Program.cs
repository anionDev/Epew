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
        /// Usage: Commandline-arguments=Base64("ProgramPathAndFile;~Arguments;~Title;~WorkingDirectory;~PrintErrorsAsInformation;~LogFile;~TimeoutInMilliseconds;~Verbose;~AddLogOverhead")
        /// The arguments PrintErrorsAsInformation and verbose are boolean values. Pass '1' to set them to true or anything else to set them to false.
        /// </remarks>
        /// <return>
        /// Returns the exitcode of the executed program.
        /// </return>
        internal static int Main()
        {
            int exitCode = -1;
            string line = "----------------------------------";
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Guid executionId = Guid.NewGuid();
            GRYLibrary.GRYLog log = GRYLibrary.GRYLog.Create();
            try
            {
                string commandLineArguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1)).Trim();
                if (commandLineArguments.Equals(string.Empty)
                    || commandLineArguments.Equals("help")
                    || commandLineArguments.Equals("--help")
                    || commandLineArguments.Equals("-h")
                    || commandLineArguments.Equals("/help")
                    || commandLineArguments.Equals("/h"))
                {
                    Console.WriteLine($"ExternalProgramExecutorWrapper v{version}");
                    Console.WriteLine("Usage: Commandline-arguments=Base64(\"ProgramPathAndFile;~Arguments;~WorkingDirectory;~Title;~PrintErrorsAsInformation;~LogFile;~TimeoutInMilliseconds;~Verbose;~AddLogOverhead\")");
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
                    titleOfExecution = $"{workingDirectory}>{programPathAndFile} {arguments}";
                }

                bool printErrorsAsInformation;
                if (argumentsSplitted.Length >= 5)
                {
                    printErrorsAsInformation = argumentsSplitted[4].Trim().Equals("1");
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
                    verbose = argumentsSplitted[7].Trim().Equals("1");
                }
                else
                {
                    verbose = true;
                }

                bool addLogOverhead;
                if (argumentsSplitted.Length >= 9)
                {
                    addLogOverhead = argumentsSplitted[8].Trim().Equals("1");
                }
                else
                {
                    addLogOverhead = true;
                }

                Console.Title = titleOfExecution;
                if (logFile != null)
                {
                    log.Configuration.WriteToLogFileIfLogFileIsAvailable = true;
                    log.Configuration.CreateLogFileIfRequiredAndIfPossible = true;
                    log.Configuration.LogFile = logFile;
                }
                if (addLogOverhead)
                {
                    log.Configuration.Format = GRYLibrary.GRYLogLogFormat.GRYLogFormat;
                }
                else
                {
                    log.Configuration.Format = GRYLibrary.GRYLogLogFormat.OnlyMessage;
                }
                log.Configuration.PrintOutputInConsole = true;
                log.Configuration.WriteToLogFileIfLogFileIsAvailable = true;
                if (verbose)
                {
                    log.Configuration.LoggedMessageTypesInConsole.Add(GRYLibrary.GRYLogLogLevel.Verbose);
                    log.Configuration.LoggedMessageTypesInLogFile.Add(GRYLibrary.GRYLogLogLevel.Verbose);
                }
                log.Log(line, GRYLibrary.GRYLogLogLevel.Verbose);
                log.Log($"ExternalProgramExecutorWrapper v{version} started", GRYLibrary.GRYLogLogLevel.Verbose);
                log.Log($"Execution-Id: {executionId}", GRYLibrary.GRYLogLogLevel.Verbose);
                log.Log($"ExternalProgramExecutorWrapper-original-argument is '{commandLineArguments}'", GRYLibrary.GRYLogLogLevel.Verbose);
                log.Log($"Start executing '{workingDirectory}>{programPathAndFile} {arguments}'", GRYLibrary.GRYLogLogLevel.Verbose);
                GRYLibrary.ExternalProgramExecutor externalProgramExecutor = GRYLibrary.ExternalProgramExecutor.CreateWithGRYLog(programPathAndFile, arguments, log, workingDirectory, titleOfExecution, printErrorsAsInformation, timeoutInMilliseconds);
                exitCode = externalProgramExecutor.StartConsoleApplicationInCurrentConsoleWindow();
            }
            catch (Exception exception)
            {
                log.Log("Error in ExternalProgramExecutionWrapper", exception);
            }
            log.Log("ExternalProgramExecutorWrapper finished", GRYLibrary.GRYLogLogLevel.Verbose);
            log.Log($"Execution-Id: {executionId}", GRYLibrary.GRYLogLogLevel.Verbose);
            log.Log($"Exit-code: {exitCode}", GRYLibrary.GRYLogLogLevel.Verbose);
            log.Log(line, GRYLibrary.GRYLogLogLevel.Verbose);
            return exitCode;
        }
    }
}
