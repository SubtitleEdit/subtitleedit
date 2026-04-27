using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Assa;

public partial class AssaStylePickerViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _buttonAcceptText;
    [ObservableProperty] private ObservableCollection<StyleDisplay> _styles;
    [ObservableProperty] private StyleDisplay? _selectedStyle;
    [ObservableProperty] private bool _showUsageCount;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    private Subtitle _subtitle;

    public AssaStylePickerViewModel()
    {
        Title = string.Empty;
        Styles = new ObservableCollection<StyleDisplay>();
        ButtonAcceptText = string.Empty;
        _subtitle = new Subtitle();
    }

    [RelayCommand]
    private void Ok()
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

    public void Initialize(List<StyleDisplay> styles, string buttonAcceptText, bool showUsageCount)
    {
        Styles.AddRange(styles);
        
        if (Styles.Count > 0)
        {
            SelectedStyle = Styles[0];
        }

        ButtonAcceptText = buttonAcceptText;
        ShowUsageCount = showUsageCount;
    }
    
    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
