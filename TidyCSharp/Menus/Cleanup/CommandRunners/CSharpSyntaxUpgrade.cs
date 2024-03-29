﻿using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Cleanup.Infra;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class CSharpSyntaxUpgrade : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
        {
            return ChangeMethodHelper(initialSourceNode);
        }

        SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode)
        {
            if (initialSourceNode is null) return null;

            if (ProjectItemDetails.SemanticModel is null) return null;

            var syntaxRewriter = new NewExpressionRewriter(ProjectItemDetails.SemanticModel
                , IsReportOnlyMode, Options);

            if (syntaxRewriter is null) return null;

            var modifiedSyntaxNode = syntaxRewriter.Visit(initialSourceNode);

            if (IsReportOnlyMode)
            {
                CollectMessages(syntaxRewriter.GetReport());
                return initialSourceNode;
            }

            return modifiedSyntaxNode;
        }

        class NewExpressionRewriter : CleanupCSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public NewExpressionRewriter(SemanticModel semanticModel, bool isReportOnlyMode, ICleanupOption options)
                : base(isReportOnlyMode, options) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
            {
                if (node.Type.IsVar) return node;
                return base.VisitVariableDeclaration(node);
            }

            public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
            {
                if (((CSharpCompilation)semanticModel.Compilation).LanguageVersion.MapSpecifiedToEffectiveVersion() != LanguageVersion.CSharp9)
                    return base.VisitObjectCreationExpression(node);

                if (node.NewKeyword == null
                    || node.Parent.IsKind(SyntaxKind.LocalDeclarationStatement)
                    || node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression)
                    || node.Parent.IsKind(SyntaxKind.UsingStatement))
                    return base.VisitObjectCreationExpression(node);

                //if (node.Parent.IsKind(SyntaxKind.LocalDeclarationStatement))
                //    return base.VisitObjectCreationExpression(node);

                //if (node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                //    return base.VisitObjectCreationExpression(node);

                //if (node.Parent.IsKind(SyntaxKind.UsingStatement))
                //    return base.VisitObjectCreationExpression(node);

                var newNode = node.WithType(SyntaxFactory.ParseTypeName(""))
                    .WithNewKeyword(node.NewKeyword.WithoutWhiteSpaceTrivia())
                    .WithArgumentList(node.ArgumentList ?? SyntaxFactory.ParseArgumentList("()"));

                var nodeTypeinfo = semanticModel.GetTypeInfo(node);
                var parentSymbol = semanticModel.GetSymbolInfo(node.Parent).Symbol;

                if (parentSymbol?.Kind == SymbolKind.Method &&
                    (parentSymbol as IMethodSymbol)?.MethodKind == MethodKind.AnonymousFunction)
                    return base.VisitObjectCreationExpression(node);

                if (node.Parent.IsKind(SyntaxKind.ReturnStatement))
                {
                    var methodDeclaration = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
                    var propertyDeclaration = node.FirstAncestorOrSelf<PropertyDeclarationSyntax>();

                    if (methodDeclaration != null)
                    {
                        if (methodDeclaration.ReturnType.ToString() != nodeTypeinfo.Type.Name)
                            return base.VisitObjectCreationExpression(node);

                        var symbol = semanticModel.GetDeclaredSymbol(methodDeclaration);

                        if (symbol.ReturnType.TypeKind != TypeKind.Class)
                            return base.VisitObjectCreationExpression(node);
                    }
                    else if (propertyDeclaration != null)
                    {
                        var symbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);

                        if (symbol.GetMethod.ReturnType.TypeKind != TypeKind.Class)
                            return base.VisitObjectCreationExpression(node);
                    }
                }
                else if (node.Parent.IsKind(SyntaxKind.Argument))
                {
                    var methodInvocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();

                    if (methodInvocation != null)
                    {
                        var methodSymbol = semanticModel.GetSymbolInfo(methodInvocation).Symbol;

                        var countofMethod = methodSymbol?.ContainingType?.GetMembers()
                            .Count(x => x.Name ==
                            (methodInvocation.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) ?
                            methodInvocation.GetRightSideNameSyntax().ToString() :
                            methodInvocation.Expression.ToString()));

                        if (countofMethod > 1)
                            return base.VisitObjectCreationExpression(node);

                        var indexOfMethod = node.FirstAncestorOrSelf<ArgumentListSyntax>().Arguments
                            .IndexOf(node.AncestorsAndSelf().FirstOrDefault(x => x.Parent.IsKind(SyntaxKind.ArgumentList)) as ArgumentSyntax);

                        if ((methodSymbol as IMethodSymbol)?.OriginalDefinition.Parameters[indexOfMethod].IsParams == true)
                            return base.VisitObjectCreationExpression(node);

                        if ((methodSymbol as IMethodSymbol)?.OriginalDefinition.IsGenericMethod == true &&
                            (methodInvocation.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) ?
                            !methodInvocation.GetRightSideNameSyntax().IsKind(SyntaxKind.GenericName) :
                            !methodInvocation.Expression.IsKind(SyntaxKind.GenericName)))
                            return base.VisitObjectCreationExpression(node);
                    }
                    else
                        return base.VisitObjectCreationExpression(node);
                }
                else if (node.Parent.IsKind(SyntaxKind.ArrayInitializerExpression))
                {
                    if (node.Parent.Parent.IsKind(SyntaxKind.ImplicitArrayCreationExpression))
                        return base.VisitObjectCreationExpression(node);
                }

                if (nodeTypeinfo.ConvertedType.ToString() == nodeTypeinfo.Type.ToString())
                {
                    if (isReportOnlyMode)
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