using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
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
            return new Rewriter(workingDocument);
        }
        class Rewriter : VariableRenamingBaseRewriter
        {
            public Rewriter(Document workingDocument) : base(workingDocument) { }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                return base.VisitMethodDeclaration(RenameDeclarations(node) as MethodDeclarationSyntax);
            }

            MethodDeclarationSyntax RenameDeclarations(MethodDeclarationSyntax methodNode)
            {
                var renamingResult = new VariableRenamer(WorkingDocument).RenameDeclarations(methodNode);
                if (renamingResult != null)
                {
                    methodNode = renamingResult.Node as MethodDeclarationSyntax;
                    WorkingDocument = renamingResult.Document;
                }

                renamingResult = new ParameterRenamer(WorkingDocument).RenameDeclarations(methodNode);
                if (renamingResult != null)
                {
                    methodNode = renamingResult.Node as MethodDeclarationSyntax;
                    WorkingDocument = renamingResult.Document;
                }

                return methodNode;
            }
        }
    }
}