using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Nikse.SubtitleEdit.Features.Shared.PromptCheckBox;

public partial class PromptCheckBoxViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _checkBoxText;
    [ObservableProperty] private bool _isChecked;

    /// <summary>Optional explanatory text shown above the check box; the block is hidden when empty.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasInfoText))]
    private string _infoText;

    public bool HasInfoText => !string.IsNullOrEmpty(InfoText);

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public PromptCheckBoxViewModel()
    {
        Title = string.Empty;
        CheckBoxText = string.Empty;
        InfoText = string.Empty;
    }

    internal void Initialize(string title, string checkBoxText, bool isChecked, string infoText = "")
    {
        Title = title;
        CheckBoxText = checkBoxText;
        IsChecked = isChecked;
        InfoText = infoText;
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
        else if (e.Key == Key.Enter)
        {
            e.Handled = true;
            Ok();
        }
    }
}
