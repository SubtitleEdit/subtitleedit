using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.VobSubColorChooser;

/// <summary>
/// The four VobSub colors, in the editor look (see <see cref="EditorChrome"/>): the color swatches
/// and the preview in sections, Reset/Invert left and Cancel/OK right in the button bar.
/// </summary>
public class VobSubColorChooserWindow : Window
{
    private const double SwatchSize = 56;

    public VobSubColorChooserWindow(VobSubColorChooserViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.VobSubColorsTitle;
        CanResize = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var headerText = new TextBlock
        {
            Text = Se.Language.Ocr.VobSubColorsHeader,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Foreground = EditorChrome.StrongTextBrush,
        };

        var descriptionText = new TextBlock
        {
            Text = Se.Language.Ocr.VobSubColorsDescription,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12.5,
            Foreground = EditorChrome.DimTextBrush,
        };

        var swatchesPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0),
            Children =
            {
                MakeSwatchTile(Se.Language.General.Background,
                    nameof(vm.BackgroundColor), nameof(vm.BackgroundHex), vm.PickBackgroundCommand),
                MakeSwatchTile(Se.Language.Ocr.VobSubColorPattern,
                    nameof(vm.PatternColor), nameof(vm.PatternHex), vm.PickPatternCommand),
                MakeSwatchTile(Se.Language.Ocr.VobSubColorEmphasis1,
                    nameof(vm.Emphasis1Color), nameof(vm.Emphasis1Hex), vm.PickEmphasis1Command),
                MakeSwatchTile(Se.Language.Ocr.VobSubColorEmphasis2,
                    nameof(vm.Emphasis2Color), nameof(vm.Emphasis2Hex), vm.PickEmphasis2Command),
            }
        };

        var colorsPanel = new StackPanel
        {
            Spacing = 6,
            Children = { headerText, descriptionText, swatchesPanel },
        };

        var sections = new StackPanel
        {
            Width = 600,
            Children =
            {
                new Border { Child = colorsPanel, Padding = new Thickness(14, 14, 14, 14) }, // the header text is the title
                EditorChrome.MakeEditorSection(Se.Language.General.Preview, MakePreviewPanel(vm)),
            },
        };

        var buttonReset = EditorChrome.MakeTextToolButton(IconNames.Restore, Se.Language.General.Reset, vm.ResetToDefaultsCommand);
        var buttonInvert = EditorChrome.MakeTextToolButton(IconNames.SwapVertical, Se.Language.Ocr.VobSubColorsInvert, vm.InvertBackgroundForegroundCommand);
        var buttonOk = EditorChrome.AsFlushPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = EditorChrome.WithoutMargin(UiUtil.MakeButtonCancel(vm.CancelCommand));

        var leftButtonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { buttonReset, buttonInvert }
        };

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(leftButtonsPanel, buttonCancel, buttonOk));

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, buttonCancel);
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }

    private static Border MakeSwatchTile(string label, string colorProperty, string hexProperty, IRelayCommand command)
    {
        var swatch = new Border
        {
            Width = SwatchSize,
            Height = SwatchSize,
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.FieldBorderBrush,
            Cursor = new Cursor(StandardCursorType.Hand),
            HorizontalAlignment = HorizontalAlignment.Center,
            // Show a checker pattern through transparency by layering on a neutral background
            Background = MakeCheckerboardBrush(),
        };

        var swatchFill = new Border
        {
            Width = SwatchSize,
            Height = SwatchSize,
            CornerRadius = new CornerRadius(6),
        };
        swatchFill.Bind(Border.BackgroundProperty, new Binding(colorProperty)
        {
            Converter = new ColorToBrushConverter(),
        });
        swatch.Child = swatchFill;

        if (Se.Settings.Appearance.ShowHints)
        {
            ToolTip.SetTip(swatch, Se.Language.General.ChooseColorDotDotDot);
        }

        swatch.PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(swatch).Properties.IsLeftButtonPressed && command.CanExecute(null))
            {
                command.Execute(null);
            }
        };

        var labelBlock = new TextBlock
        {
            Text = label,
            FontSize = 12.5,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 6, 0, 0),
        };

        var hexBlock = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = EditorChrome.DimTextBrush,
            FontSize = 11,
        };
        hexBlock.Bind(TextBlock.TextProperty, new Binding(hexProperty));

        var tilePanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Center,
            Children = { swatch, labelBlock, hexBlock },
        };

        return new Border
        {
            Padding = new Thickness(10, 10, 10, 8),
            MinWidth = 120,
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = EditorChrome.FieldBorderBrush,
            Background = EditorChrome.FieldBrush,
            Child = tilePanel,
        };
    }

    private static Border MakePreviewPanel(VobSubColorChooserViewModel vm)
    {
        var previewImage = new Image
        {
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MaxHeight = 180,
        };
        previewImage.Bind(Image.SourceProperty, new Binding(nameof(vm.PreviewImage)));

        var previewHost = BinaryOcrChrome.MakeImageFrame(previewImage, MakeCheckerboardBrush(), 8);
        previewHost.MinHeight = 140;
        return previewHost;
    }

    private static IBrush MakeCheckerboardBrush()
    {
        // Simple subtle checker effect via a DrawingBrush would require resources;
        // use a soft neutral gradient so transparent areas show consistently across themes.
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Color.FromArgb(40, 128, 128, 128), 0.0),
                new GradientStop(Color.FromArgb(20, 128, 128, 128), 1.0),
            }
        };
    }
}
