using Cameras.Application.Services;
using Cameras.Domain;
using Cameras.Tests.Fakes;
using Xunit;

namespace Cameras.Tests;

/// <summary>
/// Tests for the search rule. These run against an in-memory repository, so they exercise
/// the real service with no CSV file involved.
/// </summary>
public class CameraSearchServiceTests
{
    private static readonly Camera NeudePostkantoor =
        new(501, "UTR-CM-501 Neude rijbaan voor Postkantoor", 52.093421, 5.118278);

    private static readonly Camera Potterstraat =
        new(502, "UTR-CM-502 Potterstraat / Loeff Berchmakerstraat", 52.093599, 5.118325);

    private static readonly Camera NeudePlein =
        new(503, "UTR-CM-503 Neude plein", 52.093448, 5.118536);

    private static CameraSearchService ServiceWith(params Camera[] cameras) =>
        new(new FakeCameraRepository(cameras));

    [Fact]
    public void Search_ReturnsOnlyCamerasWhoseNameContainsTheFragment()
    {
        var service = ServiceWith(NeudePostkantoor, Potterstraat, NeudePlein);

        var matches = service.Search("Neude");

        Assert.Equal(new[] { 501, 503 }, matches.Select(camera => camera.Number).ToArray());
    }

    [Theory]
    [InlineData("Neude")]
    [InlineData("neude")]
    [InlineData("NEUDE")]
    [InlineData("nEuDe")]
    public void Search_IgnoresCase(string fragment)
    {
        var service = ServiceWith(NeudePostkantoor, Potterstraat, NeudePlein);

        Assert.Equal(2, service.Search(fragment).Count);
    }

    [Fact]
    public void Search_MatchesOnAPartialWord()
    {
        var service = ServiceWith(NeudePostkantoor, Potterstraat, NeudePlein);

        Assert.Equal(2, service.Search("eud").Count);
    }

    [Fact]
    public void Search_MatchesAgainstTheIdentifierPrefixToo()
    {
        var service = ServiceWith(NeudePostkantoor, Potterstraat, NeudePlein);

        var matches = service.Search("UTR-CM-502");

        var camera = Assert.Single(matches);
        Assert.Equal(502, camera.Number);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Search_ReturnsEveryCameraWhenNoFragmentIsSupplied(string? fragment)
    {
        var service = ServiceWith(NeudePostkantoor, Potterstraat, NeudePlein);

        Assert.Equal(3, service.Search(fragment).Count);
    }

    [Fact]
    public void Search_IgnoresSurroundingWhitespaceInTheFragment()
    {
        var service = ServiceWith(NeudePostkantoor, Potterstraat, NeudePlein);

        Assert.Equal(2, service.Search("  Neude  ").Count);
    }

    [Fact]
    public void Search_ReturnsNothingWhenThereIsNoMatch()
    {
        var service = ServiceWith(NeudePostkantoor, Potterstraat, NeudePlein);

        Assert.Empty(service.Search("Amsterdam"));
    }

    [Fact]
    public void Search_ReturnsResultsOrderedByCameraNumber()
    {
        var service = ServiceWith(NeudePlein, NeudePostkantoor, Potterstraat);

        var numbers = service.Search(null).Select(camera => camera.Number).ToArray();

        Assert.Equal(new[] { 501, 502, 503 }, numbers);
    }

    /// <summary>
    /// Two cameras share the number 545, so ordering falls back to the name to stay
    /// deterministic.
    /// </summary>
    [Fact]
    public void Search_OrdersCamerasThatShareANumberByName()
    {
        var stationsstraat = new Camera(545, "UTR-CM-545 Stationsstraat / Stationsplein", 52.091068, 5.110979);
        var oudkerkhof = new Camera(545, "UTR-CM-545 Oudkerkhof / Korte Jansstraat", 52.091968, 5.122204);
        var service = ServiceWith(stationsstraat, oudkerkhof);

        var names = service.Search(null).Select(camera => camera.Name).ToArray();

        Assert.Equal(new[] { oudkerkhof.Name, stationsstraat.Name }, names);
    }

    [Fact]
    public void Search_ReturnsNothingWhenTheRepositoryIsEmpty()
    {
        var service = ServiceWith();

        Assert.Empty(service.Search("Neude"));
    }
}
