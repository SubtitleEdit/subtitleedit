using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa.AssaProgressBar;
using Nikse.SubtitleEdit.Features.Main.GridColumns;
using Nikse.SubtitleEdit.Features.Ocr.NOcr;
using Nikse.SubtitleEdit.Features.Options.Plugins;
using Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;
using Nikse.SubtitleEdit.Features.Video.BurnIn;
using Nikse.SubtitleEdit.Features.Video.RemuxVideo;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager.VoicePacks;
using System;
using Xunit;

namespace UITests.Logic.Accessibility;

/// <summary>
/// A list row whose template is more than a bare text block, and every combo box value, is
/// announced by the item's ToString() - without an override a screen reader reads the class
/// name, e.g. "Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems.ToolbarItemDisplay"
/// (#12087). These lists are empty when their window opens, so AccessibleNamesTests cannot
/// see them; this pins the override on their item types.
/// </summary>
public class ListItemNamesTests
{
    [Theory]
    [InlineData(typeof(ToolbarItemDisplay))]
    [InlineData(typeof(GridColumnDisplay))]
    [InlineData(typeof(PluginDisplayItem))]
    [InlineData(typeof(GetPluginsDisplayItem))]
    [InlineData(typeof(VoicePackItem))]
    [InlineData(typeof(ResolutionItem))]
    [InlineData(typeof(ProgressBarChapter))]
    [InlineData(typeof(RemuxFileItem))]
    [InlineData(typeof(NOcrTrainFontItem))]
    [InlineData(typeof(SubtitleFormat))]
    public void ItemType_OverridesToString(Type type)
    {
        var declaringType = type.GetMethod(nameof(ToString), Type.EmptyTypes)!.DeclaringType;
        Assert.Equal(type, declaringType);
    }

    [Fact]
    public void SubtitleFormat_ToString_IsItsName()
    {
        Assert.Equal(new SubRip().Name, new SubRip().ToString());
    }

    [Fact]
    public void ProgressBarChapter_ToString_HasTextAndTime()
    {
        var chapter = new ProgressBarChapter { Text = "Intro", StartTimeMs = 61_500 };
        Assert.Equal("Intro, 00:01:01.500", chapter.ToString());
    }
}
