
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
	public class CSharpSyntaxUpgrade : CodeCleanerCommandRunnerBase, ICodeCleaner
	{
		public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
		{
			return ChangeMethodHelper(initialSourceNode);
		}

		SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode)
		{
			var syntaxRewriter = new NewExpressionRewriter(ProjectItemDetails.SemanticModel
				, this.IsReportOnlyMode, this.Options);
			var modifiedSyntaxNode = syntaxRewriter.Visit(initialSourceNode);
			if (IsReportOnlyMode)
			{
				this.CollectMessages(syntaxRewriter.GetReport());
				return initialSourceNode;
			}
			return modifiedSyntaxNode;
		}

		class NewExpressionRewriter : CleanupCSharpSyntaxRewriter
		{
			SemanticModel semanticModel;
			public NewExpressionRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options)
				: base(isReportOnlyMode, options) => this.semanticModel = semanticModel;

			public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
			{
				if (node.Declaration.Type.IsVar)
					return node;
				//if (node. != null)
				//{
				//    var newNode = node.WithType(SyntaxFactory.ParseTypeName(""))
				//        .WithNewKeyword(node.NewKeyword.WithoutWhitespaceTrivia());
				//    var nodeTypeinfo = semanticModel.GetTypeInfo(node);
				//    var parentSymbol = semanticModel.GetSymbolInfo(node.Parent).Symbol;
				//    if (parentSymbol?.Kind == SymbolKind.Method &&
				//        (parentSymbol as IMethodSymbol)?.MethodKind == MethodKind.AnonymousFunction)
				//        return base.VisitObjectCreationExpression(node);

				//    if (nodeTypeinfo.ConvertedType.Name == nodeTypeinfo.Type.Name)
				//        return newNode;
				//}
				return base.VisitLocalDeclarationStatement(node);
			}

			public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
			{
				if (((CSharpCompilation)this.semanticModel.Compilation).LanguageVersion.MapSpecifiedToEffectiveVersion() != LanguageVersion.CSharp9)
					return base.VisitObjectCreationExpression(node);
				if (node.NewKeyword == null)
					return base.VisitObjectCreationExpression(node);
				if (node.Parent.IsKind(SyntaxKind.ReturnStatement))
					return base.VisitObjectCreationExpression(node);
				if (node.Parent.IsKind(SyntaxKind.LocalDeclarationStatement))
					return base.VisitObjectCreationExpression(node);
				if (node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
					return base.VisitObjectCreationExpression(node);

				var newNode = node.WithType(SyntaxFactory.ParseTypeName(""))
					.WithNewKeyword(node.NewKeyword.WithoutWhitespaceTrivia())
					.WithArgumentList(node.ArgumentList ?? SyntaxFactory.ParseArgumentList("()"));
				var nodeTypeinfo = semanticModel.GetTypeInfo(node);
				var parentSymbol = semanticModel.GetSymbolInfo(node.Parent).Symbol;
				if (parentSymbol?.Kind == SymbolKind.Method &&
					(parentSymbol as IMethodSymbol)?.MethodKind == MethodKind.AnonymousFunction)
					return base.VisitObjectCreationExpression(node);
				if (node.Parent.IsKind(SyntaxKind.Argument))
				{
					var methodInvocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
					if (methodInvocation != null)
					{
						var methodSymbol = this.semanticModel.GetSymbolInfo(methodInvocation).Symbol;
						var countofMethod = methodSymbol?.ContainingType?.GetMembers()
							.Count(x => x.Name == methodInvocation.Expression.ToString());
						//var countofMethod = methodSymbol?.ContainingType?.GetMembers()
						//	.Count(x => x.Name == methodInvocation.Expression.ToString()
						//		&& x.Kind == SymbolKind.Method
						//		&& (x as IMethodSymbol)?.Parameters.Count() ==
						//			methodInvocation.ArgumentList.Arguments.Count());
						if (countofMethod > 1)
							return base.VisitObjectCreationExpression(node);
					}
					else
						return base.VisitObjectCreationExpression(node);
					//else
					//{
					//	var constructorInvocation = node.FirstAncestorOrSelf<ConstructorInitializerSyntax>();
					//	string name = string.Empty;
					//	if (constructorInvocation.IsKind(SyntaxKind.ThisConstructorInitializer))
					//	{
					//		var constructorDeclaration = node.FirstAncestorOrSelf<ConstructorDeclarationSyntax>();
					//		name = constructorDeclaration.Identifier.ToString();
					//	}else
					//	{
					//		n
					//	}

					//	var constructorSymbol = this.semanticModel.GetSymbolInfo(constructorInvocation).Symbol;
					//	var countofConstructors = constructorSymbol?.ContainingType?.GetMembers()
					//		.Count(x => x.Name == name);
					//	if (countofConstructors > 1)
					//		return base.VisitObjectCreationExpression(node);
					//}
				}
				if (nodeTypeinfo.ConvertedType.Name == nodeTypeinfo.Type.Name)
				{
					if (IsReportOnlyMode)
					{
						var lineSpan = node.GetFileLinePosSpan();
						AddReport(new ChangesReport(node)
						{
							LineNumber = lineSpan.StartLinePosition.Line,
							Column = lineSpan.StartLinePosition.Character,
							Message = "Object Creation new Syntax in c# v9",
							Generator = nameof(CSharpSyntaxUpgrade)
						});
					}
					return newNode;
				}
				return base.VisitObjectCreationExpression(node);
			}
		}
	}
}
