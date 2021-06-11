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

namespace Geeks.GeeksProductivityTools
{
	[ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F", PackageAutoLoadFlags.BackgroundLoad)]    // Microsoft.VisualStudio.VSConstants.UICONTEXT_NoSolution
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
		EnvDTE.BuildEvents solbuildEvents;
		EnvDTE.CommandEvents commandEvents;

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

		public void RefreshSolution(Solution newSolution)
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

		protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			await base.InitializeAsync(cancellationToken, progress);

			App.Initialize(GetDialogPage(typeof(OptionsPage)) as OptionsPage);

			Instance = this;

			var componentModel = (IComponentModel)await GetServiceAsync(typeof(SComponentModel));
			VsWorkspace = componentModel.GetService<VisualStudioWorkspace>();

			// Add our command handlers for menu (commands must exist in the .vsct file)
			var menuCommandService = await GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

			if (null != menuCommandService)
			{
				new ActionCustomCodeCleanup(menuCommandService).SetupCommands();
			}

			// Hook up event handlers
			events = App.DTE.Events;

			docEvents = events.DocumentEvents;
			solEvents = events.SolutionEvents;
			solbuildEvents = events.BuildEvents;
			docEvents.DocumentSaved += DocumentEvents_DocumentSaved;
			solEvents.Opened += delegate { App.Initialize(GetDialogPage(typeof(OptionsPage)) as OptionsPage); };
			solbuildEvents.OnBuildBegin += BuildEvents_OnBuildBegin;
			commandEvents = events.CommandEvents;
		}

		private void BuildEvents_OnBuildBegin(EnvDTE.vsBuildScope Scope, EnvDTE.vsBuildAction Action)
		{
			var cleanUpRunner = new ActionReadOnlyCodeCleanup();
			cleanUpRunner.RunReadOnlyCleanUp();
			CodeCleanerHost.GenerateMessages();

		}

		void DocumentEvents_DocumentSaved(EnvDTE.Document document)
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