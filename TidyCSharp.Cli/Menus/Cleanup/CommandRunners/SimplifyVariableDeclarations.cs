using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners;

public class SimplifyVariableDeclarations : CodeCleanerCommandRunnerBase
{
    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        var syntaxRewriter = new Rewriter(ProjectItemDetails.SemanticModel,
            IsReportOnlyMode, Options);

        var modifiedSyntaxNode = syntaxRewriter.Visit(initialSourceNode);

        if (IsReportOnlyMode)
        {
            CollectMessages(syntaxRewriter.GetReport());
            return initialSourceNode;
        }

        return modifiedSyntaxNode;
    }

    private class Rewriter : CleanupCSharpSyntaxRewriter
    {
        private const string VarKeyword = "var";
        private SemanticModel _semanticModel;
        public Rewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options)
            : base(isReportOnlyMode, options)
        {
            _semanticModel = semanticModel;
        }

        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            return ConvertToVar(node) ?? node;
        }

        private SyntaxNode ConvertToVar(VariableDeclarationSyntax node)
        {
            if (node.Parent is LocalDeclarationStatementSyntax == false) return null;
            if ((node.Parent as LocalDeclarationStatementSyntax).IsConst) return null;

            if (node.Type is IdentifierNameSyntax varIdentifierNameSyntax)
            {
                if (varIdentifierNameSyntax.Identifier.ValueText == VarKeyword) return null;
            }

            if (node.Variables.Count > 1) return null;

            var variable = node.Variables.FirstOrDefault();

            if (variable.Initializer == null) return null;

            var typeOfInitializer = CSharpExtensions.GetTypeInfo(_semanticModel, variable.Initializer.Value);

            var typeOfTypeDef = CSharpExtensions.GetTypeInfo(_semanticModel, node.Type);

            if (typeOfInitializer.Type?.Name == typeOfTypeDef.Type?.Name)
            {
                if (IsReportOnlyMode)
                {
                    var lineSpan = node.GetFileLinePosSpan();

                    AddReport(new ChangesReport(node)
                    {
                        LineNumber = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                        Message = "Should Convert To Var",
                        Generator = nameof(SimplifyVariableDeclarations)
                    });
                }

                node =
                    node
                        .WithType(
                            SyntaxFactory.ParseTypeName(VarKeyword)
                                .WithTrailingTrivia(SyntaxFactory.Space)
                                .WithLeadingTrivia(node.Type.GetLeadingTrivia())
                        );
            }

            return base.VisitVariableDeclaration(node);
        }
    }
}