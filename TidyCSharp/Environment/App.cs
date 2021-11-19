using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace Geeks.GeeksProductivityTools
{
    public static class App
    {
        public static DTE DTE;
        public static EnvDTE80.Events2 Events;
        public static GlobalSettings Settings;
        public static OptionsPage OptionsPage;

        public static void Initialize(OptionsPage optionsPage)
        {
            ThreadHelper.JoinableTaskFactory
                .Run(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            });

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(GlobalExceptionHandlerEvent);

            DTE = (DTE2)TidyCSharpPackage.GetGlobalService(typeof(SDTE));
            Events = (Events2)TidyCSharpPackage.GetGlobalService(typeof(Events2));
            Settings = new GlobalSettings();
            Settings.Load();
            OptionsPage = optionsPage;
        }

        static void GlobalExceptionHandlerEvent(object sender, UnhandledExceptionEventArgs args)
        {
            ErrorNotification.WriteErrorToFile((Exception)args.ExceptionObject);
            ErrorNotification.WriteErrorToOutputWindow((Exception)args.ExceptionObject);
        }
    }
}