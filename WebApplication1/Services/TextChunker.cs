using System.Text.RegularExpressions;

namespace policyBot.Services
{
    public static class TextChunker
    {
        public static List<string> ChunkText(string text, int chunkSize = 500, int overlap = 50)
        {
            var chunks = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return chunks;

            // Split by whitespace (words)
            var words = Regex.Split(text, @"\s+");
            int start = 0;

            while (start < words.Length)
            {
                int end = Math.Min(start + chunkSize, words.Length);
                var chunkWords = words[start..end]; // C# 8+ range operator
                chunks.Add(string.Join(" ", chunkWords));

                // Move start by chunkSize - overlap
                start += (chunkSize - overlap);
            }

            return chunks;
        }
        // public static List<string> ChunkText(string text, int chunkSize = 500, int overlap = 50)
        // {
        //     var chunks = new List<string>();
        //     if (string.IsNullOrWhiteSpace(text)) return chunks;

        //     int start = 0;
        //     while (start < text.Length)
        //     {
        //         int length = Math.Min(chunkSize, text.Length - start);
        //         chunks.Add(text.Substring(start, length));
        //         start += (chunkSize - overlap); // move forward with overlap
        //     }

        //     return chunks;
        // }

        public class Chunk
        {
            public string ChunkId { get; set; }
            public string SectionTitle { get; set; }
            public string Content { get; set; }
            public int PageNumber { get; set; }
        }

        public static List<Chunk> ChunkDocument(string[] pages)
        {
            var chunks = new List<Chunk>();
            int chunkId = 1;

            for (int i = 0; i < pages.Length; i++)
            {
                string pageText = pages[i];
                var sectionChunks = ChunkBySectionOrParagraph(pageText, i + 1, ref chunkId);
                chunks.AddRange(sectionChunks);
            }

            return chunks;
        }

        private static List<Chunk> ChunkBySectionOrParagraph(string text, int pageNumber, ref int chunkId)
        {
            var chunks = new List<Chunk>();
            var sectionRegex = new Regex(@"(?<=##\s|\n###\s|\n####\s)([^\n]+)", RegexOptions.Multiline);
            var matches = sectionRegex.Matches(text);

            if (matches.Count > 0)
            {
                for (int i = 0; i < matches.Count; i++)
                {
                    int start = matches[i].Index;
                    int end = (i < matches.Count - 1) ? matches[i + 1].Index : text.Length;
                    string title = matches[i].Value.Trim();
                    string content = text.Substring(start, end - start).Trim();

                    chunks.Add(new Chunk
                    {
                        ChunkId = $"chunk_{chunkId++}",
                        SectionTitle = title,
                        Content = content,
                        PageNumber = pageNumber
                    });
                }
            }
            else
            {
                var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var para in paragraphs)
                {
                    string trimmed = para.Trim();
                    if (trimmed.Length > 100)
                    {
                        chunks.Add(new Chunk
                        {
                            ChunkId = $"chunk_{chunkId++}",
                            SectionTitle = "Generic",
                            Content = trimmed,
                            PageNumber = pageNumber
                        });
                    }
                }
            }

            return chunks;
        }
    }
}
