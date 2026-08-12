using Nikse.SubtitleEdit.Logic;
using System;
using System.IO;

namespace UITests.Logic;

public class MakeWindowTitleTests : IDisposable
{
    private readonly Func<string?>? _oldProvider = UiUtil.CurrentSubtitleFileNameProvider;

    public void Dispose()
    {
        UiUtil.CurrentSubtitleFileNameProvider = _oldProvider;
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void NoProvider_KeepsPlainTitle()
    {
        UiUtil.CurrentSubtitleFileNameProvider = null;
        Assert.Equal("Auto-translate", UiUtil.MakeWindowTitle("Auto-translate"));
    }

    [Fact]
    public void UntitledSubtitle_KeepsPlainTitle()
    {
        UiUtil.CurrentSubtitleFileNameProvider = () => string.Empty;
        Assert.Equal("Auto-translate", UiUtil.MakeWindowTitle("Auto-translate"));
    }

    [Fact]
    public void NullFileName_KeepsPlainTitle()
    {
        UiUtil.CurrentSubtitleFileNameProvider = () => null;
        Assert.Equal("Auto-translate", UiUtil.MakeWindowTitle("Auto-translate"));
    }

    [Fact]
    public void FullPath_AppendsFileNameOnly()
    {
        var fullPath = Path.Combine(Path.GetTempPath(), "my movie.en.srt");
        UiUtil.CurrentSubtitleFileNameProvider = () => fullPath;
        Assert.Equal("Auto-translate - my movie.en.srt", UiUtil.MakeWindowTitle("Auto-translate"));
    }

    [Fact]
    public void ProviderIsReadAtCallTime()
    {
        var fileName = "first.srt";
        UiUtil.CurrentSubtitleFileNameProvider = () => fileName;
        Assert.Equal("Spell check - first.srt", UiUtil.MakeWindowTitle("Spell check"));

        fileName = "second.ass";
        Assert.Equal("Spell check - second.ass", UiUtil.MakeWindowTitle("Spell check"));
    }
}
