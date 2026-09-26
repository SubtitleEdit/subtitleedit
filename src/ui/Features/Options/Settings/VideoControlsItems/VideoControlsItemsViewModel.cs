using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Options.Settings.VideoControlsItems;

public partial class VideoControlsItemsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<VideoControlsItemDisplay> _items;
    [ObservableProperty] private VideoControlsItemDisplay? _selectedItem;

    public List<SeVideoControlsItem> ResultItems { get; private set; } = new List<SeVideoControlsItem>();

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    /// <summary>
    /// Raised when the order or visibility changes, so the window can update its preview.
    /// </summary>
    public event Action? LayoutChanged;

    public VideoControlsItemsViewModel()
    {
        Items = new ObservableCollection<VideoControlsItemDisplay>();
        Items.CollectionChanged += OnItemsCollectionChanged;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var item in e.NewItems?.OfType<VideoControlsItemDisplay>() ?? [])
        {
            item.PropertyChanged -= OnItemPropertyChanged;
            item.PropertyChanged += OnItemPropertyChanged;
        }

        LayoutChanged?.Invoke();
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        LayoutChanged?.Invoke();
    }

    /// <summary>
    /// The items as currently edited, in list order.
    /// </summary>
    internal List<SeVideoControlsItem> GetCurrentItems()
    {
        return Items.Select((p, i) => new SeVideoControlsItem
        {
            Type = p.Type,
            IsVisible = p.IsVisible,
            SortOrder = (i + 1) * 10,
        }).ToList();
    }

    internal void Initialize(List<SeVideoControlsItem> items)
    {
        Items.Clear();
        foreach (var item in SeVideoControlsItem.Normalize(items))
        {
            Items.Add(new VideoControlsItemDisplay(item.Type, item.IsVisible));
        }

        SelectedItem = Items.FirstOrDefault();
    }

    [RelayCommand]
    private void Ok()
    {
        ResultItems = GetCurrentItems();

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    [RelayCommand]
    private void Reset()
    {
        var selectedType = SelectedItem?.Type;
        Initialize(SeVideoControlsItem.MakeDefaults());
        SelectedItem = Items.FirstOrDefault(p => p.Type == selectedType) ?? Items.FirstOrDefault();
    }

    [RelayCommand]
    private void MoveUp()
    {
        if (SelectedItem == null)
        {
            return;
        }

        var index = Items.IndexOf(SelectedItem);
        if (index <= 0)
        {
            return;
        }

        var selected = SelectedItem;
        Items.Move(index, index - 1);
        SelectedItem = selected;
    }

    [RelayCommand]
    private void MoveDown()
    {
        if (SelectedItem == null)
        {
            return;
        }

        var index = Items.IndexOf(SelectedItem);
        if (index >= Items.Count - 1)
        {
            return;
        }

        var selected = SelectedItem;
        Items.Move(index, index + 1);
        SelectedItem = selected;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/video-player", "video-controls");
        }
    }
}
