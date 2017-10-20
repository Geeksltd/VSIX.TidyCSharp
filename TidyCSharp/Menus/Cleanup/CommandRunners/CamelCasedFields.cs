using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class CamelCasedFields : VariableRenamingBase, ICodeCleaner
    {
        protected override SyntaxNode GetWorkingNode(SyntaxNode initialSourceNode, SyntaxAnnotation annotationForSelectedNodes)
        {
            return
                initialSourceNode
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(m => !m.HasAnnotation(annotationForSelectedNodes));
        }

        protected override VariableRenamingBaseRewriter GetRewriter(Document workingDocument)
        {
            return new Rewriter(workingDocument);
        }

        class Rewriter : VariableRenamingBaseRewriter
        {
            public Rewriter(Document workingDocument) : base(workingDocument) { }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                return base.VisitClassDeclaration(RenameDeclarations(node) as ClassDeclarationSyntax);
            }

            ClassDeclarationSyntax RenameDeclarations(ClassDeclarationSyntax classNode)
            {
                var renamingResult = new FieldRenamer(WorkingDocument).RenameDeclarations(classNode);
                if (renamingResult != null)
                {
                    classNode = renamingResult.Node as ClassDeclarationSyntax;
                    WorkingDocument = renamingResult.Document;
                }

                return classNode;
            }
        }
    }
}