using CommandLine;
using CommandLine.Text;
using GRYLibrary.Core.LogObject;
using GRYLibrary.Core.LogObject.ConcreteLogTargets;
using GRYLibrary.Core.Miscellaneous;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Epew.Epew.Core
{
    internal static class Program
    {
        internal static int Main(string[] arguments)
        {
            return EpewLibrary.Core.Program.Main(arguments);
        }
    }
}
