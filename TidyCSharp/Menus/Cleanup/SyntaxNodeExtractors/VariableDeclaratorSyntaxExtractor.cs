using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors
{
	public static class VariableDeclaratorSyntaxExtractor
	{
		public static FileLinePositionSpan GetFileLinePosSpan(this VariableDeclaratorSyntax node)
		{
			return node.SyntaxTree.GetLineSpan(new TextSpan(node.Span.Start, node.Span.Length));
		}
	}
}
