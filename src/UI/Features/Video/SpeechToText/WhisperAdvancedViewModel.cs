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
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.SpeechToText;

public partial class WhisperAdvancedViewModel : ObservableObject
{
    [ObservableProperty] private string _parameters;
    [ObservableProperty] private string _helpText;
    [ObservableProperty] private bool _isWhisperCppVisible;
    [ObservableProperty] private bool _isWhisperXxlVisible;
    [ObservableProperty] private bool _isWhisperCTranslate2Visible;

    public Window? Window { get; set; }
    public List<ISpeechToTextEngine> Engines { get; set; }
    public ISpeechToTextEngine? SelectedEngine { get; set; }

    public bool OkPressed { get; private set; }

    public WhisperAdvancedViewModel()
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
        SelectedEngine = Engines.FirstOrDefault(p => p.Name == engine.Name);
    }

    [RelayCommand]
    private async Task EngineClicked(ISpeechToTextEngine engine)
    {
        var helpText = await engine.GetHelpText();
        HelpText = engine.Name + Environment.NewLine + Environment.NewLine + helpText;
        RefreshVadCpp(engine);
        SelectedEngine = Engines.FirstOrDefault(p => p.Name == engine.Name);
    }

    [RelayCommand]
    private void EnableVadCpp()
    {
        var fileName = GetVadCppFile();
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        Parameters = $"--vad --vad-model \"{fileName}\"";
    }

    [RelayCommand]
    private void EnableWordLevelCpp()
    {
        Parameters = "-owts true -ojf true";
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
        Parameters = "--vad_filter True --highlight_words True --word_timestamps True";
    }

    [RelayCommand]
    private void EnableVadCTranslate2()
    {
        Parameters = "--vad_filter True";
    }

    private static string? GetVadCppFile()
    {
        var searchPaths = new List<string>
        {
            Path.Combine(Se.WhisperFolder, "Cpp", "Models"),
            Path.Combine(Se.WhisperFolder, "Cpp"),
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