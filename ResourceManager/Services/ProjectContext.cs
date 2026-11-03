namespace ResourceManager.Services;

public class ProjectContext
{
    public string? ProjectDirectory { get; private set; }

    public string? PolishFilePath { get; private set; }

    public string? EnglishFilePath { get; private set; }

    public bool IsProjectLoaded =>
        !string.IsNullOrWhiteSpace(ProjectDirectory);

    public void SetProject(string directory)
    {
        ProjectDirectory = directory;

        PolishFilePath = System.IO.Path.Combine(
            directory, "Resources.resx");

        EnglishFilePath = System.IO.Path.Combine(
            directory, "Resources.en-US.resx");
    }
}