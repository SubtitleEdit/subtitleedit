using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Video.BurnIn;

public partial class BurnInResolutionPickerViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ResolutionItem> _resolutions;
    [ObservableProperty] private ResolutionItem? _selectedResolution;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public BurnInResolutionPickerViewModel()
    {
        Resolutions = new ObservableCollection<ResolutionItem>(ResolutionItem.GetResolutions());
    }

    public void RemoveUseSourceResolution()
    {
        Resolutions.Remove(Resolutions.First(p => p.ItemType == ResolutionItemType.UseSource));
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

    internal void ResolutionItemClicked(ResolutionItem item)
    {
        if (item.IsSeparator)
        {
            return; // ignore separators
        }

        SelectedResolution = item;
        Ok();
    }
}