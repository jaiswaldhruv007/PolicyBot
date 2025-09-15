namespace policyBot.Services
{
    public interface IEmbeddingService
    {
        Task<List<List<float>>> GetEmbeddingsAsync(List<string> chunks);
    }
}