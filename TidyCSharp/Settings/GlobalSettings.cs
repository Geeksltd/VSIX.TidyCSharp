using Newtonsoft.Json;
using System.IO;

namespace Geeks.GeeksProductivityTools
{
    public class GlobalSettings
    {
        const string SETTINGS_FILE_NAME = "geeks.settings.json";

        public GlobalSettings()
        {
        }

        //---------------------------------------

        string FilePath
        {
            get
            {
                var solutionPath = GetSolutionFilePath();
                if (solutionPath == null)
                    return string.Empty;

                return Path.Combine(solutionPath, SETTINGS_FILE_NAME);
            }
        }

        string GetSolutionFilePath()
        {
            var solution = App.DTE.Solution;

            if (solution == null || string.IsNullOrEmpty(solution.FullName))
                return null;

            return Path.GetDirectoryName(solution.FullName);
        }

        //---------------------------------------

        public GlobalSettings Load()
        {
            if (!File.Exists(FilePath))
                return new GlobalSettings();

            var json = File.ReadAllText(FilePath);
            Geeks.GeeksProductivityTools.App.Settings = JsonConvert.DeserializeObject<GlobalSettings>(json);

            return App.Settings;
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(App.Settings, Formatting.Indented);

            File.WriteAllText(FilePath, json);

            // Add file to solution items folder
            GeeksAddin.DteExtensions.GetSolutionItemsProject().ProjectItems.AddFromFile(FilePath);

            // Reload once saved
            Load();
        }
    }
}
