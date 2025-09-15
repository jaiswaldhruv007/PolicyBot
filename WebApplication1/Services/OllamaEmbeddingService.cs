namespace policyBot.Services
{
    using System.Net.Http;
    using System.Text;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using policyBot.Configuration;

    public class OllamaEmbedResponse
    {
        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("embeddings")]
        public List<List<float>> Embeddings { get; set; }

        [JsonProperty("total_duration")]
        public long TotalDuration { get; set; }

        [JsonProperty("load_duration")]
        public long LoadDuration { get; set; }

        [JsonProperty("prompt_eval_count")]
        public int PromptEvalCount { get; set; }
    }

    public class OllamaEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly EmbeddingSettings _settings;

        public OllamaEmbeddingService(IHttpClientFactory factory, IOptions<EmbeddingSettings> settings)
        {
            _httpClient = factory.CreateClient();
            _settings = settings.Value;
        }

        public async Task<List<List<float>>> GetEmbeddingsAsync(List<string> chunks)
        {
            if (chunks == null || chunks.Count == 0)
                return new List<List<float>>();

            // Prepare request body
            var requestBody = new
            {
                model = _settings.Model,
                input = chunks
            };

            var json = JsonConvert.SerializeObject(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send request with cancellation support
            using var response = await _httpClient.PostAsync(_settings.BaseUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Ollama API returned {response.StatusCode}: {errorContent}");
            }

            var responseString = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<OllamaEmbedResponse>(responseString);

            return result?.Embeddings ?? new List<List<float>>();
        }
    }
}