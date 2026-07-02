using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.AiReview;

public partial class AiReviewPromptViewModel : ObservableObject
{
    [ObservableProperty] private string _promptText;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public AiReviewPromptViewModel()
    {
        PromptText = string.Empty;
    }

    public void Initialize()
    {
        PromptText = string.IsNullOrWhiteSpace(Se.Settings.Tools.AiReview.Prompt)
            ? SeAiReview.DefaultPrompt
            : Se.Settings.Tools.AiReview.Prompt;
    }

    [RelayCommand]
    private void ResetToDefault()
    {
        PromptText = SeAiReview.DefaultPrompt;
    }

    [RelayCommand]
    private void Ok()
    {
        Se.Settings.Tools.AiReview.Prompt = string.IsNullOrWhiteSpace(PromptText)
            ? SeAiReview.DefaultPrompt
            : PromptText.Trim();
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
    }
}
