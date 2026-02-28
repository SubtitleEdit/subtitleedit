using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Files.ExportEbuStl;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.Export.ExportEbuStl;

public partial class ExportEbuStlViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<CodePageNumberItem> _codePages;
    [ObservableProperty] private CodePageNumberItem? _selectedCodePage;
    [ObservableProperty] private ObservableCollection<string> _diskFormatCodes;
    [ObservableProperty] private string? _selectedDiskFormatCode;
    [ObservableProperty] private ObservableCollection<string> _frameRates;
    [ObservableProperty] private string? _selectedFrameRate;
    [ObservableProperty] private ObservableCollection<string> _displayStandardCodes;
    [ObservableProperty] private string? _selectedDisplayStandardCode;
    [ObservableProperty] private ObservableCollection<string> _characterTables;
    [ObservableProperty] private string? _selectedCharacterTable;
    [ObservableProperty] private ObservableCollection<LanguageItem> _languageCodes;
    [ObservableProperty] private LanguageItem? _selectedLanguageCode;
    [ObservableProperty] private ObservableCollection<string> _timeCodeStatusList;
    [ObservableProperty] private string? _selectedTimeCodeStatus;

    [ObservableProperty] private ObservableCollection<int> _revisionNumbers;
    [ObservableProperty] private int? _selectedRevisionNumber;
    [ObservableProperty] private ObservableCollection<int> _maxCharactersPerRow;
    [ObservableProperty] private int? _selectedMaxCharactersPerRow;
    [ObservableProperty] private ObservableCollection<int> _maxRows;
    [ObservableProperty] private int? _selectedMaxRow;
    [ObservableProperty] private ObservableCollection<int> _discSequenceNumbers;
    [ObservableProperty] private int? _selectedDiscSequenceNumber;
    [ObservableProperty] private ObservableCollection<int> _totalNumerOfDiscsList;
    [ObservableProperty] private int? _selectedTotalNumberOfDiscs;

    [ObservableProperty] private string _originalProgramTitle;
    [ObservableProperty] private string _originalEpisodeTitle;
    [ObservableProperty] private string _translatedProgramTitle;
    [ObservableProperty] private string _translatedEpisodeTitle;
    [ObservableProperty] private string _translatorsName;
    [ObservableProperty] private string _subtitleListReferenceCode;
    [ObservableProperty] private string _countryOfOrigin;
    [ObservableProperty] private TimeSpan _startOfProgramme;

    [ObservableProperty] private ObservableCollection<string> _justifications;
    [ObservableProperty] private string? _selectedJustification;
    [ObservableProperty] private ObservableCollection<int> _topAlignments;
    [ObservableProperty] private int? _selectedTopAlignment;
    [ObservableProperty] private ObservableCollection<int> _bottomAlignments;
    [ObservableProperty] private int? _selectedBottomAlignment;
    [ObservableProperty] private ObservableCollection<int> _rowsAddByNewLine;
    [ObservableProperty] private int? _selectedRowsAddByNewLine;
    [ObservableProperty] private bool _useBox;
    [ObservableProperty] private bool _useDoubleHeight;
    [ObservableProperty] private string _errorTitle;
    [ObservableProperty] private string _errorLog;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public Subtitle Subtitle => _subtitle;
    public int? PacCodePage { get; private set; }
    public byte JustificationCode { get; private set; }

    private IFileHelper _fileHelper;
    private Subtitle _subtitle = new Subtitle();
    private Ebu.EbuGeneralSubtitleInformation _header = new Ebu.EbuGeneralSubtitleInformation();

    public ExportEbuStlViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        CodePages = new ObservableCollection<CodePageNumberItem>(CodePageNumberItem.GetCodePageNumberItems());
        SelectedCodePage = CodePages[0];

        DiskFormatCodes = new ObservableCollection<string>
        {
            "STL23.01 (non-standard)",
            "STL24.01 (non-standard)",
            "STL25.01",
            "STL29.01 (non-standard)",
            "STL30.01",
        };

        FrameRates = new ObservableCollection<string>
        {
            "23.976",
            "24",
            "25",
            "29.97",
            "30",
            "50",
            "59.94",
            "60",
            "120"
        };

        DisplayStandardCodes = new ObservableCollection<string>
        {
            "0 Open subtitling",
            "1 Level-1 teletext",
            "2 Level-2 teletext",
            "Undefined",
        };

        CharacterTables = new ObservableCollection<string>
        {
            "Latin",
            "Latin/Cyrillic",
            "Latin/Arabic",
            "Latin/Greek",
            "Latin/Hebrew",
        };

        LanguageCodes = new ObservableCollection<LanguageItem>
        {
            new("00", ""),
            new("01", "Albanian"),
            new("02", "Breton"),
            new("03", "Catalan"),
            new("04", "Croatian"),
            new("05", "Welsh"),
            new("06", "Czech"),
            new("07", "Danish"),
            new("08", "German"),
            new("09", "English"),
            new("0A", "Spanish"),
            new("0B", "Esperanto"),
            new("0C", "Estonian"),
            new("0D", "Basque"),
            new("0E", "Faroese"),
            new("0F", "French"),
            new("10", "Frisian"),
            new("11", "Irish"),
            new("12", "Gaelic"),
            new("13", "Galician"),
            new("14", "Icelandic"),
            new("15", "Italian"),
            new("16", "Lappish"),
            new("17", "Latin"),
            new("18", "Latvian"),
            new("19", "Luxembourgi"),
            new("1A", "Lithuanian"),
            new("1B", "Hungarian"),
            new("1C", "Maltese"),
            new("1D", "Dutch"),
            new("1E", "Norwegian"),
            new("1F", "Occitan"),
            new("20", "Polish"),
            new("21", "Portuguese"),
            new("22", "Romanian"),
            new("23", "Romansh"),
            new("24", "Serbian"),
            new("25", "Slovak"),
            new("26", "Slovenian"),
            new("27", "Finnish"),
            new("28", "Swedish"),
            new("29", "Turkish"),
            new("2A", "Flemish"),
            new("2B", "Wallon"),
            new("7F", "Amharic"),
            new("7E", "Arabic"),
            new("7D", "Armenian"),
            new("7C", "Assamese"),
            new("7B", "Azerbaijani"),
            new("7A", "Bambora"),
            new("79", "Bielorussian"),
            new("78", "Bengali"),
            new("77", "Bulgarian"),
            new("76", "Burmese"),
            new("75", "Chinese"),
            new("74", "Churash"),
            new("73", "Dari"),
            new("72", "Fulani"),
            new("71", "Georgian"),
            new("70", "Greek"),
            new("6F", "Gujurati"),
            new("6E", "Gurani"),
            new("6D", "Hausa"),
            new("6C", "Hebrew"),
            new("6B", "Hindi"),
            new("6A", "Indonesian"),
            new("69", "Japanese"),
            new("68", "Kannada"),
            new("67", "Kazakh"),
            new("66", "Khmer"),
            new("65", "Korean"),
            new("64", "Laotian"),
            new("63", "Macedonian"),
            new("62", "Malagasay"),
            new("61", "Malaysian"),
            new("60", "Moldavian"),
            new("5F", "Marathi"),
            new("5E", "Ndebele"),
            new("5D", "Nepali"),
            new("5C", "Oriya"),
            new("5B", "Papamiento"),
            new("5A", "Persian"),
            new("59", "Punjabi"),
            new("58", "Pushtu"),
            new("57", "Quechua"),
            new("56", "Russian"),
            new("55", "Ruthenian"),
            new("54", "Serbocroat"),
            new("53", "Shona"),
            new("52", "Sinhalese"),
            new("51", "Somali"),
            new("50", "Sranan Tongo"),
            new("4F", "Swahili"),
            new("4E", "Tadzhik"),
            new("4D", "Tamil"),
            new("4C", "Tatar"),
            new("4B", "Telugu"),
            new("4A", "Thai"),
            new("49", "Ukrainian"),
            new("48", "Urdu"),
            new("47", "Uzbek"),
            new("46", "Vietnamese"),
            new("45", "Zulu"),
        };

        TimeCodeStatusList = new ObservableCollection<string>
        {
            "Not intended for use",
            "Intended for use",
        };

        Justifications = new ObservableCollection<string>
        {
            "Unchanged presentation",
            "Left-justified text",
            "Centered text",
            "Right-justified text",
        };

        RevisionNumbers = new ObservableCollection<int>(Enumerable.Range(0, 100).ToList());
        MaxCharactersPerRow = new ObservableCollection<int>(Enumerable.Range(0, 100).ToList());
        MaxRows = new ObservableCollection<int>(Enumerable.Range(0, 100).ToList());
        DiscSequenceNumbers = new ObservableCollection<int>(Enumerable.Range(0, 10).ToList());
        TotalNumerOfDiscsList = new ObservableCollection<int>(Enumerable.Range(0, 10).ToList());
        TopAlignments = new ObservableCollection<int>(Enumerable.Range(0, 51).ToList());
        BottomAlignments = new ObservableCollection<int>(Enumerable.Range(0, 51).ToList());
        RowsAddByNewLine = new ObservableCollection<int>(Enumerable.Range(0, 11).ToList());
        OriginalEpisodeTitle = string.Empty;
        OriginalProgramTitle = string.Empty;
        TranslatorsName = string.Empty;
        TranslatedEpisodeTitle = string.Empty;
        TranslatedProgramTitle = string.Empty;
        CountryOfOrigin = string.Empty;
        SubtitleListReferenceCode = string.Empty;
        ErrorLog = string.Empty;
        ErrorTitle = Se.Language.File.EbuSaveOptions.Errors;
    }

    public void Initialize(Subtitle? subtitle)
    {
        _subtitle = subtitle ?? new Subtitle();

        Dispatcher.UIThread.Post(() =>
        {
            SelectedDiskFormatCode = DiskFormatCodes[2];
            SelectedFrameRate = FrameRates[2];
            SelectedDisplayStandardCode = DisplayStandardCodes[0];
            SelectedCharacterTable = CharacterTables[0];
            SelectedLanguageCode = LanguageCodes.FirstOrDefault(p => p.Language == "English");
            SelectedTimeCodeStatus = TimeCodeStatusList[1];
            SelectedJustification = Justifications[1];
            SelectedRevisionNumber = 1;
            SelectedMaxCharactersPerRow = 40;
            SelectedMaxRow = 23;
            SelectedDiscSequenceNumber = 1;
            SelectedTotalNumberOfDiscs = 1;
            SelectedTopAlignment = 0;
            SelectedBottomAlignment = 2;
            SelectedRowsAddByNewLine = 2;

            CheckErrors(_subtitle);
        });
    }

    [RelayCommand]
    private void Ok()
    {
        _header.CodePageNumber = SelectedCodePage?.CodePage ?? string.Empty;
        if (_header.CodePageNumber.Length < 3)
        {
            _header.CodePageNumber = "865";
        }

        _header.DiskFormatCode = SelectedDiskFormatCode?.Substring(0, 8) ?? "STL25.01";

        double d = 25.0;
        if (SelectedFrameRate != null && double.TryParse(SelectedFrameRate.Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "."), out d) && d > 20 && d < 200)
        {
            _header.FrameRateFromSaveDialog = d;
        }

        if (SelectedDisplayStandardCode != null && SelectedDisplayStandardCode.StartsWith("0"))
        {
            _header.DisplayStandardCode = "0";
        }
        if (SelectedDisplayStandardCode != null && SelectedDisplayStandardCode.StartsWith("1"))
        {
            _header.DisplayStandardCode = "1";
        }
        if (SelectedDisplayStandardCode != null && SelectedDisplayStandardCode.StartsWith("2"))
        {
            _header.DisplayStandardCode = "2";
        }
        else
        {
            _header.DisplayStandardCode = " ";
        }

        var characterTableNumber = CharacterTables.IndexOf(SelectedCharacterTable ?? CharacterTables[0]);
        _header.CharacterCodeTableNumber = "0" + characterTableNumber.ToString(CultureInfo.InvariantCulture);

        _header.LanguageCode = SelectedLanguageCode != null ? SelectedLanguageCode.Code : "09"; // Default to English
        if (_header.LanguageCode.Length != 2)
        {
            _header.LanguageCode = "0A";
        }

        _header.OriginalProgrammeTitle = OriginalProgramTitle.PadRight(32, ' ');
        _header.OriginalEpisodeTitle = OriginalEpisodeTitle.PadRight(32, ' ');
        _header.TranslatedProgrammeTitle = TranslatedProgramTitle.PadRight(32, ' ');
        _header.TranslatedEpisodeTitle = TranslatedEpisodeTitle.PadRight(32, ' ');
        _header.TranslatorsName = TranslatorsName.PadRight(32, ' ');
        _header.SubtitleListReferenceCode = SubtitleListReferenceCode.PadRight(16, ' ');
        _header.CountryOfOrigin = CountryOfOrigin;
        if (_header.CountryOfOrigin.Length != 3)
        {
            _header.CountryOfOrigin = "USA";
        }

        var timeCodeStatus = SelectedTimeCodeStatus ?? TimeCodeStatusList.Last();
        _header.TimeCodeStatus = TimeCodeStatusList.IndexOf(timeCodeStatus).ToString(CultureInfo.InvariantCulture);
        _header.TimeCodeStartOfProgramme = new TimeCode(StartOfProgramme).ToHHMMSSFF().RemoveChar(':');

        _header.RevisionNumber = SelectedRevisionNumber?.ToString("00") ?? "00";
        _header.MaximumNumberOfDisplayableCharactersInAnyTextRow = SelectedMaxCharactersPerRow?.ToString("00") ?? "00";
        _header.MaximumNumberOfDisplayableRows = SelectedMaxRow?.ToString("00") ?? "00";
        _header.DiskSequenceNumber = SelectedDiscSequenceNumber?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        _header.TotalNumberOfDisks = SelectedTotalNumberOfDiscs?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;

        JustificationCode = (byte)Justifications.IndexOf(SelectedJustification ?? Justifications[0]);
        Configuration.Settings.SubtitleSettings.EbuStlMarginTop = (int)(SelectedTopAlignment ?? 0);
        Configuration.Settings.SubtitleSettings.EbuStlMarginBottom = (int)(SelectedBottomAlignment ?? 0);
        Configuration.Settings.SubtitleSettings.EbuStlNewLineRows = (int)(SelectedRowsAddByNewLine ?? 1);
        Configuration.Settings.SubtitleSettings.EbuStlTeletextUseBox = UseBox;
        Configuration.Settings.SubtitleSettings.EbuStlTeletextUseDoubleHeight = UseDoubleHeight;

        if (_subtitle != null)
        {
            _subtitle.Header = _header.ToString();
        }

        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private async Task Import()
    {
        var format = new Ebu();
        var subtitleFileName = await _fileHelper.PickOpenFile(Window!, "Open", format.Name, format.Extension);
        if (string.IsNullOrEmpty(subtitleFileName))
        {
            return;
        }

        var buffer = await File.ReadAllBytesAsync(subtitleFileName);
        if (buffer.Length <= 270)
        {
            return;
        }

        FillHeaderFromFile(subtitleFileName);
    }

    private void FillHeaderFromFile(string fileName)
    {
        if (File.Exists(fileName))
        {
            var ebu = new Ebu();
            var temp = new Subtitle();
            ebu.LoadSubtitle(temp, null, fileName);
            FillFromHeader(ebu.Header);
            if (ebu.JustificationCodes.Count > 2 && ebu.JustificationCodes[1] == ebu.JustificationCodes[2])
            {
                if (ebu.JustificationCodes[1] >= 0 && ebu.JustificationCodes[1] < Justifications.Count)
                {
                    SelectedJustification = Justifications[ebu.JustificationCodes[1]];
                }
            }
        }
    }

    private void FillFromHeader(Ebu.EbuGeneralSubtitleInformation header)
    {
        SelectedCodePage = CodePages.FirstOrDefault(p => p.CodePage == header.CodePageNumber);

        SelectedDiskFormatCode = DiskFormatCodes.First(p => p.Contains(header.DiskFormatCode, StringComparison.OrdinalIgnoreCase));

        if (header.FrameRateFromSaveDialog is > 20 and < 200)
        {
            SelectedFrameRate = header.FrameRateFromSaveDialog.ToString(CultureInfo.CurrentCulture);
        }

        SelectedDisplayStandardCode = DisplayStandardCodes.First(p => p.StartsWith(header.DisplayStandardCode, StringComparison.InvariantCulture));

        if (int.TryParse(header.CharacterCodeTableNumber, out var tableNumber))
        {
            SelectedCharacterTable = CharacterTables[tableNumber];
        }

        SelectedLanguageCode = LanguageCodes.FirstOrDefault(p => p.Code == header.LanguageCode);
        OriginalProgramTitle = header.OriginalProgrammeTitle.TrimEnd();
        OriginalEpisodeTitle = header.OriginalEpisodeTitle.TrimEnd();
        TranslatedProgramTitle = header.TranslatedProgrammeTitle.TrimEnd();
        TranslatedEpisodeTitle = header.TranslatedEpisodeTitle.TrimEnd();
        TranslatorsName = header.TranslatorsName.TrimEnd();
        SubtitleListReferenceCode = header.SubtitleListReferenceCode.TrimEnd();
        CountryOfOrigin = header.CountryOfOrigin;

        SelectedTimeCodeStatus = TimeCodeStatusList.Last();
        if (header.TimeCodeStatus == "0")
        {
            SelectedTimeCodeStatus = TimeCodeStatusList.First();
        }

        try
        {
            // HHMMSSFF
            var hh = int.Parse(header.TimeCodeStartOfProgramme.Substring(0, 2));
            var mm = int.Parse(header.TimeCodeStartOfProgramme.Substring(2, 2));
            var ss = int.Parse(header.TimeCodeStartOfProgramme.Substring(4, 2));
            var ff = int.Parse(header.TimeCodeStartOfProgramme.Substring(6, 2));
            StartOfProgramme = new TimeCode(hh, mm, ss, SubtitleFormat.FramesToMillisecondsMax999(ff)).TimeSpan;
        }
        catch (Exception)
        {
            StartOfProgramme = new TimeSpan(0);
        }

        if (int.TryParse(header.RevisionNumber, out var number))
        {
            SelectedRevisionNumber = number;
        }
        else
        {
            SelectedRevisionNumber = 1;
        }

        if (int.TryParse(header.MaximumNumberOfDisplayableCharactersInAnyTextRow, out number))
        {
            SelectedMaxCharactersPerRow = number;
        }

        SelectedMaxRow = 23;
        if (int.TryParse(header.MaximumNumberOfDisplayableRows, out number))
        {
            SelectedMaxRow = number;
        }

        if (int.TryParse(header.DiskSequenceNumber, out number))
        {
            SelectedDiscSequenceNumber = number;
        }
        else
        {
            SelectedDiscSequenceNumber = 1;
        }

        if (int.TryParse(header.TotalNumberOfDisks, out number))
        {
            SelectedTotalNumberOfDiscs = number;
        }
        else
        {
            SelectedTotalNumberOfDiscs = 1;
        }
    }

    private string RemoveAfterParenthesisAndTrim(string input)
    {
        var index = input.IndexOf("(");
        return index >= 0 ? input.Substring(0, index).TrimEnd() : input;
    }

    private void CheckErrors(Subtitle subtitle)
    {
        if (subtitle.Paragraphs.Count == 0)
        {
            return;
        }

        var sb = new StringBuilder();
        var errorCount = 0;
        var i = 1;
        var isTeletext = SelectedDiskFormatCode?.Contains("teletext", StringComparison.OrdinalIgnoreCase) ?? false;
        foreach (var p in subtitle.Paragraphs)
        {
            var arr = p.Text.SplitToLines();
            for (var index = 0; index < arr.Count; index++)
            {
                var line = arr[index];
                var s = HtmlUtil.RemoveHtmlTags(line, true);
                if (s.Length > SelectedMaxCharactersPerRow)
                {
                    sb.AppendLine(string.Format(Se.Language.File.EbuSaveOptions.MaxLengthError, i, SelectedMaxCharactersPerRow, s.Length - SelectedMaxCharactersPerRow, s));
                    errorCount++;
                }

                if (isTeletext)
                {
                    // See https://kb.fab-online.com/0040-fabsubtitler-editor/00010-linelengthineditor/

                    // 36 characters for double height colored tex
                    if (arr.Count == 2 && s.Length > 36 && arr[index].Contains("<font ", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine($"Line {i}-{index + 1}: 36 (not {s.Length}) should be maximum characters for double height colored text");
                        errorCount++;
                    }

                    // 37 characters for double height white text
                    else if (arr.Count == 2 && s.Length > 37 && !p.Text.Contains("<font ", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.AppendLine($"Line {i}-{index + 1}: 37 (not {s.Length}) should be maximum characters for double height white text");
                        errorCount++;
                    }

                    // 38 characters for single height white text
                    else if (arr.Count == 1 && s.Length > 38)
                    {
                        sb.AppendLine($"Line {i}: 38 (not {s.Length}) should be maximum characters for single height white text");
                        errorCount++;
                    }
                }
            }

            i++;
        }

        ErrorLog = sb.ToString();
        ErrorTitle = string.Format(Se.Language.File.EbuSaveOptions.ErrorsX, errorCount);
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }
}