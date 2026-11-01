using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Shared.PickAlignment;

public partial class PickAlignmentViewModel : ObservableObject
{
    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public string Alignment { get; set; }
    public string CurrentAlignment { get; private set; }

    public PickAlignmentViewModel()
    {
        Alignment = string.Empty;
        CurrentAlignment = string.Empty;
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

    /// <summary>
    /// Marks the alignment of the selected line. Without a tag a line is at the bottom center, and
    /// without a line (a whole file in Binary Edit) nothing is marked.
    /// </summary>
    internal void Initialize(SubtitleLineViewModel? selectedSubtitle, int count)
    {
        if (selectedSubtitle == null)
        {
            CurrentAlignment = string.Empty;
            return;
        }

        CurrentAlignment = Enumerable.Range(1, 9)
            .Select(number => $"an{number}")
            .FirstOrDefault(alignment => AlignmentTagHelper.HasAlignment(selectedSubtitle.Text, alignment)) ?? "an2";
    }
}