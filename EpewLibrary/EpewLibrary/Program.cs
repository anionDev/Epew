using CommandLine;
using CommandLine.Text;
using GRYLibrary.Core.Log;
using GRYLibrary.Core.Log.ConcreteLogTargets;
using GRYLibrary.Core.Miscellaneous;
using GRYLibrary.Core.Miscellaneous.ExecutePrograms;
using GRYLibrary.Core.Miscellaneous.ExecutePrograms.WaitingStates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Epew.EpewLibrary.Core
{
    public static class Program
    {

        public static int Main(string[] arguments)
        {
            return new ProgramExecutor().Main(arguments);
        }
    }
}
