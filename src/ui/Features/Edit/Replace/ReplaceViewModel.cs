using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Features.Edit.Replace;

public partial class ReplaceViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _searchHistory;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private bool _wholeWord;
    [ObservableProperty] private string _replaceText;
    [ObservableProperty] private string _countResult;
    [ObservableProperty] private ObservableCollection<ReplaceScopeDisplay> _scopes;
    [ObservableProperty] private ReplaceScopeDisplay _selectedScope;

    /// <summary>
    /// The scope picker only makes sense with an editable original text column on screen, and is
    /// hidden otherwise (SE 4 showed it disabled) - see <see cref="EffectiveScope"/>.
    /// </summary>
    [ObservableProperty] private bool _isScopeVisible;

    [ObservableProperty]
    public partial FindMode FindMode { get; set; }

    public Window? Window { get; set; }
    public Action? FocusSearchBox { get; set; }

    /// <summary>
    /// The scope to apply: the picked one while the picker is shown, otherwise both columns - which
    /// with no original loaded means the text column, the only one there is.
    /// </summary>
    public FindScope EffectiveScope => IsScopeVisible ? SelectedScope.Scope : FindScope.TextAndOriginal;

    public bool FocusReplaceOnOpen { get; set; }
    public bool FindNextPressed { get; private set; }
    public bool ReplacePressed { get; private set; }
    public bool ReplaceAllPressed { get; private set; }
    public bool ResultFound { get; set; }

    private IFindService? _findService;
    private List<string> _subs = new List<string>();
    private List<string>? _originalSubs;
    private IFindResult? _findResult;

    public ReplaceViewModel()
    {
        SearchHistory = new ObservableCollection<string>(new List<string>());
        SearchText = string.Empty;
        ReplaceText = string.Empty;
        CountResult = string.Empty;
        Scopes = new ObservableCollection<ReplaceScopeDisplay>(ReplaceScopeDisplay.List());
        SelectedScope = Scopes[0];

        LoadSettings();
    }

    private void LoadSettings()
    {
        WholeWord = Se.Settings.Edit.Find.FindWholeWords;

        FindMode = Se.Settings.Edit.Find.FindSearchType switch
        {
            nameof(FindMode.CaseInsensitive) => FindMode.CaseInsensitive,
            nameof(FindMode.CaseSensitive) => FindMode.CaseSensitive,
            _ => FindMode.RegularExpression
        };

        var scope = Se.Settings.Edit.Find.ReplaceIn switch
        {
            nameof(FindScope.TextOnly) => FindScope.TextOnly,
            nameof(FindScope.OriginalOnly) => FindScope.OriginalOnly,
            _ => FindScope.TextAndOriginal
        };
        SelectedScope = Scopes.First(p => p.Scope == scope);
    }

    [RelayCommand]
    private async Task Replace()
    {
        ReplacePressed = true;
        ReplaceAllPressed = false;
        FindNextPressed = false;
        SaveSettings();
        if (_findResult != null)
        {
            await _findResult.HandleReplaceResult(this);
        }
    }

    [RelayCommand]
    private async Task ReplaceAll()
    {
        ReplacePressed = false;
        ReplaceAllPressed = true;
        FindNextPressed = false;
        SaveSettings();
        if (_findResult != null)
        {
            await _findResult.HandleReplaceResult(this);
        }
    }

    [RelayCommand]
    private async Task FindNext()
    {
        ReplacePressed = false;
        ReplaceAllPressed = false;
        FindNextPressed = true;
        SaveSettings();
        if (_findResult != null)
        {
            await _findResult.HandleReplaceResult(this);
        }
    }

    [RelayCommand]
    private void Count()
    {
        _findResult?.RequestFindData();

        CountResult = string.Empty;
        if (_findService == null || string.IsNullOrEmpty(SearchText))
        {
            return;
        }

        var count = _findService.Count(SearchText, _subs, WholeWord, FindMode, _originalSubs, EffectiveScope);

        if (count <= 0)
        {
            CountResult = Se.Language.General.FoundNoMatches;
        }
        else if (count == 1)
        {
            CountResult = Se.Language.General.FoundOneMatch;
        }
        else
        {
            CountResult = string.Format(Se.Language.General.FoundXMatches, count);
        }
    }

    internal void SaveSettings()
    {
        Se.Settings.Edit.Find.FindWholeWords = WholeWord;
        Se.Settings.Edit.Find.FindSearchType = FindMode.ToString();

        // Only remember a scope the user could actually see and pick (SE 4 did the same by
        // leaving the setting alone while its combo box was disabled).
        if (IsScopeVisible)
        {
            Se.Settings.Edit.Find.ReplaceIn = SelectedScope.Scope.ToString();
        }
    }
    
    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Delete && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (!string.IsNullOrWhiteSpace(SearchText) &&
                SearchHistory.Contains(SearchText))
            {
                SearchHistory.Remove(SearchText);
                SearchText = string.Empty;
                e.Handled = true;
            }
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/edit", "replace");
        }
    }

    internal void RefreshSubtitles(List<string> subs, List<string>? originalSubs = null, bool canEditOriginal = false)
    {
        _subs = subs;
        _originalSubs = originalSubs;
        IsScopeVisible = canEditOriginal;
    }

    internal void InitializeFindData(IFindService findService, List<string> subs, string selectedText, MainViewModel mainViewModel, List<string>? originalSubs = null, bool canEditOriginal = false)
    {
        _findService = findService;
        _subs = subs;
        _originalSubs = originalSubs;
        _findResult = mainViewModel;
        IsScopeVisible = canEditOriginal;
        if (!string.IsNullOrEmpty(selectedText))
        {
            SearchText = RegexUtils.EscapeNewLines(selectedText);
        }

        SearchHistory.Clear();
        foreach (var item in findService.SearchHistory)
        {
            SearchHistory.Add(RegexUtils.EscapeNewLines(item));
        }
    }

    internal async void FindTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await FindNextCommand.ExecuteAsync(null);
        }
    }

    internal async void ReplaceTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            await ReplaceCommand.ExecuteAsync(null);
        }
    }
}