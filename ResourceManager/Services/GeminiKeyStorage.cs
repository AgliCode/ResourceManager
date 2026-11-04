namespace ResourceManager.TranslationApi.Services
{
    public class GeminiKeyStorage
    {
        public string? ApiKey { get; private set; }

        public bool HasApiKey =>
            !string.IsNullOrWhiteSpace(ApiKey);

        public void Set(string apiKey)
        {
            ApiKey = string.IsNullOrWhiteSpace(apiKey)
                ? null
                : apiKey.Trim();
        }

        public void Clear()
        {
            ApiKey = null;
        }
    }
}
