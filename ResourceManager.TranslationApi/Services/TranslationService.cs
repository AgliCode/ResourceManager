
using ResourceManager.TranslationApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace ResourceManager.TranslationApi.Services;

public class TranslationService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiRequestBuilder _requestBuilder = new();
    private readonly GeminiResponseParser _responseParser = new(); 

    public TranslationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }


    public async Task<TranslationResponse> TranslateAsync(
        TranslationRequest request,
        string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("Gemini API key is missing.");

        if (request.Items.Count == 0)
            throw new ArgumentException(
                "The translation list is empty");


        string inputJson = JsonSerializer.Serialize(request.Items);

        // Prompt AI
        string prompt = $$"""
        You are a professional software localization translator.

        Translate all provided Polish resource texts into {{request.TargetLanguage}}.

        Rules:
        - Preserve every resource key exactly.
        - Translate every item.
        - Preserve placeholders such as {0}, {name} and format strings.
        - Preserve technical identifiers and markup.
        - Use natural language suitable for a software application.
        - Use comments as translation context.
        - Return only valid JSON.
        - Do not omit any resource.

        Expected JSON format:
        {
         "translations": [
          {
            "key": "ResourceKey",
            "translatedText": "translation"
          }]
        }

        Resources:
        {{inputJson}}
        """;

        string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite:generateContent";

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

        string jsonBody = _requestBuilder.Build(prompt);

        using var content = new StringContent(
                jsonBody,
                Encoding.UTF8,
                "application/json");

        httpRequest.Headers.Add("x-goog-api-key", apiKey);
        httpRequest.Content = content;

        using var response = await _httpClient.SendAsync(httpRequest);

        string responseJson = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        return _responseParser.Parse(responseJson);
    }

}
