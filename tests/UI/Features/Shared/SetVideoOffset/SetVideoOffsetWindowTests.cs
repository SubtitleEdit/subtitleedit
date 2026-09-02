using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Shared.SetVideoOffset;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;

namespace UITests.Features.Shared.SetVideoOffset;

/// <summary>
/// The window is built in code, so the drop-down of previously used offsets and the Apply button
/// next to OK are only there if the window actually adds them.
/// </summary>
public class SetVideoOffsetWindowTests : IDisposable
{
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private SetVideoOffsetWindow Open(SetVideoOffsetViewModel vm)
    {
        var window = new SetVideoOffsetWindow(vm);
        _windows.Add(window);
        return window;
    }

    private static SetVideoOffsetViewModel NewViewModel()
    {
        var vm = new SetVideoOffsetViewModel();
        vm.Initialize((_, _, _) => { }, () => { });
        return vm;
    }

    [AvaloniaFact]
    public void TheHistoryDropDownIsWiredToTheOffsetField()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        Se.Settings.General.VideoOffsetHistoryInMs = new List<long> { 35980000 };

        var vm = NewViewModel();
        var window = Open(vm);

        var comboBox = window.GetLogicalDescendants().OfType<ComboBox>().FirstOrDefault();
        Assert.NotNull(comboBox);
        Assert.Equal(vm.OffsetHistory, comboBox!.ItemsSource);

        comboBox.SelectedIndex = 0;

        Assert.Equal(TimeSpan.FromMilliseconds(35980000), vm.TimeOffset);
    }

    [AvaloniaFact]
    public void ApplySitsNextToOkResetAndCancel()
    {
        using var _ = new SettingsScope("General.VideoOffsetHistoryInMs");
        var window = Open(NewViewModel());

        var buttonTexts = window.GetLogicalDescendants().OfType<Button>()
            .Select(b => b.Content as string ?? string.Empty)
            .ToList();

        // The OK/Cancel texts carry an access-key underscore that never reaches the button.
        Assert.Contains(Se.Language.General.Ok.Replace("_", string.Empty), buttonTexts);
        Assert.Contains(Se.Language.General.Apply, buttonTexts);
        Assert.Contains(Se.Language.General.Reset, buttonTexts);
        Assert.Contains(Se.Language.General.Cancel.Replace("_", string.Empty), buttonTexts);
    }
}
