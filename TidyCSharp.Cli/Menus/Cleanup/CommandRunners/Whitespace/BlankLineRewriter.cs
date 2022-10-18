using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners.Whitespace;

public class BlankLineRewriter : CSharpSyntaxRewriterBase
{
    private SemanticModel _semanticModel;
    public BlankLineRewriter(SyntaxNode node, bool isReadOnlyMode, SemanticModel semanticModel) :
        base(node, isReadOnlyMode, null) => _semanticModel = semanticModel;
    public override SyntaxNode VisitBlock(BlockSyntax node)
    {
        var writeTrailing = false;

        node = node.ReplaceNodes(node.Statements, (nde1, nde2) =>
        {
            if (nde1.WithoutLeadingTrivia().WithoutTrailingTrivia()
                .DescendantTrivia().Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
            {
                var leading = nde1.GetLeadingTrivia();
                var trailing = nde1.GetTrailingTrivia();

                var leadingTrivias = new SyntaxTriviaList(SyntaxFactory.EndOfLine("\n"));

                if (nde1.Equals(node.Statements.FirstOrDefault()) || writeTrailing ||
                    nde1.GetLeadingTrivia().Count(x => x.IsKind(SyntaxKind.EndOfLineTrivia)) > 0)
                {
                    leadingTrivias = new SyntaxTriviaList();
                    writeTrailing = false;
                }

                var trailingTrivias = new SyntaxTriviaList();

                leadingTrivias = leadingTrivias.AddRange(leading);

                trailingTrivias = trailingTrivias.AddRange(trailing);

                if (!nde1.Equals(node.Statements.LastOrDefault()) &&
                    (node.Statements.IndexOf(nde1) + 1 < node.Statements.Count() &&
                     node.Statements[node.Statements.IndexOf(nde1) + 1]
                         .GetLeadingTrivia().Count(x => x.IsKind(SyntaxKind.EndOfLineTrivia)) < 1))
                {
                    writeTrailing = true;
                    trailingTrivias = trailingTrivias.Add(SyntaxFactory.EndOfLine("\n"));
                }

                return nde2.WithTrailingTrivia(trailingTrivias)
                    .WithLeadingTrivia(leadingTrivias);
            }

            writeTrailing = false;
            return nde2;
        });

        return base.VisitBlock(node);
    }
}