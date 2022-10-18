using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TidyCSharp.Cli.Extensions;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers.Infra;
using TidyCSharp.Cli.Menus.Cleanup.SyntaxNodeExtractors;
using TidyCSharp.Cli.Menus.Cleanup.Utils;
using TidyCSharp.Cli.Utility;

namespace TidyCSharp.Cli.Menus.ActionsOnCSharp;

public class ActionsCSharpOnFile
{
    public static async Task DoCleanupAsync(Document item, CleanupOptions cleanupOptions)
    {
        if (!item.IsCsharpFile() || item.IsCSharpDesignerFile()) return;

        try
        {
            var path = item.FilePath;
            if (path.EndsWithAny(new[] { "AssemblyInfo.cs", "TheApplication.cs" })) return;

            var documentText = item.ToSyntaxNode().GetText().ToString();
            if (documentText.Contains("[EscapeGCop(\"Auto generated code.\")]")) return;

            if (item.ToSyntaxNode()
                .DescendantNodesOfType<AttributeSyntax>()
                .Any(x => x.Name.ToString() == "EscapeGCop" &&
                          x.ArgumentList != null &&
                          x.ArgumentList.Arguments.FirstOrDefault().ToString()
                          == "\"Auto generated code.\""))
            {
                return;
            }

            foreach (var actionTypeItem in cleanupOptions.ActionTypes)
            {
                if (actionTypeItem == CodeCleanerType.NormalizeWhiteSpaces) continue;
                if (actionTypeItem == CodeCleanerType.OrganizeUsingDirectives) continue;
                if (actionTypeItem == CodeCleanerType.ConvertMsharpGeneralMethods) continue;

                await CodeCleanerHost.RunAsync(item, actionTypeItem, cleanupOptions);
            }

            if (cleanupOptions.ActionTypes.Contains(CodeCleanerType.NormalizeWhiteSpaces))
            {
                await CodeCleanerHost.RunAsync(item, CodeCleanerType.NormalizeWhiteSpaces, cleanupOptions);
            }

            if (cleanupOptions.ActionTypes.Contains(CodeCleanerType.OrganizeUsingDirectives))
            {
                await CodeCleanerHost.RunAsync(item, CodeCleanerType.OrganizeUsingDirectives, cleanupOptions);
            }
            else
            {
                //TODO : item.Save();
            }

            if (cleanupOptions.ActionTypes.Contains(CodeCleanerType.ConvertMsharpGeneralMethods))
            {
                await CodeCleanerHost.RunAsync(item, CodeCleanerType.ConvertMsharpGeneralMethods, cleanupOptions);
            }
        }
        catch (Exception e)
        {
            ErrorNotification.ErrorNotification.WriteErrorToFile(e, item.FilePath);
            ErrorNotification.ErrorNotification.WriteErrorToOutputWindow(e, item.FilePath);
            ProcessActions.GeeksProductivityToolsProcess();
        }
    }

    public static async Task ReportOnlyDoNotCleanupAsync(Document item, CleanupOptions cleanupOptions)
    {
        if (!item.IsCsharpFile() || item.IsCSharpDesignerFile()) return;

        try
        {
            // Sometimes cannot find document's file
            try
            {
                var documentText = item.ToSyntaxNode().SyntaxTree.GetText().ToString();
                if (documentText.Contains("[EscapeGCop(\"Auto generated code.\")]")) return;
            }
            catch
            {
                return;
            }

            var path = item.FilePath;
            if (path.EndsWithAny(new[] { "AssemblyInfo.cs", "TheApplication.cs" })) return;

            using (var tidyruntimelog = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyCurrentfilelog.txt"), true))
                tidyruntimelog.WriteLine(path);


            foreach (var actionTypeItem in cleanupOptions.ActionTypes)
            {
                if (actionTypeItem != CodeCleanerType.NormalizeWhiteSpaces
                    && actionTypeItem != CodeCleanerType.OrganizeUsingDirectives
                    && actionTypeItem != CodeCleanerType.ConvertMsharpGeneralMethods)
                {
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    await CodeCleanerHost.RunAsync(item, actionTypeItem, cleanupOptions, true);
                    watch.Stop();

                    using (var tidyruntimelog = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyCurrentActionslog.txt"), true))
                    {
                        tidyruntimelog.WriteLine("Phase1-" + actionTypeItem.ToString() + "-" + watch.ElapsedMilliseconds + " ms");
                    }
                }
            }

            if (cleanupOptions.ActionTypes.Contains(CodeCleanerType.NormalizeWhiteSpaces))
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                await CodeCleanerHost.RunAsync(item, CodeCleanerType.NormalizeWhiteSpaces, cleanupOptions, true);
                watch.Stop();

                using (var tidyruntimelog = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyCurrentActionslog.txt"), true))
                {
                    tidyruntimelog.WriteLine("Phase2-" + "NormalizeWhiteSpaces" + "-" + watch.ElapsedMilliseconds + " ms");
                }
            }

            if (cleanupOptions.ActionTypes.Contains(CodeCleanerType.OrganizeUsingDirectives))
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                await CodeCleanerHost.RunAsync(item, CodeCleanerType.OrganizeUsingDirectives, cleanupOptions, true);
                watch.Stop();

                using (var tidyruntimelog = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyCurrentActionslog.txt"), true))
                {
                    tidyruntimelog.WriteLine("Phase3-" + "OrganizeUsingDirectives" + "-" + watch.ElapsedMilliseconds + " ms");
                }
            }

            if (cleanupOptions.ActionTypes.Contains(CodeCleanerType.ConvertMsharpGeneralMethods))
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                await CodeCleanerHost.RunAsync(item, CodeCleanerType.ConvertMsharpGeneralMethods, cleanupOptions, true);
                watch.Stop();

                using (var tidyruntimelog = new StreamWriter(Path.Combine(Path.GetTempPath(), "TidyCurrentActionslog.txt"), true))
                {
                    tidyruntimelog.WriteLine("Phase4-" + "ConvertMsharpGeneralMethods" + "-" + watch.ElapsedMilliseconds + " ms");
                }
            }
        }
        catch (Exception e)
        {
            ErrorNotification.ErrorNotification.WriteErrorToFile(e, item.FilePath);
            ErrorNotification.ErrorNotification.WriteErrorToOutputWindow(e, item.FilePath);
            ProcessActions.GeeksProductivityToolsProcess();
        }
    }
}