using Microsoft.CodeAnalysis;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners;

public class UsingDirectiveOrganizer : CodeCleanerCommandRunnerBase
{
    private async static Task<Document> RemoveUnusedImportsAsync(Document document)
    {
        var compilation = await document.Project.GetCompilationAsync();
        var tree = await document.GetSyntaxTreeAsync();

        if(compilation == null || tree == null)
        {
            return document;
        }

        var root = tree.GetRoot();
        var unusedImportNodes = compilation.GetDiagnostics()
            .Where(d => d.Id == "CS8019")
            .Where(d => d.Location?.SourceTree == tree)
            .Select(d => root.FindNode(d.Location.SourceSpan))
            .ToList();

        if (!unusedImportNodes.Any())
        {
            return document;
        }

        return document.WithSyntaxRoot(
            root.RemoveNodes(unusedImportNodes, SyntaxRemoveOptions.KeepNoTrivia));
    }
    public override async Task<SyntaxNode> CleanUpAsync(SyntaxNode initialSourceNode)
    {
        //TODO
        var doc = await RemoveUnusedImportsAsync(ProjectItemDetails.Document);
        return await doc.GetSyntaxRootAsync();

        //var item = ProjectItemDetails.Document;

        //try
        //{
        //    item.Open(Constants.vsViewKindCode);

        //    var document = Document;
        //    document.Activate();

        //    try { document.DTE.ExecuteCommand(UsingsCommands.RemoveAndSortCommandName); }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message != "Command \"Edit.RemoveAndSort\" is not available.") throw;

        //        document.Activate();
        //        document.DTE.ExecuteCommand(UsingsCommands.RemoveAndSortCommandName);
        //    }

        //    var doc = (EnvDTE.TextDocument)(document.Object("TextDocument"));
        //    var p = doc.StartPoint.CreateEditPoint();
        //    var s = p.GetText(doc.EndPoint);
        //    var modified = SyntaxFactory.ParseSyntaxTree(s);

        //    if (IsReportOnlyMode &&
        //        !IsEquivalentToUnModified(await modified.GetRootAsync()))
        //    {
        //        CollectMessages(new ChangesReport(initialSourceNode)
        //        {
        //            LineNumber = 1,
        //            Column = 1,
        //            Message = "Your Using usage is not good",
        //            Generator = nameof(UsingDirectiveOrganizer)
        //        });

        //        document.Undo();
        //        return initialSourceNode;
        //    }

        //    document.Save();
        //}
        //catch (Exception e)
        //{
        //    ErrorNotification.ErrorNotification.WriteErrorToFile(e, initialSourceNode.GetFilePath());
        //    ErrorNotification.ErrorNotification.WriteErrorToOutputWindow(e, initialSourceNode.GetFilePath());
        //    ProcessActions.GeeksProductivityToolsProcess();
        //}

        //return item.ToSyntaxNode();
    }
}