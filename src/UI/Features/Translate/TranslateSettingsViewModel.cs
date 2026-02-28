using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class TranslateSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _mergeOptions;
    [ObservableProperty] private string _selectedMergeOptions;

    [ObservableProperty] private decimal? _serverDelaySeconds;
    [ObservableProperty] private decimal? _maxBytesRequest;

    [ObservableProperty] private string _promptText;
    [ObservableProperty] private bool _promptIsVisible;

    public TranslateSettingsWindow? Window { get; internal set; }
    public IAutoTranslator? AutoTranslator { get; internal set; }
    public bool OkPressed { get; private set; }

    public TranslateSettingsViewModel()
    {
        MergeOptions = [];
        SelectedMergeOptions = string.Empty;
        PromptText = string.Empty;
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private async Task Ok()
    {
        if (AutoTranslator == null)
        {
            return;
        }

        var engineType = AutoTranslator.GetType();
        if (engineType == typeof(ChatGptTranslate) ||
            engineType == typeof(OllamaTranslate) ||
            engineType == typeof(LmStudioTranslate) ||
            engineType == typeof(AnthropicTranslate) ||  
            engineType == typeof(PerplexityTranslate) ||  
            engineType == typeof(GroqTranslate) ||
            engineType == typeof(OpenRouterTranslate) ||
            engineType == typeof(LlamaCppTranslate))
        {
            if (!PromptText.Contains("{0}") || !PromptText.Contains("{1}"))
            {
                await MessageBox.Show(Window!, "Error",
                    "Prompt must contain {0} (source language) and {1} (target language)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        if (PromptText.Replace("{0}", string.Empty).Replace("{1}", string.Empty).Contains('{'))
        {
            await MessageBox.Show(Window!, "Error", "Character not allowed in prompt: '{' (besides '{0}' and '{1}')", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (PromptText.Replace("{0}", string.Empty).Replace("{1}", string.Empty).Contains('}'))
        {
            await MessageBox.Show(Window!, "Error", "Character not allowed in prompt: '}' (besides '{0}' and '{1}')", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        OkPressed = true;
        SaveValues();
        Window?.Close();
    }

    public void SaveValues()
    {
        if (AutoTranslator == null)
        {
            return;
        }

        Se.Settings.AutoTranslate.RequestDelaySeconds = ServerDelaySeconds ?? 0;
        Se.Settings.AutoTranslate.RequestMaxBytes = MaxBytesRequest ?? 0;
        var translate = AutoTranslator as IAutoTranslator;
        if (translate != null)
        {
            var engineType = AutoTranslator.GetType();
            if (engineType == typeof(ChatGptTranslate))
            {
                Se.Settings.AutoTranslate.ChatGptPrompt = PromptText;
            }
            else if (engineType == typeof(OllamaTranslate))
            {
                Se.Settings.Tools.OllamaPrompt = PromptText;
            }
            else if (engineType == typeof(LmStudioTranslate))
            {
                Se.Settings.Tools.LmStudioPrompt = PromptText;
            }
            else if (engineType == typeof(AnthropicTranslate))
            {
                Se.Settings.Tools.AnthropicPrompt = PromptText;
            }
            else if (engineType == typeof(PerplexityTranslate))
            {
                Se.Settings.Tools.PerplexityPrompt = PromptText;
            }
            else if (engineType == typeof(GroqTranslate))
            {
                Se.Settings.Tools.GroqPrompt = PromptText;
            }
            else if (engineType == typeof(OpenRouterTranslate))
            {
                Se.Settings.Tools.OpenRouterPrompt = PromptText;
            }
            else if (engineType == typeof(LlamaCppTranslate))
            {
                Se.Settings.Tools.LlamaCppPrompt = PromptText;
            }
        }

        Se.SaveSettings();
    }

    public void LoadValues(IAutoTranslator translator)
    {
        AutoTranslator = translator;
        if (AutoTranslator == null)
        {
            return;
        }

        MergeOptions = new ObservableCollection<string>
        {
            "Default",
            "Translate each line separately",
        };
        SelectedMergeOptions = MergeOptions[0];

        ServerDelaySeconds = Se.Settings.AutoTranslate.RequestDelaySeconds;
        MaxBytesRequest = Se.Settings.AutoTranslate.RequestMaxBytes;
        PromptText = string.Empty;
        PromptIsVisible = true;

        var engineType = AutoTranslator.GetType();
        if (engineType == typeof(ChatGptTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.ChatGptPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().ChatGptPrompt;
            }
        }
        else if (engineType == typeof(OllamaTranslate))
        {
            PromptText = Se.Settings.Tools.OllamaPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().OllamaPrompt;
            }
        }
        else if (engineType == typeof(LmStudioTranslate))
        {
            PromptText = Se.Settings.Tools.LmStudioPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().LmStudioPrompt;
            }
        }
        else if (engineType == typeof(AnthropicTranslate))
        {
            PromptText = Se.Settings.Tools.AnthropicPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().AnthropicPrompt;
            }
        }
        else if (engineType == typeof(PerplexityTranslate))
        {
            PromptText = Se.Settings.Tools.PerplexityPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().PerplexityPrompt;
            }
        }
        else if (engineType == typeof(GroqTranslate))
        {
            PromptText = Se.Settings.Tools.GroqPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().GroqPrompt;
            }
        }
        else if (engineType == typeof(OpenRouterTranslate))
        {
            PromptText = Se.Settings.Tools.OpenRouterPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().OpenRouterPrompt;
            }
        }
        else if (engineType == typeof(LlamaCppTranslate))
        {
            PromptText = Se.Settings.Tools.LlamaCppPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().LlamaCppPrompt;
            }
        }
        else
        {
            PromptIsVisible = false;
        }
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Onloaded(object? sender, RoutedEventArgs e)
    {
        UiUtil.RestoreWindowPosition(Window);
    }

    internal void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        UiUtil.SaveWindowPosition(Window);
    }
}