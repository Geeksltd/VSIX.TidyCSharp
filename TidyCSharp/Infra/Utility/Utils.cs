using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using EnvDTE;
using EnvDTE80;
using Geeks.GeeksProductivityTools;
using Microsoft.VisualStudio.Shell.Interop;
using Geeks.GeeksProductivityTools.Menus.Cleanup;

namespace GeeksAddin
{
    public static class Utils
    {
        //public static string GetSolutionName(DTE2 app)
        //{
        //    if (app == null || app.Solution == null || string.IsNullOrEmpty(app.Solution.FullName)) return "";
        //    return Path.GetFileNameWithoutExtension(app.Solution.FullName);
        //}

        public static string[] FindSolutionDirectories(DTE2 app)
        {
            var basePaths = new List<string>();

            if (app.Solution != null)
            {
                for (var i = 1; i <= app.Solution.Projects.Count; i++)
                {
                    var projectItem = app.Solution.Projects.Item(i);
                    AddPathFromProjectItem(basePaths, projectItem);
                }

                return basePaths.ToArray();
            }

            app.StatusBar.Text = "No solution or project is identified. app.Solution is " +
                (app.Solution?.GetType().Name).Or("NULL");

            App.DTE = (DTE2)TidyCSharpPackage.GetGlobalService(typeof(SDTE));

            return null;
        }

        static void AddPathFromProjectItem(List<string> basePaths, Project projectItem)
        {
            if (projectItem == null) return;

            try
            {
                // Project
                var projectFileName = projectItem.FileName;

                if (!string.IsNullOrWhiteSpace(projectFileName))
                {
                    if (projectItem.Properties.Item("FullPath").Value is string fullPath)
                        basePaths.Add(fullPath);
                }
                else
                {
                    // Folder
                    for (var i = 1; i <= projectItem.ProjectItems.Count; i++)
                        AddPathFromProjectItem(basePaths, projectItem.ProjectItems.Item(i).Object as Project);
                }
            }
            catch (Exception err)
            {
                ErrorNotification.EmailError(err);
            }
        }
    }
}
