using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Shared.PickLayer;

public partial class PickLayerViewModel : ObservableObject
{
    [ObservableProperty] private int _layer;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public PickLayerViewModel()
    {
    }

    internal void Initialize(List<SubtitleLineViewModel> selectedItems)
    {
        if (selectedItems.Count > 0)
        {
            Layer = selectedItems[0].Layer;
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
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