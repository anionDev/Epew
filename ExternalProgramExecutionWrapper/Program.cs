using System;
using System.Text;

namespace PrivateMaintenance.ExternalProgramExecutorWrapper
{
    internal class Program
    {
        /// <summary>
        /// Executes a program based on the given commandline arguments
        /// </summary>
        /// <remarks>
        /// Usage: Commandline-arguments=Base64("ProgramPathAndFile;~Arguments;~Title;~WorkingDirectory;~PrintErrorsAsInformation;~LogFile;~timeoutInMilliseconds")
        /// </remarks>
        /// <return>
        /// Returns the exitcode of the executed program.
        /// </return>
        internal static int Main()
        {
            int result = -21;
            bool debugMode = false;
            Console.WriteLine("ExternalProgramExecutorWrapper started");
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
                    Console.WriteLine("Usage: Commandline-arguments=Base64(\"ProgramPathAndFile;~Arguments;~Title;~WorkingDirectory;~PrintErrorsAsInformation;~LogFile;~timeoutInMilliseconds\")");
                }
                string decodedString = new UTF8Encoding(false).GetString(Convert.FromBase64String(commandLineArguments));
                string[] argumentsSplitted = decodedString.Split(new string[] { ";~" }, StringSplitOptions.None);
                if (argumentsSplitted.Length >= 8 && argumentsSplitted[7] == "1")
                {
                    debugMode = true;
                }
                if (debugMode)
                {
                    Console.WriteLine(decodedString);
                }
                string programPathAndFile = argumentsSplitted[0].Trim();
                string arguments = argumentsSplitted[1].Trim();
                string titleOfExecution = argumentsSplitted[2].Trim();
                Console.Title = titleOfExecution;
                string workingDirectory = argumentsSplitted[3].Trim();
                bool printErrorsAsInformation = argumentsSplitted[4].Trim().Equals("1");
                string logFile = argumentsSplitted[5].Trim();
                if (string.IsNullOrWhiteSpace(logFile))
                {
                    logFile = null;
                }
                int? timeoutInMilliseconds = null;
                if (argumentsSplitted.Length > 5)
                {
                    string timeoutAsString = argumentsSplitted[6].Trim();
                    if (!string.IsNullOrWhiteSpace(timeoutAsString))
                    {
                        timeoutInMilliseconds = int.Parse(timeoutAsString);
                    }
                }
                GRYLibrary.ExternalProgramExecutor externalProgramExecutor = null;
                externalProgramExecutor = GRYLibrary.ExternalProgramExecutor.CreateByLogFile(programPathAndFile, arguments, logFile, workingDirectory, titleOfExecution, printErrorsAsInformation, timeoutInMilliseconds);
                externalProgramExecutor.LogObject.Configuration.PrintOutputInConsole = true;
                externalProgramExecutor.LogObject.Configuration.WriteToLogFileIfLogFileIsAvailable = true;
                result = externalProgramExecutor.StartConsoleApplicationInCurrentConsoleWindow();
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error in ExternalProgramExecutionWrapper: " + exception.Message);
            }
            Console.WriteLine($"ExternalProgramExecutionWrapper finfished with exitcode {result}");
            if (debugMode)
            {
                Console.ReadLine();
            }
            return result;
        }
    }
}
