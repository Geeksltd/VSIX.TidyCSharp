using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public abstract class VariableRenamingBase : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        const string SELECTED_METHOD_ANNOTATION = "SELECTED_METHOD_ANNOTATION";

        Document WorkingDocument, orginalDocument;

        public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
        {
            var annotationForSelectedNode = new SyntaxAnnotation(SELECTED_METHOD_ANNOTATION);
            orginalDocument = ProjectItemDetails.ProjectItemDocument;
            WorkingDocument = ProjectItemDetails.ProjectItemDocument;

            if (orginalDocument == null) return initialSourceNode;

            SyntaxNode workingNode;
            var annotatedRoot = initialSourceNode;

            do
            {
                workingNode = GetWorkingNode(annotatedRoot, annotationForSelectedNode);

                if (workingNode == null) continue;

                var annotatedNode = workingNode.WithAdditionalAnnotations(annotationForSelectedNode);
                annotatedRoot = annotatedRoot.ReplaceNode(workingNode, annotatedNode);
                WorkingDocument = WorkingDocument.WithSyntaxRoot(annotatedRoot);
                annotatedRoot = await WorkingDocument.GetSyntaxRootAsync();
                annotatedNode = annotatedRoot.GetAnnotatedNodes(annotationForSelectedNode).FirstOrDefault();

                var rewriter = GetRewriter(WorkingDocument);

                rewriter.Visit(annotatedNode);
                CollectMessages(rewriter.GetReport());
                WorkingDocument = rewriter.WorkingDocument;
            } while (workingNode != null);

            return null;
        }

        protected override async Task SaveResultAsync(SyntaxNode initialSourceNode)
        {
            if (WorkingDocument is null) return;

            var text = await WorkingDocument.GetTextAsync();

            if (string.Compare(text?.ToString(), ProjectItemDetails.InitialSourceNode.GetText().ToString(), false) != 0)
            {
                await TidyCSharpPackage.Instance.RefreshSolutionAsync(WorkingDocument.Project.Solution);
            }
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
}