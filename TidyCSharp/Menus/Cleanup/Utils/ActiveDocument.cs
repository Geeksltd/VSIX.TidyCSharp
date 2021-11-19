using EnvDTE;
using System.IO;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class ActiveDocument
    {
        public static bool IsValid(ProjectItem item)
        {
            if (item == null) return false;

            var path = item.Properties.Item("FullPath").Value.ToString();

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            return true;
        }
    }
}