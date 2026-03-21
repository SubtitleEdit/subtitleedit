using System;

namespace Nikse.SubtitleEdit.Logic.Config.Language.Tools;

public class LanguageApplyDurationLimits
{
    public string Title { get; set; }
    public string FixMinDurationMs { get; set; }
    public string DoNotGoPastShotChange { get; set; }
    public string FixMaxDurationMs { get; set; }
    public string MaxDurationShouldBeHigherThanMinDuration { get; set; }
    public string ChangedDurationFromXToYCommentZ { get; set; }
    public string OnlyPartialFixed { get; set; }
    public string UnfixableX { get; set; }
    public string FixedX { get; set; }
    public string FixedXImprovedY { get; set; }
    public string NoChangesNeeded { get; set; }

    public LanguageApplyDurationLimits()
    {
        Title = "Apply duration limits";
        FixMinDurationMs = "Fix minimum duration (ms)";
        DoNotGoPastShotChange = "Do not go past shot change";
        FixMaxDurationMs = "Fix maximum duration (ms)";
        MaxDurationShouldBeHigherThanMinDuration = "Maximum duration should be higher than minimum duration";
        ChangedDurationFromXToYCommentZ =  "Changed duration from {0} to {1} {2}";
        OnlyPartialFixed = "(only partial fix)";
        UnfixableX = "Unfixable: {0}";
        FixedXImprovedY = "Fixes: {0}, Improvements: {1}";
        FixedX = "Fixed: {0}";
        NoChangesNeeded = "No changes needed";
    }
}