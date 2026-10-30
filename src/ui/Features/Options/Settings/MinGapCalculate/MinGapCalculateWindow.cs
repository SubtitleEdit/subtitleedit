using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings.MinGapCalculate;

/// <summary>
/// Frames to milliseconds at a frame rate, in the editor look (see <see cref="EditorChrome"/>):
/// the inputs, the calculation and the resulting value, Cancel + OK in the button bar.
/// </summary>
public class MinGapCalculateWindow : Window
{
    private readonly MinGapCalculateViewModel _vm;

    public MinGapCalculateWindow(MinGapCalculateViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = vm.Title;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);

        _vm = vm;
        vm.Window = this;
        DataContext = vm;

        var comboBoxFrameRate = UiUtil.MakeEditableComboBox(150, vm.FrameRates, vm, nameof(vm.SelectedFrameRate));
        AutomationProperties.SetName(comboBoxFrameRate, Se.Language.General.FrameRate);

        var numericUpDownFrames = UiUtil.MakeNumericUpDownInt(0, 100, 2, 150, vm, nameof(vm.Frames)).AsField(vm.FramesLabel, themeForeground: true);

        var labelCalculation = new TextBlock
        {
            FontSize = EditorChrome.FieldFontSize,
            Foreground = EditorChrome.DimTextBrush,
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.CalculationText)),
        };

        var labelUseAsNewGap = new TextBlock
        {
            FontWeight = FontWeight.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.UseAsNewGapText)),
        };

        // The result on the soft accent, so it reads as the value OK applies.
        var result = new Border
        {
            Background = EditorChrome.AccentSoftBrush,
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 8),
            Child = new StackPanel
            {
                Spacing = 4,
                Children = { labelCalculation, labelUseAsNewGap },
            },
        };

        var rows = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.FrameRate, comboBoxFrameRate, 110),
                EditorChrome.MakeLabeledRow(vm.FramesLabel, numericUpDownFrames, 110),
                result,
            },
        };

        var section = EditorChrome.MakeEditorSection(vm.Title, rows, first: true);
        section.Width = 360;

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(section, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, comboBoxFrameRate);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.OnKeyDown(e);
    }
}
