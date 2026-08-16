using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace SeConv.Core;

/// <summary>
/// Loads a subtitle file and produces a flat <see cref="SubtitleInfo"/> record:
/// path, size, detected format, encoding, paragraph count, total/first/last
/// timecodes, and detected language. Used by <c>seconv info</c>.
/// </summary>
internal static class SubtitleInfoGatherer
{
    public static SubtitleInfo Gather(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Subtitle file not found: {filePath}");
        }

        var fileSize = new FileInfo(filePath).Length;

        // Detect encoding before format so the displayed encoding is the *source*
        // encoding (not whatever the format used internally).
        var encoding = LanguageAutoDetect.GetEncodingFromFile(filePath);

        var (subtitle, format) = LibSEIntegration.LoadSubtitleWithFormat(filePath);

        long? firstStartMs = null;
        long? lastEndMs = null;
        if (subtitle.Paragraphs.Count > 0)
        {
            firstStartMs = (long)subtitle.Paragraphs[0].StartTime.TotalMilliseconds;
            lastEndMs = (long)subtitle.Paragraphs[^1].EndTime.TotalMilliseconds;
        }

        var language = subtitle.Paragraphs.Count > 0
            ? SubtitleLanguageDetector.DetectOrNull(subtitle)
            : null;

        return new SubtitleInfo
        {
            Path = filePath,
            FileSizeBytes = fileSize,
            Format = format.Name,
            Extension = format.Extension,
            Encoding = DescribeEncoding(encoding),
            ParagraphCount = subtitle.Paragraphs.Count,
            FirstStartMs = firstStartMs,
            LastEndMs = lastEndMs,
            DurationMs = firstStartMs.HasValue && lastEndMs.HasValue ? lastEndMs - firstStartMs : null,
            Language = language,
        };
    }

    private static string DescribeEncoding(System.Text.Encoding encoding)
    {
        var name = encoding.WebName;
        // Distinguish UTF-8 with BOM since that affects downstream tooling.
        if (encoding is System.Text.UTF8Encoding utf8)
        {
            var preamble = utf8.GetPreamble();
            return preamble.Length > 0 ? "utf-8 (with BOM)" : "utf-8";
        }
        return name;
    }
}

internal sealed record SubtitleInfo
{
    public required string Path { get; init; }
    public required long FileSizeBytes { get; init; }
    public required string Format { get; init; }
    public required string Extension { get; init; }
    public required string Encoding { get; init; }
    public required int ParagraphCount { get; init; }
    public long? FirstStartMs { get; init; }
    public long? LastEndMs { get; init; }
    public long? DurationMs { get; init; }
    public string? Language { get; init; }
}
