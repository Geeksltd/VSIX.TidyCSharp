﻿using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeValidators;

public static class ArgumentListValidator
{
    public static bool ArgumentsCountShouldBe(this ArgumentListSyntax node, int count)
    {
        return node.Arguments.Count() == count;
    }

    public static bool FirstArgumentShouldBe(this ArgumentListSyntax node, string checkArg)
    {
        return checkArg.Equals(node.Arguments.FirstOrDefault().Expression.ToString());
    }

    public static bool FirstArgumentShouldContains(this ArgumentListSyntax node, string checkArg)
    {
        return node.Arguments.FirstOrDefault().Expression.ToString().Contains(checkArg);
    }
}