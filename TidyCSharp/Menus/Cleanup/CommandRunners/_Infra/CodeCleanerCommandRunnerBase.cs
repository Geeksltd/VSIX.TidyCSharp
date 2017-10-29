using System.Linq;
using EnvDTE;
using Microsoft.CodeAnalysis;
using RoslynDocument = Microsoft.CodeAnalysis.Document;
using System;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public abstract class CodeCleanerCommandRunnerBase : ICodeCleaner
    {
        public void Run(ProjectItem item) => AsyncRun(item);

        public ProjectItemDetailsType ProjectItemDetails { get; protected set; }

        protected virtual void AsyncRun(ProjectItem item)
        {
            ProjectItemDetails = new ProjectItemDetailsType(item);

            var initialSourceNode = CleanUp(ProjectItemDetails.InitialSourceNode);

            SaveResult(initialSourceNode);
        }

        protected virtual void SaveResult(SyntaxNode initialSourceNode)
        {
            if (initialSourceNode == null || initialSourceNode == ProjectItemDetails.InitialSourceNode) return;

            if (ProjectItemDetails.ProjectItemDocument == null)
            {
                initialSourceNode.WriteSourceTo(ProjectItemDetails.FilePath);
                return;
            }
            var newDocument = ProjectItemDetails.ProjectItemDocument.WithText(initialSourceNode.GetText());

            GeeksProductivityToolsPackage.Instance.RefreshSolution(newDocument.Project.Solution);
        }
        public abstract SyntaxNode CleanUp(SyntaxNode initialSourceNode);

        public ICleanupOption Options { get; set; }

        protected bool CheckOption(int? optionItem)
        {
            if (Options == null) return true;
            if (Options.CleanupItemsInteger == null) return true;
            if (optionItem == null) return true;

            return (Options.CleanupItemsInteger & optionItem) == optionItem;
        }

        public class ProjectItemDetailsType
        {
            public ProjectItem ProjectItem { get; private set; }

            RoslynDocument _projectItemDocument;
            public RoslynDocument ProjectItemDocument
            {
                get
                {
                    if (_projectItemDocument == null)
                    {
                        _projectItemDocument = GetRoslynDomuentByProjectItem(ProjectItem);
                    }
                    return _projectItemDocument;
                }
            }

            SemanticModel _semanticModel;
            public SemanticModel SemanticModel
            {
                get
                {
                    if (_semanticModel == null)
                    {
                        _semanticModel = ProjectItemDocument.GetSemanticModelAsync().Result;
                    }
                    return _semanticModel;
                }
            }
            public SyntaxNode InitialSourceNode { get; private set; }

            public string FilePath { get; private set; }

            public ProjectItemDetailsType(ProjectItem item)
            {
                FilePath = item.ToFullPathPropertyValue();
                ProjectItem = item;
                InitialSourceNode = ProjectItemDocument != null ? ProjectItemDocument.GetSyntaxRootAsync().Result : item.ToSyntaxNode();
            }
            static RoslynDocument GetRoslynDomuentByProjectItem(ProjectItem projectItem)
            {
                return
                    GeeksProductivityToolsPackage.Instance
                        .CleanupWorkingSolution
                        .Projects.FirstOrDefault(p => p.Name == projectItem.ContainingProject.Name)
                        ?.Documents.FirstOrDefault(d => d.FilePath == projectItem.ToFullPathPropertyValue());
            }
        }
    }
}