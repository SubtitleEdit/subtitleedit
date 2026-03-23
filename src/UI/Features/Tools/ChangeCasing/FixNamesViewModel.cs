using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Dictionaries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

public partial class FixNamesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<FixNameItem> _names;
    [ObservableProperty] private ObservableCollection<FixNameHitItem> _hits;
    [ObservableProperty] private string _namesCount;
    [ObservableProperty] private string _hitCount;
    [ObservableProperty] private string _extraNames;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public string Info { get; private set; }
    public Subtitle Subtitle { get; private set; }

    private Subtitle _subtitle;
    private Subtitle _subtitleBefore;
    private NameList? _nameList;
    private List<string> _nameListInclMulti;
    private string _language;
    private const string ExpectedEndChars = " ,.!?:;…')]<-\"\r\n";
    private readonly HashSet<string> _usedNames;
    private string _oldNames;
    private readonly System.Timers.Timer _previewTimer;
    private bool _loading;
    private readonly object _lock = new object();

    public FixNamesViewModel()
    {
        Names = new ObservableCollection<FixNameItem>();
        Hits = new ObservableCollection<FixNameHitItem>();

        _loading = true;
        NamesCount = string.Empty;
        HitCount = string.Empty;
        _nameListInclMulti = new List<string>();
        _language = "en_US";
        _subtitleBefore = new Subtitle();
        _subtitle = new Subtitle();
        _usedNames = new HashSet<string>();
        ExtraNames = string.Empty;
        _oldNames = string.Empty;
        Info = string.Empty;
        Subtitle = new Subtitle();

        _previewTimer = new System.Timers.Timer(500);
        _previewTimer.Elapsed += (sender, args) =>
        {
            var namesString = string.Join(' ', Names.Where(p => p.IsChecked).Select(p => p.Name));
            if (namesString != _oldNames && !_loading)
            {
                lock (_lock)
                {
                    GeneratePreview();
                    _oldNames = namesString;
                }
            }
        };
    }

    internal void Initialize(Subtitle subtitle)
    {
        subtitle.Renumber();
        _subtitle = new Subtitle(subtitle);
        _subtitleBefore = subtitle;
        _oldNames = string.Empty;

        _language = LanguageAutoDetect.AutoDetectGoogleLanguage(_subtitle);
        if (string.IsNullOrEmpty(_language))
        {
            _language = "en_US";
        }
    }

    private void FindAllNames()
    {
        var text = HtmlUtil.RemoveHtmlTags(_subtitle.GetAllTexts());
        var textToLower = text.ToLowerInvariant();

        _nameListInclMulti = _nameList!.GetAllNames(); // Will contains both one word names and multi names
        foreach (var s in ExtraNames.Split(','))
        {
            var name = s.Trim();
            if (name.Length > 1 && !_nameListInclMulti.Contains(name))
            {
                _nameListInclMulti.Add(name);
            }
        }

        _usedNames.Clear();
        var names = new List<FixNameItem>();
        foreach (var name in _nameListInclMulti)
        {
            var startIndex = textToLower.IndexOf(name.ToLowerInvariant(), StringComparison.Ordinal);
            if (startIndex >= 0)
            {
                while (startIndex >= 0 && startIndex < text.Length &&
                       textToLower.Substring(startIndex).Contains(name.ToLowerInvariant()) && name.Length > 1 && name != name.ToLowerInvariant())
                {
                    var startOk = startIndex == 0 || "([ --'>\r\n¿¡\"”“„".Contains(text[startIndex - 1]);
                    if (startOk)
                    {
                        var end = startIndex + name.Length;
                        var endOk = end <= text.Length;
                        if (endOk)
                        {
                            endOk = end == text.Length || ExpectedEndChars.Contains(text[end]);
                        }

                        if (endOk && text.Substring(startIndex, name.Length) != name) // do not add names where casing already is correct
                        {
                            if (!_usedNames.Contains(name))
                            {
                                var skip = false;
                                var isChecked = true;
                                if (_language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                                {
                                    var isDont = text.Substring(startIndex).StartsWith("don't", StringComparison.InvariantCultureIgnoreCase);
                                    if (isDont)
                                    {
                                        skip = true;
                                    }

                                    var commonNamesAndWords = new List<string>
                                    {
                                        "US",
                                        "Lane",
                                        "Bill",
                                        "Rose",
                                    };
                                    if (commonNamesAndWords.Contains(name))
                                    {
                                        isChecked = false;
                                    }
                                }

                                if (!skip)
                                {
                                    _usedNames.Add(name);
                                    names.Add(new FixNameItem(name, isChecked));
                                    break; // break while
                                }
                            }
                        }
                    }

                    startIndex = textToLower.IndexOf(name.ToLowerInvariant(), startIndex + 2, StringComparison.Ordinal);
                }
            }
        }

        Names.Clear();
        Names.AddRange(names);
        NamesCount = string.Format("Names: {0:#,##0}", Names.Count);
    }

    private void GeneratePreview()
    {
        var hits = new List<FixNameHitItem>();
        foreach (var p in _subtitle.Paragraphs)
        {
            var text = p.Text;
            foreach (var item in Names)
            {
                var name = item.Name;

                var textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
                if (textNoTags != textNoTags.ToUpperInvariant())
                {
                    if (item.IsChecked && text != null && text.Contains(name, StringComparison.OrdinalIgnoreCase) && name.Length > 1 && name != name.ToLowerInvariant())
                    {
                        var st = new StrippableText(text);
                        st.FixCasing(new List<string> { name }, true, false, false, string.Empty);
                        text = st.MergedString;
                    }
                }
            }

            if (text != p.Text && p.Text != null && text != null)
            {
                var hit = new FixNameHitItem(p.Text, p.Number, p.Text, text, true);
                hits.Add(hit);
            }
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            Hits.Clear();
            Hits.AddRange(hits);
            HitCount = string.Format("Hits: {0:#,##0}", Hits.Count);
        });
    }

    [RelayCommand]
    public void NamesSelectAll()
    {
        foreach (var name in Names)
        {
            name.IsChecked = true;
        }
    }

    [RelayCommand]
    public void NamesInvertSelection()
    {
        foreach (var name in Names)
        {
            name.IsChecked = !name.IsChecked;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        Subtitle = new Subtitle(_subtitle, false);

        foreach (var hit in Hits)
        {
            if (hit.IsEnabled)
            {
                Subtitle.Paragraphs[hit.LineIndex - 1].Text = hit.After;
            }
        }

        Se.Settings.Tools.ChangeCasing.ExtraNames = ExtraNames;

        var noOfLinesChanged = 0;
        for (var i = 0; i < _subtitleBefore.Paragraphs.Count; i++)
        {
            if (_subtitleBefore.Paragraphs[i].Text != Subtitle.Paragraphs[i].Text)
            {
                noOfLinesChanged++;
            }
        }
        Info = $"Change casing - lines changed: {noOfLinesChanged}";

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    public void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    public void AddExtraName()
    {
        _loading = true;
        FindAllNames();
        _loading = false;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            CancelCommand.Execute(null);
        }
    }

    internal void OnLoaded(RoutedEventArgs e)
    {
        DictionaryLoader.UnpackIfNotFound().ConfigureAwait(false);
        _nameList = new NameList(Se.DictionariesFolder, _language, Configuration.Settings.WordLists.UseOnlineNames, Configuration.Settings.WordLists.NamesUrl);

        ExtraNames = Se.Settings.Tools.ChangeCasing.ExtraNames;
        FindAllNames();
        GeneratePreview();
        _previewTimer.Start();
        _loading = false;
    }
}
