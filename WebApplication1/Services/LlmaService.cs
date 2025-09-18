

namespace policyBot.Services
{
    using System.Net.Http;
    using System.Text;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using policyBot.Configuration;


    public class OllamaChatResponse
    {
        [JsonProperty("message")]
        public ChatMessage Message { get; set; }
    }

    public class ChatMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class LlmaService : IllmaService
    {
        private readonly LlmSettings _llmSettings;
        private readonly HttpClient _httpClient;

        public LlmaService(IOptions<LlmSettings> llmSettings, IHttpClientFactory factory)
        {
            _llmSettings = llmSettings.Value;
            _httpClient = factory.CreateClient();
        }

        public async Task<string> GetAnswerAsync(string question, List<string> retrievedChunks)
        {
            var context = string.Join("\n\n", retrievedChunks);

            var requestBody = new
            {
                model = _llmSettings.Model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant that answers based on context." },
                    new { role = "user", content = $"Context:\n{context}\n\nQuestion:\n{question}" }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_llmSettings.BaseUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Ollama API returned {response.StatusCode}: {errorContent}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<OllamaChatResponse>(responseString);

            return result?.Message?.Content ?? string.Empty;
        }

        public async Task<string> GetAnswerAsync(string question)
        {
            var requestBody = new
            {
                model = _llmSettings.Model, // good for general chit-chat
                messages = new[]
                {
                    new { role = "system", content = "You are a friendly AI assistant. Keep responses short and conversational." },
                    new { role = "user", content = question }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_llmSettings.BaseUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Ollama API returned {response.StatusCode}: {errorContent}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            return ParseOllamaChatResponse(responseString);
        }
        private string ParseOllamaChatResponse(string rawResponse)
        {
            var stringBuilder = new StringBuilder();

            using (var reader = new StringReader(rawResponse))
            using (var jsonReader = new JsonTextReader(reader) { SupportMultipleContent = true })
            {
                var serializer = new JsonSerializer();

                while (jsonReader.Read())
                {
                    var obj = serializer.Deserialize<OllamaChatResponse>(jsonReader);
                    if (obj?.Message?.Content != null)
                    {
                        stringBuilder.Append(obj.Message.Content);
                    }
                }
            }

            return stringBuilder.ToString();
        }
    }
}
