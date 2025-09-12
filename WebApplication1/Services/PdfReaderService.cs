namespace policyBot.Services
{
    using System.Text;

    using UglyToad.PdfPig;

    public class PdfReaderService
    {
        public string ExtractText(Stream pdfStream)
        {
            var text = new StringBuilder();
            using var document = PdfDocument.Open(pdfStream);
            foreach (var page in document.GetPages())
            {
                text.AppendLine(page.Text);
            }
            return text.ToString();
        }
    }
}