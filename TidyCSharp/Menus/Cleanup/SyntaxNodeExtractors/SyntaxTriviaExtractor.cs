using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors
{
	public static class SyntaxTriviaExtractor
	{
		public static string GetFilePath(this SyntaxTrivia trivia)
		{
			return trivia.SyntaxTree.FilePath;
		}
		public static FileLinePositionSpan GetFileLinePosSpan(this SyntaxTrivia trivia)
		{
			return trivia.SyntaxTree.GetLineSpan(new TextSpan(trivia.Span.Start, trivia.Span.Length));
		}
	}
}
