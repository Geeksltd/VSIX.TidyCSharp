using System;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup.Renaming
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using System.Reflection;

    internal static partial class RenameHelper
    {
        internal struct VariableDesignationSyntaxWrapper : ISyntaxWrapper<CSharpSyntaxNode>
        {
            private const string VariableDesignationSyntaxTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.VariableDesignationSyntax";
            private static readonly Type VariableDesignationSyntaxType;

            private readonly CSharpSyntaxNode node;

            static VariableDesignationSyntaxWrapper()
            {
                VariableDesignationSyntaxType = typeof(CSharpSyntaxNode).GetTypeInfo().Assembly.GetType(VariableDesignationSyntaxTypeName);
            }

            private VariableDesignationSyntaxWrapper(CSharpSyntaxNode node)
            {
                this.node = node;
            }

            public CSharpSyntaxNode SyntaxNode => this.node;

            public static explicit operator VariableDesignationSyntaxWrapper(SyntaxNode node)
            {
                if (node == null)
                {
                    return default(VariableDesignationSyntaxWrapper);
                }

                if (!IsInstance(node))
                {
                    throw new InvalidCastException($"Cannot cast '{node.GetType().FullName}' to '{VariableDesignationSyntaxTypeName}'");
                }

                return new VariableDesignationSyntaxWrapper((CSharpSyntaxNode)node);
            }

            public static implicit operator CSharpSyntaxNode(VariableDesignationSyntaxWrapper wrapper)
            {
                return wrapper.node;
            }

            public static bool IsInstance(SyntaxNode node)
            {
                return node != null && LightupHelpers.CanWrapNode(node, VariableDesignationSyntaxType);
            }

            internal static VariableDesignationSyntaxWrapper FromUpcast(CSharpSyntaxNode node)
            {
                return new VariableDesignationSyntaxWrapper(node);
            }
        }
    }
}
