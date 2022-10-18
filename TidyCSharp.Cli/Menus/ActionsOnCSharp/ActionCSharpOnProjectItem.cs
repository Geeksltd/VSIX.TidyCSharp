using Microsoft.CodeAnalysis;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers;
using static TidyCSharp.Cli.Menus.ActionsOnCSharp.CSharpAction;

namespace TidyCSharp.Cli.Menus.ActionsOnCSharp;

public class ActionCSharpOnProjectItem
{
    public static async Task ActionAsync(Document item, TargetAction targetAction, CleanupOptions cleanupOptions)
    {
        await targetAction(item, cleanupOptions);

        var linkedDocumentIds = item.GetLinkedDocumentIds().ToList();

        if (linkedDocumentIds == null) return;

        foreach (var linkedDocumentId in linkedDocumentIds)
        {
            await ActionAsync(item.Project.Documents.Single(a => a.Id == linkedDocumentId), targetAction, cleanupOptions);
        }
    }
}