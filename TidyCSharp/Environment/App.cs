using System;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;

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

            DTE = (DTE2)GeeksProductivityToolsPackage.GetGlobalService(typeof(SDTE));

            Settings = new GlobalSettings();
            Settings.Load();

            OptionsPage = optionsPage;
        }

        static void GlobalExceptionHandlerEvent(object sender, UnhandledExceptionEventArgs args)
        {
            ErrorNotification.EmailError((Exception)args.ExceptionObject);
        }
    }
}
