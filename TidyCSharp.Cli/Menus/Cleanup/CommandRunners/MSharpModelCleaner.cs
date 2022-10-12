using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeValidators;
using TidyCSharp.Cli.Menus.Cleanup.Utils;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners;

public class MSharpModelCleaner : CodeCleanerCommandRunnerBase
{
    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        if (ProjectItemDetails.Document.Project.Name == "#Model")
            return await ChangeMethodHelperAsync(initialSourceNode);

        return initialSourceNode;
    }

    private async Task<SyntaxNode> ChangeMethodHelperAsync(SyntaxNode initialSourceNode)
    {
        var localTimeRewriter = new LocalTimeRewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode, Options);
        var modifiedSourceNode = localTimeRewriter.Visit(initialSourceNode);
        modifiedSourceNode = await RefreshResultAsync(modifiedSourceNode);

        var cascadeDeleteRewriter = new CascadeDeleteRewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode, Options);
        modifiedSourceNode = cascadeDeleteRewriter.Visit(modifiedSourceNode);
        modifiedSourceNode = await RefreshResultAsync(modifiedSourceNode);

        var calculatedGetterRewriter = new CalculatedGetterRewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode, Options);
        modifiedSourceNode = calculatedGetterRewriter.Visit(modifiedSourceNode);
        modifiedSourceNode = await RefreshResultAsync(modifiedSourceNode);

        var transientDatabaseModeRewriter = new TransientDatabaseModeRewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode, Options);
        modifiedSourceNode = transientDatabaseModeRewriter.Visit(modifiedSourceNode);

        if (IsReportOnlyMode)
        {
            CollectMessages(localTimeRewriter.GetReport());
            CollectMessages(cascadeDeleteRewriter.GetReport());
            CollectMessages(calculatedGetterRewriter.GetReport());
            CollectMessages(transientDatabaseModeRewriter.GetReport());
            return initialSourceNode;
        }

        return modifiedSourceNode;
    }

    private class LocalTimeRewriter : CleanupCSharpSyntaxRewriter
    {
        private SemanticModel _semanticModel;
        public LocalTimeRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options) :
            base(isReportOnlyMode, options) => _semanticModel = semanticModel;

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var methodSymbol = CSharpExtensions.GetSymbolInfo(_semanticModel, node).Symbol as IMethodSymbol;
            var methodName = methodSymbol?.Name;
            var methodType = methodSymbol?.ReturnType.OriginalDefinition?.ToString();

            var acceptedArguments = new string[]
            {
                "\"c#:LocalTime.UtcNow\"","\"c#:LocalTime.Now\"",
                "cs(\"LocalTime.UtcNow\")","cs(\"LocalTime.Now\")"
            };

            if (methodName == "Default" && methodType == "MSharp.DateTimeProperty")
            {
                if (node.ArgumentsCountShouldBe(1) &&
                    node.FirstArgumentShouldBeIn(acceptedArguments))
                {
                    var lineSpan = node.GetFileLinePosSpan();

                    AddReport(new ChangesReport(node)
                    {
                        LineNumber = lineSpan.StartLinePosition.Line,
                        Column = lineSpan.StartLinePosition.Character,
                        Message = "use DefaultToNow instead.",
                        Generator = nameof(LocalTimeRewriter)
                    });

                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression, node.GetLeftSideExpression(), SyntaxFactory.IdentifierName("DefaultToNow")),
                        SyntaxFactory.ArgumentList());
                }
            }

            return base.VisitInvocationExpression(node);
        }
    }

    private class CascadeDeleteRewriter : CleanupCSharpSyntaxRewriter
    {
        private SemanticModel _semanticModel;
        public CascadeDeleteRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options) :
            base(isReportOnlyMode, options) => _semanticModel = semanticModel;
        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var methodSymbol = (CSharpExtensions.GetSymbolInfo(_semanticModel, node).Symbol as IMethodSymbol);
            var methodName = methodSymbol?.Name;

            if (node.ArgumentsCountShouldBe(1) &&
                node.FirstArgumentShouldBe("CascadeAction.CascadeDelete") &&
                methodName == "OnDelete")
            {
                var lineSpan = node.GetFileLinePosSpan();

                AddReport(new ChangesReport(node)
                {
                    LineNumber = lineSpan.StartLinePosition.Line,
                    Column = lineSpan.StartLinePosition.Character,
                    Message = "use CascadeDelete instead.",
                    Generator = nameof(CascadeDeleteRewriter)
                });

                return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        node.GetLeftSideExpression(),
                        SyntaxFactory.IdentifierName("CascadeDelete")),
                    SyntaxFactory.ArgumentList());
            }

            return base.VisitInvocationExpression(node);
        }
    }

    private class CalculatedGetterRewriter : CleanupCSharpSyntaxRewriter
    {
        private SemanticModel _semanticModel;
        public CalculatedGetterRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options) :
            base(isReportOnlyMode, options) => _semanticModel = semanticModel;

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var calculatedInvocation = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                .Where(x => (CSharpExtensions.GetSymbolInfo(_semanticModel, x).Symbol as IMethodSymbol)?.Name == "Calculated").FirstOrDefault();

            var getterInvocation = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                .Where(x => (CSharpExtensions.GetSymbolInfo(_semanticModel, x).Symbol as IMethodSymbol)?.Name == "Getter").FirstOrDefault();

            if (calculatedInvocation == null || getterInvocation == null)
                return base.VisitInvocationExpression(node);

            if ((calculatedInvocation.ArgumentsCountShouldBe(0) ||
                 (calculatedInvocation.ArgumentsCountShouldBe(1) &&
                  calculatedInvocation.FirstArgumentShouldBe("true"))) &&
                getterInvocation.ArgumentsCountShouldBe(1))
            {
                var newNode = node.ReplaceNodes(
                    node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                        .Where(x => x.MethodNameShouldBeIn(new string[] { "Getter", "Calculated" })),
                    (nde1, nde2) =>
                    {
                        if (nde1.MethodNameShouldBe("Calculated"))
                            return nde1.GetLeftSideExpression();
                        else if (nde1.MethodNameShouldBe("Getter"))
                            return SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    nde2.GetLeftSideExpression(), SyntaxFactory.IdentifierName("CalculatedFrom")),
                                nde1.ArgumentList);
                        else return nde2;
                    });

                var lineSpan = node.GetFileLinePosSpan();

                AddReport(new ChangesReport(node)
                {
                    LineNumber = lineSpan.StartLinePosition.Line,
                    Column = lineSpan.StartLinePosition.Character,
                    Message = "use CalculatedFrom instead.",
                    Generator = nameof(CalculatedGetterRewriter)
                });

                return newNode;
            }

            return base.VisitInvocationExpression(node);
        }
    }

    private class TransientDatabaseModeRewriter : CleanupCSharpSyntaxRewriter
    {
        private SemanticModel _semanticModel;
        public TransientDatabaseModeRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options) :
            base(isReportOnlyMode, options) => _semanticModel = semanticModel;
        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var methodSymbol = CSharpExtensions.GetSymbolInfo(_semanticModel, node).Symbol as IMethodSymbol;
            var methodName = methodSymbol?.Name;

            if (node.ArgumentsCountShouldBe(1) &&
                node.FirstArgumentShouldBe("DatabaseOption.Transient") &&
                methodName == "DatabaseMode")
            {
                var newNode = node.ReplaceNode(node.ArgumentList, SyntaxFactory.ArgumentList());

                newNode = newNode.ReplaceNode(newNode.DescendantNodesAndSelfOfType<IdentifierNameSyntax>()
                        .FirstOrDefault(x => x.Identifier.ToString() == "DatabaseMode"),
                    SyntaxFactory.IdentifierName("Transient"));

                var lineSpan = node.GetFileLinePosSpan();

                AddReport(new ChangesReport(node)
                {
                    LineNumber = lineSpan.StartLinePosition.Line,
                    Column = lineSpan.StartLinePosition.Character,
                    Message = "use Transient method instead.",
                    Generator = nameof(TransientDatabaseModeRewriter)
                });

                return newNode;
            }

            return base.VisitInvocationExpression(node);
        }
    }
}