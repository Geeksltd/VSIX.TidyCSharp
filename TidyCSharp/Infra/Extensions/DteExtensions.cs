namespace GeeksAddin
{
    using System;
    using System.Linq;
    using EnvDTE;
    using EnvDTE80;
    using Geeks.GeeksProductivityTools;

    static class DteExtensions
    {
        public const string SolutionItemsFolder = "Solution Items";

        public static Project GetSolutionItemsProject()
        {
            var solution = App.DTE.Solution as Solution2;
            return solution.Projects
                           .OfType<Project>()
                           .FirstOrDefault(p => p.Name.Equals(SolutionItemsFolder, StringComparison.OrdinalIgnoreCase))
                      ?? solution.AddSolutionFolder(SolutionItemsFolder);
        }
    }
}
