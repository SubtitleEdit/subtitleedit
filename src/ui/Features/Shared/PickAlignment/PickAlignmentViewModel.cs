using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;

namespace Nikse.SubtitleEdit.Features.Shared.PickAlignment;

public partial class PickAlignmentViewModel : ObservableObject
{
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public string Alignment { get; set; }

    public PickAlignmentViewModel()
    {
        Alignment = string.Empty;
    }

    [RelayCommand]
    private void SetAlignment(string alignment)
    {
        Ok(alignment);
    }

    private void Ok(string alignment)
    {
        Alignment = alignment;
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

    internal void Initialize(SubtitleLineViewModel? selectedSubtitle, int count)
    {
        
    }
}