using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.SetText;

/// <summary>
/// Render a text as the subtitle image, in the editor look (see <see cref="EditorChrome"/>): the
/// text, the font and box settings, the rendered preview, Cancel/OK at the bottom.
/// </summary>
public class SetTextWindow : Window
{
    public SetTextWindow(SetTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Tools.ImageBasedEdit.SetText;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = true;
        MinWidth = 600;
        MinHeight = 400;
        BinaryEditChrome.Apply(this);
        vm.Window = this;
        DataContext = vm;

        var textView = MakeTextView(vm);
        var fontView = MakeFontView(vm);
        var previewView = MakePreviewView(vm);
        var buttonPanel = MakeButtonPanel(vm);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Text input
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Font settings
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }, // Preview
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };

        grid.Add(textView, 0);
        grid.Add(fontView, 1);
        grid.Add(previewView, 2);

        Content = EditorChrome.MakeRoot(grid, buttonPanel);

        KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Escape)
            {
                vm.CancelCommand.Execute(null);
                e.Handled = true;
            }
            else if (UiUtil.IsHelp(e))
            {
                e.Handled = true;
                UiUtil.ShowHelp("features/binary-edit", "tools");
            }
        };
        Closing += (_, _) => vm.Closing();
    }

    private static Border MakeTextView(SetTextViewModel vm)
    {
        var textBoxText = new TextBox
        {
            MinWidth = 550,
            MinHeight = 100,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 13,
            Padding = new Thickness(8, 6),
            CornerRadius = new CornerRadius(6),
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
        };
        textBoxText.Bind(TextBox.TextProperty, new Binding(nameof(vm.Text)) { Mode = BindingMode.TwoWay });
        AutomationProperties.SetName(textBoxText, Se.Language.General.Text);

        return EditorChrome.MakeEditorSection(Se.Language.General.Text, textBoxText, first: true);
    }

    private static Border MakeFontView(SetTextViewModel vm)
    {
        var comboBoxFontName = UiUtil.MakeComboBox(vm.FontNames, vm, nameof(vm.SelectedFontName))
            .WithMinWidth(200);
        comboBoxFontName.HorizontalAlignment = HorizontalAlignment.Stretch;

        var numericUpDownFontSize = UiUtil.MakeNumericUpDownInt(8, 200, 48, 90, vm, nameof(vm.FontSize));
        numericUpDownFontSize.Classes.Add("field");

        var switchBold = EditorChrome.MakeSwitchRow(nameof(vm.FontIsBold), Se.Language.General.Bold);

        var colorPickerFontColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.FontColor));
        var colorPickerOutlineColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.OutlineColor));
        var numericUpDownOutlineWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 50, 90, vm, nameof(vm.OutlineWidth));
        numericUpDownOutlineWidth.Classes.Add("field");
        var colorPickerShadowColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.ShadowColor));
        var numericUpDownShadowWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 50, 90, vm, nameof(vm.ShadowWidth));
        numericUpDownShadowWidth.Classes.Add("field");

        var comboBoxBoxType = UiUtil.MakeComboBox(vm.BoxTypes, vm, nameof(vm.SelectedBoxType))
            .WithMinWidth(150);
        comboBoxBoxType.HorizontalAlignment = HorizontalAlignment.Left;
        var colorPickerBackgroundColor = UiUtil.MakeColorPickerButton(vm, nameof(vm.BackgroundColor));

        // The width fields belong to the color before them - name them after both
        AutomationProperties.SetName(comboBoxFontName, Se.Language.General.FontName);
        AutomationProperties.SetName(numericUpDownFontSize, Se.Language.General.Size);
        AutomationProperties.SetName(colorPickerFontColor, Se.Language.General.TextColor);
        AutomationProperties.SetName(colorPickerOutlineColor, Se.Language.General.OutlineColor);
        AutomationProperties.SetName(numericUpDownOutlineWidth, Se.Language.General.OutlineColor + " - " + Se.Language.General.Width);
        AutomationProperties.SetName(colorPickerShadowColor, Se.Language.General.Shadow);
        AutomationProperties.SetName(numericUpDownShadowWidth, Se.Language.General.Shadow + " - " + Se.Language.General.Width);
        AutomationProperties.SetName(comboBoxBoxType, Se.Language.Video.BurnIn.BoxType);
        AutomationProperties.SetName(colorPickerBackgroundColor, Se.Language.General.BackgroundColor);

        foreach (var picker in new[] { colorPickerFontColor, colorPickerOutlineColor, colorPickerShadowColor, colorPickerBackgroundColor })
        {
            picker.HorizontalAlignment = HorizontalAlignment.Left;
        }

        var grid = new Grid
        {
            ColumnSpacing = 10,
            RowSpacing = 8,
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        for (var i = 0; i < 5; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        }

        static TextBlock Label(string text) => EditorChrome.MakeDimLabel(text.TrimEnd(':'), new Thickness(0));

        // Row 0: Font name and size
        grid.Add(Label(Se.Language.General.FontName), 0, 0);
        grid.Add(comboBoxFontName, 0, 1);
        grid.Add(Label(Se.Language.General.Size), 0, 2);
        grid.Add(numericUpDownFontSize, 0, 3);

        // Row 1: Bold and font color
        grid.Add(switchBold, 1, 1);
        grid.Add(Label(Se.Language.General.TextColor), 1, 2);
        grid.Add(colorPickerFontColor, 1, 3);

        // Row 2: Outline color and width
        grid.Add(Label(Se.Language.General.OutlineColor), 2, 0);
        grid.Add(colorPickerOutlineColor, 2, 1);
        grid.Add(Label(Se.Language.General.Width), 2, 2);
        grid.Add(numericUpDownOutlineWidth, 2, 3);

        // Row 3: Shadow color and width
        grid.Add(Label(Se.Language.General.Shadow), 3, 0);
        grid.Add(colorPickerShadowColor, 3, 1);
        grid.Add(Label(Se.Language.General.Width), 3, 2);
        grid.Add(numericUpDownShadowWidth, 3, 3);

        // Row 4: Box type and background color
        grid.Add(Label(Se.Language.Video.BurnIn.BoxType), 4, 0);
        grid.Add(comboBoxBoxType, 4, 1);
        grid.Add(Label(Se.Language.General.BackgroundColor), 4, 2);
        grid.Add(colorPickerBackgroundColor, 4, 3);

        return EditorChrome.MakeEditorSection(Se.Language.General.Font, grid);
    }

    private static Border MakePreviewView(SetTextViewModel vm)
    {
        var image = new Image
        {
            Stretch = Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        image.Bind(Image.SourceProperty, new Binding(nameof(vm.PreviewBitmap)));

        var scrollViewer = new ScrollViewer
        {
            Content = image,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            MinHeight = 200,
        };

        // The rendered text is a subtitle bitmap on transparency - put it on the shared image
        // backdrop, or white text vanishes on the light theme (#15111).
        var frame = BinaryEditChrome.MakePreviewFrame(scrollViewer, ImagePreviewBackground.CreateBrush());
        var grid = new Grid
        {
            RowSpacing = 10,
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(new GridLength(1, GridUnitType.Star)) },
        };
        grid.Add(EditorChrome.MakeSectionHeader(Se.Language.General.Preview), 0);
        grid.Add(frame, 1);
        return new Border
        {
            Child = grid,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(14, 12, 14, 14),
        };
    }

    private static Border MakeButtonPanel(SetTextViewModel vm)
    {
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return EditorChrome.MakeCancelOkBar(null, buttonCancel, buttonOk);
    }
}
