namespace TidyCSharp.Cli.ErrorNotification;
internal static class ErrorNotification
{
    internal static void EmailError(string message)
    {

    }

    internal static void WriteErrorToOutputWindow(Exception ex, string itemAddress = null)
    {
        
    }

    internal static void WriteErrorToFile(Exception ex, string itemAddress = null)
    {
        File.AppendAllText(Path.GetDirectoryName(TidyCSharpPackage.Instance.Solution.FilePath) + "\\Tidy.Error.log",
            Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                FileAddress = itemAddress,
                Exception = ex,
            }));
    }
}