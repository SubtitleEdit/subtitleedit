using Avalonia.Controls;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Sync.VisualSync;

public class ManualSyncWindow : Window
{

    public ManualSyncWindow(ManualSyncViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.ChangeSpeed;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var labelOffset = UiUtil.MakeLabel(Se.Language.Sync.OffsetInSeconds);
        var numericUpDownOffsetSeconds = UiUtil.MakeNumericUpDownThreeDecimals(-10000, 100000,120, vm, nameof(vm.OffsetSeconds));
        numericUpDownOffsetSeconds.Increment = 0.1m;

        var labelSpeedFactor = UiUtil.MakeLabel(Se.Language.Sync.SpeedFactor);
        var numericUpDownSpeedFactor  = UiUtil.MakeNumericUpDownThreeDecimals(-1000, 1000, 120, vm, nameof(vm.SpeedFactor));
        numericUpDownSpeedFactor.Increment = 0.1m;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelOffset, 0);
        grid.Add(numericUpDownOffsetSeconds, 0,1);
        grid.Add(labelSpeedFactor, 1);
        grid.Add(numericUpDownSpeedFactor,1,1);
        grid.Add(buttonPanel, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
