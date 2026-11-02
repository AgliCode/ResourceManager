
namespace ResourceManager.TranslationApi.Models;

public class TranslationResponse
{
    public List<TranslationResult> Translations { get; set; } = new();
}

public class TranslationResult
{
    public string Key { get; set; } = string.Empty;

    public string EnglishText { get; set; } = string.Empty;
}
