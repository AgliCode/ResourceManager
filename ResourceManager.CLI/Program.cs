using ResourceManager.Core.Models;
using ResourceManager.Core.Services;
using ResourceManager.TranslationApi.Services;
using System.Net.NetworkInformation;
using System.Resources;
using System.Text;
using ResourceReader = ResourceManager.Core.Services.ResourceReader;
using System.Text.Json;
using System.Text.Json.Serialization;
using ResourceManager.CLI.Helpers;
using ResourceManager.CLI.Configuration;


var projectContext = new ProjectContext();
var geminiKeyStorage = new GeminiKeyStorage();
var apiKeyStorageService = new ApiKeyStorageService();
var translationApiClient = new TranslationApiClient();
var reader = new ResourceReader();
var consoleHelper = new ConsoleHelper();
var configurationManager = new ProjectConfigurationManager(projectContext);

string? savedApiKey = apiKeyStorageService.Load();

if (!string.IsNullOrWhiteSpace(savedApiKey))
{
    geminiKeyStorage.Set(savedApiKey, false);
}

configurationManager.LoadProjectConfiguration();

Console.WriteLine("Core services loaded successfully!");

using var apiProcessManager = new ApiProcessManager();

try
{
    string apiDirectory = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "API"));

    await apiProcessManager.StartAsync(apiDirectory);
}
catch (Exception ex)
{
    Console.WriteLine($"Could not start the API: {ex.Message}");
    return;
}

if (args.Length == 0)
{
    ShowHelp();
    return;
}

string command = args[0].ToLowerInvariant();

switch (command)
{
    case "help":
    case "--help":
    case "-h":
        ShowHelp();
        break;

    case "setup":
        Setup();
        break;

    case "missing":
        bool aiTranslate = args.Length > 1 && args[1].Equals("--translate", StringComparison.OrdinalIgnoreCase);
        await Missing(aiTranslate);
        break;

    case "configure":
        Configure();
        break;

    case "status":
        Status();
        break;


    default:
        Console.WriteLine($"Unknown command: {command}");
        Console.WriteLine("Use 'help' to see available commands.");
        break;
}

async Task Missing(bool AITranslate)
{
    configurationManager.LoadProjectConfiguration();

    if (!projectContext.IsProjectLoaded)
    {
        Console.WriteLine("Project is not configured. Run 'setup' first.");
        return;
    }

    if (string.IsNullOrWhiteSpace(projectContext.BaseFilePath) ||
        string.IsNullOrWhiteSpace(projectContext.TargetFilePath))
    {
        Console.WriteLine("Project configuration is incomplete.");
        return;
    }

    List<ResourceEntry> missingTranslations = reader.FindMissingTranslations(
        projectContext.BaseFilePath,
        projectContext.TargetFilePath);

    Dictionary<string, string> aiTranslations = new();

    Console.WriteLine("====================================");
    Console.WriteLine("MISSING TRANSLATIONS");
    Console.WriteLine("====================================");
    Console.WriteLine();
    Console.WriteLine($"Base:   {Path.GetFileName(projectContext.BaseFilePath)}");
    Console.WriteLine($"Target: {Path.GetFileName(projectContext.TargetFilePath)}");
    Console.WriteLine($"Missing translations: {missingTranslations.Count}");
    Console.WriteLine();

    if (AITranslate && missingTranslations.Count > 0)
    {
        string? apiKey = geminiKeyStorage.ReadApiKey();

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("Gemini API key is not saved on this computer.");
            consoleHelper.ConfigureGeminiKey(apiKeyStorageService, geminiKeyStorage);
        }

        const int batchSize = 300;

            try
            {
                var itemsToTranslate = missingTranslations
                    .Select(resource => new TranslationApiClient.TranslationApiItem
                    {
                        Key = resource.Name,
                        BaseText = resource.Value,
                        Comment = resource.Comment
                    })
                    .ToList();

                for (int j = 0; j < itemsToTranslate.Count; j += batchSize)
                {
                    var batch = itemsToTranslate
                        .Skip(j)
                        .Take(batchSize)
                        .ToList();

                    Console.WriteLine(
                        $"Translating batch {j / batchSize + 1}...");

                    var batchTranslations = await translationApiClient.TranslateAsync(
                        batch,
                        apiKey,
                        projectContext.TargetLanguage!);

                    foreach (var translation in batchTranslations)
                    {
                        aiTranslations[translation.Key] = translation.Value;
                    }

                    if (j + batchSize < itemsToTranslate.Count)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Translation error: {ex.Message}");
            }
        
    }

    int i = 1;

    foreach (var entry in missingTranslations)
    {
        Console.WriteLine($"{i}. Name: {entry.Name}");
        Console.WriteLine($"   Value: {entry.Value}");

        if (AITranslate)
        {
            if (aiTranslations.TryGetValue(entry.Name, out string? translation))
            {
                Console.WriteLine($"   Translate: {translation}");
            }
            else
            {
                Console.WriteLine("   Translate: Translation not received.");
            }
        }

        Console.WriteLine();
        i++;
    }
    if (AITranslate && aiTranslations.Count > 0)
    {
        Console.WriteLine("How would you like to approve translations?");
        Console.WriteLine();
        Console.WriteLine("1. Approve all translations");
        Console.WriteLine("2. Review translations individually");
        Console.WriteLine("3. Cancel");
        Console.WriteLine();

        Console.Write("Select an option: ");

        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                try
                {
                    ResxWriter.SaveTranslationToResx(
                        aiTranslations,
                        projectContext.TargetFilePath);

                    Console.WriteLine();
                    Console.WriteLine($"Successfully saved {aiTranslations.Count} translations.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Failed to save translations: {ex.Message}");
                }
                break;

            case "2":
                Dictionary<string, string> approvedTranslations = new();

                foreach (var entry in missingTranslations)
                {
                    if (!aiTranslations.TryGetValue(entry.Name, out string? translation))
                    {
                        continue;
                    }

                    Console.WriteLine();
                    Console.WriteLine("------------------------------------");
                    Console.WriteLine($"Name: {entry.Name}");
                    Console.WriteLine($"Original: {entry.Value}");
                    Console.WriteLine($"Translation: {translation}");
                    Console.WriteLine("------------------------------------");
                    Console.WriteLine();

                    Console.Write("Approve this translation? (y/n): ");

                    string? approval = Console.ReadLine()?.Trim().ToLowerInvariant();

                    if (approval == "y")
                    {
                        approvedTranslations[entry.Name] = translation;
                        Console.WriteLine("Translation approved.");
                    }
                    else
                    {
                        Console.WriteLine("Translation skipped.");
                    }
                }

                if (approvedTranslations.Count > 0)
                {
                    try
                    {
                        ResxWriter.SaveTranslationToResx(
                            approvedTranslations,
                            projectContext.TargetFilePath);

                        Console.WriteLine();
                        Console.WriteLine(
                            $"Successfully saved {approvedTranslations.Count} translations.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Failed to save translations: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("No translations were approved. Nothing was saved.");
                }

                break;

            case "3":
                Console.WriteLine("Translation approval cancelled.");
                break;

            default:
                Console.WriteLine("Invalid option. Please run the command again.");
                break;
        }
    }
}

void Setup()
{
    Console.WriteLine();
    Console.WriteLine("====================================");
    Console.WriteLine("       RESOURCE MANAGER CLI");
    Console.WriteLine("====================================");
    Console.WriteLine();
    Console.WriteLine("Welcome!");
    Console.WriteLine();

    Console.WriteLine("Gemini API configuration");
    Console.Write("Do you want to configure your Gemini API key? (y/n): ");
    string? answer = Console.ReadLine()?.Trim().ToLowerInvariant();

    if (answer == "y")
    {
        consoleHelper.ConfigureGeminiKey(apiKeyStorageService, geminiKeyStorage);
    }


    Console.WriteLine();
    Console.WriteLine("Project configuration");
    Console.WriteLine();

    var (resourcesDirectory, resxFiles) = consoleHelper.SelectProjectDirectory();

    string baseFilePath = consoleHelper.SelectBaseFile(resxFiles);

    string targetFilePath = consoleHelper.SelectTargetFile(resxFiles, baseFilePath);

    string targetLanguage = consoleHelper.SelectLanguage();

    projectContext.SetProject(resourcesDirectory, baseFilePath, targetFilePath, targetLanguage, resxFiles);
    configurationManager.SaveToJSON();


}

void Configure()
{
    configurationManager.LoadProjectConfiguration();

    if (!projectContext.IsProjectLoaded)
    {
        Console.WriteLine("Project is not configured. Run 'setup' first.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("====================================");
    Console.WriteLine("       PROJECT CONFIGURATION");
    Console.WriteLine("====================================");
    Console.WriteLine();
    Console.WriteLine("What would you like to configure?");
    Console.WriteLine();
    Console.WriteLine("1. Project directory");
    Console.WriteLine("2. Base RESX file");
    Console.WriteLine("3. Target RESX file");
    Console.WriteLine("4. Target language");
    Console.WriteLine("5. Gemini API key");
    Console.WriteLine("6. Back");
    Console.WriteLine();

    Console.Write("Select an option: ");

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            var (resourcesDirectory, resxFiles) = consoleHelper.SelectProjectDirectory();

            string baseFilePath = consoleHelper.SelectBaseFile(resxFiles);

            string targetFilePath = consoleHelper.SelectTargetFile(resxFiles, baseFilePath);

            projectContext.SetProject(
                resourcesDirectory, 
                baseFilePath, 
                targetFilePath, 
                projectContext.TargetLanguage!, 
                resxFiles);
            configurationManager.SaveToJSON();
            break;

        case "2":
            string newBaseFilePath = consoleHelper.SelectBaseFile(projectContext.ResxFiles);

            projectContext.SetProject(
                projectContext.ProjectDirectory!, 
                newBaseFilePath, 
                projectContext.TargetFilePath!, 
                projectContext.TargetLanguage!, 
                projectContext.ResxFiles);
            configurationManager.SaveToJSON();
            break;

        case "3":
            string newTargetFilePath = consoleHelper.SelectTargetFile(
                projectContext.ResxFiles, 
                projectContext.BaseFilePath!);

            projectContext.SetProject(
                projectContext.ProjectDirectory!, 
                projectContext.BaseFilePath!, 
                newTargetFilePath, 
                projectContext.TargetLanguage!, 
                projectContext.ResxFiles);
            configurationManager.SaveToJSON();
            break;

        case "4":
            string newTargetLanguage = consoleHelper.SelectLanguage();
            projectContext.SetProject(
                projectContext.ProjectDirectory!,
                projectContext.BaseFilePath!,
                projectContext.TargetFilePath!,
                newTargetLanguage,
                projectContext.ResxFiles);
            configurationManager.SaveToJSON();
            break;

        case "5":
            consoleHelper.ConfigureGeminiKey(apiKeyStorageService, geminiKeyStorage);
            break;

        case "6":
            return;

        default:
            Console.WriteLine("Invalid option.");
            break;
    }
}

void Status()
{
    List<ResourceEntry> allKeys = reader.ReadResources(projectContext.BaseFilePath!);
    List<ResourceEntry> missingTranslations = reader.FindMissingTranslations(projectContext.BaseFilePath!, projectContext.TargetFilePath!);
    bool isGeminiConfigured = !string.IsNullOrWhiteSpace(geminiKeyStorage.ReadApiKey());
    Console.WriteLine();
    Console.WriteLine("====================================");
    Console.WriteLine("       PROJECT STATUS");
    Console.WriteLine("====================================");
    Console.WriteLine();
    Console.WriteLine($"Project Directory:   {projectContext.ProjectDirectory}");
    Console.WriteLine($"Base file:   {Path.GetFileName(projectContext.BaseFilePath)}");
    Console.WriteLine($"Target file: {Path.GetFileName(projectContext.TargetFilePath)}");
    Console.WriteLine($"All keys: {allKeys.Count}");
    Console.WriteLine($"Missing translations: {missingTranslations.Count}");
    Console.WriteLine($"Translated keys: {allKeys.Count - missingTranslations.Count}");
    Console.WriteLine($"Gemini API key configured: {(isGeminiConfigured ? "Yes" : "No")}");
    Console.WriteLine($"Gemini API key saved locally: {(geminiKeyStorage.SaveApiKeyLocally ? "Yes" : "No")}");
}
    static void ShowHelp()
{
    Console.WriteLine();
    Console.WriteLine("RESOURCE MANAGER CLI");
    Console.WriteLine("--------------------");
    Console.WriteLine();
    Console.WriteLine("Available commands:");
    Console.WriteLine();
    Console.WriteLine("  help       Displays available commands.");
    Console.WriteLine("  setup                 Configure a project.");
    Console.WriteLine("  configure             Change project settings.");
    Console.WriteLine("  status             Display project configuration and file status.");
    Console.WriteLine("  missing               Display missing translations.");
    Console.WriteLine("  missing --translate   Translate missing entries using AI.");

}




