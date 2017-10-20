using EnvDTE;

namespace Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp
{
    public class CSharpActionDelegate
    {
        public delegate void TargetAction(ProjectItem item, Definition.CodeCleanerType[] actionType, bool fileWindowMustBeOpend = false);
    }
}
