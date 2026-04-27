using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportPlainText;

public class ExportPlainTextWindow : Window
{
    public ExportPlainTextWindow(ExportPlainTextViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.File.Export.TitleExportPlainText;
        CanResize = true;
        Width = 900;
        Height = 800;
        MinWidth = 600;
        MinHeight = 400;
        vm.Window = this;
        DataContext = vm;

        // Encoding section + buttons
        var labelEncoding = UiUtil.MakeLabel(Se.Language.General.Encoding).WithBold().WithMarginTop(10);
        var comboBoxEncoding = UiUtil.MakeComboBox(vm.Encodings, vm, nameof(vm.SelectedEncoding))
            .WithMinWidth(180)
            .WithMarginRight(10);
        var buttonSaveAs = UiUtil.MakeButton(Se.Language.General.SaveDotDotDot, vm.SaveAsCommand);
        var buttonDone = UiUtil.MakeButtonDone(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar( labelEncoding, comboBoxEncoding, buttonSaveAs, buttonDone);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(MakeSettingsView(vm), 0);
        grid.Add(MakePreviewView(vm), 0, 1);
        grid.Add(panelButtons, 2, 0, 1, 2);

        Content = grid;

        Activated += delegate { buttonSaveAs.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
    }

    private static Grid MakeSettingsView(ExportPlainTextViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        
        grid.Add(UiUtil.MakeLabel(Se.Language.General.Settings), 0);

        // Line numbers section
        var labelLineNumbers = UiUtil.MakeLabel(Se.Language.File.Export.LineNumbers).WithBold().WithMarginTop(10);
        var checkBoxShowLineNumbers = UiUtil.MakeCheckBox(Se.Language.File.Export.ShowLineNumbers, vm, nameof(vm.ShowLineNumbers));
        checkBoxShowLineNumbers.IsCheckedChanged += (s, e) => vm.SetDirty();
        var checkBoxAddNewLineAfterLineNumber = UiUtil.MakeCheckBox(Se.Language.File.Export.AddNewLineAfterLineNumber, vm, nameof(vm.AddNewLineAfterLineNumber))
            .WithBindEnabled(nameof(vm.ShowLineNumbers));
        checkBoxAddNewLineAfterLineNumber.IsCheckedChanged += (s, e) => vm.SetDirty();

        // Time codes section
        var labelTimeCodes = UiUtil.MakeLabel(Se.Language.General.TimeCodes).WithBold().WithMarginTop(10);
        var checkBoxShowTimeCodes = UiUtil.MakeCheckBox(Se.Language.General.ShowTimeCodes, vm, nameof(vm.ShowTimeCodes));
        checkBoxShowTimeCodes.IsCheckedChanged += (s, e) => vm.SetDirty();
        var labelTimeCodeFormat = UiUtil.MakeLabel(Se.Language.General.Format);
        var comboBoxTimeCodeFormat = UiUtil.MakeComboBox(vm.TimeCodeFormats, vm, nameof(vm.SelectedTimeCodeFormat))
            .BindIsEnabled(vm, nameof(vm.ShowTimeCodes));
        comboBoxTimeCodeFormat.SelectionChanged += (_,_) => vm.SetDirty();
        var panelTimeCodeFormat = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                labelTimeCodeFormat,
                comboBoxTimeCodeFormat,
            }
        };
        var labelTimeCodeSeparator = UiUtil.MakeLabel(Se.Language.General.Separator);
        var comboBoxTimeCodeSeparator = UiUtil.MakeComboBox(vm.TimeCodeSeparators, vm, nameof(vm.SelectedTimeCodeSeparator))
            .BindIsEnabled(vm, nameof(vm.ShowTimeCodes));
        comboBoxTimeCodeSeparator.SelectionChanged += (sender, args) => vm.SetDirty();
        var panelTimeCodeSeparator = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5,
            Children =
            {
                labelTimeCodeSeparator,
                comboBoxTimeCodeSeparator,
            }
        };
        var checkBoxAddNewLineAfterTimeCode = UiUtil.MakeCheckBox(Se.Language.File.Export.AddNewLineAfterTimeCode, vm, nameof(vm.AddNewLineAfterTimeCode))
            .WithBindEnabled(nameof(vm.ShowTimeCodes));
        checkBoxAddNewLineAfterTimeCode.IsCheckedChanged += (sender, args) => vm.SetDirty(); 

        // Format text section
        var labelFormatText = UiUtil.MakeLabel(Se.Language.General.Text).WithBold().WithMarginTop(10);
        var radioButtonFormatTextNone = UiUtil.MakeRadioButton(Se.Language.General.DoNoChange, vm, nameof(vm.FormatTextNone), "formatText");
        radioButtonFormatTextNone.IsCheckedChanged += (sender, args) => vm.SetDirty();
        var radioButtonFormatTextMerge = UiUtil.MakeRadioButton(Se.Language.General.MergeLines, vm, nameof(vm.FormatTextMerge), "formatText");
        radioButtonFormatTextMerge.IsCheckedChanged += (sender, args) => vm.SetDirty();
        var radioButtonFormatTextUnbreak = UiUtil.MakeRadioButton(Se.Language.General.UnbreakLines, vm, nameof(vm.FormatTextUnbreak), "formatText");
        radioButtonFormatTextUnbreak.IsCheckedChanged += (sender, args) => vm.SetDirty();
        var checkBoxRemoveStyling = UiUtil.MakeCheckBox(Se.Language.General.RemoveStyling, vm, nameof(vm.TextRemoveStyling));
        checkBoxRemoveStyling.IsCheckedChanged += (sender, args) => vm.SetDirty();

        // Spacing section
        var labelSpacing = UiUtil.MakeLabel(Se.Language.General.Spacing).WithBold().WithMarginTop(10);
        var checkBoxAddLineAfterText = UiUtil.MakeCheckBox(Se.Language.File.Export.AddNewLineAfterText, vm, nameof(vm.AddLineAfterText));
        checkBoxAddLineAfterText.IsCheckedChanged += (sender, args) => vm.SetDirty();
        var checkBoxAddLineBetweenSubtitles = UiUtil.MakeCheckBox(Se.Language.File.Export.AddLineBetweenSubtitles, vm, nameof(vm.AddLineBetweenSubtitles));
        checkBoxAddLineBetweenSubtitles.IsCheckedChanged += (sender, args) => vm.SetDirty();
        
        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 5,
            Children =
            {
                // line numbers
                labelLineNumbers,
                checkBoxShowLineNumbers,
                checkBoxAddNewLineAfterLineNumber,

                // time codes
                labelTimeCodes,
                checkBoxShowTimeCodes,
                panelTimeCodeFormat,
                panelTimeCodeSeparator,
                checkBoxAddNewLineAfterTimeCode,

                // format text
                labelFormatText,
                radioButtonFormatTextNone,
                radioButtonFormatTextMerge,
                radioButtonFormatTextUnbreak,
                checkBoxRemoveStyling,
                checkBoxAddLineAfterText,

                // spacing
                labelSpacing,
                checkBoxAddLineBetweenSubtitles,
            },
        };

        grid.Add(UiUtil.MakeBorderForControl(stackPanel), 1);
        
        return grid;
    }

    private static Grid MakePreviewView(ExportPlainTextViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(UiUtil.MakeLabel(Se.Language.General.Preview), 0);
        var textBox = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = true,
            IsReadOnly = true,
            Width = double.NaN,
            Height = double.NaN,
        };
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PreviewText)));

        grid.Add(textBox, 1);

        return grid;
    }
}
