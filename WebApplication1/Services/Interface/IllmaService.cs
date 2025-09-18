
namespace policyBot.Services
{
    public interface IllmaService
    {
        Task<string> GetAnswerAsync(string question, List<string> retrievedChunks);
        Task<string> GetAnswerAsync(string question);
    }
}