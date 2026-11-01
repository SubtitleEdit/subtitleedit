using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinaryAdjustColor;

/// <summary>
/// Adjust color in the editor look (see <see cref="EditorChrome"/>): the color swatch in a settings
/// column, the preview on the right, Cancel/OK at the bottom.
/// </summary>
public class BinaryAdjustColorWindow : Window
{
    public BinaryAdjustColorWindow(BinaryAdjustColorViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ImageBasedEdit.AdjustColor;
        Width = 700;
        Height = 400;
        MinWidth = 600;
        MinHeight = 400;
        CanResize = true;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var leftPanel = MakeControlsPanel(vm);
        var rightPanel = MakePreviewPanel(vm);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        Content = BinaryEditChrome.MakePreviewDialogRoot(leftPanel, rightPanel, EditorChrome.MakeCancelOkBar(null, buttonCancel, buttonOk), 260);

        UiUtil.FocusOnFirstActivation(this, buttonCancel); // initial focus on a safe button, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeControlsPanel(BinaryAdjustColorViewModel vm)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 10,
        };

        var swatchButton = new Button
        {
            Height = 60,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Stretch,
            Padding = new Thickness(3),
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Command = vm.ChooseColorCommand,
            Content = new Border
            {
                CornerRadius = new CornerRadius(6),
                [!Border.BackgroundProperty] = new Binding(nameof(vm.ColorSwatchBrush)),
            },
            // The swatch is the whole content, so name the button or it reads as "Border" (#12087).
            [AutomationProperties.NameProperty] = Se.Language.General.ChooseColor,
        };
        panel.Children.Add(swatchButton);
        panel.Children.Add(EditorChrome.MakeHint(Se.Language.Tools.ImageBasedEdit.ColorAdjustmentInfo));

        return EditorChrome.MakeEditorSection(Se.Language.General.Color, panel, first: true);
    }

    private static Border MakePreviewPanel(BinaryAdjustColorViewModel vm)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var image = new Image
        {
            Stretch = Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewBitmap)),
        };

        scrollViewer.Content = image;

        // Image based subtitles are light or dark text on a transparent background, both
        // invisible on a matching flat backdrop - use the mid-gray checkerboard (issue #12692).
        return BinaryEditChrome.MakePreviewFrame(scrollViewer, UiUtil.GetCheckerboardBrush());
    }
}
