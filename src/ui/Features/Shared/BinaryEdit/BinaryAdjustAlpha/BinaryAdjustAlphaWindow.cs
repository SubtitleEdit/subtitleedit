using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustAlpha;

/// <summary>
/// Adjust alpha in the editor look (see <see cref="EditorChrome"/>): alpha and threshold sliders in
/// a settings column, the preview on the right, Cancel/OK at the bottom.
/// </summary>
public class BinaryAdjustAlphaWindow : Window
{
    public BinaryAdjustAlphaWindow(BinaryAdjustAlphaViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.General.AdjustAlpha;
        Width = 800;
        Height = 600;
        CanResize = true;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        // Settings column on the left, preview on the right
        var leftPanel = MakeControlsPanel(vm, out var alphaAdjustmentSlider);
        var rightPanel = MakePreviewPanel(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        Content = BinaryEditChrome.MakePreviewDialogRoot(leftPanel, rightPanel, EditorChrome.MakeCancelOkBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, alphaAdjustmentSlider); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Slider MakeSlider(string property, double minimum, double maximum)
    {
        return new Slider
        {
            Minimum = minimum,
            Maximum = maximum,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            TickFrequency = 10,
            IsSnapToTickEnabled = false,
            [!Slider.ValueProperty] = new Binding(property)
            {
                Mode = BindingMode.TwoWay,
            },
        };
    }

    private static StackPanel MakeControlsPanel(BinaryAdjustAlphaViewModel vm, out Slider alphaAdjustmentSlider)
    {
        var panel = new StackPanel();

        alphaAdjustmentSlider = MakeSlider(nameof(vm.AlphaAdjustment), -255, 255);
        panel.Children.Add(BinaryEditChrome.MakeSliderSection(Se.Language.General.AlphaAdjustment, alphaAdjustmentSlider, nameof(vm.AlphaAdjustmentDisplay), first: true));
        panel.Children.Add(BinaryEditChrome.MakeSliderSection(Se.Language.General.AlphaThreshold, MakeSlider(nameof(vm.TransparencyThreshold), 0, 255),
            nameof(vm.TransparencyThresholdDisplay), EditorChrome.MakeHint(Se.Language.Tools.ImageBasedEdit.AlphaThresholdInfo)));

        // Reset button and info text
        var resetButton = EditorChrome.MakeActionButton("fa-solid fa-rotate-left", Se.Language.Tools.ImageBasedEdit.ResetToDefaults, vm.ResetCommand);
        resetButton.HorizontalAlignment = HorizontalAlignment.Left;
        resetButton.Margin = new Thickness(-10, 0, 0, 0); // the flat button pads its icon; line it up with the text
        var resetPanel = new StackPanel { Spacing = 10 };
        resetPanel.Children.Add(resetButton);
        resetPanel.Children.Add(EditorChrome.MakeHint(Se.Language.Tools.ImageBasedEdit.AlphaAdjustmentInfo));
        panel.Children.Add(new Border
        {
            Child = resetPanel,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 10, 14, 14),
        });

        return panel;
    }

    private static Border MakePreviewPanel(BinaryAdjustAlphaViewModel vm)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var grid = new Grid();

        var backgroundImage = new Image
        {
            Stretch = Avalonia.Media.Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!Image.SourceProperty] = new Binding(nameof(vm.CheckeredBackgroundBitmap)),
        };
        grid.Children.Add(backgroundImage);

        var previewImage = new Image
        {
            Stretch = Avalonia.Media.Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewBitmap)),
        };
        grid.Children.Add(previewImage);

        scrollViewer.Content = grid;

        vm.PreviewImage = previewImage;
        vm.BackgroundImage = backgroundImage;

        // The view model draws its own checkered backdrop behind the bitmap (sized to it)
        return BinaryEditChrome.MakePreviewFrame(scrollViewer, EditorChrome.FieldBrush);
    }
}
