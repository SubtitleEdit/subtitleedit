using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync;

/// <summary>
/// The "apply to" choice the timing dialogs of the Sync menu (adjust all times, change speed,
/// change frame rate, ...) share; the rest of their editor look comes from <see cref="EditorChrome"/>.
/// </summary>
internal static class SyncTimingChrome
{
    /// <summary>
    /// "Adjust all / selected lines / selected lines and forward" - radio buttons in one parent,
    /// so they form one group.
    /// </summary>
    internal static StackPanel MakeApplyToRadioButtons(string adjustAll, string adjustSelectedLines, string adjustSelectedLinesAndForward, string? isSelectionAvailable = null)
    {
        RadioButton Make(string text, string property, bool needsSelection)
        {
            var radioButton = new RadioButton
            {
                Content = text,
                FontSize = EditorChrome.FieldFontSize,
                [!RadioButton.IsCheckedProperty] = new Binding(property),
            };
            if (needsSelection && isSelectionAvailable != null)
            {
                radioButton[!RadioButton.IsEnabledProperty] = new Binding(isSelectionAvailable);
            }

            return radioButton;
        }

        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Children =
            {
                Make(Se.Language.Sync.AdjustAll, adjustAll, false),
                Make(Se.Language.Sync.AdjustSelectedLines, adjustSelectedLines, true),
                Make(Se.Language.Sync.AdjustSelectedLinesAndForward, adjustSelectedLinesAndForward, true),
            },
        };
    }
}
