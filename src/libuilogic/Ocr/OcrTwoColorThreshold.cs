namespace Nikse.SubtitleEdit.UiLogic.Ocr;

/// <summary>
/// Picks the brightness threshold (sum of R+G+B) for <see cref="NikseBitmap2.MakeTwoColor(int)"/>
/// before matching. The usual 200 keeps a white or yellow fill and drops a black outline, but a
/// grey drop shadow ("3D" subtitles) sums to 190-290 and fuses with the letters, so whole words
/// came out as "*". When a large share of what 200 would keep is mid-grey that Otsu's method
/// puts on the dark side, use Otsu's threshold instead. Normal subtitles keep 200: there only
/// anti-aliased edge pixels fall in that band (under 10% on real discs, against over 30% for
/// shadowed text), and a higher threshold would thin the glyphs compared to the database.
/// </summary>
public static class OcrTwoColorThreshold
{
    public const int Default = 200;
    private const double ShadowShare = 0.2;

    public static int Get(NikseBitmap2 bitmap)
    {
        var data = bitmap.GetPixelData();
        var histogram = new double[766];
        for (var i = 0; i + 3 < data.Length; i += 4)
        {
            var alpha = data[i + 3];
            if (alpha == 0)
            {
                continue;
            }

            histogram[data[i] + data[i + 1] + data[i + 2]] += alpha / 255.0;
        }

        var otsu = GetOtsuThreshold(histogram);
        if (otsu <= Default)
        {
            return Default;
        }

        double kept = 0;
        double shadow = 0;
        for (var i = Default; i < histogram.Length; i++)
        {
            kept += histogram[i];
            if (i < otsu)
            {
                shadow += histogram[i];
            }
        }

        return kept > 0 && shadow / kept > ShadowShare ? otsu : Default;
    }

    private static int GetOtsuThreshold(double[] histogram)
    {
        double total = 0;
        double sumAll = 0;
        for (var i = 0; i < histogram.Length; i++)
        {
            total += histogram[i];
            sumAll += i * histogram[i];
        }

        double weightBackground = 0;
        double sumBackground = 0;
        var bestVariance = -1.0;
        var threshold = Default;
        for (var i = 0; i < histogram.Length; i++)
        {
            weightBackground += histogram[i];
            if (weightBackground == 0)
            {
                continue;
            }

            var weightForeground = total - weightBackground;
            if (weightForeground <= 0)
            {
                break;
            }

            sumBackground += i * histogram[i];
            var meanBackground = sumBackground / weightBackground;
            var meanForeground = (sumAll - sumBackground) / weightForeground;
            var variance = weightBackground * weightForeground * (meanBackground - meanForeground) * (meanBackground - meanForeground);
            if (variance > bestVariance)
            {
                bestVariance = variance;
                threshold = i + 1;
            }
        }

        return threshold;
    }
}
