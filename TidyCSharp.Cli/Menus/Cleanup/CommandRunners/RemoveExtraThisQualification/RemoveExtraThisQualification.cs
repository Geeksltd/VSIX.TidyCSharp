using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners.RemoveExtraThisQualification.Option;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.RemoveExtraThisQualification;

public class RemoveExtraThisQualification : CodeCleanerCommandRunnerBase
{
    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        var rewriter = new Rewriter(ProjectItemDetails, IsReportOnlyMode, Options);
        var modifiedSourceNode = rewriter.Visit(initialSourceNode);

        if (IsReportOnlyMode)
        {
            CollectMessages(rewriter.GetReport());
            return initialSourceNode;
        }

        return modifiedSourceNode;
    }

    private class Rewriter : CleanupCSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;

        public Rewriter(ProjectItemDetailsType projectItemDetails, bool isReportOnlyMode, ICleanupOption options)
            : base(isReportOnlyMode, options)
        {
            _semanticModel = projectItemDetails.SemanticModel;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (node.Parent is ClassDeclarationSyntax == false)
            {
                node = Remove(node);
            }

            return base.VisitClassDeclaration(node);
        }

        private ClassDeclarationSyntax Remove(ClassDeclarationSyntax classNode)
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
                classNode = classNode.ReplaceNodes(newItems.Keys, (node1, node2) =>
                {
                    if (IsReportOnlyMode)
                    {
                        var lineSpan = node1.GetFileLinePosSpan();

                        AddReport(new ChangesReport(classNode)
                        {
                            LineNumber = lineSpan.StartLinePosition.Line,
                            Column = lineSpan.StartLinePosition.Character,
                            Message = "you can remove this Identifier",
                            Generator = nameof(RemoveExtraThisQualification)
                        });
                    }

                    return newItems[node1];
                });
            }

            return classNode;
        }

        private SyntaxNode GetMemberAccessWithoutThis(MemberAccessExpressionSyntax thisItemAsMemberAccessException)
        {
            var thisItemAsMemberAccessExceptionSymbol = _semanticModel.GetSymbolInfo(thisItemAsMemberAccessException).Symbol;

            if (thisItemAsMemberAccessExceptionSymbol is IFieldSymbol && !CheckOption((int)CleanupTypes.RemoveFromFieldsCall)) return null;
            if (thisItemAsMemberAccessExceptionSymbol is IPropertySymbol && !CheckOption((int)CleanupTypes.RemoveFromPropertiesCall)) return null;
            if (thisItemAsMemberAccessExceptionSymbol is IMethodSymbol && !CheckOption((int)CleanupTypes.RemoveFromMethodCall)) return null;

            var right = thisItemAsMemberAccessException.Name;
            var symbols = _semanticModel.LookupSymbols(thisItemAsMemberAccessException.SpanStart, name: right.Identifier.ValueText);

            if (symbols.Any(x => SymbolEqualityComparer.Default.Equals(x, thisItemAsMemberAccessExceptionSymbol)))
            {
                return right.WithLeadingTrivia(thisItemAsMemberAccessException.GetLeadingTrivia());
            }

            return null;
        }
    }
}