using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public partial class SpeechToTextAdvancedViewModel : ObservableObject
{
    [ObservableProperty] private string _parameters;
    [ObservableProperty] private string _helpText;
    [ObservableProperty] private bool _isWhisperCppVisible;
    [ObservableProperty] private bool _isWhisperXxlVisible;
    [ObservableProperty] private bool _isWhisperCTranslate2Visible;
    [ObservableProperty] private bool _isCrispAsrVisible;
    [ObservableProperty] private bool _isWordLevelCppActive;
    [ObservableProperty] private bool _isVadCppActive;
    [ObservableProperty] private bool _isVadCTranslate2Active;
    [ObservableProperty] private bool _isHighlightWordsCTranslate2Active;
    [ObservableProperty] private bool _isVadCrispAsrActive;
    [ObservableProperty] private bool _isHighlightWordsCrispAsrActive;

    public Window? Window { get; set; }
    public List<ISpeechToTextEngine> Engines { get; set; }
    public ISpeechToTextEngine? SelectedEngine { get; set; }

    public bool OkPressed { get; private set; }

    public SpeechToTextAdvancedViewModel()
    {
        Parameters = string.Empty;
        HelpText = string.Empty;
        Engines = new List<ISpeechToTextEngine>();
    }

    private void RefreshVadCpp(ISpeechToTextEngine engine)
    {
        IsWhisperCppVisible = engine.Name == WhisperEngineCpp.StaticName;
        IsWhisperXxlVisible = engine.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName;
        IsWhisperCTranslate2Visible = engine.Name == WhisperEngineCTranslate2.StaticName;
        IsCrispAsrVisible = engine is ICrispAsrEngine;
        SelectedEngine = Engines.FirstOrDefault(p => p.Name == engine.Name);
    }

    [RelayCommand]
    private async Task EngineClicked(ISpeechToTextEngine engine)
    {
        var helpText = await engine.GetHelpText();
        HelpText = engine.Name + Environment.NewLine + Environment.NewLine + helpText;
        RefreshVadCpp(engine);
        SelectedEngine = Engines.FirstOrDefault(p => p.Name == engine.Name);
        Parameters = engine.CommandLineParameter;
    }

    [RelayCommand]
    private void EnableVadCpp()
    {
        if (IsVadCppActive)
        {
            Parameters = RemoveParameters(VadRegex);
            return;
        }

        var fileName = GetVadCppFile();
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        Parameters = AddParameters(VadRegex, $"--vad --vad-model \"{fileName}\"");
    }

    // Both are switches, not key/value pairs - whisper-cli parses a following "true" as an
    // input file name and then bails out with "input file not found 'true'".
    private const string WordLevelCppParameters = "-owts -ojf";

    private static readonly Regex WordLevelCppRegex = new(
        @"(?:^|\s)(-owts|--output-words|-ojf|--output-json-full)(?=\s|$)",
        RegexOptions.Compiled);

    private static readonly Regex VadRegex = new(
        @"(?:^|\s)(--vad-model\s+""[^""]+""|--vad-model\s+\S+|--vad)(?=\s|$)",
        RegexOptions.Compiled);

    private static readonly Regex VadCTranslate2Regex = new(
        @"(?:^|\s)--vad_filter\s+\S+(?=\s|$)",
        RegexOptions.Compiled);

    private static readonly Regex HighlightWordsCTranslate2Regex = new(
        @"(?:^|\s)(--highlight_words\s+\S+|--word_timestamps\s+\S+)(?=\s|$)",
        RegexOptions.Compiled);

    private static readonly Regex HighlightWordsCrispAsrRegex = new(
        @"(?:^|\s)(-ml\s+\S+|--max-len\s+\S+|-sow|--split-on-word)(?=\s|$)",
        RegexOptions.Compiled);

    private static readonly Regex HighlightWordsCrispAsrConflictRegex = new(
        @"(?:^|\s)(-ml\s+\S+|--max-len\s+\S+|-sow|--split-on-word|--split-on-punct|-sp)(?=\s|$)",
        RegexOptions.Compiled);

    private string RemoveParameters(Regex regex)
    {
        return regex.Replace(Parameters ?? string.Empty, string.Empty).Trim();
    }

    private string AddParameters(Regex regex, string parameters)
    {
        return AddParameters(regex, parameters, Parameters ?? string.Empty);
    }

    private static string AddParameters(Regex regex, string parameters, string current)
    {
        var existing = regex.Replace(current, string.Empty).Trim();
        return string.IsNullOrWhiteSpace(existing) ? parameters : existing + " " + parameters;
    }

    // Every toggle reads its state back out of the parameter text, so typing in the box by
    // hand or switching engine leaves the buttons showing what is actually set.
    partial void OnParametersChanged(string value)
    {
        var text = value ?? string.Empty;
        IsWordLevelCppActive = WordLevelCppRegex.IsMatch(text);
        IsVadCppActive = VadRegex.IsMatch(text);
        IsVadCrispAsrActive = IsVadCppActive;
        IsVadCTranslate2Active = VadCTranslate2Regex.IsMatch(text);
        IsHighlightWordsCTranslate2Active = HighlightWordsCTranslate2Regex.IsMatch(text);
        IsHighlightWordsCrispAsrActive = HighlightWordsCrispAsrRegex.IsMatch(text);
    }

    [RelayCommand]
    private void EnableWordLevelCpp()
    {
        // A press used to overwrite the parameter box and there was no way to press it back
        // off again, so word-level output stayed on for every later transcription.
        Parameters = IsWordLevelCppActive
            ? RemoveParameters(WordLevelCppRegex)
            : AddParameters(WordLevelCppRegex, WordLevelCppParameters);
    }

    [RelayCommand]
    private void WhisperXxlSettingStandard()
    {
        Parameters = "--standard";
    }

    [RelayCommand]
    private void WhisperXxlSettingStandardAsia()
    {
        Parameters = "--standard_asia";
    }

    [RelayCommand]
    private void WhisperXxlSettingSentence()
    {
        Parameters = "--sentence";
    }

    [RelayCommand]
    private void WhisperXxlSettingOneWord()
    {
        Parameters = "--one_word 2";
    }

    [RelayCommand]
    private void WhisperXxlSettingHighLightWord()
    {
        Parameters = "--highlight_words true --max_line_width 43 --max_line_count 2";
    }

    [RelayCommand]
    private void WhisperCTranslate2HighLightWord()
    {
        if (IsHighlightWordsCTranslate2Active)
        {
            Parameters = RemoveParameters(HighlightWordsCTranslate2Regex);
            return;
        }

        // This button has always switched the VAD filter on together with highlighting.
        var withVad = IsVadCTranslate2Active
            ? Parameters ?? string.Empty
            : AddParameters(VadCTranslate2Regex, "--vad_filter True");

        Parameters = AddParameters(
            HighlightWordsCTranslate2Regex,
            "--highlight_words True --word_timestamps True",
            withVad);
    }

    [RelayCommand]
    private void EnableVadCTranslate2()
    {
        Parameters = IsVadCTranslate2Active
            ? RemoveParameters(VadCTranslate2Regex)
            : AddParameters(VadCTranslate2Regex, "--vad_filter True");
    }

    [RelayCommand]
    private void EnableVadCrispAsr()
    {
        if (IsVadCrispAsrActive)
        {
            Parameters = RemoveParameters(VadRegex);
            return;
        }

        var fileName = GetVadCrispAsrFile();
        var vadArgs = string.IsNullOrEmpty(fileName)
            ? "--vad"
            : $"--vad --vad-model \"{fileName}\"";

        Parameters = AddParameters(VadRegex, vadArgs);
    }

    [RelayCommand]
    private void EnableHighlightWordsCrispAsr()
    {
        if (IsHighlightWordsCrispAsrActive)
        {
            Parameters = RemoveParameters(HighlightWordsCrispAsrRegex);
            return;
        }

        // The length/split switches this turns on conflict with the "standard" ones, so those
        // are dropped as it goes on - but only its own switches come off again.
        Parameters = AddParameters(HighlightWordsCrispAsrConflictRegex, "-ml 1 -sow");
    }

    [RelayCommand]
    private void StandardCrispAsr()
    {
        Parameters = "--max-len 50 --split-on-punct";
    }

    private string? GetVadCrispAsrFile()
    {
        var folder = Se.CrispAsrFolder;
        if (!Directory.Exists(folder))
        {
            return null;
        }

        var files = Directory.GetFiles(folder, "ggml-silero-v*.bin", SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
        {
            return files.OrderByDescending(p => p).First();
        }

        var fallback = Path.Combine(folder, "ggml-silero-vad.bin");
        return File.Exists(fallback) ? fallback : null;
    }

    private static string? GetVadCppFile()
    {
        var searchPaths = new List<string>
        {
            Path.Combine(Se.SpeechToTextFolder, "Cpp", "Models"),
            Path.Combine(Se.SpeechToTextFolder, "Cpp"),
        };

        foreach (var searchPath in searchPaths)
        {
            if (!Directory.Exists(searchPath))
            {
                continue;
            }

            var files = Directory.GetFiles(searchPath, "ggml-silero-v*.bin", SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                return files.OrderByDescending(p => p).First();
            }
        }

        return null;
    }

    [RelayCommand]
    private void Ok()
    {
        if (SelectedEngine != null)
        {
            SelectedEngine.CommandLineParameter = Parameters;
            Se.SaveSettings();
        }

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}