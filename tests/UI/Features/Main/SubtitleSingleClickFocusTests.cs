using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Options.Settings;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Main;

// Single-click on a subtitle row can focus the text box without touching the video (#17218).
public class SubtitleSingleClickFocusTests
{
    [Theory]
    [InlineData(SubtitleSingleClickActionType.FocusTextBox, true)]
    [InlineData(SubtitleSingleClickActionType.GoToSubtitleAndPauseAndFocusTextBox, true)]
    [InlineData(SubtitleSingleClickActionType.GoToSubtitleAndPlayAndFocusTextBox, true)]
    [InlineData(SubtitleSingleClickActionType.None, false)]
    [InlineData(SubtitleSingleClickActionType.GoToSubtitleAndPause, false)]
    [InlineData(SubtitleSingleClickActionType.GoToWaveformOnlyNoVideoPosition, false)]
    public void SingleClickFocusesTextBox(SubtitleSingleClickActionType action, bool expected)
    {
        Assert.Equal(expected, MainViewModel.SingleClickFocusesTextBox(action.ToString()));
    }

    [Fact]
    public void SingleClickFocusesTextBox_NullOrUnknown_IsFalse()
    {
        Assert.False(MainViewModel.SingleClickFocusesTextBox(null));
        Assert.False(MainViewModel.SingleClickFocusesTextBox("garbage"));
    }

    [Fact]
    public void SettingsCombo_MapsFocusTextBoxText_ToItsAction()
    {
        var text = Se.Language.Options.Settings.GridFocusTextBox;
        Assert.Equal(nameof(SubtitleSingleClickActionType.FocusTextBox), SettingsViewModel.MapToSelectedSubtitleSingleClickAction(text));
    }
}
