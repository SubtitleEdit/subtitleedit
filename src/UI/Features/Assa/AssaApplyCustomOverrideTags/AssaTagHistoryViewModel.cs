using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyCustomOverrideTags;

public partial class AssaTagHistoryViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _overrideTags;
    [ObservableProperty] private string? _selectedOverrideTag;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    private Subtitle _subtitle = new();

    public Subtitle ResultSubtitle => _subtitle;

    public AssaTagHistoryViewModel()
    {
        OverrideTags = new ObservableCollection<string>(Se.Settings.Assa.LastOverrideTags);
        SelectedOverrideTag = OverrideTags.FirstOrDefault();
    }

    [RelayCommand]
    private async Task Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
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
    }
}
