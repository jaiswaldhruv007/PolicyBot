using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

public class QdrantVectorDb
{
    private readonly QdrantClient _client;
    private readonly string _collectionName = "pdf_chunks";
    private readonly int _vectorSize = 768; // Set this to your embedding dimension

    public QdrantVectorDb(QdrantClient client)
    {
        _client = client;
    }

    public async Task CreateCollectionIfNotExistsAsync()
    {
        var collections = await _client.ListCollectionsAsync();
        if (!collections.Any(c => c == _collectionName))
        {
            await _client.CreateCollectionAsync(
                _collectionName,
                new VectorParams
                {
                    Size = (ulong)_vectorSize,
                    Distance = Distance.Cosine
                }
            );
        }
    }

    public async Task SaveAsync(string fileName, List<string> chunks, List<List<float>> embeddings)
    {
        // Ensure the collection exists before upserting
        await CreateCollectionIfNotExistsAsync();

        var points = new List<PointStruct>();
        for (int i = 0; i < chunks.Count; i++)
        {
            var vector = new Vector();
            vector.Data.AddRange(embeddings[i]);

            var point = new PointStruct
            {
                Id = new PointId { Uuid = Guid.NewGuid().ToString() },
                Vectors = new Vectors { Vector = vector }
            };
            point.Payload.Add("text", new Qdrant.Client.Grpc.Value { StringValue = chunks[i] });
            point.Payload.Add("fileName", new Qdrant.Client.Grpc.Value { StringValue = fileName });
            points.Add(point);
        }

        await _client.UpsertAsync(_collectionName, points);
    }

    public async Task<List<object>> GetChunksAsync(string fileName = null)
    {
        Filter filter = null;
        if (!string.IsNullOrEmpty(fileName))
        {
            filter = new Filter
            {
                Must = {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "fileName",
                            Match = new Match { Keyword = fileName }
                        }
                    }
                }
            };
        }

        var scrollResult = await _client.ScrollAsync(
            _collectionName,
            filter: filter,
            limit: 1000
        );

        var result = new List<object>();
        foreach (var point in scrollResult.Result)
        {
            var payload = point.Payload;
            var chunk = payload.ContainsKey("text") ? payload["text"].StringValue : null;
            var file = payload.ContainsKey("fileName") ? payload["fileName"].StringValue : null;
            var embedding = point.Vectors?.Vector?.Data?.ToList();
            result.Add(new { FileName = file, Chunk = chunk, Embedding = embedding });
        }
        return result;
    }
}