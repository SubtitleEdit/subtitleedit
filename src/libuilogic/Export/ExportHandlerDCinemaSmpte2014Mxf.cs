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
        writer.Resources.AddRange(_images);
        writer.Write(_fileName, xml);
        _images.Clear();
    }
}
