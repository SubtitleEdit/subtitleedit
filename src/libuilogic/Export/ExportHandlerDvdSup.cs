using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;

namespace Nikse.SubtitleEdit.UiLogic.Export;

public class ExportHandlerDvdSup : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.DvdSup;
    public string Extension => ".sup";
    public bool UseFileName => true;
    public string Title => string.Format("Export to {0}", "DVD sup");

    private DvdSupWriter? _dvdSupWriter;

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _dvdSupWriter = new DvdSupWriter(fileOrFolderName, imageParameter.ScreenWidth, imageParameter.ScreenHeight, imageParameter.BottomTopMargin, imageParameter.LeftRightMargin, imageParameter.FontColor, imageParameter.OutlineColor, true);
    }

    public void CreateParagraph(ImageParameter param)
    {
    }

    public void WriteParagraph(ImageParameter param)
    {
        if (_dvdSupWriter == null)
        {
            throw new InvalidOperationException("DvdSupWriter is not initialized. Call WriteHeader first.");
        }

        var p = new Paragraph(param.Text, param.StartTime.TotalMilliseconds, param.EndTime.TotalMilliseconds);
        _dvdSupWriter.WriteParagraph(p, param.Bitmap, MapAlignment(param));
    }

    private static BluRayContentAlignment MapAlignment(ImageParameter param)
    {
        var alignment = BluRayContentAlignment.BottomCenter;
        switch (param.Alignment)
        {
            case ExportAlignment.BottomLeft:
                alignment = BluRayContentAlignment.BottomLeft;
                break;
            case ExportAlignment.BottomRight:
                alignment = BluRayContentAlignment.BottomRight;
                break;
            case ExportAlignment.MiddleLeft:
                alignment = BluRayContentAlignment.MiddleLeft;
                break;
            case ExportAlignment.MiddleRight:
                alignment = BluRayContentAlignment.MiddleRight;
                break;
            case ExportAlignment.TopLeft:
                alignment = BluRayContentAlignment.TopLeft;
                break;
            case ExportAlignment.TopRight:
                alignment = BluRayContentAlignment.TopRight;
                break;
            case ExportAlignment.TopCenter:
                alignment = BluRayContentAlignment.TopCenter;
                break;
            case ExportAlignment.MiddleCenter:
                alignment = BluRayContentAlignment.MiddleCenter;
                break;
            case ExportAlignment.BottomCenter:
                break;
        }
        return alignment;
    }

    public void WriteFooter()
    {
        if (_dvdSupWriter == null)
        {
            throw new InvalidOperationException("DvdSupWriter is not initialized. Call WriteHeader first.");
        }

        _dvdSupWriter.Dispose();
    }
}
