using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.PickLayerFilter;

public partial class PickLayerFilterViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<LayerItem> _layers;
    [ObservableProperty] private LayerItem? _selectedLayer;
    [ObservableProperty] private bool _hideFromWaveform;
    [ObservableProperty] private bool _hideFromGridView;
    [ObservableProperty] private bool _hideFromVideoPreview;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public List<int>? SelectedLayers { get; private set; }
    public DataGrid LayerGrid { get; set; }

    public PickLayerFilterViewModel()
    {
        Layers = new ObservableCollection<LayerItem>();
        SelectedLayers = new List<int>();
        LayerGrid = new DataGrid();
        HideFromWaveform = Se.Settings.Assa.HideLayersFromWaveform;
        HideFromGridView = Se.Settings.Assa.HideLayersFromSubtitleGrid;
        HideFromVideoPreview = Se.Settings.Assa.HideLayersFromVideoPreview;
    }

    internal void Initialize(List<SubtitleLineViewModel> subtitleLineViewModels, List<int>? visibleLayers)
    {
        if (visibleLayers == null)
        {
            visibleLayers = subtitleLineViewModels.Select(p => p.Layer).Distinct().OrderBy(p => p).ToList();
        }

        foreach (var layer in subtitleLineViewModels.Select(p => p.Layer).Distinct().OrderBy(p => p))
        {
            var layerUsageCount = subtitleLineViewModels.Count(p => p.Layer == layer);
            var item = new LayerItem(layer, visibleLayers.Contains(layer), layerUsageCount);
            Layers.Add(item);
        }
    }

    [RelayCommand]
    private void SelectAll()
    {
        foreach (var layer in Layers)
        {
            layer.IsSelected = true;
        }
    }

    [RelayCommand]
    private void RemoveFilter()
    {
        SelectedLayers = null;
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void InvertSelection()
    {
        foreach (var layer in Layers)
        {
            layer.IsSelected = !layer.IsSelected;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        Se.Settings.Assa.HideLayersFromWaveform = HideFromWaveform;
        Se.Settings.Assa.HideLayersFromSubtitleGrid = HideFromGridView;
        Se.Settings.Assa.HideLayersFromVideoPreview = HideFromVideoPreview;
        Se.SaveSettings();

        SelectedLayers = Layers.Where(l => l.IsSelected).Select(l => l.Layer).ToList();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Return || e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
        else if (e.Key == Key.A && (e.KeyModifiers & KeyModifiers.Control) != 0)
        {
            e.Handled = true;
            SelectAll();
        }
        else if (e.Key == Key.A && (e.KeyModifiers & KeyModifiers.Meta) != 0)
        {
            e.Handled = true;
            SelectAll();
        }
        else if (e.Key == Key.I && (e.KeyModifiers & KeyModifiers.Shift) != 0)
        {
            e.Handled = true;
            InvertSelection();
        }
    }

    internal void Loaded()
    {
        SelectAndScrollToRow(LayerGrid, 0);
    }

    private void SelectAndScrollToRow(DataGrid? datagrid, int index)
    {
        if (index < 0 || datagrid == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            datagrid.Focus();

            if (datagrid.SelectedIndex != index)
            {
                datagrid.SelectedIndex = index;
            }

            datagrid.ScrollIntoView(datagrid.SelectedItem, null);
        });
    }

    internal void LayerGridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Space)
        {
            e.Handled = true;
            var selectedItem = LayerGrid.SelectedItem as LayerItem;
            if (selectedItem != null)
            {
                selectedItem.IsSelected = !selectedItem.IsSelected;
            }
        }
        else if (e.Key == Key.Enter || e.Key == Key.Return)
        {
            e.Handled = true;
            Ok();
        }
        else if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.A && (e.KeyModifiers & KeyModifiers.Control) != 0)
        {
            e.Handled = true;
            SelectAll();
        }
        else if (e.Key == Key.A && (e.KeyModifiers & KeyModifiers.Meta) != 0)
        {
            e.Handled = true;
            SelectAll();
        }
        else if (e.Key == Key.I && (e.KeyModifiers & KeyModifiers.Shift) != 0)
        {
            e.Handled = true;
            InvertSelection();
        }
        else if (e.Key == Key.Up && (e.KeyModifiers & KeyModifiers.Alt) != 0)
        {
            e.Handled = true;
            ScrollUp();
        }
        else if (e.Key == Key.Down && (e.KeyModifiers & KeyModifiers.Alt) != 0)
        {
            e.Handled = true;
            ScrollDown();
        }
    }

    private void ScrollDown()
    {
        var idx = LayerGrid.SelectedIndex;
        if (idx != -1)
        {
            var nextIdx = Math.Min(Layers.Count - 1, idx + 1);
            SelectAndScrollToRow(LayerGrid, nextIdx);
        }
    }

    private void ScrollUp()
    {
        var idx = LayerGrid.SelectedIndex;
        if (idx != -1)
        {
            var nextIdx = Math.Max(0, idx - 1);
            SelectAndScrollToRow(LayerGrid, nextIdx);
        }
    }
}