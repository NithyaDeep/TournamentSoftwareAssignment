using Cameras.Application.Services;
using Cameras.Infrastructure.Csv;
using Xunit;

namespace Cameras.Tests;

/// <summary>
/// Tests that run against the supplied <c>data/cameras-defb.csv</c>.
/// </summary>
/// <remarks>
/// The tests above cover the rules in isolation. These cover the actual file, which
/// catches a different class of problem: if the dataset is replaced and its shape or
/// quirks change, these fail and say so.
/// </remarks>
public class CameraDatasetTests
{
    private static CsvCameraRepository LoadDataset() =>
        new(CameraDataFile.ResolveDefaultPath());

    [Fact]
    public void Dataset_ContainsEightyEightCameras()
    {
        var repository = LoadDataset();

        Assert.Equal(88, repository.GetAll().Count);
    }

    [Fact]
    public void Dataset_ContainsExactlyOneUnreadableRow()
    {
        var repository = LoadDataset();

        var warning = Assert.Single(repository.Warnings);
        Assert.Equal(54, warning.LineNumber);
        Assert.Contains("ERROR 1207", warning.RawLine);
    }

    /// <summary>
    /// The assignment's worked example: searching for "Neude" lists these six cameras.
    /// </summary>
    [Fact]
    public void SearchingForNeude_ReturnsTheSixCamerasFromTheAssignmentExample()
    {
        var service = new CameraSearchService(LoadDataset());

        var numbers = service.Search("Neude").Select(camera => camera.Number).ToArray();

        Assert.Equal(new[] { 501, 503, 504, 505, 506, 507 }, numbers);
    }

    [Fact]
    public void Dataset_HasTwoCamerasNumbered545()
    {
        var repository = LoadDataset();

        var duplicates = repository.GetAll().Where(camera => camera.Number == 545).ToArray();

        Assert.Equal(2, duplicates.Length);
    }

    [Fact]
    public void Dataset_HasNoCameraOnAColumnBoundary()
    {
        var repository = LoadDataset();

        var onBoundary = repository.GetAll()
            .Where(camera => camera.Number is 600 or 700 or 800)
            .ToArray();

        // Documents why the inclusive/exclusive choice at the boundaries cannot change
        // how this particular dataset is displayed.
        Assert.Empty(onBoundary);
    }

    [Fact]
    public void Dataset_CoordinatesAllFallWithinUtrecht()
    {
        var repository = LoadDataset();

        Assert.All(repository.GetAll(), camera =>
        {
            Assert.InRange(camera.Latitude, 52.0, 52.2);
            Assert.InRange(camera.Longitude, 5.0, 5.2);
        });
    }
}
