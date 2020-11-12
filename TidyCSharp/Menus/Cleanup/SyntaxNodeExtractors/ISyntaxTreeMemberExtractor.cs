using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public interface ISyntaxTreeMemberExtractor
    {
        List<SyntaxToken> Extraxt(SyntaxNode root, SyntaxKind kind);
    }
}