namespace policyBot.Services
{
    public interface IAskHRService
    {
        Task<string> GetReplyAsync(string question);
    }
}