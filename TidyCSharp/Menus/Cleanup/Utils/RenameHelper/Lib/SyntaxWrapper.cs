using System;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup.Renaming
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    internal abstract class SyntaxWrapper<TNode>
    {
        public static SyntaxWrapper<TNode> Default { get; } = FindDefaultSyntaxWrapper();

        public abstract TNode Wrap(SyntaxNode node);

        public abstract SyntaxNode Unwrap(TNode node);

        static SyntaxWrapper<TNode> FindDefaultSyntaxWrapper()
        {
            if (typeof(SyntaxNode).GetTypeInfo().IsAssignableFrom(typeof(TNode).GetTypeInfo()))
            {
                return new DirectCastSyntaxWrapper();
            }

            return new ConversionSyntaxWrapper();
        }

        sealed class DirectCastSyntaxWrapper : SyntaxWrapper<TNode>
        {
            public override SyntaxNode Unwrap(TNode node) => (SyntaxNode)(object)node;

            public override TNode Wrap(SyntaxNode node) => (TNode)(object)node;
        }

        sealed class ConversionSyntaxWrapper : SyntaxWrapper<TNode>
        {
            readonly Func<TNode, SyntaxNode> unwrapAccessor;
            readonly Func<SyntaxNode, TNode> wrapAccessor;

            public ConversionSyntaxWrapper()
            {
                unwrapAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TNode, SyntaxNode>(typeof(TNode), nameof(ISyntaxWrapper<SyntaxNode>.SyntaxNode));

                var explicitOperator = typeof(TNode).GetTypeInfo().GetDeclaredMethods("op_Explicit")
                    .Single(m => m.ReturnType == typeof(TNode) && m.GetParameters()[0].ParameterType == typeof(SyntaxNode));

                var syntaxParameter = Expression.Parameter(typeof(SyntaxNode), "syntax");

                var wrapAccessorExpression =
                    Expression.Lambda<Func<SyntaxNode, TNode>>(
                        Expression.Call(explicitOperator, syntaxParameter),
                        syntaxParameter);

                wrapAccessor = wrapAccessorExpression.Compile();
            }

            public override SyntaxNode Unwrap(TNode node) => unwrapAccessor(node);

            public override TNode Wrap(SyntaxNode node) => wrapAccessor(node);
        }
    }
}