using System.Text.Json;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Options.Settings;

namespace UITests.Features.Options.Settings;

public class ProfileImportExportTests
{
    private static ProfileImportExport Read(string json)
    {
        return JsonSerializer.Deserialize<ProfileImportExport>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    // Import used to int.Parse every field, so a single missing or blank rule threw a
    // FormatException and took the whole file down with it.
    [Fact]
    public void ImportKeepsMissingRulesNullSoTheCurrentSettingIsUsed()
    {
        var imported = Read("{\"profiles\":[{\"name\":\"Sparse\"}]}").ToProfileDisplayList();

        Assert.NotNull(imported);
        var p = Assert.Single(imported!);
        Assert.Equal("Sparse", p.Name);
        Assert.Null(p.MaxLines);
        Assert.Null(p.MinDurationMs);
        Assert.Null(p.MaxDurationMs);
        Assert.Null(p.SingleLineMaxLength);
        Assert.Null(p.MaxCharsPerSec);
        Assert.NotNull(p.CustomContinuationStyle);
    }

    [Fact]
    public void ImportIgnoresAnUnparseableRuleRatherThanFailingTheFile()
    {
        var json = "{\"profiles\":[{\"name\":\"Odd\",\"subtitleLineMaximumLength\":\"forty-two\",\"maxNumberOfLines\":\"2\"}]}";

        var imported = Read(json).ToProfileDisplayList();

        var p = Assert.Single(imported!);
        Assert.Null(p.SingleLineMaxLength);
        Assert.Equal(2, p.MaxLines);
    }

    [Fact]
    public void ImportReadsEveryRuleWhenPresent()
    {
        var json = "{\"profiles\":[{\"name\":\"Full\",\"maxNumberOfLines\":\"2\",\"mergeLinesShorterThan\":\"43\"," +
                   "\"minimumMillisecondsBetweenLines\":\"83\",\"subtitleLineMaximumLength\":\"42\"," +
                   "\"subtitleMaximumCharactersPerSeconds\":\"20\",\"subtitleMaximumDisplayMilliseconds\":\"7007\"," +
                   "\"subtitleMaximumWordsPerMinute\":\"240\",\"subtitleMinimumDisplayMilliseconds\":\"833\"," +
                   "\"subtitleOptimalCharactersPerSeconds\":\"15\"}]}";

        var p = Assert.Single(Read(json).ToProfileDisplayList()!);

        Assert.Equal(2, p.MaxLines);
        Assert.Equal(43, p.UnbreakLinesShorterThan);
        Assert.Equal(83, p.MinGapMs);
        Assert.Equal(42, p.SingleLineMaxLength);
        Assert.Equal(20, p.MaxCharsPerSec);
        Assert.Equal(7007, p.MaxDurationMs);
        Assert.Equal(240, p.MaxWordsPerMin);
        Assert.Equal(833, p.MinDurationMs);
        Assert.Equal(15, p.OptimalCharsPerSec);
    }

    [Fact]
    public void ExportImportRoundTripsTheCustomContinuationStyle()
    {
        var source = new ProfileDisplay
        {
            Name = "P1",
            CustomContinuationStyle = new CustomContinuationStyle { Pause = 555, Suffix = "..", GapPrefix = "—" },
        };

        var json = JsonSerializer.Serialize(
            new ProfileImportExport(new List<ProfileDisplay> { source }),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        var p = Assert.Single(Read(json).ToProfileDisplayList()!);

        Assert.Equal(555, p.CustomContinuationStyle.Pause);
        Assert.Equal("..", p.CustomContinuationStyle.Suffix);
        Assert.Equal("—", p.CustomContinuationStyle.GapPrefix);
    }
}
