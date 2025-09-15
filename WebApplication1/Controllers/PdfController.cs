namespace policyBot.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.IO;
    using System.Collections.Generic;

    using policyBot.Services;

    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfReaderService _pdfReader;
        private readonly IEmbeddingService _embeddingService;

        public PdfController(PdfReaderService pdfReader, IEmbeddingService embeddingService)
        {
            _pdfReader = pdfReader;
            _embeddingService = embeddingService;
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

        [HttpGet("all")]
        public async Task<IActionResult> GetAllPdfChunks(int chunkSize = 500, int overlap = 50)
        {
            var pdfDirectory = Path.Combine(Directory.GetCurrentDirectory(), "", "pdf");
            if (!Directory.Exists(pdfDirectory))
            {
                return NotFound("PDF directory not found.");
            }

            var result = new List<object>();
            var pdfFiles = Directory.GetFiles(pdfDirectory, "*.pdf");

            foreach (var pdfPath in pdfFiles)
            {
                using var stream = System.IO.File.OpenRead(pdfPath);
                string text = _pdfReader.ExtractText(stream);
                var chunks = TextChunker.ChunkText(text, chunkSize, overlap);
                var embeddings = await _embeddingService.GetEmbeddingsAsync(chunks);

                result.Add(new
                {
                    FileName = Path.GetFileName(pdfPath),
                    TotalChunks = chunks.Count,
                    Chunks = chunks,
                    Embeddings = embeddings
                });
            }

            return Ok(result);
        }
    }
}
