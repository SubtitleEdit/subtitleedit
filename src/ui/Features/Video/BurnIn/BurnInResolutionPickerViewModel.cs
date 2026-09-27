using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
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
        var item = Resolutions.FirstOrDefault(p => p.ItemType == ResolutionItemType.UseSource);
        if (item != null)
        {
            Resolutions.Remove(item);
        }
    }

    public void RemovePickResolution()
    {
        var item = Resolutions.FirstOrDefault(p => p.ItemType == ResolutionItemType.PickResolution);
        if (item != null)
        {
            Resolutions.Remove(item);
        }
    }

    public void SetSourceResolution(int width, int height)
    {
        var item = Resolutions.FirstOrDefault(p => p.ItemType == ResolutionItemType.UseSource);
        if (item != null && width > 0 && height > 0)
        {
            item.DisplayName = $"{Se.Language.General.UseSourceResolution} ({width}x{height})";
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
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/burn-in", "video-settings");
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