using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;

public partial class AssaTagHistoryViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _overrideTags;
    [ObservableProperty] private string? _selectedOverrideTag;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    public AssaTagHistoryViewModel()
    {
        OverrideTags = new ObservableCollection<string>(Se.Settings.Assa.LastOverrideTags);
        SelectedOverrideTag = OverrideTags.FirstOrDefault();
    }

    [RelayCommand]
    private void Ok()
    {
        Se.Settings.Assa.LastOverrideTags= OverrideTags.ToList();
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private void DeleteItem()
    {
        if (SelectedOverrideTag == null)
        {
            return;
        }

        var tag = SelectedOverrideTag;
        OverrideTags.Remove(tag);
        SelectedOverrideTag = OverrideTags.FirstOrDefault();
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
        else if (e.Key == Key.Enter)
        {
            Ok();
        }
    }
}
