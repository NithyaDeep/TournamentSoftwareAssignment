using Cameras.Domain;

namespace Cameras.Infrastructure.Csv;

/// <summary>
/// The outcome of parsing the camera CSV: the cameras that were read, and a record of
/// every row that could not be read.
/// </summary>
public sealed record CsvParseResult(
    IReadOnlyList<Camera> Cameras,
    IReadOnlyList<CsvRowWarning> Warnings);

public sealed record CsvRowWarning(int LineNumber, string RawLine, string Reason);
