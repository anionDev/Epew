using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Epew.EpewLibrary.Tests.Testcases
{
    [TestClass]
    public class BasicTests
    {
        [TestMethod]
        public void RunWithEmptyArgumentList()
        {
            Assert.AreEqual(2147393802, Core.Program.Main(new string[] {}));//TODO improve tests
        }
    }
}