using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class RemoveAttributeKeywork : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return RemoveAttributeKeyworkHelper(initialSourceNode, ProjectItemDetails.SemanticModel);
        }

        SyntaxNode RemoveAttributeKeyworkHelper(SyntaxNode initialSourceNode, SemanticModel semanticModel)
        {
            initialSourceNode = new Rewriter(semanticModel).Visit(initialSourceNode);
            return initialSourceNode;
        }

        static string Attribute_Keywork = SyntaxKind.Attribute.ToString();
        class Rewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public Rewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitAttribute(AttributeSyntax node)
            {
                if (node.Name is IdentifierNameSyntax newNameNode)
                {
                    if (newNameNode.Identifier.ValueText.EndsWith(Attribute_Keywork))
                    {
                        var orginalNodeTypeInfo = semanticModel.GetTypeInfo(node.Name);

                        if (orginalNodeTypeInfo.Type == null) base.VisitAttribute(node);

                        if (orginalNodeTypeInfo.Type.Name == newNameNode.Identifier.ValueText)
                        {
                            var newName = newNameNode.Identifier.ValueText.TrimEnd(Attribute_Keywork);

                            node = node.WithName(SyntaxFactory.IdentifierName(newName));
                        }
                    }
                }

                return base.VisitAttribute(node);
            }
        }
    }
}