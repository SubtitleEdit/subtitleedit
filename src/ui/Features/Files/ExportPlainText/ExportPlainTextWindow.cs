using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportPlainText;

/// <summary>
/// Export plain text, in the editor look (see <see cref="EditorChrome"/>): the options (shared
/// with batch convert via PlainTextFormatter, #16027) in sections on the left, the preview on the
/// right, the encoding plus Done and Save as in the button bar - or Cancel and OK in batch mode.
/// </summary>
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
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Dialog);

        // Encoding + buttons
        var labelEncoding = EditorChrome.MakeDimLabel(Se.Language.General.Encoding, new Thickness(0, 0, 8, 0));
        var comboBoxEncoding = UiUtil.MakeComboBox(vm.Encodings, vm, nameof(vm.SelectedEncoding))
            .WithMinWidth(180)
            .WithAccessibleName(Se.Language.General.Encoding);
        var panelEncoding = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Children = { labelEncoding, comboBoxEncoding },
        }.WithBindIsVisible(nameof(vm.IsNotBatchMode));

        var buttonSaveAs = UiUtil.MakeButton(Se.Language.General.SaveDotDotDot, vm.SaveAsCommand)
            .WithBindIsVisible(nameof(vm.IsNotBatchMode));
        buttonSaveAs.Classes.Add("primary");
        var buttonDone = UiUtil.MakeButtonDone(vm.CancelCommand)
            .WithBindIsVisible(nameof(vm.IsNotBatchMode));

        // Batch convert mode: OK stores the options, Cancel leaves them as they were.
        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand)
            .WithBindIsVisible(nameof(vm.IsBatchMode));
        buttonOk.Classes.Add("primary");
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand)
            .WithBindIsVisible(nameof(vm.IsBatchMode));

        // The accept button answers Enter (#14586) - only the visible one, so a hidden Save as
        // never runs in batch mode.
        buttonSaveAs.Bind(Button.IsDefaultProperty, new Binding(nameof(vm.IsNotBatchMode)));
        buttonDone.Bind(Button.IsDefaultProperty, new Binding(nameof(vm.IsNotBatchMode)));
        buttonOk.Bind(Button.IsDefaultProperty, new Binding(nameof(vm.IsBatchMode)));

        var sections = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto, MinWidth = 300 },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
        };
        sections.Add(MakeSettingsView(vm, out var firstSettingsControl), 0, 0);
        sections.Add(MakePreviewView(vm), 0, 1);

        Content = EditorChrome.MakeRoot(sections, EditorChrome.MakeDockedButtonBar(panelEncoding, buttonDone, buttonSaveAs, buttonCancel, buttonOk));

        UiUtil.FocusOnFirstActivation(this, () => (vm.IsBatchMode ? firstSettingsControl : comboBoxEncoding).Focus()); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.OnKeyDown;
        Loaded += delegate { UiUtil.RestoreWindowPosition(this); };
        Closing += delegate { UiUtil.SaveWindowPosition(this); };
    }

    private static ScrollViewer MakeSettingsView(ExportPlainTextViewModel vm, out Control firstSettingsControl)
    {
        // Line numbers section
        var rowShowLineNumbers = EditorChrome.MakeToggleRow(nameof(vm.ShowLineNumbers), Se.Language.File.Export.ShowLineNumbers, out var switchShowLineNumbers);
        switchShowLineNumbers.IsCheckedChanged += (s, e) => vm.SetDirty();
        firstSettingsControl = switchShowLineNumbers;
        var rowAddNewLineAfterLineNumber = EditorChrome.MakeToggleRow(nameof(vm.AddNewLineAfterLineNumber), Se.Language.File.Export.AddNewLineAfterLineNumber, out var switchAddNewLineAfterLineNumber);
        rowAddNewLineAfterLineNumber.Bind(IsEnabledProperty, new Binding(nameof(vm.ShowLineNumbers)));
        switchAddNewLineAfterLineNumber.IsCheckedChanged += (s, e) => vm.SetDirty();

        // Time codes section
        var rowShowTimeCodes = EditorChrome.MakeToggleRow(nameof(vm.ShowTimeCodes), Se.Language.General.ShowTimeCodes, out var switchShowTimeCodes);
        switchShowTimeCodes.IsCheckedChanged += (s, e) => vm.SetDirty();
        var comboBoxTimeCodeFormat = UiUtil.MakeComboBox(vm.TimeCodeFormats, vm, nameof(vm.SelectedTimeCodeFormat))
            .BindIsEnabled(vm, nameof(vm.ShowTimeCodes))
            .WithAccessibleName(Se.Language.General.Format);
        comboBoxTimeCodeFormat.MinWidth = 160;
        comboBoxTimeCodeFormat.SelectionChanged += (_,_) => vm.SetDirty();
        var comboBoxTimeCodeSeparator = UiUtil.MakeComboBox(vm.TimeCodeSeparators, vm, nameof(vm.SelectedTimeCodeSeparator))
            .BindIsEnabled(vm, nameof(vm.ShowTimeCodes))
            .WithAccessibleName(Se.Language.General.Separator);
        comboBoxTimeCodeSeparator.MinWidth = 160;
        comboBoxTimeCodeSeparator.SelectionChanged += (sender, args) => vm.SetDirty();
        var gridTimeCodeFields = EditorChrome.MakeFieldGrid((Se.Language.General.Format, comboBoxTimeCodeFormat), (Se.Language.General.Separator, comboBoxTimeCodeSeparator));
        var rowAddNewLineAfterTimeCode = EditorChrome.MakeToggleRow(nameof(vm.AddNewLineAfterTimeCode), Se.Language.File.Export.AddNewLineAfterTimeCode, out var switchAddNewLineAfterTimeCode);
        rowAddNewLineAfterTimeCode.Bind(IsEnabledProperty, new Binding(nameof(vm.ShowTimeCodes)));
        switchAddNewLineAfterTimeCode.IsCheckedChanged += (sender, args) => vm.SetDirty();

        // Format text section
        var radioButtonFormatTextNone = UiUtil.MakeRadioButton(Se.Language.General.DoNoChange, vm, nameof(vm.FormatTextNone), "formatText");
        radioButtonFormatTextNone.IsCheckedChanged += (sender, args) => vm.SetDirty();
        var radioButtonFormatTextMerge = UiUtil.MakeRadioButton(Se.Language.File.Export.MergeAllLines, vm, nameof(vm.FormatTextMerge), "formatText");
        radioButtonFormatTextMerge.IsCheckedChanged += (sender, args) => vm.SetDirty();
        var radioButtonFormatTextUnbreak = UiUtil.MakeRadioButton(Se.Language.General.UnbreakLines, vm, nameof(vm.FormatTextUnbreak), "formatText");
        radioButtonFormatTextUnbreak.IsCheckedChanged += (sender, args) => vm.SetDirty();
        var panelFormatText = new StackPanel
        {
            Spacing = 0,
            Children = { radioButtonFormatTextNone, radioButtonFormatTextMerge, radioButtonFormatTextUnbreak },
        };
        foreach (var radioButton in panelFormatText.Children)
        {
            ((RadioButton)radioButton).FontSize = 12.5;
        }

        var rowRemoveStyling = EditorChrome.MakeToggleRow(nameof(vm.TextRemoveStyling), Se.Language.General.RemoveStyling, out var switchRemoveStyling);
        switchRemoveStyling.IsCheckedChanged += (sender, args) => vm.SetDirty();
        var rowAddLineAfterText = EditorChrome.MakeToggleRow(nameof(vm.AddLineAfterText), Se.Language.File.Export.AddNewLineAfterText, out var switchAddLineAfterText);
        switchAddLineAfterText.IsCheckedChanged += (sender, args) => vm.SetDirty();

        // Spacing section
        var rowAddLineBetweenSubtitles = EditorChrome.MakeToggleRow(nameof(vm.AddLineBetweenSubtitles), Se.Language.File.Export.AddLineBetweenSubtitles, out var switchAddLineBetweenSubtitles);
        switchAddLineBetweenSubtitles.IsCheckedChanged += (sender, args) => vm.SetDirty();

        static StackPanel Stack(params Control[] controls)
        {
            var panel = new StackPanel { Spacing = 8 };
            panel.Children.AddRange(controls);
            return panel;
        }

        var sections = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.File.Export.LineNumbers, Stack(rowShowLineNumbers, rowAddNewLineAfterLineNumber), first: true),
                EditorChrome.MakeEditorSection(Se.Language.General.TimeCodes, Stack(rowShowTimeCodes, gridTimeCodeFields, rowAddNewLineAfterTimeCode)),
                EditorChrome.MakeEditorSection(Se.Language.General.Text, Stack(panelFormatText, rowRemoveStyling, rowAddLineAfterText)),
                EditorChrome.MakeEditorSection(Se.Language.General.Spacing, Stack(rowAddLineBetweenSubtitles)),
            },
        };

        // Scrolls on short screens, so the button bar stays reachable
        return new ScrollViewer
        {
            Content = sections,
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        };
    }

    private static Border MakePreviewView(ExportPlainTextViewModel vm)
    {
        var textBox = EditorChrome.AsTextArea(new TextBox
        {
            AcceptsReturn = true,
            // Read-only preview - see ManualChosenEncodingWindow (#14313).
            AcceptsTab = false,
            IsReadOnly = true,
            TextWrapping = TextWrapping.Wrap,
            Width = double.NaN,
            Height = double.NaN,
        }).WithAccessibleName(Se.Language.General.Preview);
        textBox.Bind(TextBox.TextProperty, new Binding(nameof(vm.PreviewText)));

        return EditorChrome.MakeFillSection(Se.Language.General.Preview, textBox, lineLeft: true);
    }
}
