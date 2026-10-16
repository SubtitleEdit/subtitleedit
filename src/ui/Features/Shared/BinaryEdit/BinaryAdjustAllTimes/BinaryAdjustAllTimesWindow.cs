using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Sync;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAllTimes;

/// <summary>
/// Adjust all times of image-based subtitles, in the editor look (see <see cref="EditorChrome"/>):
/// the adjustment with Show earlier/later and which lines to adjust side by side; the total
/// adjustment and Done at the bottom. Every Show earlier/later is applied at once.
/// </summary>
public class BinaryAdjustAllTimesWindow : Window
{
    public BinaryAdjustAllTimesWindow(BinaryAdjustAllTimesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Sync.AdjustAllTimes;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var timeCodeUpDown = new TimeCodeUpDown
        {
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [Avalonia.Automation.AutomationProperties.NameProperty] = Se.Language.General.Adjustment,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.Adjustment))
            {
                Mode = BindingMode.TwoWay,
            }
        };

        var buttonShowEarlier = MakeShowButton("fa-solid fa-backward", Se.Language.Sync.ShowEarlier, vm.ShowEarlierCommand);
        var buttonShowLater = MakeShowButton("fa-solid fa-forward", Se.Language.Sync.ShowLater, vm.ShowLaterCommand);
        var panelShowButtons = new Grid
        {
            ColumnSpacing = 8,
            ColumnDefinitions = { new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(1, GridUnitType.Star) },
        };
        panelShowButtons.Add(buttonShowEarlier, 0, 0);
        panelShowButtons.Add(buttonShowLater, 0, 1);

        var adjustmentRows = new StackPanel { Spacing = 10 };
        adjustmentRows.Children.Add(timeCodeUpDown);
        adjustmentRows.Children.Add(panelShowButtons);
        var adjustmentSection = EditorChrome.MakeEditorSection(Se.Language.General.Adjustment, adjustmentRows, first: true);
        adjustmentSection.MinWidth = 280;

        var panelRadioButtons = SyncTimingChrome.MakeApplyToRadioButtons(
            nameof(vm.AdjustAll),
            nameof(vm.AdjustSelectedLines),
            nameof(vm.AdjustSelectedLinesAndForward),
            nameof(vm.IsSelectionAvailable));
        var linesSection = EditorChrome.MakeEditorSection(Se.Language.General.ApplyTo, panelRadioButtons, first: true);

        var totalAdjustmentLabel = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0));
        totalAdjustmentLabel.TextTrimming = TextTrimming.CharacterEllipsis;
        totalAdjustmentLabel.Bind(TextBlock.TextProperty, new Binding(nameof(vm.TotalAdjustmentInfo)));

        var buttonOk = UiUtil.MakeButtonDone(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        Content = EditorChrome.MakeRoot(EditorChrome.MakeSideBySide(adjustmentSection, linesSection), EditorChrome.MakeButtonBar(totalAdjustmentLabel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, timeCodeUpDown); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    /// <summary>
    /// Show earlier/later: the actions of this window, so real buttons with an icon and the text.
    /// </summary>
    private static Button MakeShowButton(string icon, string text, System.Windows.Input.ICommand command)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Center };
        content.Children.Add(new Optris.Icons.Avalonia.Icon { Value = icon, FontSize = 11, VerticalAlignment = VerticalAlignment.Center });
        content.Children.Add(new TextBlock { Text = text, FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center });
        var button = new Button
        {
            Content = content,
            Command = command,
            MinWidth = 110,
            Padding = new Thickness(12, 6),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };
        Avalonia.Automation.AutomationProperties.SetName(button, text);
        return button;
    }
}
