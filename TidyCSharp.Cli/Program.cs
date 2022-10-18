using Microsoft.Build.Locator;
using TidyCSharp.Cli.Menus.Cleanup.CommandsHandlers;
using CommandLine;
using TidyCSharp.Cli.Menus.Cleanup.CommandRunners._Infra;

namespace TidyCSharp.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
#if DEBUG
        if (args.Length == 0)
            args = new string[] { "-p", "D:\\Projects\\Repositories\\hub\\MS.Hub.sln", "-o", "C:\\tmp\\TidyDiag.txt", "-m", "r" };
#endif

        await Parser.Default.ParseArguments<CliOptions>(args)
                   .WithParsedAsync(async o =>
                   {
                       await Start(o);
                   });
    }

    private static async Task Start(CliOptions o)
    {
        MSBuildLocator.RegisterDefaults();

        await new TidyCSharpPackage()
            .InitializeAsync(o.SolutionPath);

        if (o.Mode == "w")
        {
            var cleanUpRunner = new ActionSafeRulesCodeCleanup();
            await cleanUpRunner.RunSafeRulesCleanUpAsync();
        }
        else if (o.Mode == "r")
        {
            var cleanUpRunner = new ActionReadOnlyCodeCleanup();
            cleanUpRunner.RunReadOnlyCleanUp();

            CodeCleanerHost.GenerateMessages(o.OutputPath);

        }

    }
}
