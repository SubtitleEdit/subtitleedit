using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.ErrorList;

public partial class ErrorListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ErrorListItem> _subtitles;
    [ObservableProperty] private ErrorListItem? _selectedSubtitle;
    [ObservableProperty] private bool _hasErrors;
    [ObservableProperty] private string _summary = string.Empty;

    public ObservableCollection<SummaryCard> Cards { get; } = new();

    public Window? Window { get; set; }

    public bool GoToPressed { get; private set; }

    private readonly List<ErrorListItem> _allItems = new();

    public ErrorListViewModel()
    {
        Subtitles = new ObservableCollection<ErrorListItem>();
    }

    [RelayCommand]
    private void GoTo()
    {
        if (SelectedSubtitle == null)
        {
            return;
        }

        GoToPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void SetFilter(SummaryCard? card)
    {
        if (card == null)
        {
            return;
        }

        ApplyFilter(card);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    /// <summary>
    /// The items come ready-made from the caller: it is the only place that still knows each
    /// line's real neighbours, which the gap/overlap wording needs.
    /// </summary>
    internal void Initialize(List<ErrorListItem> errorListItems, int totalLines)
    {
        var l = Se.Language.ErrorList;
        _allItems.Clear();
        _allItems.AddRange(errorListItems);

        var lineCount = errorListItems.Select(p => p.Number).Distinct().Count();
        Summary = errorListItems.Count == 0
            ? l.NoErrors
            : string.Format(l.SummaryX, errorListItems.Count, lineCount, totalLines);

        Cards.Clear();
        Cards.Add(new SummaryCard { Key = null, Label = l.All, Count = errorListItems.Count, Brush = LineError.AllBrush, IsActive = true });
        foreach (var type in Enum.GetValues<LineErrorType>())
        {
            var count = errorListItems.Count(p => p.Type == type);
            Cards.Add(new SummaryCard { Key = type, Label = LineError.GetLabel(type), Hint = LineError.GetHint(type), Count = count, Brush = LineError.GetBrush(type) });
        }

        ApplyFilter(Cards[0]);
    }

    private void ApplyFilter(SummaryCard card)
    {
        foreach (var c in Cards)
        {
            c.IsActive = ReferenceEquals(c, card);
        }

        var filtered = card.Key is LineErrorType type
            ? _allItems.Where(p => p.Type == type)
            : _allItems;

        Subtitles = new ObservableCollection<ErrorListItem>(filtered);
        SelectedSubtitle = Subtitles.FirstOrDefault();
    }

    /// <summary>
    /// Follows the selection itself instead of the grid's SelectionChanged event: the row the
    /// grid selects on its own (AlwaysSelected) never raises that event, which left Go to
    /// disabled while row 0 looked selected.
    /// </summary>
    partial void OnSelectedSubtitleChanged(ErrorListItem? value)
    {
        HasErrors = value != null;
    }

    internal void OnGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(GoTo);
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            GoTo();
        }
    }
}
