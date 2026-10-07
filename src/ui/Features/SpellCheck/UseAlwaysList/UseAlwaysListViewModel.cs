using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.SpellCheck;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.SpellCheck.UseAlwaysList;

/// <summary>
/// Editor for the spell check "use always" list (&lt;lang&gt;_UseAlways.xml) - the pairs stored by
/// "Change all" / "Use always". Until now the only way to get rid of a bad pair (like "and" -> "&amp;")
/// was to hand-edit the XML file. (#15767)
/// </summary>
public partial class UseAlwaysListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<UseAlwaysLanguageItem> _languages;
    [ObservableProperty] private UseAlwaysLanguageItem? _selectedLanguage;
    [ObservableProperty] private ObservableCollection<UseAlwaysPairItem> _pairs;
    [ObservableProperty] private UseAlwaysPairItem? _selectedPair;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private string _editFrom;
    [ObservableProperty] private string _editTo;
    [ObservableProperty] private bool _isEditingExisting;
    [ObservableProperty] private string _countText;
    [ObservableProperty] private string _unusedText;
    [ObservableProperty] private bool _hasUnused;
    [ObservableProperty] private bool _isEmpty;
    [ObservableProperty] private bool _isNoMatch;
    [ObservableProperty] private bool _isRememberOff;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private readonly IFolderHelper _folderHelper;
    private readonly Dictionary<string, List<UseAlwaysPairItem>> _pairsByLanguage = new();
    private readonly HashSet<string> _dirtyLanguages = new();
    private readonly Dictionary<string, Task<SpellChecker?>> _spellCheckers = new();
    private List<UseAlwaysPairItem> _currentPairs = new();

    public UseAlwaysListViewModel(IFolderHelper folderHelper)
    {
        _folderHelper = folderHelper;
        Languages = new ObservableCollection<UseAlwaysLanguageItem>();
        Pairs = new ObservableCollection<UseAlwaysPairItem>();
        SearchText = string.Empty;
        EditFrom = string.Empty;
        EditTo = string.Empty;
        CountText = string.Empty;
        UnusedText = string.Empty;
        IsRememberOff = !Se.Settings.Tools.SpellCheckRememberUseAlwaysList;
    }

    /// <summary>
    /// Loads the languages and selects the one matching <paramref name="dictionaryFileName"/>
    /// (the dictionary in use in the spell check window), else the last used / English.
    /// </summary>
    public void Initialize(string? dictionaryFileName)
    {
        LoadLanguages();

        var code = dictionaryFileName != null ? GetLanguageCode(dictionaryFileName) : null;
        SelectedLanguage =
            Languages.FirstOrDefault(l => code != null && l.Code == code) ??
            Languages.FirstOrDefault(l => l.Code == Se.Settings.Options.LastLanguage) ??
            Languages.FirstOrDefault(l => l.Code.StartsWith("en", StringComparison.Ordinal)) ??
            Languages.FirstOrDefault();
    }

    private static string GetLanguageCode(string dictionaryFileName)
    {
        // Same five-letter name SpellChecker.Initialize uses for <lang>_UseAlways.xml.
        var name = Path.GetFileNameWithoutExtension(dictionaryFileName);
        return SpellCheckDictionaryDisplay.GetFiveLetterLanguageName(name) ?? "en_US";
    }

    private void LoadLanguages()
    {
        Languages.Clear();
        var folder = Se.DictionariesFolder;
        var items = new List<UseAlwaysLanguageItem>();

        foreach (var dictionary in new SpellChecker().GetDictionaryLanguages(folder))
        {
            var code = GetLanguageCode(dictionary.DictionaryFileName);
            if (items.All(i => i.Code != code))
            {
                items.Add(new UseAlwaysLanguageItem(dictionary.Name, code, dictionary.DictionaryFileName));
            }
        }

        // Lists left behind without an installed dictionary (e.g. from Subtitle Edit 4) are
        // still applied once that dictionary comes back, so they must be visible here too.
        if (Directory.Exists(folder))
        {
            foreach (var file in Directory.GetFiles(folder, "*_UseAlways.xml"))
            {
                var name = Path.GetFileName(file);
                var code = name.Substring(0, name.Length - "_UseAlways.xml".Length);
                if (code.Length > 0 && items.All(i => i.Code != code))
                {
                    items.Add(new UseAlwaysLanguageItem("[" + code + "]", code, null));
                }
            }
        }

        foreach (var item in items.OrderBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            Languages.Add(item);
        }
    }

    partial void OnSelectedLanguageChanged(UseAlwaysLanguageItem? value)
    {
        _currentPairs = value == null ? new List<UseAlwaysPairItem>() : GetPairs(value);
        SearchText = string.Empty;
        ClearEditor();
        RefreshPairs();

        if (value != null)
        {
            _ = MarkUnusedPairsAsync(value, _currentPairs.ToList());
        }
    }

    partial void OnSearchTextChanged(string value) => RefreshPairs();

    partial void OnSelectedPairChanged(UseAlwaysPairItem? value)
    {
        if (value != null)
        {
            EditFrom = value.From;
            EditTo = value.To;
        }
    }

    partial void OnEditFromChanged(string value)
    {
        var from = value.Trim();
        IsEditingExisting = from.Length > 0 && _currentPairs.Any(p => p.From == from);
    }

    private List<UseAlwaysPairItem> GetPairs(UseAlwaysLanguageItem language)
    {
        if (_pairsByLanguage.TryGetValue(language.Code, out var cached))
        {
            return cached;
        }

        var list = new List<UseAlwaysPairItem>();
        try
        {
            var fileName = UseAlwaysListFile.GetFileName(Se.DictionariesFolder, language.Code);
            list.AddRange(UseAlwaysListFile.Load(fileName).Select(p => new UseAlwaysPairItem(p.Key, p.Value)));
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "Unable to load spell check \"use always\" list for " + language.Code);
        }

        list.Sort(ComparePairs);
        _pairsByLanguage[language.Code] = list;
        return list;
    }

    private static int ComparePairs(UseAlwaysPairItem a, UseAlwaysPairItem b)
    {
        var result = string.Compare(a.From, b.From, StringComparison.OrdinalIgnoreCase);
        return result != 0 ? result : string.CompareOrdinal(a.From, b.From);
    }

    private void RefreshPairs()
    {
        var search = SearchText.Trim();
        var selected = SelectedPair;

        Pairs.Clear();
        foreach (var pair in _currentPairs)
        {
            if (search.Length == 0 ||
                pair.From.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                pair.To.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            {
                Pairs.Add(pair);
            }
        }

        if (selected != null && Pairs.Contains(selected))
        {
            SelectedPair = selected;
        }

        UpdateCounts();
    }

    private void UpdateCounts()
    {
        IsEmpty = _currentPairs.Count == 0;
        IsNoMatch = !IsEmpty && Pairs.Count == 0;
        CountText = string.Format(Se.Language.SpellCheck.XPairs, _currentPairs.Count);

        var unused = _currentPairs.Count(p => p.IsUnused);
        HasUnused = unused > 0;
        UnusedText = string.Format(Se.Language.SpellCheck.XUnusedPairs, unused);
    }

    private Task<SpellChecker?> GetSpellChecker(UseAlwaysLanguageItem language)
    {
        if (string.IsNullOrEmpty(language.DictionaryFileName))
        {
            return Task.FromResult<SpellChecker?>(null);
        }

        if (!_spellCheckers.TryGetValue(language.Code, out var task))
        {
            var dictionaryFileName = language.DictionaryFileName;
            var twoLetter = language.Code.Length >= 2 ? language.Code.Substring(0, 2) : language.Code;
            task = Task.Run(() =>
            {
                try
                {
                    var checker = new SpellChecker();
                    return checker.Initialize(dictionaryFileName, twoLetter) ? checker : null;
                }
                catch (Exception exception)
                {
                    Se.LogError(exception, "Unable to load spell check dictionary " + dictionaryFileName);
                    return null;
                }
            });
            _spellCheckers[language.Code] = task;
        }

        return task;
    }

    private async Task MarkUnusedPairsAsync(UseAlwaysLanguageItem language, List<UseAlwaysPairItem> pairs)
    {
        var checker = await GetSpellChecker(language);
        if (checker == null)
        {
            return;
        }

        var results = await Task.Run(() => pairs.Select(p => checker.IsWordCorrect(p.From)).ToList());
        Dispatcher.UIThread.Post(() =>
        {
            for (var i = 0; i < pairs.Count; i++)
            {
                pairs[i].IsUnused = results[i];
            }

            if (SelectedLanguage == language)
            {
                UpdateCounts();
            }
        });
    }

    private void ClearEditor()
    {
        SelectedPair = null;
        EditFrom = string.Empty;
        EditTo = string.Empty;
    }

    private void MarkDirty()
    {
        if (SelectedLanguage != null)
        {
            _dirtyLanguages.Add(SelectedLanguage.Code);
        }
    }

    [RelayCommand]
    private async Task AddOrUpdate()
    {
        var language = SelectedLanguage;
        var from = EditFrom.Trim();
        var to = EditTo.Trim();
        if (language == null || from.Length == 0 || to.Length == 0)
        {
            return;
        }

        if (from == to)
        {
            if (Window != null)
            {
                await MessageBox.Show(Window, Se.Language.General.Error, Se.Language.SpellCheck.UseAlwaysSameWord, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return;
        }

        var pair = _currentPairs.FirstOrDefault(p => p.From == from);
        if (pair == null)
        {
            pair = new UseAlwaysPairItem(from, to);
            var index = 0;
            while (index < _currentPairs.Count && ComparePairs(_currentPairs[index], pair) < 0)
            {
                index++;
            }

            _currentPairs.Insert(index, pair);
        }
        else
        {
            pair.To = to;
        }

        MarkDirty();
        SearchText = string.Empty;
        RefreshPairs();
        SelectedPair = pair;
        IsEditingExisting = true;
        _ = MarkUnusedPairsAsync(language, new List<UseAlwaysPairItem> { pair });
    }

    [RelayCommand]
    private void Remove(UseAlwaysPairItem? item)
    {
        var pair = item ?? SelectedPair;
        if (pair == null)
        {
            return;
        }

        var index = Pairs.IndexOf(pair);
        _currentPairs.Remove(pair);
        MarkDirty();
        RefreshPairs();

        if (Pairs.Count == 0)
        {
            ClearEditor();
        }
        else
        {
            SelectedPair = Pairs[Math.Clamp(index, 0, Pairs.Count - 1)];
        }
    }

    [RelayCommand]
    private void RemoveUnused()
    {
        if (_currentPairs.RemoveAll(p => p.IsUnused) > 0)
        {
            MarkDirty();
            ClearEditor();
            RefreshPairs();
        }
    }

    [RelayCommand]
    private void New()
    {
        ClearEditor();
    }

    [RelayCommand]
    private void OpenDictionariesFolder()
    {
        if (Window == null)
        {
            return;
        }

        var folder = Se.DictionariesFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _ = _folderHelper.OpenFolder(Window, folder);
    }

    [RelayCommand]
    private async Task Ok()
    {
        foreach (var code in _dirtyLanguages)
        {
            try
            {
                var fileName = UseAlwaysListFile.GetFileName(Se.DictionariesFolder, code);
                UseAlwaysListFile.Save(fileName, _pairsByLanguage[code].Select(p => new KeyValuePair<string, string>(p.From, p.To)));
            }
            catch (Exception exception)
            {
                Se.LogError(exception, "Unable to save spell check \"use always\" list for " + code);
                if (Window != null)
                {
                    await MessageBox.Show(Window, Se.Language.General.Error, exception.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return;
            }
        }

        if (SelectedLanguage != null)
        {
            Se.Settings.Options.LastLanguage = SelectedLanguage.Code;
        }

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
        Dispatcher.UIThread.Post(() => Window?.Close());
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/spell-check");
        }
    }

    internal void EditTextBoxKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            _ = AddOrUpdate();
        }
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            e.Handled = true;
            Remove(null);
        }
    }
}
