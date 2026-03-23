using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Files.FormatProperties.TmpegEncXmlProperties;

public partial class TmpegEncXmlPropertiesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _fontNames;
    [ObservableProperty] private string? _selectedFontName;
    [ObservableProperty] private decimal _fontHeight;
    [ObservableProperty] private decimal _offsetX;
    [ObservableProperty] private decimal _offsetY;
    [ObservableProperty] private bool _isFontBold;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public TmpegEncXmlPropertiesViewModel()
    {
        FontNames = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        var fn = Enumerable.FirstOrDefault<string>(FontNames, p => p == Se.Settings.Formats.TmpegEncXmlFontName);
        SelectedFontName = fn ?? Enumerable.FirstOrDefault<string>(FontNames);

        FontHeight = Se.Settings.Formats.TmpegEncXmlFontHeight;
        OffsetX = Se.Settings.Formats.TmpegEncXmlOffsetX;
        OffsetY = Se.Settings.Formats.TmpegEncXmlOffsetY;
        IsFontBold = Se.Settings.Formats.TmpegEncXmlFontBold;
    }

    private void SaveSettings()
    {
        Se.Settings.Formats.TmpegEncXmlFontName = SelectedFontName ?? new SeFormats().TmpegEncXmlFontName;
        Se.Settings.Formats.TmpegEncXmlFontHeight = FontHeight;
        Se.Settings.Formats.TmpegEncXmlOffsetX = OffsetX;
        Se.Settings.Formats.TmpegEncXmlOffsetY = OffsetY;
        Se.Settings.Formats.TmpegEncXmlFontBold = IsFontBold;

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
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