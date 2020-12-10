using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup.NormalizeWhitespace
{
    public class BlankLineRewriter : CSharpSyntaxRewriter
    {
        SemanticModel semanticModel;
        public BlankLineRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;


        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            bool writeTrailing = false;
            node = node.ReplaceNodes(node.Statements, (nde1, nde2) =>
               {
                   if (nde1.WithoutLeadingTrivia().WithoutTrailingTrivia()
                    .DescendantTrivia().Any(x => x.IsKind(SyntaxKind.EndOfLineTrivia)))
                   {
                       var leading = nde1.GetLeadingTrivia();
                       var trailing = nde1.GetTrailingTrivia();

                       SyntaxTriviaList leadingTrivias = new SyntaxTriviaList(SyntaxFactory.EndOfLine("\n"));
                       if (nde1.Equals(node.Statements.FirstOrDefault()) || writeTrailing ||
                            nde1.GetLeadingTrivia().Count(x => x.IsKind(SyntaxKind.EndOfLineTrivia)) > 0)
                       {
                           leadingTrivias = new SyntaxTriviaList();
                           writeTrailing = false;
                       }
                       SyntaxTriviaList trailingTrivias = new SyntaxTriviaList();

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
}
