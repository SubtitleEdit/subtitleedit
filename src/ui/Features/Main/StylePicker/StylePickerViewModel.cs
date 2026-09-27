using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Main.StylePicker;

public partial class StylePickerViewModel : ObservableObject
{
    public const int NumberKeyCount = 10;

    [ObservableProperty] private string _filterText;
    [ObservableProperty] private StylePickerItem? _selectedItem;
    [ObservableProperty] private string _selectionInfo;
    [ObservableProperty] private string _currentStyleInfo;

    public ObservableCollection<StylePickerItem> VisibleItems { get; }

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    /// <summary>The style to set. Null when cancelled.</summary>
    public string? ResultStyle { get; private set; }

    /// <summary>True when <see cref="ResultStyle"/> does not exist yet and must be added to the header.</summary>
    public bool ResultIsNewStyle { get; private set; }

    /// <summary>True when the picker was closed to open the styles manager.</summary>
    public bool OpenStylesManager { get; private set; }

    private readonly List<StylePickerItem> _allItems = new();
    private bool _isSsa;
    private SsaStyle _newStyleTemplate = new();
    private IReadOnlyList<string> _shortcutTexts = Array.Empty<string>();

    public StylePickerViewModel()
    {
        _filterText = string.Empty;
        _selectionInfo = string.Empty;
        _currentStyleInfo = string.Empty;
        VisibleItems = new ObservableCollection<StylePickerItem>();
    }

    /// <param name="styles">Styles in header order - the order of the context menu and the number keys.</param>
    /// <param name="lineCounts">Number of lines per style, over the whole file.</param>
    /// <param name="selectedStyles">Styles of the selected lines, in grid order.</param>
    /// <param name="selectedLineCount">Number of lines the style will be applied to.</param>
    /// <param name="isSsa">SSA (legacy alignment values) rather than ASS.</param>
    /// <param name="shortcutTexts">Display text of the "Set style 1-10" shortcuts, by position.</param>
    /// <param name="newStyleTemplate">Settings a style typed as a new name is created with, shown in its details.</param>
    public void Initialize(IReadOnlyList<SsaStyle> styles, IReadOnlyDictionary<string, int> lineCounts,
        IReadOnlyList<string> selectedStyles, int selectedLineCount, bool isSsa,
        IReadOnlyList<string>? shortcutTexts = null, SsaStyle? newStyleTemplate = null)
    {
        _isSsa = isSsa;
        _shortcutTexts = shortcutTexts ?? Array.Empty<string>();
        _newStyleTemplate = newStyleTemplate ?? new SsaStyle();
        var distinctSelected = selectedStyles
            .Select(p => string.IsNullOrEmpty(p) ? "Default" : p)
            .Distinct()
            .ToList();

        _allItems.Clear();
        foreach (var style in styles)
        {
            lineCounts.TryGetValue(style.Name, out var lineCount);
            _allItems.Add(new StylePickerItem(style, lineCount, distinctSelected.Contains(style.Name), false, isSsa));
        }

        SelectionInfo = string.Format(Se.Language.General.ActorPickerLinesSelectedX, selectedLineCount);
        CurrentStyleInfo = distinctSelected.Count switch
        {
            0 => string.Empty,
            <= 3 => string.Format(Se.Language.General.StylePickerCurrentStyleX, string.Join(", ", distinctSelected)),
            _ => string.Format(Se.Language.General.StylePickerCurrentStyleX,
                string.Join(", ", distinctSelected.Take(3)) + ", ... (" + distinctSelected.Count + ")"),
        };

        UpdateNumberTexts();
        UpdateVisibleItems();

        // Start on the style of the selection, so Enter alone keeps it and arrows move from there.
        SelectedItem = distinctSelected.Count == 1
            ? VisibleItems.FirstOrDefault(p => p.Name == distinctSelected[0]) ?? VisibleItems.FirstOrDefault()
            : VisibleItems.FirstOrDefault();
    }

    partial void OnFilterTextChanged(string value)
    {
        UpdateVisibleItems();
        SelectedItem = VisibleItems.FirstOrDefault();
    }

    private void UpdateNumberTexts()
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

        // A name that is not a style yet can be added as a new one. Style names are unique
        // ignoring case in ASS, so "default" does not offer a second "Default". The row comes
        // last, so Enter on "vo" still picks "voice".
        if (filter.Length > 0 && !_allItems.Any(p => p.Name.Equals(filter, StringComparison.OrdinalIgnoreCase)))
        {
            VisibleItems.Add(new StylePickerItem(new SsaStyle(_newStyleTemplate) { Name = filter }, 0, false, true, _isSsa));
        }
    }

    internal void Pick(StylePickerItem? item)
    {
        if (item == null)
        {
            return;
        }

        ResultStyle = item.Name;
        ResultIsNewStyle = item.IsNew;
        OkPressed = true;
        Window?.Close();
    }

    internal void ShowStylesManager()
    {
        OpenStylesManager = true;
        Window?.Close();
    }

    internal void Cancel()
    {
        Window?.Close();
    }

    private bool IsFilterEmpty => string.IsNullOrEmpty(FilterText);

    /// <summary>
    /// Runs before the filter box sees the key (the window handler tunnels), so number keys
    /// pick a style instead of being typed - as long as nothing has been typed yet.
    /// </summary>
    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
            return;
        }

        if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/subtitle-grid", "setting-styles");
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

        if (e.Key == Key.PageUp || e.Key == Key.PageDown)
        {
            e.Handled = true;
            MoveSelection(e.Key == Key.PageUp ? -10 : 10);
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
            // Swallow unused number keys too, so "7" with five styles does not start a filter.
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
}
