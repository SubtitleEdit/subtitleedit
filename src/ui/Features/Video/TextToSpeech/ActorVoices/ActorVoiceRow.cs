using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Voices;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.ActorVoices;

// One row in the cast grid. Each row owns its own voice list (filled by the VM after the engine
// is loaded) and instruction text so different rows can use entirely different engines, voices,
// and instructions.
//
// The VM listens for EngineChangedByUser to know when the engine combo was changed by the user
// and a new voice list needs to be loaded. The VM uses SuppressEngineEvent when it's the one
// changing the engine (e.g. during initial population) so the event doesn't fire back into a
// reload loop.
public partial class ActorVoiceRow : ObservableObject
{
    [ObservableProperty] private string _actor;
    [ObservableProperty] private ObservableCollection<Voice> _voices;
    [ObservableProperty] private ObservableCollection<string> _models;
    [ObservableProperty] private ITtsEngine? _selectedEngine;
    [ObservableProperty] private Voice? _selectedVoice;
    [ObservableProperty] private string? _selectedModel;
    [ObservableProperty] private string _instruction;
    [ObservableProperty] private bool _hasModel;
    [ObservableProperty] private bool _hasInstruction;
    [ObservableProperty] private bool _hasAdvancedSettings;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private bool _isVoiceComboEnabled;

    public event EventHandler<ITtsEngine?>? EngineChangedByUser;

    // Set by the VM around its own writes so OnSelectedEngineChanged doesn't fire the event back
    // into the VM, which would cause an infinite reload loop.
    public bool SuppressEngineEvent { get; set; }

    public ActorVoiceRow()
    {
        _actor = string.Empty;
        _voices = new ObservableCollection<Voice>();
        _models = new ObservableCollection<string>();
        _instruction = string.Empty;
        _isVoiceComboEnabled = true;
    }

    partial void OnSelectedEngineChanged(ITtsEngine? value)
    {
        if (SuppressEngineEvent)
        {
            return;
        }

        EngineChangedByUser?.Invoke(this, value);
    }
}
