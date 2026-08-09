using Avalonia.Media;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Lower third: an interview/documentary style banner bottom-left. The line's first row is
/// the name (bold), any following rows are the role (smaller, dimmed), and a colored accent
/// bar slides in between them. All geometry is derived from the header's script resolution
/// (\pos/\move are in PlayRes space, not video pixels).
/// </summary>
public class AdvancedEffectLowerThird : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectLowerThird;
    public string Description => Se.Language.Assa.AdvancedEffectLowerThirdDescription;
    public bool UsesAudio => false;

    /// <summary>
    /// Color of the accent bar under the name.
    /// </summary>
    public Color AccentColor { get; set; } = Color.FromRgb(0x00, 0xB4, 0xFF);

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(
        string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        var (w, h) = AdvancedEffectUtil.GetScriptResolution(header, width, height);

        int x = (int)(w * 0.06);
        int nameY = (int)(h * 0.83);
        int barY = (int)(h * 0.845);
        int roleY = (int)(h * 0.90);
        int barWidth = (int)(w * 0.30);
        int barHeight = Math.Max(2, (int)(h * 0.010));
        string accent = AdvancedEffectUtil.ToAssColor(AccentColor);

        foreach (var sub in subtitles)
        {
            int durationMs = (int)sub.Duration.TotalMilliseconds;
            var cleanText = Utilities.RemoveSsaTags(sub.Text)
                .Replace("\r\n", "\n").Replace("\r", "\n").Trim();
            if (durationMs <= 0 || string.IsNullOrEmpty(cleanText))
            {
                result.Add(AdvancedEffectUtil.PassThrough(sub));
                continue;
            }

            var lines = cleanText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string nameText = lines[0];
            string roleText = string.Join("\\N", lines.Skip(1));
            int slideMs = Math.Max(1, Math.Min(500, durationMs / 3));

            // Accent bar slides in fully from off-screen left
            var bar = new SubtitleLineViewModel
            {
                StartTime = sub.StartTime,
                EndTime = sub.EndTime,
                Layer = sub.Layer + 1,
                Text = $"{{\\p1\\an7\\bord0\\shad0\\1c{accent}\\fad(150,250)" +
                       $"\\move({-barWidth},{barY},{x},{barY},0,{slideMs})}}" +
                       $"m 0 0 l {barWidth} 0 l {barWidth} {barHeight} l 0 {barHeight}",
            };
            result.Add(bar);

            // Name: bold, slides in a short distance with the bar
            var name = new SubtitleLineViewModel(sub, generateNewId: true);
            name.Layer = sub.Layer + 2;
            name.Text = $"{{\\an1\\b1\\fad(200,250)" +
                        $"\\move({x - (int)(w * 0.04)},{nameY},{x},{nameY},0,{slideMs})}}" + nameText;
            result.Add(name);

            if (roleText.Length > 0)
            {
                // Role: smaller and dimmed, trails slightly behind the name
                var role = new SubtitleLineViewModel(sub, generateNewId: true);
                role.Layer = sub.Layer + 3;
                role.Text = $"{{\\an1\\fscx80\\fscy80\\1c&HDDDDDD&\\fad(300,250)" +
                            $"\\move({x - (int)(w * 0.03)},{roleY},{x},{roleY},0,{slideMs})}}" + roleText;
                result.Add(role);
            }
        }

        return result;
    }
}
