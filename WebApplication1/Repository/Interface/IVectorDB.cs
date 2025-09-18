using Qdrant.Client.Grpc;

namespace policyBot.Repository
{
    public interface IVectorDB
    {
        Task SaveAsync(string fileName, List<string> chunks, List<List<float>> embeddings);
        Task<List<object>> GetChunksAsync(string fileName = null);
        Task<IReadOnlyList<ScoredPoint>> SearchAsync(List<float> queryVector, ulong limit = 5);
        Task CreateCollectionIfNotExistsAsync();
    }
}