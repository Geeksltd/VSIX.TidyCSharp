namespace Geeks.GeeksProductivityTools.Utils
{
    public class ProcessActions
    {
        public static void GeeksProductivityToolsProcess()
        {
            return;
            var visualStudioProcesses = System.Diagnostics.Process.GetProcessesByName("devenv");
            foreach (var process in visualStudioProcesses)
            {
                if (!process.MainWindowTitle.Contains("GeeksProductivityTools")) continue;

                process.Kill();
                break;
            }
        }
    }
}
