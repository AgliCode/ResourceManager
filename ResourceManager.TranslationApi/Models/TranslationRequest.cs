namespace ResourceManager.TranslationApi.Models;

public class TranslationRequest
{
    public List<TranslationItem> Items { get; set; } = new();
}

public class TranslationItem
{
    public string Key { get; set; } = string.Empty;

    public string PolishText { get; set; } = string.Empty;

    public string? Comment { get; set; }
}