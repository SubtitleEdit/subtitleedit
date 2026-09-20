using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.ActorPicker;

public partial class ActorPickerViewModel : ObservableObject
{
    public const int NumberKeyCount = 10;

    [ObservableProperty] private string _filterText;
    [ObservableProperty] private ActorPickerItem? _selectedItem;
    [ObservableProperty] private string _selectionInfo;

    public ObservableCollection<ActorPickerItem> VisibleItems { get; }

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>The actor to set - an empty string removes the actor. Null when cancelled.</summary>
    public string? ResultActor { get; private set; }

    private readonly List<ActorPickerItem> _allItems = new();
    private IReadOnlyList<string> _shortcutTexts = Array.Empty<string>();

    public ActorPickerViewModel()
    {
        _filterText = string.Empty;
        _selectionInfo = string.Empty;
        VisibleItems = new ObservableCollection<ActorPickerItem>();
    }

    /// <param name="actors">Actors in shortcut order.</param>
    /// <param name="lineCounts">Number of lines per actor.</param>
    /// <param name="shortcutTexts">Display text of the "Set actor 1-10" shortcuts, by position.</param>
    /// <param name="currentActor">Actor of the selected line(s), if any.</param>
    /// <param name="selectedLineCount">Number of lines the actor will be applied to.</param>
    public void Initialize(IReadOnlyList<string> actors, IReadOnlyDictionary<string, int> lineCounts,
        IReadOnlyList<string> shortcutTexts, string currentActor, int selectedLineCount)
    {
        _shortcutTexts = shortcutTexts;
        _allItems.Clear();
        foreach (var actor in actors)
        {
            lineCounts.TryGetValue(actor, out var lineCount);
            _allItems.Add(new ActorPickerItem(actor, actor, lineCount, actor == currentActor, false));
        }

        SelectionInfo = string.Format(Se.Language.General.ActorPickerLinesSelectedX, selectedLineCount);
        UpdatePositionTexts();
        UpdateVisibleItems();
        SelectedItem = VisibleItems.FirstOrDefault(p => p.IsCurrent) ?? VisibleItems.FirstOrDefault();
    }

    public List<string> GetActorsInOrder()
    {
        return _allItems.Select(p => p.Name).ToList();
    }

    partial void OnFilterTextChanged(string value)
    {
        UpdateVisibleItems();
        SelectedItem = VisibleItems.FirstOrDefault();
    }

    private void UpdatePositionTexts()
    {
        for (var i = 0; i < _allItems.Count; i++)
        {
            // Keys run 1..9 then 0, like the number row.
            _allItems[i].NumberText = i < NumberKeyCount ? ((i + 1) % 10).ToString() : string.Empty;
            _allItems[i].ShortcutText = i < _shortcutTexts.Count ? _shortcutTexts[i] : string.Empty;
        }
    }

    private void UpdateVisibleItems()
    {
        var filter = (FilterText ?? string.Empty).Trim();
        VisibleItems.Clear();
        foreach (var item in _allItems)
        {
            item.IsNumberKeyActive = filter.Length == 0;
            if (filter.Length == 0 || item.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                VisibleItems.Add(item);
            }
        }

        // Anything typed that is not an existing actor can be added as a new one. The row comes
        // last, so Enter on "An" still picks "Anna" - unless nothing matches at all.
        if (filter.Length > 0 && !_allItems.Any(p => p.Name == filter))
        {
            var displayName = string.Format(Se.Language.General.ActorPickerNewActorX, filter);
            VisibleItems.Add(new ActorPickerItem(filter, displayName, 0, false, true));
        }
    }

    internal void Pick(ActorPickerItem? item)
    {
        if (item == null)
        {
            return;
        }

        ResultActor = item.Name;
        OkPressed = true;
        Window?.Close();
    }

    internal void PickRemoveActor()
    {
        ResultActor = string.Empty;
        OkPressed = true;
        Window?.Close();
    }

    internal void Cancel()
    {
        Window?.Close();
    }

    private bool IsFilterEmpty => string.IsNullOrEmpty(FilterText);

    /// <summary>
    /// Runs before the filter box sees the key (the window handler tunnels), so number keys
    /// pick an actor instead of being typed - as long as nothing has been typed yet.
    /// </summary>
    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
            return;
        }

        if (e.KeyModifiers == KeyModifiers.Alt && (e.Key == Key.Up || e.Key == Key.Down))
        {
            e.Handled = true;
            MoveSelected(e.Key == Key.Up ? -1 : 1);
            return;
        }

        if (e.KeyModifiers != KeyModifiers.None)
        {
            return;
        }

        if (e.Key == Key.Up || e.Key == Key.Down)
        {
            e.Handled = true;
            MoveSelection(e.Key == Key.Up ? -1 : 1);
            return;
        }

        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Pick(SelectedItem);
            return;
        }

        if (!IsFilterEmpty)
        {
            return;
        }

        if (e.Key == Key.Delete)
        {
            e.Handled = true;
            PickRemoveActor();
            return;
        }

        var index = e.Key switch
        {
            Key.D1 or Key.NumPad1 => 0,
            Key.D2 or Key.NumPad2 => 1,
            Key.D3 or Key.NumPad3 => 2,
            Key.D4 or Key.NumPad4 => 3,
            Key.D5 or Key.NumPad5 => 4,
            Key.D6 or Key.NumPad6 => 5,
            Key.D7 or Key.NumPad7 => 6,
            Key.D8 or Key.NumPad8 => 7,
            Key.D9 or Key.NumPad9 => 8,
            Key.D0 or Key.NumPad0 => 9,
            _ => -1,
        };

        if (index >= 0)
        {
            // Swallow unused number keys too, so "7" with five actors does not start a filter.
            e.Handled = true;
            if (index < _allItems.Count)
            {
                Pick(_allItems[index]);
            }
        }
    }

    private void MoveSelection(int direction)
    {
        if (VisibleItems.Count == 0)
        {
            return;
        }

        var index = SelectedItem != null ? VisibleItems.IndexOf(SelectedItem) : -1;
        index = Math.Clamp(index + direction, 0, VisibleItems.Count - 1);
        SelectedItem = VisibleItems[index];
    }

    private void MoveSelected(int direction)
    {
        // Reordering a filtered list would move the actor past rows that are not shown.
        var item = SelectedItem;
        if (item == null || item.IsNew || !IsFilterEmpty)
        {
            return;
        }

        var index = _allItems.IndexOf(item);
        var newIndex = index + direction;
        if (index < 0 || newIndex < 0 || newIndex >= _allItems.Count)
        {
            return;
        }

        _allItems.RemoveAt(index);
        _allItems.Insert(newIndex, item);
        VisibleItems.Move(index, newIndex);
        UpdatePositionTexts();
        SelectedItem = item;
    }
}
