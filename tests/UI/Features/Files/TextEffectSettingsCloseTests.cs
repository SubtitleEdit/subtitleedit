using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Headless.XUnit;
using Nikse.SubtitleEdit.Features.Files.ExportImageBased;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using Nikse.SubtitleEdit.UiLogic.Export;

namespace UITests.Features.Files;

/// <summary>
/// The text effect settings dialog pushes every slider change straight into the export dialog's
/// view model so the preview follows live. Any close that is not an OK must put the original
/// values back - the Cancel button and Escape go through Cancel(), while the title bar X and
/// Alt+F4 only raise the window's Closing event, which the window forwards to OnWindowClosing().
/// </summary>
public class TextEffectSettingsCloseTests
{
    private sealed class NullServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private static ExportImageBasedViewModel BuildParent()
    {
        var parent = new ExportImageBasedViewModel(
            new FileHelper(),
            new FolderHelper(),
            new WindowService(new NullServiceProvider()));

        var subtitles = new ObservableCollection<SubtitleLineViewModel>
        {
            new SubtitleLineViewModel
            {
                Number = 1,
                Text = "Hello world",
                StartTime = TimeSpan.FromSeconds(1),
                EndTime = TimeSpan.FromSeconds(3),
            },
        };

        parent.Initialize(new ExportHandlerBluRaySup(), subtitles, null, null);

        parent.SelectedTextEffect = parent.TextEffectItems.First(t => t.Preset == TextEffectPreset.NeonGlow);
        parent.TextEffectStrength = 120;
        parent.TextEffectLetterSpacing = 5;
        parent.TextEffectArcBend = 10;
        parent.TextEffectWave = 15;
        return parent;
    }

    [AvaloniaFact]
    public void CloseWithoutOk_PutsTheLivePushedValuesBack()
    {
        var parent = BuildParent();
        var vm = new TextEffectViewModel();
        vm.Initialize(parent);

        vm.SelectedPreset = vm.Presets.First(p => p.Preset == TextEffectPreset.Fire);
        vm.Strength = 250;
        vm.Wave = 90;

        // The changes reached the parent live...
        Assert.Equal(TextEffectPreset.Fire, parent.SelectedTextEffect?.Preset);
        Assert.Equal(250, parent.TextEffectStrength);
        Assert.Equal(90, parent.TextEffectWave);

        // ...and a close without OK (title bar X, Alt+F4) takes them back out.
        vm.OnWindowClosing();

        Assert.Equal(TextEffectPreset.NeonGlow, parent.SelectedTextEffect?.Preset);
        Assert.Equal(120, parent.TextEffectStrength);
        Assert.Equal(5, parent.TextEffectLetterSpacing);
        Assert.Equal(10, parent.TextEffectArcBend);
        Assert.Equal(15, parent.TextEffectWave);
    }

    [AvaloniaFact]
    public void CloseAfterOk_KeepsTheValues()
    {
        var parent = BuildParent();
        var vm = new TextEffectViewModel();
        vm.Initialize(parent);

        vm.Strength = 250;
        vm.OkCommand.Execute(null);
        vm.OnWindowClosing();

        Assert.Equal(250, parent.TextEffectStrength);
    }

    /// <summary>
    /// Cancel restores and then closes, and the close raises the window's Closing event too -
    /// the restore must not run a second time over values set after the first one.
    /// </summary>
    [AvaloniaFact]
    public void CancelFollowedByTheClosingEvent_RestoresOnlyOnce()
    {
        var parent = BuildParent();
        var vm = new TextEffectViewModel();
        vm.Initialize(parent);

        vm.Strength = 250;
        vm.CancelCommand.Execute(null);
        Assert.Equal(120, parent.TextEffectStrength);

        parent.TextEffectStrength = 175;
        vm.OnWindowClosing();

        Assert.Equal(175, parent.TextEffectStrength);
    }
}
