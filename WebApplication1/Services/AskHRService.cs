using policyBot.Repository;

namespace policyBot.Services
{
    public class AskHRService : IAskHRService
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorDB _vectorDb;
        private readonly IllmaService _llmService;

        public AskHRService(IEmbeddingService embeddingService, IVectorDB vectorDb, IllmaService llmService)
        {
            _embeddingService = embeddingService;
            _vectorDb = vectorDb;
            _llmService = llmService;
        }

        public async Task<string> GetReplyAsync(string question)
        {
            // Step 1: Get embedding of user query
            var queryEmbedding = await _embeddingService.GetEmbeddingAsync(question);
            // Step 2: Search in Qdrant
            var searchResults = await _vectorDb.SearchAsync(queryEmbedding);
            if (searchResults == null || searchResults.Count == 0)
            {
                // No results at all → chit-chat
                return await _llmService.GetAnswerAsync(question);
            }
            // Step 3: Evaluate top score

            var topResult = searchResults[0];
            double threshold = 0.70; // tweak based on testing

            if (topResult.Score < threshold)
            {
                // Looks like chit-chat → no knowledge match
                return await _llmService.GetAnswerAsync(question);
            }

            // Step 4: Knowledge query → build context from retrieved docs
            var retrievedChunks = searchResults
                .Select(r => r.Payload["text"].ToString())
                .ToList();

            return await _llmService.GetAnswerAsync(question, retrievedChunks);

        }
    }
}