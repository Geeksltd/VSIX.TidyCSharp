using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;

namespace Geeks.GeeksProductivityTools.Utils
{
    public class SolutionActions
    {
        public static List<Project> FindProjects(DTE2 dteServiceProvider)
        {
            var projects = dteServiceProvider.Solution.Projects;
            var result = new List<Project>();
            var item = projects.GetEnumerator();

            while (item.MoveNext())
            {
                var project = item.Current as Project;
                if (project == null) continue;

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                    result.AddRange(GetSolutionFolderProjects(project));

                else result.Add(project);
            }

            return result;
        }

        static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            try
            {
                if (solutionFolder.ProjectItems == null) return null;

                var result = new List<Project>();

                for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
                {
                    var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                    if (subProject == null) continue;

                    // another solution folder
                    if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                    {
                        var solutionFolderProjects = GetSolutionFolderProjects(subProject);

                        if (solutionFolder != null)
                            result.AddRange(GetSolutionFolderProjects(subProject));
                    }

                    else result.Add(subProject);
                }

                return result;
            }
            catch (Exception e)
            {
                ErrorNotification.EmailError(e);
                ProcessActions.GeeksProductivityToolsProcess();
                return null;
            }
        }
    }
}