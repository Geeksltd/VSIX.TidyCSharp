using EnvDTE;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public abstract class CodeCleanerCommandRunnerBase : ICodeCleaner
    {
        static IList<ChangesReport> ChangesReports;
        public bool IsReportOnlyMode { get; set; }

        static CodeCleanerCommandRunnerBase()
        {
            ChangesReports = ChangesReports ?? new List<ChangesReport>();
        }
        public void Run(ProjectItem item) => AsyncRun(item);

        public ProjectItemDetailsType ProjectItemDetails { get; protected set; }
        public ProjectItemDetailsType UnModifiedProjectItemDetails { get; protected set; }

        protected virtual async void AsyncRun(ProjectItem item)
        {
            ProjectItemDetails = new ProjectItemDetailsType(item);

            if (IsReportOnlyMode)
            {
                RefreshResult(item.ToSyntaxNode());
            }

            UnModifiedProjectItemDetails = ProjectItemDetails;

            var initialSourceNode = CleanUp(ProjectItemDetails.InitialSourceNode);

            if (!IsReportOnlyMode)
                SaveResult(await initialSourceNode);
        }

        protected virtual SyntaxNode RefreshResult(SyntaxNode initialSourceNode)
        {
            if (UnModifiedProjectItemDetails != null && UnModifiedProjectItemDetails.InitialSourceNode.ToFullString().Replace("\r", "")
                .Equals(initialSourceNode.ToFullString().Replace("\r", "")))
            {
                ProjectItemDetails = new ProjectItemDetailsType(ProjectItemDetails.ProjectItem);
                return ProjectItemDetails.InitialSourceNode;
            }

            if (ProjectItemDetails.ProjectItemDocument != null)
            {
                var newDocument = ProjectItemDetails.ProjectItemDocument.WithSyntaxRoot(initialSourceNode);
                TidyCSharpPackage.Instance.RefreshSolution(newDocument.Project.Solution);
            }

            ProjectItemDetails = new ProjectItemDetailsType(ProjectItemDetails.ProjectItem);
            return ProjectItemDetails.InitialSourceNode;
        }

        protected virtual async Task SaveResult(SyntaxNode initialSourceNode)
        {
            if (initialSourceNode == null || initialSourceNode == ProjectItemDetails.InitialSourceNode) return;

            if (UnModifiedProjectItemDetails.InitialSourceNode.ToFullString().Replace("\r", "")
                .Equals(initialSourceNode.ToFullString().Replace("\r", "")))
                return;

            if (ProjectItemDetails.ProjectItemDocument == null)
            {
                initialSourceNode.WriteSourceTo(ProjectItemDetails.FilePath);
                return;
            }

            var newDocument = ProjectItemDetails.ProjectItemDocument.WithText(initialSourceNode.GetText());
            TidyCSharpPackage.Instance.RefreshSolution(newDocument.Project.Solution);
        }

        public virtual async Task<SyntaxNode> CleanUp(SyntaxNode initialSourceNode) => null;

        public bool IsEquivalentToUnModified(SyntaxNode initialSourceNode)
        {
            return initialSourceNode.IsEquivalentTo(UnModifiedProjectItemDetails.InitialSourceNode);
        }

        public void CollectMessages(params ChangesReport[] changesReports)
        {
            foreach (var report in changesReports)
                ChangesReports.Add(report);
        }

        public static void GenerateMessages()
        {
            var xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;

            var textWriter = XmlWriter.Create(Path.GetDirectoryName(App.DTE.Solution.FullName)
                + "\\Tidy.log", xmlWriterSettings);

            textWriter.WriteStartDocument();
            textWriter.WriteStartElement("Reports");

            foreach (var change in ChangesReports)
            {
                textWriter.WriteStartElement("Report");
                textWriter.WriteAttributeString("Generator", change.Generator);
                textWriter.WriteElementString("LineNumber", change.LineNumber.ToString());
                textWriter.WriteElementString("Column", change.Column.ToString());
                textWriter.WriteElementString("FileName", change.FileName);
                textWriter.WriteElementString("Message", change.Message);
                textWriter.WriteEndElement();
            }

            textWriter.WriteEndElement();
            textWriter.WriteEndDocument();
            textWriter.Flush();
            textWriter.Close();
            ChangesReports.Clear();
        }

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
                    // if (_projectItemDocument == null)
                    // {
                    _projectItemDocument = GetRoslynDomuentByProjectItem(ProjectItem);
                    // }

                    return _projectItemDocument;
                }
            }

            SemanticModel _semanticModel;
            public SemanticModel SemanticModel
            {
                get
                {
                    if (_semanticModel == null && ProjectItemDocument != null)
                    {
                        Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory
                            .Run(async delegate
                        {
                            _semanticModel = await ProjectItemDocument.GetSemanticModelAsync();
                        });
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

                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory
                    .Run(async delegate
                {
                    InitialSourceNode = ProjectItemDocument != null ? await ProjectItemDocument.GetSyntaxRootAsync() : item.ToSyntaxNode();
                });
            }
            static RoslynDocument GetRoslynDomuentByProjectItem(ProjectItem projectItem)
            {
                var path = projectItem.ToFullPathPropertyValue();

                var document =
                    TidyCSharpPackage.Instance
                        .CleanupWorkingSolution
                        .Projects.FirstOrDefault(p => p.Name == projectItem.ContainingProject.Name)
                        ?.Documents.FirstOrDefault(d => d.FilePath == path);

                if (document == null)
                {
                    return
                        TidyCSharpPackage.Instance
                       .CleanupWorkingSolution.GetDocument(
                            TidyCSharpPackage.Instance
                           .CleanupWorkingSolution.GetDocumentIdsWithFilePath(path)
                           .FirstOrDefault());
                }

                return document;
            }
        }
    }
}