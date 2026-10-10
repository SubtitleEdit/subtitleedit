using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Sync.ChangeFrameRate;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Sync.ChangeFrameRate;

/// <summary>
/// Typing a rate that is not in the list must not flag the combo boxes with
/// "Could not convert '(null)' to System.Double" (#15869).
/// </summary>
public class ChangeFrameRateWindowTypedRateTests : IDisposable
{
    private readonly double _savedFrom = Se.Settings.Synchronization.ChangeFrameRateFrom;
    private readonly double _savedTo = Se.Settings.Synchronization.ChangeFrameRateTo;

    public void Dispose()
    {
        Se.Settings.Synchronization.ChangeFrameRateFrom = _savedFrom;
        Se.Settings.Synchronization.ChangeFrameRateTo = _savedTo;
    }

    private static (ChangeFrameRateViewModel Vm, ChangeFrameRateWindow Window, ComboBox From, ComboBox To) Show()
    {
        Se.Settings.Synchronization.ChangeFrameRateFrom = 23.976;
        Se.Settings.Synchronization.ChangeFrameRateTo = 25;
        var vm = new ChangeFrameRateViewModel(new StubFileHelper());
        var window = new ChangeFrameRateWindow(vm);
        window.Show();
        var combos = window.GetLogicalDescendants().OfType<ComboBox>().ToList();
        return (vm, window, combos[0], combos[1]);
    }

    [AvaloniaFact]
    public void TypingUnlistedRateShowsNoErrorAndKeepsCurrentRate()
    {
        var (vm, window, from, to) = Show();

        from.Text = "12";
        to.Text = "15";

        Assert.False(DataValidationErrors.GetHasErrors(from));
        Assert.False(DataValidationErrors.GetHasErrors(to));
        Assert.Equal(23.976, vm.SelectedFromFrameRate);
        Assert.Equal(25, vm.SelectedToFrameRate);
        window.Close();
    }
}
