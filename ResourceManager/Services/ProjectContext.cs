namespace ResourceManager.Services;

public class ProjectContext
{
    public string? ProjectDirectory { get; private set; }

    public string? BaseFilePath { get; private set; }

    public string? TargetFilePath { get; private set; }
    public string? TargetLanguage { get; private set; }
    public string[] ResxFiles { get; private set; }

    public bool IsProjectLoaded =>
        !string.IsNullOrWhiteSpace(ProjectDirectory);

    public void SetProject(string directory, string baseFilePath, string targetFilePath, string targetLanguage, string[] resxFiles)
    {
        ProjectDirectory = directory;

        BaseFilePath = baseFilePath;

        TargetFilePath = targetFilePath;

        TargetLanguage = targetLanguage;

        ResxFiles = resxFiles;
    }
}