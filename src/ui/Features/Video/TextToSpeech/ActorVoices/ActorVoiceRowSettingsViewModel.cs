using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ActorVoices;

// Per-row advanced settings dialog. Shows the controls that don't fit neatly in a grid cell:
//   - free-text voice instruction (Qwen3 VoiceDesign)
//   - OmniVoice keyword picker (gender/age/pitch/accent/whisper) which composes into Instruction
// On OK, the user's edits are reflected in the result so the caller can copy them back to the row.
public partial class ActorVoiceRowSettingsViewModel : ObservableObject
{
    [ObservableProperty] private string _actor;
    [ObservableProperty] private string _engineName;
    [ObservableProperty] private string _voiceName;
    [ObservableProperty] private string _instruction;
    [ObservableProperty] private bool _isInstructionTextVisible;
    [ObservableProperty] private bool _isOmniVoicePickerVisible;
    [ObservableProperty] private bool _isOmniVoicePickerEnabled;
    [ObservableProperty] private bool _isClonedVoiceNoteVisible;
    [ObservableProperty] private string _selectedOmniVoiceGender;
    [ObservableProperty] private string _selectedOmniVoiceAge;
    [ObservableProperty] private string _selectedOmniVoicePitch;
    [ObservableProperty] private string _selectedOmniVoiceAccent;
    [ObservableProperty] private bool _omniVoiceWhisper;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    public ObservableCollection<string> OmniVoiceGenders { get; }
    public ObservableCollection<string> OmniVoiceAges { get; }
    public ObservableCollection<string> OmniVoicePitches { get; }
    public ObservableCollection<string> OmniVoiceAccents { get; }

    public const string OmniVoiceAny = "(any)";

    private bool _suppressKeywordSync;

    public ActorVoiceRowSettingsViewModel()
    {
        _actor = string.Empty;
        _engineName = string.Empty;
        _voiceName = string.Empty;
        _instruction = string.Empty;

        OmniVoiceGenders = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionGenders);
        OmniVoiceAges = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionAges);
        OmniVoicePitches = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionPitches);
        OmniVoiceAccents = BuildKeywordOptions(OmniVoiceTtsCpp.InstructionAccents);

        _selectedOmniVoiceGender = OmniVoiceAny;
        _selectedOmniVoiceAge = OmniVoiceAny;
        _selectedOmniVoicePitch = OmniVoiceAny;
        _selectedOmniVoiceAccent = OmniVoiceAny;
    }

    public void Initialize(ActorVoiceRow row)
    {
        Actor = row.Actor;
        EngineName = row.SelectedEngine?.Name ?? string.Empty;
        VoiceName = row.SelectedVoice?.Name ?? string.Empty;
        Instruction = row.Instruction ?? string.Empty;

        // Qwen3 free-text instruction only does anything on the VoiceDesign model — Base models
        // ignore it. Don't show it for the other Qwen3 models. OmniVoice always uses the keyword
        // picker, but the picker is informational (disabled with a note) when a cloned voice is
        // selected because omnivoice-tts ignores --instruct in that case.
        IsInstructionTextVisible =
            (row.SelectedEngine is Qwen3TtsCpp && Qwen3TtsCpp.IsVoiceDesignModel(row.SelectedModel))
            || (row.SelectedEngine is Qwen3TtsCrispAsr && Qwen3TtsCrispAsr.IsVoiceDesignModel(row.SelectedModel));

        IsOmniVoicePickerVisible = row.SelectedEngine is OmniVoiceTtsCpp;
        var isDefaultOmniVoice = row.SelectedVoice?.EngineVoice is OmniVoiceVoice ov && string.IsNullOrEmpty(ov.FilePath);
        IsOmniVoicePickerEnabled = IsOmniVoicePickerVisible && isDefaultOmniVoice;
        IsClonedVoiceNoteVisible = IsOmniVoicePickerVisible && !isDefaultOmniVoice;

        if (IsOmniVoicePickerVisible)
        {
            SyncOmniVoicePickerFromInstruction();
        }
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    private void Close()
    {
        Window?.Close();
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
        UiUtil.SaveWindowPosition(Window);
    }

    partial void OnSelectedOmniVoiceGenderChanged(string value) => OnOmniVoicePickerChanged();
    partial void OnSelectedOmniVoiceAgeChanged(string value) => OnOmniVoicePickerChanged();
    partial void OnSelectedOmniVoicePitchChanged(string value) => OnOmniVoicePickerChanged();
    partial void OnSelectedOmniVoiceAccentChanged(string value) => OnOmniVoicePickerChanged();
    partial void OnOmniVoiceWhisperChanged(bool value) => OnOmniVoicePickerChanged();

    private void OnOmniVoicePickerChanged()
    {
        if (!_suppressKeywordSync && IsOmniVoicePickerVisible)
        {
            Instruction = BuildOmniVoiceInstruction();
        }
    }

    private string BuildOmniVoiceInstruction()
    {
        var parts = new List<string>();
        if (IsReal(SelectedOmniVoiceGender)) parts.Add(SelectedOmniVoiceGender);
        if (IsReal(SelectedOmniVoiceAge)) parts.Add(SelectedOmniVoiceAge);
        if (IsReal(SelectedOmniVoicePitch)) parts.Add(SelectedOmniVoicePitch);
        if (IsReal(SelectedOmniVoiceAccent)) parts.Add(SelectedOmniVoiceAccent);
        if (OmniVoiceWhisper) parts.Add(OmniVoiceTtsCpp.InstructionWhisper);
        return string.Join(", ", parts);
    }

    // Reflects the saved instruction string back into the picker controls so opening the dialog
    // a second time shows the previously chosen keywords. Same parse/regenerate dance as the main
    // TTS window's SyncOmniVoicePickerFromInstruction.
    private void SyncOmniVoicePickerFromInstruction()
    {
        var present = (Instruction ?? string.Empty)
            .Split(',')
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _suppressKeywordSync = true;
        SelectedOmniVoiceGender = Match(OmniVoiceTtsCpp.InstructionGenders, present);
        SelectedOmniVoiceAge = Match(OmniVoiceTtsCpp.InstructionAges, present);
        SelectedOmniVoicePitch = Match(OmniVoiceTtsCpp.InstructionPitches, present);
        SelectedOmniVoiceAccent = Match(OmniVoiceTtsCpp.InstructionAccents, present);
        OmniVoiceWhisper = present.Contains(OmniVoiceTtsCpp.InstructionWhisper);
        _suppressKeywordSync = false;

        Instruction = BuildOmniVoiceInstruction();
    }

    private static string Match(string[] group, HashSet<string> present)
        => Array.Find(group, present.Contains) ?? OmniVoiceAny;

    private static bool IsReal(string? value)
        => !string.IsNullOrEmpty(value) && value != OmniVoiceAny;

    private static ObservableCollection<string> BuildKeywordOptions(string[] keywords)
    {
        var options = new ObservableCollection<string> { OmniVoiceAny };
        foreach (var keyword in keywords)
        {
            options.Add(keyword);
        }
        return options;
    }
}
