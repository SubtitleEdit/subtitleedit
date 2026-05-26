namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageBeautifyTimeCodesProfile
{
    public string Title { get; set; } = "Edit beautify time codes profile";
    public string LoadPreset { get; set; } = "Load preset";
    public string PresetDefault { get; set; } = "Default";
    public string PresetNetflix { get; set; } = "Netflix";
    public string PresetSdi { get; set; } = "SDI";

    public string General { get; set; } = "General";
    public string Gap { get; set; } = "Gap";
    public string GapSuffix { get; set; } = "frames (also updates non-zero gap fields below)";
    public string Milliseconds { get; set; } = "ms";
    public string Frames { get; set; } = "frames";
    public string Zones { get; set; } = "Zones";

    public string InCues { get; set; } = "In cues";
    public string OutCues { get; set; } = "Out cues";

    public string ConnectedSubtitles { get; set; } = "Connected subtitles";
    public string InCueClosest { get; set; } = "In cue is closest";
    public string OutCueClosest { get; set; } = "Out cue is closest";
    public string TreatAsConnected { get; set; } = "Treat as connected if gap smaller than:";

    public string Chaining { get; set; } = "Chaining";
    public string InCueOnShot { get; set; } = "In cue on shot change";
    public string OutCueOnShot { get; set; } = "Out cue on shot change";
    public string MaxGap { get; set; } = "Max. gap:";
    public string ShotChangeBehavior { get; set; } = "If there is a shot change in between:";
    public string DontChain { get; set; } = "Don't chain";
    public string ExtendCrossingShotChange { get; set; } = "Extend across shot change";
    public string ExtendUntilShotChange { get; set; } = "Extend until shot change";
    public string CheckGeneral { get; set; } = "Apply 'general' chaining rules too";

    public string SubtitlePreviewText { get; set; } = "Subtitle text.";
    public string ResetWarning { get; set; } = "This will overwrite all current settings. Continue?";

    // Hints
    public string HintGap { get; set; } = "Minimum gap between subtitles, expressed in frames. Used by all 'gap' fields below as the base value.";
    public string HintZones { get; set; } = "Snap (red) and warn (green) thresholds in frames, measured from the nearest shot change. Four values, left to right: left green, left red, right red, right green.";
    public string HintConnected { get; set; } = "When two subtitles are connected (gap below threshold), the gap from the closest cue is preserved.";
    public string HintChaining { get; set; } = "How to handle subtitles that almost touch — extend them together, or push apart, depending on whether a shot change sits in between.";
}
