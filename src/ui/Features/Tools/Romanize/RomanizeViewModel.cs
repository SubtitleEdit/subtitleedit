using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Protobuf.WellKnownTypes;
using Nikse.SubtitleEdit.Core.Romanize;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.Romanize;

public partial class RomanizeViewModel : ObservableObject
{
    [ObservableProperty] private bool _romanize = true;
    [ObservableProperty] private bool _romanizeKorean = true;
    [ObservableProperty] private bool _romanizeJapanese = true;
    [ObservableProperty] private bool _romanizeRussian = true;
    [ObservableProperty] private bool? _subtitleItemsMerged;
    [ObservableProperty] private RomanizedLinePositions? _subtitleItemsRomanizedLinePosition;
    [ObservableProperty] private ObservableCollection<RomanizeSubtitleLineItem> _subtitleItems;

    protected bool _updatingRomanizeFlags = false;

    public List<SubtitleLineViewModel> Subtitles { get; }

    public RomanizeViewModel()
    {
        Subtitles = [];
        SubtitleItems = [];

        LoadSettings();
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles)
    {
        SubtitleItems.Clear();
        Subtitles.Clear();
        Subtitles.AddRange(subtitles);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(Romanize):
                if (_updatingRomanizeFlags) break;
                _updatingRomanizeFlags = true;
                RomanizeKorean = RomanizeJapanese = RomanizeRussian = Romanize;
                _updatingRomanizeFlags = false;
                RomanizeAll();
                break;
            case nameof(RomanizeJapanese):
            case nameof(RomanizeKorean):
            case nameof(RomanizeRussian):
                if (_updatingRomanizeFlags) break;
                _updatingRomanizeFlags = true;
                Romanize = RomanizeKorean && RomanizeJapanese && RomanizeRussian;
                _updatingRomanizeFlags = false;
                RomanizeAll();
                break;

            case nameof(SubtitleItemsMerged):
                if (SubtitleItemsMerged is not null)
                    RomanizeAll();
                break;

            case nameof(SubtitleItemsRomanizedLinePosition):
                if (SubtitleItemsRomanizedLinePosition is not null)
                    RomanizeAll();
                break;

            default: break;
        }
    }

    public bool OkPressed { get; private set; }
    public Window? Window { get; set; }

    [RelayCommand]
    private void Ok()
    {
        if (Window == null)
        {
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }
    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }
    [RelayCommand]
    private void RomanizeAll()
    {
        List<RomanizeSubtitleLineItem> subtitles = [.. Subtitles.Select((subtitle, index) =>
        {
            var previous = SubtitleItems.ElementAtOrDefault(index);
            var item = new RomanizeSubtitleLineItem
            {
                Merged = SubtitleItemsMerged ?? previous?.Merged ?? default,
                RomanizedLinePosition = SubtitleItemsRomanizedLinePosition ?? previous?.RomanizedLinePosition ?? default,

                LineNumber = subtitle.Number,
                TextOriginal = subtitle.Text,
                TextRomanized = TextRomanize(subtitle.Text),
            };

            item.Text = previous is null ? item.TextRomanized : TextAlter(item.TextOriginal, item.TextRomanized, item.Merged, item.RomanizedLinePosition);

            return item;
        })];

        SubtitleItemsMerged = null;
        SubtitleItemsRomanizedLinePosition = null;

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleItems.Clear();
            SubtitleItems.AddRange(subtitles);
        });
    }
    [RelayCommand]
    private void RomanizeSingle(int index)
    {
        var previous = SubtitleItems.ElementAt(index);
        var subtitle = new RomanizeSubtitleLineItem
        {
            Merged = previous.Merged,
            RomanizedLinePosition = previous.RomanizedLinePosition,

            LineNumber = Subtitles[index].Number,
            TextOriginal = Subtitles[index].Text,
            TextRomanized = TextRomanize(Subtitles[index].Text),
        };

        subtitle.Text = TextAlter(subtitle.TextOriginal, subtitle.TextRomanized, subtitle.Merged, subtitle.RomanizedLinePosition);

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleItems[index] = subtitle;
        });
    }

    private void LoadSettings() { }
    private void SaveSettings() { }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/romanize");
        }
    }
    internal string TextRomanize(string text)
    {
        return IRomanizer.RomanizeText(text, new CultureInfo?[]
        {
            RomanizeJapanese ? JapaneseRomanizer.Culture : null,
            RomanizeKorean ? KoreanRomanizer.Culture : null,
            RomanizeRussian ? RussianRomanizer.Culture : null,

        }.OfType<CultureInfo>());
    }
    internal string TextAlter(string original, string romanized, bool merge, RomanizedLinePositions position)
    {
        string
            _original = merge is false ? original : original.Replace("\r", " ").Replace("\n", " "),
            _romanized = merge is false ? romanized : romanized.Replace("\r", " ").Replace("\n", " ");

        return position switch
        {
            RomanizedLinePositions.Above => string.Format("{0}\n{1}", _romanized, _original),
            RomanizedLinePositions.Below => string.Format("{0}\n{1}", _original, _romanized),
            RomanizedLinePositions.Before => string.Format("{0} {1}", _romanized, _original),
            RomanizedLinePositions.After => string.Format("{0} {1}", _original, _romanized),
            RomanizedLinePositions.Replace or _ => _romanized
        };

    }
}