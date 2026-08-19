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

    /// <summary>
    /// Batch convert reuses main-window dialogs as settings editors over a whole list of files -
    /// naming the main window's subtitle in their title bar claims a file they have nothing to do
    /// with (their view model is even initialized with an empty subtitle).
    /// </summary>
    [Fact]
    public void InsideASuppressionScope_KeepsPlainTitle()
    {
        UiUtil.CurrentSubtitleFileNameProvider = () => "my movie.srt";

        using (UiUtil.SuppressSubtitleFileNameInTitle())
        {
            Assert.Equal("Remove text for hearing impaired",
                UiUtil.MakeWindowTitle("Remove text for hearing impaired"));
        }

        Assert.Equal("Remove text for hearing impaired - my movie.srt",
            UiUtil.MakeWindowTitle("Remove text for hearing impaired"));
    }

    // Ref-counted, so a nested scope cannot re-enable the suffix for the outer one.
    [Fact]
    public void NestedSuppressionScopes_RestoreOnlyAfterTheOutermost()
    {
        UiUtil.CurrentSubtitleFileNameProvider = () => "my movie.srt";

        using (UiUtil.SuppressSubtitleFileNameInTitle())
        {
            using (UiUtil.SuppressSubtitleFileNameInTitle())
            {
                Assert.Equal("Settings", UiUtil.MakeWindowTitle("Settings"));
            }

            Assert.Equal("Settings", UiUtil.MakeWindowTitle("Settings"));
        }

        Assert.Equal("Settings - my movie.srt", UiUtil.MakeWindowTitle("Settings"));
    }

    // Disposing twice must not push the counter below zero and disable suppression elsewhere.
    [Fact]
    public void DoubleDispose_DoesNotUnbalanceTheCounter()
    {
        UiUtil.CurrentSubtitleFileNameProvider = () => "my movie.srt";

        var scope = UiUtil.SuppressSubtitleFileNameInTitle();
        scope.Dispose();
        scope.Dispose();

        using (UiUtil.SuppressSubtitleFileNameInTitle())
        {
            Assert.Equal("Settings", UiUtil.MakeWindowTitle("Settings"));
        }

        Assert.Equal("Settings - my movie.srt", UiUtil.MakeWindowTitle("Settings"));
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
