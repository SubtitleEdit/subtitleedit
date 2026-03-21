using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls;

namespace Nikse.SubtitleEdit.Features.Video.GoToVideoPosition;

public partial class GoToVideoPositionViewModel : ObservableObject
{
    [ObservableProperty] private TimeSpan _time;
    
    public Window? Window { get; set; }
    public TimeCodeUpDown UpDown { get; set; }

    public bool OkPressed { get; private set; }

    public GoToVideoPositionViewModel()
    {
        UpDown = new TimeCodeUpDown();   
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

    public void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Cancel();
        }
        else if (e.Key == Key.Enter)
        {
            Ok();
        }
    }

    public void Activated()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var textBox = UpDown.GetVisualDescendants()
                .OfType<TextBox>()
                .FirstOrDefault();
            textBox?.SelectAll();
            UpDown.Focus(); 
        });
    }
}