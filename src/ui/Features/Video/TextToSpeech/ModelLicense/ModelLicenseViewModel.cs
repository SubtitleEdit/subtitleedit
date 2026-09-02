using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ModelLicense;

/// <summary>
/// Everything one engine's licence gate needs to say and do. The summary points are
/// deliberately a summary with a link to the full text rather than the whole agreement —
/// nobody reads a wall of legalese in a modal, and the points listed are the ones that bite.
/// </summary>
/// <param name="DialogTitle">Window title, e.g. "Higgs Audio v3 - model license".</param>
/// <param name="Header">Headline inside the dialog.</param>
/// <param name="Intro">One paragraph on why this dialog exists.</param>
/// <param name="SummaryPoints">The terms that actually change what a user may do.</param>
/// <param name="LicenseUrl">Full licence text.</param>
/// <param name="ModelPageUrl">The model's home page.</param>
/// <param name="AcceptCheckBoxText">Checkbox label naming the licence being accepted.</param>
/// <param name="Accept">Stamps the acceptance into settings (persisting is the VM's job).</param>
public sealed record ModelLicenseDefinition(
    string DialogTitle,
    string Header,
    string Intro,
    string[] SummaryPoints,
    string LicenseUrl,
    string ModelPageUrl,
    string AcceptCheckBoxText,
    Action Accept);

/// <summary>
/// First-run licence gate for model weights that carry their own terms. The audio.cpp engine
/// binaries are Apache-2.0 and need no acceptance, but several of the models SE offers on that
/// runtime (Higgs Audio v3, Fish Audio S2 Pro) are research / non-commercial licensed, which
/// puts real conditions on the user. So this is shown once, before the multi-GB download
/// starts, and the answer is remembered per licence version by the engine's own
/// IsLicenseAccepted / AcceptLicense pair — the same scheme
/// <see cref="Engines.IndexTts25AudioCpp.LicenseVersion"/> uses.
/// </summary>
public partial class ModelLicenseViewModel : ObservableObject
{
    [ObservableProperty] private bool _isAccepted;

    public ModelLicenseDefinition Definition { get; private set; } =
        new(string.Empty, string.Empty, string.Empty, Array.Empty<string>(), string.Empty, string.Empty, string.Empty, () => { });

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public void Initialize(ModelLicenseDefinition definition)
    {
        Definition = definition;
    }

    [RelayCommand]
    private void Accept()
    {
        if (!IsAccepted)
        {
            return;
        }

        // The definition's Accept only assigns the settings field; persist it right away, as
        // the voice-cloning consent dialog does - otherwise an acceptance given once is lost if
        // the session ends before the next settings save and the user is prompted again.
        Definition.Accept();
        Se.SaveSettings();
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
    private void OpenLicense() => OpenUrl(Definition.LicenseUrl);

    [RelayCommand]
    private void OpenModelPage() => OpenUrl(Definition.ModelPageUrl);

    private static void OpenUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"Model license dialog: could not open {url}");
        }
    }
}
