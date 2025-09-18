namespace policyBot.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.IO;
    using System.Collections.Generic;

    using policyBot.Services;
    [ApiController]
    [Route("api/[controller]")]
    public class AskHRController : ControllerBase
    {
        private readonly IAskHRService _askHRService;
        public AskHRController(IAskHRService askHRService)
        {
            _askHRService = askHRService;
        }
        [HttpGet("GetReply")]
        public async Task<IActionResult> GetReply(string question)
        {
            var reply = await _askHRService.GetReplyAsync(question);
            return Ok(reply);
        }
    }
}