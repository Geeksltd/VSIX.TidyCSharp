using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeValidators;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class MSharpModelCleaner : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override async Task<SyntaxNode> CleanUp(SyntaxNode initialSourceNode)
        {
            if (ProjectItemDetails.ProjectItem.ContainingProject.Name == "#Model")
                return await ChangeMethodHelper(initialSourceNode);

            return initialSourceNode;
        }

        async Task<SyntaxNode> ChangeMethodHelper(SyntaxNode initialSourceNode)
        {
            var localTimeRewriter = new LocalTimeRewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode, Options);
            var modifiedSourceNode = localTimeRewriter.Visit(initialSourceNode);
            modifiedSourceNode = await RefreshResult(modifiedSourceNode);

            var cascadeDeleteRewriter = new CascadeDeleteRewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode, Options);
            modifiedSourceNode = cascadeDeleteRewriter.Visit(modifiedSourceNode);
            modifiedSourceNode = await RefreshResult(modifiedSourceNode);

            var calculatedGetterRewriter = new CalculatedGetterRewriter(ProjectItemDetails.SemanticModel, IsReportOnlyMode, Options);
            modifiedSourceNode = calculatedGetterRewriter.Visit(modifiedSourceNode);
            modifiedSourceNode = await RefreshResult(modifiedSourceNode);

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

        class LocalTimeRewriter : CleanupCSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public LocalTimeRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options) :
                base(isReportOnlyMode, options) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
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
        class CascadeDeleteRewriter : CleanupCSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public CascadeDeleteRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options) :
                base(isReportOnlyMode, options) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var methodSymbol = (semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol);
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
        class CalculatedGetterRewriter : CleanupCSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public CalculatedGetterRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options) :
                base(isReportOnlyMode, options) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var calculatedInvocation = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "Calculated").FirstOrDefault();

                var getterInvocation = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "Getter").FirstOrDefault();

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
        class TransientDatabaseModeRewriter : CleanupCSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public TransientDatabaseModeRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options) :
                base(isReportOnlyMode, options) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
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
}