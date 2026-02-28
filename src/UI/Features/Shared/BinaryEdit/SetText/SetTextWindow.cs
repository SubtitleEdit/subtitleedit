using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.SetText;

public class SetTextWindow : Window
{
    public SetTextWindow(SetTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Set Text";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = true;
        MinWidth = 600;
        MinHeight = 400;
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
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) }, // Buttons
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
        };

        grid.Add(textView, 0);
        grid.Add(fontView, 1);
        grid.Add(previewView, 2);
        grid.Add(buttonPanel, 3);

        Content = grid;

        KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Escape)
            {
                vm.CancelCommand.Execute(null);
                e.Handled = true;
            }
        };
        Closing += (_, _) => vm.Closing();
    }

    private static Border MakeTextView(SetTextViewModel vm)
    {
        var labelText = UiUtil.MakeLabel("Text:");
        var textBoxText = new TextBox
        {
            MinWidth = 550,
            MinHeight = 100,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
        };
        textBoxText.Bind(TextBox.TextProperty, new Binding(nameof(vm.Text)) { Mode = BindingMode.TwoWay });

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Children =
            {
                labelText,
                textBoxText,
            }
        };

        return UiUtil.MakeBorderForControl(stackPanel);
    }

    private static Border MakeFontView(SetTextViewModel vm)
    {
        var labelFontName = UiUtil.MakeLabel(Se.Language.General.FontName);
        labelFontName.Margin = new Thickness(5);
        
        var comboBoxFontName = UiUtil.MakeComboBox(vm.FontNames, vm, nameof(vm.SelectedFontName))
            .WithMinWidth(200);
        comboBoxFontName.Margin = new Thickness(5);

        var labelFontSize = UiUtil.MakeLabel(Se.Language.General.Size);
        labelFontSize.Margin = new Thickness(5);
        
        var numericUpDownFontSize = UiUtil.MakeNumericUpDownInt(8, 200, 48, 120, vm, nameof(vm.FontSize));
        numericUpDownFontSize.Margin = new Thickness(5);

        var checkBoxBold = UiUtil.MakeCheckBox(Se.Language.General.Bold, vm, nameof(vm.FontIsBold));
        checkBoxBold.Margin = new Thickness(5);

        var labelFontColor = UiUtil.MakeLabel(Se.Language.General.TextColor);
        labelFontColor.Margin = new Thickness(5);
        
        var colorPickerFontColor = UiUtil.MakeColorPicker(vm, nameof(vm.FontColor));
        colorPickerFontColor.Margin = new Thickness(5);

        var labelOutlineColor = UiUtil.MakeLabel(Se.Language.General.OutlineColor);
        labelOutlineColor.Margin = new Thickness(5);
        
        var colorPickerOutlineColor = UiUtil.MakeColorPicker(vm, nameof(vm.OutlineColor));
        colorPickerOutlineColor.Margin = new Thickness(5);

        var labelOutlineWidth = UiUtil.MakeLabel(Se.Language.General.Width);
        labelOutlineWidth.Margin = new Thickness(5);
        
        var numericUpDownOutlineWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 50, 120, vm, nameof(vm.OutlineWidth));
        numericUpDownOutlineWidth.Margin = new Thickness(5);

        var labelShadowColor = UiUtil.MakeLabel(Se.Language.General.Shadow);
        labelShadowColor.Margin = new Thickness(5);
        
        var colorPickerShadowColor = UiUtil.MakeColorPicker(vm, nameof(vm.ShadowColor));
        colorPickerShadowColor.Margin = new Thickness(5);

        var labelShadowWidth = UiUtil.MakeLabel(Se.Language.General.Width);
        labelShadowWidth.Margin = new Thickness(5);
        
        var numericUpDownShadowWidth = UiUtil.MakeNumericUpDownOneDecimal(0, 50, 120, vm, nameof(vm.ShadowWidth));
        numericUpDownShadowWidth.Margin = new Thickness(5);

        var labelBoxType = UiUtil.MakeLabel("Box type:");
        labelBoxType.Margin = new Thickness(5);
        
        var comboBoxBoxType = UiUtil.MakeComboBox(vm.BoxTypes, vm, nameof(vm.SelectedBoxType))
            .WithMinWidth(150);
        comboBoxBoxType.Margin = new Thickness(5);

        var labelBackgroundColor = UiUtil.MakeLabel("Background color:");
        labelBackgroundColor.Margin = new Thickness(5);
        
        var colorPickerBackgroundColor = UiUtil.MakeColorPicker(vm, nameof(vm.BackgroundColor));
        colorPickerBackgroundColor.Margin = new Thickness(5);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };

        // Row 0: Font name and size
        grid.Add(labelFontName, 0, 0);
        grid.Add(comboBoxFontName, 0, 1);
        grid.Add(labelFontSize, 0, 2);
        grid.Add(numericUpDownFontSize, 0, 3);

        // Row 1: Bold and font color
        grid.Add(checkBoxBold, 1, 0, 1, 2);
        grid.Add(labelFontColor, 1, 2);
        grid.Add(colorPickerFontColor, 1, 3);

        // Row 2: Outline color and width
        grid.Add(labelOutlineColor, 2, 0);
        grid.Add(colorPickerOutlineColor, 2, 1);
        grid.Add(labelOutlineWidth, 2, 2);
        grid.Add(numericUpDownOutlineWidth, 2, 3);

        // Row 3: Shadow color and width
        grid.Add(labelShadowColor, 3, 0);
        grid.Add(colorPickerShadowColor, 3, 1);
        grid.Add(labelShadowWidth, 3, 2);
        grid.Add(numericUpDownShadowWidth, 3, 3);

        // Row 4: Box type and background color
        grid.Add(labelBoxType, 4, 0);
        grid.Add(comboBoxBoxType, 4, 1);
        grid.Add(labelBackgroundColor, 4, 2);
        grid.Add(colorPickerBackgroundColor, 4, 3);

        return UiUtil.MakeBorderForControl(grid);
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

        return UiUtil.MakeBorderForControl(scrollViewer);
    }

    private static Panel MakeButtonPanel(SetTextViewModel vm)
    {
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);

        return UiUtil.MakeButtonBar(buttonOk, buttonCancel);
    }
}

