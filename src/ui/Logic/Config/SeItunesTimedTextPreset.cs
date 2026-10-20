using System;
using System.IO;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic.Config;

/// <summary>
/// A named iTunes Timed Text export setup (File > Properties, #16257): everything the properties
/// window holds except the per-file title/description, so a preset gives every saved .itt the
/// same header whatever the source file was.
/// </summary>
public class SeItunesTimedTextPreset
{
    public const string HeaderSourceCurrentFile = "CurrentFile";
    public const string HeaderSourceTemplateFile = "TemplateFile";

    public string Name { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string TimeBase { get; set; } = string.Empty;
    public string FrameRate { get; set; } = string.Empty;
    public string FrameRateMultiplier { get; set; } = string.Empty;
    public string DropMode { get; set; } = string.Empty;
    public string DefaultStyle { get; set; } = string.Empty;
    public string DefaultRegion { get; set; } = string.Empty;
    public string DefaultColor { get; set; } = "white";
    public string StyleAttribute { get; set; } = "tts:fontStyle";
    public string TimeCodeFormat { get; set; } = "Frames";
    public string TopOrigin { get; set; } = string.Empty;
    public string TopExtent { get; set; } = string.Empty;
    public string BottomOrigin { get; set; } = string.Empty;
    public string BottomExtent { get; set; } = string.Empty;
    public string HeaderSource { get; set; } = HeaderSourceCurrentFile;
    public string TemplateFile { get; set; } = string.Empty;

    /// <summary>
    /// The template header libse gets: the template file's text when the header source is a
    /// template file that exists and parses as TTML, otherwise empty (= use the source header).
    /// libse never reads files itself.
    /// </summary>
    public static string LoadHeaderTemplate(SeFormats formats)
    {
        if (formats.TimedTextItunesHeaderSource != HeaderSourceTemplateFile ||
            string.IsNullOrWhiteSpace(formats.TimedTextItunesTemplateFile) ||
            !File.Exists(formats.TimedTextItunesTemplateFile))
        {
            return string.Empty;
        }

        try
        {
            var text = File.ReadAllText(formats.TimedTextItunesTemplateFile);
            return ItunesTimedText.LoadHeaderTemplate(text) == null ? string.Empty : text;
        }
        catch (Exception exception)
        {
            SeLogger.Error(exception, "Could not read iTunes Timed Text template " + formats.TimedTextItunesTemplateFile);
            return string.Empty;
        }
    }
}
