using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Edit.AlignmentPicker;

public partial class AlignmentPickerViewModel : ObservableObject
{
    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public string Alignment { get; private set; }

    public AlignmentPickerViewModel()
    {
        Alignment = string.Empty;
    }
    
    [RelayCommand]                   
    private void An1()
    {
        Alignment = "an1";
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void An2() 
    {
        Alignment = "an2";
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void An3() 
    {
        Alignment = "an3";
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void An4() 
    {
        Alignment = "an4";
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void An5() 
    {
        Alignment = "an5";
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void An6() 
    {
        Alignment = "an6";
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void An7() 
    {
        Alignment = "an7";
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void An8() 
    {
        Alignment = "an8";
        OkPressed = true;
        Window?.Close();
    }
    
    [RelayCommand]                   
    private void An9() 
    {
        Alignment = "an9";
        OkPressed = true;
        Window?.Close();
    }
    
    public void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && Window is not null)
        {
            e.Handled = true;   
            Window.Close();
        }
    }
}