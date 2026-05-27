using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.ReviewSpeech;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.VideoPlayers.LibMpvDynamic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ActorVoices;

// Cast dialog VM: lets the user pair each ASSA actor (or WebVTT voice name) with a TTS engine
// and voice, optionally with a free-text voice-design instruction. Voices for an engine are
// fetched once and cached on the VM so flipping multiple rows to the same engine doesn't refetch.
// On OK, exposes the resulting list of ActorVoiceMapping to the caller via Mappings + OkPressed.
public partial class ActorVoiceMappingViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ActorVoiceRow> _rows;
    [ObservableProperty] private ActorVoiceRow? _selectedRow;
    [ObservableProperty] private ObservableCollection<ITtsEngine> _engines;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private bool _isMappingEmpty;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public List<ActorVoiceMapping> Mappings { get; private set; }
    public ActorVoiceDetector.CastKind Kind { get; private set; }

    private readonly IWindowService _windowService;
    private readonly Dictionary<string, Voice[]> _voiceCache;
    private readonly Dictionary<string, string[]> _modelCache;
    private string _waveFolder;
    private LibMpvDynamicPlayer? _mpvContext;
    private readonly Lock _playLock;
    private CancellationTokenSource _cancellationTokenSource;
    private ITtsEngine? _fallbackEngine;
    private Voice? _fallbackVoice;
    private string _voiceTestText;

    public ActorVoiceMappingViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        _voiceCache = new Dictionary<string, Voice[]>(StringComparer.OrdinalIgnoreCase);
        _modelCache = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        _rows = new ObservableCollection<ActorVoiceRow>();
        _engines = new ObservableCollection<ITtsEngine>();
        _statusText = string.Empty;
        _waveFolder = Path.GetTempPath();
        _playLock = new Lock();
        _cancellationTokenSource = new CancellationTokenSource();
        Mappings = new List<ActorVoiceMapping>();
        _voiceTestText = "Hello, how are you doing?";
    }

    public void Initialize(
        IEnumerable<string> actorNames,
        IEnumerable<ITtsEngine> engines,
        IEnumerable<ActorVoiceMapping> existingMappings,
        ITtsEngine? fallbackEngine,
        Voice? fallbackVoice,
        ActorVoiceDetector.CastKind kind,
        string waveFolder)
    {
        Kind = kind;
        _fallbackEngine = fallbackEngine;
        _fallbackVoice = fallbackVoice;
        _waveFolder = string.IsNullOrEmpty(waveFolder) ? Path.GetTempPath() : waveFolder;
        _voiceTestText = string.IsNullOrWhiteSpace(Se.Settings.Video.TextToSpeech.VoiceTestText)
            ? "Hello, how are you doing?"
            : Se.Settings.Video.TextToSpeech.VoiceTestText;

        Engines.Clear();
        foreach (var engine in engines)
        {
            Engines.Add(engine);
        }

        var mappingByActor = (existingMappings ?? Array.Empty<ActorVoiceMapping>())
            .Where(m => !string.IsNullOrWhiteSpace(m.Actor))
            .GroupBy(m => m.Actor.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        Rows.Clear();
        foreach (var actor in actorNames.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var row = new ActorVoiceRow { Actor = actor };
            ITtsEngine? engineForRow = null;
            string voiceName = string.Empty;
            string instruction = string.Empty;

            string modelName = string.Empty;
            if (mappingByActor.TryGetValue(actor, out var saved))
            {
                engineForRow = Engines.FirstOrDefault(e => string.Equals(e.Name, saved.EngineName, StringComparison.OrdinalIgnoreCase));
                voiceName = saved.VoiceName ?? string.Empty;
                modelName = saved.Model ?? string.Empty;
                instruction = saved.Instruction ?? string.Empty;
            }

            engineForRow ??= fallbackEngine ?? Engines.FirstOrDefault();
            row.Instruction = instruction;
            row.HasInstruction = EngineSupportsInstruction(engineForRow);
            row.HasAdvancedSettings = HasAdvancedSettingsForRow(engineForRow, modelName);
            row.HasModel = engineForRow?.HasModel ?? false;
            // Don't pre-seed row.SelectedModel here: when Rows.Add binds the ComboBox, Models is
            // still empty, so Avalonia's SelectingItemsControl resets SelectedItem to null and
            // (TwoWay) writes null back over our seed value. AssignEngineToRowAsync below loads
            // the model list and applies the saved selection *after* items exist.
            row.EngineChangedByUser += (_, newEngine) => _ = AssignEngineToRowAsync(row, newEngine, string.Empty, string.Empty);
            row.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(ActorVoiceRow.SelectedVoice))
                {
                    UpdateStatus();
                }
                else if (args.PropertyName == nameof(ActorVoiceRow.SelectedModel))
                {
                    // Qwen3 only takes voice instructions on the VoiceDesign model. When the user
                    // flips models the row's "..." button has to enable/disable accordingly.
                    row.HasAdvancedSettings = HasAdvancedSettingsForRow(row.SelectedEngine, row.SelectedModel);
                }
            };

            Rows.Add(row);
            _ = AssignEngineToRowAsync(row, engineForRow, voiceName, modelName);
        }

        SelectedRow = Rows.FirstOrDefault();
        UpdateStatus();
    }

    private async Task AssignEngineToRowAsync(ActorVoiceRow row, ITtsEngine? engine, string preferredVoiceName, string preferredModelName)
    {
        if (engine == null)
        {
            row.SelectedEngine = null;
            row.Voices.Clear();
            row.Models.Clear();
            row.SelectedVoice = null;
            row.SelectedModel = null;
            row.HasModel = false;
            row.HasInstruction = false;
            row.HasAdvancedSettings = false;
            return;
        }

        row.IsBusy = true;
        try
        {
            var voices = await GetVoicesAsync(engine);
            var models = engine.HasModel ? await GetModelsAsync(engine) : Array.Empty<string>();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                row.SuppressEngineEvent = true;
                try
                {
                    row.SelectedEngine = engine;
                }
                finally
                {
                    row.SuppressEngineEvent = false;
                }

                // Replace the collections wholesale instead of Clear()+Add. With Clear()+Add the
                // ComboBox sees an empty ItemsSource for one dispatcher tick and resets its
                // SelectedItem to null, which (TwoWay) writes null back to row.SelectedModel and
                // wipes out the value we're about to apply. Reassigning the property fires a
                // single PropertyChanged with a fully-populated collection in hand.
                row.Voices = new ObservableCollection<Voice>(voices);
                row.Models = new ObservableCollection<string>(models);

                var pickedVoice = PickBestVoice(row.Actor, voices, preferredVoiceName, engine);
                var pickedModel = PickBestModel(models, preferredModelName);

                row.SelectedVoice = pickedVoice;
                row.SelectedModel = pickedModel;
                row.HasModel = engine.HasModel && models.Length > 0;
                row.HasInstruction = EngineSupportsInstruction(engine);
                row.HasAdvancedSettings = HasAdvancedSettingsForRow(engine, pickedModel);
                row.IsVoiceComboEnabled = voices.Length > 1;
                UpdateStatus();

                // Defensive second pass on the next dispatcher tick: if the ComboBox happens to
                // process a deferred SelectedItem reset *after* our assignment, re-apply it so
                // the user (and the OK handler) sees the right value. No-op when the value is
                // already correct.
                Dispatcher.UIThread.Post(() =>
                {
                    if (pickedModel != null && !string.Equals(row.SelectedModel, pickedModel, StringComparison.Ordinal))
                    {
                        row.SelectedModel = pickedModel;
                    }
                    if (pickedVoice != null && !ReferenceEquals(row.SelectedVoice, pickedVoice))
                    {
                        row.SelectedVoice = pickedVoice;
                    }
                }, DispatcherPriority.Background);
            });
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, $"Cast dialog: loading voices/models for {engine.Name} failed.");
        }
        finally
        {
            row.IsBusy = false;
        }
    }

    private async Task<string[]> GetModelsAsync(ITtsEngine engine)
    {
        if (_modelCache.TryGetValue(engine.Name, out var cached))
        {
            return cached;
        }

        var models = await engine.GetModels();
        _modelCache[engine.Name] = models;
        return models;
    }

    private static string? PickBestModel(string[] models, string preferred)
    {
        if (models.Length == 0)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(preferred))
        {
            var match = Array.Find(models, m => string.Equals(m, preferred, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                return match;
            }
        }

        return models[0];
    }


    private async Task<Voice[]> GetVoicesAsync(ITtsEngine engine)
    {
        if (_voiceCache.TryGetValue(engine.Name, out var cached))
        {
            return cached;
        }

        var voices = await engine.GetVoices(string.Empty);
        _voiceCache[engine.Name] = voices;
        return voices;
    }

    // Picks the best voice for an actor:
    //   1. the previously-saved voice name (case-insensitive) if it's still in the list
    //   2. a voice whose name contains the actor name (e.g. actor "Aria" → "en-US-AriaNeural")
    //   3. the global fallback voice if this row's engine is the global engine
    //   4. the first voice
    private Voice? PickBestVoice(string actor, Voice[] voices, string preferredVoiceName, ITtsEngine engine)
    {
        if (voices.Length == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(preferredVoiceName))
        {
            var saved = voices.FirstOrDefault(v => string.Equals(v.Name, preferredVoiceName, StringComparison.OrdinalIgnoreCase));
            if (saved != null)
            {
                return saved;
            }
        }

        if (!string.IsNullOrWhiteSpace(actor))
        {
            var match = voices.FirstOrDefault(v => v.Name.Contains(actor, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                return match;
            }
        }

        if (_fallbackEngine != null
            && _fallbackVoice != null
            && string.Equals(engine.Name, _fallbackEngine.Name, StringComparison.OrdinalIgnoreCase))
        {
            var fallback = voices.FirstOrDefault(v => string.Equals(v.Name, _fallbackVoice.Name, StringComparison.OrdinalIgnoreCase));
            if (fallback != null)
            {
                return fallback;
            }
        }

        return voices.First();
    }

    private static bool EngineSupportsInstruction(ITtsEngine? engine)
    {
        // Mirrors the engines that opt into RefreshInstructionVisibility in the main TTS VM. We
        // expose the field unconditionally for these engines - the cast dialog doesn't know which
        // sub-model the row will use, and an unused instruction is harmless.
        return engine is Qwen3TtsCpp or Qwen3TtsCrispAsr or OmniVoiceTtsCpp;
    }

    // True when opening the row-settings sub-window would actually surface usable controls for
    // this engine+model+voice combination. Used to disable the row's "..." button so users don't
    // open an empty dialog.
    //   - Qwen3:     only if the VoiceDesign model is selected (other Qwen3 models ignore the
    //                instruction field)
    //   - OmniVoice: always (cloned voices still get the disabled picker with a note)
    //   - others:    never
    private static bool HasAdvancedSettingsForRow(ITtsEngine? engine, string? model)
    {
        return engine switch
        {
            Qwen3TtsCpp => Qwen3TtsCpp.IsVoiceDesignModel(model),
            Qwen3TtsCrispAsr => Qwen3TtsCrispAsr.IsVoiceDesignModel(model),
            OmniVoiceTtsCpp => true,
            _ => false,
        };
    }

    [RelayCommand]
    private async Task TestRow(ActorVoiceRow? row)
    {
        if (row?.SelectedEngine == null || row.SelectedVoice == null || Window == null)
        {
            return;
        }

        if (!await TtsVoiceInstaller.EnsureVoiceInstalled(row.SelectedEngine, row.SelectedVoice, Window, _windowService))
        {
            return;
        }

        var generatingAudioVm = _windowService.ShowWindow<GeneratingAudioWindow, GeneratingAudioViewModel>(Window!);
        _cancellationTokenSource = generatingAudioVm.CancellationTokenSource;

        try
        {
            row.IsPlaying = true;
            var result = await TtsInstructionSwap.RunAsync(row.SelectedEngine, row.Instruction, () =>
                row.SelectedEngine.Speak(Utilities.UnbreakLine(_voiceTestText), _waveFolder, row.SelectedVoice,
                    null, null, null, _cancellationTokenSource.Token));

            if (!_cancellationTokenSource.IsCancellationRequested && File.Exists(result.FileName))
            {
                await PlayAudio(result.FileName);
            }
        }
        catch (OperationCanceledException)
        {
            // user cancelled
        }
        catch (Exception ex)
        {
            SeLogger.Error(ex, "Cast dialog: test voice failed.");
            if (Window != null)
            {
                await MessageBox.Show(Window, Se.Language.General.Error, ex.Message,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            row.IsPlaying = false;
            generatingAudioVm.Close();
        }
    }

    private async Task PlayAudio(string fileName)
    {
        lock (_playLock)
        {
            _mpvContext?.Stop();
            _mpvContext?.Dispose();

            _mpvContext = new LibMpvDynamicPlayer();
            _mpvContext.LoadLib();
            var err = _mpvContext.Initialize();
            if (err < 0)
            {
                throw new InvalidOperationException($"Failed to initialize mpv: {_mpvContext.GetErrorString(err)}");
            }
        }

        await _mpvContext.LoadAudio(fileName);
    }

    [RelayCommand]
    private void ApplyDefaultToAll()
    {
        if (_fallbackEngine == null)
        {
            return;
        }

        foreach (var row in Rows)
        {
            if (row.SelectedEngine == _fallbackEngine)
            {
                // Resolve the fallback voice against the row's own Voices collection — the
                // _fallbackVoice instance came from the main window's Voices list and may not
                // be the same instance the row has. Setting an out-of-list reference on the
                // ComboBox is silently rejected (binding warning, SelectedVoice stays null).
                if (row.SelectedVoice == null && _fallbackVoice != null)
                {
                    row.SelectedVoice = row.Voices.FirstOrDefault(v =>
                        string.Equals(v.Name, _fallbackVoice.Name, StringComparison.OrdinalIgnoreCase));
                }
                continue;
            }

            _ = AssignEngineToRowAsync(row, _fallbackEngine, _fallbackVoice?.Name ?? string.Empty, string.Empty);
        }
    }

    [RelayCommand]
    private async Task ShowRowSettings(ActorVoiceRow? row)
    {
        if (row?.SelectedEngine == null || Window == null
            || !HasAdvancedSettingsForRow(row.SelectedEngine, row.SelectedModel))
        {
            return;
        }

        var capturedRow = row;
        var result = await _windowService.ShowDialogAsync<ActorVoiceRowSettingsWindow, ActorVoiceRowSettingsViewModel>(
            Window!, vm => vm.Initialize(capturedRow));

        if (result.OkPressed)
        {
            capturedRow.Instruction = result.Instruction ?? string.Empty;
        }
    }

    [RelayCommand]
    private async Task ClearAll()
    {
        if (Window == null)
        {
            return;
        }

        var answer = await MessageBox.Show(
            Window,
            Se.Language.Video.TextToSpeech.ActorVoicesTitle,
            Se.Language.Video.TextToSpeech.ClearAllAssignmentsConfirm,
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (answer != MessageBoxResult.Yes)
        {
            return;
        }

        foreach (var row in Rows)
        {
            row.Instruction = string.Empty;
            row.SelectedVoice = null;
        }

        UpdateStatus();
    }

    [RelayCommand]
    private void Ok()
    {
        Mappings = Rows
            .Select(r => new ActorVoiceMapping
            {
                Actor = r.Actor,
                EngineName = r.SelectedEngine?.Name ?? string.Empty,
                VoiceName = r.SelectedVoice?.Name ?? string.Empty,
                Model = r.SelectedModel ?? string.Empty,
                Instruction = (r.Instruction ?? string.Empty).Trim(),
            })
            .Where(m => !string.IsNullOrEmpty(m.EngineName) && !string.IsNullOrEmpty(m.VoiceName))
            .ToList();

        Se.Settings.Video.TextToSpeech.LastActorVoiceMappings = MergeWithPersistedMappings(Mappings);
        Se.SaveSettings();

        OkPressed = true;
        Close();
    }

    // Keep mappings for actors not in this subtitle so a user who jumps between projects keeps
    // their full cast remembered. New/updated rows from this dialog overwrite anything saved
    // under the same actor name.
    private static List<ActorVoiceMapping> MergeWithPersistedMappings(List<ActorVoiceMapping> current)
    {
        var previous = Se.Settings.Video.TextToSpeech.LastActorVoiceMappings ?? new List<ActorVoiceMapping>();
        var byActor = previous
            .Where(m => !string.IsNullOrWhiteSpace(m.Actor))
            .GroupBy(m => m.Actor.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var m in current)
        {
            byActor[m.Actor.Trim()] = m;
        }

        return byActor.Values.ToList();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        StopPlayback();
        Dispatcher.UIThread.Post(() => Window?.Close());
    }

    private void StopPlayback()
    {
        try
        {
            _cancellationTokenSource.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // CTS already disposed via OnClosing
        }

        lock (_playLock)
        {
            _mpvContext?.Stop();
            _mpvContext?.Dispose();
            _mpvContext = null;
        }
    }

    private void UpdateStatus()
    {
        var total = Rows.Count;
        var assigned = Rows.Count(r => r.SelectedVoice != null);
        StatusText = total == 0
            ? string.Empty
            : string.Format(Se.Language.Video.TextToSpeech.ActorVoicesAssignedXOfY, assigned, total);
        IsMappingEmpty = total == 0;
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Close();
        }
    }

    internal void OnClosing(WindowClosingEventArgs e)
    {
        StopPlayback();
        // The CTS we created in the constructor is reassigned in TestRow to one
        // owned by GeneratingAudioViewModel, so by the time we get here we may
        // be holding either. Dispose what we have — borrowing the test-run CTS
        // is harmless because the test dialog has already closed.
        try { _cancellationTokenSource.Dispose(); } catch (ObjectDisposedException) { }
        UiUtil.SaveWindowPosition(Window);
    }
}
