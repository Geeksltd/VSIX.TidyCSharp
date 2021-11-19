using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class CamelCasedLocalVariable : VariableRenamingBase, ICodeCleaner
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
        class Rewriter : VariableRenamingBaseRewriter
        {
            public Rewriter(Document workingDocument, bool isReportOnlyMode, ICleanupOption options) : base(workingDocument, isReportOnlyMode, options) { }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                MethodDeclarationSyntax methodDeclarationSyntax = null;

                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory
                    .Run(async delegate
                {
                    methodDeclarationSyntax = await RenameDeclarations(node);
                });

                return base.VisitMethodDeclaration(methodDeclarationSyntax as MethodDeclarationSyntax);
            }

            async Task<MethodDeclarationSyntax> RenameDeclarations(MethodDeclarationSyntax methodNode)
            {
                if (CheckOption((int)CamelCasedMethodVariable.CleanupTypes.Local_variable))
                {
                    var renamingResult = await new VariableRenamer(WorkingDocument).RenameDeclarations(methodNode);

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

                if (CheckOption((int)CamelCasedMethodVariable.CleanupTypes.Method_Parameter))
                {
                    var renamingResult = await new ParameterRenamer(WorkingDocument).RenameDeclarations(methodNode);

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
}