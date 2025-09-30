namespace policyBot.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.IO;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using policyBot.Services;
    using Qdrant.Client.Grpc;
    using System.Linq;
    using policyBot.Repository;

    [ApiController]
    [Route("api/[controller]")]
    public class VectorDBController : ControllerBase
    {
        private readonly IVectorDB _vectorDb;
        public VectorDBController(IVectorDB vectorDb)
        {
            _vectorDb = vectorDb;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            await _vectorDb.DeleteAllAsync();
            return Ok("Deleted successfully");
        }
    }
}