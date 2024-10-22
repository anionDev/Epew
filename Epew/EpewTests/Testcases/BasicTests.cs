using Epew.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Epew.Tests.Testcases
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void Run()
        {
            // arrange
            string output = "test";
            string[] arguments = new string[] { "--Program", "echo2", "--Argument", output };
            ProgramExecutor pe = new ProgramExecutor();

            // act
            int result = pe.Main(arguments);

            // assert
            Assert.AreEqual(0, result);
            Assert.AreEqual(0, pe._ExternalProgramExecutor.ExitCode);
            Assert.AreEqual(1, pe._ExternalProgramExecutor.AllStdOutLines.Length);
            Assert.AreEqual(output, pe._ExternalProgramExecutor.AllStdOutLines[0]);
            Assert.AreEqual(0, pe._ExternalProgramExecutor.AllStdErrLines.Length);
        }
    }
}