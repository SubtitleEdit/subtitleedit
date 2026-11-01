using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustBrightness;

/// <summary>
/// Adjust brightness in the editor look (see <see cref="EditorChrome"/>): brightness, contrast and
/// gamma sliders in a settings column, the preview on the right, Cancel/OK at the bottom.
/// </summary>
public class BinaryAdjustBrightnessWindow : Window
{
    public BinaryAdjustBrightnessWindow(BinaryAdjustBrightnessViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ImageBasedEdit.AdjustBrightness;
        Width = 800;
        Height = 600;
        MinWidth = 600;
        MinHeight = 400;
        CanResize = true;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        // Settings column on the left, preview on the right
        var leftPanel = MakeControlsPanel(vm, out var brightnessInputSlider);
        var rightPanel = MakePreviewPanel(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        Content = BinaryEditChrome.MakePreviewDialogRoot(leftPanel, rightPanel, EditorChrome.MakeCancelOkBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, brightnessInputSlider); // initial focus on an input, not an action button - a focused button clicks on bare Space
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

    private static StackPanel MakeControlsPanel(BinaryAdjustBrightnessViewModel vm, out Slider brightnessInputSlider)
    {
        var l = Se.Language.Tools.ImageBasedEdit;
        var panel = new StackPanel();

        brightnessInputSlider = MakeSlider(nameof(vm.Brightness), -100, 100);
        panel.Children.Add(BinaryEditChrome.MakeSliderSection(Se.Language.General.Brightness, brightnessInputSlider, nameof(vm.BrightnessDisplay), first: true));
        panel.Children.Add(BinaryEditChrome.MakeSliderSection(l.Contrast, MakeSlider(nameof(vm.Contrast), -100, 100), nameof(vm.ContrastDisplay)));
        panel.Children.Add(BinaryEditChrome.MakeSliderSection(l.Gamma, MakeSlider(nameof(vm.Gamma), 1, 300), nameof(vm.GammaDisplay)));

        // Reset button and info text
        var resetButton = EditorChrome.MakeActionButton("fa-solid fa-rotate-left", l.ResetToDefaults, vm.ResetCommand);
        resetButton.HorizontalAlignment = HorizontalAlignment.Left;
        resetButton.Margin = new Thickness(-10, 0, 0, 0); // the flat button pads its icon; line it up with the text
        var resetPanel = new StackPanel { Spacing = 10 };
        resetPanel.Children.Add(resetButton);
        resetPanel.Children.Add(EditorChrome.MakeHint(l.BrightnessAdjustmentInfo));
        panel.Children.Add(new Border
        {
            Child = resetPanel,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 10, 14, 14),
        });

        return panel;
    }

    private static Border MakePreviewPanel(BinaryAdjustBrightnessViewModel vm)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var image = new Image
        {
            Stretch = Avalonia.Media.Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewBitmap)),
        };

        scrollViewer.Content = image;

        vm.PreviewImage = image;

        // Image based subtitles are light or dark text on a transparent background, both
        // invisible on a matching flat backdrop - use the mid-gray checkerboard (issue #12692).
        return BinaryEditChrome.MakePreviewFrame(scrollViewer, UiUtil.GetCheckerboardBrush());
    }
}
