using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
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
            engineType == typeof(OpenAiCompatibleTranslate) ||
            engineType == typeof(OllamaTranslate) ||
            engineType == typeof(LmStudioTranslate) ||
            engineType == typeof(AnthropicTranslate) ||
            engineType == typeof(PerplexityTranslate) ||
            engineType == typeof(GroqTranslate) ||
            engineType == typeof(OpenRouterTranslate) ||
            engineType == typeof(NvidiaTranslate) ||
            engineType == typeof(MistralTranslate) ||
            engineType == typeof(GeminiTranslate) ||
            engineType == typeof(DeepSeekTranslate) ||
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
        Se.Settings.AutoTranslate.EngineStrategies[AutoTranslator.Name] =
            SelectedMergeOptions == Se.Language.Translate.TranslateEachLineSeparately
                ? nameof(TranslateStrategy.TranslateEachLineSeparately)
                : nameof(TranslateStrategy.Default);
        var translate = AutoTranslator as IAutoTranslator;
        if (translate != null)
        {
            var engineType = AutoTranslator.GetType();
            if (engineType == typeof(ChatGptTranslate))
            {
                Se.Settings.AutoTranslate.ChatGptPrompt = PromptText;
                Configuration.Settings.Tools.ChatGptPrompt = PromptText;
            }
            else if (engineType == typeof(OpenAiCompatibleTranslate))
            {
                Se.Settings.AutoTranslate.OpenAiCompatiblePrompt = PromptText;
                Configuration.Settings.Tools.OpenAiCompatibleTranslatePrompt = PromptText;
            }
            else if (engineType == typeof(OllamaTranslate))
            {
                Se.Settings.AutoTranslate.OllamaPrompt = PromptText;
                Configuration.Settings.Tools.OllamaPrompt = PromptText;
            }
            else if (engineType == typeof(LmStudioTranslate))
            {
                Se.Settings.AutoTranslate.LmStudioPrompt = PromptText;
                Configuration.Settings.Tools.LmStudioPrompt = PromptText;
            }
            else if (engineType == typeof(AnthropicTranslate))
            {
                Se.Settings.AutoTranslate.AnthropicPrompt = PromptText;
                Configuration.Settings.Tools.AnthropicPrompt = PromptText;
            }
            else if (engineType == typeof(PerplexityTranslate))
            {
                Se.Settings.AutoTranslate.PerplexityPrompt = PromptText;
                Configuration.Settings.Tools.PerplexityPrompt = PromptText;
            }
            else if (engineType == typeof(GroqTranslate))
            {
                Se.Settings.AutoTranslate.GroqPrompt = PromptText;
                Configuration.Settings.Tools.GroqPrompt = PromptText;
            }
            else if (engineType == typeof(OpenRouterTranslate))
            {
                Se.Settings.AutoTranslate.OpenRouterPrompt = PromptText;
                Configuration.Settings.Tools.OpenRouterPrompt = PromptText;
            }
            else if (engineType == typeof(NvidiaTranslate))
            {
                Se.Settings.AutoTranslate.NvidiaPrompt = PromptText;
                Configuration.Settings.Tools.NvidiaPrompt = PromptText;
            }
            else if (engineType == typeof(MistralTranslate))
            {
                Se.Settings.AutoTranslate.MistralPrompt = PromptText;
                Configuration.Settings.Tools.AutoTranslateMistralPrompt = PromptText;
            }
            else if (engineType == typeof(GeminiTranslate))
            {
                Se.Settings.AutoTranslate.GeminiPrompt = PromptText;
                Configuration.Settings.Tools.GeminiPrompt = PromptText;
            }
            else if (engineType == typeof(DeepSeekTranslate))
            {
                Se.Settings.AutoTranslate.DeepSeekPrompt = PromptText;
                Configuration.Settings.Tools.DeepSeekPrompt = PromptText;
            }
            else if (engineType == typeof(LlamaCppTranslate))
            {
                Se.Settings.AutoTranslate.LlamaCppPrompt = PromptText;
                Configuration.Settings.Tools.LlamaCppPrompt = PromptText;
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
            Se.Language.General.Default,
            Se.Language.Translate.TranslateEachLineSeparately,
        };
        SelectedMergeOptions = Se.Settings.AutoTranslate.IsTranslateEachLineSeparately(AutoTranslator.Name)
            ? MergeOptions[1]
            : MergeOptions[0];

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
        else if (engineType == typeof(OpenAiCompatibleTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.OpenAiCompatiblePrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().OpenAiCompatiblePrompt;
            }
        }
        else if (engineType == typeof(OllamaTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.OllamaPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().OllamaPrompt;
            }
        }
        else if (engineType == typeof(LmStudioTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.LmStudioPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().LmStudioPrompt;
            }
        }
        else if (engineType == typeof(AnthropicTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.AnthropicPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().AnthropicPrompt;
            }
        }
        else if (engineType == typeof(PerplexityTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.PerplexityPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().PerplexityPrompt;
            }
        }
        else if (engineType == typeof(GroqTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.GroqPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().GroqPrompt;
            }
        }
        else if (engineType == typeof(OpenRouterTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.OpenRouterPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().OpenRouterPrompt;
            }
        }
        else if (engineType == typeof(NvidiaTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.NvidiaPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().NvidiaPrompt;
            }
        }
        else if (engineType == typeof(MistralTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.MistralPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().MistralPrompt;
            }
        }
        else if (engineType == typeof(GeminiTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.GeminiPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().GeminiPrompt;
            }
        }
        else if (engineType == typeof(DeepSeekTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.DeepSeekPrompt;
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                PromptText = new SeAutoTranslate().DeepSeekPrompt;
            }
        }
        else if (engineType == typeof(LlamaCppTranslate))
        {
            PromptText = Se.Settings.AutoTranslate.LlamaCppPrompt;
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