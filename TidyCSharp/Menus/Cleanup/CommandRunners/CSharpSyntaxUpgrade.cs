using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class CSharpSyntaxUpgrade : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return ChangeMethodHelper(initialSourceNode);
        }

        SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode)
        {
            initialSourceNode = new NewExpressionRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            return initialSourceNode;
        }

        class NewExpressionRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public NewExpressionRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
            {
                if (node.Declaration.Type.IsVar)
                    return node;
                return base.VisitLocalDeclarationStatement(node);
            }

            public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
            {
                if (node.NewKeyword != null)
                {
                    var newNode = node.WithType(SyntaxFactory.ParseTypeName(""))
                        .WithNewKeyword(node.NewKeyword.WithoutWhitespaceTrivia());
                    var nodeTypeinfo = semanticModel.GetTypeInfo(node);
                    var parentSymbol = semanticModel.GetSymbolInfo(node.Parent).Symbol;
                    if (parentSymbol?.Kind == SymbolKind.Method &&
                        (parentSymbol as IMethodSymbol)?.MethodKind == MethodKind.AnonymousFunction)
                        return base.VisitObjectCreationExpression(node);

                    if (nodeTypeinfo.ConvertedType.Name == nodeTypeinfo.Type.Name)
                        return newNode;
                }
                return base.VisitObjectCreationExpression(node);
            }
        }
    }
}
