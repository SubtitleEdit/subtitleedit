using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

/// <summary>
/// Prompts for a free width x height for image based export, e.g. 1920x800 for a cropped
/// video (#17132).
/// </summary>
public partial class CustomResolutionViewModel : ObservableObject
{
    public const int MinSize = 16;
    public const int MaxSize = 16384;

    [ObservableProperty] private int? _resolutionWidth;
    [ObservableProperty] private int? _resolutionHeight;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public CustomResolutionViewModel()
    {
        ResolutionWidth = 1920;
        ResolutionHeight = 1080;
    }

    public void Initialize(int width, int height)
    {
        ResolutionWidth = width is >= MinSize and <= MaxSize ? width : 1920;
        ResolutionHeight = height is >= MinSize and <= MaxSize ? height : 1080;
    }

    public int Width => ResolutionWidth ?? 0;
    public int Height => ResolutionHeight ?? 0;

    [RelayCommand]
    private void Ok()
    {
        if (ResolutionWidth is not (>= MinSize and <= MaxSize) ||
            ResolutionHeight is not (>= MinSize and <= MaxSize))
        {
            return;
        }

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
    }
}
