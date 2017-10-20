using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using System;
using Microsoft.CodeAnalysis.Rename;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class RemoveExtraThisQualification : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return new Rewriter(ProjectItemDetails).Visit(initialSourceNode);
        }


        class Rewriter : CSharpSyntaxRewriter
        {
            private readonly SemanticModel semanticModel;

            public Rewriter(ProjectItemDetailsType projectItemDetails)
            {
                semanticModel = projectItemDetails.SemanticModel;
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.Parent is ClassDeclarationSyntax == false)
                {
                    node = Remove(node);
                }

                return base.VisitClassDeclaration(node);
            }

            ClassDeclarationSyntax Remove(ClassDeclarationSyntax classNode)
            {
                var thises = classNode.DescendantNodes().OfType<ThisExpressionSyntax>();
                var newItems = new Dictionary<MemberAccessExpressionSyntax, SyntaxNode>();

                foreach (var thisItem in thises)
                {
                    if (thisItem.Parent is MemberAccessExpressionSyntax thisItemAsMemberAccessException)
                    {
                        var newAccess = GetMemberAccessWithoutThis(thisItemAsMemberAccessException);
                        if (newAccess != null)
                        {
                            newItems.Add(thisItemAsMemberAccessException, newAccess);
                        }
                    }
                }

                if (newItems.Any())
                {
                    classNode = classNode.ReplaceNodes(newItems.Keys, (node1, node2) => newItems[node1]);
                }

                return classNode;
            }

            private SyntaxNode GetMemberAccessWithoutThis(MemberAccessExpressionSyntax thisItemAsMemberAccessException)
            {
                var right = thisItemAsMemberAccessException.Name;
                var symbols = semanticModel.LookupSymbols(thisItemAsMemberAccessException.SpanStart, name: right.Identifier.ValueText);
                var thisItemAsMemberAccessExceptionSymbol = semanticModel.GetSymbolInfo(thisItemAsMemberAccessException).Symbol;
                if (symbols.Any(x => x == thisItemAsMemberAccessExceptionSymbol))
                {
                    return right.WithLeadingTrivia(thisItemAsMemberAccessException.GetLeadingTrivia());
                }
                return null;
            }
        }
    }
}