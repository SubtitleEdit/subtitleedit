using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.PickLayer;

public class PickLayerWindow : Window
{
    public PickLayerWindow(PickLayerViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.PickLayerTitle;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        MinWidth = 300;
        vm.Window = this;
        DataContext = vm;

        // Layer display
        var panelLayer = new StackPanel
        {
            Spacing = 0,
            Orientation = Orientation.Vertical,
            Margin = new Thickness(0, 0, 10, 0),
        };
        var labelLayer = new TextBlock
        {
            Text = Se.Language.General.Layer,
        };
        panelLayer.Children.Add(labelLayer);
        var upDownLayer = new NumericUpDown
        {
            DataContext = vm,
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.Layer))
            {
                Mode = BindingMode.TwoWay,
            },
            Minimum = int.MinValue,
            Maximum = int.MaxValue,
            Increment = 1,
            FormatString = "F0",
        };
        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(upDownLayer, Se.Language.General.Layer);
        }
        panelLayer.Children.Add(upDownLayer);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var buttonPanel = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(panelLayer, 0);
        grid.Add(buttonPanel, 1);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
