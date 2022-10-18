using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners.CamelCasedMethodVariable.Option;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;
using TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.CamelCasedMethodVariable;

public class CamelCasedLocalVariable : VariableRenamingBase
{
    protected override SyntaxNode GetWorkingNode(SyntaxNode initialSourceNode, SyntaxAnnotation annotationForSelectedNodes)
    {
        return
            initialSourceNode
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => !m.HasAnnotation(annotationForSelectedNodes));
    }

    protected override VariableRenamingBaseRewriter GetRewriter(Document workingDocument)
    {
        return new Rewriter(workingDocument, IsReportOnlyMode, Options);
    }

    private class Rewriter : VariableRenamingBaseRewriter
    {
        public Rewriter(Document workingDocument, bool isReportOnlyMode, ICleanupOption options) : base(workingDocument, isReportOnlyMode, options) { }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            MethodDeclarationSyntax methodDeclarationSyntax = null;

            Task.Run(async delegate
                {
                    methodDeclarationSyntax = await RenameDeclarationsAsync(node);
                }).GetAwaiter().GetResult();

            return base.VisitMethodDeclaration(methodDeclarationSyntax as MethodDeclarationSyntax);
        }

        private async Task<MethodDeclarationSyntax> RenameDeclarationsAsync(MethodDeclarationSyntax methodNode)
        {
            if (CheckOption((int)CleanupTypes.LocalVariable))
            {
                var renamingResult = await new VariableRenamer(WorkingDocument).RenameDeclarationsAsync(methodNode);

                if (renamingResult != null && renamingResult.Node != null)
                {
                    methodNode = renamingResult.Node as MethodDeclarationSyntax;
                    WorkingDocument = renamingResult.Document;
                }

                if (renamingResult != null)
                {
                    var lineSpan = methodNode.GetFileLinePosSpan();

                    AddReport(new ChangesReport(methodNode)
                    {
                        LineNumber = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                        Message = "Camel Cased Methods",
                        Generator = nameof(CamelCasedLocalVariable)
                    });
                }
            }

            if (CheckOption((int)CleanupTypes.MethodParameter))
            {
                var renamingResult = await new ParameterRenamer(WorkingDocument).RenameDeclarationsAsync(methodNode);

                if (renamingResult != null && renamingResult.Node != null)
                {
                    methodNode = renamingResult.Node as MethodDeclarationSyntax;
                    WorkingDocument = renamingResult.Document;
                }

                if (renamingResult != null)
                {
                    var lineSpan = methodNode.GetFileLinePosSpan();

                    AddReport(new ChangesReport(methodNode)
                    {
                        LineNumber = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                        Message = "Camel Cased Methods",
                        Generator = nameof(CamelCasedLocalVariable)
                    });
                }
            }

            return methodNode;
        }
    }
}