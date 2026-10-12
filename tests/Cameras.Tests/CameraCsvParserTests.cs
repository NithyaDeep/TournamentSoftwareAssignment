using System.Globalization;
using Cameras.Infrastructure.Csv;
using Xunit;

namespace Cameras.Tests;

/// <summary>
/// Tests for the CSV parser, driven by the quirks that are actually present in the
/// supplied dataset rather than by invented edge cases.
/// </summary>
public class CameraCsvParserTests
{
    private const string Header = "Camera;Latitude;Longitude";

    [Fact]
    public void Parse_IgnoresTheHeaderRow()
    {
        var result = CameraCsvParser.Parse([Header]);

        Assert.Empty(result.Cameras);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_IgnoresBlankLines()
    {
        var result = CameraCsvParser.Parse([
            Header,
            string.Empty,
            "UTR-CM-503 Neude plein;52.093448;5.118536",
            "   ",
        ]);

        Assert.Single(result.Cameras);
        Assert.Empty(result.Warnings);
    }

    /// <summary>
    /// The dataset uses two naming conventions: a space after the identifier for the
    /// early cameras and a hyphen for the later ones. Both must yield the same number.
    /// </summary>
    [Theory]
    [InlineData("UTR-CM-501 Neude rijbaan voor Postkantoor;52.093421;5.118278", 501)]
    [InlineData("UTR-CM-544-Janskerkhof (plein);52.093902;5.120875", 544)]
    [InlineData("UTR-CM-803-Waterlinieweg;52.078233;5.141773", 803)]
    public void Parse_ReadsTheCameraNumberFromEitherNameFormat(string line, int expectedNumber)
    {
        var result = CameraCsvParser.Parse([Header, line]);

        var camera = Assert.Single(result.Cameras);
        Assert.Equal(expectedNumber, camera.Number);
    }

    /// <summary>
    /// Line 54 of the supplied file is a database error message rather than a camera.
    /// It must be skipped, and the caller must be told about it.
    /// </summary>
    [Fact]
    public void Parse_SkipsTheCorruptRowAndReportsIt()
    {
        const string corruptRow = "ERROR 1207 Could not read camera information from database";

        var result = CameraCsvParser.Parse([
            Header,
            "UTR-CM-556-Laan v Puntenburg / Moreelsepark;52.086608;5.114757",
            corruptRow,
            "UTR-CM-557-Stationsplein / Adama v Scheltemabaan;52.088382;5.112054",
        ]);

        Assert.Equal(2, result.Cameras.Count);

        var warning = Assert.Single(result.Warnings);
        Assert.Equal(3, warning.LineNumber);
        Assert.Equal(corruptRow, warning.RawLine);
        Assert.Contains("Expected 3 fields", warning.Reason);
    }

    /// <summary>Camera 567 in the source file has a trailing space in its name.</summary>
    [Fact]
    public void Parse_TrimsSurroundingWhitespaceFromTheName()
    {
        var result = CameraCsvParser.Parse([
            Header,
            "UTR-CM-567-Leidseweg / Damstraat ;52.090331;5.104229",
        ]);

        var camera = Assert.Single(result.Cameras);
        Assert.Equal("UTR-CM-567-Leidseweg / Damstraat", camera.Name);
    }

    [Theory]
    [InlineData("UTR-CM-501 Neude plein;not-a-number;5.118278", "latitude")]
    [InlineData("UTR-CM-501 Neude plein;52.093421;not-a-number", "longitude")]
    public void Parse_SkipsRowsWithAnUnreadableCoordinate(string line, string expectedInReason)
    {
        var result = CameraCsvParser.Parse([Header, line]);

        Assert.Empty(result.Cameras);
        var warning = Assert.Single(result.Warnings);
        Assert.Contains(expectedInReason, warning.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_SkipsRowsWithoutACameraNumber()
    {
        var result = CameraCsvParser.Parse([Header, "Some other camera;52.09;5.11"]);

        Assert.Empty(result.Cameras);
        var warning = Assert.Single(result.Warnings);
        Assert.Contains("UTR-CM-", warning.Reason);
    }

    /// <summary>
    /// The file uses a decimal point. Parsing must not depend on the machine's regional
    /// settings, or a Dutch locale would read "52.093421" as fifty-two million.
    /// </summary>
    [Fact]
    public void Parse_ReadsCoordinatesIndependentlyOfTheCurrentCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("nl-NL");

            var result = CameraCsvParser.Parse([
                Header,
                "UTR-CM-501 Neude rijbaan voor Postkantoor;52.093421;5.118278",
            ]);

            var camera = Assert.Single(result.Cameras);
            Assert.Equal(52.093421, camera.Latitude, precision: 6);
            Assert.Equal(5.118278, camera.Longitude, precision: 6);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    /// <summary>
    /// Two different cameras in the source file are both numbered 545. Neither may be
    /// discarded, which is why the camera number is not treated as a unique key.
    /// </summary>
    [Fact]
    public void Parse_KeepsBothCamerasThatShareANumber()
    {
        var result = CameraCsvParser.Parse([
            Header,
            "UTR-CM-545 Oudkerkhof / Korte Jansstraat;52.091968;5.122204",
            "UTR-CM-545 Stationsstraat / Stationsplein;52.091068;5.110979",
        ]);

        Assert.Equal(2, result.Cameras.Count);
        Assert.All(result.Cameras, camera => Assert.Equal(545, camera.Number));
        Assert.Equal(2, result.Cameras.Select(camera => camera.Name).Distinct().Count());
    }

    [Fact]
    public void Parse_TreatsAFileWithoutAHeaderAsDataOnly()
    {
        var result = CameraCsvParser.Parse(["UTR-CM-503 Neude plein;52.093448;5.118536"]);

        var camera = Assert.Single(result.Cameras);
        Assert.Equal(503, camera.Number);
    }
}
