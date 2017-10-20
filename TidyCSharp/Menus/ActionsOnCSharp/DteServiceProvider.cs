using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace Geeks.GeeksProductivityTools.Menus.ActionsOnCSharp
{
    public class DteServiceProvider
    {
        static DTE2 _dteServiceProvider;
        public static DTE2 Instance => _dteServiceProvider ?? ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
    }
}
