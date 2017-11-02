using EnvDTE;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class CodeCleaner
    {
        //TODO: By Alireza =>  To return Syntax node and pass syntaxNode no next clean up function and dont close windows for each cleanup , just for something like organize usings
        //public ICodeCleaner Cleaner { get; private set; }
        ICodeCleaner Cleaner;
        ProjectItem Item;

        public CodeCleaner(ICodeCleaner cleaner, ProjectItem item)
        {
            Cleaner = cleaner;
            Item = item;
        }

        public void Run() => Cleaner.Run(Item);
    }
}
