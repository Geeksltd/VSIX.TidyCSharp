
using Microsoft.CodeAnalysis;

namespace TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;

public interface ICodeCleaner
{
    Task RunAsync(Document item);
}