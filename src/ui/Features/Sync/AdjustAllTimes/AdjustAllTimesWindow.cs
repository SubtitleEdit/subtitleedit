using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync.AdjustAllTimes;

/// <summary>
/// Adjust all times, in the editor look (see <see cref="EditorChrome"/>): the adjustment with Show
/// earlier/later and which lines to adjust side by side; the last adjustment, help and Done at the
/// bottom. Stays open while editing - every Show earlier/later is applied at once.
/// </summary>
public class AdjustAllTimesWindow : Window
{
    public AdjustAllTimesWindow(AdjustAllTimesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = UiUtil.MakeWindowTitle(Se.Language.Sync.AdjustAllTimes);
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        var timeCodeUpDown = new TimeCodeUpDown
        {
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.Adjustment))
            {
                Mode = BindingMode.TwoWay,
            }
        }.WithAccessibleName(Se.Language.Sync.AdjustAllTimes); // no visible label; the window title names the only input (#12087)

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
            nameof(vm.AdjustSelectedLinesAndForward));
        var linesSection = EditorChrome.MakeEditorSection(Se.Language.General.ApplyTo, panelRadioButtons, first: true);

        var buttonHelp = EditorChrome.CreateToolButton("fa-solid fa-circle-question", AdjustAllTimesViewModel.GetShortcutsText(), vm.ShowHelpCommand);
        var buttonOk = UiUtil.MakeButtonDone(vm.OkCommand);
        buttonOk.Classes.Add("primary");

        var labelStatus = EditorChrome.MakeDimLabel(string.Empty, new Thickness(0)).WithBindText(vm, nameof(vm.StatusText));
        labelStatus.TextTrimming = TextTrimming.CharacterEllipsis;

        Content = EditorChrome.MakeRoot(EditorChrome.MakeSideBySide(adjustmentSection, linesSection), EditorChrome.MakeDockedButtonBar(labelStatus, buttonHelp, buttonOk));

        Loaded += delegate
        {
            buttonOk.Focus(); // hack to make OnKeyDown work
            UiUtil.RestoreWindowPosition(this);
        };
        AddHandler(KeyDownEvent, (_, e) => vm.OnKeyDown(e), RoutingStrategies.Tunnel);
        Closing += (_, e) => vm.OnClosing(e);
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
