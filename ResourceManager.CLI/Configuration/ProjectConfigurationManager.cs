using ResourceManager.Core.Services;
using System.Text.Json;

namespace ResourceManager.CLI.Configuration
{
    public class ProjectConfigurationManager
    {
        private readonly  string _configPath;
        private readonly ProjectContext _projectContext;

        public ProjectConfigurationManager(ProjectContext projectContext)
        {
            _projectContext = projectContext;
            string configFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ResourceManager");

            Directory.CreateDirectory(configFolder);
            _configPath = Path.Combine(configFolder, "project-config.json");
        }
        public bool LoadProjectConfiguration()
        {
            if (!File.Exists(_configPath))
            {
                Console.WriteLine("Project is not configured. Run 'setup' first.");
                return false;
            }

            string json = File.ReadAllText(_configPath);

            var savedContext = JsonSerializer.Deserialize<ProjectContext>(json);

            if (savedContext == null)
            {
                Console.WriteLine("Could not load project configuration.");
                return false;
            }

            _projectContext.SetProject(
                savedContext.ProjectDirectory!,
                savedContext.BaseFilePath!,
                savedContext.TargetFilePath!,
                savedContext.TargetLanguage!,
                savedContext.ResxFiles);
            return true;
        }

        public void SaveToJSON()
        {
            string json = JsonSerializer.Serialize(
            _projectContext,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_configPath, json);
        }
    }
}
