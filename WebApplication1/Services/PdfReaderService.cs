namespace policyBot.Services
{
    using System.Text;
    using Microsoft.Extensions.Logging;
    using UglyToad.PdfPig;

    public class PdfReaderService
    {
        private readonly ILogger<PdfReaderService> _logger;

        public PdfReaderService(ILogger<PdfReaderService> logger)
        {
            _logger = logger;
        }

        public string ExtractText(Stream pdfStream)
        {
            var text = new StringBuilder();
            try
            {
                _logger.LogInformation("PDF text extraction started.");
                using var document = PdfDocument.Open(pdfStream);
                foreach (var page in document.GetPages())
                {
                    text.AppendLine(page.Text);
                }
                _logger.LogInformation("PDF text extraction completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF.");
                throw;
            }
            return text.ToString();
        }
    }
}