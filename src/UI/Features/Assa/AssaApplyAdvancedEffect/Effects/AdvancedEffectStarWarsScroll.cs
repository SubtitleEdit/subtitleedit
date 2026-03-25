using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectStarWarsScroll : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectStarWarsScroll;
    public string Description => Se.Language.Assa.AdvancedEffectStarWarsScrollDescription;
    public bool UsesAudio => false;
    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        int screenWidth = width > 0 ? width : 1356;
        int screenHeight = height > 0 ? height : 678;
        int centerX = screenWidth / 2;

        string swYellow = "&H00D7FF&";

        // PHYSICS SETTINGS
        double travelDurationMs = 25000;
        double leadTimeMs = 1500; // Even lower to ensure it hits dialogue timing
        int internalLineSpacing = 80;

        int yStartBase = screenHeight + 50;
        int yEndBase = -300;

        foreach (var sub in subtitles)
        {
            string processedText = sub.Text.Replace("\\N", "\n").Replace("\\n", "\n");
            var clean = Utilities.RemoveSsaTags(processedText);
            if (string.IsNullOrWhiteSpace(clean)) continue;

            var lines = clean.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                var charLine = new SubtitleLineViewModel(sub, generateNewId: true);

                double startTimeMs = sub.StartTime.TotalMilliseconds - leadTimeMs;
                if (startTimeMs < 0) startTimeMs = 0;

                charLine.StartTime = TimeSpan.FromMilliseconds(startTimeMs);
                // We give it an extra 2 seconds of life to ensure the fade finishes
                charLine.EndTime = charLine.StartTime.Add(TimeSpan.FromMilliseconds(travelDurationMs + 2000));

                int yOffset = i * internalLineSpacing;

                // THE TAGS
                string tags =
                    $@"\an5\b1\bord2\blur0.6\1c{swYellow}" +
                    $@"\frx52\fscy75" +
                    $@"\fad(1200,0)" +
                    $@"\fscx140\fscy140" +
                    $@"\move({centerX},{yStartBase + yOffset},{centerX},{yEndBase + yOffset},0,{(int)travelDurationMs})" +

                    // SCALE
                    $@"\t(0,{(int)travelDurationMs},\fscx35\fscy20)" +

                    // FADE: Start turning transparent at 60% of the journey (around the upper-middle)
                    // and finish by 90% of the journey.
                    $@"\t({(int)(travelDurationMs * 0.6)},{(int)(travelDurationMs * 0.9)},\alpha&HFF&)";

                charLine.Text = "{" + tags + "}" + lines[i].Trim();
                result.Add(charLine);
            }
        }

        return result;
    }
}

