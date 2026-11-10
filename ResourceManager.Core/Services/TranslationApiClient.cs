using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace ResourceManager.Core.Services;

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
            BaseAddress = new Uri("http://127.0.0.1:5177/")
        };
    }
    public async Task<Dictionary<string, string>> TranslateAsync(
     List<TranslationApiItem> items,
     string apiKey,
     string targetLanguage = "en-US")
    {
        var request = new TranslationApiRequest
        {
            TargetLanguage = targetLanguage,
            Items = items
        };

        using var httpRequest = new HttpRequestMessage(
        HttpMethod.Post,
        "api/Translation");

        httpRequest.Headers.Add("x-goog-api-key", apiKey);
        httpRequest.Content = JsonContent.Create(request);

        using var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TranslationApiResponse>();

        if (result == null)
            throw new Exception("Failed to read the API response.");

        return result.Translations.ToDictionary(
            item => item.Key,
            item => item.TranslatedText);
    }
}