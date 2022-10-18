using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper.Lib;

internal struct LocalFunctionStatementSyntaxWrapper : ISyntaxWrapper<StatementSyntax>
{
    private const string LocalFunctionStatementSyntaxTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax";
    private static readonly Type LocalFunctionStatementSyntaxType;

    private static readonly Func<StatementSyntax, SyntaxTokenList> ModifiersAccessor;
    private static readonly Func<StatementSyntax, TypeSyntax> ReturnTypeAccessor;
    private static readonly Func<StatementSyntax, SyntaxToken> IdentifierAccessor;
    private static readonly Func<StatementSyntax, TypeParameterListSyntax> TypeParameterListAccessor;
    private static readonly Func<StatementSyntax, ParameterListSyntax> ParameterListAccessor;
    private static readonly Func<StatementSyntax, SyntaxList<TypeParameterConstraintClauseSyntax>> ConstraintClausesAccessor;
    private static readonly Func<StatementSyntax, BlockSyntax> BodyAccessor;
    private static readonly Func<StatementSyntax, ArrowExpressionClauseSyntax> ExpressionBodyAccessor;
    private static readonly Func<StatementSyntax, SyntaxToken> SemicolonTokenAccessor;
    private static readonly Func<StatementSyntax, SyntaxTokenList, StatementSyntax> WithModifiersAccessor;
    private static readonly Func<StatementSyntax, TypeSyntax, StatementSyntax> WithReturnTypeAccessor;
    private static readonly Func<StatementSyntax, SyntaxToken, StatementSyntax> WithIdentifierAccessor;
    private static readonly Func<StatementSyntax, TypeParameterListSyntax, StatementSyntax> WithTypeParameterListAccessor;
    private static readonly Func<StatementSyntax, ParameterListSyntax, StatementSyntax> WithParameterListAccessor;
    private static readonly Func<StatementSyntax, SyntaxList<TypeParameterConstraintClauseSyntax>, StatementSyntax> WithConstraintClausesAccessor;
    private static readonly Func<StatementSyntax, BlockSyntax, StatementSyntax> WithBodyAccessor;
    private static readonly Func<StatementSyntax, ArrowExpressionClauseSyntax, StatementSyntax> WithExpressionBodyAccessor;
    private static readonly Func<StatementSyntax, SyntaxToken, StatementSyntax> WithSemicolonTokenAccessor;

    private readonly StatementSyntax _node;

    static LocalFunctionStatementSyntaxWrapper()
    {
        LocalFunctionStatementSyntaxType = typeof(CSharpSyntaxNode).GetTypeInfo().Assembly.GetType(LocalFunctionStatementSyntaxTypeName);
        ModifiersAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxTokenList>(LocalFunctionStatementSyntaxType, nameof(Modifiers));
        ReturnTypeAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, TypeSyntax>(LocalFunctionStatementSyntaxType, nameof(ReturnType));
        IdentifierAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxToken>(LocalFunctionStatementSyntaxType, nameof(Identifier));
        TypeParameterListAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, TypeParameterListSyntax>(LocalFunctionStatementSyntaxType, nameof(TypeParameterList));
        ParameterListAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, ParameterListSyntax>(LocalFunctionStatementSyntaxType, nameof(ParameterList));
        ConstraintClausesAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxList<TypeParameterConstraintClauseSyntax>>(LocalFunctionStatementSyntaxType, nameof(ConstraintClauses));
        BodyAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, BlockSyntax>(LocalFunctionStatementSyntaxType, nameof(Body));
        ExpressionBodyAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, ArrowExpressionClauseSyntax>(LocalFunctionStatementSyntaxType, nameof(ExpressionBody));
        SemicolonTokenAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<StatementSyntax, SyntaxToken>(LocalFunctionStatementSyntaxType, nameof(SemicolonToken));
        WithModifiersAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, SyntaxTokenList>(LocalFunctionStatementSyntaxType, nameof(Modifiers));
        WithReturnTypeAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, TypeSyntax>(LocalFunctionStatementSyntaxType, nameof(ReturnType));
        WithIdentifierAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, SyntaxToken>(LocalFunctionStatementSyntaxType, nameof(Identifier));
        WithTypeParameterListAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, TypeParameterListSyntax>(LocalFunctionStatementSyntaxType, nameof(TypeParameterList));
        WithParameterListAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, ParameterListSyntax>(LocalFunctionStatementSyntaxType, nameof(ParameterList));
        WithConstraintClausesAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, SyntaxList<TypeParameterConstraintClauseSyntax>>(LocalFunctionStatementSyntaxType, nameof(ConstraintClauses));
        WithBodyAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, BlockSyntax>(LocalFunctionStatementSyntaxType, nameof(Body));
        WithExpressionBodyAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, ArrowExpressionClauseSyntax>(LocalFunctionStatementSyntaxType, nameof(ExpressionBody));
        WithSemicolonTokenAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<StatementSyntax, SyntaxToken>(LocalFunctionStatementSyntaxType, nameof(SemicolonToken));
    }
    private LocalFunctionStatementSyntaxWrapper(StatementSyntax node) => _node = node;

    public StatementSyntax SyntaxNode => _node;

    public SyntaxTokenList Modifiers => ModifiersAccessor(SyntaxNode);

    public TypeSyntax ReturnType => ReturnTypeAccessor(SyntaxNode);

    public SyntaxToken Identifier => IdentifierAccessor(SyntaxNode);

    public TypeParameterListSyntax TypeParameterList => TypeParameterListAccessor(SyntaxNode);

    public ParameterListSyntax ParameterList => ParameterListAccessor(SyntaxNode);

    public SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses
    {
        get
        {
            return ConstraintClausesAccessor(SyntaxNode);
        }
    }

    public BlockSyntax Body => BodyAccessor(SyntaxNode);

    public ArrowExpressionClauseSyntax ExpressionBody => ExpressionBodyAccessor(SyntaxNode);

    public SyntaxToken SemicolonToken => SemicolonTokenAccessor(SyntaxNode);

    public static explicit operator LocalFunctionStatementSyntaxWrapper(SyntaxNode node)
    {
        if (node == null)
        {
            return default(LocalFunctionStatementSyntaxWrapper);
        }

        if (!IsInstance(node))
        {
            throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{LocalFunctionStatementSyntaxTypeName}'");
        }

        return new LocalFunctionStatementSyntaxWrapper((StatementSyntax)node);
    }

    public static implicit operator StatementSyntax(LocalFunctionStatementSyntaxWrapper wrapper)
    {
        return wrapper._node;
    }

    public static bool IsInstance(SyntaxNode node)
    {
        return node != null && LightupHelpers.CanWrapNode(node, LocalFunctionStatementSyntaxType);
    }

    public LocalFunctionStatementSyntaxWrapper WithModifiers(SyntaxTokenList modifiers)
    {
        return new LocalFunctionStatementSyntaxWrapper(WithModifiersAccessor(SyntaxNode, modifiers));
    }

    public LocalFunctionStatementSyntaxWrapper WithReturnType(TypeSyntax returnType)
    {
        return new LocalFunctionStatementSyntaxWrapper(WithReturnTypeAccessor(SyntaxNode, returnType));
    }

    public LocalFunctionStatementSyntaxWrapper WithIdentifier(SyntaxToken identifier)
    {
        return new LocalFunctionStatementSyntaxWrapper(WithIdentifierAccessor(SyntaxNode, identifier));
    }

    public LocalFunctionStatementSyntaxWrapper WithTypeParameterList(TypeParameterListSyntax typeParameterList)
    {
        return new LocalFunctionStatementSyntaxWrapper(WithTypeParameterListAccessor(SyntaxNode, typeParameterList));
    }

    public LocalFunctionStatementSyntaxWrapper WithParameterList(ParameterListSyntax parameterList)
    {
        return new LocalFunctionStatementSyntaxWrapper(WithParameterListAccessor(SyntaxNode, parameterList));
    }

    public LocalFunctionStatementSyntaxWrapper WithConstraintClauses(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
    {
        return new LocalFunctionStatementSyntaxWrapper(WithConstraintClausesAccessor(SyntaxNode, constraintClauses));
    }

    public LocalFunctionStatementSyntaxWrapper WithBody(BlockSyntax body)
    {
        return new LocalFunctionStatementSyntaxWrapper(WithBodyAccessor(SyntaxNode, body));
    }

    public LocalFunctionStatementSyntaxWrapper WithExpressionBody(ArrowExpressionClauseSyntax expressionBody)
    {
        return new LocalFunctionStatementSyntaxWrapper(WithExpressionBodyAccessor(SyntaxNode, expressionBody));
    }

    public LocalFunctionStatementSyntaxWrapper WithSemicolonToken(SyntaxToken semicolonToken)
    {
        return new LocalFunctionStatementSyntaxWrapper(WithSemicolonTokenAccessor(SyntaxNode, semicolonToken));
    }

    public LocalFunctionStatementSyntaxWrapper AddModifiers(params SyntaxToken[] items)
    {
        return WithModifiers(Modifiers.AddRange(items));
    }

    public LocalFunctionStatementSyntaxWrapper AddTypeParameterListParameters(params TypeParameterSyntax[] items)
    {
        var typeParameterList = TypeParameterList ?? SyntaxFactory.TypeParameterList();
        return WithTypeParameterList(typeParameterList.WithParameters(typeParameterList.Parameters.AddRange(items)));
    }

    public LocalFunctionStatementSyntaxWrapper AddParameterListParameters(params ParameterSyntax[] items)
    {
        return WithParameterList(ParameterList.WithParameters(ParameterList.Parameters.AddRange(items)));
    }

    public LocalFunctionStatementSyntaxWrapper AddConstraintClauses(params TypeParameterConstraintClauseSyntax[] items)
    {
        return WithConstraintClauses(ConstraintClauses.AddRange(items));
    }

    public LocalFunctionStatementSyntaxWrapper AddBodyStatements(params StatementSyntax[] items)
    {
        var body = Body ?? SyntaxFactory.Block();
        return WithBody(body.WithStatements(body.Statements.AddRange(items)));
    }
}