using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Features.Video.RemuxVideo;
using Nikse.SubtitleEdit.Features.Video.SpeechToText.OpenAiCompatible;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.IndexTts25License;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ModelLicense;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceCloneConsent;

namespace UITests.Features.Shared;

/// <summary>
/// Escape closes a dialog in Subtitle Edit, but each dialog wires that up itself, so the ones that
/// forgot only show up under the hands of someone using the keyboard.
/// </summary>
public class DialogEscapeTests
{
    [AvaloniaFact]
    public void PickAudioTrack_ClosesOnEscape_WithoutOk()
    {
        var vm = new PickAudioTrackViewModel();
        PressEscapeAndAssertClosed(new PickAudioTrackWindow(vm));
        Assert.False(vm.OkPressed);
    }

    [AvaloniaFact]
    public void ModelLicense_ClosesOnEscape_WithoutAccepting()
    {
        var accepted = false;
        var vm = new ModelLicenseViewModel();
        vm.Initialize(new ModelLicenseDefinition("title", "header", "intro", ["point"], "https://example.com", "https://example.com", "accept", () => accepted = true));
        vm.IsAccepted = true;
        PressEscapeAndAssertClosed(new ModelLicenseWindow(vm));
        Assert.False(vm.OkPressed);
        Assert.False(accepted);
    }

    [AvaloniaFact]
    public void IndexTts25License_ClosesOnEscape_WithoutAccepting()
    {
        var vm = new IndexTts25LicenseViewModel();
        PressEscapeAndAssertClosed(new IndexTts25LicenseWindow(vm));
        Assert.False(vm.OkPressed);
    }

    [AvaloniaFact]
    public void VoiceCloneConsent_ClosesOnEscape_WithoutAccepting()
    {
        var vm = new VoiceCloneConsentViewModel();
        PressEscapeAndAssertClosed(new VoiceCloneConsentWindow(vm));
        Assert.False(vm.OkPressed);
    }

    [AvaloniaFact]
    public void TranscriptionProgress_EscapeCancelsWhileRunning()
    {
        var vm = new TranscriptionProgressViewModel();
        PressEscapeAndAssertClosed(new TranscriptionProgressWindow(vm));
        Assert.True(vm.CancellationToken.IsCancellationRequested);
    }

    [AvaloniaFact]
    public void TranscriptionProgress_EscapeOnlyClosesWhenCompleted()
    {
        var vm = new TranscriptionProgressViewModel { IsCompleted = true };
        PressEscapeAndAssertClosed(new TranscriptionProgressWindow(vm));
        Assert.False(vm.CancellationToken.IsCancellationRequested);
    }

    [AvaloniaFact]
    public void TimeCodeUpDown_FocusTextBox_PutsFocusInTheField()
    {
        var control = new TimeCodeUpDown();
        var window = new Window { Content = control };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        control.FocusTextBox();

        Assert.IsType<TextBox>(window.FocusManager?.GetFocusedElement());
        window.Close();
    }

    private static void PressEscapeAndAssertClosed(Window window)
    {
        var closed = false;
        window.Closed += (_, _) => closed = true;
        window.Show();
        Dispatcher.UIThread.RunJobs();

        window.KeyPressQwerty(PhysicalKey.Escape, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();

        Assert.True(closed, $"{window.GetType().Name} is still open after Escape");
    }
}
