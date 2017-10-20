using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class RemoveAttributeKeywork : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return RemoveAttributeKeyworkHelper(initialSourceNode);
        }

        public static SyntaxNode RemoveAttributeKeyworkHelper(SyntaxNode initialSourceNode)
        {
            initialSourceNode = new Rewriter().Visit(initialSourceNode);
            return initialSourceNode;
        }

        static string Attribute_Keywork = SyntaxKind.Attribute.ToString();
        class Rewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitAttribute(AttributeSyntax node)
            {
                if(node.Name is IdentifierNameSyntax newNameNode)
                {
                    if(newNameNode.Identifier.ValueText.EndsWith(Attribute_Keywork))
                    {
                        var newName = newNameNode.Identifier.ValueText.TrimEnd(Attribute_Keywork);

                        node = node.WithName(SyntaxFactory.IdentifierName(newName));
                    }
                }

                return base.VisitAttribute(node);
            }

        }
    }
}