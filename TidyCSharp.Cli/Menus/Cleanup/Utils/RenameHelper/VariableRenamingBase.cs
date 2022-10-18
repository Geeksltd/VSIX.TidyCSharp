using Microsoft.CodeAnalysis;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper;

public abstract class VariableRenamingBase : CodeCleanerCommandRunnerBase
{
    private const string SelectedMethodAnnotation = "SELECTED_METHOD_ANNOTATION";

    private Document _workingDocument, _orginalDocument;

    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        var annotationForSelectedNode = new SyntaxAnnotation(SelectedMethodAnnotation);
        _orginalDocument = ProjectItemDetails.Document;
        _workingDocument = ProjectItemDetails.Document;

        if (_orginalDocument == null) return initialSourceNode;

        SyntaxNode workingNode;
        var annotatedRoot = initialSourceNode;

        do
        {
            workingNode = GetWorkingNode(annotatedRoot, annotationForSelectedNode);

            if (workingNode == null) continue;

            var annotatedNode = workingNode.WithAdditionalAnnotations(annotationForSelectedNode);
            annotatedRoot = annotatedRoot?.ReplaceNode(workingNode, annotatedNode);
            _workingDocument = _workingDocument.WithSyntaxRoot(annotatedRoot);
            annotatedRoot = await _workingDocument.GetSyntaxRootAsync();
            annotatedNode = annotatedRoot?.GetAnnotatedNodes(annotationForSelectedNode).FirstOrDefault();

            var rewriter = GetRewriter(_workingDocument);

            rewriter.Visit(annotatedNode);
            CollectMessages(rewriter.GetReport());
            _workingDocument = rewriter.WorkingDocument;
        } while (workingNode != null);

        return null;
    }

    protected override async Task SaveResultAsync(SyntaxNode initialSourceNode)
    {
        await base.SaveResultAsync((await _workingDocument.GetSyntaxTreeAsync()).GetRoot());
    }

    protected abstract SyntaxNode GetWorkingNode(SyntaxNode initialSourceNode, SyntaxAnnotation annotationForSelectedNodes);

    protected abstract VariableRenamingBaseRewriter GetRewriter(Document workingDocument);

    protected abstract class VariableRenamingBaseRewriter : CleanupCSharpSyntaxRewriter
    {
        public Document WorkingDocument { get; protected set; }
        protected VariableRenamingBaseRewriter(Document workingDocument, bool isReportOnlyMode, ICleanupOption options) : base(isReportOnlyMode, options)
        {
            WorkingDocument = workingDocument;
        }
    }
}