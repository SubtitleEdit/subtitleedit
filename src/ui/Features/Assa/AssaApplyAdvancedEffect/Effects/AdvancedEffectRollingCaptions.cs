using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Rolling captions, like a live chat or a teleprompter: each line slides up into place from
/// below, and when the next line arrives the previous one moves up above it and dims. Lines
/// that follow each other closely roll; after a pause the captions start fresh.
/// </summary>
public class AdvancedEffectRollingCaptions : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectRollingCaptions;
    public string Description => Se.Language.Assa.AdvancedEffectRollingCaptionsDescription;
    public bool UsesAudio => false;

    /// <summary>Lines closer than this roll into each other; a longer pause starts fresh.</summary>
    private const double MaxRollGapMs = 1000;
    private const int SlideMs = 220;

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        var lines = subtitles.OrderBy(s => s.StartTime).ToList();
        var (scriptWidth, scriptHeight) = AdvancedEffectUtil.GetScriptResolution(header, width, height);
        var x = scriptWidth / 2;

        // a line rolls into the next when that starts soon after; it is then shown at the bottom
        // only until the next line arrives
        var rolls = new bool[lines.Count];
        for (var i = 0; i + 1 < lines.Count; i++)
        {
            rolls[i] = lines[i + 1].StartTime > lines[i].StartTime &&
                       lines[i + 1].StartTime.TotalMilliseconds <= lines[i].EndTime.TotalMilliseconds + MaxRollGapMs;
        }

        TimeSpan BottomEnd(int index) => rolls[index] ? lines[index + 1].StartTime : lines[index].EndTime;

        for (var i = 0; i < lines.Count; i++)
        {
            var sub = lines[i];
            var style = CaptionWords.GetStyle(header, sub);
            var marginV = int.TryParse(sub.MarginV, out var lineMarginV) && lineMarginV > 0 ? lineMarginV : style.MarginVertical;
            var baseY = scriptHeight - marginV;
            var lineHeight = (double)style.FontSize * 1.2 * Math.Max(1, (double)style.ScaleY / 100);
            var text = AdvancedEffectUtil.RemovePositionTags(sub.Text);
            var slide = (int)Math.Round(lineHeight * 0.8);

            // 1) the current line: slides up from below into the bottom position
            var current = AdvancedEffectUtil.PassThrough(sub);
            current.EndTime = BottomEnd(i);
            current.Text = $"{{\\an2\\move({x},{baseY + slide},{x},{baseY},0,{SlideMs})\\fad(150,{(rolls[i] ? 0 : 150)})}}" + text;
            result.Add(current);

            // 2) pushed up by the next line: moves up by the next line's height, dimmed, and leaves
            //    when the next line itself is pushed up (or ends)
            if (rolls[i])
            {
                var next = lines[i + 1];
                var upY = baseY - (int)Math.Round(lineHeight * CountLines(next.Text));
                var previous = AdvancedEffectUtil.PassThrough(sub);
                previous.StartTime = next.StartTime;
                previous.EndTime = BottomEnd(i + 1);
                previous.Text = $"{{\\an2\\move({x},{baseY},{x},{upY},0,{SlideMs})\\t(0,{SlideMs},\\alpha&H70&)\\fad(0,150)}}" + text;
                result.Add(previous);
            }
        }

        return result;
    }

    private static int CountLines(string text)
    {
        var normalized = text.Replace("\\N", "\n").Replace("\\n", "\n").Replace("\r\n", "\n");
        return normalized.Split('\n').Length;
    }
}
