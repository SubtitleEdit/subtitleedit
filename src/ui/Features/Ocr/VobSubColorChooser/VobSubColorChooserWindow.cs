using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared.ColorPicker;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr.VobSubColorChooser;

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
        vm.Window = this;
        DataContext = vm;

        var headerText = new TextBlock
        {
            Text = Se.Language.Ocr.VobSubColorsHeader,
            FontSize = 14,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 4),
        };

        var descriptionText = new TextBlock
        {
            Text = Se.Language.Ocr.VobSubColorsDescription,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 560,
            Opacity = 0.8,
            Margin = new Thickness(0, 0, 0, 12),
        };

        var swatchesPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 14,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12),
            Children =
            {
                MakeSwatchTile(Se.Language.Ocr.VobSubColorBackground,
                    nameof(vm.BackgroundColor), nameof(vm.BackgroundHex), vm.PickBackgroundCommand),
                MakeSwatchTile(Se.Language.Ocr.VobSubColorPattern,
                    nameof(vm.PatternColor), nameof(vm.PatternHex), vm.PickPatternCommand),
                MakeSwatchTile(Se.Language.Ocr.VobSubColorEmphasis1,
                    nameof(vm.Emphasis1Color), nameof(vm.Emphasis1Hex), vm.PickEmphasis1Command),
                MakeSwatchTile(Se.Language.Ocr.VobSubColorEmphasis2,
                    nameof(vm.Emphasis2Color), nameof(vm.Emphasis2Hex), vm.PickEmphasis2Command),
            }
        };

        var previewPanel = MakePreviewPanel(vm);

        var buttonReset = UiUtil.MakeButton(Se.Language.General.Reset, vm.ResetToDefaultsCommand);
        var buttonInvert = UiUtil.MakeButton(Se.Language.Ocr.VobSubColorsInvert, vm.InvertBackgroundForegroundCommand);
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        var leftButtonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Left,
            Children = { buttonReset, buttonInvert }
        };

        var rightButtonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 6,
            HorizontalAlignment = HorizontalAlignment.Right,
            Children = { buttonOk, buttonCancel }
        };

        var buttonRow = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            Margin = new Thickness(0, 12, 0, 0),
        };
        buttonRow.Add(leftButtonsPanel, 0, 0);
        buttonRow.Add(rightButtonsPanel, 0, 1);

        var rootPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = UiUtil.MakeWindowMargin(),
            Spacing = 0,
            Width = 600,
            Children =
            {
                headerText,
                descriptionText,
                swatchesPanel,
                previewPanel,
                buttonRow,
            }
        };

        Content = rootPanel;

        Activated += delegate { buttonOk.Focus(); };
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
            BorderBrush = new SolidColorBrush(Color.FromArgb(180, 160, 160, 160)),
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

        ToolTip.SetTip(swatch, Se.Language.General.ChooseColorDotDotDot);
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
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 6, 0, 0),
        };

        var hexBlock = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            Opacity = 0.7,
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
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 128, 128, 128)),
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

        var previewHost = new Border
        {
            MinHeight = 140,
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(60, 128, 128, 128)),
            Background = MakeCheckerboardBrush(),
            Child = previewImage,
        };

        var label = new TextBlock
        {
            Text = Se.Language.General.Preview,
            FontWeight = FontWeight.SemiBold,
            Margin = new Thickness(0, 0, 0, 6),
        };

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children = { label, previewHost },
        };

        return new Border { Child = panel };
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
