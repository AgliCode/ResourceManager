using ResourceManager.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResourceManager.TranslationApi.Services;

namespace ResourceManager.CLI.Helpers
{
    public class ConsoleHelper
    {
        public string ReadSecret()
        {
            var password = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Length--;
                        Console.Write("\b \b");
                    }

                    continue;
                }

                if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }

            return password.ToString();
        }
        public int SelectResxFile(string message, string[] files)
        {
            while (true)
            {
                Console.WriteLine(message);
                Console.Write("> ");

                string? input = Console.ReadLine();

                if (int.TryParse(input, out int selection) &&
                    selection >= 1 &&
                    selection <= files.Length)
                {
                    return selection - 1;
                }

                Console.WriteLine("Invalid selection. Try again.");
            }
        }

        public (string resourcesDirectory, string[] resxFiles) SelectProjectDirectory()
        {
            string resourcesDirectory = string.Empty;
            while (true)
            {
                Console.Write("Enter project directory: ");
                resourcesDirectory = Console.ReadLine()?.Trim().Trim('"') ?? "";

                if (Directory.Exists(resourcesDirectory))
                    break;

                Console.WriteLine("Directory does not exist. Try again.");
            }
            string[] resxFiles = Directory.GetFiles(
            resourcesDirectory,
            "*.resx",
            SearchOption.TopDirectoryOnly);

            return (resourcesDirectory, resxFiles);
        
        }

        public string SelectBaseFile(string[] resxFiles)
        {
            if (resxFiles.Length == 0)
            {
                Console.WriteLine("No RESX files found in this directory.");
                return string.Empty;
            }

            Console.WriteLine();
            Console.WriteLine("Available RESX files:");

            for (int i = 0; i < resxFiles.Length; i++)
            {
                Console.WriteLine($"  {i + 1}. {Path.GetFileName(resxFiles[i])}");
            }

            Console.WriteLine();

            int baseIndex = SelectResxFile("Select BASE RESX file:", resxFiles);

            return resxFiles[baseIndex];
        }

        public string SelectTargetFile(string[] resxFiles, string baseFilePath)
        {
            if (resxFiles.Length == 1)
            {
                return baseFilePath;
            }

            Console.WriteLine();
            Console.WriteLine("Available RESX files:");

            for (int i = 0; i < resxFiles.Length; i++)
            {
                Console.WriteLine($"  {i + 1}. {Path.GetFileName(resxFiles[i])}");
            }

            Console.WriteLine();

            int targetIndex = SelectResxFile("Select TARGET RESX file:", resxFiles);

            return resxFiles[targetIndex];
        }

        public string SelectLanguage()
        {
            var languageService = new LanguageService();
            var languages = languageService.GetAvailableLanguages();

            string targetLanguage = string.Empty;

            while (string.IsNullOrWhiteSpace(targetLanguage))
            {
                Console.WriteLine();
                Console.WriteLine("Target language");
                Console.Write("Search by language name or code (e.g. English or en-US): ");

                string search = Console.ReadLine()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(search))
                {
                    Console.WriteLine("Please enter a language name or code.");
                    continue;
                }

                var matchingLanguages = languages
                    .Where(language =>
                        language.EnglishName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        language.NativeName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        language.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .Take(100)
                    .ToList();

                if (matchingLanguages.Count == 0)
                {
                    Console.WriteLine("No matching languages found. Try again.");
                    continue;
                }

                Console.WriteLine();
                Console.WriteLine("Matching languages:");

                for (int i = 0; i < matchingLanguages.Count; i++)
                {
                    Console.WriteLine(
                        $"  {i + 1}. {matchingLanguages[i].EnglishName} ({matchingLanguages[i].Name})");
                }

                Console.WriteLine();
                Console.Write("Select language: ");

                if (int.TryParse(Console.ReadLine(), out int selection) &&
                    selection >= 1 &&
                    selection <= matchingLanguages.Count)
                {
                    targetLanguage = matchingLanguages[selection - 1].Name;
                }
                else
                {
                    Console.WriteLine("Invalid selection. Try again.");
                }
            }
            return targetLanguage;
        }

        public void ConfigureGeminiKey(ApiKeyStorageService apiKeyStorageService, GeminiKeyStorage geminiKeyStorage)
        {
            Console.WriteLine();
            Console.WriteLine("Enter your Gemini API key:");
            string apiKey = ReadSecret();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("API key cannot be empty.");
                return;
            }

            Console.WriteLine();
            Console.Write("Remember API key on this computer? (y/n): ");
            bool rememberKey = Console.ReadLine()?.Trim().ToLowerInvariant() == "y";

            if (rememberKey)
            {
                apiKeyStorageService.Save(apiKey);
                geminiKeyStorage.Set(string.Empty, true);
            }
            else
            {
                apiKeyStorageService.Delete();
                geminiKeyStorage.Set(apiKey, false);
            }
        }
    }
}
