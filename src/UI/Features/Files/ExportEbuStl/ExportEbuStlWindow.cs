using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Files.Export.ExportEbuStl;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportEbuStl;

public class ExportEbuStlWindow : Window
{
    public ExportEbuStlWindow(ExportEbuStlViewModel vm)
    {
        UiUtil.InitializeWindow(this, GetType().Name);
        Title = "Export EBU STL";
        SizeToContent = SizeToContent.WidthAndHeight;
        CanResize = false;
        vm.Window = this;
        DataContext = vm;

        var tabControl = new TabControl();
        var tabGeneral = new TabItem
        {
            Header = Se.Language.General.General,
            Content = MakeGeneralView(vm),
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
            [!Avalonia.Controls.Primitives.HeaderedContentControl.HeaderProperty] = new Binding(nameof(vm.ErrorTitle)) { Source = vm },
            Content = MakeErrorsView(vm),
        };
        tabControl.Items.Add(tabErrors);

        var buttonOk = UiUtil.MakeButtonOk(vm.OkCommand);
        var buttonCancel = UiUtil.MakeButtonCancel(vm.CancelCommand);
        var panelButtons = UiUtil.MakeButtonBar(buttonOk, buttonCancel);

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
            },
            Margin = UiUtil.MakeWindowMargin(),
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        grid.Add(tabControl, 0);
        grid.Add(panelButtons, 1);

        Content = grid;

        Activated += delegate { buttonOk.Focus(); }; // hack to make OnKeyDown work
        KeyDown += vm.OnKeyDown;
    }

    private Border MakeGeneralView(ExportEbuStlViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };


        var textBoxWidth = 225;

        var labelCodePageNumber = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.CodePageNumber);
        var comboBoxCodeNumbers = UiUtil.MakeComboBox(vm.CodePages, vm, nameof(vm.SelectedCodePage)).WithMinWidth(textBoxWidth);

        var labelDiskFormatCode = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.DiskFormatCode);
        var comboboxDiskFormatCodes = UiUtil.MakeComboBox(vm.DiskFormatCodes, vm, nameof(vm.SelectedDiskFormatCode)).WithMinWidth(textBoxWidth);

        var labelFrameRate = UiUtil.MakeLabel(Se.Language.General.FrameRate);
        var comboBoxFrameRates = UiUtil.MakeComboBox(vm.FrameRates, vm, nameof(vm.SelectedFrameRate)).WithMinWidth(textBoxWidth);

        var labelDisplayStandardCode = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.DisplayStandardCode);
        var comboBoxDisplayStandardCodes = UiUtil.MakeComboBox(vm.DisplayStandardCodes, vm, nameof(vm.SelectedDisplayStandardCode)).WithMinWidth(textBoxWidth);

        var labelCharacterTable = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.CharacterCodeTable);
        var comboBoxCharacterTables = UiUtil.MakeComboBox(vm.CharacterTables, vm, nameof(vm.SelectedCharacterTable)).WithMinWidth(textBoxWidth);

        var labelLanguageCode = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.LanguageCode);
        var comboBoxLanguageCodes = UiUtil.MakeComboBox(vm.LanguageCodes, vm, nameof(vm.SelectedLanguageCode)).WithMinWidth(textBoxWidth);

        var labelOriginalProgramTitle = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.OriginalProgramTitle);
        var textBoxOriginalProgramTitle = UiUtil.MakeTextBox(textBoxWidth, vm, nameof(vm.OriginalProgramTitle));

        var labelOriginalEpisodeTitle = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.OriginalEpisodeTitle);
        var textBoxOriginalEpisodeTitle = UiUtil.MakeTextBox(textBoxWidth, vm, nameof(vm.OriginalEpisodeTitle));

        var labelTranslatedProgramTitle = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.TranslatedProgramTitle);
        var textBoxTranslatedProgramTitle = UiUtil.MakeTextBox(textBoxWidth, vm, nameof(vm.TranslatedProgramTitle));

        var labelTranslatedEpisodeTitle = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.TranslatedEpisodeTitle);
        var textBoxTranslatedEpisodeTitle = UiUtil.MakeTextBox(textBoxWidth, vm, nameof(vm.TranslatedEpisodeTitle));

        var labelTranslatorsName = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.TranslatorsName);
        var textBoxTranslatorsName = UiUtil.MakeTextBox(textBoxWidth, vm, nameof(vm.TranslatorsName));

        grid.Add(labelCodePageNumber, 0, 0);
        grid.Add(comboBoxCodeNumbers, 0, 1);

        grid.Add(labelDiskFormatCode, 1, 0);
        grid.Add(comboboxDiskFormatCodes, 1, 1);

        grid.Add(labelFrameRate, 2, 0);
        grid.Add(comboBoxFrameRates, 2, 1);

        grid.Add(labelDisplayStandardCode, 3, 0);
        grid.Add(comboBoxDisplayStandardCodes, 3, 1);

        grid.Add(labelCharacterTable, 4, 0);
        grid.Add(comboBoxCharacterTables, 4, 1);

        grid.Add(labelLanguageCode, 5, 0);
        grid.Add(comboBoxLanguageCodes, 5, 1);

        grid.Add(labelOriginalProgramTitle, 6, 0);
        grid.Add(textBoxOriginalProgramTitle, 6, 1);

        grid.Add(labelOriginalEpisodeTitle, 7, 0);
        grid.Add(textBoxOriginalEpisodeTitle, 7, 1);

        grid.Add(labelTranslatedProgramTitle, 8, 0);
        grid.Add(textBoxTranslatedProgramTitle, 8, 1);

        grid.Add(labelTranslatedEpisodeTitle, 9, 0);
        grid.Add(textBoxTranslatedEpisodeTitle, 9, 1);

        grid.Add(labelTranslatorsName, 10, 0);
        grid.Add(textBoxTranslatorsName, 10, 1);


        var labelSubtitleListReferenceCode = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.SubtitleListReferenceCode);
        var textBoxSubtitleListReferenceCode = UiUtil.MakeTextBox(textBoxWidth, vm, nameof(vm.SubtitleListReferenceCode));

        var labelCountryOfOrigin = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.CountryOfOrigin);
        var textBoxCountryOfOrigin = UiUtil.MakeTextBox(textBoxWidth, vm, nameof(vm.CountryOfOrigin));

        var labelTimeCodeStatus = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.TimeCodeStatus);
        var comboBoxTimeCodeStatus = UiUtil.MakeComboBox(vm.TimeCodeStatusList, vm, nameof(vm.SelectedTimeCodeStatus)).WithMinWidth(textBoxWidth);

        var labelStartOfProgramme = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.TimeCodeStartOfProgramme);
        var timeCodeUpDownStartOfProgramme = new TimeCodeUpDown
        {
            DataContext = vm,
            [!TimeCodeUpDown.ValueProperty] = new Binding(nameof(vm.StartOfProgramme))
            {
                Mode = BindingMode.TwoWay,
            }
        };

        grid.Add(labelSubtitleListReferenceCode, 0, 2);
        grid.Add(textBoxSubtitleListReferenceCode, 0, 3);

        grid.Add(labelCountryOfOrigin, 1, 2);
        grid.Add(textBoxCountryOfOrigin, 1, 3);

        grid.Add(labelTimeCodeStatus, 2, 2);
        grid.Add(comboBoxTimeCodeStatus, 2, 3);

        grid.Add(labelStartOfProgramme, 3, 2);
        grid.Add(timeCodeUpDownStartOfProgramme, 3, 3);


        var labelRevisionNumber = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.RevisionNumber);
        var comboBoxRevisionNumbers = UiUtil.MakeComboBox(vm.RevisionNumbers, vm, nameof(vm.SelectedRevisionNumber)).WithMinWidth(textBoxWidth);

        var labelMaxCharsPerRow = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.MaxNoOfDisplayableChars);
        var comboBoxMaxCharsPerRowList = UiUtil.MakeComboBox(vm.MaxCharactersPerRow, vm, nameof(vm.SelectedMaxCharactersPerRow)).WithMinWidth(textBoxWidth);

        var labelMaxRows = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.MaxNumberOfDisplayableRows);
        var comboBoxMaxRows = UiUtil.MakeComboBox(vm.MaxRows, vm, nameof(vm.SelectedMaxRow)).WithMinWidth(textBoxWidth);

        var labelDiscSequenceNumber = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.DiscSequenceNumber);
        var comboBoxDiscSequenceNumbers = UiUtil.MakeComboBox(vm.DiscSequenceNumbers, vm, nameof(vm.SelectedDiscSequenceNumber)).WithMinWidth(textBoxWidth);

        var labelTotalNumberOfDisks = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.TotalNumberOfDisks);
        var comboBoxTotalNumberOfDisks = UiUtil.MakeComboBox(vm.TotalNumerOfDiscsList, vm, nameof(vm.SelectedTotalNumberOfDiscs)).WithMinWidth(textBoxWidth);

        grid.Add(labelRevisionNumber, 5, 2);
        grid.Add(comboBoxRevisionNumbers, 5, 3);

        grid.Add(labelMaxCharsPerRow, 6, 2);
        grid.Add(comboBoxMaxCharsPerRowList, 6, 3);

        grid.Add(labelMaxRows, 7, 2);
        grid.Add(comboBoxMaxRows, 7, 3);

        grid.Add(labelDiscSequenceNumber, 8, 2);
        grid.Add(comboBoxDiscSequenceNumbers, 8, 3);

        grid.Add(labelTotalNumberOfDisks, 9, 2);
        grid.Add(comboBoxTotalNumberOfDisks, 9, 3);

        var buttonImport = UiUtil.MakeButton(Se.Language.General.ImportDotDotDot, vm.ImportCommand);
        grid.Add(buttonImport, 0, 4);

        return UiUtil.MakeBorderForControl(grid);
    }

    private Border MakeTextAndTimingView(ExportEbuStlViewModel vm)
    {
        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
                new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) },
            },
            ColumnSpacing = 10,
            RowSpacing = 10,
            Width = double.NaN,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        var textBoxWidth = 225;

        var labelJustification = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.JustificationCode);
        var comboBoxJustifications = UiUtil.MakeComboBox(vm.Justifications, vm, nameof(vm.SelectedJustification)).WithMinWidth(textBoxWidth);


        var labelVerticalPosition = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.VerticalPosition);

        var labelMarginTop = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.MarginTop);
        var comboBoxMarginTops = UiUtil.MakeComboBox(vm.TopAlignments, vm, nameof(vm.SelectedTopAlignment)).WithMinWidth(textBoxWidth);

        var labelMarginBottom = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.MarginBottom);
        var comboBoxMarginBottoms = UiUtil.MakeComboBox(vm.BottomAlignments, vm, nameof(vm.SelectedBottomAlignment)).WithMinWidth(textBoxWidth);

        var labelRowsAddedByNewLine = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.NewLineRows);
        var comboBoxRowsAddedByNewLine = UiUtil.MakeComboBox(vm.RowsAddByNewLine, vm, nameof(vm.SelectedRowsAddByNewLine)).WithMinWidth(textBoxWidth);


        var labelTeletext = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.Teletext);

        var labelUseBox = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.UseBox);
        var checkBoxUseBox = UiUtil.MakeCheckBox(vm, nameof(vm.UseBox));

        var labelUseDoubleHeight = UiUtil.MakeLabel(Se.Language.File.EbuSaveOptions.DoubleHeight);
        var checkBoxUseDoubleHeight = UiUtil.MakeCheckBox(vm, nameof(vm.UseDoubleHeight));


        grid.Add(labelJustification, 0, 0);
        grid.Add(comboBoxJustifications, 0, 1);

        grid.Add(labelVerticalPosition, 2, 0);

        grid.Add(labelMarginTop, 3, 0);
        grid.Add(comboBoxMarginTops, 3, 1);

        grid.Add(labelMarginBottom, 4, 0);
        grid.Add(comboBoxMarginBottoms, 4, 1);

        grid.Add(labelRowsAddedByNewLine, 5, 0);
        grid.Add(comboBoxRowsAddedByNewLine, 5, 1);


        grid.Add(labelTeletext, 7, 0);

        grid.Add(labelUseBox, 8, 0);
        grid.Add(checkBoxUseBox, 8, 1);

        grid.Add(labelUseDoubleHeight, 9, 0);
        grid.Add(checkBoxUseDoubleHeight, 9, 1);

        return UiUtil.MakeBorderForControl(grid).WithMinWidth(944).WithMinHeight(465);
    }

    private Border MakeErrorsView(ExportEbuStlViewModel vm)
    {
        var label = UiUtil.MakeLabel(string.Empty).WithBindText(vm, nameof(vm.ErrorLog)).WithAlignmentTop();
        return UiUtil.MakeBorderForControl(label).WithMinWidth(944).WithMinHeight(465);
    }
}
