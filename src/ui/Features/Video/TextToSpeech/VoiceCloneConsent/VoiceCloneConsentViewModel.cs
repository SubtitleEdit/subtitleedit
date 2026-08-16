using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceCloneConsent;

/// <summary>
/// First-clone gate for text-to-speech voice cloning. Shown once, before the first reference
/// recording is imported, and remembered per terms version by
/// <see cref="VoiceCloningConsent"/>.
/// </summary>
public partial class VoiceCloneConsentViewModel : ObservableObject
{
    /// <summary>
    /// The Commission's own page on the AI Act transparency obligations. Linked rather than
    /// summarised further: the dialog states the duty, this is where a user goes to check it.
    /// </summary>
    public const string AiActUrl = "https://digital-strategy.ec.europa.eu/en/policies/regulatory-framework-ai";

    /// <summary>
    /// The points that actually change what the user may do, in the order they matter: whose
    /// voice, what has to be said about the result, why SE cannot say it for them, what is off
    /// limits, and what leaves the machine.
    /// </summary>
    public static string[] ConsentPoints =>
    [
        Se.Language.Video.TextToSpeech.VoiceCloneConsentPointPermission,
        Se.Language.Video.TextToSpeech.VoiceCloneConsentPointDisclose,
        Se.Language.Video.TextToSpeech.VoiceCloneConsentPointNoMarking,
        Se.Language.Video.TextToSpeech.VoiceCloneConsentPointNoImpersonation,
        Se.Language.Video.TextToSpeech.VoiceCloneConsentPointLocal,
        Se.Language.Video.TextToSpeech.VoiceCloneConsentPointModelLicense,
    ];

    [ObservableProperty] private bool _isAccepted;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    [RelayCommand]
    private void Accept()
    {
        if (!IsAccepted)
        {
            return;
        }

        VoiceCloningConsent.Accept();
        Se.SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        // "Not now": leaves the terms unaccepted so the clone is refused and the question comes
        // back next time. See VoiceCloningConsent.Decline for why this stops short of switching
        // cloning off outright.
        VoiceCloningConsent.Decline();
        Se.SaveSettings();
        OkPressed = false;
        Window?.Close();
    }

    [RelayCommand]
    private void OpenAiAct()
    {
        try
        {
            Process.Start(new ProcessStartInfo(AiActUrl) { UseShellExecute = true });
        }
        catch (System.Exception ex)
        {
            Se.LogError(ex, $"Voice clone consent: could not open {AiActUrl}");
        }
    }
}
