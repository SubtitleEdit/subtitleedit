using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.UndoRedo;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Edit.ShowHistory;

public partial class ShowHistoryViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ShowHistoryDisplayItem> _historyItems;
    [ObservableProperty] private ShowHistoryDisplayItem? _selectedHistoryItem;
    [ObservableProperty] private bool _isRollbackEnabled;

    private IUndoRedoManager? _undoRedoManager;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public ShowHistoryViewModel()
    {
        HistoryItems = new ObservableCollection<ShowHistoryDisplayItem>();
    }

    public void Initialize(IUndoRedoManager undoRedoManager)
    {
        _undoRedoManager = undoRedoManager;
        foreach (var item in _undoRedoManager.UndoList.OrderByDescending(p => p.Created))
        {
            HistoryItems.Add(new ShowHistoryDisplayItem()
            {
                Time = item.Created.ToString("yyyy-MM-dd HH:mm:ss"),
                Description = item.Description,
                Hash = item.Hash,
            });
        }
    }

    [RelayCommand]
    private void RollbackTo()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}