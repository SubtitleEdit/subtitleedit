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
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeCasing;

public partial class FixNamesViewModel : ObservableObject, IClosingCleanup
{
    [ObservableProperty] private ObservableCollection<FixNameItem> _names;
    [ObservableProperty] private ObservableCollection<FixNameHitItem> _hits;
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
    private static readonly string[] CommonWords = ["US", "Lane", "Bill", "Rose"];
    private readonly HashSet<string> _usedNames;

    public FixNamesViewModel()
    {
        Names = new ObservableCollection<FixNameItem>();
        Hits = new ObservableCollection<FixNameHitItem>();

        _nameListInclMulti = new List<string>();
        _language = "en_US";
        _subtitleBefore = new Subtitle();
        _subtitle = new Subtitle();
        _usedNames = new HashSet<string>();
        ExtraNames = string.Empty;
        Info = string.Empty;
        Subtitle = new Subtitle();
    }

    internal void Initialize(Subtitle subtitle)
    {
        subtitle.Renumber();
        _subtitle = new Subtitle(subtitle);
        _subtitleBefore = subtitle;
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

        const string english = "en";
        const string dont = "don't";

        foreach (var name in _nameListInclMulti)
        {
            // filter out invalid names
            if (name.Length <= 1 || name == name.ToLowerInvariant())
            {
                continue;
            }

            var startIndex = text.IndexOf(name, StringComparison.OrdinalIgnoreCase);
            while (startIndex >= 0)
            {
                if (IsWordBoundary(text, startIndex, name) && !text.AsSpan().Slice(startIndex, name.Length).Equals(name, StringComparison.Ordinal)) // do not add names where casing already is correct
                {
                    if (!_usedNames.Contains(name))
                    {
                        var skip = false;
                        var isChecked = true;
                        if (_language.StartsWith(english, StringComparison.OrdinalIgnoreCase))
                        {
                            skip = text.AsSpan()[startIndex..].StartsWith(dont, StringComparison.OrdinalIgnoreCase);
                            isChecked = !CommonWords.Contains(name);
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

        foreach (var item in names)
        {
            // Single notification point: checkbox clicks, the space toggle and
            // select-all/invert all go through IsChecked, so no call site can
            // forget to refresh the preview.
            item.PropertyChanged += (_, _) => RequestPreview();
        }

        Names.Clear();
        Names.AddRange(names);
    }

    private static bool IsWordBoundary(string text, int startIndex, string name)
    {
        var afterNameIndex = startIndex + name.Length;
        return (startIndex == 0 || PrefixChars.Contains(text[startIndex - 1]))
               && (afterNameIndex == text.Length || SuffixChars.Contains(text[afterNameIndex]));
    }

    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isClosing;

    private void RequestPreview(int delayMilliseconds = 500)
    {
        if (_isClosing)
        {
            return;
        }

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        _ = DebouncedPreviewAsync(delayMilliseconds, _cancellationTokenSource.Token);
    }

    private async Task DebouncedPreviewAsync(int delayMilliseconds, CancellationToken token)
    {
        try
        {
            await Task.Delay(delayMilliseconds, token).ConfigureAwait(false);

            // Snapshot on the UI thread - FindAllNames mutates Names there, so the
            // background computation below must not enumerate the live collection.
            var activeNames = await Dispatcher.UIThread.InvokeAsync(() =>
                Names.Where(n => n.IsChecked).Select(n => n.Name).ToArray());
            token.ThrowIfCancellationRequested();

            GeneratePreview(activeNames);
        }
        catch (OperationCanceledException)
        {
            // Superseded by a newer request, or the window closed.
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Fix names preview failed");
        }
    }

    public void OnClosingCleanup()
    {
        // Null out so a repeated Closed callback (or a late RequestPreview) never
        // touches the disposed source.
        _isClosing = true;
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
    }

    private void GeneratePreview(string[] activeNames)
    {
        var hits = new List<FixNameHitItem>();

        // reusable array
        var processingNames = new string[1];

        foreach (var p in _subtitle.Paragraphs)
        {
            var text = p.Text;
            foreach (var name in activeNames)
            {
                // no extra processing if paragraph doesn't contain name
                if (!text.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var textNoTags = HtmlUtil.RemoveHtmlTags(text, true);

                // has letter and not already uppercase
                if (textNoTags != textNoTags.ToUpperInvariant())
                {
                    var st = new StrippableText(text);
                    processingNames[0] = name;
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
        });
    }

    [RelayCommand]
    public void NamesSelectAll()
    {
        foreach (var name in Names)
        {
            name.IsChecked = true; // PropertyChanged requests the preview
        }
    }

    [RelayCommand]
    public void NamesInvertSelection()
    {
        foreach (var name in Names)
        {
            name.IsChecked = !name.IsChecked; // PropertyChanged requests the preview
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
        FindAllNames();
        RequestPreview(0);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            CancelCommand.Execute(null);
        }
    }

    internal async void OnLoaded(RoutedEventArgs e)
    {
        try
        {
            // Must finish before NameList reads Se.DictionariesFolder — on first
            // run the folder is only populated by this unpack.
            await DictionaryLoader.UnpackIfNotFound();
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Failed to unpack bundled dictionaries");
        }

        _nameList = new NameList(Se.DictionariesFolder, _language, false, string.Empty);

        ExtraNames = Se.Settings.Tools.ChangeCasing.ExtraNames;
        FindAllNames();
        RequestPreview(0);
    }
}
