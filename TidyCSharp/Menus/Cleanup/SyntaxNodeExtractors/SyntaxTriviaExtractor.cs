using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors
{
    public static class SyntaxTriviaExtractor
    {
        public static string GetFilePath(this SyntaxTrivia trivia) => trivia.SyntaxTree.FilePath;

        public static FileLinePositionSpan GetFileLinePosSpan(this SyntaxTrivia trivia)
        {
            return trivia.SyntaxTree.GetLineSpan(new TextSpan(trivia.Span.Start, trivia.Span.Length));
        }
    }
}