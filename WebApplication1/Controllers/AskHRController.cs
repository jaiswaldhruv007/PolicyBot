namespace policyBot.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.IO;
    using System.Collections.Generic;

    using policyBot.Services;
    [ApiController]
    [Route("api/[controller]")]
    public class AskHRController : ControllerBase
    {
        private readonly IAskHRService _askHRService;
        private readonly ILogger<AskHRController> _logger;

        public AskHRController(IAskHRService askHRService, ILogger<AskHRController> logger)
        {
            _askHRService = askHRService;
            _logger = logger;
        }

        [HttpGet("GetReply")]
        public async Task<IActionResult> GetReply(string question)
        {
            _logger.LogInformation("GetReply called with question: {Question}", question);
            var reply = await _askHRService.GetReplyAsync(question);
            return Ok(reply);
        }
    }
}