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
        /// Usage: Commandline-arguments=Base64("ProgramPathAndFile;~Arguments;~Title;~WorkingDirectory;~PrintErrorsAsInformation;~LogFile;~TimeoutInMilliseconds;~Verbose")
        /// The arguments PrintErrorsAsInformation and verbose are boolean values. Pass '1' to set them to true or anything else to set them to false.
        /// </remarks>
        /// <return>
        /// Returns the exitcode of the executed program.
        /// </return>
        internal static int Main()
        {
            int exitCode = -1;
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            GRYLibrary.GRYLog log = GRYLibrary.GRYLog.Create();
            Guid executionId = Guid.NewGuid();
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
                    Console.WriteLine("ExternalProgramExecutorWrapper v" + version);
                    Console.WriteLine("Usage: Commandline-arguments=Base64(\"ProgramPathAndFile;~Arguments;~Title;~WorkingDirectory;~PrintErrorsAsInformation;~LogFile;~TimeoutInMilliseconds;~Verbose\")");
                    return exitCode;
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
                log.Log("------------------------------------------", GRYLibrary.GRYLogLogLevel.Verbose);
                log.Log("ExternalProgramExecutorWrapper v" + version + " started", GRYLibrary.GRYLogLogLevel.Verbose);
                log.Log("Execution-Id: " + executionId, GRYLibrary.GRYLogLogLevel.Verbose);
                log.Log("ExternalProgramExecutorWrapper-original-argument is '" + commandLineArguments + "'", GRYLibrary.GRYLogLogLevel.Verbose);
                log.Log($"Start executing '{workingDirectory}>{programPathAndFile} {arguments}'", GRYLibrary.GRYLogLogLevel.Verbose);
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
                    log.Log(errorMessage, exception);
                }
                catch
                {
                    Console.WriteLine(errorMessage + ": " + exception.ToString());
                }
            }
            log.Log("ExternalProgramExecutorWrapper finished", GRYLibrary.GRYLogLogLevel.Verbose);
            log.Log("Execution-Id: " + executionId, GRYLibrary.GRYLogLogLevel.Verbose);
            log.Log("Exit-code: " + exitCode.ToString(), GRYLibrary.GRYLogLogLevel.Verbose);
            log.Log("------------------------------------------");
            return exitCode;
        }
    }
}
