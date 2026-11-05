using ResourceManager.Services;
using System.Security.Cryptography;
using System.Text;

namespace ResourceManager.TranslationApi.Services
{
    public class GeminiKeyStorage
    {
        public string? ApiKey { get; private set; }
        public bool SaveApiKeyLocally { get; set; }

        private ApiKeyStorageService _apiKeyStorageService = new();

        public bool HasApiKey =>
            !string.IsNullOrWhiteSpace(ApiKey);

        public void Set(string apiKey, bool saveApiKeyLocally)
        {
            ApiKey = string.IsNullOrWhiteSpace(apiKey)
                ? null
                : apiKey.Trim();
            SaveApiKeyLocally = saveApiKeyLocally;
        }

        public void ClearApiKeyFromMemory()
        {
            ApiKey = null;
        }

        public void DeleteSavedApiKey()
        {
            _apiKeyStorageService.Delete();
        }

        public string? ReadApiKey()
        {
            if (SaveApiKeyLocally)
            {
                string? savedApiKey = _apiKeyStorageService.Load();

                if (!string.IsNullOrWhiteSpace(savedApiKey))
                    return savedApiKey;
            }

            return ApiKey;
        }

    }
}
