using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Files.ExportDvbTeletext;

public partial class ExportDvbTeletextViewModel : ObservableObject
{
    // 888 is the page subtitles are transmitted on in most of Europe.
    [ObservableProperty] private int _pageNumber = 888;
    [ObservableProperty] private string _languageCode;
    [ObservableProperty] private ObservableCollection<DvbTeletextSubtitleTypeItem> _subtitleTypes;
    [ObservableProperty] private DvbTeletextSubtitleTypeItem _selectedSubtitleType;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>
    /// The DVB teletext descriptor announces a page either as subtitles (teletext_type 0x02)
    /// or as subtitles for the hearing impaired (0x05) - a receiver lists them separately, so a
    /// broadcaster carrying both needs to say which one this page is.
    /// </summary>
    public bool HearingImpaired => SelectedSubtitleType?.IsHearingImpaired ?? false;

    public ExportDvbTeletextViewModel()
    {
        LanguageCode = "eng";
        SubtitleTypes =
        [
            new DvbTeletextSubtitleTypeItem(Se.Language.File.Export.ExportDvbTeletextSubtitleTypeNormal, false),
            new DvbTeletextSubtitleTypeItem(Se.Language.File.Export.ExportDvbTeletextSubtitleTypeHearingImpaired, true),
        ];
        SelectedSubtitleType = SubtitleTypes[0];
    }

    public void Initialize(int pageNumber, string languageCode, bool hearingImpaired)
    {
        PageNumber = pageNumber;
        LanguageCode = languageCode;
        SelectedSubtitleType = SubtitleTypes.First(t => t.IsHearingImpaired == hearingImpaired);
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
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
    }
}
