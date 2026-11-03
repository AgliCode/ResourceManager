using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace ResourceManager.Services;

public class TranslationApiClient
{
    private readonly HttpClient _httpClient;
    public class TranslationApiRequest
    {
        public List<TranslationApiItem> Items { get; set; } = new();
        public string TargetLanguage { get; set; } = "en-US";
    }

    public class TranslationApiItem
    {
        public string Key { get; set; } = string.Empty;
        public string PolishText { get; set; } = string.Empty;
        public string? Comment { get; set; }
    }

    public class TranslationApiResponse
    {
        public List<TranslationApiResult> Translations { get; set; } = new();
    }

    public class TranslationApiResult
    {
        public string Key { get; set; } = string.Empty;
        public string TranslatedText { get; set; } = string.Empty;
    }

    public TranslationApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7221/")
        };
    }
    public async Task<Dictionary<string, string>> TranslateAsync(
    List<TranslationApiItem> items,
     string targetLanguage = "en-US")
    {
        var request = new TranslationApiRequest
        {
            TargetLanguage = targetLanguage,
            Items = items
        };

        var response = await _httpClient.PostAsJsonAsync(
            "api/Translation",
            request);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TranslationApiResponse>();

        if (result == null)
            throw new Exception("Failed to read the API response.");

        return result.Translations.ToDictionary(
            item => item.Key,
            item => item.TranslatedText);
    }
}