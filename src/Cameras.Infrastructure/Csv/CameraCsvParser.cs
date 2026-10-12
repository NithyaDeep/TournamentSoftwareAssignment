using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Cameras.Domain;

namespace Cameras.Infrastructure.Csv;

/// <summary>
/// Turns the lines of the semicolon-separated camera file into <see cref="Camera"/> objects.
/// </summary>
public static class CameraCsvParser
{
    private const char Delimiter = ';';
    private const int ExpectedFieldCount = 3;
    private const string HeaderFirstField = "Camera";

    /// <summary>
    /// Matches the camera number inside a name. The source file uses two formats —
    /// "UTR-CM-501 Neude plein" and "UTR-CM-544-Janskerkhof (plein)" — so the number
    /// cannot be found by splitting on a space.
    /// </summary>
    private static readonly Regex CameraNumberPattern =
        new(@"UTR-CM-(\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Parses the supplied lines, which are expected to start with a header row.</summary>
    public static CsvParseResult Parse(IEnumerable<string> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var cameras = new List<Camera>();
        var warnings = new List<CsvRowWarning>();

        var lineNumber = 0;
        var headerHandled = false;

        foreach (var line in lines)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // The first non-blank line is the header when it looks like one. If it does
            // not, it is treated as data so that a headerless file still parses.
            if (!headerHandled)
            {
                headerHandled = true;

                if (LooksLikeHeader(line))
                {
                    continue;
                }
            }

            if (TryParseCamera(line, out var camera, out var reason))
            {
                cameras.Add(camera);
            }
            else
            {
                warnings.Add(new CsvRowWarning(lineNumber, line, reason));
            }
        }

        return new CsvParseResult(cameras, warnings);
    }

    private static bool LooksLikeHeader(string line)
    {
        var firstField = line.Split(Delimiter)[0].Trim();
        return firstField.Equals(HeaderFirstField, StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseCamera(
        string line,
        [NotNullWhen(true)] out Camera? camera,
        out string reason)
    {
        camera = null;

        var fields = line.Split(Delimiter);
        if (fields.Length != ExpectedFieldCount)
        {
            reason = $"Expected {ExpectedFieldCount} fields separated by '{Delimiter}' but found {fields.Length}.";
            return false;
        }

        // Names carry trailing whitespace in the source data (camera 567), so trim.
        var name = fields[0].Trim();
        if (name.Length == 0)
        {
            reason = "The camera name is empty.";
            return false;
        }

        var numberMatch = CameraNumberPattern.Match(name);
        if (!numberMatch.Success)
        {
            reason = "No camera number of the form 'UTR-CM-<digits>' was found in the name.";
            return false;
        }

        var numberText = numberMatch.Groups[1].Value;
        if (!int.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
        {
            reason = $"The camera number '{numberText}' is not a valid integer.";
            return false;
        }

        if (!TryParseCoordinate(fields[1], out var latitude))
        {
            reason = $"The latitude '{fields[1].Trim()}' is not a valid number.";
            return false;
        }

        if (!TryParseCoordinate(fields[2], out var longitude))
        {
            reason = $"The longitude '{fields[2].Trim()}' is not a valid number.";
            return false;
        }

        camera = new Camera(number, name, latitude, longitude);
        reason = string.Empty;
        return true;
    }

    private static bool TryParseCoordinate(string value, out double coordinate) =>
        double.TryParse(
            value.Trim(),
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out coordinate);
}
