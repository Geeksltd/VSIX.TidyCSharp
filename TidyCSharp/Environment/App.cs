using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace Geeks.GeeksProductivityTools
{
    public static class App
    {
        public static DTE Dte { get;private set;}
        public static GlobalSettings Settings { get;set;}

        public static void Initialize(OptionsPage optionsPage)
        {
            ThreadHelper.JoinableTaskFactory
                .Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            });

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandlerEvent);

            Dte = (DTE2)TidyCSharpPackage.GetGlobalService(typeof(SDTE));
            Settings = new GlobalSettings();
            Settings.Load();
        }

        static void GlobalExceptionHandlerEvent(object sender, UnhandledExceptionEventArgs args)
        {
            ErrorNotification.WriteErrorToFile((Exception)args.ExceptionObject);
            ErrorNotification.WriteErrorToOutputWindow((Exception)args.ExceptionObject);
        }
    }
}