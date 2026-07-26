using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    [ObservableProperty] private ObservableCollection<RomanizeSubtitleLineItem> _subtitleItems;
    [ObservableProperty] private RomanizeSubtitleLineItem? _selectedSubtitleItem;
    [ObservableProperty] private bool _romanizeKorean = true;
    [ObservableProperty] private bool _romanizeJapanese = true;
    [ObservableProperty] private bool _romanizeRussian = true;

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
            case nameof(RomanizeJapanese):
            case nameof(RomanizeKorean):
            case nameof(RomanizeRussian):
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
        List<RomanizeSubtitleLineItem> subtitles = [.. Subtitles.Select(_ =>
        {
            string text = IRomanizer.RomanizeText(_.Text, new CultureInfo?[]
            {
                RomanizeJapanese ? JapaneseRomanizer.Culture : null,
                RomanizeKorean ? KoreanRomanizer.Culture : null,
                RomanizeRussian ? RussianRomanizer.Culture : null,

            }.OfType<CultureInfo>());

            return new RomanizeSubtitleLineItem
            {
                LineNumber = _.Number,
                Text = text,
                TextRomanized = text,
                TextOriginal = _.Text,
            };
        })];

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
            TextRomanized = IRomanizer.RomanizeText(Subtitles[index].Text, new CultureInfo?[]
            {
                RomanizeJapanese ? JapaneseRomanizer.Culture : null,
                RomanizeKorean ? KoreanRomanizer.Culture : null,
                RomanizeRussian ? RussianRomanizer.Culture : null,

            }.OfType<CultureInfo>()),
        };

        string original = subtitle.TextOriginal, romanized = subtitle.TextRomanized;

        if (subtitle.Merged)
        {
            original = subtitle.TextOriginal.Replace("\r", " ").Replace("\n", " ");
            romanized = subtitle.TextRomanized.Replace("\r", " ").Replace("\n", " ");
        }

        subtitle.Text = subtitle.RomanizedLinePosition switch
        {
            RomanizedLinePositions.Above => string.Format("{0}\n{1}", romanized, original),
            RomanizedLinePositions.Below => string.Format("{0}\n{1}", original, romanized),
            RomanizedLinePositions.Before => string.Format("{0} {1}", romanized, original),
            RomanizedLinePositions.After => string.Format("{0} {1}", original, romanized),
            RomanizedLinePositions.Replace or _ => romanized
        };

        Dispatcher.UIThread.Post(() =>
        {
            SubtitleItems[index] = subtitle;
        });
    }

    private void LoadSettings()
    { }
    private void SaveSettings()
    {
        Se.SaveSettings();
    }

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
}
