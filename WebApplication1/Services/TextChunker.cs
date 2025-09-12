namespace policyBot.Services
{
    public static class TextChunker
    {
        public static List<string> ChunkText(string text, int chunkSize = 500, int overlap = 50)
        {
            var chunks = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return chunks;

            int start = 0;
            while (start < text.Length)
            {
                int length = Math.Min(chunkSize, text.Length - start);
                chunks.Add(text.Substring(start, length));
                start += (chunkSize - overlap); // move forward with overlap
            }

            return chunks;
        }
    }
}
