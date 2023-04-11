using Epew.Core;
using GRYLibrary.Core.Miscellaneous.Event;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

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
            string[] arguments = new string[] { "-p", "echo2", "-a", output };
            var pe = new ProgramExecutor();

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