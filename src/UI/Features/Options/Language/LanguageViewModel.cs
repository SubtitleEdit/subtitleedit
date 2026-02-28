using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Language;

public partial class LanguageViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<LanguageItem> _languages;
    [ObservableProperty] private LanguageItem? _selectedLanguage;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public LanguageViewModel()
    {
        Languages = new ObservableCollection<LanguageItem>();
    }

    [RelayCommand]
    private void Ok()
    {
        if (SelectedLanguage == null)
        {
            return;
        }

        Se.Settings.General.Language = SelectedLanguage.Name;
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

    internal void OnLoaded()
    {
        var jsonFiles = System.IO.Directory.GetFiles(Se.TranslationFolder, "*.json", System.IO.SearchOption.TopDirectoryOnly);
        Languages.Clear();
        foreach (var file in jsonFiles.OrderBy(p => p))
        {
            var language = System.IO.Path.GetFileNameWithoutExtension(file);
            if (!string.IsNullOrEmpty(language))
            {
                Languages.Add(new LanguageItem
                {
                    FileName = file,
                    Name = language,
                });
            }
        }

        SelectedLanguage = Languages.FirstOrDefault(p => p.Name == Se.Settings.General.Language) ?? Languages.FirstOrDefault();
    }
}