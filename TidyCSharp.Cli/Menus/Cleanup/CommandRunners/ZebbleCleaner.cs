using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners;

public class ZebbleCleaner : CodeCleanerCommandRunnerBase
{
    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        if (ProjectItemDetails.SemanticModel is null) return initialSourceNode;

        var syntaxRewriter = new ReadOnlyRewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode, Options);
        var modifiedSourceNode = syntaxRewriter.Visit(initialSourceNode);

        if (IsReportOnlyMode)
        {
            CollectMessages(syntaxRewriter.GetReport());
            return initialSourceNode;
        }

        return modifiedSourceNode;
    }

    private class ReadOnlyRewriter : CleanupCSharpSyntaxRewriter
    {
        private SemanticModel _semanticModel;

        public ReadOnlyRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options)
            : base(isReportOnlyMode, options)
        {
            _semanticModel = semanticModel;
            IsReportOnlyMode = isReportOnlyMode;
        }
        public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)) return node;
            if (CSharpExtensions.GetTypeInfo(_semanticModel, node.Declaration.Type).Type.Name != "Bindable") return node;
            if (!node.Declaration.Type.IsKind(SyntaxKind.GenericName)) return node;

            if (CSharpExtensions.GetTypeInfo(_semanticModel, node.Declaration.Type).Type.ContainingNamespace.Name != "Zebble" &&
                CSharpExtensions.GetTypeInfo(_semanticModel, node.Declaration.Type).Type.ContainingNamespace.Name != "Olive")
                return node;

            if (IsReportOnlyMode)
            {
                var lineSpan = node.GetFileLinePosSpan();

                AddReport(new ChangesReport(node)
                {
                    LineNumber = lineSpan.StartLinePosition.Line,
                    Column = lineSpan.StartLinePosition.Character,
                    Message = "Should Add Readonly Modifier",
                    Generator = nameof(ZebbleCleaner)
                });

                return node;
            }

            return node.AddModifiers(
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
                    .WithTrailingTrivia(SyntaxFactory.ParseTrailingTrivia(" ")));
        }
    }
}