using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Geeks.GeeksProductivityTools.Menus.Cleanup
{
    public static class SyntaxTokenExtensions
    {
        public static SyntaxNode RemovePrivateTokens(this SyntaxNode root, List<SyntaxToken> tokens)
        {
            if (!tokens.Any()) return root;

            return root.ReplaceTokens(tokens, MakeReplacementToken(tokens));
        }

        static Func<SyntaxToken, SyntaxToken, SyntaxToken> MakeReplacementToken(List<SyntaxToken> tokens)
        {
            // replace with the LeadingTrivia so that the comments (if any) will not be lost also the private keyword is replaced at the same time
            return (oldToken, newToken) => SyntaxFactory.ParseToken(oldToken.LeadingTrivia.ToFullString());
        }

        static object _lockFileWrite = new object();
        public static void WriteSourceTo(this SyntaxNode sourceCode, string filePath)
        {
            lock (_lockFileWrite)
            {
                var encoding = DetectFileEncoding(filePath);

                var source = sourceCode.ToFullString().Trim(new[] { '\r', '\n' });
                var fileText = File.ReadAllText(filePath).Trim(new[] { '\r', '\n' });

                var bEqual = string.Compare(source, fileText, StringComparison.Ordinal) == 0;

                if (!bEqual)
                {
                    using (var write = new StreamWriter(filePath, false, encoding))
                        write.Write(sourceCode.ToFullString());
                }
            }
        }

        static Encoding DetectFileEncoding(string filePath)
        {
            Encoding encoding = null;

            using (var reader = new StreamReader(filePath))
            {
                encoding = reader.CurrentEncoding;
            }

            using (var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] bom = new byte[4];
                reader.Read(bom, 0, 4);
                if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                {
                    encoding = new UTF8Encoding(true);
                }
                else if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76)
                {
                    encoding = new UTF7Encoding(true);
                }
                else if (bom[0] == 0xff && bom[1] == 0xfe)
                {
                    encoding = new UnicodeEncoding(false, true);
                }
                else if (bom[0] == 0xfe && bom[1] == 0xff)
                {
                    encoding = new UTF8Encoding(false);
                    //encoding = new BigEndianUnicode(true);
                }
                else if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff)
                {
                    encoding = new UTF32Encoding(false, true);
                }
                else
                {
                    encoding = new UTF8Encoding(false);
                }

            }
            return encoding;
        }

        public static SyntaxNode ToSyntaxNode(this ProjectItem item)
        {
            lock (_lockFileWrite)
            {
                return CSharpSyntaxTree.ParseText(File.ReadAllText(item.ToFullPathPropertyValue())).GetRoot();
            }
        }

        public static string ToFullPathPropertyValue(this ProjectItem item)
        {
            if (item == null) return null;
            return item.Properties.Item("FullPath").Value.ToString();
        }

        public static SyntaxToken WithoutTrivia(this SyntaxToken token)
        {
            return token.WithLeadingTrivia().WithTrailingTrivia();
        }
        public static SyntaxTriviaList WithoutWhitespaceTrivia(this SyntaxTriviaList triviaList)
        {
            return new SyntaxTriviaList().AddRange(triviaList.Where(t => !t.IsWhitespaceTrivia()));
        }

        public static SyntaxToken WithoutWhitespaceTrivia(this SyntaxToken token)
        {
            return
                token
                    .WithLeadingTrivia(token.LeadingTrivia.Where(t => !t.IsWhitespaceTrivia()))
                    .WithTrailingTrivia(token.TrailingTrivia.Where(t => !t.IsWhitespaceTrivia()));
        }
        public static T WithoutWhitespaceTrivia<T>(this T token)
            where T : SyntaxNode
        {
            return
                token
                    .WithLeadingTrivia(token.GetLeadingTrivia().Where(t => !t.IsWhitespaceTrivia()))
                    .WithTrailingTrivia(token.GetTrailingTrivia().Where(t => !t.IsWhitespaceTrivia()));
        }
        public static bool HasNoneWhitespaceTrivia(this IEnumerable<SyntaxTrivia> triviaList, SyntaxKind[] exceptionList = null)
        {
            if (exceptionList == null)
                return triviaList.Any(t => !t.IsWhitespaceTrivia());

            return triviaList.Any(t => !t.IsWhitespaceTrivia() && exceptionList.Any(e => t.IsKind(e)) == false);
        }
        public static bool IsWhitespaceTrivia(this SyntaxTrivia trivia)
        {
            return trivia.IsKind(SyntaxKind.EndOfLineTrivia) || trivia.IsKind(SyntaxKind.WhitespaceTrivia);
        }
        public static bool HasNoneWhitespaceTrivia(this SyntaxNode node, SyntaxKind[] exceptionList = null)
        {
            if (node.ContainsDirectives) return true;
            if (node.HasStructuredTrivia) return true;
            if (node.DescendantTrivia(descendIntoTrivia: true).HasNoneWhitespaceTrivia(exceptionList)) return true;
            return false;
        }
        public static bool HasNoneWhitespaceTrivia(this SyntaxToken token, SyntaxKind[] exceptionList = null)
        {
            if (token.ContainsDirectives) return true;
            if (token.HasStructuredTrivia) return true;
            if (token.GetAllTrivia().HasNoneWhitespaceTrivia()) return true;
            return false;
        }

        public static bool IsPrivate(this FieldDeclarationSyntax field)
        {
            return IsPrivate(field.Modifiers);
        }
        public static bool IsPrivate(this PropertyDeclarationSyntax field)
        {
            return IsPrivate(field.Modifiers);
        }

        public static bool IsPublic(this FieldDeclarationSyntax field)
        {
            return field.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }
        public static bool IsPublic(this PropertyDeclarationSyntax field)
        {
            return field.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
        }
        public static bool IsProtected(this FieldDeclarationSyntax field)
        {
            return field.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword));
        }
        public static bool IsProtected(this PropertyDeclarationSyntax field)
        {
            return field.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword));
        }
        public static bool IsInternal(this FieldDeclarationSyntax field)
        {
            return field.Modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword));
        }
        public static bool IsInternal(this PropertyDeclarationSyntax field)
        {
            return field.Modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword));
        }

        public static bool IsPrivate(this LocalDeclarationStatementSyntax local)
        {
            return IsPrivate(local.Modifiers);
        }

        private static bool IsPrivate(SyntaxTokenList modifiers)
        {
            return
                modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)) ||
                modifiers
                    .Any(
                        m =>
                            m.IsKind(SyntaxKind.PublicKeyword) ||
                            m.IsKind(SyntaxKind.ProtectedKeyword) ||
                            m.IsKind(SyntaxKind.InternalKeyword)
                    ) == false;
        }
    }
}
