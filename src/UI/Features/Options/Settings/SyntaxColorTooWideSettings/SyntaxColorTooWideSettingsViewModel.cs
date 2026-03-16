using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Settings.SyntaxColorTooWideSettings;

public partial class SyntaxColorTooWideSettingsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _fonts = [];
    [ObservableProperty] private string _selectedFont;
    [ObservableProperty] private int _fontSize;
    [ObservableProperty] private int _maxWidthPixels;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }


    public SyntaxColorTooWideSettingsViewModel()
    {
        Fonts = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        SelectedFont = Fonts[0];
    }

    internal void Initialize(int tooWidePixels, string fontName, int fontSize)
    {
        SelectedFont = fontName;
        FontSize = fontSize;
        MaxWidthPixels = tooWidePixels;
    }

    [RelayCommand]
    private void Ok()
    {
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