using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic.Config;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.IndexTts25License;

/// <summary>
/// First-run licence gate for the IndexTTS-2.5 weights. The audio.cpp engine binaries are
/// Apache-2.0 and need no acceptance, but the model weights are under the bilibili Model Use
/// License, which is not an OSI-approved open-source licence and puts real conditions on the
/// user (commercial thresholds, prohibited uses, and a duty to pass the terms downstream).
/// So this is shown once, before the 3.3 GB download starts, and the answer is remembered per
/// licence version in <see cref="IndexTts25AudioCpp.LicenseVersion"/>.
/// </summary>
public partial class IndexTts25LicenseViewModel : ObservableObject
{
    public const string LicenseUrl = "https://huggingface.co/IndexTeam/IndexTTS-2.5/blob/main/LICENSE";
    public const string ModelUrl = "https://huggingface.co/IndexTeam/IndexTTS-2.5";

    /// <summary>
    /// Summary of the terms that actually change what a user may do. Deliberately a summary
    /// with a link to the full text rather than the whole agreement — nobody reads a wall of
    /// legalese in a modal, and the points below are the ones that bite.
    /// </summary>
    public static string[] LicenseSummaryPoints { get; } =
    {
        "The model may be used free of charge, including for commercial work, unless your product had over 100 million monthly active users last month or over 1 billion RMB revenue last year — those cases need a separate licence from bilibili.",
        "Generated audio must not be used to build or improve other AI models, except IndexTTS itself or non-commercial models.",
        "High-risk deployment (medical, autonomous driving, military, critical infrastructure, large-scale biometric surveillance, automated credit or employment decisions) is prohibited without your own compliance work.",
        "If you pass the model or anything derived from it to someone else, you must pass these terms on with it.",
        "The model is provided as is, with no warranty, and bilibili accepts no liability for what is generated.",
        "You are responsible for having the right to clone any voice you use as a reference. The model does not check consent.",
    };

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

        IndexTts25AudioCpp.AcceptLicense();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        OkPressed = false;
        Window?.Close();
    }

    [RelayCommand]
    private void OpenLicense() => OpenUrl(LicenseUrl);

    [RelayCommand]
    private void OpenModelPage() => OpenUrl(ModelUrl);

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (System.Exception ex)
        {
            Se.LogError(ex, $"IndexTTS 2.5: could not open {url}");
        }
    }
}
