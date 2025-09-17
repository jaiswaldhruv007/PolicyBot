namespace policyBot.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.IO;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using policyBot.Services;
    using Qdrant.Client.Grpc;
    using System.Linq;

    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfReaderService _pdfReader;
        private readonly IEmbeddingService _embeddingService;
        private readonly QdrantVectorDb _vectorDb; // Use QdrantVectorDb

        public PdfController(PdfReaderService pdfReader, IEmbeddingService embeddingService, QdrantVectorDb vectorDb)
        {
            _pdfReader = pdfReader;
            _embeddingService = embeddingService;
            _vectorDb = vectorDb;
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

                // Save embeddings to Qdrant vector DB
                await SaveEmbeddingsToVectorDbAsync(Path.GetFileName(pdfPath), chunks, embeddings);

                result.Add(new
                {
                    FileName = Path.GetFileName(pdfPath),   
                    TotalChunks = chunks.Count,
                    Chunks = chunks,
                    Embeddings = embeddings,
                    SavedTo = "Qdrant collection: pdf_chunks"
                }); 
            }

            return Ok(result);
        }

        [HttpGet("chunks")]
        public async Task<IActionResult> GetSavedChunks(string? fileName = null)
        {
            var chunks = await _vectorDb.GetChunksAsync(fileName);
            return Ok(chunks);
        }

        // Save embeddings to Qdrant vector DB
        private async Task SaveEmbeddingsToVectorDbAsync(string fileName, List<string> chunks, List<List<float>> embeddings)
        {
            await _vectorDb.SaveAsync(fileName, chunks, embeddings);
        }
    }
}
