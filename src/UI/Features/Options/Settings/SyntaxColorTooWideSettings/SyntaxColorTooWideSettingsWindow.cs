using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SyntaxColorTooWideSettings;

public class SyntaxColorTooWideSettingsWindow : Window
{
    public SyntaxColorTooWideSettingsWindow(SyntaxColorTooWideSettingsViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Settings.ColorTextTooWide;
        CanResize = true;
        Width = 660;
        MinWidth = 460;
        SizeToContent = SizeToContent.Height;
        vm.Window = this;
        DataContext = vm;

        // Row 0 – font name
        var labelFont = UiUtil.MakeLabel(Se.Language.General.FontName);
        var comboFont = UiUtil.MakeComboBox(vm.Fonts, vm, nameof(vm.SelectedFont))
            .WithMinWidth(200);
        comboFont.SelectionChanged += (_, _) => vm.MarkPreviewDirty();

        // Row 1 – font size
        var labelFontSize = UiUtil.MakeLabel(Se.Language.General.FontSize);
        var numericFontSize = UiUtil.MakeNumericUpDownInt(4, 200, 24, 100, vm, nameof(vm.FontSize));
        numericFontSize.ValueChanged += (_, _) => vm.MarkPreviewDirty();

        // Row 2 – max width
        var labelMaxWidth = UiUtil.MakeLabel("Max width (px):");
        var numericMaxWidth = UiUtil.MakeNumericUpDownInt(10, 9999, 560, 120, vm, nameof(vm.MaxWidthPixels));
        numericMaxWidth.ValueChanged += (_, _) => vm.MarkPreviewDirty();

        // Row 3 – separator

        // Row 4 – sample text
        var labelSampleText = UiUtil.MakeLabel("Sample text:");
        var textBoxSampleText = new TextBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            [!TextBox.TextProperty] = new Binding(nameof(vm.SampleText)) { Mode = BindingMode.TwoWay },
        };
        textBoxSampleText.TextChanged += (_, _) => vm.MarkPreviewDirty();

        // Row 5 – box width
        var labelBoxWidth = UiUtil.MakeLabel("Box width (px):");
        var numericBoxWidth = UiUtil.MakeNumericUpDownInt(100, 9999, 720, 120, vm, nameof(vm.SampleBoxWidth));
        numericBoxWidth.ValueChanged += (_, _) => vm.MarkPreviewDirty();

        // Row 6 – preview image
        var previewImage = new Image
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            DataContext = vm,
            [!Image.SourceProperty] = new Binding(nameof(vm.ImagePreview)),
        };
        var previewBorder = UiUtil.MakeBorderForControl(previewImage).WithMinHeight(60);

        // Row 7 – hint
        var hint = new TextBlock
        {
            Text = "Green lines = max-width limit   |   Red area = text exceeds limit",
            FontStyle = FontStyle.Italic,
            Opacity = 0.65,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };

        // Row 8 – buttons
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // font
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // font size
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // max width
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // separator
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // sample text
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // box width
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // preview
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // hint
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 8,
            RowSpacing = 8,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(labelFont, 0, 0);
        grid.Add(comboFont, 0, 1);
        grid.Add(labelFontSize, 1, 0);
        grid.Add(numericFontSize, 1, 1);
        grid.Add(labelMaxWidth, 2, 0);
        grid.Add(numericMaxWidth, 2, 1);
        grid.Add(UiUtil.MakeVerticalSeperator(), 3, 0, 1, 2);
        grid.Add(labelSampleText, 4, 0);
        grid.Add(textBoxSampleText, 4, 1);
        grid.Add(labelBoxWidth, 5, 0);
        grid.Add(numericBoxWidth, 5, 1);
        grid.Add(previewBorder, 6, 0, 1, 2);
        grid.Add(hint, 7, 0, 1, 2);
        grid.Add(panelButtons, 8, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); };
        KeyDown += (_, e) => vm.OnKeyDown(e);
    }
}
