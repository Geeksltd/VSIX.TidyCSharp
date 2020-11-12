using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class ConvertFullNameTypesToBuiltInTypes : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            return ConvertFullNameTypesToBuiltInTypesHelper(initialSourceNode);
        }

        public SyntaxNode ConvertFullNameTypesToBuiltInTypesHelper(SyntaxNode initialSource)
        {
            var builtInTypesMapDic = TypesMapItem.GetBuiltInTypesDic();

            var selectedTokensList =
                initialSource
                    .DescendantNodes()
                    .Where
                    (
                        n =>
                            (n is IdentifierNameSyntax || n is QualifiedNameSyntax)
                            &&
                            builtInTypesMapDic.ContainsKey(n.WithoutTrivia().ToFullString())
                    );
            return initialSource.ReplaceNodes(
                    selectedTokensList,
                    (oldNode1, oldNode2) =>
                    {
                        if (oldNode1.Parent is QualifiedNameSyntax) return oldNode1;
                        if (oldNode1.Parent is MemberAccessExpressionSyntax)
                        {
                            if ((oldNode1.Parent as MemberAccessExpressionSyntax).Expression != oldNode1) return oldNode1;
                            var symbol = ProjectItemDetails.SemanticModel.GetSymbolInfo(oldNode1).Symbol;
                            if (symbol != null && symbol.Kind != SymbolKind.NamedType) return oldNode1;
                        }
                        else if (oldNode1 is IdentifierNameSyntax == false && oldNode1 is QualifiedNameSyntax == false) return oldNode1;
                        else
                        {
                            var symbol = ProjectItemDetails.SemanticModel.GetSymbolInfo(oldNode1).Symbol;
                            if (symbol != null && symbol.Kind != SymbolKind.NamedType) return oldNode1;
                        }

                        return
                            builtInTypesMapDic[oldNode1.WithoutTrivia().ToFullString()]
                                .NewNode
                                .WithLeadingTrivia(oldNode1.GetLeadingTrivia())
                                .WithTrailingTrivia(oldNode1.GetTrailingTrivia());
                    }
                );
        }
    }
}