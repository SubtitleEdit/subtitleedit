using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate.LlamaCppAdvanced;

/// <summary>
/// Edits the global settings of the advanced llama.cpp translate engine: context options
/// (synopsis, glossary, rolling history), batch/style options and sampling overrides.
/// </summary>
public partial class LlamaCppAdvancedSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _synopsis = string.Empty;
    [ObservableProperty] private string _glossary = string.Empty;
    [ObservableProperty] private int _historyPairs;
    [ObservableProperty] private int _batchSize;
    [ObservableProperty] private bool _keepLineBreaks;
    [ObservableProperty] private ObservableCollection<string> _formalities = new();
    [ObservableProperty] private string? _selectedFormality;
    [ObservableProperty] private string _prompt = string.Empty;
    [ObservableProperty] private double? _temperature;
    [ObservableProperty] private double? _topP;
    [ObservableProperty] private int _topK;
    [ObservableProperty] private double? _repeatPenalty;
    [ObservableProperty] private int _maxTokens;
    [ObservableProperty] private int _contextSize;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public void Initialize()
    {
        var settings = Se.Settings.AutoTranslate.LlamaCppAdvanced;

        Formalities = new ObservableCollection<string>
        {
            SeLlamaCppAdvanced.FormalityDefault,
            SeLlamaCppAdvanced.FormalityFormal,
            SeLlamaCppAdvanced.FormalityInformal,
        };
        SelectedFormality = Formalities.Contains(settings.Formality) ? settings.Formality : Formalities[0];

        Synopsis = settings.Synopsis;
        Glossary = settings.Glossary;
        HistoryPairs = settings.HistoryPairs;
        BatchSize = settings.BatchSize;
        KeepLineBreaks = settings.KeepLineBreaks;
        Prompt = settings.Prompt;
        Temperature = settings.Temperature;
        TopP = settings.TopP;
        TopK = settings.TopK;
        RepeatPenalty = settings.RepeatPenalty;
        MaxTokens = settings.MaxTokens;
        ContextSize = settings.ContextSize;
    }

    [RelayCommand]
    private void Ok()
    {
        var settings = Se.Settings.AutoTranslate.LlamaCppAdvanced;
        settings.Synopsis = Synopsis?.Trim() ?? string.Empty;
        settings.Glossary = Glossary?.Trim() ?? string.Empty;
        settings.HistoryPairs = Math.Clamp(HistoryPairs, 0, 50);
        settings.BatchSize = Math.Clamp(BatchSize, 1, 50);
        settings.KeepLineBreaks = KeepLineBreaks;
        settings.Formality = SelectedFormality ?? SeLlamaCppAdvanced.FormalityDefault;
        settings.Prompt = Prompt?.Trim() ?? string.Empty;
        settings.Temperature = Temperature ?? -1;
        settings.TopP = TopP ?? -1;
        settings.TopK = TopK;
        settings.RepeatPenalty = RepeatPenalty ?? -1;
        settings.MaxTokens = MaxTokens;
        settings.ContextSize = Math.Clamp(ContextSize, 2048, 262144);
        Se.SaveSettings();

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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/auto-translate-advanced");
        }
    }
}
