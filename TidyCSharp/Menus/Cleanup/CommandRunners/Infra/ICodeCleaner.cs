using EnvDTE;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public interface ICodeCleaner
    {
        void Run(ProjectItem item);
    }
}
