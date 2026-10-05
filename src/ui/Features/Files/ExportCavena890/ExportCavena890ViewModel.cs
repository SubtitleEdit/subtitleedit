using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.ExportCavena890;

public partial class ExportCavena890ViewModel : ObservableObject
{
    [ObservableProperty] private string _translatedTitle;
    [ObservableProperty] private string _originalTitle;
    [ObservableProperty] private string _translator;
    [ObservableProperty] private string _comment;
    [ObservableProperty] private string _language;
    [ObservableProperty] private TimeSpan _startOfProgramme;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public ExportCavena890ViewModel()
    {
        TranslatedTitle = string.Empty;
        OriginalTitle = string.Empty;
        Translator = Se.Settings.File.ExportCavena890Translator ?? string.Empty;
        Comment = string.Empty;
        Language = Se.Settings.File.ExportCavena890Language ?? string.Empty;
        StartOfProgramme = TimeSpan.FromMilliseconds(Math.Max(0, Se.Settings.File.ExportCavena890StartOfProgrammeMs));
    }

    [RelayCommand]
    private void Ok()
    {
        Se.Settings.File.ExportCavena890Translator = Translator ?? string.Empty;
        Se.Settings.File.ExportCavena890Language = Language ?? string.Empty;
        Se.Settings.File.ExportCavena890StartOfProgrammeMs = StartOfProgramme.TotalMilliseconds;
        Se.SaveSettings();

        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private void Importl()
    {
    }
    
    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/file", "export-to-cavena-890");
        }
    }
}