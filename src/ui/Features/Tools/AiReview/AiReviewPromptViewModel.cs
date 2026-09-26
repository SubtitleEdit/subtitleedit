using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

/// <summary>
/// Drafts the reference context with the review engine: (owner window for engine dialogs,
/// progress (part, part count - 0 means "writing synopsis"), token) -> generated text, or null
/// when the engine could not be prepared (the user was already told why).
/// </summary>
public delegate Task<string?> AiReviewContextGenerate(Window owner, Action<int, int> progress, CancellationToken cancellationToken);

public partial class AiReviewPromptViewModel : ObservableObject
{
    [ObservableProperty] private string _promptText;
    [ObservableProperty] private string _contextText;
    [ObservableProperty] private ObservableCollection<AiReviewPromptTemplate> _templates;
    [ObservableProperty] private AiReviewPromptTemplate? _selectedTemplate;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private bool _canGenerate;
    [ObservableProperty] private string _generateButtonText;
    [ObservableProperty] private string _generateStatus;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private AiReviewContextGenerate? _generateContext;
    private CancellationTokenSource? _generateCancellation;

    public AiReviewPromptViewModel()
    {
        PromptText = string.Empty;
        ContextText = string.Empty;
        Templates = new ObservableCollection<AiReviewPromptTemplate>(AiReviewPromptTemplates.GetAll());
        GenerateButtonText = Se.Language.Tools.AiReview.GenerateContext;
        GenerateStatus = string.Empty;
    }

    /// <param name="generateContext">Null hides the "Generate with AI" button (no subtitle/engine to use).</param>
    public void Initialize(AiReviewContextGenerate? generateContext = null)
    {
        PromptText = string.IsNullOrWhiteSpace(Se.Settings.Tools.AiReview.Prompt)
            ? SeAiReview.DefaultPrompt
            : Se.Settings.Tools.AiReview.Prompt;
        ContextText = Se.Settings.Tools.AiReview.Context ?? string.Empty;
        _generateContext = generateContext;
        CanGenerate = generateContext != null;
    }

    partial void OnSelectedTemplateChanged(AiReviewPromptTemplate? value)
    {
        if (value == null)
        {
            return;
        }

        PromptText = value.Prompt;

        // Back to the placeholder: the combo is an action ("fill the prompt from..."), and a kept
        // selection would claim the prompt still matches the template after the user edits it.
        Dispatcher.UIThread.Post(() => SelectedTemplate = null);
    }

    [RelayCommand]
    private void ResetToDefault()
    {
        PromptText = SeAiReview.DefaultPrompt;
    }

    [RelayCommand]
    private async Task GenerateContext()
    {
        if (IsGenerating)
        {
            _generateCancellation?.Cancel();
            return;
        }

        if (_generateContext == null || Window == null)
        {
            return;
        }

        var l = Se.Language.Tools.AiReview;
        if (!string.IsNullOrWhiteSpace(ContextText))
        {
            var answer = await MessageBox.Show(Window, l.ReferenceContext, l.ReplaceContextQuestion,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
        }

        _generateCancellation = new CancellationTokenSource();
        IsGenerating = true;
        GenerateButtonText = Se.Language.General.Stop;
        try
        {
            var result = await _generateContext(Window, (part, count) =>
            {
                GenerateStatus = part == 0
                    ? l.GeneratingContextSynopsis
                    : string.Format(l.GeneratingContextPartXOfY, part, count);
            }, _generateCancellation.Token);

            if (result == null)
            {
                return; // engine not ready - already reported
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                await MessageBox.Show(Window, l.ReferenceContext, l.NoContextGenerated, MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ContextText = result;
        }
        catch (OperationCanceledException)
        {
            // stopped by the user - keep the old context
        }
        catch (HttpRequestException e)
        {
            await MessageBox.Show(Window, Se.Language.General.Error,
                string.Format(l.EngineError, e.Message), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            IsGenerating = false;
            GenerateButtonText = l.GenerateContext;
            GenerateStatus = string.Empty;
            _generateCancellation?.Dispose();
            _generateCancellation = null;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        _generateCancellation?.Cancel();
        Se.Settings.Tools.AiReview.Prompt = string.IsNullOrWhiteSpace(PromptText)
            ? SeAiReview.DefaultPrompt
            : PromptText.Trim();
        Se.Settings.Tools.AiReview.Context = (ContextText ?? string.Empty).Trim();
        Se.SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _generateCancellation?.Cancel();
        Window?.Close();
    }

    internal void OnClosing()
    {
        _generateCancellation?.Cancel();
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
            UiUtil.ShowHelp("features/ai-review");
        }
    }
}
