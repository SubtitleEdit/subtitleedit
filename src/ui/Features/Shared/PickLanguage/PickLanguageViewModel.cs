using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.PickLanguage;

public partial class PickLanguageViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<PickLanguageDisplay> _languages;
    [ObservableProperty] private PickLanguageDisplay? _selectedLanguage;
    [ObservableProperty] private string _searchText;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private readonly List<PickLanguageDisplay> _all;

    public PickLanguageViewModel()
    {
        _searchText = string.Empty;
        _all = Iso639Dash2LanguageCode.List
            .Where(l => !string.IsNullOrEmpty(l.TwoLetterCode))
            .GroupBy(l => l.TwoLetterCode.ToLowerInvariant())
            .Select(g => g.First())
            .Select(l => new PickLanguageDisplay { Code = l.TwoLetterCode.ToLowerInvariant(), Name = l.EnglishName })
            .OrderBy(l => l.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _languages = new ObservableCollection<PickLanguageDisplay>(_all);
    }

    /// <summary>
    /// Hide languages that are already in the favorites list.
    /// </summary>
    public void Initialize(IEnumerable<string> excludeCodes)
    {
        var exclude = new HashSet<string>(
            (excludeCodes ?? Enumerable.Empty<string>()).Select(LanguageFavorites.NormalizeTwoLetter),
            StringComparer.OrdinalIgnoreCase);

        _all.RemoveAll(l => exclude.Contains(l.Code));
        SearchTextChanged();
    }

    public void SearchTextChanged()
    {
        var filter = SearchText?.Trim() ?? string.Empty;
        var items = string.IsNullOrEmpty(filter)
            ? _all
            : _all.Where(l =>
                l.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                l.Code.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();

        Languages.Clear();
        foreach (var item in items)
        {
            Languages.Add(item);
        }

        if (SelectedLanguage == null && Languages.Count > 0)
        {
            SelectedLanguage = Languages[0];
        }
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

        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        OkPressed = false;
        Close();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            CancelCommand.Execute(null);
        }
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
            OkCommand.Execute(null);
        }
    }
}
