using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners;

public class ConvertFullNameTypesToBuiltInTypes : CodeCleanerCommandRunnerBase
{
    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        var syntaxRewriter = new Rewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode
            , Options);

        var modifiedSourceNode = syntaxRewriter.ConvertFullNameTypesToBuiltInTypesHelper(initialSourceNode);

        if (IsReportOnlyMode)
        {
            CollectMessages(syntaxRewriter.GetReport());
            return initialSourceNode;
        }

        return modifiedSourceNode;
    }

    private class Rewriter : CleanupCSharpSyntaxRewriter
    {
        private SemanticModel _semanticModel;
        public Rewriter(SemanticModel semanticModel,
            bool isReportOnlyMode, ICleanupOption options) : base(isReportOnlyMode, options)
        {
            _semanticModel = semanticModel;
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
                        var symbol = _semanticModel.GetSymbolInfo(oldNode1).Symbol;
                        if (symbol != null && symbol.Kind != SymbolKind.NamedType) return oldNode1;
                    }
                    else if (oldNode1 is IdentifierNameSyntax == false && oldNode1 is QualifiedNameSyntax == false) return oldNode1;
                    else
                    {
                        var symbol = _semanticModel.GetSymbolInfo(oldNode1).Symbol;
                        if (symbol != null && symbol.Kind != SymbolKind.NamedType) return oldNode1;
                    }

                    var lineSpan = oldNode1.GetFileLinePosSpan();

                    AddReport(new ChangesReport(oldNode1)
                    {
                        LineNumber = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                        Message = "Convert full name types to built in Types",
                        Generator = nameof(ConvertFullNameTypesToBuiltInTypes)
                    });

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