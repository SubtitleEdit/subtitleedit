using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Logic;

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
            e.Handled = true; // the OK button is IsDefault and would run OK again on the same Enter
            Ok();
        }
        else if (UiUtil.IsHelp(e))
        {
            e.Handled = true;
            UiUtil.ShowHelp("features/video-player", "go-to-video-position");
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