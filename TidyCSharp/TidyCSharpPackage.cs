using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.CodeAnalysis;
using Geeks.GeeksProductivityTools.Menus.Cleanup;
using System.Linq;

namespace Geeks.GeeksProductivityTools
{
    [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]    // Microsoft.VisualStudio.VSConstants.UICONTEXT_NoSolution
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideOptionPage(typeof(OptionsPage), "Geeks productivity tools", "General", 0, 0, true)]
    [Guid(GuidList.GuidGeeksProductivityToolsPkgString)]
    public sealed class TidyCSharpPackage : Package
    {
        public TidyCSharpPackage() { }

        // Strongly reference events so that it's not GC'd
        EnvDTE.DocumentEvents docEvents;
        EnvDTE.SolutionEvents solEvents;
        EnvDTE.Events events;

        public static TidyCSharpPackage Instance { get; private set; }
        public Workspace VsWorkspace { get; set; }

        bool bResetWorkingSolution = false;
        Solution _CleanupWorkingSolution = null;
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
        public void RefreshSolution(Solution newSolution)
        {
            _CleanupWorkingSolution = ExtactChanges(_CleanupWorkingSolution, newSolution);
        }

        private Solution ExtactChanges(Solution oldSolution, Solution newSolution)
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
                        var documentRoot = changedDocument.GetSyntaxRootAsync().Result;
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
            bool b = VsWorkspace.TryApplyChanges(changedSolution);
            _CleanupWorkingSolution = null;
            bResetWorkingSolution = true;
        }
        protected override void Initialize()
        {
            base.Initialize();
            App.Initialize(GetDialogPage(typeof(OptionsPage)) as OptionsPage);

            Instance = this;

            var componentModel = (IComponentModel)this.GetService(typeof(SComponentModel));
            VsWorkspace = componentModel.GetService<VisualStudioWorkspace>();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (null != menuCommandService)
            {
                new Menus.Cleanup.ActionCustomCodeCleanup(menuCommandService).SetupCommands();
            }

            // Hook up event handlers
            events = App.DTE.Events;
            docEvents = events.DocumentEvents;
            solEvents = events.SolutionEvents;
            docEvents.DocumentSaved += DocumentEvents_DocumentSaved;
            solEvents.Opened += delegate { App.Initialize(GetDialogPage(typeof(OptionsPage)) as OptionsPage); };
        }

        void DocumentEvents_DocumentSaved(EnvDTE.Document document)
        {
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
