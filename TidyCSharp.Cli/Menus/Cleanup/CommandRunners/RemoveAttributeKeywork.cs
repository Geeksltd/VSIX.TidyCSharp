using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Extensions;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners;

public class RemoveAttributeKeywork : CodeCleanerCommandRunnerBase
{
    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        return RemoveAttributeKeyworkHelper(initialSourceNode, ProjectItemDetails.SemanticModel);
    }

    private SyntaxNode RemoveAttributeKeyworkHelper(SyntaxNode initialSourceNode, SemanticModel semanticModel)
    {
        var syntaxRewriter = new Rewriter(semanticModel, IsReportOnlyMode, Options);
        var modifiedSyntaxNode = syntaxRewriter.Visit(initialSourceNode);

        if (IsReportOnlyMode)
        {
            CollectMessages(syntaxRewriter.GetReport());
            return initialSourceNode;
        }

        return modifiedSyntaxNode;
    }

    private static string _attributeKeywork = SyntaxKind.Attribute.ToString();

    private class Rewriter : CleanupCSharpSyntaxRewriter
    {
        private SemanticModel _semanticModel;
        public Rewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options)
            : base(isReportOnlyMode, options) => _semanticModel = semanticModel;

        public override SyntaxNode VisitAttribute(AttributeSyntax node)
        {
            if (node.Name is IdentifierNameSyntax newNameNode)
            {
                if (newNameNode.Identifier.ValueText.EndsWith(_attributeKeywork))
                {
                    var orginalNodeTypeInfo = CSharpExtensions.GetTypeInfo(_semanticModel, node.Name);

                    if (orginalNodeTypeInfo.Type == null) base.VisitAttribute(node);

                    if (orginalNodeTypeInfo.Type.Name == newNameNode.Identifier.ValueText)
                    {
                        var newName = newNameNode.Identifier.ValueText.TrimEnd(_attributeKeywork);

                        if (IsReportOnlyMode)
                        {
                            var lineSpan = node.GetFileLinePosSpan();

                            AddReport(new ChangesReport(node)
                            {
                                LineNumber = lineSpan.StartLinePosition.Line,
                                Column = lineSpan.StartLinePosition.Character,
                                Message = "Attributes should not ended with \"Attribute\"",
                                Generator = nameof(RemoveAttributeKeywork)
                            });
                        }

                        node = node.WithName(SyntaxFactory.IdentifierName(newName));
                    }
                }
            }

            return base.VisitAttribute(node);
        }
    }
}