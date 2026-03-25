using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Assa.AssaApplyAdvancedEffect.Effects;

/// <summary>
/// Matrix effect: falling green character columns (background) with a letter-by-letter
/// green-to-white reveal that builds the subtitle text from the rain.
/// </summary>
public class AdvancedEffectMatrix : IAdvancedEffectDisplay
{
    public string Name => Se.Language.Assa.AdvancedEffectMatrix;
    public string Description => Se.Language.Assa.AdvancedEffectMatrixDescription;
    public bool UsesAudio => false;
    public int ColumnCount { get; set; } = 55;

    private static readonly char[] MatrixPool =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789$!?@#%&*<>[]{}|/;:".ToCharArray();

    private const string HeadColor = "&HCCFFCC&"; // bright white-green stream head
    private const string BrightGreen = "&H00FF00&"; // pure green
    private const string MidGreen = "&H007800&"; // mid-green trail
    private const string DarkGreen = "&H003400&"; // dark-green tail

    public override string ToString() => Name;

    public List<SubtitleLineViewModel> ApplyEffect(string header, List<SubtitleLineViewModel> subtitles, int width, int height, WavePeakData2? wavePeaks)
    {
        var result = new List<SubtitleLineViewModel>();
        if (subtitles.Count == 0) return result;

        var rng = new Random(subtitles[0].Text.GetHashCode());

        int w = width > 0 ? width : 1280;
        int h = height > 0 ? height : 720;

        var globalStart = subtitles.Min(s => s.StartTime);
        var globalEnd = subtitles.Max(s => s.EndTime);
        double totalVideoMs = (globalEnd - globalStart).TotalMilliseconds;

        // ── MATRIX RAIN COLUMNS ─────────────────────────────────────────────────
        int cols = ColumnCount;
        int colW = w / cols;
        int fs = Math.Max(10, (int)(colW * 0.65)); // compact grid

        for (int col = 0; col < cols; col++)
        {
            int colX = col * colW + colW / 2;
            double streamStart = rng.Next(0, (int)(totalVideoMs * 0.25)) + col * 25;

            while (streamStart < totalVideoMs)
            {
                int streamLen = rng.Next(8, 18);
                int fallDur = rng.Next(1200, 3500);
                int interval = fallDur / (streamLen + 2); // stagger between chars in stream

                for (int j = 0; j < streamLen; j++)
                {
                    double charStartMs = streamStart + j * interval;
                    if (charStartMs >= totalVideoMs) break;

                    var entry = new SubtitleLineViewModel();
                    entry.StartTime = globalStart.Add(TimeSpan.FromMilliseconds(charStartMs));
                    entry.EndTime = entry.StartTime.Add(TimeSpan.FromMilliseconds(fallDur));
                    if (entry.StartTime >= globalEnd) continue;

                    char ch = MatrixPool[rng.Next(MatrixPool.Length)];

                    string color, extra;
                    int alpha;
                    if (j == 0) // head — first launched, deepest, brightest
                    {
                        color = HeadColor;
                        extra = $"\\blur3.5\\bord0.5\\3c{BrightGreen}";
                        alpha = 0x00;
                    }
                    else
                    {
                        float pos = (float)j / (streamLen - 1); // 0 = head … 1 = tail
                        color = pos < 0.35f ? BrightGreen : pos < 0.65f ? MidGreen : DarkGreen;
                        float trailBlur = Math.Max(1.0f, 3.0f - pos * 2.0f);
                        extra = $"\\blur{trailBlur:F1}\\bord0";
                        alpha = Math.Min(0xD0, (int)(pos * 200));
                    }

                    string tags =
                        $"\\an5\\1c{color}{extra}\\alpha&H{alpha:X2}&\\shad0\\fs{fs}" +
                        $"\\move({colX},-20,{colX},{h + 20})";

                    entry.Text = "{" + tags + "}" + ch;
                    result.Add(entry);
                }

                streamStart += fallDur + rng.Next(100, 2500);
            }
        }

        // ── SUBTITLE CHARACTERS FALL INTO POSITION ───────────────────────────────────────
        // Each character falls the full screen height in two seamless halves:
        //   Part 1 — scrambled green, falls from top to the reveal point
        //   Part 2 — actual white char, continues falling from the same Y to the bottom
        // The reveal fraction varies left-to-right, creating a diagonal sweep as the text descends.
        for (int subIdx = 0; subIdx < subtitles.Count; subIdx++)
        {
            var sub = subtitles[subIdx];
            // With preRoll applied, every subtitle's chars land at restY at that subtitle's StartTime.
            // So we only need the raw gap to the next subtitle to know when to clear old text.
            double nextSubOffsetMs = subIdx + 1 < subtitles.Count
                ? (subtitles[subIdx + 1].StartTime - sub.StartTime).TotalMilliseconds
                : double.MaxValue;
            var cleanText = Utilities.RemoveSsaTags(sub.Text)
                .Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Trim();
            if (string.IsNullOrEmpty(cleanText))
            {
                result.Add(sub);
                continue;
            }

            var chars = cleanText.ToCharArray();
            int charCount = chars.Length;
            double fallDuration = sub.Duration.TotalMilliseconds;

            var subRng = new Random(sub.Text.GetHashCode());
            // preRollMs: shift every animation backwards so each char arrives at restY at sub.StartTime.
            // restStartMs = fallDuration × (h×0.88 + 20) / (h + 40) for any revealFrac, so this is constant.
            double preRollMs = fallDuration * (h * 0.88 + 20.0) / (h + 40.0);
            var scrambled = new char[charCount];
            for (int k = 0; k < charCount; k++)
                scrambled[k] = chars[k] == ' ' ? ' ' : MatrixPool[subRng.Next(MatrixPool.Length)];

            int startCol = Math.Max(0, (cols - charCount) / 2);
            int visibleCount = chars.Count(c => c != ' ');

            // Reveal wave spans from 55 % to 80 % of the fall — lower-screen region
            const double revealFracStart = 0.55;
            const double revealFracEnd = 0.80;

            int visIdx = 0;
            for (int i = 0; i < charCount; i++)
            {
                int colIdx = Math.Min(startCol + i, cols - 1);
                int charX = colIdx * colW + colW / 2;

                if (chars[i] == ' ') continue;

                double revealFrac = visibleCount > 1
                    ? revealFracStart + (revealFracEnd - revealFracStart) * visIdx / (visibleCount - 1)
                    : (revealFracStart + revealFracEnd) / 2.0;

                double charRevealMs = fallDuration * revealFrac;
                int revealY = (int)(-20 + (h + 40) * revealFrac); // Y exactly at the reveal moment

                // Part 1: scrambled green char falls from top to revealY (starts before sub.StartTime)
                var falling = new SubtitleLineViewModel(sub, generateNewId: true);
                falling.StartTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(-preRollMs));
                falling.EndTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(charRevealMs - preRollMs));
                falling.Text = $"{{\\an5\\1c{BrightGreen}\\blur3\\bord0\\shad0\\fs{fs}" +
                               $"\\move({charX},-20,{charX},{revealY})}}" + scrambled[i];
                result.Add(falling);

                // Part 2: actual char falls from revealY down to restY (with flash on reveal)
                int restY = (int)(h * 0.88); // resting line — near bottom, subtitle-style
                double fallToRestMs = (restY - revealY) * fallDuration / (h + 40.0);
                double restStartMs = charRevealMs + fallToRestMs;

                if (fallToRestMs > 0)
                {
                    var revealed = new SubtitleLineViewModel(sub, generateNewId: true);
                    revealed.StartTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(charRevealMs - preRollMs));
                    revealed.EndTime = sub.StartTime; // arrives at restY exactly at subtitle start time
                    int flashDur = Math.Min(300, (int)fallToRestMs);
                    revealed.Text = $"{{\\an5\\1c{HeadColor}\\blur3\\bord1\\shad0\\fs{fs}" +
                                    $"\\t(0,{flashDur},\\1c&HFFFFFF&\\blur1)" +
                                    $"\\move({charX},{revealY},{charX},{restY})}}" + chars[i];
                    result.Add(revealed);
                }

                // Part 3: char rests at restY until subtitle ends so the text is readable
                if (restStartMs < fallDuration)
                {
                    var resting = new SubtitleLineViewModel(sub, generateNewId: true);
                    resting.StartTime = sub.StartTime; // all chars land at restY at subtitle start time
                    resting.EndTime = sub.StartTime.Add(TimeSpan.FromMilliseconds(
                        Math.Min(2000, nextSubOffsetMs)));
                    resting.Text = $"{{\\an5\\pos({charX},{restY})\\1c&HFFFFFF&\\blur0.5\\bord1\\shad0\\fs{fs}}}" + chars[i];
                    result.Add(resting);
                }

                visIdx++;
            }
        }

        return result;
    }
}

