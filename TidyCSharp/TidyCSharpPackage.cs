using Geeks.GeeksProductivityTools.Menus.Cleanup;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Geeks.GeeksProductivityTools
{
    [ProvideAutoLoad("adfc4e64-0397-11d1-9f4e-00a0c911004f", PackageAutoLoadFlags.BackgroundLoad)]    // Microsoft.VisualStudio.VSConstants.UICONTEXT_NoSolution
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideService(typeof(IMenuCommandService), IsAsyncQueryable = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionsPage), "Geeks productivity tools", "General", 0, 0, true)]
    [Guid(GuidList.GuidGeeksProductivityToolsPkgString)]
    [ProvideAppCommandLine("TidyReportSwitch", typeof(TidyCSharpPackage), Arguments = "0", DemandLoad = 1)]
    public sealed class TidyCSharpPackage : AsyncPackage
    {
        public TidyCSharpPackage() { }

        // Strongly reference events so that it's not GC'd
        EnvDTE.DocumentEvents docEvents;
        EnvDTE.SolutionEvents solEvents;
        EnvDTE.Events events;
        EnvDTE.BuildEvents buildEvent;

        string TidyProcesLogsPath = Path.Combine(Path.GetTempPath() + "Tidy.Process.Log.txt");

        public static TidyCSharpPackage Instance { get; private set; }

        public Workspace VsWorkspace { get; set; }

        bool bResetWorkingSolution;
        Solution _CleanupWorkingSolution;

        public Solution CleanupWorkingSolution
        {
            get
            {
                if (bResetWorkingSolution || _CleanupWorkingSolution == null)
                {
                    _CleanupWorkingSolution = VsWorkspace.CurrentSolution;
                    bResetWorkingSolution = false;
                }

                return _CleanupWorkingSolution;
            }
        }

        public async Task RefreshSolutionAsync(Solution newSolution)
        {
            _CleanupWorkingSolution = ExtactChanges(_CleanupWorkingSolution, newSolution);
        }

        Solution ExtactChanges(Solution oldSolution, Solution newSolution)
        {
            lock (Instance)
            {
                var pchanges = oldSolution.GetChanges(newSolution).GetProjectChanges();
                var changedSolution = oldSolution;

                foreach (var changedProject in pchanges)
                {
                    var docchanges = changedProject.GetChangedDocuments();

                    foreach (var changedDocumentId in docchanges)
                    {
                        var changedDocument = changedProject.OldProject.GetDocument(changedDocumentId);
                        SyntaxNode documentRoot = null;
                        ThreadHelper.JoinableTaskFactory
                        .Run(async delegate
                        {
                            documentRoot = await changedDocument.GetSyntaxRootAsync();
                        });
                        documentRoot.WriteSourceTo(changedDocument.FilePath);
                        changedSolution = changedSolution.WithDocumentSyntaxRoot(changedDocumentId, documentRoot);

                        {
                            var otherSharedProjects = changedSolution.Projects.Where(p => p.Name != changedDocument.Project.Name);

                            foreach (var otherProject in otherSharedProjects)
                            {
                                foreach (var documentItem in otherProject.Documents.Where(d => d.FilePath == changedDocument.FilePath))
                                {
                                    changedSolution = changedSolution.WithDocumentText(documentItem.Id, documentRoot.GetText());
                                }
                            }
                        }
                    }
                }

                return changedSolution;
            }
        }

        public void SaveSolutionChanges()
        {
            if (_CleanupWorkingSolution == null) return;
            var changedSolution = ExtactChanges(VsWorkspace.CurrentSolution, _CleanupWorkingSolution);
            var b = VsWorkspace.TryApplyChanges(changedSolution);
            _CleanupWorkingSolution = null;
            bResetWorkingSolution = true;
        }

        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, false))
               await tidyLogWriter.WriteLineAsync("Initialization has begun...");


            await base.InitializeAsync(cancellationToken, progress);

            using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
                await tidyLogWriter.WriteLineAsync("Base has initialized");


            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
                await tidyLogWriter.WriteLineAsync("Switched to main thread");


            App.Initialize(GetDialogPage(typeof(OptionsPage)) as OptionsPage);

            using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
                await tidyLogWriter.WriteLineAsync("App initialized");


            Instance = this;

            var componentModel = (IComponentModel)await GetServiceAsync(typeof(SComponentModel)).ConfigureAwait(true);

            if (componentModel != null)
                VsWorkspace = componentModel.GetService<VisualStudioWorkspace>();

            using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
            {
                if (componentModel == null)
                    await tidyLogWriter.WriteLineAsync("component Model is null");
                else
                    await tidyLogWriter.WriteLineAsync("component has value");
            }

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var menuCommandService = await GetServiceAsync(typeof(IMenuCommandService)).ConfigureAwait(true) as OleMenuCommandService;

            if (null != menuCommandService)
            {
                new ActionCustomCodeCleanup(menuCommandService).SetupCommands();
            }

            using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
            {
                if (menuCommandService == null)
                    await tidyLogWriter.WriteLineAsync("menu Command Service  is null");
                else
                    await tidyLogWriter.WriteLineAsync("menu Command Service has value");
            }

            // Hook up event handlers
            events = App.Dte.Events;
            buildEvent = events.BuildEvents;
            docEvents = events.DocumentEvents;
            solEvents = events.SolutionEvents;
            buildEvent.OnBuildBegin += BuildEvent_OnBuildBegin;
            docEvents.DocumentSaved += DocEvents_DocumentSaved;
            solEvents.Opened += delegate { App.Initialize(GetDialogPage(typeof(OptionsPage)) as OptionsPage); };

            using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
                await tidyLogWriter.WriteLineAsync("Initialization has finished...");

        }

        void BuildEvent_OnBuildBegin(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
        {
            if (File.Exists(Path.Combine(Path.GetTempPath(), "TidyCurrentfilelog.txt")))
            {
                try
                {
                    File.Delete(Path.Combine(Path.GetTempPath(), "TidyCurrentfilelog.txt"));
                }
                catch
                {
                }
            }

            if (File.Exists(Path.Combine(Path.GetTempPath(), "TidyCurrentActionslog.txt")))
            {
                try
                {
                    File.Delete(Path.Combine(Path.GetTempPath(), "TidyCurrentActionslog.txt"));
                }
                catch
                {
                }
            }

            using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
                tidyLogWriter.WriteLine("Build process has finished...");


            var commandLineArgs = Environment.GetCommandLineArgs().ToList();
            var isReportMode = false;

            if (commandLineArgs != null && commandLineArgs.Any())
            {
                foreach (var argum in commandLineArgs)
                {
                    if (argum.Equals("/tidyreportswitch", StringComparison.OrdinalIgnoreCase))
                    {
                        isReportMode = true;
                        break;
                    }
                }
            }

            using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
            {
                tidyLogWriter.WriteLine("Command line arguments ========================");

                if (commandLineArgs != null && commandLineArgs.Any())
                {
                    foreach (var argum in commandLineArgs)
                        tidyLogWriter.WriteLine(argum);

                }
                else
                {
                    tidyLogWriter.WriteLine("No arguments!!!");
                }

                if (isReportMode)
                {
                    tidyLogWriter.WriteLine("Report mode is on");
                }
                else
                {
                    tidyLogWriter.WriteLine("Report mode is off");
                }
            }

            if (isReportMode)
            {
                using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
                    tidyLogWriter.WriteLine("ActionReadOnlyCodeCleanup");


                var cleanUpRunner = new ActionReadOnlyCodeCleanup();

                using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
                    tidyLogWriter.WriteLine("RunReadOnlyCleanUp");


                cleanUpRunner.RunReadOnlyCleanUp();

                using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
                    tidyLogWriter.WriteLine("GenerateMessages");


                CodeCleanerHost.GenerateMessages();

                using (var tidyLogWriter = new StreamWriter(TidyProcesLogsPath, true))
                    tidyLogWriter.WriteLine("Tidy Report process has done.");

            }
        }

        void DocEvents_DocumentSaved(EnvDTE.Document document)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                if (document.Name.EndsWith(".cs") ||
                    document.Name.EndsWith(".css") ||
                    document.Name.EndsWith(".js") ||
                    document.Name.EndsWith(".ts"))
                {
                    document.DTE.ExecuteCommand("Edit.FormatDocument");
                }

                if (!document.Saved) document.Save();
            }
            catch
            {
            }
        }
    }
}