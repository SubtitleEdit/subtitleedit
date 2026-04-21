using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class TranslationErrorViewModel : ObservableObject
{
    [ObservableProperty] private string _engineName;
    [ObservableProperty] private string _friendlyMessage;
    [ObservableProperty] private string _hint;
    [ObservableProperty] private string _technicalDetails;
    [ObservableProperty] private bool _detailsExpanded;
    [ObservableProperty] private string _toggleDetailsText;

    public Window? Window { get; set; }

    public TranslationErrorViewModel()
    {
        _engineName = string.Empty;
        _friendlyMessage = string.Empty;
        _hint = string.Empty;
        _technicalDetails = string.Empty;
        _detailsExpanded = false;
        _toggleDetailsText = Se.Language.Translate.ShowTechnicalDetails;
    }

    public void Initialize(string engineName, string technicalDetails)
    {
        EngineName = engineName;
        FriendlyMessage = string.Format(Se.Language.Translate.TranslationFailedMessage, engineName);
        Hint = Se.Language.Translate.TranslationFailedHint;
        TechnicalDetails = technicalDetails;
        ToggleDetailsText = Se.Language.Translate.ShowTechnicalDetails;
    }

    partial void OnDetailsExpandedChanged(bool value)
    {
        ToggleDetailsText = value
            ? Se.Language.Translate.HideTechnicalDetails
            : Se.Language.Translate.ShowTechnicalDetails;
    }

    [RelayCommand]
    private void ToggleDetails()
    {
        DetailsExpanded = !DetailsExpanded;
    }

    [RelayCommand]
    private void Ok()
    {
        Window?.Close();
    }

    public void KeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape || e.Key == Key.Enter)
        {
            Ok();
        }
    }
}
