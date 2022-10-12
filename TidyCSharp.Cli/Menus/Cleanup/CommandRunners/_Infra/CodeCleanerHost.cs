using Microsoft.CodeAnalysis;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;
using TidyCSharp.Cli.Menus.Cleanup.Utils;
using TidyCSharp.Cli.Properties;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;

public class CodeCleanerHost
{
    public static async Task RunAsync(Document item, CodeCleanerType command, CleanupOptions cleanupOptions, bool isReportOnly = false)
    {
        Console.WriteLine($"Processing {Path.GetFileName(item.FilePath)} : {command}");
        if (!ActiveDocument.IsValid(item))
            ErrorNotification.ErrorNotification.EmailError(Resources.PrivateModifierCleanUpFailed);

        else
        {
            var instance = CodeCleanerFactory.Create(command, cleanupOptions, isReportOnly);
            await new CodeCleaner(instance, item).RunAsync();
        }
    }

    public static void GenerateMessages(string outputPath) => CodeCleanerCommandRunnerBase.GenerateMessages(outputPath);
}