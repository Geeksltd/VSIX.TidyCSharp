using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public interface ISyntaxTreeMemberExtractor
    {
        List<SyntaxToken> Extraxt(SyntaxNode root, SyntaxKind kind);
    }
}
