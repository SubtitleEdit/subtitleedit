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

    /// <summary>
    /// The name selected in the names list; the preview follows it to the first line it changes
    /// (like SE4), also after the preview is regenerated.
    /// </summary>
    public FixNameItem? SelectedName { get; set; }

    /// <summary>
    /// Raised on the UI thread after <see cref="Hits"/> has been replaced by a new preview.
    /// </summary>
    public event EventHandler? HitsRefreshed;
    public bool OkPressed { get; private set; }
    public string Info { get; private set; }
    public Subtitle Subtitle { get; private set; }

    private Subtitle _subtitle;
    private Subtitle _subtitleBefore;
    private NameList? _nameList;
    private List<string> _nameListInclMulti;
    private string _language;

    public FixNamesViewModel()
    {
        Names = new ObservableCollection<FixNameItem>();
        Hits = new ObservableCollection<FixNameHitItem>();

        _nameListInclMulti = new List<string>();
        _language = "en_US";
        _subtitleBefore = new Subtitle();
        _subtitle = new Subtitle();
        ExtraNames = string.Empty;
        Info = string.Empty;
        Subtitle = new Subtitle();
    }

    // Lines already changed by the step that opened this dialog (normal casing), so the
    // final "lines changed" info covers the whole operation, like SE4's combined count.
    private int _noOfFixesBefore;

    internal void Initialize(Subtitle subtitle, int noOfFixesBefore = 0)
    {
        _noOfFixesBefore = noOfFixesBefore;
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
        _nameListInclMulti = _nameList!.GetAllNames(); // Will contains both one word names and multi names

        var names = FixNamesLogic.FindNames(_subtitle, _nameListInclMulti, ExtraNames, _language)
            .Select(n => new FixNameItem(n.Name, n.IsChecked))
            .ToList();

        // Refresh (after editing extra names) must not undo the user's own check/uncheck choices (#17243).
        KeepUserChecks(names, Names);

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

    /// <summary>
    /// Names that were already listed keep the check state the user gave them; only names new to
    /// the list get the default from <see cref="FixNamesLogic.FindNames"/>.
    /// </summary>
    internal static void KeepUserChecks(IEnumerable<FixNameItem> newNames, IEnumerable<FixNameItem> oldNames)
    {
        var previous = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var old in oldNames)
        {
            previous[old.Name] = old.IsChecked;
        }

        foreach (var item in newNames)
        {
            if (previous.TryGetValue(item.Name, out var isChecked))
            {
                item.IsChecked = isChecked;
            }
        }
    }

    // Line numbers the user unchecked in the preview. Kept across preview rebuilds (Refresh, name
    // toggles) so an unchecked line stays unchecked, also when it drops out of the preview and comes
    // back later (#17243).
    private readonly HashSet<int> _disabledLines = new();

    /// <summary>
    /// Updates the remembered unchecked preview lines from the current preview. Lines not in the
    /// current preview keep what was remembered for them. Must run on the UI thread.
    /// </summary>
    internal static void UpdateDisabledLines(ISet<int> disabledLines, IEnumerable<FixNameHitItem> hits)
    {
        foreach (var hit in hits)
        {
            if (hit.IsEnabled)
            {
                disabledLines.Remove(hit.LineIndex);
            }
            else
            {
                disabledLines.Add(hit.LineIndex);
            }
        }
    }

    internal static List<FixNameHitItem> BuildHits(Subtitle subtitle, string[] activeNames, ISet<int> disabledLines, CancellationToken token = default)
    {
        var hits = new List<FixNameHitItem>();

        foreach (var p in subtitle.Paragraphs)
        {
            token.ThrowIfCancellationRequested();
            var text = FixNamesLogic.ApplyNames(p.Text, activeNames);

            if (text != p.Text)
            {
                hits.Add(new FixNameHitItem(p.Text, p.Number, p.Text, text, !disabledLines.Contains(p.Number)));
            }
        }

        return hits;
    }

    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isClosing;
    private bool _previewPending;
    private bool _suppressPreviewRequests;
    private Task _previewTask = Task.CompletedTask;

    private void RequestPreview(int delayMilliseconds = 500)
    {
        if (_isClosing || _suppressPreviewRequests)
        {
            return;
        }

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
        _previewPending = true;
        _previewTask = DebouncedPreviewAsync(delayMilliseconds, _cancellationTokenSource.Token);
    }

    private async Task DebouncedPreviewAsync(int delayMilliseconds, CancellationToken token)
    {
        try
        {
            await Task.Delay(delayMilliseconds, token).ConfigureAwait(false);

            // Snapshot on the UI thread - FindAllNames mutates Names there, so the
            // background computation below must not enumerate the live collection.
            var (activeNames, disabledLines) = await Dispatcher.UIThread.InvokeAsync(() =>
            {
                UpdateDisabledLines(_disabledLines, Hits);
                return (Names.Where(n => n.IsChecked).Select(n => n.Name).ToArray(), new HashSet<int>(_disabledLines));
            });
            token.ThrowIfCancellationRequested();

            GeneratePreview(activeNames, disabledLines, token);
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

    private void GeneratePreview(string[] activeNames, ISet<int> disabledLines, CancellationToken token)
    {
        var hits = BuildHits(_subtitle, activeNames, disabledLines, token);

        Dispatcher.UIThread.Invoke(() =>
        {
            // Cancellation happens on the UI thread, so checking here (not before the Invoke)
            // guarantees a superseded computation - one that was already past the earlier token
            // check when a newer request cancelled it - can never overwrite the newer result.
            if (token.IsCancellationRequested)
            {
                return;
            }

            Hits.Clear();
            Hits.AddRange(hits);
            _previewPending = false;
            HitsRefreshed?.Invoke(this, EventArgs.Empty);
        });
    }

    /// <summary>
    /// The first preview line the selected name changes, or null when it changes none (e.g. the
    /// name is unchecked, so it is not part of the preview). Uses the same rule that created the
    /// hit, so "US" does not match inside "USB".
    /// </summary>
    internal FixNameHitItem? FindFirstHitForSelectedName()
    {
        var name = SelectedName?.Name;
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var names = new[] { name };
        return Hits.FirstOrDefault(hit => FixNamesLogic.ApplyNames(hit.Before, names) != hit.Before);
    }

    [RelayCommand]
    public void NamesSelectAll()
    {
        SetAllNames(_ => true);
    }

    [RelayCommand]
    public void NamesInvertSelection()
    {
        SetAllNames(name => !name.IsChecked);
    }

    private void SetAllNames(Func<FixNameItem, bool> getValue)
    {
        // Each IsChecked change fires the PropertyChanged handler; suppress those
        // per-item requests during the bulk update and issue one immediate one,
        // so a whole-list toggle doesn't sit through the 500 ms debounce.
        _suppressPreviewRequests = true;
        try
        {
            foreach (var name in Names)
            {
                name.IsChecked = getValue(name);
            }
        }
        finally
        {
            _suppressPreviewRequests = false;
        }

        RequestPreview(0);
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
    private async Task Ok()
    {
        // A preview may still be pending (the 500 ms debounce, or a computation in flight), so
        // Hits can lag behind the checkboxes - applying it would use fixes for names the user
        // just deselected. Flush with a zero-delay preview and wait for it before committing.
        if (_previewPending)
        {
            RequestPreview(0);
            await _previewTask;
        }

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

        Info = $"Change casing - lines changed: {noOfLinesChanged + _noOfFixesBefore}";

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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/change-casing", "fix-names");
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
