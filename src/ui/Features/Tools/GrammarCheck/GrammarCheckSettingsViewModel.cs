using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.GrammarCheck;

/// <summary>
/// The LanguageTool settings that are not worth a spot in the main toolbar: the credentials a
/// premium (or protected self-hosted) server wants, the rules to ignore and the request size.
/// </summary>
public partial class GrammarCheckSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _username;
    [ObservableProperty] private string _apiKey;
    [ObservableProperty] private string _disabledRules;
    [ObservableProperty] private int _maxLinesPerBatch;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public GrammarCheckSettingsViewModel()
    {
        _username = string.Empty;
        _apiKey = string.Empty;
        _disabledRules = string.Empty;
        _maxLinesPerBatch = 25;
    }

    public void Initialize()
    {
        var settings = Se.Settings.Tools.GrammarCheck;
        Username = settings.Username;
        ApiKey = settings.ApiKey;
        DisabledRules = settings.DisabledRules;
        MaxLinesPerBatch = settings.MaxLinesPerBatch;
    }

    [RelayCommand]
    private void Ok()
    {
        var settings = Se.Settings.Tools.GrammarCheck;
        settings.Username = Username.Trim();
        settings.ApiKey = ApiKey.Trim();
        settings.DisabledRules = DisabledRules.Trim();
        settings.MaxLinesPerBatch = MaxLinesPerBatch < 1 ? 1 : MaxLinesPerBatch;
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
