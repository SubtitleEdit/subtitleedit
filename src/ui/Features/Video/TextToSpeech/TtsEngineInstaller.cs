using Avalonia.Controls;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.IndexTts25License;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ModelLicense;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech;

/// <summary>
/// The engine install/download flow (engine binaries, models, runtime prompts) shared by the
/// main TTS window and the review window. Extracted from TextToSpeechViewModel so the review
/// window's Regenerate can offer the same download prompts instead of silently doing nothing
/// when the selected engine is not installed.
/// </summary>
public static class TtsEngineInstaller
{
    /// <param name="apiKey">The API key to validate for cloud engines; pass null to use the
    /// engine's saved key from settings (review window, which has no API-key field).</param>
    public static async Task<bool> EnsureEngineInstalled(
        ITtsEngine engine,
        Window? window,
        IWindowService windowService,
        string? region,
        string? model,
        string? apiKeyOverride,
        string? keyFileOverride,
        Func<Task> refreshVoices)
    {
        if (window == null)
        {
            return false;
        }

        var apiKey = apiKeyOverride ?? GetSavedApiKey(engine);
        var keyFile = keyFileOverride ?? GetSavedKeyFile(engine);

        if (engine is Qwen3TtsCpp)
        {
            if (!await engine.IsInstalled(region))
            {
                var qwen3Variant = Qwen3TtsCppDownloadService.WindowsVariantVulkan;
                if (Configuration.IsRunningOnWindows)
                {
                    var variantAnswer = await MessageBox.Show(
                        window,
                        string.Format(Se.Language.General.DownloadXQuestion, "Qwen3 TTS"),
                        Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.RequiresXSelectBuild, "Qwen3 TTS"),
                        MessageBoxButtons.Cancel,
                        MessageBoxIcon.Question,
                        "CPU",
                        "Vulkan (GPU)",
                        "CUDA (NVIDIA GPU)");

                    if (variantAnswer == MessageBoxResult.None || variantAnswer == MessageBoxResult.Cancel)
                    {
                        return false;
                    }

                    qwen3Variant = variantAnswer switch
                    {
                        MessageBoxResult.Custom1 => Qwen3TtsCppDownloadService.WindowsVariantCpu,
                        MessageBoxResult.Custom3 => Qwen3TtsCppDownloadService.WindowsVariantCuda,
                        _ => Qwen3TtsCppDownloadService.WindowsVariantVulkan,
                    };

                    if (qwen3Variant == Qwen3TtsCppDownloadService.WindowsVariantVulkan && !VulkanHelper.IsInstalled())
                    {
                        var vulkanAnswer = await MessageBox.Show(
                            window,
                            Se.Language.General.VulkanRuntimeMayBeRequired,
                            Se.Language.General.VulkanRuntimeNotDetectedContinue,
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (vulkanAnswer == MessageBoxResult.No)
                        {
                            UiUtil.OpenUrl("https://vulkan.lunarg.com/sdk/home");
                            return false;
                        }

                        if (vulkanAnswer != MessageBoxResult.Yes)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    var answer = await MessageBox.Show(
                        window,
                        string.Format(Se.Language.General.DownloadXQuestion, "Qwen3 TTS"),
                        Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.RequiresXDownloadAndUse, "Qwen3 TTS"),
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (answer != MessageBoxResult.Yes)
                    {
                        return false;
                    }
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadQwen3TtsCpp(qwen3Variant));
                if (!dlResult.OkPressed)
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
            }

            var qwen3ModelKey = Qwen3TtsCpp.ResolveModelKey(model);
            if (!Qwen3TtsCpp.IsModelsInstalled(qwen3ModelKey))
            {
                var sizeText = GetModelDownloadSizeText(engine, model);
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelsQuestion, "Qwen3 TTS"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelsSizeZDownload, "Qwen3 TTS", qwen3ModelKey, sizeText),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadQwen3TtsModels(qwen3ModelKey));
                return dlResult.OkPressed && Qwen3TtsCpp.IsModelsInstalled(qwen3ModelKey);
            }

            return true;
        }

        if (engine is Qwen3TtsCrispAsr)
        {
            // Runtime first: the same crispasr.exe that Speech-to-text / Chatterbox use.
            if (!await TtsVoiceInstaller.EnsureCrispAsrForQwen3(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var crispAsrModelKey = Qwen3TtsCrispAsr.ResolveModelKey(model);
            if (!Qwen3TtsCrispAsr.AreModelsInstalled(crispAsrModelKey))
            {
                var sizeText = GetModelDownloadSizeText(engine, model);
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelsQuestion, "Qwen3 TTS (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelsSizeZDownload, "Qwen3 TTS (CrispASR)", crispAsrModelKey, sizeText),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadQwen3TtsCrispAsrModels(crispAsrModelKey));
                if (!dlResult.OkPressed || !Qwen3TtsCrispAsr.AreModelsInstalled(crispAsrModelKey))
                {
                    return false;
                }

                // The download dialog also pulls voices.zip when none are present, so
                // refresh the voice list to surface them in the combo.
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is VibeVoiceCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForVibeVoice(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var vibeModelKey = VibeVoiceCrispAsr.ResolveModelKey(model);
            if (!VibeVoiceCrispAsr.AreModelsInstalled(vibeModelKey))
            {
                // Model key already includes the size in its label (e.g. "Q8_0 (~2.8 GB)") so
                // we don't append a separate size — avoids duplication in the prompt.
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelQuestion, "VibeVoice (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelDownload, "VibeVoice (CrispASR)", vibeModelKey),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadVibeVoiceCrispAsrModels(vibeModelKey));
                if (!dlResult.OkPressed || !VibeVoiceCrispAsr.AreModelsInstalled(vibeModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is IndexTtsCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForIndexTts(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var indexModelKey = IndexTtsCrispAsr.ResolveModelKey(model);
            if (!IndexTtsCrispAsr.AreModelsInstalled(indexModelKey))
            {
                // Model key already includes the size in its label (e.g. "Q8_0 (~870 MB)")
                // so we don't append a separate size — avoids duplication in the prompt.
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelsQuestion, "IndexTTS (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelsDownload, "IndexTTS (CrispASR)", indexModelKey),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadIndexTtsCrispAsrModels(indexModelKey));
                if (!dlResult.OkPressed || !IndexTtsCrispAsr.AreModelsInstalled(indexModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is SupertonicCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForSupertonic(window, windowService, forceRedownload: false))
            {
                return false;
            }

            if (!SupertonicCrispAsr.IsModelInstalled())
            {
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelQuestion, "Supertonic (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XRequiresModelSizeYDownload, "Supertonic (CrispASR)", "~200 MB"),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadSupertonicCrispAsrModels());
                if (!dlResult.OkPressed || !SupertonicCrispAsr.IsModelInstalled())
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is PocketTtsCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForPocketTts(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var pocketModelKey = PocketTtsCrispAsr.ResolveModelKey(model);
            if (!PocketTtsCrispAsr.AreModelsInstalled(pocketModelKey))
            {
                // Model key already carries the language and size in its label (e.g.
                // "German Q8_0 (~124 MB)") so we don't append a separate size.
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelQuestion, "Pocket TTS (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelDownload, "Pocket TTS (CrispASR)", pocketModelKey),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadPocketTtsCrispAsrModels(pocketModelKey));
                if (!dlResult.OkPressed || !PocketTtsCrispAsr.AreModelsInstalled(pocketModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is Confucius4TtsCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForConfucius4Tts(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var confuciusModelKey = Confucius4TtsCrispAsr.ResolveModelKey(model);
            if (!Confucius4TtsCrispAsr.AreModelsInstalled(confuciusModelKey))
            {
                // Model key already carries the total download size, so no separate size here.
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelsQuestion, "Confucius4-TTS (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelsDownload, "Confucius4-TTS (CrispASR)", confuciusModelKey),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadConfucius4TtsCrispAsrModels(confuciusModelKey));
                if (!dlResult.OkPressed || !Confucius4TtsCrispAsr.AreModelsInstalled(confuciusModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is DotsTtsCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForDotsTts(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var dotsModelKey = DotsTtsCrispAsr.ResolveModelKey(model);
            if (!DotsTtsCrispAsr.AreModelsInstalled(dotsModelKey))
            {
                // Model key already carries the total download size, so no separate size here.
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelsQuestion, "dots.tts (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelsDownload, "dots.tts (CrispASR)", dotsModelKey),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadDotsTtsCrispAsrModels(dotsModelKey));
                if (!dlResult.OkPressed || !DotsTtsCrispAsr.AreModelsInstalled(dotsModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is IndexTts25AudioCpp)
        {
            // The model licence gate comes before anything is fetched: the weights are under
            // the bilibili Model Use License (not open source), so the user has to accept it
            // once before the first 3.3 GB download rather than after.
            if (!IndexTts25AudioCpp.IsLicenseAccepted())
            {
                var licenseResult = await windowService.ShowDialogAsync<IndexTts25LicenseWindow, IndexTts25LicenseViewModel>(window, _ => { });
                if (!licenseResult.OkPressed || !IndexTts25AudioCpp.IsLicenseAccepted())
                {
                    await MessageBox.Show(
                        window,
                        "IndexTTS 2.5",
                        $"{Environment.NewLine}{Se.Language.Video.IndexTts25LicenseDeclined}",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return false;
                }
            }

            if (!await TtsVoiceInstaller.EnsureAudioCppRuntime(window, windowService, forceRedownload: false, "IndexTTS 2.5", IndexTts25AudioCpp.FamilyName))
            {
                return false;
            }

            var indexTts25ModelKey = IndexTts25AudioCpp.ResolveModelKey(model);
            if (!IndexTts25AudioCpp.AreModelsInstalled(indexTts25ModelKey))
            {
                // Model key already carries its size (e.g. "Q8_0 (~3.3 GB)").
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelQuestion, "IndexTTS 2.5"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelDownload, "IndexTTS 2.5 (audio.cpp)", indexTts25ModelKey),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadIndexTts25AudioCppModels(indexTts25ModelKey));
                if (!dlResult.OkPressed || !IndexTts25AudioCpp.AreModelsInstalled(indexTts25ModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is HiggsTtsAudioCpp)
        {
            return await EnsureAudioCppEngineWithLicense(
                window, windowService, refreshVoices,
                engineDisplayName: "Higgs Audio v3",
                requiredFamily: HiggsTtsAudioCpp.FamilyName,
                licenseDefinition: HiggsTtsAudioCpp.LicenseDefinition,
                isLicenseAccepted: HiggsTtsAudioCpp.IsLicenseAccepted,
                modelKey: HiggsTtsAudioCpp.ResolveModelKey(model),
                areModelsInstalled: HiggsTtsAudioCpp.AreModelsInstalled,
                startDownloadModels: (vm, key) => vm.StartDownloadHiggsTtsAudioCppModels(key));
        }

        if (engine is FishTtsAudioCpp)
        {
            return await EnsureAudioCppEngineWithLicense(
                window, windowService, refreshVoices,
                engineDisplayName: "Fish Audio S2 Pro",
                requiredFamily: FishTtsAudioCpp.FamilyName,
                licenseDefinition: FishTtsAudioCpp.LicenseDefinition,
                isLicenseAccepted: FishTtsAudioCpp.IsLicenseAccepted,
                modelKey: FishTtsAudioCpp.ResolveModelKey(model),
                areModelsInstalled: FishTtsAudioCpp.AreModelsInstalled,
                startDownloadModels: (vm, key) => vm.StartDownloadFishTtsAudioCppModels(key));
        }

        if (engine is BreezeTtsAudioCpp)
        {
            return await EnsureAudioCppEngineWithLicense(
                window, windowService, refreshVoices,
                engineDisplayName: "Breeze TTS 2",
                requiredFamily: BreezeTtsAudioCpp.FamilyName,
                licenseDefinition: BreezeTtsAudioCpp.LicenseDefinition,
                isLicenseAccepted: BreezeTtsAudioCpp.IsLicenseAccepted,
                modelKey: BreezeTtsAudioCpp.ResolveModelKey(model),
                areModelsInstalled: BreezeTtsAudioCpp.AreModelsInstalled,
                startDownloadModels: (vm, key) => vm.StartDownloadBreezeTtsAudioCppModels(key));
        }

        if (engine is FireRedTts3AudioCpp)
        {
            // Apache-2.0 weights: no licence gate, otherwise the same runtime + model flow.
            return await EnsureAudioCppEngineWithLicense(
                window, windowService, refreshVoices,
                engineDisplayName: "FireRedTTS3",
                requiredFamily: FireRedTts3AudioCpp.FamilyName,
                licenseDefinition: null,
                isLicenseAccepted: () => true,
                modelKey: FireRedTts3AudioCpp.ResolveModelKey(model),
                areModelsInstalled: FireRedTts3AudioCpp.AreModelsInstalled,
                startDownloadModels: (vm, key) => vm.StartDownloadFireRedTts3AudioCppModels(key));
        }

        if (engine is KugelAudioAudioCpp)
        {
            // MIT weights: no licence gate, otherwise the same runtime + model flow.
            return await EnsureAudioCppEngineWithLicense(
                window, windowService, refreshVoices,
                engineDisplayName: "KugelAudio",
                requiredFamily: KugelAudioAudioCpp.FamilyName,
                licenseDefinition: null,
                isLicenseAccepted: () => true,
                modelKey: KugelAudioAudioCpp.ResolveModelKey(model),
                areModelsInstalled: KugelAudioAudioCpp.AreModelsInstalled,
                startDownloadModels: (vm, key) => vm.StartDownloadKugelAudioAudioCppModels(key));
        }

        if (engine is VoxtralTtsAudioCpp)
        {
            // CC BY-NC 4.0 weights and voices: licence gate first, like Fish and Breeze.
            return await EnsureAudioCppEngineWithLicense(
                window, windowService, refreshVoices,
                engineDisplayName: "Voxtral TTS",
                requiredFamily: VoxtralTtsAudioCpp.FamilyName,
                licenseDefinition: VoxtralTtsAudioCpp.LicenseDefinition,
                isLicenseAccepted: VoxtralTtsAudioCpp.IsLicenseAccepted,
                modelKey: VoxtralTtsAudioCpp.ResolveModelKey(model),
                areModelsInstalled: VoxtralTtsAudioCpp.AreModelsInstalled,
                startDownloadModels: (vm, key) => vm.StartDownloadVoxtralTtsAudioCppModels(key));
        }

        if (engine is ZonosTtsCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForZonos(window, windowService, forceRedownload: false))
            {
                return false;
            }

            if (!ZonosTtsCrispAsr.AreModelsInstalled())
            {
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelsQuestion, "Zonos TTS (CrispASR)"),
                    Environment.NewLine + Se.Language.Video.TextToSpeech.ZonosRequiresModels,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadZonosTtsCrispAsrModels());
                if (!dlResult.OkPressed || !ZonosTtsCrispAsr.AreModelsInstalled())
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is CosyVoice3CrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForCosyVoice3(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var cosyModelKey = CosyVoice3CrispAsr.ResolveModelKey(model);
            if (!CosyVoice3CrispAsr.AreModelsInstalled(cosyModelKey))
            {
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelsQuestion, "CosyVoice3 (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.CosyVoiceXRequiresModels, cosyModelKey),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadCosyVoice3CrispAsrModels(cosyModelKey));
                if (!dlResult.OkPressed || !CosyVoice3CrispAsr.AreModelsInstalled(cosyModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is F5TtsCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForF5Tts(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var f5ModelKey = F5TtsCrispAsr.ResolveModelKey(model);
            if (!F5TtsCrispAsr.AreModelsInstalled(f5ModelKey))
            {
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelQuestion, "F5-TTS (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelDownload, "F5-TTS (CrispASR)", f5ModelKey),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadF5TtsCrispAsrModels(f5ModelKey));
                if (!dlResult.OkPressed || !F5TtsCrispAsr.AreModelsInstalled(f5ModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is VoxCPM2CrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForVoxCPM2(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var voxModelKey = VoxCPM2CrispAsr.ResolveModelKey(model);
            if (!VoxCPM2CrispAsr.AreModelsInstalled(voxModelKey))
            {
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelQuestion, "VoxCPM2 (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelDownload, "VoxCPM2 (CrispASR)", voxModelKey),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadVoxCPM2CrispAsrModels(voxModelKey));
                if (!dlResult.OkPressed || !VoxCPM2CrispAsr.AreModelsInstalled(voxModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is OmniVoiceCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForOmniVoice(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var omniCrispModelKey = OmniVoiceCrispAsr.ResolveModelKey(model);
            if (!OmniVoiceCrispAsr.AreModelsInstalled(omniCrispModelKey))
            {
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelQuestion, "OmniVoice (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelDownload, "OmniVoice TTS (CrispASR)", omniCrispModelKey),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadOmniVoiceCrispAsrModels(omniCrispModelKey));
                if (!dlResult.OkPressed || !OmniVoiceCrispAsr.AreModelsInstalled(omniCrispModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is MossTtsCrispAsr)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForMossTts(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var mossModelKey = MossTtsCrispAsr.ResolveModelKey(model);
            if (!MossTtsCrispAsr.AreModelsInstalled(mossModelKey))
            {
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelQuestion, "MOSS-TTS (CrispASR)"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelDownload, "MOSS-TTS (CrispASR)", mossModelKey),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadMossTtsCrispAsrModels(mossModelKey));
                if (!dlResult.OkPressed || !MossTtsCrispAsr.AreModelsInstalled(mossModelKey))
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
                return true;
            }

            return true;
        }

        if (engine is KokoroTtsCpp)
        {
            if (!await engine.IsInstalled(region))
            {
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.General.DownloadXQuestion, "Kokoro TTS"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.RequiresXDownloadAndUse, "Kokoro TTS"),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadKokoroTtsCpp());
                if (!dlResult.OkPressed)
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
            }

            if (!KokoroTtsCpp.AreModelsInstalled())
            {
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelsQuestion, "Kokoro TTS"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XRequiresModelsSizeYDownload, "Kokoro TTS", "~380 MB"),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadKokoroTtsModels());
                return dlResult.OkPressed && KokoroTtsCpp.AreModelsInstalled();
            }

            return true;
        }

        if (engine is ChatterboxTtsCpp)
        {
            if (!await TtsVoiceInstaller.EnsureCrispAsrForChatterbox(window, windowService, forceRedownload: false))
            {
                return false;
            }

            var chatterboxModelKey = ChatterboxTtsCpp.ResolveModelKey(model);
            if (!ChatterboxTtsCpp.AreModelsInstalled(chatterboxModelKey))
            {
                var sizeText = GetModelDownloadSizeText(engine, model);
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelsQuestion, "Chatterbox TTS"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelsSizeZDownload, "Chatterbox TTS", chatterboxModelKey, sizeText),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadChatterboxModels(chatterboxModelKey));
                return dlResult.OkPressed && ChatterboxTtsCpp.AreModelsInstalled(chatterboxModelKey);
            }

            return true;
        }

        if (engine is OmniVoiceTtsCpp)
        {
            if (!await engine.IsInstalled(region))
            {
                var omniVariant = OmniVoiceDownloadService.WindowsVariantVulkan;
                if (Configuration.IsRunningOnWindows)
                {
                    var variantAnswer = await MessageBox.Show(
                        window,
                        string.Format(Se.Language.General.DownloadXQuestion, "OmniVoice TTS"),
                        Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.RequiresXSelectBuild, "OmniVoice TTS"),
                        MessageBoxButtons.Cancel,
                        MessageBoxIcon.Question,
                        "CPU",
                        "Vulkan",
                        "CUDA");

                    if (variantAnswer == MessageBoxResult.None || variantAnswer == MessageBoxResult.Cancel)
                    {
                        return false;
                    }

                    omniVariant = variantAnswer switch
                    {
                        MessageBoxResult.Custom1 => OmniVoiceDownloadService.WindowsVariantCpu,
                        MessageBoxResult.Custom3 => OmniVoiceDownloadService.WindowsVariantCuda,
                        _ => OmniVoiceDownloadService.WindowsVariantVulkan,
                    };

                    if (omniVariant == OmniVoiceDownloadService.WindowsVariantVulkan && !VulkanHelper.IsInstalled())
                    {
                        var vulkanAnswer = await MessageBox.Show(
                            window,
                            Se.Language.General.VulkanRuntimeMayBeRequired,
                            Se.Language.General.VulkanRuntimeNotDetectedContinue,
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (vulkanAnswer == MessageBoxResult.No)
                        {
                            UiUtil.OpenUrl("https://vulkan.lunarg.com/sdk/home");
                            return false;
                        }

                        if (vulkanAnswer != MessageBoxResult.Yes)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    var answer = await MessageBox.Show(
                        window,
                        string.Format(Se.Language.General.DownloadXQuestion, "OmniVoice TTS"),
                        Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.RequiresXDownloadAndUse, "OmniVoice TTS"),
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    if (answer != MessageBoxResult.Yes)
                    {
                        return false;
                    }
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadOmniVoice(omniVariant));
                if (!dlResult.OkPressed)
                {
                    return false;
                }

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await refreshVoices();
                });
            }

            if (!OmniVoiceTtsCpp.IsModelsInstalled())
            {
                var answer = await MessageBox.Show(
                    window,
                    string.Format(Se.Language.Video.TextToSpeech.DownloadXModelsQuestion, "OmniVoice TTS"),
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XRequiresModelsSizeYDownload, "OmniVoice TTS", "~1.4 GB"),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (answer != MessageBoxResult.Yes)
                {
                    return false;
                }

                var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadOmniVoiceModels());
                return dlResult.OkPressed && OmniVoiceTtsCpp.IsModelsInstalled();
            }

            return true;
        }

        if (await engine.IsInstalled(region) || window == null)
        {
            return true;
        }

        if (engine is Piper)
        {
            var answer = await MessageBox.Show(
                window,
                string.Format(Se.Language.General.DownloadX, "Piper"),
                Se.Language.Video.TextToSpeech.DownloadPiperPrompt,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            var result = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(window, vm => vm.StartDownloadPiper());
            return await engine.IsInstalled(region);
        }

        if (engine is AllTalk)
        {
            var answer = await MessageBox.Show(
                window,
                Se.Language.General.Error,
                Se.Language.Video.TextToSpeech.AllTalkRequiresServer,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            await window.Launcher.LaunchUriAsync(new Uri("https://github.com/erew123/alltalk_tts"));

            return await engine.IsInstalled(region);
        }

        if (engine is EdgeTts)
        {
            var answer = await MessageBox.Show(
                window,
                Se.Language.General.Error,
                Se.Language.Video.TextToSpeech.EdgeTtsRequiresCli,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            await window.Launcher.LaunchUriAsync(new Uri("https://github.com/rany2/edge-tts"));
            return await engine.IsInstalled(region);
        }

        if (engine.HasKeyFile)
        {
            if (string.IsNullOrEmpty(keyFile) || !File.Exists(keyFile))
            {
                await MessageBox.Show(
                window,
                Se.Language.General.Error,
                string.Format(Se.Language.Video.TextToSpeech.XRequiresKeyFile, engine.Name),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

                return false;
            }

            return true;
        }

        if (engine.HasApiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                await MessageBox.Show(
                window,
                Se.Language.General.Error,
                string.Format(Se.Language.Video.TextToSpeech.XRequiresApiKey, engine.Name),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

                return false;
            }

            return true;
        }

        return false;
    }
    public static string GetModelDownloadSizeText(ITtsEngine? engine, string? modelKey)
    {
        if (engine == null || string.IsNullOrEmpty(modelKey))
        {
            return string.Empty;
        }

        return engine switch
        {
            Qwen3TtsCpp => Qwen3TtsCpp.ResolveModelKey(modelKey) switch
            {
                Qwen3TtsCpp.ModelKey17BBase => "~2.7 GB",
                Qwen3TtsCpp.ModelKey17BVoiceDesign => "~2.8 GB",
                _ => "~1.6 GB",
            },
            // Both keys ship the same ~358 MB 12 Hz codec; the talker is ~2 GB regardless.
            Qwen3TtsCrispAsr => "~2.4 GB",
            ChatterboxTtsCpp => ChatterboxTtsCpp.ResolveModelKey(modelKey) == ChatterboxTtsCpp.ModelKeyTurbo
                ? "~1 GB"
                : "~990 MB",
            _ => string.Empty,
        };
    }

    /// <summary>
    /// The shared install flow for the audio.cpp engines whose weights carry their own licence
    /// (Higgs Audio v3, Fish Audio S2 Pro): licence gate first — nothing is fetched before the
    /// user accepts, the weights are not open source — then the shared audio.cpp runtime, then
    /// the engine's own GGUF. Same shape as the IndexTts25AudioCpp branch above, which predates
    /// this helper and keeps its own engine-specific licence window.
    /// </summary>
    private static async Task<bool> EnsureAudioCppEngineWithLicense(
        Window window,
        IWindowService windowService,
        Func<Task> refreshVoices,
        string engineDisplayName,
        string requiredFamily,
        ModelLicenseDefinition? licenseDefinition,
        Func<bool> isLicenseAccepted,
        string modelKey,
        Func<string?, bool> areModelsInstalled,
        Action<DownloadTtsViewModel, string> startDownloadModels)
    {
        // A null definition means the weights need no acceptance (Apache-2.0 FireRedTTS3).
        if (licenseDefinition != null && !isLicenseAccepted())
        {
            var licenseResult = await windowService.ShowDialogAsync<ModelLicenseWindow, ModelLicenseViewModel>(
                window, vm => vm.Initialize(licenseDefinition));
            if (!licenseResult.OkPressed || !isLicenseAccepted())
            {
                await MessageBox.Show(
                    window,
                    engineDisplayName,
                    Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XNeedsModelLicense, engineDisplayName),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return false;
            }
        }

        if (!await TtsVoiceInstaller.EnsureAudioCppRuntime(window, windowService, forceRedownload: false, engineDisplayName, requiredFamily))
        {
            return false;
        }

        if (!areModelsInstalled(modelKey))
        {
            // Model key already carries its size (e.g. "Q8_0 (~4.7 GB)").
            var answer = await MessageBox.Show(
                window,
                string.Format(Se.Language.Video.TextToSpeech.DownloadXModelQuestion, engineDisplayName),
                Environment.NewLine + string.Format(Se.Language.Video.TextToSpeech.XYRequiresModelDownload, engineDisplayName + " (audio.cpp)", modelKey),
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return false;
            }

            var dlResult = await windowService.ShowDialogAsync<DownloadTtsWindow, DownloadTtsViewModel>(
                window, vm => startDownloadModels(vm, modelKey));
            if (!dlResult.OkPressed || !areModelsInstalled(modelKey))
            {
                return false;
            }

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await refreshVoices();
            });
        }

        return true;
    }

    private static string GetSavedKeyFile(ITtsEngine engine) => engine switch
    {
        GoogleSpeech => Se.Settings.Video.TextToSpeech.GoogleKeyFile,
        _ => string.Empty,
    };

    private static string GetSavedApiKey(ITtsEngine engine) => engine switch
    {
        AzureSpeech => Se.Settings.Video.TextToSpeech.AzureApiKey,
        ElevenLabs => Se.Settings.Video.TextToSpeech.ElevenLabsApiKey,
        Murf => Se.Settings.Video.TextToSpeech.MurfApiKey,
        MistralSpeech => Se.Settings.Providers.MistralApiKey,
        OpenAiCompatibleSpeech => OpenAiCompatibleSpeech.GetApiKey(OpenAiCompatibleSpeech.SavedProvider),
        _ => string.Empty,
    };
}
