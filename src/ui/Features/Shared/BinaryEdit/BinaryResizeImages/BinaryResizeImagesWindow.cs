using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.ValueConverters;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryResizeImages;

/// <summary>
/// Resize images in the editor look (see <see cref="EditorChrome"/>): the percentage in a settings
/// column, the preview on the right, Cancel/OK at the bottom.
/// </summary>
public class BinaryResizeImagesWindow : Window
{
    public BinaryResizeImagesWindow(BinaryResizeImagesViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ImageBasedEdit.ResizeImages;
        Width = 800;
        Height = 600;
        MinWidth = 600;
        MinHeight = 400;
        CanResize = true;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        // Settings column on the left, preview on the right
        var leftPanel = MakeControlsPanel(vm, out var percentageInput);
        var rightPanel = MakePreviewPanel(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        Content = BinaryEditChrome.MakePreviewDialogRoot(leftPanel, rightPanel, EditorChrome.MakeCancelOkBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, percentageInput); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeControlsPanel(BinaryResizeImagesViewModel vm, out NumericUpDown percentageInput)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
        };

        // Percentage
        var percentageUpDown = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 1000,
            Increment = 10,
            FormatString = "0",
            [!NumericUpDown.ValueProperty] = new Binding(nameof(vm.Percentage))
            {
                Mode = BindingMode.TwoWay,
                Converter = new NullableIntConverter(),
            },
        };
        EditorChrome.AsField(percentageUpDown, Se.Language.Tools.ImageBasedEdit.Percentage, 100, alignLeft: true, center: true);
        percentageUpDown.InnerRightContent = new TextBlock
        {
            Text = "%",
            FontSize = EditorChrome.ScaledFont(11),
            Foreground = EditorChrome.FaintTextBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
        };
        panel.Children.Add(percentageUpDown);
        percentageInput = percentageUpDown;

        // Image size display
        var imageSizeLabel = new TextBlock
        {
            [!TextBlock.TextProperty] = new Binding(nameof(vm.ImageSizeText)),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontSize = EditorChrome.FieldFontSize,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
        };
        panel.Children.Add(imageSizeLabel);

        // Info text
        panel.Children.Add(EditorChrome.MakeHint(Se.Language.Tools.ImageBasedEdit.ResizeImagesInfo));

        return EditorChrome.MakeEditorSection(Se.Language.Tools.ImageBasedEdit.Percentage, panel, first: true);
    }

    private static Border MakePreviewPanel(BinaryResizeImagesViewModel vm)
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
