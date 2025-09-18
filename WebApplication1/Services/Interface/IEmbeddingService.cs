namespace policyBot.Services
{
    public interface IEmbeddingService
    {
        Task<List<List<float>>> GetEmbeddingAsync(List<string> chunks);
        Task<List<float>> GetEmbeddingAsync(string chunk);
    }
}