﻿using System.Collections;
using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace TidyCSharp.Cli.Menus.Cleanup.Utils.RenameHelper.Lib;

internal abstract class SeparatedSyntaxListWrapper<TNode> : IEquatable<SeparatedSyntaxListWrapper<TNode>>, IReadOnlyList<TNode>
{
    private static readonly SyntaxWrapper<TNode> SyntaxWrapper = SyntaxWrapper<TNode>.Default;

    public static SeparatedSyntaxListWrapper<TNode> UnsupportedEmpty { get; } =
        new UnsupportedSyntaxList();

    public abstract int Count
    {
        get;
    }

    public abstract TextSpan FullSpan
    {
        get;
    }

    public abstract int SeparatorCount
    {
        get;
    }

    public abstract TextSpan Span
    {
        get;
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract object UnderlyingList
    {
        get;
    }

    public abstract TNode this[int index]
    {
        get;
    }

    public static bool operator ==(SeparatedSyntaxListWrapper<TNode> left, SeparatedSyntaxListWrapper<TNode> right)
    {
        throw new NotImplementedException();
    }

    public static bool operator !=(SeparatedSyntaxListWrapper<TNode> left, SeparatedSyntaxListWrapper<TNode> right)
    {
        throw new NotImplementedException();
    }

    // Summary:
    //     Creates a new list with the specified node added to the end.
    //
    // Parameters:
    //   node:
    //     The node to add.
    public SeparatedSyntaxListWrapper<TNode> Add(TNode node)
        => Insert(Count, node);

    // Summary:
    //     Creates a new list with the specified nodes added to the end.
    //
    // Parameters:
    //   nodes:
    //     The nodes to add.
    public SeparatedSyntaxListWrapper<TNode> AddRange(IEnumerable<TNode> nodes)
        => InsertRange(Count, nodes);

    public abstract bool Any();

    public abstract bool Contains(TNode node);

    public bool Equals(SeparatedSyntaxListWrapper<TNode> other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object obj)
    {
        throw new NotImplementedException();
    }

    public abstract TNode First();

    public abstract TNode FirstOrDefault();

    public Enumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<TNode>)this).GetEnumerator();

    public override abstract int GetHashCode();

    public abstract SyntaxToken GetSeparator(int index);

    public abstract IEnumerable<SyntaxToken> GetSeparators();

    public abstract SyntaxNodeOrTokenList GetWithSeparators();

    public abstract int IndexOf(Func<TNode, bool> predicate);

    public abstract int IndexOf(TNode node);

    public abstract SeparatedSyntaxListWrapper<TNode> Insert(int index, TNode node);

    public abstract SeparatedSyntaxListWrapper<TNode> InsertRange(int index, IEnumerable<TNode> nodes);

    public abstract TNode Last();

    public abstract int LastIndexOf(Func<TNode, bool> predicate);

    public abstract int LastIndexOf(TNode node);

    public abstract TNode LastOrDefault();

    public abstract SeparatedSyntaxListWrapper<TNode> Remove(TNode node);

    public abstract SeparatedSyntaxListWrapper<TNode> RemoveAt(int index);

    public abstract SeparatedSyntaxListWrapper<TNode> Replace(TNode nodeInList, TNode newNode);

    public abstract SeparatedSyntaxListWrapper<TNode> ReplaceRange(TNode nodeInList, IEnumerable<TNode> newNodes);

    public abstract SeparatedSyntaxListWrapper<TNode> ReplaceSeparator(SyntaxToken separatorToken, SyntaxToken newSeparator);

    public abstract string ToFullString();

    public override abstract string ToString();

    public struct Enumerator
    {

        public TNode Current
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class AutoWrapSeparatedSyntaxList<TSyntax> : SeparatedSyntaxListWrapper<TNode>
        where TSyntax : SyntaxNode
    {
        private readonly SeparatedSyntaxList<TSyntax> _syntaxList;

        public AutoWrapSeparatedSyntaxList(SeparatedSyntaxList<TSyntax> syntaxList)
        {
            _syntaxList = syntaxList;
        }

        public override int Count
            => _syntaxList.Count;

        public override TextSpan FullSpan
            => _syntaxList.FullSpan;

        public override int SeparatorCount
            => _syntaxList.SeparatorCount;

        public override TextSpan Span
            => _syntaxList.Span;

        public override object UnderlyingList
            => _syntaxList;

        public override TNode this[int index]
            => SyntaxWrapper.Wrap(_syntaxList[index]);

        public override bool Any()
            => _syntaxList.Any();

        public override bool Contains(TNode node)
            => _syntaxList.Contains(SyntaxWrapper.Unwrap(node));

        public override TNode First()
            => SyntaxWrapper.Wrap(_syntaxList.FirstOrDefault());

        public override TNode FirstOrDefault()
            => SyntaxWrapper.Wrap(_syntaxList.FirstOrDefault());

        public override int GetHashCode()
            => _syntaxList.GetHashCode();

        public override SyntaxToken GetSeparator(int index)
            => _syntaxList.GetSeparator(index);

        public override IEnumerable<SyntaxToken> GetSeparators()
            => _syntaxList.GetSeparators();

        public override SyntaxNodeOrTokenList GetWithSeparators()
            => _syntaxList.GetWithSeparators();

        public override int IndexOf(TNode node)
            => _syntaxList.IndexOf((TSyntax)SyntaxWrapper.Unwrap(node));

        public override int IndexOf(Func<TNode, bool> predicate)
            => _syntaxList.IndexOf(node => predicate(SyntaxWrapper.Wrap(node)));

        public override SeparatedSyntaxListWrapper<TNode> Insert(int index, TNode node)
            => new AutoWrapSeparatedSyntaxList<TSyntax>(_syntaxList.Insert(index, (TSyntax)SyntaxWrapper.Unwrap(node)));

        public override SeparatedSyntaxListWrapper<TNode> InsertRange(int index, IEnumerable<TNode> nodes)
            => new AutoWrapSeparatedSyntaxList<TSyntax>(_syntaxList.InsertRange(index, nodes.Select(node => (TSyntax)SyntaxWrapper.Unwrap(node))));

        public override TNode Last()
            => SyntaxWrapper.Wrap(_syntaxList.Last());

        public override int LastIndexOf(TNode node)
            => _syntaxList.LastIndexOf((TSyntax)SyntaxWrapper.Unwrap(node));

        public override int LastIndexOf(Func<TNode, bool> predicate)
            => _syntaxList.LastIndexOf(node => predicate(SyntaxWrapper.Wrap(node)));

        public override TNode LastOrDefault()
            => SyntaxWrapper.Wrap(_syntaxList.LastOrDefault());

        public override SeparatedSyntaxListWrapper<TNode> Remove(TNode node)
            => new AutoWrapSeparatedSyntaxList<TSyntax>(_syntaxList.Remove((TSyntax)SyntaxWrapper.Unwrap(node)));

        public override SeparatedSyntaxListWrapper<TNode> RemoveAt(int index)
            => new AutoWrapSeparatedSyntaxList<TSyntax>(_syntaxList.RemoveAt(index));

        public override SeparatedSyntaxListWrapper<TNode> Replace(TNode nodeInList, TNode newNode)
            => new AutoWrapSeparatedSyntaxList<TSyntax>(_syntaxList.Replace((TSyntax)SyntaxWrapper.Unwrap(nodeInList), (TSyntax)SyntaxWrapper.Unwrap(newNode)));

        public override SeparatedSyntaxListWrapper<TNode> ReplaceRange(TNode nodeInList, IEnumerable<TNode> newNodes)
            => new AutoWrapSeparatedSyntaxList<TSyntax>(_syntaxList.ReplaceRange((TSyntax)SyntaxWrapper.Unwrap(nodeInList), newNodes.Select(node => (TSyntax)SyntaxWrapper.Unwrap(node))));

        public override SeparatedSyntaxListWrapper<TNode> ReplaceSeparator(SyntaxToken separatorToken, SyntaxToken newSeparator)
            => new AutoWrapSeparatedSyntaxList<TSyntax>(_syntaxList.ReplaceSeparator(separatorToken, newSeparator));

        public override string ToFullString()
            => _syntaxList.ToFullString();

        public override string ToString()
            => _syntaxList.ToString();
    }

    private sealed class UnsupportedSyntaxList : SeparatedSyntaxListWrapper<TNode>
    {
        private static readonly SeparatedSyntaxList<SyntaxNode> SyntaxList = default(SeparatedSyntaxList<SyntaxNode>);

        public UnsupportedSyntaxList()
        {
        }

        public override int Count
            => 0;

        public override TextSpan FullSpan
            => SyntaxList.FullSpan;

        public override int SeparatorCount
            => 0;

        public override TextSpan Span
            => SyntaxList.Span;

        public override object UnderlyingList
            => null;

        public override TNode this[int index]
            => SyntaxWrapper.Wrap(SyntaxList[index]);

        public override bool Any()
            => false;

        public override bool Contains(TNode node)
            => false;

        public override TNode First()
            => SyntaxWrapper.Wrap(SyntaxList.FirstOrDefault());

        public override TNode FirstOrDefault()
            => SyntaxWrapper.Wrap(default(SyntaxNode));

        public override int GetHashCode()
            => SyntaxList.GetHashCode();

        public override SyntaxToken GetSeparator(int index)
            => SyntaxList.GetSeparator(index);

        public override IEnumerable<SyntaxToken> GetSeparators()
            => SyntaxList.GetSeparators();

        public override SyntaxNodeOrTokenList GetWithSeparators()
            => SyntaxList.GetWithSeparators();

        public override int IndexOf(TNode node)
            => SyntaxList.IndexOf(SyntaxWrapper.Unwrap(node));

        public override int IndexOf(Func<TNode, bool> predicate)
            => SyntaxList.IndexOf(node => predicate(SyntaxWrapper.Wrap(node)));

        public override SeparatedSyntaxListWrapper<TNode> Insert(int index, TNode node)
        {
            throw new NotSupportedException();
        }

        public override SeparatedSyntaxListWrapper<TNode> InsertRange(int index, IEnumerable<TNode> nodes)
        {
            throw new NotSupportedException();
        }

        public override TNode Last()
            => SyntaxWrapper.Wrap(SyntaxList.Last());

        public override int LastIndexOf(TNode node)
            => SyntaxList.LastIndexOf(SyntaxWrapper.Unwrap(node));

        public override int LastIndexOf(Func<TNode, bool> predicate)
            => SyntaxList.LastIndexOf(node => predicate(SyntaxWrapper.Wrap(node)));

        public override TNode LastOrDefault()
            => SyntaxWrapper.Wrap(default(SyntaxNode));

        public override SeparatedSyntaxListWrapper<TNode> Remove(TNode node)
        {
            throw new NotSupportedException();
        }

        public override SeparatedSyntaxListWrapper<TNode> RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public override SeparatedSyntaxListWrapper<TNode> Replace(TNode nodeInList, TNode newNode)
        {
            throw new NotSupportedException();
        }

        public override SeparatedSyntaxListWrapper<TNode> ReplaceRange(TNode nodeInList, IEnumerable<TNode> newNodes)
        {
            throw new NotSupportedException();
        }

        public override SeparatedSyntaxListWrapper<TNode> ReplaceSeparator(SyntaxToken separatorToken, SyntaxToken newSeparator)
        {
            throw new NotSupportedException();
        }

        public override string ToFullString()
            => SyntaxList.ToFullString();

        public override string ToString()
            => SyntaxList.ToString();
    }
}