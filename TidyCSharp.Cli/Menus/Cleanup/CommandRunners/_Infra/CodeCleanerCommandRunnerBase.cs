using System.Xml;
using Microsoft.CodeAnalysis;
using TidyCSharp.Cli.Environment;
using TidyCSharp.Cli.Menus.Cleanup.Utils;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;

public abstract class CodeCleanerCommandRunnerBase : ICodeCleaner
{
    private static IList<ChangesReport> _changesReports;
    public bool IsReportOnlyMode { get; set; }

    static CodeCleanerCommandRunnerBase()
    {
        _changesReports ??= new List<ChangesReport>();
    }

    public ProjectItemDetailsType ProjectItemDetails { get; private set; }
    public ProjectItemDetailsType UnModifiedProjectItemDetails { get; protected set; }

    public virtual async Task RunAsync(Document item)
    {
        ProjectItemDetails = new ProjectItemDetailsType(item);

        if (IsReportOnlyMode)
        {
            await RefreshResultAsync(item.ToSyntaxNode());
        }

        UnModifiedProjectItemDetails = ProjectItemDetails;

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
                ErrorNotification.ErrorNotification.WriteErrorToFile(ex);
            }
        }
    }

    protected async virtual Task<SyntaxNode> RefreshResultAsync(SyntaxNode initialSourceNode)
    {
        if (UnModifiedProjectItemDetails != null && UnModifiedProjectItemDetails.InitialSourceNode.ToFullString().Replace("\r", "")
                .Equals(initialSourceNode.ToFullString().Replace("\r", "")))
        {
            ProjectItemDetails = new ProjectItemDetailsType(ProjectItemDetails.Document);
            return ProjectItemDetails.InitialSourceNode;
        }

        //if (ProjectItemDetails.Document != null)
        {
            var newDocument = ProjectItemDetails.Document.WithSyntaxRoot(initialSourceNode);
            ProjectItemDetails.UpdateDocument(newDocument);

            // await TidyCSharpPackage.Instance.RefreshSolutionAsync(newDocument.Project.Solution);
        }

        ProjectItemDetails = new ProjectItemDetailsType(ProjectItemDetails.Document);
        return ProjectItemDetails.InitialSourceNode;
    }

    protected virtual async Task SaveResultAsync(SyntaxNode initialSourceNode)
    {
        if (initialSourceNode == null || initialSourceNode == ProjectItemDetails.InitialSourceNode) return;

        if (UnModifiedProjectItemDetails.InitialSourceNode.ToFullString().Replace("\r", "")
            .Equals(initialSourceNode.ToFullString().Replace("\r", "")))
            return;

        //if (ProjectItemDetails.Document == null)
        {
            initialSourceNode.WriteSourceTo(ProjectItemDetails.FilePath);
            //return;
        }

        var newDocument = ProjectItemDetails.Document.WithText(initialSourceNode.GetText());
        ProjectItemDetails.UpdateDocument(newDocument);
        UnModifiedProjectItemDetails.UpdateDocument(newDocument);
        //await TidyCSharpPackage.Instance.RefreshSolutionAsync(newDocument.Project.Solution);
    }

    public virtual async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode) => null;

    public bool IsEquivalentToUnModified(SyntaxNode initialSourceNode)
    {
        return initialSourceNode.IsEquivalentTo(UnModifiedProjectItemDetails.InitialSourceNode);
    }

    public void CollectMessages(params ChangesReport[] changesReports)
    {
        foreach (var report in changesReports)
            _changesReports.Add(report);
    }

    public static void GenerateMessages(string outputPath)
    {
        var xmlWriterSettings = new XmlWriterSettings();
        xmlWriterSettings.Indent = true;

        using (var textWriter = XmlWriter.Create(outputPath, xmlWriterSettings))
        {
            textWriter.WriteStartDocument();
            textWriter.WriteStartElement("Reports");

            foreach (var change in _changesReports)
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
        _changesReports.Clear();
    }

    public ICleanupOption Options { get; set; }

    public bool CheckOption(int? optionItem) => Options.Should(optionItem);

    public class ProjectItemDetailsType
    {
        public Document Document { get; private set; }

        private SemanticModel _semanticModel;
        public SemanticModel SemanticModel
        {
            get
            {
                if (_semanticModel == null && Document != null)
                {
                    Task.Run(async delegate
                        {
                            _semanticModel = await Document.GetSemanticModelAsync();
                        }).GetAwaiter().GetResult();
                }

                return _semanticModel;
            }
        }
        public SyntaxNode InitialSourceNode { get; private set; }

        public string FilePath { get; private set; }

        public ProjectItemDetailsType(Document item)
        {
            FilePath = item.ToFullPathPropertyValue();
            Document = item;

            Task.Run(async delegate
                {
                    InitialSourceNode = item != null ? await item.GetSyntaxRootAsync() : item.ToSyntaxNode();
                }).GetAwaiter().GetResult();
        }

        public void UpdateDocument(Document document)=>Document=document;
    }
}