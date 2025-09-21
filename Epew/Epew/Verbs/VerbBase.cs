namespace Epew.Core.Verbs
{
    public abstract class VerbBase
    {
        public abstract void Accept(IVerbBaseVisitor visitor);
        public abstract T Accept<T>(IVerbBaseVisitor<T> visitor);
    }
    public interface IVerbBaseVisitor
    {
        void Handle(RunCLI runWithOptionsFromCLI);
        void Handle(RunFile runWithOptionsFromFile);
    }
    public interface IVerbBaseVisitor<T>
    {
        T Handle(RunCLI runWithOptionsFromCLI);
        T Handle(RunFile runWithOptionsFromFile);
    }
}
