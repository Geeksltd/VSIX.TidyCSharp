namespace TidyCSharp.Cli.Environment;

public static class App
{
    public static void Initialize()
    {
        AppDomain.CurrentDomain.UnhandledException += GlobalExceptionHandlerEvent;
    }

    private static void GlobalExceptionHandlerEvent(object sender, UnhandledExceptionEventArgs args)
    {
        ErrorNotification.ErrorNotification.WriteErrorToFile((Exception)args.ExceptionObject);
        ErrorNotification.ErrorNotification.WriteErrorToOutputWindow((Exception)args.ExceptionObject);
    }
}