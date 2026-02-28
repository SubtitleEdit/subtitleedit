using Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.Config;

public class SeFile
{
    public bool ShowRecentFiles { get; set; } = true;
    public int RecentFilesMaximum { get; set; } = 25;
    public List<RecentFile> RecentFiles { get; set; } = new();
    public List<SeExportCustomFormatItem> ExportCustomFormats { get; set; } = new();
    public SeExportImages ExportImages { get; set; } = new();
    public SeExportPlainText ExportPlainText { get; set; } = new();
    public SeDCinemaSmpte DCinemaSmpte { get; set; } = new();

    public SeFile()
    {
        ExportCustomFormats.Add(new SeExportCustomFormatItem
        {
            Name = "SubRip",
            Extension = "srt",
            FormatHeader = string.Empty,
            FormatParagraph = "{number}" + Environment.NewLine + "{start} --> {end}" + Environment.NewLine + "{text}" + Environment.NewLine + Environment.NewLine,
            FormatFooter = string.Empty,
            FormatTimeCode = "hh:mm:ss,zzz",
            FormatNewLine = null,
        });
        ExportCustomFormats.Add(new SeExportCustomFormatItem
        {
            Name = "MicroDVD",
            Extension = "sub",
            FormatHeader = string.Empty,
            FormatParagraph = "{{start}}{{end}}{text}" + Environment.NewLine,
            FormatFooter = string.Empty,
            FormatTimeCode = "ff",
            FormatNewLine = "||",
        });
    }

    public void AddToRecentFiles(string subtitleFileName, string subtitleFileNameOriginal, string videoFileName, int selectedLine, string encoding, long VideoOffsetInMs, bool videoIsSmpte, int audioTrack)
    {
        RecentFiles.RemoveAll(rf => rf.SubtitleFileName == subtitleFileName && rf.SubtitleFileNameOriginal == subtitleFileNameOriginal);

        RecentFiles.Insert(0, new RecentFile
        {
            SubtitleFileName = subtitleFileName,
            SubtitleFileNameOriginal = subtitleFileNameOriginal,
            VideoFileName = videoFileName,
            SelectedLine = selectedLine,
            Encoding = encoding,
            VideoOffsetInMs = VideoOffsetInMs,
            VideoIsSmpte = videoIsSmpte,
            AudioTrack = audioTrack,
        });

        if (RecentFiles.Count > RecentFilesMaximum)
        {
            RecentFiles.RemoveAt(RecentFiles.Count - 1);
        }
    }
}