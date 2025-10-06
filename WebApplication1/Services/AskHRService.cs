using Microsoft.Extensions.Logging;
using policyBot.Repository;

namespace policyBot.Services
{
    public class AskHRService : IAskHRService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorDB _vectorDb;
        private readonly IllmaService _llmService;
        private readonly ILogger<AskHRService> _logger;

        public AskHRService(IEmbeddingService embeddingService, IVectorDB vectorDb, IllmaService llmService, ILogger<AskHRService> logger)
        {
            _embeddingService = embeddingService;
            _vectorDb = vectorDb;
            _llmService = llmService;
            _logger = logger;
        }

        public async Task<string> GetReplyAsync(string question)
        {
            _logger.LogInformation("GetReplyAsync called with question: {Question}", question);
            
            // Step 1: Get embedding of user query
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(question);
            // Step 2: Search in Qdrant
            var searchResults = await _vectorDb.SearchAsync(queryEmbedding);
            if (searchResults == null || searchResults.Count == 0)
            {
                // No results at all → chit-chat
                _logger.LogInformation("No search results found, falling back to chit-chat.");
                return await _llmService.GetAnswerAsync(question);
            }
            // Step 3: Evaluate top score


            var topResult = searchResults.OrderByDescending(r => r.Score).FirstOrDefault();
            if (topResult == null)
            {
                // No results at all → chit-chat
                _logger.LogInformation("No top result found, falling back to chit-chat.");
                return await _llmService.GetAnswerAsync(question);
            }
            double threshold = 0.56; // tweak based on testing

            if (topResult.Score < threshold)
            {
                // Looks like chit-chat → no knowledge match
                _logger.LogInformation("Top result score below threshold, falling back to chit-chat. Score: {Score}", topResult.Score);
                return await _llmService.GetAnswerAsync(question);
            }

            // Step 4: Knowledge query → build context from retrieved docs
            var retrievedChunks = new List<string> { topResult.Payload["text"].ToString() };
            // var retrievedChunks = searchResults
            //     .Select(r => r.Payload["text"].ToString())
            //     .ToList();

            _logger.LogInformation("Providing answer from knowledge with top result score: {Score}", topResult.Score);

            return await _llmService.GetAnswerAsync(question, retrievedChunks);

        }
    }
}