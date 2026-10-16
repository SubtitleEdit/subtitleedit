using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SyntaxColorTooWideSettings;

/// <summary>
/// "Text too wide" measuring settings in the editor look (see <see cref="EditorChrome"/>): the
/// font and max width, then the sample with its preview, Cancel + OK in the button bar. The
/// preview image keeps its own colors.
/// </summary>
public class SyntaxColorTooWideSettingsWindow : Window
{
    private const double LabelWidth = 140;

    public SyntaxColorTooWideSettingsWindow(SyntaxColorTooWideSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.ColorTextTooWide;
        CanResize = true;
        Width = 660;
        MinWidth = 460;
        SizeToContent = SizeToContent.Height;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog | EditorWindowStyles.CompactListRows);
        vm.Window = this;
        DataContext = vm;

        // Font name
        var comboFont = UiUtil.MakeComboBox(vm.Fonts, vm, nameof(vm.SelectedFont))
            .WithMinWidth(220);
        comboFont.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(comboFont, Se.Language.General.FontName);
        comboFont.SelectionChanged += (_, _) => vm.MarkPreviewDirty();

        // Font size
        var numericFontSize = UiUtil.MakeNumericUpDownInt(4, 200, 24, 100, vm, nameof(vm.FontSize))
            .AsField(Se.Language.General.FontSize, themeForeground: true);
        numericFontSize.ValueChanged += (_, _) => vm.MarkPreviewDirty();

        // Max width
        var numericMaxWidth = UiUtil.MakeNumericUpDownInt(10, 9999, 560, 100, vm, nameof(vm.MaxWidthPixels))
            .AsField(Se.Language.General.MaxWidthPixels, themeForeground: true);
        numericMaxWidth.ValueChanged += (_, _) => vm.MarkPreviewDirty();

        var measureContent = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.FontName, comboFont, LabelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.FontSize, numericFontSize, LabelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.MaxWidthPixels, numericMaxWidth, LabelWidth),
            },
        };

        // Sample text
        var textBoxSampleText = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SampleText)) { Mode = BindingMode.TwoWay },
        }.AsTextField(Se.Language.General.SampleText);
        textBoxSampleText.TextChanged += (_, _) => vm.MarkPreviewDirty();

        // Box width
        var numericBoxWidth = UiUtil.MakeNumericUpDownInt(100, 9999, 720, 100, vm, nameof(vm.SampleBoxWidth))
            .AsField(Se.Language.General.BoxWidthPixels, themeForeground: true);
        numericBoxWidth.ValueChanged += (_, _) => vm.MarkPreviewDirty();

        // Preview image
        var previewImage = new Image
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            DataContext = vm,
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
        };
        var previewBorder = new Border
        {
            Child = previewImage,
            MinHeight = 60,
            Padding = new Thickness(4),
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            ClipToBounds = true,
        };

        // Hint
        var hint = EditorChrome.MakeWrappingDimLabel(Se.Language.Options.Settings.PixelWidthInfo);

        var sampleContent = new StackPanel
        {
            Spacing = 8,
            Children =
            {
                EditorChrome.MakeLabeledRow(Se.Language.General.SampleText, textBoxSampleText, LabelWidth),
                EditorChrome.MakeLabeledRow(Se.Language.General.BoxWidthPixels, numericBoxWidth, LabelWidth),
                previewBorder,
                hint,
            },
        };

        var sections = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.Font, measureContent, first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.Preview, sampleContent),
            },
        };

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(null, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, comboFont); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
