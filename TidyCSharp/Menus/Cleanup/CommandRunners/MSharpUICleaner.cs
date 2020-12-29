using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeExtractors;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeTypeConverter;
using Geeks.VSIX.TidyCSharp.Menus.Cleanup.SyntaxNodeValidators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using RoslynDocument = Microsoft.CodeAnalysis.Document;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class MSharpUICleaner : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            if (App.DTE.ActiveDocument.ProjectItem.ProjectItems.ContainingProject.Name == "#UI")
                return ChangeMethodHelper(initialSourceNode);
            return initialSourceNode;
        }

        SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode)
        {
            initialSourceNode = new SendItemIdRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new IfWorkFlowRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new CustomColumnRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new ElementsNewSyntaxRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new FormModuleWorkFlowRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new GeneralWorkFlowRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new CombinedExpressionsFormRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new ListModuleWorkFlowRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new FullSearchRewriter(ProjectItemDetails.SemanticModel
                , ProjectItemDetails.ProjectItemDocument.Project.Solution
                , ProjectItemDetails.ProjectItemDocument)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new CustomFieldRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            initialSourceNode = this.RefreshResult(initialSourceNode);
            initialSourceNode = new OnClickGoWorkFlowRewriter(ProjectItemDetails.SemanticModel)
                .Visit(initialSourceNode);
            return initialSourceNode;
        }

        //cancelsave,deletecancelsave
        class CombinedExpressionsFormRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public CombinedExpressionsFormRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            class StatementType
            {
                public int Index { get; set; }
                public ExpressionStatementSyntax Row { get; set; }
                public string MethodName { get; set; }
                public List<int> RemovedIndexPositions { get; set; }
            }

            private IEnumerable<StatementType> GetStatementTypesContainMethodName(
                IEnumerable<StatementType> statementTypes, string methodName)
            {
                return statementTypes.Where(x => x.MethodName == methodName)
                    .Where(x => x.Row.DescendantNodesOfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.IdentifierShouldBe("button"))
                    .Where(x => x.Row.MethodNameShouldBe(methodName));
            }
            public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                var t = node.DescendantNodesOfType<ExpressionStatementSyntax>()
                    .Select((r, i) => new StatementType
                    {
                        Row = r,
                        Index = i,
                        MethodName = (semanticModel.GetSymbolInfo(r.Expression).Symbol as IMethodSymbol)?.Name
                    });

                var deleteMethods = GetStatementTypesContainMethodName(t, "Delete");
                var cancelMethods = GetStatementTypesContainMethodName(t, "Cancel");
                var saveMethods = GetStatementTypesContainMethodName(t, "Save");

                int deleteIndex = -1;

                if (deleteMethods.Any())
                {
                    deleteIndex = deleteMethods.FirstOrDefault().Index;
                }
                if (!cancelMethods.Any() || !saveMethods.Any())
                {
                    return base.VisitConstructorDeclaration(node);
                }

                List<int> statementsToRemove = new List<int>();
                cancelMethods.Union(saveMethods).Union(deleteMethods).OrderByDescending(x => x.Index)
                    .Aggregate((arg1, arg2) =>
                    {
                        if (arg1.MethodName == "Save" && arg2.MethodName == "Cancel")
                        {
                            statementsToRemove.Add(arg1.Index);
                            return new StatementType
                            {
                                Index = arg2.Index,
                                Row = SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ParseExpression("button"),
                                                SyntaxFactory.IdentifierName("CancelSave")),
                                            SyntaxFactory.ArgumentList())),
                                MethodName = "CancelSave",
                                RemovedIndexPositions = new List<int>() { arg1.Index }
                            };
                        }
                        if (arg1.MethodName == "CancelSave" && arg2.MethodName == "Delete")
                        {
                            statementsToRemove.AddRange(arg1.RemovedIndexPositions);
                            statementsToRemove.Add(arg1.Index);
                            return new StatementType
                            {
                                Index = arg2.Index,
                                Row = SyntaxFactory.ExpressionStatement(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.ParseExpression("button"),
                                                SyntaxFactory.IdentifierName("DeleteCancelSave")),
                                            SyntaxFactory.ArgumentList())),
                                MethodName = "DeleteCancelSave",
                                RemovedIndexPositions = new List<int>(arg1.RemovedIndexPositions) { arg1.Index }
                            };
                        }
                        return arg2;
                    });
                if (!statementsToRemove.Any())
                    return base.VisitConstructorDeclaration(node);
                node = node.RemoveNodes(cancelMethods.Union(saveMethods).Union(deleteMethods)
                    .Where(x => statementsToRemove.Contains(x.Index)).Select(x => x.Row),
                    SyntaxRemoveOptions.KeepEndOfLine);
                t = node.DescendantNodesOfType<ExpressionStatementSyntax>()
                    .Select((r, i) => new StatementType
                    {
                        Row = r,
                        Index = i,
                        MethodName = r.FirstDescendantNode<MemberAccessExpressionSyntax>().Name.ToString()
                    });

                deleteMethods = GetStatementTypesContainMethodName(t, "Delete");
                cancelMethods = GetStatementTypesContainMethodName(t, "Cancel");
                saveMethods = GetStatementTypesContainMethodName(t, "Save");

                node = node.ReplaceNodes(cancelMethods.Union(saveMethods).Union(deleteMethods)
                    .Select(x => x.Row),
                        (arg1, arg2) =>
                        {
                            if (arg1.MethodNameShouldBe("Delete"))
                            {
                                return SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParseExpression("button"),
                                                    SyntaxFactory.IdentifierName("DeleteCancelSave")),
                                                SyntaxFactory.ArgumentList()));
                            }
                            if (arg1.MethodNameShouldBe("Cancel"))
                            {
                                return SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParseExpression("button"),
                                                    SyntaxFactory.IdentifierName("CancelSave")),
                                                SyntaxFactory.ArgumentList()));
                            }
                            return arg2;
                        });
                return node;
            }
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.ClassShouldHaveBase() &&
                    node.ClassShouldHaveGenericBase() &&
                    node.GenericClassShouldInheritFrom("FormModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }
        }
        class IfWorkFlowRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public IfWorkFlowRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
            {
                var ifNode = node.DescendantNodesOfType<InvocationExpressionSyntax>()
                    .Where(x =>
                        x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                        x.MethodNameShouldBe("If") &&
                        x.LeftSideShouldBeIdentifier(false))
                    .FirstOrDefault();
                if (ifNode != null)
                {
                    node = node.ReplaceNodes(ifNode.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.MethodNameShouldBe("If") &&
                                nde1.LeftSideShouldBeIdentifier(false))
                            {
                                return nde2.FirstDescendantNode<InvocationExpressionSyntax>();
                            }
                            else if (nde1.MethodNameShouldBe("If") &&
                               nde1.LeftSideShouldBeIdentifier())
                            {
                                return nde2;
                            }
                            if (nde1.LeftSideShouldBeIdentifier())
                            {
                                return SyntaxFactory.InvocationExpression(
                                       SyntaxFactory.MemberAccessExpression(
                                           SyntaxKind.SimpleMemberAccessExpression,
                                           SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParseExpression(nde1.GetLeftSideIdentifier()),
                                                    SyntaxFactory.IdentifierName("If"))
                                                , SyntaxFactory.ArgumentList(ifNode.ArgumentList.Arguments)), nde1.GetRightSideNameSyntax()),
                                    SyntaxFactory.ArgumentList(nde1.ArgumentList.Arguments)).WithLeadingTrivia(nde1.GetLeadingTrivia());
                            }
                            return nde2;
                        });
                    return VisitSimpleLambdaExpression(node);
                }
                return node;
            }
        }
        class CustomColumnRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public CustomColumnRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var customColumnNode = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                    .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "CustomColumn").FirstOrDefault();

                if (customColumnNode != null &&
                    (node.GetArgumentsOfMethod("LabelText") != null || node.GetArgumentsOfMethod("HeaderTemplate") != null) &&
                    node.GetArgumentsOfMethod("DisplayExpression") != null)
                {
                    var newNode = node.ReplaceNodes(node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.ToString() == "CustomColumn()")
                            {
                                SeparatedSyntaxList<ArgumentSyntax> args = new SeparatedSyntaxList<ArgumentSyntax>();
                                var argsLabelText = node.GetArgumentsOfMethod("LabelText")
                                    ?? node.GetArgumentsOfMethod("HeaderTemplate");
                                var argsDisplayExpr = node.GetArgumentsOfMethod("DisplayExpression");

                                if (argsLabelText?.Arguments.Count == 1)
                                    args = args.Add(argsLabelText.Arguments.FirstOrDefault());

                                if (argsDisplayExpr?.Arguments.Count == 1)
                                    args = args.Add(argsDisplayExpr.Arguments.FirstOrDefault());

                                return SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                         SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ParseExpression("column"),
                                    SyntaxFactory.IdentifierName("Custom")),
                                    SyntaxFactory.ArgumentList(args))
                                .WithLeadingTrivia(nde1.GetLeadingTrivia());
                            }
                            else if (nde1.MethodNameShouldBeIn(new string[] { "LabelText", "HeaderTemplate", "DisplayExpression" })
                                && nde1.ArgumentsCountShouldBe(1))
                            {
                                return nde2.FirstDescendantNode<InvocationExpressionSyntax>();
                            }

                            return nde2;
                        });
                    return newNode;
                }
                return base.VisitInvocationExpression(node);
            }
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.ClassShouldHaveBase() &&
                   node.ClassShouldHaveGenericBase() &&
                   node.GenericClassShouldInheritFrom("ListModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }
        }
        class SendItemIdRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public SendItemIdRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                var methodName = methodSymbol?.Name;
                if (methodName == "Send")
                {
                    if (node.ArgumentsCountShouldBe(2) &&
                        node.FirstArgumentShouldBe("\"item\"") &&
                        node.LastArgumentShouldBe("\"item.ID\""))
                    {
                        var member = node.FirstDescendantNode<InvocationExpressionSyntax>();
                        member = member
                            .WithTrailingTrivia(member.GetTrailingTrivia().Union(node.GetLeadingTrivia()));
                        return SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression, member, SyntaxFactory.IdentifierName("SendItemId")),
                            SyntaxFactory.ArgumentList());
                    }
                }
                return base.VisitInvocationExpression(node);
            }
        }
        //column.search.field.
        class ElementsNewSyntaxRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public ElementsNewSyntaxRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var methodSymbol = semanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                var methodName = methodSymbol?.Name;
                var methodType = methodSymbol?.ReturnType.OriginalDefinition?.ToString();

                var methodDefTypes = new string[] {
                    "MSharp.PropertyFilterElement<TEntity>",
                    "MSharp.ViewElement<TEntity>",
                    "MSharp.ViewElement<TEntity, TProperty>",
                    "MSharp.NumberFormElement",
                    "MSharp.BooleanFormElement",
                    "MSharp.DateTimeFormElement",
                    "MSharp.StringFormElement",
                    "MSharp.AssociationFormElement",
                    "MSharp.BinaryFormElement",
                    "MSharp.CommonFilterElement<TEntity>"
                };
                var methodDefNames = new string[] { "Field", "Column", "Search" };

                if (methodDefTypes.Contains(methodType) && methodDefNames.Contains(methodName))
                {
                    if (node.ArgumentsCountShouldBe(1) && node.ArgumentList.Arguments[0].Expression is SimpleLambdaExpressionSyntax)
                    {
                        var arg = node.ArgumentList.Arguments[0].Expression.As<SimpleLambdaExpressionSyntax>()?
                            .Body.As<MemberAccessExpressionSyntax>()?.Name;
                        return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            node.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) ?
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                node.GetLeftSideExpression(),
                                SyntaxFactory.IdentifierName(node.GetRightSideNameSyntax()?.ToString().ToLower()))
                            : SyntaxFactory.ParseExpression(methodName.ToLower()),
                            SyntaxFactory.IdentifierName(arg.ToString())))
                            .WithLeadingTrivia(node.GetLeadingTrivia());
                    }
                }

                if (methodType == "MSharp.ListButton<TEntity>" || methodType == "MSharp.ModuleButton")
                {
                    switch (methodName)
                    {
                        case "ButtonColumn":
                            return node.WithExpression(SyntaxFactory.ParseExpression("column.Button"))
                                .WithLeadingTrivia(node.GetLeadingTrivia()); ;
                        case "LinkColumn":
                            return node.WithExpression(SyntaxFactory.ParseExpression("column.Link"))
                                .WithLeadingTrivia(node.GetLeadingTrivia());
                        case "SearchButton":
                            return node.WithExpression(SyntaxFactory.ParseExpression("search.Button"))
                                .WithLeadingTrivia(node.GetLeadingTrivia());
                    }
                }
                return base.VisitInvocationExpression(node);
            }
        }
        //cancel/save/delete
        class FormModuleWorkFlowRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public FormModuleWorkFlowRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var s = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "OnClick").FirstOrDefault();
                if (s == null)
                    return base.VisitInvocationExpression(node);
                var methodSymbol = (semanticModel.GetSymbolInfo(s).Symbol as IMethodSymbol);

                if (s.DescendantNodesOfType<SimpleLambdaExpressionSyntax>().Count() == 1 &&
                    methodSymbol?.ReturnType.OriginalDefinition?.ToString() == "MSharp.ModuleButton")
                {
                    var saveMethods = new string[] { "ReturnToPreviousPage", "SaveInDatabase" };
                    var saveModalMethods = new string[] { "CloseModal(Refresh.Full)", "SaveInDatabase" };
                    var cancelMethods = new string[] { "ReturnToPreviousPage" };
                    var cancelModalMethods = new string[] { "CloseModal" };
                    var deleteMethods = new string[] { "ReturnToPreviousPage", "DeleteItem" };
                    var deleteModalMethods = new string[] { "CloseModal(Refresh.Full)", "DeleteItem" };
                    var optionalMethods = new string[] { "GentleMessage(\"Deleted successfully.\")" };

                    var lambdaExpressionArgument = s.FirstDescendantNode<SimpleLambdaExpressionSyntax>();
                    var invocations = lambdaExpressionArgument.DescendantNodesOfType<InvocationExpressionSyntax>();
                    if (invocations.Count() == 1 &&
                        (invocations.All(x => cancelMethods.Any(y => x.ToString().Contains(y))) ||
                     (invocations.All(x => cancelModalMethods.Any(y => x.ToString().Contains(y))))))
                    {
                        var m = node;
                        var methodSelector = invocations.Any(x => x.ToString().Contains("ReturnToPreviousPage"));
                        var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("button"),
                                SyntaxFactory.IdentifierName(methodSelector ? "Cancel" : "ModalCancel")),
                            SyntaxFactory.ArgumentList());

                        while (m != null && m.ChildNodes().Any())
                        {
                            var m2 = m.ChildNodes();
                            if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                                m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>();
                                var arguments = m2.LastOrDefault().As<ArgumentListSyntax>();
                                if (methodName.MethodNameShouldBe("Icon") && (!arguments.ArgumentsCountShouldBe(1)
                                    || !arguments.FirstArgumentShouldBe("FA.Backward")))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.MethodNameShouldBe("CausesValidation") && (!arguments.ArgumentsCountShouldBe(1)
                                        || !arguments.FirstArgumentShouldBe("false")))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.MethodNameShouldBe("OnClick") && (!arguments.ArgumentsCountShouldBe(1) ||
                                    ((!arguments.FirstArgumentShouldContains("ReturnToPreviousPage")) &&
                                    (!arguments.FirstArgumentShouldContains("CloseModal")))))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                else if (!methodName.MethodNameShouldBeIn(new string[] { "OnClick", "CausesValidation", "Icon" }))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                m = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>()?.Expression
                                    .As<InvocationExpressionSyntax>();
                            }
                            else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                               m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault().As<IdentifierNameSyntax>();
                                var arguments = m2.LastOrDefault().As<ArgumentListSyntax>();
                                if (methodName.ToString() == "Button" && (!arguments.ArgumentsCountShouldBe(1)
                                        || !arguments.FirstArgumentShouldBe("\"Cancel\"")))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                m = m2.FirstOrDefault().As<InvocationExpressionSyntax>();
                            }
                            else return node;
                        }
                        return newExpression
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithTrailingTrivia(node.GetTrailingTrivia());
                    }
                    else if ((invocations.Count() == 2 || invocations.Count() == 3) &&
                        invocations.All(x => x.Expression is MemberAccessExpressionSyntax &&
                        x.LeftSideShouldBeIdentifier()) &&
                        (invocations.All(x => saveMethods.Any(y => x.ToString().Contains(y))) ||
                        invocations.All(x => saveModalMethods.Any(y => x.ToString().Contains(y)))))
                    {
                        var m = node;
                        var methodSelector = invocations.Any(x => x.ToString().Contains("ReturnToPreviousPage"));
                        var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("button"),
                                SyntaxFactory.IdentifierName(methodSelector ? "Save" : "ModalSave")),
                            SyntaxFactory.ArgumentList());

                        while (m != null && m.ChildNodes().Any())
                        {
                            var m2 = m.ChildNodes();
                            if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                                m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>();
                                var arguments = m2.LastOrDefault().As<ArgumentListSyntax>();
                                if (methodName.MethodNameShouldBe("Icon") && (!arguments.ArgumentsCountShouldBe(1)
                                    || !arguments.FirstArgumentShouldBe("FA.Check")))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.MethodNameShouldBe("CausesValidation") && (!arguments.ArgumentsCountShouldBe(1)
                                    || !arguments.FirstArgumentShouldBe("true")))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.MethodNameShouldBe("OnClick") && ((!arguments.ArgumentsCountShouldBe(1)) ||
                                    !arguments.GetBlockSyntaxOfFirstArgument()
                                        .Statements.Any(x => saveMethods.Any(y => x.ToString().Contains(y))) ||
                                    !arguments.GetBlockSyntaxOfFirstArgument()
                                        .Statements.Any(x => saveModalMethods.Any(y => x.ToString().Contains(y)))))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                else if (!methodName.MethodNameShouldBeIn(new string[] { "OnClick", "CausesValidation", "Icon" }))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                m = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>()?.Expression
                                     .As<InvocationExpressionSyntax>();
                            }
                            else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                               m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault().As<IdentifierNameSyntax>();
                                var arguments = m2.LastOrDefault().As<ArgumentListSyntax>();
                                if (methodName.ToString() == "Button" && (!arguments.ArgumentsCountShouldBe(1)
                                        || !arguments.FirstArgumentShouldBe("\"Save\"")))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                m = m2.FirstOrDefault().As<InvocationExpressionSyntax>();
                            }
                            else return node;
                        }
                        return newExpression
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithTrailingTrivia(node.GetTrailingTrivia());
                    }
                    else if ((invocations.Count() == 2 || invocations.Count() == 3) &&
                        invocations.All(x => x.Expression is MemberAccessExpressionSyntax &&
                        x.LeftSideShouldBeIdentifier()) &&
                        (invocations.All(x => deleteMethods.Union(optionalMethods).Any(y => x.ToString().Contains(y))) ||
                        invocations.All(x => deleteModalMethods.Union(optionalMethods).Any(y => x.ToString().Contains(y)))))
                    {
                        var m = node;
                        var methodSelector = invocations.Any(x => x.ToString().Contains("ReturnToPreviousPage"));
                        var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("button"),
                                SyntaxFactory.IdentifierName(methodSelector ? "Delete" : "ModalDelete")),
                            SyntaxFactory.ArgumentList());

                        while (m != null && m.ChildNodes().Any())
                        {
                            var m2 = m.ChildNodes();
                            if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                                m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>();
                                var arguments = m2.LastOrDefault().As<ArgumentListSyntax>();
                                if (methodName.MethodNameShouldBe("VisibleIf") && (!arguments.ArgumentsCountShouldBe(1)
                                    || !arguments.FirstArgumentShouldBe("CommonCriterion.IsEditMode_Item_IsNew")))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.MethodNameShouldBe("Icon") && (!arguments.ArgumentsCountShouldBe(1)
                                        || !arguments.FirstArgumentShouldBe("FA.Close")))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.MethodNameShouldBe("ConfirmQuestion") && (!arguments.ArgumentsCountShouldBe(1)
                                        || !arguments.FirstArgumentShouldBe("\"Are you sure you want to delete it?\"")))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.MethodNameShouldBe("OnClick") && ((!arguments.ArgumentsCountShouldBe(1)) ||
                                    !arguments.GetBlockSyntaxOfFirstArgument()
                                        .Statements.Any(x => deleteMethods.Any(y => x.ToString().Contains(y))) ||
                                    !arguments.GetBlockSyntaxOfFirstArgument()
                                        .Statements.Any(x => deleteModalMethods.Any(y => x.ToString().Contains(y)))))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                else if (!methodName.MethodNameShouldBeIn(new string[] {
                                   "OnClick","VisibleIf","Icon","ConfirmQuestion"}))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                m = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>()?.Expression
                                     .As<InvocationExpressionSyntax>();
                            }
                            else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                               m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault().As<IdentifierNameSyntax>();
                                var arguments = m2.LastOrDefault().As<ArgumentListSyntax>();
                                if (methodName.ToString() == "Button" && (!arguments.ArgumentsCountShouldBe(1)
                                        || !arguments.FirstArgumentShouldBe("\"Delete\"")))
                                {
                                    return base.VisitInvocationExpression(node);
                                }
                                m = m2.FirstOrDefault().As<InvocationExpressionSyntax>();
                            }
                            else return node;
                        }
                        return newExpression
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithTrailingTrivia(node.GetTrailingTrivia());
                    }


                }
                return base.VisitInvocationExpression(node);
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.ClassShouldHaveBase() &&
                   node.ClassShouldHaveGenericBase() &&
                   node.GenericClassShouldInheritFrom("FormModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }
        }
        //onlick-go -> go
        class GeneralWorkFlowRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public GeneralWorkFlowRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var s = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "OnClick").FirstOrDefault();
                if (s == null)
                    return base.VisitInvocationExpression(node);
                var methodSymbol = (semanticModel.GetSymbolInfo(s).Symbol as IMethodSymbol);

                if (s.ArgumentsCountShouldBe(1) &&
                    (s.FirstArgument()
                    .DescendantNodesOfType<ExpressionStatementSyntax>().Count() == 1 ||
                    s.FirstArgument()
                    .DescendantNodesOfType<SimpleLambdaExpressionSyntax>().Count() == 1) &&
                    s.FirstArgument()
                    .DescendantNodesOfType<GenericNameSyntax>().Any(x => x.Identifier.ToString() == "Go"))
                {
                    GenericNameSyntax goIdentifier = s.FirstArgument()
                        .DescendantNodesOfType<GenericNameSyntax>()
                        .FirstOrDefault(x => x.Identifier.ToString() == "Go");

                    var newNode = node.ReplaceNodes(s.FirstArgument()
                        .DescendantNodesOfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.Expression.As<MemberAccessExpressionSyntax>()?.Name is GenericNameSyntax &&
                            nde1.MethodNameShouldBe("Go") &&
                                nde1.ArgumentsCountShouldBe(0))
                            {
                                if (nde1.LeftSideShouldBeIdentifier(false))
                                    return nde2.FirstDescendantNode<InvocationExpressionSyntax>();
                                else
                                    return nde2.FirstDescendantNode<IdentifierNameSyntax>();
                            }
                            return nde2;
                        });

                    newNode = newNode.ReplaceNode(newNode.DescendantNodesAndSelfOfType<IdentifierNameSyntax>()
                            .FirstOrDefault(x => x.Identifier.ToString() == "OnClick"),
                                SyntaxFactory.GenericName(goIdentifier.Identifier, goIdentifier.TypeArgumentList));

                    var argument = newNode.FirstArgument().Expression.As<SimpleLambdaExpressionSyntax>();
                    if (argument != null && argument.Body.IsKind(SyntaxKind.IdentifierName))
                        return newNode.WithArgumentList(SyntaxFactory.ArgumentList());
                    return newNode;
                }
                return base.VisitInvocationExpression(node);
            }
        }
        class ListModuleWorkFlowRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public ListModuleWorkFlowRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var s = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "Go" ||
                        (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "OnClick" ||
                        (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "Link").FirstOrDefault();
                if (s == null)
                    return base.VisitInvocationExpression(node);
                var editRequiredArguments = new string[] { "SendItemId", "SendReturnUrl" };
                var newRequiredArguments = new string[] { "SendReturnUrl" };
                var methodSymbol = (semanticModel.GetSymbolInfo(s).Symbol as IMethodSymbol);

                if (s.DescendantNodesOfType<SimpleLambdaExpressionSyntax>().Count() == 1 &&
                    s.ArgumentsCountShouldBe(1) &&
                    editRequiredArguments.All(x => (s.FirstArgument().DescendantNodesOfType<InvocationExpressionSyntax>()
                        .Select(y => y.GetRightSideNameSyntax()?.ToString())).Any(y => y == x)) &&
                    s.DescendantNodesOfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                       x.LeftSideShouldBeIdentifier() &&
                       x.GetLeftSideExpression().As<IdentifierNameSyntax>()?.Identifier.ToString() == "column").Count() == 1)
                {
                    var m = node;
                    var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("column"),
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("Edit"),
                                s.Expression.As<MemberAccessExpressionSyntax>()?.Name.As<GenericNameSyntax>()?.TypeArgumentList)),
                            SyntaxFactory.ArgumentList());
                    while (m != null && m.ChildNodes().Any())
                    {
                        var m2 = m.ChildNodes();
                        if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                            m2.LastOrDefault() is ArgumentListSyntax)
                        {
                            var methodName = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>();
                            var arguments = m2.LastOrDefault().As<ArgumentListSyntax>();
                            if (methodName.MethodNameShouldBe("Go") && ((!arguments.ArgumentsCountShouldBe(1)) ||
                                   !(editRequiredArguments.All(x => arguments.FirstArgument()
                                   .Expression.DescendantNodesOfType<InvocationExpressionSyntax>()
                                   .FirstOrDefault().ToString().Contains(x)))))
                            {
                                return base.VisitInvocationExpression(node);
                            }
                            else if (methodName.MethodNameShouldBe("Icon") && (!arguments.ArgumentsCountShouldBe(1)
                                        || !arguments.FirstArgumentShouldBe("FA.Edit")))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (methodName.MethodNameShouldBe("NoText") && (!arguments.ArgumentsCountShouldBe(0)))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (methodName.MethodNameShouldBe("HeaderText") && (!arguments.ArgumentsCountShouldBe(1)
                                        || !arguments.FirstArgumentShouldBe("\"Actions\"")))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (methodName.MethodNameShouldBe("GridColumnCssClass") && (!arguments.ArgumentsCountShouldBe(1)
                                        || !arguments.FirstArgumentShouldBe("\"actions\"")))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (methodName.MethodNameShouldBe("Button") && (!arguments.ArgumentsCountShouldBe(1)
                                        || !arguments.FirstArgumentShouldBe("\"Edit\"")))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (
                                !methodName.MethodNameShouldBeIn(new string[] {
                                "Go","GridColumnCssClass","HeaderText","NoText",
                                "Icon" , "Button" }))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            m = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>()?.Expression
                                .As<InvocationExpressionSyntax>();
                        }
                        else return node;
                    }
                    return newExpression
                                .WithLeadingTrivia(node.GetLeadingTrivia())
                                .WithTrailingTrivia(node.GetTrailingTrivia());
                }
                else if (s.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().Count() == 1 &&
                            s.ArgumentsCountShouldBe(1) &&
                            newRequiredArguments.All(x => (s.FirstArgument().DescendantNodesOfType<InvocationExpressionSyntax>()
                            .Select(y => y.GetRightSideNameSyntax()?.ToString())).Any(y => y == x)))
                {
                    var m = node;
                    var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("button"),
                                SyntaxFactory.GenericName(SyntaxFactory.Identifier("New"),
                                s.Expression.As<MemberAccessExpressionSyntax>()?.Name.As<GenericNameSyntax>()?.TypeArgumentList)),
                            SyntaxFactory.ArgumentList());
                    while (m != null && m.ChildNodes().Any())
                    {
                        var m2 = m.ChildNodes();
                        if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                            m2.LastOrDefault() is ArgumentListSyntax)
                        {
                            var methodName = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>();
                            var arguments = m2.LastOrDefault().As<ArgumentListSyntax>();
                            if (methodName.Name.Identifier.ToString() == "Go" && ((!arguments.ArgumentsCountShouldBe(1)) ||
                                   !(newRequiredArguments.All(x => arguments.FirstArgument()
                                   .Expression.DescendantNodesOfType<InvocationExpressionSyntax>()
                                   .FirstOrDefault().ToString().Contains(x)))))
                            {
                                return base.VisitInvocationExpression(node);
                            }
                            else if (methodName.MethodNameShouldBe("Icon") && (!arguments.ArgumentsCountShouldBe(1)
                                        || !arguments.FirstArgumentShouldBe("FA.Plus")))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            else if (!methodName.MethodNameShouldBeIn(new string[] { "Go", "Icon" }))
                            {
                                newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                            }
                            m = m2.FirstOrDefault().As<MemberAccessExpressionSyntax>()?.Expression
                                .As<InvocationExpressionSyntax>();
                        }
                        else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                             m2.LastOrDefault() is ArgumentListSyntax)
                        {
                            var methodName = m2.FirstOrDefault().As<IdentifierNameSyntax>();
                            var arguments = m2.LastOrDefault().As<ArgumentListSyntax>();
                            if (methodName.ToString() == "Button" && (!arguments.ArgumentsCountShouldBe(1)
                                    || !arguments.FirstArgumentShouldBe("\"New\"")))
                            {
                                return base.VisitInvocationExpression(node);
                            }
                            m = m2.FirstOrDefault().As<InvocationExpressionSyntax>();
                        }
                        else return node;
                    }
                    return newExpression
                                .WithLeadingTrivia(node.GetLeadingTrivia())
                                .WithTrailingTrivia(node.GetTrailingTrivia());
                }
                else if (s.ArgumentsCountShouldBe(1) &&
                    (s.FirstArgument()
                    .DescendantNodesOfType<ExpressionStatementSyntax>().Count() == 1 ||
                    s.FirstArgument()
                    .DescendantNodesOfType<SimpleLambdaExpressionSyntax>().Count() == 1) &&
                    s.FirstArgument()
                    .DescendantNodesOfType<IdentifierNameSyntax>().Any(x => x.Identifier.ToString() == "Export") &&
                    s.DescendantNodesOfType<InvocationExpressionSyntax>().Where(x => x.Expression is IdentifierNameSyntax &&
                        x.Expression.As<IdentifierNameSyntax>()?.Identifier.ToString() == "Button" &&
                        x.ArgumentsCountShouldBe(1) &&
                        x.FirstArgumentShouldBe("\"Export\"")).Count() == 1)
                {
                    InvocationExpressionSyntax exportInvocation = s.DescendantNodesOfType<InvocationExpressionSyntax>()
                        .FirstOrDefault(x => x.Expression is MemberAccessExpressionSyntax &&
                            x.MethodNameShouldBe("Export"));
                    var neededArguments = exportInvocation.ArgumentList;
                    var newNode = node.ReplaceNodes(s.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.Expression is MemberAccessExpressionSyntax &&
                            nde1.GetRightSideNameSyntax() is IdentifierNameSyntax &&
                            nde1.MethodNameShouldBe("OnClick") &&
                            nde1.ArgumentsCountShouldBe(1))
                            {
                                if (nde1.LeftSideShouldBeIdentifier(false))
                                    return nde2.FirstDescendantNode<InvocationExpressionSyntax>();
                                else return nde2.FirstDescendantNode<IdentifierNameSyntax>();
                            }
                            else if (nde1.Expression is IdentifierNameSyntax &&
                               ((IdentifierNameSyntax)nde1.Expression).Identifier.ToString() == "Button" &&
                               nde1.ArgumentsCountShouldBe(1) &&
                               nde1.FirstArgumentShouldBe("\"Export\""))
                            {
                                return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ParseExpression("button"),
                                    SyntaxFactory.IdentifierName("Export")), neededArguments)
                                        .WithLeadingTrivia(nde1.GetLeadingTrivia())
                                        .WithTrailingTrivia(nde1.GetTrailingTrivia());
                            }

                            return nde2;
                        });

                    return newNode;
                }
                else if (s.ArgumentsCountShouldBe(1) &&
                    (s.FirstArgument()
                    .DescendantNodesOfType<ExpressionStatementSyntax>().Count() == 1 ||
                    s.FirstArgument()
                    .DescendantNodesOfType<SimpleLambdaExpressionSyntax>().Count() == 1) &&
                    s.FirstArgument()
                    .DescendantNodesOfType<IdentifierNameSyntax>().Any(x => x.Identifier.ToString() == "Reload") &&
                    s.DescendantNodesOfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                        x.MethodNameShouldBe("Button") &&
                        x.ArgumentsCountShouldBe(1) &&
                        x.FirstArgumentShouldBe("\"Search\"")).Count() == 1)
                {
                    InvocationExpressionSyntax iconInvocation = s.DescendantNodesOfType<InvocationExpressionSyntax>()
                            .FirstOrDefault(x => x.Expression is MemberAccessExpressionSyntax &&
                            x.MethodNameShouldBe("Icon"));
                    var neededArguments = iconInvocation.ArgumentList;
                    var newNode = node.ReplaceNodes(s.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.Expression is MemberAccessExpressionSyntax &&
                           nde1.GetRightSideNameSyntax() is IdentifierNameSyntax &&
                            nde1.MethodNameShouldBe("OnClick") &&
                                nde1.ArgumentsCountShouldBe(1))
                            {
                                if (nde1.LeftSideShouldBeIdentifier(false))
                                    return nde2.FirstDescendantNode<InvocationExpressionSyntax>();
                                else return nde2.FirstDescendantNode<IdentifierNameSyntax>();
                            }
                            else if (nde1.Expression is MemberAccessExpressionSyntax &&
                           nde1.GetRightSideNameSyntax() is IdentifierNameSyntax &&
                            nde1.MethodNameShouldBe("Icon") &&
                                nde1.ArgumentsCountShouldBe(1))
                            {
                                if (nde1.LeftSideShouldBeIdentifier(false))
                                    return nde2.FirstDescendantNode<InvocationExpressionSyntax>();
                                else return nde2.FirstDescendantNode<IdentifierNameSyntax>();
                            }
                            else if (nde1.Expression is MemberAccessExpressionSyntax &&
                                nde1.GetRightSideNameSyntax() is IdentifierNameSyntax &&
                                nde1.MethodNameShouldBe("NoText") &&
                                nde1.ArgumentsCountShouldBe(1))
                            {
                                if (nde1.LeftSideShouldBeIdentifier(false))
                                    return nde2.FirstDescendantNode<InvocationExpressionSyntax>();
                                else return nde2.FirstDescendantNode<IdentifierNameSyntax>();
                            }
                            else if (nde1.Expression is MemberAccessExpressionSyntax &&
                               nde1.MethodNameShouldBe("Button") &&
                               nde1.ArgumentsCountShouldBe(1) &&
                               nde1.FirstArgumentShouldBe("\"Search\""))
                            {
                                SeparatedSyntaxList<ArgumentSyntax> args = new SeparatedSyntaxList<ArgumentSyntax>();
                                args = args.AddRange(neededArguments.Arguments.Where(x => x.Expression.ToString() != "FA.Search"));
                                return SyntaxFactory.InvocationExpression(SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ParseExpression("search"),
                                    SyntaxFactory.IdentifierName("Icon")), SyntaxFactory.ArgumentList(args))
                                            .WithLeadingTrivia(nde1.GetLeadingTrivia())
                                            .WithTrailingTrivia(nde1.GetTrailingTrivia());
                            }

                            return nde2;
                        });

                    return newNode;
                }
                else if (node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                         x.LeftSideShouldBeIdentifier() &&
                         x.GetLeftSideExpression().As<IdentifierNameSyntax>()?.Identifier.ToString() == "column").Count() == 1 &&
                         node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                        x.MethodNameShouldBe("HeaderText") &&
                        x.ArgumentsCountShouldBe(1)).Count() == 1 &&
                        node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                         x.MethodNameShouldBe("Link") &&
                        x.ArgumentsCountShouldBe(1)).Count() == 1)
                {
                    var linkMethod = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                        x.MethodNameShouldBe("Link") &&
                        x.ArgumentsCountShouldBe(1)).FirstOrDefault();

                    var headerTextMethod = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().Where(x => x.Expression is MemberAccessExpressionSyntax &&
                        x.MethodNameShouldBe("HeaderText") &&
                        x.ArgumentsCountShouldBe(1)).FirstOrDefault();

                    if (linkMethod.FirstArgument().ToString()
                        .Contains("item." + headerTextMethod.ArgumentList.Arguments.FirstOrDefault()
                        .ToString().Trim('"')))
                    {
                        SeparatedSyntaxList<ArgumentSyntax> args = new SeparatedSyntaxList<ArgumentSyntax>();
                        args = args.Add(SyntaxFactory.Argument(
                                SyntaxFactory.SimpleLambdaExpression(SyntaxFactory.Parameter(
                                    SyntaxFactory.ParseToken("x")), null,
                                                SyntaxFactory.MemberAccessExpression
                                                (SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ParseExpression("x"),
                                                SyntaxFactory.IdentifierName(headerTextMethod.ArgumentList.Arguments.FirstOrDefault()
                                    .ToString().Trim('"'))))));
                        var newNode = node.ReplaceNodes(node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>(),
                       (nde1, nde2) =>
                       {
                           if (nde1.Expression is MemberAccessExpressionSyntax &&
                            nde1.GetRightSideNameSyntax() is IdentifierNameSyntax &&
                            nde1.GetRightSideNameSyntax().Identifier.ToString() == "HeaderText" &&
                                nde1.ArgumentsCountShouldBe(1))
                           {
                               if (nde1.LeftSideShouldBeIdentifier(false))
                                   return nde2.FirstDescendantNode<InvocationExpressionSyntax>();
                               else return nde2.FirstDescendantNode<IdentifierNameSyntax>();
                           }
                           else if (nde1.Expression is MemberAccessExpressionSyntax &&
                             nde1.MethodNameShouldBe("Link") &&
                             nde1.ArgumentsCountShouldBe(1))
                           {
                               return SyntaxFactory.InvocationExpression(
                                     SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression
                                     , SyntaxFactory.ParseExpression("column"),
                                     SyntaxFactory.IdentifierName("Link")),
                                     SyntaxFactory.ArgumentList(args))
                                            .WithLeadingTrivia(nde1.GetLeadingTrivia())
                                            .WithTrailingTrivia(nde1.GetTrailingTrivia());
                           }

                           return nde2;
                       });
                        return newNode;
                    }
                }
                return base.VisitInvocationExpression(node);
            }
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.ClassShouldHaveBase() &&
                   node.ClassShouldHaveGenericBase() &&
                   node.GenericClassShouldInheritFrom("ListModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }
        }
        class FullSearchRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            Solution solution;
            RoslynDocument roslynDocument;

            bool shouldConvertToFullSearch = false;
            public FullSearchRewriter(SemanticModel semanticModel,
                Solution solution,
                RoslynDocument roslynDocument)
            {
                this.semanticModel = semanticModel;
                this.solution = solution;
                this.roslynDocument = roslynDocument;
            }

            public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
            {
                var declarationSymbol = semanticModel.GetSymbolInfo(node.Type).Symbol;
                var identifierName = semanticModel.GetDeclaredSymbol(node.Variables.FirstOrDefault());
                if (declarationSymbol?.Name == "ModuleButton" &&
                    node.Variables.Count() == 1)
                {
                    if (node.Variables.FirstOrDefault().Initializer.Value is InvocationExpressionSyntax &&
                        node.Variables.FirstOrDefault()
                        .Initializer.Value.As<InvocationExpressionSyntax>().MethodNameShouldBe("Icon"))
                    {
                        var invocation = node.Variables.FirstOrDefault().Initializer.Value
                            .As<InvocationExpressionSyntax>();
                        var result = Task.Run(() => GetReferencedSymbolsAsync(identifierName)).Result;
                        if (result.Count() == 1 && invocation.ArgumentsCountShouldBe(0))
                        {
                            SyntaxNode nextNode = node.SyntaxTree.GetRoot().FindNode(result.FirstOrDefault().Locations
                                .FirstOrDefault().Location.SourceSpan);
                            if (nextNode.Ancestors().OfType<InvocationExpressionSyntax>().Any())
                            {
                                var isNextInvocOk = CheckNextInvocationExpression(nextNode.Ancestors()
                                    .OfType<InvocationExpressionSyntax>().FirstOrDefault(),
                                    identifierName.ToString());
                                if (isNextInvocOk)
                                {
                                    shouldConvertToFullSearch = true;
                                    return SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            SyntaxFactory.ParseExpression("search"),
                                            SyntaxFactory.IdentifierName("FullWithIcon")),
                                        SyntaxFactory.ArgumentList())
                                        .WithLeadingTrivia(node.GetLeadingTrivia())
                                        .WithTrailingTrivia(node.GetTrailingTrivia());
                                }
                            }
                        }
                    }
                }
                return base.VisitVariableDeclaration(node);
            }
            public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
            {
                if (node.Declaration is VariableDeclarationSyntax)
                {
                    var t = VisitVariableDeclaration(node.Declaration);
                    if (t.IsKind(SyntaxKind.InvocationExpression))
                        return SyntaxFactory.ExpressionStatement(t.As<InvocationExpressionSyntax>())
                            .WithTrailingTrivia(node.GetTrailingTrivia())
                            .WithLeadingTrivia(node.GetLeadingTrivia());
                }
                return base.VisitLocalDeclarationStatement(node);
            }

            public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                return base.VisitVariableDeclarator(node);
            }
            public bool CheckNextInvocationExpression(InvocationExpressionSyntax node
                , string identifierString)
            {
                var s = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "AfterControlAddon").FirstOrDefault();
                while (s.Expression != null)
                {
                    if (s.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                    {
                        var memberExpression = s.Expression.As<MemberAccessExpressionSyntax>();
                        if (memberExpression.MethodNameShouldBe("AfterControlAddon") &&
                            s.ArgumentsCountShouldBe(1) &&
                            s.FirstArgumentShouldBe($"{identifierString}.Ref"))
                        {
                            s = memberExpression.Expression.As<InvocationExpressionSyntax>();
                            continue;
                        }
                    }
                    else if (s.Expression.IsKind(SyntaxKind.IdentifierName))
                    {
                        var identifierExpression = s.Expression.As<IdentifierNameSyntax>();
                        if (identifierExpression.Identifier.ToString() == "Search" &&
                            s.ArgumentsCountShouldBe(1) &&
                            s.FirstArgumentShouldBe("GeneralSearch.AllFields"))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                return false;
            }
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.ClassShouldHaveBase() &&
                   node.ClassShouldHaveGenericBase() &&
                   node.GenericClassShouldInheritFrom("ListModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }

            public async Task<IEnumerable<ReferencedSymbol>> GetReferencedSymbolsAsync(ISymbol symbol)
            {
                return await SymbolFinder.FindReferencesAsync(symbol,
                    solution, documents:
                    ImmutableHashSet.Create(this.roslynDocument));
            }
            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var s = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "AfterControlAddon").FirstOrDefault();
                if (s != null && shouldConvertToFullSearch)
                {
                    shouldConvertToFullSearch = false;
                    return SyntaxFactory.ParseExpression("");
                }
                return base.VisitInvocationExpression(node);
            }

            public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
            {
                var result = base.VisitExpressionStatement(node);
                if (result.ToString() == ";")
                    return null;
                return result;
            }
        }
        class CustomFieldRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public CustomFieldRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var customColumnNode = node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                    .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "CustomField").FirstOrDefault();
                node.GetArgumentsOfMethod("PropertyType");
                if (customColumnNode != null)
                {
                    var newNode = node.ReplaceNodes(node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.ToString() == "CustomField()")
                            {
                                SeparatedSyntaxList<ArgumentSyntax> args = new SeparatedSyntaxList<ArgumentSyntax>();
                                var argsLabel =
                                node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                                    .Any(x =>
                                    x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                    x.MethodNameShouldBe("Label"))
                                    ?
                                node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                                    .Where(x => x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                    x.MethodNameShouldBe("Label")).FirstOrDefault().ArgumentList : null;
                                var argsControlType =
                                node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                                    .Any(x =>
                                    x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                    x.MethodNameShouldBe("Control"))
                                    ?
                                node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                                    .Where(x =>
                                    x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                     x.MethodNameShouldBe("Control")).FirstOrDefault().ArgumentList
                                    : SyntaxFactory.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(
                                        SyntaxFactory.Argument(
                                        SyntaxFactory.ParseExpression("ControlType.Textbox"))));
                                var argsPropertyType =
                                     node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                                     .Any(x =>
                                     x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                                     x.MethodNameShouldBe("PropertyType"))
                                    ? node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>()
                                        .Where(x => x.MethodNameShouldBe("PropertyType")).FirstOrDefault().ArgumentList
                                        : null;

                                if (argsPropertyType != null && argsLabel == null)
                                {
                                    argsLabel = SyntaxFactory.ArgumentList(new SeparatedSyntaxList<ArgumentSyntax>().Add(
                                        SyntaxFactory.Argument(
                                        SyntaxFactory.ParseExpression("\"\""))));
                                }

                                if (argsLabel != null && argsLabel.ArgumentsCountShouldBe(1))
                                    args = args.Add(argsLabel.FirstArgument());

                                if (argsControlType.ArgumentsCountShouldBe(1))
                                    args = args.Add(argsControlType.FirstArgument());

                                return SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                         SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ParseExpression("field"),
                                    (argsPropertyType != null &&
                                        argsPropertyType.Arguments.FirstOrDefault()
                                        .Expression.IsKind(SyntaxKind.StringLiteralExpression) ?
                                    SyntaxFactory.GenericName("Custom")
                                        .WithTypeArgumentList(
                                        SyntaxFactory.TypeArgumentList(new SeparatedSyntaxList<TypeSyntax>()
                                        .Add(SyntaxFactory.ParseTypeName(argsPropertyType.FirstArgument()
                                        .ToString().Trim('"'))))) as SimpleNameSyntax
                                    : SyntaxFactory.IdentifierName("Custom") as SimpleNameSyntax)),
                                    SyntaxFactory.ArgumentList(args))
                                .WithLeadingTrivia(nde1.GetLeadingTrivia());
                            }
                            else if (nde1.MethodNameShouldBe("Label") &&
                               nde1.ArgumentsCountShouldBe(1))
                            {
                                return node.MethodNameShouldBe("Label") ?
                                    nde2.FirstDescendantNode<InvocationExpressionSyntax>().WithoutTrailingTrivia() :
                                    nde2.FirstDescendantNode<InvocationExpressionSyntax>();
                            }
                            else if (nde1.MethodNameShouldBe("Control") &&
                              nde1.ArgumentsCountShouldBe(1))
                            {
                                return node.MethodNameShouldBe("Control") ?
                                    nde2.FirstDescendantNode<InvocationExpressionSyntax>().WithoutTrailingTrivia() :
                                    nde2.FirstDescendantNode<InvocationExpressionSyntax>();
                            }
                            else if (nde1.MethodNameShouldBe("PropertyType") &&
                               nde1.ArgumentsCountShouldBe(1) &&
                                   nde1.GetArgumentsOfMethod("PropertyType").FirstArgument()
                                    .Expression.IsKind(SyntaxKind.StringLiteralExpression))
                            {
                                return
                                    node.MethodNameShouldBe("PropertyType") ?
                                    nde2.FirstDescendantNode<InvocationExpressionSyntax>().WithoutTrailingTrivia() :
                                    nde2.FirstDescendantNode<InvocationExpressionSyntax>();
                            }

                            return nde2;
                        });
                    return newNode;
                }
                return base.VisitInvocationExpression(node);
            }
            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.ClassShouldHaveBase() &&
                   node.ClassShouldHaveGenericBase() &&
                   node.GenericClassShouldInheritFrom("FormModule"))
                    return base.VisitClassDeclaration(node);
                return node;
            }
        }
        //onclick,go method should be last invocation
        class OnClickGoWorkFlowRewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public OnClickGoWorkFlowRewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var listInvocations = new string[] { "OnClick" };
                var invocation = node.DescendantNodesOfType<InvocationExpressionSyntax>()
                    .Where(x =>
                        x.Expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
                        x.MethodNameShouldBeIn(listInvocations))
                    .FirstOrDefault();
                if (invocation != null)
                {
                    node = node.ReplaceNodes(node.DescendantNodesAndSelfOfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.MethodNameShouldBeIn(listInvocations))
                            {
                                return nde2.GetLeftSideExpression();
                            }
                            return nde2;
                        });

                    return SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        node,
                        invocation.GetRightSideNameSyntax().WithoutTrailingTrivia()),
                        invocation.ArgumentList.WithoutTrailingTrivia());
                }
                return base.VisitInvocationExpression(node);
            }
        }
    }
}