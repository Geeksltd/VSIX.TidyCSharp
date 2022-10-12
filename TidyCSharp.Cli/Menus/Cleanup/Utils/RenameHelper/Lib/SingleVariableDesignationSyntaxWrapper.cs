using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper.Lib;

using static RenameHelper;

internal struct SingleVariableDesignationSyntaxWrapper : ISyntaxWrapper<CSharpSyntaxNode>
{
    private const string SingleVariableDesignationSyntaxTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.SingleVariableDesignationSyntax";
    private static readonly Type SingleVariableDesignationSyntaxType;

    private static readonly Func<CSharpSyntaxNode, SyntaxToken> IdentifierAccessor;
    private static readonly Func<CSharpSyntaxNode, SyntaxToken, CSharpSyntaxNode> WithIdentifierAccessor;

    private readonly CSharpSyntaxNode _node;

    static SingleVariableDesignationSyntaxWrapper()
    {
        SingleVariableDesignationSyntaxType = typeof(CSharpSyntaxNode).GetTypeInfo().Assembly.GetType(SingleVariableDesignationSyntaxTypeName);
        IdentifierAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<CSharpSyntaxNode, SyntaxToken>(SingleVariableDesignationSyntaxType, nameof(Identifier));
        WithIdentifierAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<CSharpSyntaxNode, SyntaxToken>(SingleVariableDesignationSyntaxType, nameof(Identifier));
    }
    private SingleVariableDesignationSyntaxWrapper(CSharpSyntaxNode node) => _node = node;

    public CSharpSyntaxNode SyntaxNode => _node;

    public SyntaxToken Identifier => IdentifierAccessor(SyntaxNode);

    public static explicit operator SingleVariableDesignationSyntaxWrapper(VariableDesignationSyntaxWrapper node)
    {
        return (SingleVariableDesignationSyntaxWrapper)node.SyntaxNode;
    }

    public static explicit operator SingleVariableDesignationSyntaxWrapper(SyntaxNode node)
    {
        if (node == null)
        {
            return default(SingleVariableDesignationSyntaxWrapper);
        }

        if (!IsInstance(node))
        {
            throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{SingleVariableDesignationSyntaxTypeName}'");
        }

        return new SingleVariableDesignationSyntaxWrapper((CSharpSyntaxNode)node);
    }

    public static implicit operator VariableDesignationSyntaxWrapper(SingleVariableDesignationSyntaxWrapper wrapper)
    {
        return VariableDesignationSyntaxWrapper.FromUpcast(wrapper._node);
    }

    public static implicit operator CSharpSyntaxNode(SingleVariableDesignationSyntaxWrapper wrapper)
    {
        return wrapper._node;
    }

    public static bool IsInstance(SyntaxNode node)
    {
        return node != null && LightupHelpers.CanWrapNode(node, SingleVariableDesignationSyntaxType);
    }

    public SingleVariableDesignationSyntaxWrapper WithIdentifier(SyntaxToken identifier)
    {
        return new SingleVariableDesignationSyntaxWrapper(WithIdentifierAccessor(SyntaxNode, identifier));
    }
}