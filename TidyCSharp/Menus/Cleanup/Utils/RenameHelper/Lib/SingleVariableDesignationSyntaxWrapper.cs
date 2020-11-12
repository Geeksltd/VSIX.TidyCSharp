using System;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup.Renaming
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using System.Reflection;
    using static Geeks.GeeksProductivityTools.Menus.Cleanup.Renaming.RenameHelper;

    internal struct SingleVariableDesignationSyntaxWrapper : ISyntaxWrapper<CSharpSyntaxNode>
    {
        const string SingleVariableDesignationSyntaxTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.SingleVariableDesignationSyntax";
        static readonly Type SingleVariableDesignationSyntaxType;

        static readonly Func<CSharpSyntaxNode, SyntaxToken> IdentifierAccessor;
        static readonly Func<CSharpSyntaxNode, SyntaxToken, CSharpSyntaxNode> WithIdentifierAccessor;

        readonly CSharpSyntaxNode node;

        static SingleVariableDesignationSyntaxWrapper()
        {
            SingleVariableDesignationSyntaxType = typeof(CSharpSyntaxNode).GetTypeInfo().Assembly.GetType(SingleVariableDesignationSyntaxTypeName);
            IdentifierAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<CSharpSyntaxNode, SyntaxToken>(SingleVariableDesignationSyntaxType, nameof(Identifier));
            WithIdentifierAccessor = LightupHelpers.CreateSyntaxWithPropertyAccessor<CSharpSyntaxNode, SyntaxToken>(SingleVariableDesignationSyntaxType, nameof(Identifier));
        }
        private SingleVariableDesignationSyntaxWrapper(CSharpSyntaxNode node) => this.node = node;

        public CSharpSyntaxNode SyntaxNode => this.node;

        public SyntaxToken Identifier => IdentifierAccessor(this.SyntaxNode);

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
            return VariableDesignationSyntaxWrapper.FromUpcast(wrapper.node);
        }

        public static implicit operator CSharpSyntaxNode(SingleVariableDesignationSyntaxWrapper wrapper)
        {
            return wrapper.node;
        }

        public static bool IsInstance(SyntaxNode node)
        {
            return node != null && LightupHelpers.CanWrapNode(node, SingleVariableDesignationSyntaxType);
        }

        public SingleVariableDesignationSyntaxWrapper WithIdentifier(SyntaxToken identifier)
        {
            return new SingleVariableDesignationSyntaxWrapper(WithIdentifierAccessor(this.SyntaxNode, identifier));
        }
    }
}