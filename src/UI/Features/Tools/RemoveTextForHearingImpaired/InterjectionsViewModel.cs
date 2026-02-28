using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.RemoveTextForHearingImpaired;

public partial class InterjectionsViewModel : ObservableObject
{
    [ObservableProperty] private string _interjectionsText;
    [ObservableProperty] private string _interjectionsSkipStartText;
    
    public Window? Window { get; set; }
    
    public bool OkPressed { get; private set; }

    private string _languageCode;

    public InterjectionsViewModel()
    {
        InterjectionsText = string.Empty;
        InterjectionsSkipStartText = string.Empty;
        _languageCode = string.Empty;
    }
    
    [RelayCommand]                   
    private void Ok() 
    {
        var interjectionLanguage = Se.Settings.Tools.RemoveTextForHi.Interjections.FirstOrDefault(p =>
            p.LanguageCode == _languageCode);
        
        if (interjectionLanguage == null)
        {
            return;
        }
        
        interjectionLanguage.Interjections = InterjectionsText.SplitToLines()
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .ToList();
        
        interjectionLanguage.SkipStartList = InterjectionsSkipStartText.SplitToLines()
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .ToList();
        
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

    public void Initialize(RemoveTextForHearingImpairedViewModel.LanguageItem? selectedLanguage)
    {
        if (selectedLanguage == null)
        {
            throw new ArgumentNullException(nameof(selectedLanguage), "Selected language cannot be null");
        }

        _languageCode = selectedLanguage.Code;
        var interjectionLanguage = Se.Settings.Tools.RemoveTextForHi.Interjections.FirstOrDefault(p =>
                                    p.LanguageCode == _languageCode);
        
        if (interjectionLanguage == null)
        {
            return;
        }
        
        InterjectionsText = string.Join(Environment.NewLine, interjectionLanguage.Interjections);
        InterjectionsSkipStartText = string.Join(Environment.NewLine, interjectionLanguage.SkipStartList);
    }
}