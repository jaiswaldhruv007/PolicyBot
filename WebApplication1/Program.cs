using Qdrant.Client;
using policyBot.Configuration;
using policyBot.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<policyBot.Services.PdfReaderService>();
builder.Services.Configure<EmbeddingSettings>(
    builder.Configuration.GetSection("Embedding"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();
builder.Services.AddSingleton<InMemoryVectorDb>();

// Add QdrantClient and QdrantVectorDb
builder.Services.AddSingleton<QdrantClient>(sp =>
    new QdrantClient("localhost", 6334)); // Use your Qdrant endpoint
builder.Services.AddSingleton<QdrantVectorDb>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
