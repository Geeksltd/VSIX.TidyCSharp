using System;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup.Renaming
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Reflection;
    internal struct LocalFunctionStatementSyntaxWrapper : ISyntaxWrapper<StatementSyntax>
    {
        const string LocalFunctionStatementSyntaxTypeName = "Microsoft.CodeAnalysis.CSharp.Syntax.LocalFunctionStatementSyntax";
        static readonly Type LocalFunctionStatementSyntaxType;

        static readonly Func<StatementSyntax, SyntaxTokenList> ModifiersAccessor;
        static readonly Func<StatementSyntax, TypeSyntax> ReturnTypeAccessor;
        static readonly Func<StatementSyntax, SyntaxToken> IdentifierAccessor;
        static readonly Func<StatementSyntax, TypeParameterListSyntax> TypeParameterListAccessor;
        static readonly Func<StatementSyntax, ParameterListSyntax> ParameterListAccessor;
        static readonly Func<StatementSyntax, SyntaxList<TypeParameterConstraintClauseSyntax>> ConstraintClausesAccessor;
        static readonly Func<StatementSyntax, BlockSyntax> BodyAccessor;
        static readonly Func<StatementSyntax, ArrowExpressionClauseSyntax> ExpressionBodyAccessor;
        static readonly Func<StatementSyntax, SyntaxToken> SemicolonTokenAccessor;
        static readonly Func<StatementSyntax, SyntaxTokenList, StatementSyntax> WithModifiersAccessor;
        static readonly Func<StatementSyntax, TypeSyntax, StatementSyntax> WithReturnTypeAccessor;
        static readonly Func<StatementSyntax, SyntaxToken, StatementSyntax> WithIdentifierAccessor;
        static readonly Func<StatementSyntax, TypeParameterListSyntax, StatementSyntax> WithTypeParameterListAccessor;
        static readonly Func<StatementSyntax, ParameterListSyntax, StatementSyntax> WithParameterListAccessor;
        static readonly Func<StatementSyntax, SyntaxList<TypeParameterConstraintClauseSyntax>, StatementSyntax> WithConstraintClausesAccessor;
        static readonly Func<StatementSyntax, BlockSyntax, StatementSyntax> WithBodyAccessor;
        static readonly Func<StatementSyntax, ArrowExpressionClauseSyntax, StatementSyntax> WithExpressionBodyAccessor;
        static readonly Func<StatementSyntax, SyntaxToken, StatementSyntax> WithSemicolonTokenAccessor;

        readonly StatementSyntax node;

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
        private LocalFunctionStatementSyntaxWrapper(StatementSyntax node) => this.node = node;

        public StatementSyntax SyntaxNode => this.node;

        public SyntaxTokenList Modifiers => ModifiersAccessor(this.SyntaxNode);

        public TypeSyntax ReturnType => ReturnTypeAccessor(this.SyntaxNode);

        public SyntaxToken Identifier => IdentifierAccessor(this.SyntaxNode);

        public TypeParameterListSyntax TypeParameterList => TypeParameterListAccessor(this.SyntaxNode);

        public ParameterListSyntax ParameterList => ParameterListAccessor(this.SyntaxNode);

        public SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses
        {
            get
            {
                return ConstraintClausesAccessor(this.SyntaxNode);
            }
        }

        public BlockSyntax Body => BodyAccessor(this.SyntaxNode);

        public ArrowExpressionClauseSyntax ExpressionBody => ExpressionBodyAccessor(this.SyntaxNode);

        public SyntaxToken SemicolonToken => SemicolonTokenAccessor(this.SyntaxNode);

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
            return wrapper.node;
        }

        public static bool IsInstance(SyntaxNode node)
        {
            return node != null && LightupHelpers.CanWrapNode(node, LocalFunctionStatementSyntaxType);
        }

        public LocalFunctionStatementSyntaxWrapper WithModifiers(SyntaxTokenList modifiers)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithModifiersAccessor(this.SyntaxNode, modifiers));
        }

        public LocalFunctionStatementSyntaxWrapper WithReturnType(TypeSyntax returnType)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithReturnTypeAccessor(this.SyntaxNode, returnType));
        }

        public LocalFunctionStatementSyntaxWrapper WithIdentifier(SyntaxToken identifier)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithIdentifierAccessor(this.SyntaxNode, identifier));
        }

        public LocalFunctionStatementSyntaxWrapper WithTypeParameterList(TypeParameterListSyntax typeParameterList)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithTypeParameterListAccessor(this.SyntaxNode, typeParameterList));
        }

        public LocalFunctionStatementSyntaxWrapper WithParameterList(ParameterListSyntax parameterList)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithParameterListAccessor(this.SyntaxNode, parameterList));
        }

        public LocalFunctionStatementSyntaxWrapper WithConstraintClauses(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithConstraintClausesAccessor(this.SyntaxNode, constraintClauses));
        }

        public LocalFunctionStatementSyntaxWrapper WithBody(BlockSyntax body)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithBodyAccessor(this.SyntaxNode, body));
        }

        public LocalFunctionStatementSyntaxWrapper WithExpressionBody(ArrowExpressionClauseSyntax expressionBody)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithExpressionBodyAccessor(this.SyntaxNode, expressionBody));
        }

        public LocalFunctionStatementSyntaxWrapper WithSemicolonToken(SyntaxToken semicolonToken)
        {
            return new LocalFunctionStatementSyntaxWrapper(WithSemicolonTokenAccessor(this.SyntaxNode, semicolonToken));
        }

        public LocalFunctionStatementSyntaxWrapper AddModifiers(params SyntaxToken[] items)
        {
            return this.WithModifiers(this.Modifiers.AddRange(items));
        }

        public LocalFunctionStatementSyntaxWrapper AddTypeParameterListParameters(params TypeParameterSyntax[] items)
        {
            var typeParameterList = this.TypeParameterList ?? SyntaxFactory.TypeParameterList();
            return this.WithTypeParameterList(typeParameterList.WithParameters(typeParameterList.Parameters.AddRange(items)));
        }

        public LocalFunctionStatementSyntaxWrapper AddParameterListParameters(params ParameterSyntax[] items)
        {
            return this.WithParameterList(this.ParameterList.WithParameters(this.ParameterList.Parameters.AddRange(items)));
        }

        public LocalFunctionStatementSyntaxWrapper AddConstraintClauses(params TypeParameterConstraintClauseSyntax[] items)
        {
            return this.WithConstraintClauses(this.ConstraintClauses.AddRange(items));
        }

        public LocalFunctionStatementSyntaxWrapper AddBodyStatements(params StatementSyntax[] items)
        {
            var body = this.Body ?? SyntaxFactory.Block();
            return this.WithBody(body.WithStatements(body.Statements.AddRange(items)));
        }
    }
}