using Microsoft.CodeAnalysis;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;

public class CodeCleaner
{
    // TODO: By Alireza =>  To return Syntax node and pass syntaxNode no next clean up function and dont close windows for each cleanup , just for something like organize usings
    // public ICodeCleaner Cleaner { get; private set; }
    private ICodeCleaner _cleaner;
    private Document _item;

    public CodeCleaner(ICodeCleaner cleaner, Document item)
    {
        _cleaner = cleaner;
        _item = item;
    }

    public Task RunAsync() => _cleaner.RunAsync(_item);
}