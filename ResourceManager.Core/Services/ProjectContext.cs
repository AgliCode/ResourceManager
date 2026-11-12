using System.Text.Json.Serialization;

namespace ResourceManager.Core.Services;

public class ProjectContext
{
    [JsonInclude] public string? ProjectDirectory { get; private set; }

    [JsonInclude] public string? BaseFilePath { get; private set; }

    [JsonInclude] public string? TargetFilePath { get; private set; }
    [JsonInclude] public string? TargetLanguage { get; private set; }
    [JsonInclude] public string[] ResxFiles { get; private set; } = Array.Empty<string>();


    public bool IsProjectLoaded => !string.IsNullOrWhiteSpace(ProjectDirectory);

    public bool HasSeparateTargetFile => !string.Equals(BaseFilePath, TargetFilePath, StringComparison.OrdinalIgnoreCase);
    public void SetProject(string directory, string baseFilePath, string targetFilePath, string targetLanguage, string[] resxFiles)
    {
        ProjectDirectory = directory;

        BaseFilePath = baseFilePath;

        TargetFilePath = targetFilePath;

        TargetLanguage = targetLanguage;

        ResxFiles = resxFiles;

    }

    
}