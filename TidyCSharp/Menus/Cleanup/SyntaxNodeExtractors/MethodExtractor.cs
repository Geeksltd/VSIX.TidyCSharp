using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public class MethodExtractor : ISyntaxTreeMemberExtractor
    {
        public List<SyntaxToken> Extraxt(SyntaxNode root, SyntaxKind kind)
        {
            return root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                                         .Where(mth => mth.Modifiers.Any(m => m.IsKind(kind)))
                                         .SelectMany(method => method.Modifiers.Where(m => m.IsKind(kind)))
                                         .ToList();
        }
    }
}
