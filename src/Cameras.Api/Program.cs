using Cameras.Api.Contracts;
using Cameras.Application.Abstractions;
using Cameras.Application.Services;
using Cameras.Infrastructure.Csv;
using Microsoft.OpenApi.Models;

const string WebCorsPolicy = "web";

var builder = WebApplication.CreateBuilder(args);

// The React dev server runs on 5173. Only that origin is allowed, rather than
// AllowAnyOrigin, so the policy says what it actually needs to permit.
builder.Services.AddCors(options => options.AddPolicy(
    WebCorsPolicy,
    policy => policy
        .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "Utrecht Cameras API",
    Version = "v1",
    Description = "Serves the Utrecht traffic-camera dataset from data/cameras-defb.csv.",
}));

var configuredPath = builder.Configuration["CameraData:Path"];
var csvPath = string.IsNullOrWhiteSpace(configuredPath)
    ? CameraDataFile.ResolveDefaultPath()
    : configuredPath;

var repository = new CsvCameraRepository(csvPath);
builder.Services.AddSingleton<ICameraRepository>(repository);
builder.Services.AddSingleton<CameraSearchService>();

var app = builder.Build();

foreach (var warning in repository.Warnings)
{
    app.Logger.LogWarning(
        "Skipped line {LineNumber} while loading camera data: {Reason}",
        warning.LineNumber,
        warning.Reason);
}

app.Logger.LogInformation(
    "Loaded {CameraCount} cameras from {SourceFile}.",
    repository.GetAll().Count,
    Path.GetFileName(repository.SourcePath));

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Utrecht Cameras API v1");
    options.DocumentTitle = "Utrecht Cameras API";
});

app.UseCors(WebCorsPolicy);

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapGet("/api/cameras", (CameraSearchService search, string? name) =>
        search.Search(name).Select(CameraResponse.From).ToList())
    .WithName("GetCameras")
    .WithSummary("Lists cameras, optionally filtered by part of the camera name.")
    .WithDescription(
        "Returns every camera when 'name' is omitted. When supplied, 'name' is matched " +
        "case-insensitively against the full camera name, so 'neude' returns the six " +
        "cameras around the Neude.")
    .WithOpenApi();

app.Run();
