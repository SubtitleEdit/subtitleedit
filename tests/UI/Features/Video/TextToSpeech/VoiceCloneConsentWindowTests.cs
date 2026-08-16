using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.LogicalTree;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceCloneConsent;
using Nikse.SubtitleEdit.Logic.Config;

namespace UITests.Features.Video.TextToSpeech;

/// <summary>
/// The consent window is built in code, and its one real invariant - the accept button cannot be
/// pressed until the checkbox is ticked - lives in a binding that a build cannot prove. These tests
/// construct the window and check it.
/// </summary>
public class VoiceCloneConsentWindowTests : IDisposable
{
    // Every window opened by a test is closed again in Dispose: a window left open outlives the
    // test and races with the headless session teardown.
    private readonly List<Window> _windows = new();

    public void Dispose()
    {
        foreach (var window in _windows)
        {
            window.Close();
        }

        _windows.Clear();
    }

    private Window Open(VoiceCloneConsentViewModel vm)
    {
        var window = new VoiceCloneConsentWindow(vm);
        _windows.Add(window);
        return window;
    }

    private static Button? FindButton(Control root, string content) =>
        root.GetLogicalDescendants().OfType<Button>().FirstOrDefault(b => Equals(b.Content, content));

    [AvaloniaFact]
    public void AcceptButton_IsDisabledUntilTheCheckboxIsTicked()
    {
        var vm = new VoiceCloneConsentViewModel();
        var window = Open(vm);

        var accept = FindButton(window, Se.Language.Video.TextToSpeech.VoiceCloneConsentAccept);
        Assert.NotNull(accept);
        Assert.False(accept!.IsEnabled, "accepting must be a deliberate act, not a click on a default-focused button");

        vm.IsAccepted = true;

        Assert.True(accept.IsEnabled);
    }

    [AvaloniaFact]
    public void EveryConsentPointReachesTheWindow()
    {
        // A dropped bullet is a term the user was never shown, which is the whole point of the
        // dialog - and nothing else would notice.
        var window = Open(new VoiceCloneConsentViewModel());

        var texts = window.GetLogicalDescendants().OfType<TextBlock>()
            .Select(t => t.Text ?? string.Empty)
            .ToList();

        foreach (var point in VoiceCloneConsentViewModel.ConsentPoints)
        {
            Assert.Contains(point, texts);
        }

        Assert.Contains(Se.Language.Video.TextToSpeech.VoiceCloneConsentCheckBox, texts);
    }
}
