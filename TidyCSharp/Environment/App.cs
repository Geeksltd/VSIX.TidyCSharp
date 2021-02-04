using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace Geeks.GeeksProductivityTools
{
    public static class App
    {
        public static DTE2 DTE;
        public static GlobalSettings Settings;
        public static OptionsPage OptionsPage;

        public static void Initialize(OptionsPage optionsPage)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandlerEvent);

            DTE = (DTE2)TidyCSharpPackage.GetGlobalService(typeof(SDTE));

            Settings = new GlobalSettings();
            Settings.Load();

            OptionsPage = optionsPage;
        }

        static void GlobalExceptionHandlerEvent(object sender, UnhandledExceptionEventArgs args)
        {
            ErrorNotification.WriteErrorToFile((Exception)args.ExceptionObject);
            ErrorNotification.EmailError((Exception)args.ExceptionObject);
        }
    }
}