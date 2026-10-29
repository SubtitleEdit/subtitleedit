using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Features.Ocr.BinaryOcr;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Ocr;

/// <summary>
/// Image pre-processing for OCR, in the editor look (see <see cref="EditorChrome"/>): the options
/// as switches on top, the image before and after below, Reset left and Cancel/OK right in the
/// button bar.
/// </summary>
public class PreProcessingWindow : Window
{
    public PreProcessingWindow(PreProcessingViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Ocr.PreProcessingTitle;
        CanResize = true;
        Width = 800;
        Height = 600;
        MinWidth = 700;
        MinHeight = 500;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);
        vm.Window = this;
        DataContext = vm;

        var switchCropTransparent = EditorChrome.MakeSwitch(nameof(vm.CropTransparentColors), Se.Language.Ocr.CropTransparent);
        switchCropTransparent.IsCheckedChanged += (s, e) => vm.RequestPreviewUpdate();

        var switchInverseColors = EditorChrome.MakeSwitch(nameof(vm.InverseColors), Se.Language.Ocr.InverseColors);
        switchInverseColors.IsCheckedChanged += (s, e) => vm.RequestPreviewUpdate();

        var switchBinarize = EditorChrome.MakeSwitch(nameof(vm.Binarize), Se.Language.Ocr.Binarize);
        switchBinarize.IsCheckedChanged += (s, e) => vm.RequestPreviewUpdate();

        var switchOneColor = EditorChrome.MakeSwitch(nameof(vm.ToOneColor), Se.Language.Ocr.OneColor);
        switchOneColor.IsCheckedChanged += (s, e) => vm.RequestPreviewUpdate();
        var numericUpDownOneColorThreshold = EditorChrome.MakeField(nameof(vm.OneColorDarknessThreshold), 1, 255, 80)
            .WithBindEnabled(nameof(vm.ToOneColor))
            .WithAccessibleName(Se.Language.Ocr.DarknessThreshold);
        numericUpDownOneColorThreshold.ValueChanged += (s, e) => vm.RequestPreviewUpdate();

        var switchRemoveBorders = EditorChrome.MakeSwitch(nameof(vm.RemoveBorders), Se.Language.Ocr.RemoveBorders);
        switchRemoveBorders.IsCheckedChanged += (s, e) => vm.RequestPreviewUpdate();
        var numericUpDownBorderSize = EditorChrome.MakeField(nameof(vm.BorderSize), 1, 20, 80)
            .WithBindEnabled(nameof(vm.RemoveBorders))
            .WithAccessibleName(Se.Language.Ocr.BorderSize);
        numericUpDownBorderSize.ValueChanged += (s, e) => vm.RequestPreviewUpdate();

        // Switches in two columns: the on/off options left, the ones with a value right
        var options = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
            },
            ColumnSpacing = 24,
            RowSpacing = 6,
        };
        options.Add(BinaryOcrChrome.MakeSwitchRow(switchCropTransparent, Se.Language.Ocr.CropTransparent), 0, 0);
        options.Add(BinaryOcrChrome.MakeSwitchRow(switchInverseColors, Se.Language.Ocr.InverseColors), 1, 0);
        options.Add(BinaryOcrChrome.MakeSwitchRow(switchBinarize, Se.Language.Ocr.Binarize), 2, 0);
        options.Add(MakeOptionWithValue(switchOneColor, Se.Language.Ocr.OneColor, Se.Language.Ocr.DarknessThreshold, numericUpDownOneColorThreshold), 0, 1);
        options.Add(MakeOptionWithValue(switchRemoveBorders, Se.Language.Ocr.RemoveBorders, Se.Language.Ocr.BorderSize, numericUpDownBorderSize), 1, 1);

        var body = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
        };
        body.Add(EditorChrome.MakeEditorSection(Se.Language.General.Options, options, first: true), 0, 0);
        body.Add(EditorChrome.MakeDockedFillSection(Se.Language.General.Before, MakeOriginalImageView(vm)), 1, 0);
        body.Add(EditorChrome.MakeDockedFillSection(Se.Language.General.After, MakePostProcessedImageView(vm)), 2, 0);

        var buttonOk = EditorChrome.AsFlushPrimary(UiUtil.MakeButtonOk(vm.OkCommand));
        var buttonCancel = EditorChrome.WithoutMargin(UiUtil.MakeButtonCancel(vm.CancelCommand));
        var buttonReset = EditorChrome.MakeTextToolButton(IconNames.Restore, Se.Language.General.Reset, vm.ResetCommand);
        buttonReset.HorizontalAlignment = HorizontalAlignment.Left;
        Content = EditorChrome.MakeRoot(body, EditorChrome.MakeDockedButtonBar(buttonReset, buttonCancel, buttonOk));

        // initial focus on an input, not an action button - a focused button clicks on bare Space
        UiUtil.FocusOnFirstActivation(this, switchCropTransparent);
        KeyDown += vm.KeyDown;
    }

    private static StackPanel MakeOptionWithValue(ToggleSwitch toggle, string text, string valueLabel, Control field)
    {
        var row = BinaryOcrChrome.MakeSwitchRow(toggle, text);
        row.Children.Add(EditorChrome.MakeDimLabel(valueLabel, new Thickness(8, 0, 0, 0)));
        row.Children.Add(field);
        return row;
    }

    private static Control MakeOriginalImageView(PreProcessingViewModel vm)
    {
        var image = new Image
        {
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
            Source = vm.SourceBitmap,
            MaxHeight = 200,
        };

        return BinaryOcrChrome.MakeImageFrame(image, UiUtil.GetCheckerboardBrush());
    }

    private static Border MakePostProcessedImageView(PreProcessingViewModel vm)
    {
        var image = new Image
        {
            [!Image.SourceProperty] = new Binding(nameof(vm.PreviewImage)),
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Stretch = Stretch.Uniform,
            MaxHeight = 200,
        };

        return BinaryOcrChrome.MakeImageFrame(image, UiUtil.GetCheckerboardBrush());
    }
}
