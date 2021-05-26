using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using VSOLEInterop = Microsoft.VisualStudio.OLE.Interop;

namespace Geeks.VSIX.TidyCSharp.Extensions
{
    class RunCodeAnalysisRulesCommand 
    {
        private bool _overallBuildSuccess;
        private bool _customBuildInProgress;

        private void CustomBuild_MenuItemCallback(object sender, EventArgs e)
        {
            // Listen to the necessary build events.
            DTE2 dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
            dte.Events.BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
            dte.Events.BuildEvents.OnBuildProjConfigDone += BuildEvents_OnBuildProjConfigDone;

            try
            {
                // Build the active project.
                _customBuildInProgress = true;
                dte.ExecuteCommand("Build.BuildSelection");
            }
            catch (COMException)
            {
                _customBuildInProgress = false;
                WriteToOutputWindow("Build", "Could not determine project to build from selection");
            }
        }

        private void BuildEvents_OnBuildProjConfigDone(string project, string projectConfig, string platform, string solutionConfig, bool success)
        {
            // Ignore this build event if we didn't start it.
            if (!_customBuildInProgress)
            {
                return;
            }

            // Keep track of the overall build success.
            _overallBuildSuccess = success;
        }

        private void BuildEvents_OnBuildDone(EnvDTE.vsBuildScope scope, EnvDTE.vsBuildAction action)
        {
            // Ignore this build event if we didn't start it.
            if (!_customBuildInProgress)
            {
                return;
            }

            _customBuildInProgress = false;

            if (_overallBuildSuccess)
            {
                // Launch the debugger.
                DTE2 dte = (DTE2)Package.GetGlobalService(typeof(SDTE));
                dte.ExecuteCommand("Debug.Start");
            }
            else
            {
                WriteToOutputWindow("Build", "Custom build failed.");
            }
        }

        private void WriteToOutputWindow(string paneName, string message)
        {
            DTE2 dte = (DTE2)Package.GetGlobalService(typeof(SDTE));

            Window window = dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
            OutputWindow outputWindow = (OutputWindow)window.Object;

            OutputWindowPane targetPane = outputWindow.OutputWindowPanes.Cast<OutputWindowPane>()
                .FirstOrDefault(x => x.Name.ToLower() == paneName.ToLower());

            if (targetPane == null)
            {
                targetPane = outputWindow.OutputWindowPanes.Add(paneName);
            }

            targetPane.Activate();
            outputWindow.ActivePane.OutputString(message);
            outputWindow.ActivePane.OutputString(Environment.NewLine);
        }
    }
}
