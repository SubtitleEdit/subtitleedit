using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Shared.BinaryEdit.BinarySettings;

public partial class BinarySettingsViewModel : ObservableObject
{
    [ObservableProperty] private int _marginLeft;
    [ObservableProperty] private int _marginTop;
    [ObservableProperty] private int _marginRight;
    [ObservableProperty] private int _marginBottom;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public BinarySettingsViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        MarginLeft = Se.Settings.Tools.BinEditLeftMargin;
        MarginTop = Se.Settings.Tools.BinEditTopMargin;
        MarginRight = Se.Settings.Tools.BinEditRightMargin;
        MarginBottom = Se.Settings.Tools.BinEditBottomMargin;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.BinEditLeftMargin = MarginLeft;
        Se.Settings.Tools.BinEditTopMargin = MarginTop;
        Se.Settings.Tools.BinEditRightMargin = MarginRight;
        Se.Settings.Tools.BinEditBottomMargin = MarginBottom;

        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task Ok()
    {
        var msg = GetValidationError();
        if (!string.IsNullOrEmpty(msg))
        {
            await MessageBox.Show(Window!, Se.Language.General.Error, msg, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    private string GetValidationError()
    {
        if (Window == null)
        {
            return "Window is null";
        }

        if (MarginLeft < 0)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Left margin");
        }

        if (MarginTop < 0)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Top margin");
        }

        if (MarginRight < 0)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Right margin");
        }

        if (MarginBottom < 0)
        {
            return string.Format(Se.Language.General.PleaseEnterAValidValueForX, "Bottom margin");
        }

        return string.Empty;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}
