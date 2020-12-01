using Geeks.GeeksProductivityTools;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Geeks.VSIX.TidyCSharp.Cleanup
{
    public class MSharpUICleaner : CodeCleanerCommandRunnerBase, ICodeCleaner
    {
        public override SyntaxNode CleanUp(SyntaxNode initialSourceNode)
        {
            if (App.DTE.ActiveDocument.ProjectItem.ProjectItems.ContainingProject.Name == "#UI")
                return ChangeMethodHelper(initialSourceNode, ProjectItemDetails.SemanticModel);
            return initialSourceNode;
        }

        SyntaxNode ChangeMethodHelper(SyntaxNode initialSourceNode, SemanticModel semanticModel)
        {
            initialSourceNode = new Rewriter(semanticModel).Visit(initialSourceNode);
            this.RefreshResult(initialSourceNode);
            return initialSourceNode;
        }

        class Rewriter : CSharpSyntaxRewriter
        {
            SemanticModel semanticModel;
            public Rewriter(SemanticModel semanticModel) => this.semanticModel = semanticModel;

            class StatementType
            {
                public int Index { get; set; }
                public ExpressionStatementSyntax Row { get; set; }
                public string MethodName { get; set; }
                public List<int> RemovedIndexPositions { get; set; }
            }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node == null) return base.Visit(node);

                if (node is ConstructorDeclarationSyntax)
                {
                    return VisitConstructorDeclaration(node as ConstructorDeclarationSyntax);
                }
                else if (node is InvocationExpressionSyntax)
                {
                    return VisitInvocationExpression(node as InvocationExpressionSyntax);
                }
                else if (node is ExpressionStatementSyntax)
                {
                    return VisitExpressionStatement(node as ExpressionStatementSyntax);
                }
                return base.Visit(node);
            }

            //cancelsave,deletecancelsave
            public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
            {
                var t = node.DescendantNodes().OfType<ExpressionStatementSyntax>()
                    .Select((r, i) => new StatementType
                    {
                        Row = r,
                        Index = i,
                        MethodName = (semanticModel.GetSymbolInfo(r.Expression).Symbol as IMethodSymbol)?.Name
                    });
                var deleteMethods = t.Where(x => x.MethodName == "Delete")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Delete");
                var cancelMethods = t.Where(x => x.MethodName == "Cancel")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Cancel");
                var saveMethods = t.Where(x => x.MethodName == "Save")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Save");
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
                t = node.DescendantNodes().OfType<ExpressionStatementSyntax>()
                    .Select((r, i) => new StatementType
                    {
                        Row = r,
                        Index = i,
                        MethodName = r.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString()
                    });
                deleteMethods = t.Where(x => x.MethodName == "Delete")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Delete");
                cancelMethods = t.Where(x => x.MethodName == "Cancel")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Cancel");
                saveMethods = t.Where(x => x.MethodName == "Save")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count() == 1)
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Expression.ToString() == "button")
                    .Where(x => x.Row.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Save");
                node = node.ReplaceNodes(cancelMethods.Union(saveMethods).Union(deleteMethods)
                    .Select(x => x.Row),
                        (arg1, arg2) =>
                        {
                            if (arg1.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Delete")
                            {
                                return SyntaxFactory.ExpressionStatement(
                                            SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParseExpression("button"),
                                                    SyntaxFactory.IdentifierName("DeleteCancelSave")),
                                                SyntaxFactory.ArgumentList()));
                            }
                            if (arg1.DescendantNodes().OfType<MemberAccessExpressionSyntax>().FirstOrDefault()
                        .Name.ToString() == "Cancel")
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

            //senditemid, column.search.field.
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
                    if (node.ArgumentList.Arguments.Count == 1 && node.ArgumentList.Arguments[0].Expression is SimpleLambdaExpressionSyntax)
                    {
                        var arg = ((MemberAccessExpressionSyntax)((SimpleLambdaExpressionSyntax)node.ArgumentList.Arguments[0].Expression).Body).Name;
                        return SyntaxFactory.ParseExpression($"{methodName.ToLower()}.{arg}()")
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

                if (methodName == "Send")
                {
                    if (node.ArgumentList.Arguments.Count == 2 &&
                        node.ArgumentList.Arguments.FirstOrDefault().ToString() == "\"item\"" &&
                        node.ArgumentList.Arguments.LastOrDefault().ToString() == "\"item.ID\"")
                    {
                        var member = node.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
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

            //customColumn and cancel/save/delete
            public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax node)
            {
                var s = node.DescendantNodes().OfType<InvocationExpressionSyntax>()
                        .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "OnClick").FirstOrDefault();
                var customColumnNode = node.DescendantNodes().OfType<InvocationExpressionSyntax>()
                    .Where(x => (semanticModel.GetSymbolInfo(x).Symbol as IMethodSymbol)?.Name == "CustomColumn").FirstOrDefault();

                if (customColumnNode != null)
                {
                    var newNode = node.ReplaceNodes(node.DescendantNodes().OfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (nde1.ToString() == "CustomColumn()")
                            {
                                SeparatedSyntaxList<ArgumentSyntax> args = new SeparatedSyntaxList<ArgumentSyntax>();
                                var argsHeaderText = node.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                    .Where(x => ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() ==
                                    "HeaderText").FirstOrDefault().ArgumentList;
                                var argsDisplayExpr = node.DescendantNodes().OfType<InvocationExpressionSyntax>()
                                    .Where(x => ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "DisplayExpression").FirstOrDefault().ArgumentList;

                                if (argsHeaderText.Arguments.Count == 1)
                                    args = args.Add(argsHeaderText.Arguments.FirstOrDefault());

                                if (argsDisplayExpr.Arguments.Count == 1)
                                    args = args.Add(argsDisplayExpr.Arguments.FirstOrDefault());

                                return SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                         SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ParseExpression("column"),
                                    SyntaxFactory.IdentifierName("Custom")),
                                    SyntaxFactory.ArgumentList(args))
                                .WithLeadingTrivia(nde1.GetLeadingTrivia());
                            }
                            else if (((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() ==
                               "HeaderText" &&
                               nde1.ArgumentList.Arguments.Count == 1)
                            {
                                return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                            }
                            else if (((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() ==
                              "DisplayExpression" &&
                              nde1.ArgumentList.Arguments.Count == 1)
                            {
                                return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                            }

                            return nde2;
                        });
                    return newNode;
                }

                if (s == null)
                    return base.VisitExpressionStatement(node);
                var methodSymbol = (semanticModel.GetSymbolInfo(s).Symbol as IMethodSymbol);
                if (s.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().Count() == 1 &&
                    methodSymbol?.ReturnType.OriginalDefinition?.ToString() == "MSharp.ModuleButton")
                {
                    var saveMethods = new string[] { "ReturnToPreviousPage", "SaveInDatabase" };
                    var saveModalMethods = new string[] { "CloseModal(Refresh.Full)", "SaveInDatabase" };
                    var cancelMethods = new string[] { "ReturnToPreviousPage" };
                    var cancelModalMethods = new string[] { "CloseModal" };
                    var deleteMethods = new string[] { "ReturnToPreviousPage", "DeleteItem" };
                    var deleteModalMethods = new string[] { "CloseModal(Refresh.Full)", "DeleteItem" };
                    var optionalMethods = new string[] { "GentleMessage(\"Deleted successfully.\")" };

                    var lambdaExpressionArgument = s.DescendantNodes().OfType<SimpleLambdaExpressionSyntax>().FirstOrDefault();
                    var invocations = lambdaExpressionArgument.DescendantNodes().OfType<InvocationExpressionSyntax>();
                    if (invocations.Count() == 1 &&
                        (invocations.All(x => cancelMethods.Any(y => x.ToString().Contains(y))) ||
                     (invocations.All(x => cancelModalMethods.Any(y => x.ToString().Contains(y))))))
                    {
                        var m = node.ChildNodes().FirstOrDefault();
                        var methodSelector = invocations.Any(x => x.ToString().Contains("ReturnToPreviousPage"));
                        var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("button"),
                                SyntaxFactory.IdentifierName(methodSelector ? "Cancel" : "ModalCancel")),
                            SyntaxFactory.ArgumentList());

                        while (m.ChildNodes().Any())
                        {
                            var m2 = m.ChildNodes();
                            if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                                m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as MemberAccessExpressionSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.Name.ToString() == "Icon" && (arguments.Arguments.Count != 1
                                    || arguments.Arguments.FirstOrDefault().Expression.ToString() != "FA.Backward"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "CausesValidation" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "false"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "OnClick" && (arguments.Arguments.Count != 1 ||
                                    ((!arguments.Arguments.FirstOrDefault().Expression.ToString().Contains("ReturnToPreviousPage")) &&
                                    (!arguments.Arguments.FirstOrDefault().Expression.ToString().Contains("CloseModal")))))
                                {
                                    return base.VisitExpressionStatement(node);
                                }
                                else if (methodName.Name.ToString() != "OnClick" &&
                                   methodName.Name.ToString() != "CausesValidation" &&
                                   methodName.Name.ToString() != "Icon")
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                m = (m2.FirstOrDefault() as MemberAccessExpressionSyntax).Expression;
                            }
                            else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                               m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as IdentifierNameSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.ToString() == "Button" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"Cancel\""))
                                {
                                    return base.VisitExpressionStatement(node);
                                }
                                m = m2.FirstOrDefault();
                            }
                        }
                        return SyntaxFactory.ExpressionStatement(newExpression)
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithTrailingTrivia(node.GetTrailingTrivia());
                    }
                    else if (((invocations.Count() == 2 || invocations.Count() == 3) &&
                        invocations.All(x => x.Expression is MemberAccessExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)(x.Expression)).Expression is IdentifierNameSyntax)) &&
                        (invocations.All(x => saveMethods.Any(y => x.ToString().Contains(y))) ||
                        invocations.All(x => saveModalMethods.Any(y => x.ToString().Contains(y)))))
                    {
                        var m = node.ChildNodes().FirstOrDefault();
                        var methodSelector = invocations.Any(x => x.ToString().Contains("ReturnToPreviousPage"));
                        var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("button"),
                                SyntaxFactory.IdentifierName(methodSelector ? "Save" : "ModalSave")),
                            SyntaxFactory.ArgumentList());

                        while (m.ChildNodes().Any())
                        {
                            var m2 = m.ChildNodes();
                            if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                                m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as MemberAccessExpressionSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.Name.ToString() == "Icon" && (arguments.Arguments.Count != 1
                                    || arguments.Arguments.FirstOrDefault().Expression.ToString() != "FA.Check"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "CausesValidation" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "true"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "OnClick" && ((arguments.Arguments.Count != 1) ||
                                    !((arguments.Arguments.FirstOrDefault().Expression as SimpleLambdaExpressionSyntax).Body as BlockSyntax)
                                        .Statements.Any(x => saveMethods.Any(y => x.ToString().Contains(y))) ||
                                    !((arguments.Arguments.FirstOrDefault().Expression as SimpleLambdaExpressionSyntax).Body as BlockSyntax)
                                        .Statements.Any(x => saveModalMethods.Any(y => x.ToString().Contains(y)))))
                                {
                                    return base.VisitExpressionStatement(node);
                                }
                                else if (methodName.Name.ToString() != "OnClick" &&
                                   methodName.Name.ToString() != "CausesValidation" &&
                                   methodName.Name.ToString() != "Icon")
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                m = (m2.FirstOrDefault() as MemberAccessExpressionSyntax).Expression;
                            }
                            else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                               m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as IdentifierNameSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.ToString() == "Button" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"Save\""))
                                {
                                    return base.VisitExpressionStatement(node);
                                }
                                m = m2.FirstOrDefault();
                            }
                        }
                        return SyntaxFactory.ExpressionStatement(newExpression)
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithTrailingTrivia(node.GetTrailingTrivia());
                    }
                    else if (((invocations.Count() == 2 || invocations.Count() == 3) &&
                        invocations.All(x => x.Expression is MemberAccessExpressionSyntax &&
                        ((MemberAccessExpressionSyntax)(x.Expression)).Expression is IdentifierNameSyntax)) &&
                        (invocations.All(x => deleteMethods.Union(optionalMethods).Any(y => x.ToString().Contains(y))) ||
                        invocations.All(x => deleteModalMethods.Union(optionalMethods).Any(y => x.ToString().Contains(y)))))
                    {
                        var m = node.ChildNodes().FirstOrDefault();
                        var methodSelector = invocations.Any(x => x.ToString().Contains("ReturnToPreviousPage"));
                        var newExpression = SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.ParseExpression("button"),
                                SyntaxFactory.IdentifierName(methodSelector ? "Delete" : "ModalDelete")),
                            SyntaxFactory.ArgumentList());

                        while (m.ChildNodes().Any())
                        {
                            var m2 = m.ChildNodes();
                            if (m2.FirstOrDefault() is MemberAccessExpressionSyntax &&
                                m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as MemberAccessExpressionSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.Name.ToString() == "VisibleIf" && (arguments.Arguments.Count != 1
                                    || arguments.Arguments.FirstOrDefault().Expression.ToString() != "CommonCriterion.IsEditMode_Item_IsNew"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "Icon" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "FA.Close"))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "ConfirmQuestion" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"Are you sure you want to delete it?\""))
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                        SyntaxFactory.MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                else if (methodName.Name.ToString() == "OnClick" && ((arguments.Arguments.Count != 1) ||
                                    !((arguments.Arguments.FirstOrDefault().Expression as SimpleLambdaExpressionSyntax).Body as BlockSyntax)
                                        .Statements.Any(x => deleteMethods.Any(y => x.ToString().Contains(y))) ||
                                    !((arguments.Arguments.FirstOrDefault().Expression as SimpleLambdaExpressionSyntax).Body as BlockSyntax)
                                        .Statements.Any(x => deleteModalMethods.Any(y => x.ToString().Contains(y)))))
                                {
                                    return base.VisitExpressionStatement(node);
                                }
                                else if (methodName.Name.ToString() != "OnClick" &&
                                   methodName.Name.ToString() != "VisibleIf" &&
                                   methodName.Name.ToString() != "Icon" &&
                                   methodName.Name.ToString() != "ConfirmQuestion")
                                {
                                    newExpression = SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression, newExpression, methodName.Name), arguments);
                                }
                                m = (m2.FirstOrDefault() as MemberAccessExpressionSyntax).Expression;
                            }
                            else if (m2.FirstOrDefault() is IdentifierNameSyntax &&
                               m2.LastOrDefault() is ArgumentListSyntax)
                            {
                                var methodName = m2.FirstOrDefault() as IdentifierNameSyntax;
                                var arguments = m2.LastOrDefault() as ArgumentListSyntax;
                                if (methodName.ToString() == "Button" && (arguments.Arguments.Count != 1
                                        || arguments.Arguments.FirstOrDefault().Expression.ToString() != "\"Delete\""))
                                {
                                    return base.VisitExpressionStatement(node);
                                }
                                m = m2.FirstOrDefault();
                            }
                        }
                        return SyntaxFactory.ExpressionStatement(newExpression)
                            .WithLeadingTrivia(node.GetLeadingTrivia())
                            .WithTrailingTrivia(node.GetTrailingTrivia());
                    }


                }
                return base.VisitExpressionStatement(node);
            }

            //if problem
            public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
            {
                var ifNode = node.DescendantNodes().OfType<InvocationExpressionSyntax>()
                    .Where(x => ((MemberAccessExpressionSyntax)x.Expression).Name.ToString() == "If" &&
                    !(((MemberAccessExpressionSyntax)x.Expression).Expression is IdentifierNameSyntax))
                    .FirstOrDefault();
                if (ifNode != null)
                {
                    node = node.ReplaceNodes(ifNode.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>(),
                        (nde1, nde2) =>
                        {
                            if (((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() == "If" &&
                                !(((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax))
                            {
                                return nde2.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                            }
                            else if (((MemberAccessExpressionSyntax)nde1.Expression).Name.ToString() == "If" &&
                               (((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax))
                            {
                                return nde2;
                            }
                            if (((MemberAccessExpressionSyntax)nde1.Expression).Expression is IdentifierNameSyntax)
                            {
                                return SyntaxFactory.InvocationExpression(
                                       SyntaxFactory.MemberAccessExpression(
                                           SyntaxKind.SimpleMemberAccessExpression,
                                           SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.ParseExpression(((MemberAccessExpressionSyntax)nde1.Expression).Expression.ToString()),
                                                    SyntaxFactory.IdentifierName("If"))
                                                , SyntaxFactory.ArgumentList(ifNode.ArgumentList.Arguments)), ((MemberAccessExpressionSyntax)nde1.Expression).Name),
                                    SyntaxFactory.ArgumentList(nde1.ArgumentList.Arguments)).WithLeadingTrivia(nde1.GetLeadingTrivia());
                            }
                            return nde2;
                        });
                    return VisitSimpleLambdaExpression(node);
                }
                return node;
            }
        }
    }
}