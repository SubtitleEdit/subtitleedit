using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync.VisualSync;

/// <summary>
/// Manual sync (from visual sync's Sync menu), in the editor look (see <see cref="EditorChrome"/>):
/// offset and speed factor in one section, Cancel and OK at the bottom.
/// </summary>
public class ManualSyncWindow : Window
{
    public ManualSyncWindow(ManualSyncViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ManualSync;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.TimeCodes);
        vm.Window = this;
        DataContext = vm;

        var numericUpDownOffsetSeconds = UiUtil.MakeNumericUpDownThreeDecimals(-10000, 100000, 120, vm, nameof(vm.OffsetSeconds));
        numericUpDownOffsetSeconds.Increment = 0.1m;
        numericUpDownOffsetSeconds.Classes.Add("field");
        AutomationProperties.SetName(numericUpDownOffsetSeconds, Se.Language.Sync.OffsetInSeconds);

        var numericUpDownSpeedFactor = UiUtil.MakeNumericUpDownThreeDecimals(-1000, 1000, 120, vm, nameof(vm.SpeedFactor));
        numericUpDownSpeedFactor.Increment = 0.1m;
        numericUpDownSpeedFactor.Classes.Add("field");
        AutomationProperties.SetName(numericUpDownSpeedFactor, Se.Language.Sync.SpeedFactor);

        var rows = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnSpacing = 12,
            RowSpacing = 7,
        };
        rows.Add(EditorChrome.MakeDimLabel(Se.Language.Sync.OffsetInSeconds, new Thickness(0)), 0, 0);
        rows.Add(numericUpDownOffsetSeconds, 0, 1);
        rows.Add(EditorChrome.MakeDimLabel(Se.Language.Sync.SpeedFactor, new Thickness(0)), 1, 0);
        rows.Add(numericUpDownSpeedFactor, 1, 1);

        var section = EditorChrome.MakeEditorSection(Se.Language.General.ManualSync, rows, first: true);
        section.MinWidth = 300;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, numericUpDownOffsetSeconds); // initial focus on an input, not an action button - a focused button clicks on bare Space
        Loaded += (_, _) => UiUtil.RestoreWindowPosition(this);
        Closing += (_, _) => UiUtil.SaveWindowPosition(this);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
