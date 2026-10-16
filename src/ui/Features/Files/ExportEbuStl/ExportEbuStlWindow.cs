using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Files.Export.ExportEbuStl;
using Nikse.SubtitleEdit.Features.Files.FormatProperties;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportEbuStl;

/// <summary>
/// EBU STL save options in the editor look (see <see cref="FormatPropertiesChrome"/>): the GSI
/// header, text and timing, and the errors found, as tabs over the panel; Import in the button bar.
/// </summary>
public class ExportEbuStlWindow : Window
{
    private const double FieldWidth = 225;

    // All tabs get the size of the biggest one, so switching tabs does not resize the window
    private const double TabMinWidth = 932;
    private const double TabMinHeight = 556;

    public ExportEbuStlWindow(ExportEbuStlViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = Se.Language.Options.Shortcuts.FileExportEbuStl;
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        EditorChrome.ApplyWindow(this, EditorWindowStyles.Form);
        vm.Window = this;
        DataContext = vm;

        var tabControl = new TabControl { Padding = new Thickness(0) };
        var tabGeneral = new TabItem
        {
            Header = Se.Language.General.General,
            Content = MakeGeneralView(vm, out var comboBoxCodePage),
        };
        tabControl.Items.Add(tabGeneral);

        var tabTextAndTiming = new TabItem
        {
            Header = Se.Language.File.EbuSaveOptions.TextAndTimingInformation,
            Content = MakeTextAndTimingView(vm),
        };
        tabControl.Items.Add(tabTextAndTiming);

        var tabErrors = new TabItem
        {
            [!HeaderedContentControl.HeaderProperty] = new Binding(nameof(vm.ErrorTitle)) { Source = vm },
            Content = MakeErrorsView(vm),
        };
        tabControl.Items.Add(tabErrors);

        var buttonImport = FormatPropertiesChrome.MakeAction("fa-solid fa-file-import", Se.Language.General.ImportDotDotDot, vm.ImportCommand);
        var buttonBar = EditorChrome.MakeDockedCancelOkBar(buttonImport, UiUtil.MakeButtonCancel(vm.CancelCommand), UiUtil.MakeButtonOk(vm.OkCommand));

        Content = FormatPropertiesChrome.MakeContentRoot(new Border { Child = tabControl, Padding = new Thickness(8, 4, 8, 0) }, buttonBar);

        UiUtil.FocusOnFirstActivation(this, comboBoxCodePage); // initial focus on an input, not an action button - a focused button clicks on bare Space
        KeyDown += vm.OnKeyDown;
    }

    private static Control MakeGeneralView(ExportEbuStlViewModel vm, out ComboBox comboBoxCodePage)
    {
        var l = Se.Language.File.EbuSaveOptions;

        // Format
        comboBoxCodePage = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.CodePages, vm, nameof(vm.SelectedCodePage)), l.CodePageNumber, FieldWidth);
        var comboboxDiskFormatCodes = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.DiskFormatCodes, vm, nameof(vm.SelectedDiskFormatCode)), l.DiskFormatCode, FieldWidth);
        var comboBoxFrameRates = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.FrameRates, vm, nameof(vm.SelectedFrameRate)), Se.Language.General.FrameRate, FieldWidth);
        var comboBoxDisplayStandardCodes = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.DisplayStandardCodes, vm, nameof(vm.SelectedDisplayStandardCode)), l.DisplayStandardCode, FieldWidth);
        var comboBoxCharacterTables = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.CharacterTables, vm, nameof(vm.SelectedCharacterTable)), l.CharacterCodeTable, FieldWidth);
        var comboBoxLanguageCodes = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.LanguageCodes, vm, nameof(vm.SelectedLanguageCode)), l.LanguageCode, FieldWidth);
        var format = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(l.CodePageNumber, comboBoxCodePage),
            FormatPropertiesChrome.Row(l.DiskFormatCode, comboboxDiskFormatCodes),
            FormatPropertiesChrome.Row(Se.Language.General.FrameRate, comboBoxFrameRates),
            FormatPropertiesChrome.Row(l.DisplayStandardCode, comboBoxDisplayStandardCodes),
            FormatPropertiesChrome.Row(l.CharacterCodeTable, comboBoxCharacterTables),
            FormatPropertiesChrome.Row(l.LanguageCode, comboBoxLanguageCodes));

        // Programme
        var textBoxOriginalProgramTitle = HeaderText(vm, nameof(vm.OriginalProgramTitle), l.OriginalProgramTitle, 32);
        var textBoxOriginalEpisodeTitle = HeaderText(vm, nameof(vm.OriginalEpisodeTitle), l.OriginalEpisodeTitle, 32);
        var textBoxTranslatedProgramTitle = HeaderText(vm, nameof(vm.TranslatedProgramTitle), l.TranslatedProgramTitle, 32);
        var textBoxTranslatedEpisodeTitle = HeaderText(vm, nameof(vm.TranslatedEpisodeTitle), l.TranslatedEpisodeTitle, 32);
        var textBoxTranslatorsName = HeaderText(vm, nameof(vm.TranslatorsName), l.TranslatorsName, 32);
        var textBoxSubtitleListReferenceCode = HeaderText(vm, nameof(vm.SubtitleListReferenceCode), l.SubtitleListReferenceCode, 16);
        var textBoxCountryOfOrigin = HeaderText(vm, nameof(vm.CountryOfOrigin), l.CountryOfOrigin, 3);
        var programme = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(l.OriginalProgramTitle, textBoxOriginalProgramTitle),
            FormatPropertiesChrome.Row(l.OriginalEpisodeTitle, textBoxOriginalEpisodeTitle),
            FormatPropertiesChrome.Row(l.TranslatedProgramTitle, textBoxTranslatedProgramTitle),
            FormatPropertiesChrome.Row(l.TranslatedEpisodeTitle, textBoxTranslatedEpisodeTitle),
            FormatPropertiesChrome.Row(l.TranslatorsName, textBoxTranslatorsName),
            FormatPropertiesChrome.Row(l.SubtitleListReferenceCode, textBoxSubtitleListReferenceCode),
            FormatPropertiesChrome.Row(l.CountryOfOrigin, textBoxCountryOfOrigin));

        // Time code
        var comboBoxTimeCodeStatus = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.TimeCodeStatusList, vm, nameof(vm.SelectedTimeCodeStatus)), l.TimeCodeStatus, FieldWidth);
        var timeCodeUpDownStartOfProgramme = new TimeCodeUpDown
        {
            DataContext = vm,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.StartOfProgramme))
            {
                Mode = BindingMode.TwoWay,
            }
        };
        AutomationProperties.SetName(timeCodeUpDownStartOfProgramme, l.TimeCodeStartOfProgramme);
        var timeCode = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(l.TimeCodeStatus, comboBoxTimeCodeStatus),
            FormatPropertiesChrome.Row(l.TimeCodeStartOfProgramme, timeCodeUpDownStartOfProgramme));

        // Disc and display
        var comboBoxRevisionNumbers = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.RevisionNumbers, vm, nameof(vm.SelectedRevisionNumber)), l.RevisionNumber, 120);
        var comboBoxMaxCharsPerRowList = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.MaxCharactersPerRow, vm, nameof(vm.SelectedMaxCharactersPerRow)), l.MaxNoOfDisplayableChars, 120);
        var comboBoxMaxRows = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.MaxRows, vm, nameof(vm.SelectedMaxRow)), l.MaxNumberOfDisplayableRows, 120);
        var comboBoxDiscSequenceNumbers = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.DiscSequenceNumbers, vm, nameof(vm.SelectedDiscSequenceNumber)), l.DiscSequenceNumber, 120);
        var comboBoxTotalNumberOfDisks = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.TotalNumerOfDiscsList, vm, nameof(vm.SelectedTotalNumberOfDiscs)), l.TotalNumberOfDisks, 120);
        var disc = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(l.RevisionNumber, comboBoxRevisionNumbers),
            FormatPropertiesChrome.Row(l.MaxNoOfDisplayableChars, comboBoxMaxCharsPerRowList),
            FormatPropertiesChrome.Row(l.MaxNumberOfDisplayableRows, comboBoxMaxRows),
            FormatPropertiesChrome.Row(l.DiscSequenceNumber, comboBoxDiscSequenceNumbers),
            FormatPropertiesChrome.Row(l.TotalNumberOfDisks, comboBoxTotalNumberOfDisks));

        var left = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.Format, format, first: true),
                EditorChrome.MakeEditorSection(l.Programme, programme),
            }
        };
        var right = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.TimeCodes, timeCode, first: true),
                EditorChrome.MakeEditorSection(l.DiscAndDisplay, disc),
            }
        };

        return MakeTab(FormatPropertiesChrome.SideBySide(left, right));
    }

    private static TextBox HeaderText(ExportEbuStlViewModel vm, string property, string name, int maxLength)
    {
        var textBox = EditorChrome.AsTextField(UiUtil.MakeTextBox(FieldWidth, vm, property), name);
        textBox.MaxLength = maxLength; // EBU STL header field width
        return textBox;
    }

    private static Control MakeTextAndTimingView(ExportEbuStlViewModel vm)
    {
        var l = Se.Language.File.EbuSaveOptions;

        var comboBoxJustifications = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.Justifications, vm, nameof(vm.SelectedJustification)), l.JustificationCode, FieldWidth);
        var text = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(l.JustificationCode, comboBoxJustifications));

        var comboBoxMarginTops = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.TopAlignments, vm, nameof(vm.SelectedTopAlignment)), l.MarginTop, 120);
        var comboBoxMarginBottoms = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.BottomAlignments, vm, nameof(vm.SelectedBottomAlignment)), l.MarginBottom, 120);
        var comboBoxRowsAddedByNewLine = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.RowsAddByNewLine, vm, nameof(vm.SelectedRowsAddByNewLine)), l.NewLineRows, 120);
        var verticalPosition = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(l.MarginTop, comboBoxMarginTops),
            FormatPropertiesChrome.Row(l.MarginBottom, comboBoxMarginBottoms),
            FormatPropertiesChrome.Row(l.NewLineRows, comboBoxRowsAddedByNewLine));

        // Both are teletext control codes: "0 Open subtitling" writes neither, so leaving them
        // enabled there only promises a box the file and the video preview never show.
        var switchUseBox = EditorChrome.MakeSwitchRow(nameof(vm.UseBox), l.UseBox);
        switchUseBox.Bind(IsEnabledProperty, new Binding(nameof(vm.IsTeletext)));
        var switchUseDoubleHeight = EditorChrome.MakeSwitchRow(nameof(vm.UseDoubleHeight), l.DoubleHeight);
        switchUseDoubleHeight.Bind(IsEnabledProperty, new Binding(nameof(vm.IsTeletext)));
        var teletext = FormatPropertiesChrome.MakeRows(switchUseBox, switchUseDoubleHeight);

        // Not a save option: an STL file carries a character table, not a typeface. It is here so
        // someone with a teletext face installed can have the preview look like a decoder.
        var comboBoxPreviewFont = FormatPropertiesChrome.Combo(UiUtil.MakeComboBox(vm.PreviewFonts, vm, nameof(vm.SelectedPreviewFont)), l.PreviewFont, FieldWidth);
        var buttonPreviewFont = EditorChrome.CreateToolButton("fa-solid fa-ellipsis", Se.Language.Tools.PickFontNameTitle, vm.PickPreviewFontCommand, 28);

        // The drop-down is a name; this shows what the name looks like, in the font itself.
        var labelPreviewFontSample = new TextBlock
        {
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            MaxWidth = 340,
            HorizontalAlignment = HorizontalAlignment.Left,
            [!TextBlock.TextProperty] = new Binding(nameof(vm.PreviewFontSample)) { Source = vm },
            [!TextBlock.FontFamilyProperty] = new Binding(nameof(vm.PreviewFontFamily)) { Source = vm },
        };
        var previewFontSample = new Border
        {
            Child = labelPreviewFontSample,
            Background = EditorChrome.FieldBrush,
            BorderBrush = EditorChrome.FieldBorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 8),
        };
        var videoPreview = FormatPropertiesChrome.MakeRows(
            FormatPropertiesChrome.Row(l.PreviewFont, comboBoxPreviewFont, buttonPreviewFont),
            previewFontSample);

        var left = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(Se.Language.General.Text, text, first: true),
                EditorChrome.MakeEditorSection(l.VerticalPosition, verticalPosition),
            }
        };
        var right = new StackPanel
        {
            Children =
            {
                EditorChrome.MakeEditorSection(l.Teletext, teletext, first: true),
                EditorChrome.MakeEditorSection(l.VideoPreview, videoPreview),
            }
        };

        return MakeTab(FormatPropertiesChrome.SideBySide(left, right));
    }

    private static Control MakeErrorsView(ExportEbuStlViewModel vm)
    {
        var label = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ErrorLog)).WithAlignmentTop();
        label.Padding = new Thickness(0);
        return MakeTab(new Border { Child = label, Padding = new Thickness(14, 12) });
    }

    private static Border MakeTab(Control content)
    {
        return new Border
        {
            Child = content,
            MinWidth = TabMinWidth,
            MinHeight = TabMinHeight,
            BorderBrush = EditorChrome.LineBrush,
            BorderThickness = new Thickness(0, 1, 0, 0),
        };
    }
}
