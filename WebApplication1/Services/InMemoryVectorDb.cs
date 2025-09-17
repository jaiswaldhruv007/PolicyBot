using System.Collections.Concurrent;

public class InMemoryVectorDb
{
    // Each entry: fileName -> list of (chunk, embedding)
    private readonly ConcurrentDictionary<string, List<(string Chunk, List<float> Embedding)>> _db = new();

    public void Save(string fileName, List<string> chunks, List<List<float>> embeddings)
    {
        var entries = new List<(string, List<float>)>();
        for (int i = 0; i < chunks.Count; i++)
        {
            entries.Add((chunks[i], embeddings[i]));
        }
        _db[fileName] = entries;
    }

    public List<(string Chunk, List<float> Embedding)> GetByFileName(string fileName)
    {
        return _db.TryGetValue(fileName, out var entries) ? entries : new List<(string, List<float>)>();
    }
}