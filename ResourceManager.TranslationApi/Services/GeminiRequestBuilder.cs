using System.Text.Json;


namespace ResourceManager.TranslationApi.Services
{
    public class GeminiRequestBuilder()
    {
        public string Build(string prompt)
        {
            var body = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
                generationConfig = new
                {
                    responseMimeType = "application/json",
                    temperature = 0.2
                }
            };
            string jsonBody = JsonSerializer.Serialize(body);

            return jsonBody;

        }

    }
}
