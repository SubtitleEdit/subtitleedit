using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Settings.WaveformToolbarItems;

public partial class WaveformToolbarItemsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ToolbarItemDisplay> _toolbarItems;
    [ObservableProperty] private ToolbarItemDisplay? _selectedToolbarItem;
    [ObservableProperty] private int _selectedFontSize;
    [ObservableProperty] private int _selectedLeftMargin;
    [ObservableProperty] private int _selectedRightMargin;

    public List<SeWaveformToolbarItem> ResultToolbarItems { get; set; } = new List<SeWaveformToolbarItem>();

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    /// <summary>
    /// False when OK was pressed but the items are identical to what <see cref="Initialize"/>
    /// received - callers use this to skip saving and, more importantly, the layout rebuild
    /// (which re-docks the video controls while they are undocked, #12520).
    /// </summary>
    public bool HasChanges { get; private set; }

    private bool _updatingFromSelection;
    private List<SeWaveformToolbarItem> _initialToolbarItems = new List<SeWaveformToolbarItem>();

    public WaveformToolbarItemsViewModel()
    {
        ToolbarItems = new ObservableCollection<ToolbarItemDisplay>();
    }

    partial void OnSelectedToolbarItemChanged(ToolbarItemDisplay? value)
    {
        _updatingFromSelection = true;
        try
        {
            SelectedFontSize = value?.FontSize ?? 12;
            SelectedLeftMargin = value?.LeftMargin ?? 5;
            SelectedRightMargin = value?.RightMargin ?? 5;
        }
        finally
        {
            _updatingFromSelection = false;
        }
    }

    partial void OnSelectedFontSizeChanged(int value)
    {
        if (_updatingFromSelection || SelectedToolbarItem == null)
        {
            return;
        }
        SelectedToolbarItem.FontSize = value;
    }

    partial void OnSelectedLeftMarginChanged(int value)
    {
        if (_updatingFromSelection || SelectedToolbarItem == null)
        {
            return;
        }
        SelectedToolbarItem.LeftMargin = value;
    }

    partial void OnSelectedRightMarginChanged(int value)
    {
        if (_updatingFromSelection || SelectedToolbarItem == null)
        {
            return;
        }
        SelectedToolbarItem.RightMargin = value;
    }

    internal void Initialize(List<SeWaveformToolbarItem> toolbarItems)
    {
        _initialToolbarItems = toolbarItems.OrderBy(x => x.SortOrder).ToList();

        ToolbarItems.Clear();
        foreach (var item in _initialToolbarItems)
        {
            ToolbarItems.Add(new ToolbarItemDisplay(item.Type, item.IsVisible, item.FontSize, item.LeftMargin, item.RightMargin));
        }

        SelectedToolbarItem = ToolbarItems.FirstOrDefault();
    }

    [RelayCommand]
    private void Ok()
    {
        ResultToolbarItems = new List<SeWaveformToolbarItem>();
        for (var i = 0; i < ToolbarItems.Count; i++)
        {
            var display = ToolbarItems[i];
            var item = new SeWaveformToolbarItem(display);
            item.SortOrder = (i + 1) * 10;
            ResultToolbarItems.Add(item);
        }

        HasChanges = !AreToolbarItemsEqual(_initialToolbarItems, ResultToolbarItems);
        OkPressed = true;
        Window?.Close();
    }

    // Both lists are in display order (Initialize sorts by SortOrder; Ok builds in list order),
    // so the SortOrder values themselves don't need comparing - only the sequence and content.
    private static bool AreToolbarItemsEqual(List<SeWaveformToolbarItem> before, List<SeWaveformToolbarItem> after)
    {
        if (before.Count != after.Count)
        {
            return false;
        }

        for (var i = 0; i < before.Count; i++)
        {
            var a = before[i];
            var b = after[i];
            if (a.Type != b.Type ||
                a.IsVisible != b.IsVisible ||
                a.FontSize != b.FontSize ||
                a.LeftMargin != b.LeftMargin ||
                a.RightMargin != b.RightMargin)
            {
                return false;
            }
        }

        return true;
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void MoveUp()
    {
        if (SelectedToolbarItem == null)
        {
            return;
        }

        var index = ToolbarItems.IndexOf(SelectedToolbarItem);
        if (index <= 0)
        {
            return;
        }

        var selected = SelectedToolbarItem;
        ToolbarItems.Move(index, index - 1);
        SelectedToolbarItem = selected;
    }

    [RelayCommand]
    private void MoveDown()
    {
        if (SelectedToolbarItem == null)
        {
            return;
        }

        var index = ToolbarItems.IndexOf(SelectedToolbarItem);
        if (index >= ToolbarItems.Count - 1)
        {
            return;
        }

        var selected = SelectedToolbarItem;
        ToolbarItems.Move(index, index + 1);
        SelectedToolbarItem = selected;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}