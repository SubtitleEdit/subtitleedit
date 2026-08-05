using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.PickVobSubLanguage;

public partial class PickVobSubLanguageViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<VobSubLanguageDisplay> _languages;
    [ObservableProperty] private VobSubLanguageDisplay? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<VobSubLanguageCueDisplay> _rows;

    public Window? Window { get; set; }
    public TableView LanguagesGrid { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }

    public int SelectedStreamId { get; private set; }
    public string SelectedLanguageString { get; private set; }

    private const int PreviewCount = 20;

    private Dictionary<int, List<VobSubMergedPack>> _streamIdDictionary;
    private List<SKColor>? _palette;

    public PickVobSubLanguageViewModel()
    {
        Languages = new ObservableCollection<VobSubLanguageDisplay>();
        Rows = new ObservableCollection<VobSubLanguageCueDisplay>();
        LanguagesGrid = new TableView();
        WindowTitle = string.Empty;
        SelectedLanguageString = string.Empty;
        _streamIdDictionary = new Dictionary<int, List<VobSubMergedPack>>();
    }

    public void Initialize(Dictionary<int, List<VobSubMergedPack>> streamIdDictionary, List<SKColor> palette, List<string> idxLanguages, string fileName)
    {
        _streamIdDictionary = streamIdDictionary;
        _palette = palette;
        WindowTitle = string.Format(Se.Language.General.PickVobSubLanguageTitle, Path.GetFileName(fileName));

        foreach (var streamId in streamIdDictionary.Keys.OrderBy(k => k))
        {
            Languages.Add(new VobSubLanguageDisplay
            {
                StreamId = streamId,
                StreamIdHex = $"0x{streamId:X2}",
                Language = LookupLanguage(streamId, idxLanguages),
                Count = streamIdDictionary[streamId].Count,
            });
        }
    }

    private static string LookupLanguage(int streamId, List<string> idxLanguages)
    {
        // Idx.cs formats entries as "{LanguageName} ‎(0x{streamId:x})"
        var marker = $"(0x{streamId:x})";
        var match = idxLanguages.FirstOrDefault(l => l.Contains(marker, StringComparison.OrdinalIgnoreCase));
        return match ?? string.Empty;
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    [RelayCommand]
    private void Ok()
    {
        if (SelectedLanguage == null)
        {
            return;
        }

        SelectedStreamId = SelectedLanguage.StreamId;
        SelectedLanguageString = SelectedLanguage.Language;
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    internal void OnKeyDownHandler(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
            e.Handled = true;
        }
        else if (e.Key == Key.Enter && LanguagesGrid.IsKeyboardFocusWithin)
        {
            Ok();
            e.Handled = true;
        }
    }

    internal void LanguagesGridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        LanguageChanged();
    }

    private void LanguageChanged()
    {
        Rows.Clear();

        var selected = SelectedLanguage;
        if (selected == null || !_streamIdDictionary.TryGetValue(selected.StreamId, out var packs))
        {
            return;
        }

        for (var i = 0; i < PreviewCount && i < packs.Count; i++)
        {
            var pack = packs[i];
            if (_palette != null)
            {
                pack.Palette = _palette;
            }

            var bitmap = pack.GetBitmap();
            Rows.Add(new VobSubLanguageCueDisplay
            {
                Number = i + 1,
                Show = pack.StartTime,
                Duration = pack.EndTime - pack.StartTime,
                Image = new Image { Source = bitmap.ToAvaloniaBitmap() },
            });
        }
    }

    internal void SelectAndScrollToRow(int index)
    {
        if (index < 0 || index >= Languages.Count)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            // Select via the view model, not just the grid index - see the same fix in
            // PickMatroskaTrackViewModel: AlwaysSelected has already put the grid on row 0, so
            // re-assigning the index raises no SelectionChanged and SelectedLanguage stayed null,
            // which left the preview empty and made OK do nothing.
            SelectedLanguage = Languages[index];
            LanguagesGrid.SelectedIndex = index;
            if (LanguagesGrid.SelectedItem is { } selectedItem)
            {
                LanguagesGrid.ScrollIntoView(selectedItem);
            }

            LanguageChanged();
        }, DispatcherPriority.Background);
    }
}
