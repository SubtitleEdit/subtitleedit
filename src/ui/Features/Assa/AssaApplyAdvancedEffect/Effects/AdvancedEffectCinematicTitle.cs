using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Cinematic title reveal: letter spacing animates from wide tracking to normal while the
/// text resolves from a soft blur into focus, with a slow fade - the classic film-title
/// look. Long lines get a mirrored defocus/tracking-out exit. Coordinate-free, so it works
/// at any script resolution and keeps the line's own styling and position.
/// </summary>
public class AdvancedEffectCinematicTitle : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectCinematicTitle;
    public string Description => Se.Language.Assa.AdvancedEffectCinematicTitleDescription;
    public bool UsesAudio => false;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(
        string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();

        foreach (var sub in subtitles)
        {
            int durationMs = (int)sub.Duration.TotalMilliseconds;
            if (durationMs <= 0 || string.IsNullOrWhiteSpace(sub.Text))
            {
                result.Add(AdvancedEffectUtil.PassThrough(sub));
                continue;
            }

            int revealMs = Math.Min(1200, durationMs / 2);
            int fadeInMs = Math.Min(700, durationMs / 3);
            int fadeOutMs = Math.Min(500, durationMs / 4);

            var tags = $"{{\\fad({fadeInMs},{fadeOutMs})\\fsp20\\blur16" +
                       $"\\t(0,{revealMs},\\fsp0\\blur0)";

            // Mirrored exit (tracking out + defocus) only when the line is long enough
            // that it does not collide with the reveal
            const int exitMs = 800;
            if (durationMs > revealMs + exitMs + 500)
            {
                tags += $"\\t({durationMs - exitMs},{durationMs},\\fsp12\\blur10)";
            }
            tags += "}";

            var title = new SubtitleLineViewModel(sub, generateNewId: true);
            title.Text = tags + sub.Text;
            result.Add(title);
        }

        return result;
    }
}
