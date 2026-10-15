using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Features.Edit;
using Nikse.SubtitleEdit.Features.Video;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts.PickMilliseconds;

public class PickMillisecondsWindow : Window
{
    public PickMillisecondsWindow(PickMillisecondsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 400;
        CanResize = false;
        Title = Se.Language.Options.Shortcuts.MoveVideoPositionMilliseconds;
        EditDialogChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var numericUpDownMilliseconds = UiUtil.MakeNumericUpDownInt(1, 1_000_000, 5000, 160, vm, nameof(vm.Milliseconds));
        numericUpDownMilliseconds.HorizontalAlignment = HorizontalAlignment.Left;
        AutomationProperties.SetName(numericUpDownMilliseconds, Se.Language.General.Milliseconds);

        var row = VideoSubDialogChrome.MakeRow(Se.Language.General.Milliseconds, numericUpDownMilliseconds, 110);
        row.Margin = new Thickness(16, 18);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = VideoSubDialogChrome.MakeButtonBar(null, buttonCancel, buttonOk);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(VideoSubDialogChrome.MakePanel(row), 0);
        grid.Add(buttonPanel, 1);

        Content = grid;

        UiUtil.FocusOnFirstActivation(this, numericUpDownMilliseconds); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (s, e) => vm.OnKeyDown(e);
    }
}
