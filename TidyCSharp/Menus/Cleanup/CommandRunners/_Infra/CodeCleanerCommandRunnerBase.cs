using EnvDTE;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using System;
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
        public void Run(ProjectItem item) => Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory
                            .Run(async delegate
                            {
                                await RunAsync(item);
                            });

        public ProjectItemDetailsType ProjectItemDetails { get; protected set; }
        public ProjectItemDetailsType UNModifiedProjectItemDetails { get; protected set; }

        protected virtual async Task RunAsync(ProjectItem item)
        {
            ProjectItemDetails = new ProjectItemDetailsType(item);

            if (IsReportOnlyMode)
            {
                await RefreshResultAsync(item.ToSyntaxNode());
            }

            UNModifiedProjectItemDetails = ProjectItemDetails;

            var initialSourceNode = CleanUpAsync(ProjectItemDetails.InitialSourceNode);

            if (!IsReportOnlyMode)
            {
                try
                {
                    var result = await initialSourceNode;
                    await SaveResultAsync(result);
                }
                catch (Exception ex)
                {
                    ErrorNotification.WriteErrorToFile(ex);
                }
            }
        }

        protected async virtual Task<SyntaxNode> RefreshResultAsync(SyntaxNode initialSourceNode)
        {
            if (UNModifiedProjectItemDetails != null && UNModifiedProjectItemDetails.InitialSourceNode.ToFullString().Replace("\r", "")
                .Equals(initialSourceNode.ToFullString().Replace("\r", "")))
            {
                ProjectItemDetails = new ProjectItemDetailsType(ProjectItemDetails.ProjectItem);
                return ProjectItemDetails.InitialSourceNode;
            }

            if (ProjectItemDetails.ProjectItemDocument != null)
            {
                var newDocument = ProjectItemDetails.ProjectItemDocument.WithSyntaxRoot(initialSourceNode);
                await TidyCSharpPackage.Instance.RefreshSolutionAsync(newDocument.Project.Solution);
            }

            ProjectItemDetails = new ProjectItemDetailsType(ProjectItemDetails.ProjectItem);
            return ProjectItemDetails.InitialSourceNode;
        }

        protected virtual async Task SaveResultAsync(SyntaxNode initialSourceNode)
        {
            if (initialSourceNode == null || initialSourceNode == ProjectItemDetails.InitialSourceNode) return;

            if (UNModifiedProjectItemDetails.InitialSourceNode.ToFullString().Replace("\r", "")
                .Equals(initialSourceNode.ToFullString().Replace("\r", "")))
                return;

            if (ProjectItemDetails.ProjectItemDocument == null)
            {
                initialSourceNode.WriteSourceTo(ProjectItemDetails.FilePath);
                return;
            }

            var newDocument = ProjectItemDetails.ProjectItemDocument.WithText(initialSourceNode.GetText());
            await TidyCSharpPackage.Instance.RefreshSolutionAsync(newDocument.Project.Solution);
        }

        public virtual async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode) => null;

        public bool IsEquivalentToUNModified(SyntaxNode initialSourceNode)
        {
            return initialSourceNode.IsEquivalentTo(UNModifiedProjectItemDetails.InitialSourceNode);
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

            using (var textWriter = XmlWriter.Create(Path.GetDirectoryName(App.Dte.Solution.FullName) + "\\Tidy.log", xmlWriterSettings))
            {
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
            }
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