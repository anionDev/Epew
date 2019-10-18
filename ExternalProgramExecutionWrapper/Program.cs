using System;
using System.Text;

namespace ExternalProgramExecutorWrapper
{
    internal class Program
    {
        /// <summary>
        /// Executes a program based on the given commandline arguments
        /// </summary>
        /// <remarks>
        /// Usage: Commandline-arguments=Base64("ProgramPathAndFile;~Arguments;~Title;~WorkingDirectory;~PrintErrorsAsInformation;~LogFile;~TimeoutInMilliseconds;~Verbose")
        /// The arguments PrintErrorsAsInformation and verbose are boolean values. Pass '1' to set them to true or anything else to set them to false.
        /// </remarks>
        /// <return>
        /// Returns the exitcode of the executed program.
        /// </return>
        internal static int Main()
        {
            int exitCode = -1;
            Console.WriteLine("ExternalProgramExecutorWrapper started");
            GRYLibrary.GRYLog log = GRYLibrary.GRYLog.Create();
            Guid executionId = Guid.NewGuid();
            try
            {
                string commandLineArguments = GRYLibrary.Utilities.GetCommandLineArguments();
                if (commandLineArguments.Equals(string.Empty)
                    || commandLineArguments.Equals("help")
                    || commandLineArguments.Equals("--help")
                    || commandLineArguments.Equals("-h")
                    || commandLineArguments.Equals("/help")
                    || commandLineArguments.Equals("/h"))
                {
                    Console.WriteLine("Usage: Commandline-arguments=Base64(\"ProgramPathAndFile;~Arguments;~Title;~WorkingDirectory;~PrintErrorsAsInformation;~LogFile;~TimeoutInMilliseconds;~Verbose\")");
                }
                string decodedString = new UTF8Encoding(false).GetString(Convert.FromBase64String(commandLineArguments));
                string[] argumentsSplitted = decodedString.Split(new string[] { ";~" }, StringSplitOptions.None);
                string programPathAndFile = argumentsSplitted[0].Trim();
                string arguments = argumentsSplitted[1].Trim();
                string titleOfExecution = argumentsSplitted[2].Trim();
                Console.Title = titleOfExecution;
                string workingDirectory = argumentsSplitted[3].Trim();
                bool printErrorsAsInformation = argumentsSplitted[4].Trim().Equals("1");
                string logFile = argumentsSplitted[5].Trim();
                if (string.IsNullOrEmpty(logFile))
                {
                    logFile = null;
                }

                int? timeoutInMilliseconds = null;
                string timeoutAsString = argumentsSplitted[6].Trim();
                if (!string.IsNullOrEmpty(timeoutAsString))
                {
                    timeoutInMilliseconds = int.Parse(timeoutAsString);
                }

                bool verbose = argumentsSplitted[7].Trim().Equals("1");

                log.Configuration.LogFile = logFile;
                if (verbose)
                {
                    log.Configuration.LoggedMessageTypesInConsole.Add(GRYLibrary.GRYLogLogLevel.Verbose);
                    log.Configuration.LoggedMessageTypesInLogFile.Add(GRYLibrary.GRYLogLogLevel.Verbose);
                }
                log.LogVerboseMessage("------------------------------------------");
                log.LogVerboseMessage("ExternalProgramExecutorWrapper started");
                log.LogVerboseMessage("Execution-Id: " + executionId);
                log.LogVerboseMessage("ExternalProgramExecutorWrapper-original-argument is '" + commandLineArguments + "'");
                log.LogVerboseMessage($"Start executing '{workingDirectory}>{programPathAndFile} {arguments}'");
                GRYLibrary.ExternalProgramExecutor externalProgramExecutor = GRYLibrary.ExternalProgramExecutor.CreateWithGRYLog(programPathAndFile, arguments, log, workingDirectory, titleOfExecution, printErrorsAsInformation, timeoutInMilliseconds);
                externalProgramExecutor.LogObject.Configuration.PrintOutputInConsole = true;
                externalProgramExecutor.LogObject.Configuration.WriteToLogFileIfLogFileIsAvailable = true;
                exitCode = externalProgramExecutor.StartConsoleApplicationInCurrentConsoleWindow();
            }
            catch (Exception exception)
            {
                string errorMessage = "Error in ExternalProgramExecutionWrapper";
                try
                {
                    log.LogError(errorMessage, exception);
                }
                catch
                {
                    Console.WriteLine(errorMessage + ": " + exception.ToString());
                }
            }
            log.LogVerboseMessage("ExternalProgramExecutorWrapper finished");
            log.LogVerboseMessage("Execution-Id: " + executionId);
            log.LogVerboseMessage("Exit-code: " + exitCode.ToString());
            log.LogVerboseMessage("------------------------------------------");
            return exitCode;
        }
    }
}
