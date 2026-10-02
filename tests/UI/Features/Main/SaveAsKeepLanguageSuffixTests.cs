using Nikse.SubtitleEdit.Features.Main;

namespace UITests.Features.Main;

// #15530: "movie.da.ass" converted to SubRip suggested "movie.srt" - the video-name based
// "Save as" suggestion dropped the language tag of the loaded subtitle.
public class SaveAsKeepLanguageSuffixTests
{
    private static string P(params string[] parts) => Path.Combine(parts);

    [Fact]
    public void TwoLetterCode_IsKept()
    {
        Assert.Equal(P("v", "movie.da"), MainViewModel.KeepSubtitleLanguageSuffix(P("v", "movie"), P("v", "movie.da")));
    }

    [Fact]
    public void ThreeLetterCodeAndExtraTag_AreKept()
    {
        Assert.Equal(P("v", "movie.eng.forced"), MainViewModel.KeepSubtitleLanguageSuffix(P("v", "movie"), P("v", "movie.eng.forced")));
    }

    [Fact]
    public void RegionCode_IsKept()
    {
        Assert.Equal(P("v", "movie.pt-BR"), MainViewModel.KeepSubtitleLanguageSuffix(P("v", "movie"), P("v", "movie.pt-BR")));
    }

    [Fact]
    public void SubtitleInOtherFolder_KeepsVideoFolder()
    {
        Assert.Equal(P("v", "movie.da"), MainViewModel.KeepSubtitleLanguageSuffix(P("v", "movie"), P("s", "movie.da")));
    }

    [Fact]
    public void NonLanguageSuffix_IsNotKept()
    {
        Assert.Equal(P("v", "movie"), MainViewModel.KeepSubtitleLanguageSuffix(P("v", "movie"), P("v", "movie.final")));
    }

    [Fact]
    public void DifferentName_IsNotKept()
    {
        Assert.Equal(P("v", "movie"), MainViewModel.KeepSubtitleLanguageSuffix(P("v", "movie"), P("v", "other.da")));
    }

    [Fact]
    public void SameName_IsUnchanged()
    {
        Assert.Equal(P("v", "movie.da"), MainViewModel.KeepSubtitleLanguageSuffix(P("v", "movie.da"), P("v", "movie.da")));
    }

    // With "Append language code" on, the code is appended after the kept tag: "movie.da.forced.da",
    // or "movie.en.da" for an English subtitle translated to Danish. Leave the tag to that setting.
    [Theory]
    [InlineData("TwoLetterLanguageCode")]
    [InlineData("ThreeLEtterLanguageCode")]
    [InlineData("ThreeLetterLanguageCodeBibliographic")]
    public void AppendLanguageCodeOn_TagIsNotKept(string appendSetting)
    {
        Assert.Equal(P("v", "movie"), MainViewModel.KeepSubtitleLanguageSuffix(P("v", "movie"), P("v", "movie.da.forced"), appendSetting));
        Assert.Equal(P("v", "movie"), MainViewModel.KeepSubtitleLanguageSuffix(P("v", "movie"), P("v", "movie.en"), appendSetting));
    }

    [Fact]
    public void AppendLanguageCodeNone_TagIsKept()
    {
        Assert.Equal(P("v", "movie.da.forced"), MainViewModel.KeepSubtitleLanguageSuffix(P("v", "movie"), P("v", "movie.da.forced"), "None"));
    }
}
