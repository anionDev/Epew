using Epew.Core.Helper;
using Epew.Core.Runner;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Epew.Tests.Testcases
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void Echo()
        {
            // arrange
            string output = "test";
            string[] arguments = new string[] { "--Program", "echo2", "--Argument", output };
            ProgramStarter pe = new ProgramStarter();

            // act
            int result = pe.Main(arguments);

            // assert
            Assert.AreEqual(0, result);
            Assert.IsNotNull(pe.Result);
            Assert.IsTrue(pe.Result is RunWithArgumentsFromCLI);
            RunWithArgumentsFromCLI resultRunner = (RunWithArgumentsFromCLI)pe.Result;
            Assert.AreEqual(0, resultRunner._ExternalProgramExecutor.ExitCode);
            Assert.AreEqual(1, resultRunner._ExternalProgramExecutor.AllStdOutLines.Length);
            Assert.AreEqual(output, resultRunner._ExternalProgramExecutor.AllStdOutLines[0]);
            Assert.AreEqual(0, resultRunner._ExternalProgramExecutor.AllStdErrLines.Length);
        }
    }
}