using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Edit.Find;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static Nikse.SubtitleEdit.Logic.FindService;

namespace Nikse.SubtitleEdit.Features.Edit.Replace;

public partial class ReplaceViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _searchHistory;
    [ObservableProperty] private string _searchText;
    [ObservableProperty] private bool _wholeWord;
    [ObservableProperty] private string _replaceText;
    [ObservableProperty] private bool _findTypeNormal;
    [ObservableProperty] private bool _findTypeCanseInsensitive;
    [ObservableProperty] private bool _findTypeRegularExpression;
    [ObservableProperty] private string _countResult;

    public Window? Window { get; set; }

    public bool FindNextPressed { get; private set; }
    public bool ReplacePressed { get; private set; }
    public bool ReplaceAllPressed { get; private set; }

    private IFindService? _findService;
    private List<string> _subs = new List<string>();
    private IFindResult? _findResult;

    public ReplaceViewModel()
    {
        SearchHistory = new ObservableCollection<string>(new List<string>());
        SearchText = string.Empty;
        ReplaceText = string.Empty;
        CountResult = string.Empty;

        LoadSettings();
    }

    private void LoadSettings()
    {
        WholeWord = Se.Settings.Edit.Find.FindWholeWords;

        if (Se.Settings.Edit.Find.FindSearchType == FindMode.CaseInsensitive.ToString())
        {
            FindTypeCanseInsensitive = true;
        }
        else if (Se.Settings.Edit.Find.FindSearchType == FindMode.CaseSensitive.ToString())
        {
            FindTypeNormal = true;
        }
        else
        {
            FindTypeRegularExpression = true;
        }
    }

    [RelayCommand]
    private void Replace()
    {
        ReplacePressed = true;
        ReplaceAllPressed = false;
        FindNextPressed = false;
        _findResult?.HandleReplaceResult(this);
    }

    [RelayCommand]
    private void ReplaceAll()
    {
        ReplacePressed = false;
        ReplaceAllPressed = true;
        FindNextPressed = false;
        _findResult?.HandleReplaceResult(this);
    }

    [RelayCommand]
    private void FindNext()
    {
        ReplacePressed = false;
        ReplaceAllPressed = false;
        FindNextPressed = true;
        _findResult?.HandleReplaceResult(this);
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

        _findService.Initialize(
            _subs,
            0,
            WholeWord,
            FindTypeNormal ? FindMode.CaseSensitive : FindTypeCanseInsensitive ? FindMode.CaseInsensitive : FindMode.RegularExpression);

        var count = _findService.Count(SearchText);

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

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void InitializeFindData(IFindService findService, List<string> subs, string selectedText, MainViewModel mainViewModel)
    {
        _findService = findService;
        _subs = subs;
        _findResult = mainViewModel;
        if (!string.IsNullOrEmpty(selectedText))
        {
            SearchText = selectedText;
        }

        SearchHistory.Clear();
        foreach (var item in findService.SearchHistory)
        {
            SearchHistory.Add(item);
        }
    }

    internal void FindTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            FindNext();
        }
    }
}