using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper.Lib;

internal abstract class SyntaxWrapper<TNode>
{
    public static SyntaxWrapper<TNode> Default { get; } = FindDefaultSyntaxWrapper();

    public abstract TNode Wrap(SyntaxNode node);

    public abstract SyntaxNode Unwrap(TNode node);

    private static SyntaxWrapper<TNode> FindDefaultSyntaxWrapper()
    {
        if (typeof(SyntaxNode).GetTypeInfo().IsAssignableFrom(typeof(TNode).GetTypeInfo()))
        {
            return new DirectCastSyntaxWrapper();
        }

        return new ConversionSyntaxWrapper();
    }

    private sealed class DirectCastSyntaxWrapper : SyntaxWrapper<TNode>
    {
        public override SyntaxNode Unwrap(TNode node) => (SyntaxNode)(object)node;

        public override TNode Wrap(SyntaxNode node) => (TNode)(object)node;
    }

    private sealed class ConversionSyntaxWrapper : SyntaxWrapper<TNode>
    {
        private readonly Func<TNode, SyntaxNode> _unwrapAccessor;
        private readonly Func<SyntaxNode, TNode> _wrapAccessor;

        public ConversionSyntaxWrapper()
        {
            _unwrapAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<TNode, SyntaxNode>(typeof(TNode), nameof(ISyntaxWrapper<SyntaxNode>.SyntaxNode));

            var explicitOperator = typeof(TNode).GetTypeInfo().GetDeclaredMethods("op_Explicit")
                .Single(m => m.ReturnType == typeof(TNode) && m.GetParameters()[0].ParameterType == typeof(SyntaxNode));

            var syntaxParameter = Expression.Parameter(typeof(SyntaxNode), "syntax");

            var wrapAccessorExpression =
                Expression.Lambda<Func<SyntaxNode, TNode>>(
                    Expression.Call(explicitOperator, syntaxParameter),
                    syntaxParameter);

            _wrapAccessor = wrapAccessorExpression.Compile();
        }

        public override SyntaxNode Unwrap(TNode node) => _unwrapAccessor(node);

        public override TNode Wrap(SyntaxNode node) => _wrapAccessor(node);
    }
}