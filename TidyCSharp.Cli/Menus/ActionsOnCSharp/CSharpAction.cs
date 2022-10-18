using Microsoft.CodeAnalysis;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers;

namespace TidyCSharp.Cli.Menus.ActionsOnCSharp;

public class CSharpAction
{
    public delegate Task TargetAction(Document item, CleanupOptions cleanupOptions);
}