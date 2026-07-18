using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SeConv.Core;
using Xunit;

namespace SeConvTests.Core;

/// <summary>
/// Regression tests for #12582: a file declaring the Scenarist_SCC signature must be
/// decoded by the SCC parsers or fail - it must never fall through to the generic
/// digit-scraping guesser, which turned malformed SCC into corrupt SRT (negative-duration
/// cues, raw source text) while still reporting success.
/// </summary>
public class MalformedSccTest : IDisposable
{
    private readonly string _tempRoot;

    // The minimal reproducer from #12582: a garbage payload row and a control-only row.
    private const string MalformedScc =
        "Scenarist_SCC V1.0\n" +
        "\n" +
        "00:00:25:00\tæ@æ@ðmBP ðnRðð÷P PÐaPP\n" +
        "\n" +
        "00:00:45:00\t942c 942c\n";

    // One valid caption ("Hello world", display at 1s, clear at 3s).
    private const string ValidSccRows =
        "00:00:01:00\t9420 9420 94ae 94ae 9452 9452 97a1 97a1 c8e5 ecec ef20 f7ef f2ec 6480 942f 942f\n" +
        "\n" +
        "00:00:03:00\t942c 942c\n";

    public MalformedSccTest()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "MalformedSccTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private string WriteFile(string name, string content)
    {
        var path = Path.Combine(_tempRoot, name);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void MalformedScc_NothingDecodes_Throws()
    {
        var path = WriteFile("invalid.scc", MalformedScc);

        var ex = Assert.Throws<InvalidOperationException>(() => LibSEIntegration.LoadSubtitleWithFormat(path));

        Assert.Contains("Scenarist_SCC", ex.Message);
        Assert.Contains("2 undecodable row(s)", ex.Message);
    }

    [Fact]
    public void ValidScc_LoadsWithoutWarnings()
    {
        var path = WriteFile("valid.scc", "Scenarist_SCC V1.0\n\n" + ValidSccRows);
        var warnings = new List<string>();

        var (subtitle, format) = LibSEIntegration.LoadSubtitleWithFormat(path, warnings: warnings);

        Assert.IsType<ScenaristClosedCaptions>(format);
        var paragraph = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Hello world", paragraph.Text);
        Assert.Equal(1000, paragraph.StartTime.TotalMilliseconds, precision: 0);
        Assert.Equal(3000, paragraph.EndTime.TotalMilliseconds, precision: 0);
        Assert.Empty(warnings);
    }

    [Fact]
    public void PartiallyDamagedScc_LoadsDecodableRows_WithWarning()
    {
        var path = WriteFile("partial.scc",
            "Scenarist_SCC V1.0\n\n" + ValidSccRows + "\n00:00:05:00\tæ@æ@ðmBP garbage row\n");
        var warnings = new List<string>();

        var (subtitle, format) = LibSEIntegration.LoadSubtitleWithFormat(path, warnings: warnings);

        Assert.IsType<ScenaristClosedCaptions>(format);
        var paragraph = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Hello world", paragraph.Text);
        var warning = Assert.Single(warnings);
        Assert.Contains("could not be decoded", warning);
    }

    [Fact]
    public void DropFrameScc_LoadsWithDropFrameVariant()
    {
        var path = WriteFile("dropframe.scc",
            "Scenarist_SCC V1.0\n\n" + ValidSccRows.Replace(":00\t", ";00\t"));

        var (subtitle, format) = LibSEIntegration.LoadSubtitleWithFormat(path);

        Assert.IsType<ScenaristClosedCaptionsDropFrame>(format);
        var paragraph = Assert.Single(subtitle.Paragraphs);
        Assert.Equal("Hello world", paragraph.Text);
    }

    // Same corruption class without the SCC signature: the last-resort guesser scrapes
    // digits out of arbitrary text ("V1.0" became a 42 ms -> 0 ms cue in #12582). Its
    // output must be rejected when the guessed timing is implausible.
    [Fact]
    public void UnknownFormatGuess_ImplausibleTiming_Throws()
    {
        var path = WriteFile("not-a-subtitle.txt", MalformedScc.Replace("Scenarist_SCC", "Some_Other_Log"));

        var ex = Assert.Throws<InvalidOperationException>(() => LibSEIntegration.LoadSubtitleWithFormat(path));

        Assert.Contains("Unable to determine subtitle format", ex.Message);
    }
}
