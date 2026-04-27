using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

public class AdvancedEffectNeonBurst : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectNeonBurst;
    public string Description => Se.Language.Assa.AdvancedEffectNeonBurstDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        Random rng = new Random(subtitles[0].Text.GetHashCode());

        foreach (var sub in subtitles)
        {
            // 1. SELECT NEON COLOR
            // Modern shorts use 'Electric Lime', 'Cyan', or 'Hot Pink'
            string[] neonColors = { "&H00FF00&", "&HFFFF00&", "&HFF00FF&", "&H00FFFF&" };
            string chosenColor = neonColors[rng.Next(neonColors.Length)];

            // 2. CREATE THE GLOW LAYER (The 'Bloom')
            // We create a duplicate line that sits behind the text with a heavy blur
            var glowLayer = new SubtitleLineViewModel()
            {
                StartTime = sub.StartTime,
                EndTime = sub.EndTime,
                // Deep neon glow: High blur, matching color, slightly transparent
                Text = "{\\bord5\\blur8\\shad0\\1c" + chosenColor + "\\3c" + chosenColor + "\\alpha&H60&" +
                       GetPopTags(0, 150) + "}" + HtmlUtil.RemoveHtmlTags(sub.Text, true) // CleanText removes existing tags
            };

            // 3. CREATE THE SHARP TOP LAYER (The 'Core')
            var coreLayer = new SubtitleLineViewModel()
            {
                StartTime = sub.StartTime,
                EndTime = sub.EndTime,
                // Sharp white core with a thin neon border
                Text = "{\\bord2\\blur0.5\\shad0\\1c&HFFFFFF&\\3c" + chosenColor +
                       GetPopTags(0, 150) + "}" + HtmlUtil.RemoveHtmlTags(sub.Text, true)
            };

            result.Add(glowLayer);
            result.Add(coreLayer);
        }

        return result;
    }

    private static string GetPopTags(int startOffset, int duration)
    {
        // Starts at 125% size and shrinks down to 100% quickly
        return $"\\fscx125\\fscy125\\t({startOffset},{duration},\\fscx100\\fscy100)";
    }
}

