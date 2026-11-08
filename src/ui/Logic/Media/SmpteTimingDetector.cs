using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Opt-in SMPTE auto-detect (Settings > Video player, off by default): a Timed Text file that
/// says its time codes count frames on the non-drop SMPTE clock of an NTSC rate
/// (ttp:timeBase="smpte", ttp:frameRate 24/30/60 with ttp:frameRateMultiplier "1000 1001" or
/// "999 1000", and no drop-frame ttp:dropMode) has row times that run 0.1 % fast against the
/// video - exactly what SMPTE timing corrects.
/// </summary>
/// <remarks>
/// Drop-frame time codes are left alone: they skip frame numbers to stay on the wall clock, so
/// read nominally they already match the video. ttp:timeBase="media" files hold media time and
/// never need SMPTE timing, whatever their frame rate multiplier says.
/// </remarks>
public static class SmpteTimingDetector
{
    public static bool IsSmpteTimedText(string? header)
    {
        if (string.IsNullOrEmpty(header) || header.IndexOf("timeBase", StringComparison.Ordinal) < 0)
        {
            return false;
        }

        string? timeBase = null;
        string? frameRate = null;
        string? multiplier = null;
        string? dropMode = null;
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Ignore,
                XmlResolver = null,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true,
            };
            using var reader = XmlReader.Create(new StringReader(header), settings);
            if (reader.MoveToContent() != XmlNodeType.Element || reader.LocalName != "tt")
            {
                return false;
            }

            while (reader.MoveToNextAttribute())
            {
                switch (reader.LocalName)
                {
                    case "timeBase":
                        timeBase = reader.Value.Trim();
                        break;
                    case "frameRate":
                        frameRate = reader.Value.Trim();
                        break;
                    case "frameRateMultiplier":
                        multiplier = string.Join(" ", reader.Value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
                        break;
                    case "dropMode":
                        dropMode = reader.Value.Trim();
                        break;
                }
            }
        }
        catch (XmlException)
        {
            return false;
        }

        if (!string.Equals(timeBase, "smpte", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (multiplier != "1000 1001" && multiplier != "999 1000")
        {
            return false;
        }

        if (!double.TryParse(frameRate, NumberStyles.Float, CultureInfo.InvariantCulture, out var fps) ||
            (Math.Abs(fps - 24) > 0.01 && Math.Abs(fps - 30) > 0.01 && Math.Abs(fps - 60) > 0.01))
        {
            return false;
        }

        return string.IsNullOrEmpty(dropMode) || string.Equals(dropMode, "nonDrop", StringComparison.OrdinalIgnoreCase);
    }
}
