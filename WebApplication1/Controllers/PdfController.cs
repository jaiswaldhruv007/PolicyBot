namespace policyBot.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    using policyBot.Services;

    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfReaderService _pdfReader;

        public PdfController(PdfReaderService pdfReader)
        {
            _pdfReader = pdfReader;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPdf(IFormFile file, int chunkSize = 500, int overlap = 50)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using var stream = file.OpenReadStream();
            string text = _pdfReader.ExtractText(stream);

            var chunks = TextChunker.ChunkText(text, chunkSize, overlap);

            return Ok(new
            {
                FileName = file.FileName,
                TotalChunks = chunks.Count,
                Chunks = chunks
            });
        }
    }
}
