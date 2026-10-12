using System.Globalization;
using Cameras.Application.Services;
using Cameras.Domain;
using Cameras.Infrastructure.Csv;

namespace Cameras.Cli;

/// <summary>
/// Command-line entry point. Searches cameras by part of their name and prints the
/// matches as "number | name | latitude | longitude".
/// </summary>
public static class Program
{
    private const int ExitSuccess = 0;
    private const int ExitUsageError = 1;
    private const int ExitDataError = 2;

    public static int Main(string[] args)
    {
        if (args.Any(argument => argument is "-h" or "--help"))
        {
            PrintUsage(Console.Out);
            return ExitSuccess;
        }

        if (!TryParseArguments(args, out var options, out var argumentError))
        {
            Console.Error.WriteLine(argumentError);
            PrintUsage(Console.Error);
            return ExitUsageError;
        }

        try
        {
            var dataPath = string.IsNullOrWhiteSpace(options.DataPath)
                ? CameraDataFile.ResolveDefaultPath()
                : options.DataPath;

            var repository = new CsvCameraRepository(dataPath);

            // Data problems go to stderr so that piping stdout stays clean.
            foreach (var warning in repository.Warnings)
            {
                Console.Error.WriteLine($"warning: line {warning.LineNumber}: {warning.Reason}");
            }

            var matches = new CameraSearchService(repository).Search(options.Name);

            if (matches.Count == 0)
            {
                Console.Error.WriteLine(options.Name is null
                    ? "The dataset contains no cameras."
                    : $"No cameras found matching '{options.Name}'.");
                return ExitSuccess;
            }

            foreach (var camera in matches)
            {
                Console.WriteLine(Format(camera));
            }

            return ExitSuccess;
        }
        catch (FileNotFoundException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return ExitDataError;
        }
        catch (IOException exception)
        {
            Console.Error.WriteLine($"Could not read the camera data file: {exception.Message}");
            return ExitDataError;
        }
    }

    /// <summary>
    /// Formats one camera for output. The invariant culture keeps coordinates printed with
    /// a decimal point on machines configured for a locale that uses a decimal comma.
    /// </summary>
    private static string Format(Camera camera) => string.Join(
        " | ",
        camera.Number.ToString(CultureInfo.InvariantCulture),
        camera.Name,
        camera.Latitude.ToString(CultureInfo.InvariantCulture),
        camera.Longitude.ToString(CultureInfo.InvariantCulture));

    private static bool TryParseArguments(string[] args, out CliOptions options, out string error)
    {
        string? name = null;
        string? dataPath = null;

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--name":
                    if (!TryReadValue(args, ref index, out name))
                    {
                        options = default;
                        error = "--name requires a value, for example: --name Neude";
                        return false;
                    }

                    break;

                case "--data":
                    if (!TryReadValue(args, ref index, out dataPath))
                    {
                        options = default;
                        error = "--data requires a path to a CSV file.";
                        return false;
                    }

                    break;

                default:
                    options = default;
                    error = $"Unknown argument '{args[index]}'.";
                    return false;
            }
        }

        options = new CliOptions(name, dataPath);
        error = string.Empty;
        return true;
    }

    private static bool TryReadValue(string[] args, ref int index, out string? value)
    {
        var next = index + 1;

        if (next >= args.Length || args[next].StartsWith("--", StringComparison.Ordinal))
        {
            value = null;
            return false;
        }

        value = args[next];
        index = next;
        return true;
    }

    private static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("Search Utrecht traffic cameras by part of their name.");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  dotnet run --project src/Cameras.Cli -- --name <text>");
        writer.WriteLine();
        writer.WriteLine("Options:");
        writer.WriteLine("  --name <text>   Case-insensitive text to look for in the camera name.");
        writer.WriteLine("                  Omit to list every camera.");
        writer.WriteLine("  --data <path>   Use a different CSV file instead of the bundled one.");
        writer.WriteLine("  -h, --help      Show this message.");
        writer.WriteLine();
        writer.WriteLine("Example:");
        writer.WriteLine("  dotnet run --project src/Cameras.Cli -- --name Neude");
    }

    private readonly record struct CliOptions(string? Name, string? DataPath);
}
