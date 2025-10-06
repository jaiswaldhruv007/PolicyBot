

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
            string systemPrompt = @"You are a helpful HR assistant named AskHR. Only answer questions using the provided context.
            Do not make up answers.
            If the answer is not present in the context, respond exactly: ""I could not find this in the HR policies.""
            Answer clearly and concisely.";
            var requestBody = new
            {
                model = _llmSettings.Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
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
            return ParseOllamaChatResponse(responseString);
        }

        public async Task<string> GetAnswerAsync(string question)
        {
var requestBody = new
{
    model = _llmSettings.Model, // e.g. "gpt-5"
    messages = new[]
    {
        new {
            role = "system",
            content = "You are AskHR, a concise HR chatbot. " +
                      "If the user's question is general chit-chat, reply normally in ONE short sentence. " +
                      "If the user's question is not chit-chat or unrelated to HR, reply exactly: \"I could not find this in the HR policies.\""
        },
        new {
            role = "system",
            content = "IMPORTANT: Your reply must be exactly ONE sentence. Do not write more than one sentence. Do not include emojis or follow-up questions."
        },
        new { role = "user", content = question }
    },
    max_tokens = 50,
    temperature = 0.2
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
