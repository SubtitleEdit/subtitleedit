using Nikse.SubtitleEdit.Core.ContainerFormats.MaterialExchangeFormat;

namespace Nikse.SubtitleEdit.UiLogic.Export;

/// <summary>
/// D-Cinema SMPTE 2014 image subtitles as one SMPTE ST 429-5 timed text track file: the
/// index.xml of <see cref="ExportHandlerDCinemaSmpte2014Png"/> with each PNG embedded as an
/// ancillary resource under its urn:uuid - the subtitle asset of a SMPTE DCP.
/// </summary>
public class ExportHandlerDCinemaSmpte2014Mxf : ExportHandlerDCinemaSmpte2014Png
{
    public override string Extension => ".mxf";
    public override bool UseFileName => true;
    public override string Title => string.Format("Export to {0}", "DCinema SMPTE 2014 (MXF)");

    /// <summary>
    /// The reel (picture) length - every asset of a DCP reel must have it, so the track runs to
    /// it instead of ending at the last subtitle. Null = until the last subtitle.
    /// </summary>
    public TimeSpan? ReelDuration { get; set; }

    /// <summary>
    /// The reel length as a picture frame count (e.g. a time code typed by the user) - takes
    /// precedence over <see cref="ReelDuration"/>.
    /// </summary>
    public long? ReelFrames { get; set; }

    private string _fileName = string.Empty;
    private readonly List<TimedTextMxfResource> _images = new();

    protected override void PrepareOutput(string fileOrFolderName)
    {
        _fileName = fileOrFolderName;
        _images.Clear();
    }

    protected override void SaveImage(string uuid, byte[] png)
    {
        _images.Add(new TimedTextMxfResource(Guid.Parse(uuid), TimedTextMxfResource.MimeTypePng, png));
    }

    protected override void SaveXml(string xml)
    {
        var writer = new TimedTextMxfWriter();
        if (ReelFrames is { } reelFrames && reelFrames > 0)
        {
            writer.ContainerDuration = reelFrames;
        }
        else if (ReelDuration is { TotalMilliseconds: > 0 } reelDuration)
        {
            writer.ContainerDuration = GetReelFrames(reelDuration, FrameRate);
        }

        // Throws (before the file is created) when there are more images than
        // TimedTextMxfWriter.MaxResources - one track file cannot hold them.
        writer.Resources.AddRange(_images);
        writer.Write(_fileName, xml);
        _images.Clear();
    }

    /// <summary>
    /// The picture frame count of a reel from its real-time length: a 23.976 video plays as 24
    /// frames per second in a DCP, but it has duration x 23.976 frames (1 h = 86314, not 86400),
    /// so count with the actual (fractional) rate, not the integer edit rate.
    /// </summary>
    public static long GetReelFrames(TimeSpan duration, double frameRate)
    {
        if (frameRate <= 0)
        {
            frameRate = 24;
        }

        return (long)Math.Round(duration.TotalSeconds * frameRate, MidpointRounding.AwayFromZero);
    }
}
