using EnvDTE;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

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

        protected virtual SyntaxNode RefreshResult(SyntaxNode initialSourceNode)
        {
            var newDocument = ProjectItemDetails.ProjectItemDocument.WithSyntaxRoot(initialSourceNode);
            TidyCSharpPackage.Instance.RefreshSolution(newDocument.Project.Solution);
            ProjectItemDetails = new ProjectItemDetailsType(ProjectItemDetails.ProjectItem);
            return ProjectItemDetails.InitialSourceNode;
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
            TidyCSharpPackage.Instance.RefreshSolution(newDocument.Project.Solution);
        }

        public abstract SyntaxNode CleanUp(SyntaxNode initialSourceNode);

        public ICleanupOption Options { get; set; }

        public bool CheckOption(int? optionItem) => Options.Should(optionItem);

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
                    TidyCSharpPackage.Instance
                        .CleanupWorkingSolution
                        .Projects.FirstOrDefault(p => p.Name == projectItem.ContainingProject.Name)
                        ?.Documents.FirstOrDefault(d => d.FilePath == projectItem.ToFullPathPropertyValue());
            }
        }
    }
}