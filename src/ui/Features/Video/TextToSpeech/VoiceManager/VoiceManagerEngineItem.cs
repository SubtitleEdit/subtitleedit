using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Features.Video.TextToSpeech.Engines;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Video.TextToSpeech.VoiceManager;

public partial class VoiceManagerEngineItem : ObservableObject
{
    public ITtsEngine Engine { get; }
    public string Name => Engine.Name;
    public string Icon { get; }
    public bool IsCloning => Engine.SupportsVoiceCloning;

    /// <summary>"12" once the engine's voices have been listed; empty until then.</summary>
    [ObservableProperty] private string _countText = string.Empty;
    [ObservableProperty] private bool _hasCount;

    /// <summary>Null until the (possibly slow) install check has run; then the engine's answer.</summary>
    [ObservableProperty] private bool? _isInstalled;

    public VoiceManagerEngineItem(ITtsEngine engine)
    {
        Engine = engine;
        Icon = engine.SupportsVoiceCloning ? IconNames.AccountVoice
            : engine.HasApiKey ? IconNames.Web
            : engine is Piper ? IconNames.Download
            : IconNames.Robot;
    }

    public void SetCount(int count)
    {
        CountText = count.ToString();
        HasCount = true;
    }

    public override string ToString() => Name;
}
