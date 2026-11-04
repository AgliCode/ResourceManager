namespace ResourceManager.Services;

public class ProjectContext
{
    public string? ProjectDirectory { get; private set; }

    public string? PolishFilePath { get; private set; }

    public string? EnglishFilePath { get; private set; }
    public string? TargetLanguage { get; private set; }
    public string[] ResxFiles { get; private set; }

    public bool IsProjectLoaded =>
        !string.IsNullOrWhiteSpace(ProjectDirectory);

    public void SetProject(string directory, string baseFilePath, string targetFilePath, string targetLanguage, string[] resxFiles)
    {
        ProjectDirectory = directory;

        PolishFilePath = baseFilePath;

        EnglishFilePath = targetFilePath;

        TargetLanguage = targetLanguage;

        ResxFiles = resxFiles;
    }
}