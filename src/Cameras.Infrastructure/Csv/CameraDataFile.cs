namespace Cameras.Infrastructure.Csv;

/// <summary>
/// Locates the camera CSV file.
/// </summary>
public static class CameraDataFile
{
    public const string DefaultFileName = "cameras-defb.csv";
    private const string DataFolderName = "data";

    /// <summary>
    /// Returns the path to the camera CSV.
    /// </summary>
    public static string ResolveDefaultPath()
    {
        var alongsideBuildOutput = Path.Combine(AppContext.BaseDirectory, DataFolderName, DefaultFileName);
        if (File.Exists(alongsideBuildOutput))
        {
            return alongsideBuildOutput;
        }

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, DataFolderName, DefaultFileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        return alongsideBuildOutput;
    }
}
