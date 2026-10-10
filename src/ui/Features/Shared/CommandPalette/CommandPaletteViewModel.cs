using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.CommandPalette;

public partial class CommandPaletteViewModel : ObservableObject
{
    private const int MaxRecent = 10;

    // Most recently run first; session only, so it never touches Settings.json.
    private static readonly List<string> RecentActionNames = new();

    [ObservableProperty] private string _searchText;
    [ObservableProperty] private CommandPaletteItem? _selectedItem;
    [ObservableProperty] private bool _isNothingFound;

    public ObservableCollection<CommandPaletteItem> Items { get; }

    public Window? Window { get; set; }
    public ListBox? ListBox { get; set; }

    public IRelayCommand? SelectedCommand { get; private set; }

    private List<CommandPaletteItem> _allItems;

    public CommandPaletteViewModel()
    {
        _searchText = string.Empty;
        _allItems = new List<CommandPaletteItem>();
        Items = new ObservableCollection<CommandPaletteItem>();
    }

    public void Initialize(List<CommandPaletteItem> items)
    {
        _allItems = items;
        Filter();
    }

    partial void OnSearchTextChanged(string value)
    {
        Filter();
    }

    private void Filter()
    {
        var query = (SearchText ?? string.Empty).Trim().ToLowerInvariant();
        var tokens = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        IEnumerable<CommandPaletteItem> result;
        if (tokens.Length == 0)
        {
            result = _allItems
                .OrderBy(GetRecentRank)
                .ThenBy(p => p.DisplayName, StringComparer.CurrentCultureIgnoreCase);
        }
        else
        {
            result = _allItems
                .Select(p => (Item: p, Score: GetScore(p, query, tokens)))
                .Where(p => p.Score >= 0)
                .OrderBy(p => p.Score)
                .ThenBy(p => GetRecentRank(p.Item))
                .ThenBy(p => p.Item.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .Select(p => p.Item);
        }

        Items.Clear();
        foreach (var item in result)
        {
            Items.Add(item);
        }

        SelectedItem = Items.FirstOrDefault();
        IsNothingFound = Items.Count == 0;
        if (SelectedItem != null)
        {
            ListBox?.ScrollIntoView(SelectedItem);
        }
    }

    /// <summary>
    /// Lower is better, -1 means no match. Every token must occur somewhere (name, group or
    /// shortcut keys); the ranking then prefers name prefix, word-start or initials matches
    /// ("fce" finds "Fix common errors").
    /// </summary>
    private static int GetScore(CommandPaletteItem item, string query, string[] tokens)
    {
        var initialsMatch = tokens.Length == 1 && query.Length > 1 && item.Initials.StartsWith(query, StringComparison.Ordinal);
        if (!initialsMatch)
        {
            foreach (var token in tokens)
            {
                if (!item.SearchText.Contains(token, StringComparison.Ordinal))
                {
                    return -1;
                }
            }
        }

        if (item.SearchName.StartsWith(query, StringComparison.Ordinal))
        {
            return 0;
        }

        if (initialsMatch)
        {
            return 1;
        }

        if (tokens.All(t => IsWordStart(item.SearchName, t)))
        {
            return 2;
        }

        if (item.SearchName.Contains(query, StringComparison.Ordinal))
        {
            return 3;
        }

        return tokens.All(t => item.SearchName.Contains(t, StringComparison.Ordinal)) ? 4 : 5;
    }

    private static bool IsWordStart(string text, string token)
    {
        var idx = text.IndexOf(token, StringComparison.Ordinal);
        while (idx >= 0)
        {
            if (idx == 0 || !char.IsLetterOrDigit(text[idx - 1]))
            {
                return true;
            }

            idx = text.IndexOf(token, idx + 1, StringComparison.Ordinal);
        }

        return false;
    }

    private static int GetRecentRank(CommandPaletteItem item)
    {
        var idx = RecentActionNames.IndexOf(item.ActionName);
        return idx < 0 ? int.MaxValue : idx;
    }

    [RelayCommand]
    private void Ok()
    {
        var item = SelectedItem;
        if (item == null)
        {
            return;
        }

        RecentActionNames.Remove(item.ActionName);
        RecentActionNames.Insert(0, item.ActionName);
        if (RecentActionNames.Count > MaxRecent)
        {
            RecentActionNames.RemoveAt(RecentActionNames.Count - 1);
        }

        SelectedCommand = item.Command;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    public void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                e.Handled = true;
                Cancel();
                break;
            case Key.Enter:
                e.Handled = true;
                Ok();
                break;
            case Key.Down:
                e.Handled = true;
                MoveSelection(1);
                break;
            case Key.Up:
                e.Handled = true;
                MoveSelection(-1);
                break;
            case Key.PageDown:
                e.Handled = true;
                MoveSelection(10);
                break;
            case Key.PageUp:
                e.Handled = true;
                MoveSelection(-10);
                break;
        }
    }

    private void MoveSelection(int delta)
    {
        if (Items.Count == 0)
        {
            return;
        }

        var idx = SelectedItem == null ? -1 : Items.IndexOf(SelectedItem);
        idx = Math.Clamp(idx + delta, 0, Items.Count - 1);
        SelectedItem = Items[idx];
        ListBox?.ScrollIntoView(SelectedItem);
    }
}
