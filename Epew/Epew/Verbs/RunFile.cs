using CommandLine;

namespace Epew.Core.Verbs
{
    [Verb(nameof(RunFile), HelpText = "Runs a program using the arguments specified in a file.")]
    public class RunFile:VerbBase
    {
        [Option('f', nameof(File), Required = true, HelpText = "File which contains the commandline-arguments")]
        public string File { get; set; }
        public override void Accept(IVerbBaseVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(IVerbBaseVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
    }
}
