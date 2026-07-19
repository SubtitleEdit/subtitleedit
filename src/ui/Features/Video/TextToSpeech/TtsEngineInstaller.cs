using Avalonia.Controls;
using Avalonia.Threading;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.DownloadTts;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
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
                        "Download Qwen3 TTS?",
                        $"{Environment.NewLine}\"Text to speech\" requires Qwen3 TTS.{Environment.NewLine}{Environment.NewLine}Select a build to download:",
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
                            "Vulkan runtime may be required",
                            $"The Vulkan version requires the Vulkan runtime (vulkan-1.dll) which usually ships with current GPU drivers, but was not detected on this system.{Environment.NewLine}{Environment.NewLine}You can install it from:{Environment.NewLine}https://vulkan.lunarg.com/sdk/home{Environment.NewLine}{Environment.NewLine}Continue with Vulkan download anyway?",
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
                        "Download Qwen3 TTS?",
                        $"{Environment.NewLine}\"Text to speech\" requires Qwen3 TTS.{Environment.NewLine}{Environment.NewLine}Download and use Qwen3 TTS?",
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
                    "Download Qwen3 TTS models?",
                    $"{Environment.NewLine}\"Qwen3 TTS\" ({qwen3ModelKey}) requires models ({sizeText}).{Environment.NewLine}{Environment.NewLine}Download models?",
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
                    "Download Qwen3 TTS (CrispASR) models?",
                    $"{Environment.NewLine}\"Qwen3 TTS (CrispASR)\" ({crispAsrModelKey}) requires models ({sizeText}).{Environment.NewLine}{Environment.NewLine}Download models?",
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
                    "Download VibeVoice (CrispASR) model?",
                    $"{Environment.NewLine}\"VibeVoice (CrispASR)\" ({vibeModelKey}) requires a model.{Environment.NewLine}{Environment.NewLine}Download model?",
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
                    "Download IndexTTS (CrispASR) models?",
                    $"{Environment.NewLine}\"IndexTTS (CrispASR)\" ({indexModelKey}) requires models.{Environment.NewLine}{Environment.NewLine}Download models?",
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
                    "Download Zonos TTS (CrispASR) models?",
                    $"{Environment.NewLine}\"Zonos TTS (CrispASR)\" requires the Zonos transformer + DAC codec (~1.8 GB).{Environment.NewLine}{Environment.NewLine}Download models?",
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
                    "Download CosyVoice3 (CrispASR) models?",
                    $"{Environment.NewLine}\"CosyVoice3 (CrispASR)\" ({cosyModelKey}) requires LLM + flow + hift + s3tok + campplus + voice-bank GGUFs (all sized into the total above).{Environment.NewLine}{Environment.NewLine}Download models?",
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
                    "Download F5-TTS (CrispASR) model?",
                    $"{Environment.NewLine}\"F5-TTS (CrispASR)\" ({f5ModelKey}) requires a model.{Environment.NewLine}{Environment.NewLine}Download model?",
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
                    "Download VoxCPM2 (CrispASR) model?",
                    $"{Environment.NewLine}\"VoxCPM2 (CrispASR)\" ({voxModelKey}) requires a model.{Environment.NewLine}{Environment.NewLine}Download model?",
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
                    "Download MOSS-TTS (CrispASR) model?",
                    $"{Environment.NewLine}\"MOSS-TTS (CrispASR)\" ({mossModelKey}) requires a model.{Environment.NewLine}{Environment.NewLine}Download model?",
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
                    "Download Kokoro TTS?",
                    $"{Environment.NewLine}\"Text to speech\" requires Kokoro TTS.{Environment.NewLine}{Environment.NewLine}Download and use Kokoro TTS?",
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
                    "Download Kokoro TTS models?",
                    $"{Environment.NewLine}\"Kokoro TTS\" requires models (~380 MB).{Environment.NewLine}{Environment.NewLine}Download models?",
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
                    "Download Chatterbox TTS models?",
                    $"{Environment.NewLine}\"Chatterbox TTS\" ({chatterboxModelKey}) requires models ({sizeText}).{Environment.NewLine}{Environment.NewLine}Download models?",
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
                        "Download OmniVoice TTS?",
                        $"{Environment.NewLine}\"Text to speech\" requires OmniVoice TTS.{Environment.NewLine}{Environment.NewLine}Select a build to download:",
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
                            "Vulkan runtime may be required",
                            $"The Vulkan version requires the Vulkan runtime (vulkan-1.dll) which usually ships with current GPU drivers, but was not detected on this system.{Environment.NewLine}{Environment.NewLine}You can install it from:{Environment.NewLine}https://vulkan.lunarg.com/sdk/home{Environment.NewLine}{Environment.NewLine}Continue with Vulkan download anyway?",
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
                        "Download OmniVoice TTS?",
                        $"{Environment.NewLine}\"Text to speech\" requires OmniVoice TTS.{Environment.NewLine}{Environment.NewLine}Download and use OmniVoice TTS?",
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
                    "Download OmniVoice TTS models?",
                    $"{Environment.NewLine}\"OmniVoice TTS\" requires models (~1.4 GB).{Environment.NewLine}{Environment.NewLine}Download models?",
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
                $"\"AllTalk\" text to speech requires a running local AllTalk web server.{Environment.NewLine}{Environment.NewLine}Read more?",
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
                $"\"EdgeTts\" text to speech requires the edge-tts CLI tool.{Environment.NewLine}{Environment.NewLine}Install with: pipx install edge-tts{Environment.NewLine}(or pip install edge-tts){Environment.NewLine}{Environment.NewLine}Read more?",
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
                $"\"{engine.Name}\" requires a key file",
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
                $"\"{engine.Name}\" requires an API key",
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
        MistralSpeech => Se.Settings.Video.TextToSpeech.MistralApiKey,
        _ => string.Empty,
    };
}
