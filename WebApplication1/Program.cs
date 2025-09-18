using Qdrant.Client;
using policyBot.Configuration;
using policyBot.Services;
using policyBot.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<policyBot.Services.PdfReaderService>();
builder.Services.Configure<EmbeddingSettings>(
    builder.Configuration.GetSection("Embedding"));
builder.Services.Configure<LlmSettings>(
builder.Configuration.GetSection("llma"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();
builder.Services.AddScoped<IAskHRService, AskHRService>();

// Add QdrantClient and QdrantVectorDb
builder.Services.AddSingleton<QdrantClient>(sp =>
    new QdrantClient("localhost", 6334)); // Use your Qdrant endpoint
builder.Services.AddScoped<IVectorDB, QdrantVectorDb>();
builder.Services.AddScoped<IllmaService, LlmaService>();

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
