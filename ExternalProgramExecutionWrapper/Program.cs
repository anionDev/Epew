using GRYLibrary.Core;
using GRYLibrary.Core.Log.ConcreteLogTargets;
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
            System.Guid executionId = System.Guid.NewGuid();
            GRYLibrary.Core.Log.GRYLog log = GRYLibrary.Core.Log.GRYLog.Create();
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
                    System.Console.WriteLine("Usage: Commandline-arguments=Base64(\"ProgramPathAndFile;~Arguments;~WorkingDirectory;~Title;~PrintErrorsAsInformation;~LogFile;~TimeoutInMilliseconds;~Verbose;~AddLogOverhead\")");
                    return exitCode;
                }
                string decodedString = new UTF8Encoding(false).GetString(System.Convert.FromBase64String(commandLineArguments));
                string[] argumentsSplitted;
                if (decodedString.Contains(";~"))
                {
                    argumentsSplitted = decodedString.Split(new string[] { ";~" }, System.StringSplitOptions.None);
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
                try
                {
                    System.Console.Title = titleOfExecution;
                }
                catch
                {
                    GRYLibrary.Core.Utilities.NoOperation();
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
                        log.Configuration.GetLogTarget<Console>().LogLevels.Add(Microsoft.Extensions.Logging.LogLevel.Debug);
                        log.Configuration.GetLogTarget<Console>().LogLevels.Add(Microsoft.Extensions.Logging.LogLevel.Debug);
                    }
                }
                log.Log(line, Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log($"ExternalProgramExecutorWrapper v{version} started", Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log($"Execution-Id: {executionId}", Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log($"ExternalProgramExecutorWrapper-original-argument is '{commandLineArguments}'", Microsoft.Extensions.Logging.LogLevel.Debug);
                log.Log($"Start executing '{workingDirectory}>{programPathAndFile} {arguments}'", Microsoft.Extensions.Logging.LogLevel.Debug);
                ExternalProgramExecutor externalProgramExecutor = ExternalProgramExecutor.CreateByGRYLog(programPathAndFile, arguments, log, workingDirectory, titleOfExecution, printErrorsAsInformation, timeoutInMilliseconds);
                exitCode = externalProgramExecutor.StartConsoleApplicationInCurrentConsoleWindow();
            }
            catch (System.Exception exception)
            {
                log.Log("Error in ExternalProgramExecutionWrapper", exception);
            }
            log.Log("ExternalProgramExecutorWrapper finished", Microsoft.Extensions.Logging.LogLevel.Debug);
            log.Log($"Execution-Id: {executionId}", Microsoft.Extensions.Logging.LogLevel.Debug);
            log.Log($"Exit-code: {exitCode}", Microsoft.Extensions.Logging.LogLevel.Debug);
            log.Log(line, Microsoft.Extensions.Logging.LogLevel.Debug);
            return exitCode;
        }
    }
}
