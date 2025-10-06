namespace policyBot.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System.IO;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using policyBot.Services;
    using Qdrant.Client.Grpc;
    using System.Linq;
    using policyBot.Repository;

    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfReaderService _pdfReader;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorDB _vectorDb;
        private readonly ILogger<PdfController> _logger;

        public PdfController(PdfReaderService pdfReader, IEmbeddingService embeddingService, IVectorDB vectorDb, ILogger<PdfController> logger)
        {
            _pdfReader = pdfReader;
            _embeddingService = embeddingService;
            _vectorDb = vectorDb;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPdf(IFormFile file, int chunkSize = 500, int overlap = 50)
        {
            _logger.LogInformation("UploadPdf called for file: {FileName}", file?.FileName);
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("No file uploaded.");
                return BadRequest("No file uploaded.");
            }

            using var stream = file.OpenReadStream();
            string text = _pdfReader.ExtractText(stream);

            var chunks = TextChunker.ChunkText(text, chunkSize, overlap);

            _logger.LogInformation("PDF {FileName} chunked into {ChunkCount} chunks.", file.FileName, chunks.Count);
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
            _logger.LogInformation("GetAllPdfChunks called.");
            var pdfDirectory = Path.Combine(Directory.GetCurrentDirectory(), "", "pdfs");
            if (!Directory.Exists(pdfDirectory))
            {
                _logger.LogWarning("PDF directory not found: {PdfDirectory}", pdfDirectory);
                return NotFound("PDF directory not found.");
            }

            var result = new List<object>();
            var pdfFiles = Directory.GetFiles(pdfDirectory, "*.pdf");
            await _vectorDb.CreateCollectionIfNotExistsAsync(); // Ensure collection exists
            var tasks = pdfFiles.Select(async pdfPath =>
            {
                using var stream = System.IO.File.OpenRead(pdfPath);
                string text = _pdfReader.ExtractText(stream);
                var chunks = TextChunker.ChunkText(text, chunkSize, overlap);
                var embeddings = await _embeddingService.GetEmbeddingAsync(chunks);

                // Save embeddings to Qdrant vector DB
                await _vectorDb.SaveAsync(Path.GetFileName(pdfPath), chunks, embeddings);

                _logger.LogInformation("Processed and saved chunks for PDF: {PdfFile}", pdfPath);
                return new
                {
                    FileName = Path.GetFileName(pdfPath),
                    TotalChunks = chunks.Count,
                    Chunks = chunks,
                    SavedTo = "Qdrant collection: HR_Policies"
                };
            });

            // Run all tasks in parallel
            var results = await Task.WhenAll(tasks);

            return Ok(results);
        }

        [HttpGet("chunks")]
        public async Task<IActionResult> GetSavedChunks(string? fileName = null)
        {
            _logger.LogInformation("GetSavedChunks called for file: {FileName}", fileName);
            var chunks = await _vectorDb.GetChunksAsync(fileName);
            return Ok(chunks);
        }
    }
}
