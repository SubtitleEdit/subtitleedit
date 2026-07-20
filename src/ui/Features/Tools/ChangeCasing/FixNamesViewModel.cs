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
using System.Threading;

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
    private const string PrefixChars = "([ --'>\r\n¿¡\"”“„";
    private const string SuffixChars = " ,.!?:;…')]<-\"\r\n";
    private readonly HashSet<string> _usedNames;
    private string _oldNames;
    private readonly System.Timers.Timer _previewTimer;
    private bool _loading;
    private readonly Lock _lock = new();

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

        string[] commonWords = ["US", "Lane", "Bill", "Rose"];
        const string english = "en";
        const string dont = "don't";

        foreach (var name in _nameListInclMulti)
        {
            var startIndex = text.IndexOf(name, StringComparison.OrdinalIgnoreCase);

            // filter out invalid names
            if (name.Length <= 1 || name == name.ToLowerInvariant())
            {
                continue;
            }

            while (startIndex >= 0)
            {
                if (IsWordBoundary(text, startIndex, name) && !text.AsSpan().Slice(startIndex, name.Length).Equals(name, StringComparison.Ordinal)) // do not add names where casing already is correct
                {
                    if (!_usedNames.Contains(name))
                    {
                        bool skip = false;
                        var isChecked = true;
                        if (_language.StartsWith(english, StringComparison.OrdinalIgnoreCase))
                        {
                            skip = text.AsSpan()[startIndex..].StartsWith(dont, StringComparison.OrdinalIgnoreCase);
                            isChecked = !commonWords.Contains(name);
                        }

                        if (!skip)
                        {
                            _usedNames.Add(name);
                            names.Add(new FixNameItem(name, isChecked));
                            break; // break while
                        }
                    }
                }

                startIndex = text.IndexOf(name, startIndex + name.Length, StringComparison.OrdinalIgnoreCase);
            }
        }

        Names.Clear();
        Names.AddRange(names);
        NamesCount = $"Names: {Names.Count:#,##0}";
    }

    private bool IsWordBoundary(string text, int startIndex, string name)
    {
        var afterNameIndex = startIndex + name.Length;
        return (startIndex == 0 || PrefixChars.Contains(text[startIndex - 1]))
               && (afterNameIndex == text.Length || (afterNameIndex < text.Length && SuffixChars.Contains(text[afterNameIndex])));
    }

    private void GeneratePreview()
    {
        var hits = new List<FixNameHitItem>();

        // reusable array
        var processingNames = new string[1];

        // filter out non-active name to avoid extra processing
        var activeNameItems = Names.Where(n => n.IsChecked).ToArray();
        
        foreach (var p in _subtitle.Paragraphs)
        {
            var text = p.Text;
            foreach (var item in activeNameItems)
            {
                // no extra processing if paragraph doesn't contain name
                if (!text.Contains(item.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var textNoTags = HtmlUtil.RemoveHtmlTags(text, true);
                
                // has letter and not already uppercase
                if (textNoTags != textNoTags.ToUpperInvariant())
                {
                    var st = new StrippableText(text);
                    processingNames[0] = item.Name;
                    st.FixCasing(processingNames, true, false, false, string.Empty);
                    text = st.MergedString;
                }
            }

            if (text != p.Text)
            {
                hits.Add(new FixNameHitItem(p.Text, p.Number, p.Text, text, true));
            }
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            Hits.Clear();
            Hits.AddRange(hits);
            HitCount = $"Hits: {Hits.Count:#,##0}";
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
    public void HitsSelectAll()
    {
        foreach (var hit in Hits)
        {
            hit.IsEnabled = true;
        }
    }

    [RelayCommand]
    public void HitsInvertSelection()
    {
        foreach (var hit in Hits)
        {
            hit.IsEnabled = !hit.IsEnabled;
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
        _nameList = new NameList(Se.DictionariesFolder, _language, false, string.Empty);

        ExtraNames = Se.Settings.Tools.ChangeCasing.ExtraNames;
        FindAllNames();
        GeneratePreview();
        _previewTimer.Start();
        _loading = false;
    }
}
