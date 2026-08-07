using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.NetflixQualityCheck;
using Xunit;

namespace UITests.Logic.NetflixQualityCheck;

public class NetflixCheckMaxCpsTests
{
    private static NetflixQualityController RunCheck(string text, string language, int durationMs)
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(text, 0, durationMs));
        var controller = new NetflixQualityController { Language = language };

        new NetflixCheckMaxCps("test").Check(subtitle, controller);

        return controller;
    }

    // 40 characters in one second is 40 CPS, well over the English limit of 20.
    private const string FortyCharacters = "Forty characters of plain dialogue here.";

    [Fact]
    public void ReportsSubtitlesOverTheReadingSpeedLimit()
    {
        var controller = RunCheck(FortyCharacters, "en", 1000);

        Assert.Single(controller.Records);
    }

    // The proposed duration used to be derived from the user's own profile setting, so it
    // could leave the subtitle still over the Netflix limit.
    [Fact]
    public void ProposedDurationMeetsTheNetflixLimit()
    {
        var controller = RunCheck(FortyCharacters, "en", 1000);
        var fixedParagraph = controller.Records[0].FixedParagraph;

        Assert.NotNull(fixedParagraph);
        Assert.True(fixedParagraph!.GetCharactersPerSecond() <= 20.001,
            $"Expected at most 20 CPS, got {fixedParagraph.GetCharactersPerSecond():0.##}");
    }

    [Fact]
    public void ProposedDurationUsesTheChildrensLimitWhenSet()
    {
        var subtitle = new Subtitle();
        subtitle.Paragraphs.Add(new Paragraph(FortyCharacters, 0, 1000));
        var controller = new NetflixQualityController { Language = "en", IsChildrenProgram = true };

        new NetflixCheckMaxCps("test").Check(subtitle, controller);

        var fixedParagraph = controller.Records[0].FixedParagraph;
        Assert.NotNull(fixedParagraph);
        Assert.True(fixedParagraph!.GetCharactersPerSecond() <= 17.001,
            $"Expected at most 17 CPS, got {fixedParagraph.GetCharactersPerSecond():0.##}");
    }

    [Fact]
    public void DoesNotReportSubtitlesWithinTheLimit()
    {
        var controller = RunCheck(FortyCharacters, "en", 4000);

        Assert.Empty(controller.Records);
    }
}
