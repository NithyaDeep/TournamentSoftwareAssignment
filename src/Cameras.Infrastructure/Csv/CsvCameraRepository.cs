using Cameras.Application.Abstractions;
using Cameras.Domain;

namespace Cameras.Infrastructure.Csv;

/// <summary>
/// Reads the camera CSV once and serves every request from memory.
/// </summary>
public sealed class CsvCameraRepository : ICameraRepository
{
    private readonly IReadOnlyList<Camera> _cameras;

    public CsvCameraRepository(string csvPath)
    {
        if (string.IsNullOrWhiteSpace(csvPath))
        {
            throw new ArgumentException("A path to the camera CSV file is required.", nameof(csvPath));
        }

        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException(
                $"Camera data file not found at '{csvPath}'. Pass --data <path> or check that " +
                "data/cameras-defb.csv is present in the repository.",
                csvPath);
        }

        SourcePath = csvPath;

        var result = CameraCsvParser.Parse(File.ReadLines(csvPath));
        _cameras = result.Cameras;
        Warnings = result.Warnings;
    }

    public string SourcePath { get; }

    public IReadOnlyList<CsvRowWarning> Warnings { get; }

    public IReadOnlyList<Camera> GetAll() => _cameras;
}
