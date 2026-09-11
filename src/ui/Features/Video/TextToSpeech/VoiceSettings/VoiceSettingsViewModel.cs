using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Shared.PromptTextBox;
using Nikse.SubtitleEdit.Features.Video.SpeechToText;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceCloneConsent;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceSettings;

public partial class VoiceSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _voiceTestText;
    [ObservableProperty] private bool _isImportVoiceVisible;
    [ObservableProperty] private bool _isDragOver;

    private static readonly string[] SupportedAudioExtensions = { ".wav", ".mp3" };

    private ITtsEngine? _engine;
    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public bool RefreshVoices { get; private set; }

    public VoiceSettingsViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        VoiceTestText = Se.Settings.Video.TextToSpeech.VoiceTestText;
        _fileHelper = fileHelper;
        _windowService = windowService;
    }

    [RelayCommand]
    private void Ok()
    {
        Se.Settings.Video.TextToSpeech.VoiceTestText = VoiceTestText;
        Se.SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private async Task ImportVoice()
    {
        if (Window == null || _engine == null)
        {
            return;
        }

        // Ask before the file picker rather than after, so a user who declines is not first made
        // to hunt for a recording. The drag-drop path has no such "before", which is why
        // ImportVoiceFromFileAsync checks again - both funnel through there.
        if (!await EnsureVoiceCloningConsentAsync())
        {
            return;
        }

        string fileName;
        if (_engine is Piper)
        {
            // Piper voices are trained models (.onnx + .onnx.json), not audio to clone from.
            fileName = await _fileHelper.PickOpenFile(Window!, Se.Language.Video.TextToSpeech.ImportPiperVoiceTitle, "Piper voice model", "*.onnx");
        }
        else
        {
            fileName = await _fileHelper.PickOpenFile(Window!, "Open audio file (for clone)", Se.Language.General.AudioFiles, "*.wav;*.mp3");
        }

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        await ImportVoiceFromFileAsync(fileName);
    }

    private async Task ImportVoiceFromFileAsync(string fileName)
    {
        if (Window == null || _engine == null)
        {
            return;
        }

        // The choke point every import passes through - the button above and the drop handler
        // below. Gating only the button would leave drag-drop cloning unasked.
        if (!await EnsureVoiceCloningConsentAsync())
        {
            return;
        }

        bool ok;
        if (_engine is OmniVoiceTtsCpp omniEngine)
        {
            // OmniVoice voice cloning needs both the WAV and a transcript of what is spoken in
            // it. Read it from a sibling .txt next to the source if present, otherwise prompt
            // the user — without a transcript the engine silently falls back to its default
            // voice, which surfaces as "custom voice does not work".
            var transcript = TryReadSiblingTranscript(fileName);
            if (string.IsNullOrWhiteSpace(transcript))
            {
                var audioFileName = fileName;
                var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
                {
                    vm.Initialize(
                        Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                        string.Empty,
                        500,
                        150);
                    vm.ConfigureExtraButton(
                        Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot,
                        () => RunSpeechToTextAsync(audioFileName));
                });

                if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
                {
                    return;
                }

                transcript = result.Text.Trim();
            }

            ok = omniEngine.ImportVoice(fileName, transcript);
        }
        else if (_engine is CosyVoice3CrispAsr cosyEngine)
        {
            // CosyVoice3's s3tok speech tokenizer REQUIRES a transcript at synth time — without
            // it the server fails outright. Always prompt (matching OmniVoice's flow), with the
            // sibling-text autofill so existing .txt sidecars don't force a re-type.
            var transcript = TryReadSiblingTranscript(fileName) ?? string.Empty;
            var audioFileName = fileName;
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
            {
                vm.Initialize(
                    Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                    transcript,
                    500,
                    150);
                vm.ConfigureExtraButton(
                    Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot,
                    () => RunSpeechToTextAsync(audioFileName));
            });

            if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
            {
                return;
            }

            ok = cosyEngine.ImportVoice(fileName, result.Text.Trim());
        }
        else if (_engine is F5TtsCrispAsr f5Engine)
        {
            // F5-TTS cloning quality depends sharply on an accurate transcription. Not strictly
            // required by the server, but synthesis with no ref-text falls back to a generic
            // voice, so prompt the same way as CosyVoice3.
            var transcript = TryReadSiblingTranscript(fileName) ?? string.Empty;
            var audioFileName = fileName;
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
            {
                vm.Initialize(
                    Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                    transcript,
                    500,
                    150);
                vm.ConfigureExtraButton(
                    Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot,
                    () => RunSpeechToTextAsync(audioFileName));
            });

            if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
            {
                return;
            }

            ok = f5Engine.ImportVoice(fileName, result.Text.Trim());
        }
        else if (_engine is Qwen3TtsCrispAsr qwenCrispEngine)
        {
            // The Qwen3 (CrispASR) "Voice clone" (Base) model needs the spoken transcription of
            // the reference WAV — the backend loads it as ref-text and errors without it. Prefer a
            // sibling .txt; otherwise auto-transcribe with Whisper so the user rarely has to type
            // it. Either way show the result for a quick review/correction (clone quality is
            // sensitive to ref-text accuracy), keeping the STT button to re-run if needed.
            var transcript = TryReadSiblingTranscript(fileName);
            if (Qwen3TtsCrispAsr.LooksLikeUnusableTranscript(transcript))
            {
                transcript = await RunSpeechToTextAsync(fileName) ?? string.Empty;
            }

            var audioFileName = fileName;
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
            {
                vm.Initialize(
                    Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                    transcript ?? string.Empty,
                    500,
                    150);
                vm.ConfigureExtraButton(
                    Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot,
                    () => RunSpeechToTextAsync(audioFileName));
            });

            if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
            {
                return;
            }

            ok = qwenCrispEngine.ImportVoice(fileName, result.Text.Trim());
        }
        else if (_engine is VoxCPM2CrispAsr voxEngine)
        {
            // VoxCPM2 clones zero-shot from audio alone; a transcription (ref-text) is optional
            // but improves quality, so prompt like F5-TTS while allowing an empty answer.
            var transcript = TryReadSiblingTranscript(fileName) ?? string.Empty;
            var audioFileName = fileName;
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
            {
                vm.Initialize(
                    Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                    transcript,
                    500,
                    150);
                vm.ConfigureExtraButton(
                    Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot,
                    () => RunSpeechToTextAsync(audioFileName));
            });

            ok = result.OkPressed
                ? voxEngine.ImportVoice(fileName, (result.Text ?? string.Empty).Trim())
                : voxEngine.ImportVoice(fileName);
        }
        else if (_engine is OmniVoiceCrispAsr omniCrispEngine)
        {
            // OmniVoice clones zero-shot from audio alone; a transcription (ref-text) is optional
            // but improves quality, so prompt like VoxCPM2 while allowing an empty answer.
            var transcript = TryReadSiblingTranscript(fileName) ?? string.Empty;
            var audioFileName = fileName;
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
            {
                vm.Initialize(
                    Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                    transcript,
                    500,
                    150);
                vm.ConfigureExtraButton(
                    Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot,
                    () => RunSpeechToTextAsync(audioFileName));
            });

            ok = result.OkPressed
                ? omniCrispEngine.ImportVoice(fileName, (result.Text ?? string.Empty).Trim())
                : omniCrispEngine.ImportVoice(fileName);
        }
        else if (_engine is FishTtsAudioCpp fishEngine)
        {
            // Fish Audio S2 Pro REQUIRES the transcript at synth time — audio.cpp answers a
            // voice_ref without reference_text with a server error. Always prompt (matching
            // CosyVoice3's flow), with the sibling-text autofill so existing .txt sidecars
            // don't force a re-type.
            var transcript = TryReadSiblingTranscript(fileName) ?? string.Empty;
            var audioFileName = fileName;
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
            {
                vm.Initialize(
                    Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                    transcript,
                    500,
                    150);
                vm.ConfigureExtraButton(
                    Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot,
                    () => RunSpeechToTextAsync(audioFileName));
            });

            if (!result.OkPressed || string.IsNullOrWhiteSpace(result.Text))
            {
                return;
            }

            ok = fishEngine.ImportVoice(fileName, result.Text.Trim());
        }
        else if (_engine is HiggsTtsAudioCpp higgsEngine)
        {
            // Higgs clones zero-shot from audio alone; a transcription (reference_text) is
            // optional but improves quality, so prompt like VoxCPM2 while allowing an empty
            // answer.
            var transcript = TryReadSiblingTranscript(fileName) ?? string.Empty;
            var audioFileName = fileName;
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
            {
                vm.Initialize(
                    Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                    transcript,
                    500,
                    150);
                vm.ConfigureExtraButton(
                    Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot,
                    () => RunSpeechToTextAsync(audioFileName));
            });

            ok = result.OkPressed
                ? higgsEngine.ImportVoice(fileName, (result.Text ?? string.Empty).Trim())
                : higgsEngine.ImportVoice(fileName);
        }
        else if (_engine is FireRedTts3AudioCpp fireRedEngine)
        {
            // FireRedTTS3 clones zero-shot from audio alone; a transcription (reference_text) is
            // optional but improves quality, so prompt like VoxCPM2 while allowing an empty
            // answer.
            var transcript = TryReadSiblingTranscript(fileName) ?? string.Empty;
            var audioFileName = fileName;
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
            {
                vm.Initialize(
                    Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                    transcript,
                    500,
                    150);
                vm.ConfigureExtraButton(
                    Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot,
                    () => RunSpeechToTextAsync(audioFileName));
            });

            ok = result.OkPressed
                ? fireRedEngine.ImportVoice(fileName, (result.Text ?? string.Empty).Trim())
                : fireRedEngine.ImportVoice(fileName);
        }
        else if (_engine is MossTtsCrispAsr mossEngine)
        {
            // MOSS-TTS clones zero-shot from audio alone; a transcription (ref-text) is optional
            // but improves quality, so prompt like VoxCPM2 while allowing an empty answer.
            var transcript = TryReadSiblingTranscript(fileName) ?? string.Empty;
            var audioFileName = fileName;
            var result = await _windowService.ShowDialogAsync<PromptTextBoxWindow, PromptTextBoxViewModel>(Window!, vm =>
            {
                vm.Initialize(
                    Se.Language.Video.TextToSpeech.VoiceCloneTranscriptTitle,
                    transcript,
                    500,
                    150);
                vm.ConfigureExtraButton(
                    Se.Language.Video.TextToSpeech.UseSpeechToTextDotDotDot,
                    () => RunSpeechToTextAsync(audioFileName));
            });

            ok = result.OkPressed
                ? mossEngine.ImportVoice(fileName, (result.Text ?? string.Empty).Trim())
                : mossEngine.ImportVoice(fileName);
        }
        else if (_engine is Piper piperEngine)
        {
            ok = piperEngine.ImportVoice(fileName);
            if (!ok)
            {
                // The only user-fixable failure: the .onnx.json config is not next to the model.
                await MessageBox.Show(
                    Window,
                    Se.Language.Video.TextToSpeech.PiperVoiceConfigMissingTitle,
                    string.Format(Se.Language.Video.TextToSpeech.PiperVoiceConfigMissingMessage, Path.GetFileName(fileName) + ".json"));
                return;
            }
        }
        else
        {
            ok = _engine.ImportVoice(fileName);
        }

        var importedFileName = Path.GetFileName(fileName);
        if (!ok)
        {
            // Without this the dialog just closed with nothing to show for it, which reads as
            // "imported" - and users then copy the file into the voices folder by hand instead,
            // bypassing the conversion every cloning engine does on import (#13508).
            await MessageBox.Show(
                Window,
                Se.Language.General.Error,
                string.Format(Se.Language.Video.TextToSpeech.VoiceXCouldNotBeImported, importedFileName));
            return;
        }

        await MessageBox.Show(Window, Se.Language.Video.TextToSpeech.VoiceImportSuccessTitle, string.Format(Se.Language.Video.TextToSpeech.VoiceXImported, importedFileName));
        RefreshVoices = true;
    }

    /// <summary>
    /// Shows the first-clone consent dialog when it is still owed, and reports whether cloning may
    /// go ahead. A no-op once accepted, and for Piper, whose import is a trained model rather than
    /// somebody's voice.
    /// </summary>
    private Task<bool> EnsureVoiceCloningConsentAsync() =>
        VoiceCloneConsentPrompt.EnsureAsync(
            _engine,
            Window!,
            () => _windowService.ShowDialogAsync<VoiceCloneConsentWindow, VoiceCloneConsentViewModel>(Window!, _ => { }));

    internal void OnDragOver(object? sender, DragEventArgs e)
    {
        if (!IsImportVoiceVisible)
        {
            e.DragEffects = DragDropEffects.None;
            IsDragOver = false;
            e.Handled = true;
            return;
        }

        if (e.DataTransfer.Contains(DataFormat.File) && HasSupportedImportFile(e))
        {
            e.DragEffects = DragDropEffects.Copy;
            IsDragOver = true;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
            IsDragOver = false;
        }

        e.Handled = true;
    }

    internal void OnDragLeave(object? sender, DragEventArgs e)
    {
        IsDragOver = false;
    }

    internal void OnDrop(object? sender, DragEventArgs e)
    {
        IsDragOver = false;

        if (!IsImportVoiceVisible || !e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        var fileName = e.DataTransfer.TryGetFiles()?
            .Select(f => f.Path.LocalPath)
            .FirstOrDefault(IsSupportedImportFile);

        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        Dispatcher.UIThread.PostSafe(() => ImportVoiceFromFileAsync(fileName));
    }

    private bool HasSupportedImportFile(DragEventArgs e)
    {
        var files = e.DataTransfer.TryGetFiles();
        return files != null && files.Any(f => IsSupportedImportFile(f.Path.LocalPath));
    }

    private bool IsSupportedImportFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        var ext = Path.GetExtension(fileName);

        // Piper imports voice models, not audio to clone from.
        if (_engine is Piper)
        {
            return string.Equals(ext, ".onnx", StringComparison.OrdinalIgnoreCase);
        }

        return SupportedAudioExtensions.Any(s => string.Equals(s, ext, StringComparison.OrdinalIgnoreCase));
    }

    private static string? TryReadSiblingTranscript(string audioFileName)
    {
        var siblingTextFile = Path.ChangeExtension(audioFileName, ".txt");
        if (!File.Exists(siblingTextFile))
        {
            return null;
        }

        try
        {
            // Voice-pack sidecars can be Wikimedia attribution blurbs, not spoken
            // transcriptions - pre-filling the transcript prompt with one poisons the
            // ref-text when the user just clicks OK. Treat a blurb as "no transcript".
            var text = File.ReadAllText(siblingTextFile);
            return Qwen3TtsCrispAsr.LooksLikeUnusableTranscript(text) ? null : text;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> RunSpeechToTextAsync(string audioFileName)
    {
        if (Window == null)
        {
            return null;
        }

        var sttResult = await _windowService.ShowDialogAsync<SpeechToTextWindow, SpeechToTextViewModel>(Window, vm =>
        {
            vm.Initialize(audioFileName, -1);
        });

        if (!sttResult.OkPressed || sttResult.TranscribedSubtitle == null || sttResult.TranscribedSubtitle.Paragraphs.Count == 0)
        {
            return null;
        }

        return string.Join(' ', sttResult.TranscribedSubtitle.Paragraphs
            .Select(p => p.Text?.Replace('\n', ' ').Replace('\r', ' ').Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t)));
    }

    [RelayCommand]
    private void RefreshVoiceList()
    {
        Se.Settings.Video.TextToSpeech.VoiceTestText = VoiceTestText;
        Se.SaveSettings();
        RefreshVoices = true;
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(ITtsEngine engine)
    {
        _engine = engine;

        // Every cloning engine imports a reference recording; Piper is the one engine that
        // imports something else (a trained .onnx voice model), so it is named on its own
        // rather than counted as cloning.
        IsImportVoiceVisible = engine.SupportsVoiceCloning || engine is Piper;
    }
}