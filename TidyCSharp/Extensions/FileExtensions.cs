using EnvDTE;
using System;

namespace Geeks.GeeksProductivityTools.Extensions
{
    public static class FileExtensions
    {
        public static bool IsCsharpFile(this ProjectItem projectItem)
        {
            return projectItem.Kind == Constants.vsProjectItemKindPhysicalFile && projectItem.Name.EndsWith(".cs");
        }

        public static bool IsCSharpDesignerFile(this ProjectItem projectItem)
        {
            return projectItem.Kind == Constants.vsProjectItemKindPhysicalFile
                && projectItem.Name.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase);
        }
    }
}