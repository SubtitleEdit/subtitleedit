using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.ShotChanges;

public partial class ShotChangeListViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ShotChangeItem> _shotChanges;
    [ObservableProperty] private ShotChangeItem? _selectedShotChange;
    [ObservableProperty] private bool _hasShotChanges;

    public Window? Window { get; set; }

    public bool GoToPressed { get; private set; }
    public bool OKProssed { get; private set; }

    public ShotChangeListViewModel()
    {
        ShotChanges = new ObservableCollection<ShotChangeItem>();
    }

    [RelayCommand]
    private async Task Clear()
    {
        if (Window == null)
        {
            return;
        }

        var result = await MessageBox.Show(
            Window,
            Se.Language.General.Clear,
            Se.Language.Video.ShotChanges.ShotChangesClearQuestion,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        ShotChanges.Clear();
        OKProssed = true;

        Window?.Close();
    }

    [RelayCommand]
    private void GoTo()
    {
        GoToPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task DeleteSelectedLine(ShotChangeItem shotChange)
    {
        if (shotChange == null || Window == null)
        {
            return;
        }

        var result = await MessageBox.Show(
            Window,
            Se.Language.General.DeleteCurrentLine,
            Se.Language.Video.ShotChanges.DeleteSelectedShotChangeQuestion,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        ShotChanges.Remove(shotChange);
        OKProssed = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        OKProssed = false;
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

    internal void Initialize(List<double> shotChanges)
    {
        foreach (var time in shotChanges)
        {
            ShotChanges.Add(new ShotChangeItem(ShotChanges.Count, time));
        }

        HasShotChanges = SelectedShotChange != null;
    }

    internal void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        HasShotChanges = SelectedShotChange != null;
    }

    internal void OnShotChangeGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            GoTo();
        });
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            e.Handled = true;
            if (SelectedShotChange != null)
            {
                Dispatcher.UIThread.Invoke(async void() =>
                {
                    await DeleteSelectedLine(SelectedShotChange);
                });
            }
        }
    }
}