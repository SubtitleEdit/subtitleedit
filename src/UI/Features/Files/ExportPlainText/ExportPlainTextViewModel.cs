using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Files.ExportPlainText;

public partial class ExportPlainTextViewModel : ObservableObject
{
    // settings controls
    [ObservableProperty] private bool _formatTextNone;
    [ObservableProperty] private bool _formatTextMerge;
    [ObservableProperty] private bool _formatTextUnbreak;
    [ObservableProperty] private bool _textRemoveStyling;
    [ObservableProperty] private bool _showLineNumbers;
    [ObservableProperty] private bool _addNewLineAfterLineNumber;
    [ObservableProperty] private bool _showTimeCodes;
    [ObservableProperty] private bool _addNewLineAfterTimeCode;
    [ObservableProperty] private ObservableCollection<string> _timeCodeFormats;
    [ObservableProperty] private string _selectedTimeCodeFormat;
    [ObservableProperty] private ObservableCollection<string> _timeCodeSeparators;
    [ObservableProperty] private string _selectedTimeCodeSeparator;
    [ObservableProperty] private bool _addLineAfterText;
    [ObservableProperty] private bool _addLineBetweenSubtitles;

    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    [ObservableProperty] private TextEncoding? _selectedEncoding;

    [ObservableProperty] private string _previewText;


    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private IWindowService _windowService;
    private IFileHelper _fileHelper;

    private List<SubtitleLineViewModel> _subtitles;
    private string? _videoFileName;
    private string _title;
    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private string? _subtitleFileName;

    public ExportPlainTextViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        _title = string.Empty;

        TimeCodeFormats = new ObservableCollection<string>
        {
            "hh:mm:ss,zzz",
            "hh:mm:ss.zzz",
            "hh:mm:ss:ff",
            Se.Language.General.Seconds,
        };
        SelectedTimeCodeFormat = TimeCodeFormats[0];

        TimeCodeSeparators = new ObservableCollection<string>
        {
            " --> ",
            " - ",
            "-",
            " > ",
            " ~ ",
            " | ",
            " / ",
            " \\ ",
            " : ",
            " "
        };
        SelectedTimeCodeSeparator = TimeCodeSeparators[0];

        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        SelectedEncoding = Encodings.FirstOrDefault(p => p.DisplayName == Se.Settings.General.DefaultEncoding) ??
                           Encodings[0];
        PreviewText = string.Empty;
        _subtitles = new List<SubtitleLineViewModel>();

        _timerUpdatePreview = new System.Timers.Timer();
        _timerUpdatePreview.Interval = 250;
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;

        FormatTextNone = false;
        FormatTextNone = false;
        FormatTextUnbreak = false;
        if (Se.Settings.File.ExportPlainText.TextProcessing == "Merge")
        {
            FormatTextMerge = true;
        }
        else if (Se.Settings.File.ExportPlainText.TextProcessing == "Unbreak")
        {
            FormatTextUnbreak = true;
        }
        else
        {
            FormatTextNone = true;
        }

        TextRemoveStyling = Se.Settings.File.ExportPlainText.TextRemoveStyling;
        ShowLineNumbers = Se.Settings.File.ExportPlainText.ShowLineNumbers;
        AddNewLineAfterLineNumber = Se.Settings.File.ExportPlainText.AddNewLineAfterLineNumber;
        ShowTimeCodes = Se.Settings.File.ExportPlainText.ShowTimeCodes;
        AddNewLineAfterTimeCode = Se.Settings.File.ExportPlainText.AddNewLineAfterTimeCode;
        SelectedTimeCodeFormat = TimeCodeFormats.FirstOrDefault(p=>p == Se.Settings.File.ExportPlainText.TimeCodeFormat) ?? TimeCodeFormats.First();
        SelectedTimeCodeSeparator = TimeCodeSeparators.FirstOrDefault(p => p == Se.Settings.File.ExportPlainText.TimeCodeSeparator) ?? TimeCodeSeparators.First();
        AddLineAfterText = Se.Settings.File.ExportPlainText.AddLineAfterText;
        AddLineBetweenSubtitles = Se.Settings.File.ExportPlainText.AddLineBetweenSubtitles;
    }

    private void SaveSettings()
    {
        Se.Settings.File.ExportPlainText.TextRemoveStyling = TextRemoveStyling;
        Se.Settings.File.ExportPlainText.TextRemoveStyling = TextRemoveStyling;
        Se.Settings.File.ExportPlainText.ShowLineNumbers = ShowLineNumbers;
        Se.Settings.File.ExportPlainText.AddNewLineAfterLineNumber = AddNewLineAfterLineNumber;
        Se.Settings.File.ExportPlainText.ShowTimeCodes = ShowTimeCodes;
        Se.Settings.File.ExportPlainText.AddNewLineAfterTimeCode = AddNewLineAfterTimeCode;
        Se.Settings.File.ExportPlainText.TimeCodeFormat = SelectedTimeCodeFormat;
        Se.Settings.File.ExportPlainText.TimeCodeSeparator = SelectedTimeCodeSeparator;
        Se.Settings.File.ExportPlainText.AddLineAfterText = AddLineAfterText;
        Se.Settings.File.ExportPlainText.AddLineBetweenSubtitles = AddLineBetweenSubtitles;

        Se.SaveSettings();
    }

    private void TimerUpdatePreviewElapsed(object? sender, ElapsedEventArgs e)
    {
        _timerUpdatePreview.Stop();

        if (_dirty)
        {
            _dirty = false;
            PreviewText = GetExportText();
        }

        _timerUpdatePreview.Start();
    }

    private string GetExportText()
    {
        var sb = new StringBuilder();

        foreach (var subtitleLine in _subtitles)
        {
            var text = subtitleLine.Text ?? string.Empty;

            // Remove styling tags if requested
            if (TextRemoveStyling)
            {
                text = HtmlUtil.RemoveHtmlTags(text, true);
            }

            // Handle text formatting modes
            if (FormatTextMerge)
            {
                text = text.Replace(Environment.NewLine, " ");
            }
            else if (FormatTextUnbreak)
            {
                text = text.Replace(Environment.NewLine, " ");
                text = Utilities.UnbreakLine(text);
            }

            // Line number
            if (ShowLineNumbers)
            {
                sb.Append(subtitleLine.Number);
                if (AddNewLineAfterLineNumber)
                {
                    sb.AppendLine();
                }
                else
                {
                    sb.Append(' ');
                }
            }

            // Time codes
            if (ShowTimeCodes)
            {
                var start = FormatTimeCode(subtitleLine.StartTime.TotalMilliseconds);
                var end = FormatTimeCode(subtitleLine.EndTime.TotalMilliseconds);
                sb.Append(start);
                sb.Append(SelectedTimeCodeSeparator);
                sb.Append(end);
                if (AddNewLineAfterTimeCode)
                {
                    sb.AppendLine();
                }
                else
                {
                    sb.Append(' ');
                }
            }

            // Subtitle text
            sb.AppendLine(text.Trim());

            // Add optional blank line(s)
            if (AddLineAfterText)
            {
                sb.AppendLine();
            }

            if (AddLineBetweenSubtitles)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString().TrimEnd();
    }

    private string FormatTimeCode(double totalMilliseconds)
    {
        var tc = new TimeCode(totalMilliseconds);

        if (SelectedTimeCodeFormat == Se.Language.General.Seconds)
        {
            return $"{tc.TotalSeconds:0.000}";
        }

        return SelectedTimeCodeFormat switch
        {
            "hh:mm:ss,zzz" => tc.ToString(false),
            "hh:mm:ss.zzz" => tc.ToString().Replace(',', '.'),
            "hh:mm:ss:ff" => tc.ToHHMMSSFF(),
            _ => tc.ToString()
        };
    }

    [RelayCommand]
    private async Task SaveAs()
    {
        if (Window == null)
        {
            return;
        }

        var text = GetExportText();

        var fileName = await _fileHelper.PickSaveFile(Window, ".txt", _title, Se.Language.General.SaveFileAsTitle);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        await System.IO.File.WriteAllTextAsync(fileName, text, SelectedEncoding?.Encoding ?? Encoding.UTF8);
        SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        SaveSettings();
        Window?.Close();
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(List<SubtitleLineViewModel> subtitles, string? subtitleFileName, string? videoFileName)
    {
        _subtitles = subtitles;
        _subtitleFileName = subtitleFileName;
        _videoFileName = videoFileName;
        _timerUpdatePreview.Start();
        _dirty = true;
    }

    public void SetDirty()
    {
        _dirty = true;
    }
}