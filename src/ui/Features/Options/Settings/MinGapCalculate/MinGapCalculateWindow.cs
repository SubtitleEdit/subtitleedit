using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings.MinGapCalculate;

public class MinGapCalculateWindow : Window
{
    private readonly MinGapCalculateViewModel _vm;

    public MinGapCalculateWindow(MinGapCalculateViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.MinGapCalculateTitle;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var labelFrameRate = UiUtil.MakeLabel(Se.Language.General.FrameRate);
        var comboBoxFrameRate = UiUtil.MakeEditableComboBox(150, vm.FrameRates, vm, nameof(vm.SelectedFrameRate));

        var labelFrames = UiUtil.MakeLabel(Se.Language.Options.Settings.MinGapCalculateFrames);
        var numericUpDownFrames = UiUtil.MakeNumericUpDownInt(0, 100, 2, 100, vm, nameof(vm.Frames));

        var labelCalculation = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.CalculationText)),
        };

        var labelUseAsNewGap = new TextBlock
        {
            FontWeight = FontWeight.Bold,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.UseAsNewGapText)),
        };

        var buttonPanel = UiUtil.MakeButtonBar(
            UiUtil.MakeButtonOk(vm.OkCommand),
            UiUtil.MakeButtonCancel(vm.CancelCommand));

        Content = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
            Margin = UiUtil.MakeWindowMargin(),
            Children =
            {
                labelFrameRate,
                comboBoxFrameRate,
                labelFrames,
                numericUpDownFrames,
                labelCalculation,
                labelUseAsNewGap,
                buttonPanel,
            },
        };

        Activated += delegate { comboBoxFrameRate.Focus(); };
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
