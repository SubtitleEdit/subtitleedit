using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.UiLogic.AutoTranslate;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class TranslateSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _mergeOptions;
    [ObservableProperty] private string _selectedMergeOptions;

    [ObservableProperty] private decimal? _serverDelaySeconds;
    [ObservableProperty] private int? _maxBytesRequest;

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
    private void ResetPrompt()
    {
        if (AutoTranslator == null)
        {
            return;
        }

        var defaultPrompt = GetPrompt(new SeAutoTranslate(), AutoTranslator.GetType());
        if (defaultPrompt != null)
        {
            PromptText = defaultPrompt;
        }
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
        Configuration.Settings.Tools.AutoTranslateDelaySeconds = (int)Math.Round(ServerDelaySeconds ?? 0, MidpointRounding.AwayFromZero);
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
        MaxBytesRequest = (int)Se.Settings.AutoTranslate.RequestMaxBytes;
        PromptText = string.Empty;
        PromptIsVisible = true;

        var engineType = AutoTranslator.GetType();
        var defaultPrompt = GetPrompt(new SeAutoTranslate(), engineType);
        if (defaultPrompt == null)
        {
            PromptIsVisible = false;
            return;
        }

        PromptText = GetPrompt(Se.Settings.AutoTranslate, engineType) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(PromptText))
        {
            PromptText = defaultPrompt;
        }
    }

    /// <summary>
    /// Returns the prompt of the given settings object for the engine, or null if the engine has no prompt.
    /// Pass a fresh <see cref="SeAutoTranslate"/> for the built-in default, or the current settings for the saved one.
    /// </summary>
    private static string? GetPrompt(SeAutoTranslate settings, Type engineType)
    {
        if (engineType == typeof(ChatGptTranslate))
        {
            return settings.ChatGptPrompt;
        }

        if (engineType == typeof(OpenAiCompatibleTranslate))
        {
            return settings.OpenAiCompatiblePrompt;
        }

        if (engineType == typeof(OllamaTranslate))
        {
            return settings.OllamaPrompt;
        }

        if (engineType == typeof(LmStudioTranslate))
        {
            return settings.LmStudioPrompt;
        }

        if (engineType == typeof(AnthropicTranslate))
        {
            return settings.AnthropicPrompt;
        }

        if (engineType == typeof(PerplexityTranslate))
        {
            return settings.PerplexityPrompt;
        }

        if (engineType == typeof(GroqTranslate))
        {
            return settings.GroqPrompt;
        }

        if (engineType == typeof(OpenRouterTranslate))
        {
            return settings.OpenRouterPrompt;
        }

        if (engineType == typeof(NvidiaTranslate))
        {
            return settings.NvidiaPrompt;
        }

        if (engineType == typeof(MistralTranslate))
        {
            return settings.MistralPrompt;
        }

        if (engineType == typeof(GeminiTranslate))
        {
            return settings.GeminiPrompt;
        }

        if (engineType == typeof(DeepSeekTranslate))
        {
            return settings.DeepSeekPrompt;
        }

        if (engineType == typeof(LlamaCppTranslate))
        {
            return settings.LlamaCppPrompt;
        }

        return null;
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