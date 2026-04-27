using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.VobSub;
using Nikse.SubtitleEdit.Logic.Config;
using System;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerVobSub : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.VobSub;
    public string Extension => ".sub";
    public bool UseFileName => true;    
    public string Title => string.Format(Se.Language.General.ExportToX, "VobSub/idx");

    private int _width;
    private int _height;
    VobSubWriter? _vobSubWriter;

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _width = imageParameter.ScreenWidth;
        _height = imageParameter.ScreenHeight;
        _vobSubWriter = new VobSubWriter(fileOrFolderName, _width, _height, imageParameter.BottomTopMargin, imageParameter.LeftRightMargin, 32, imageParameter.FontColor, imageParameter.OutlineColor, true, DvdSubtitleLanguage.English);
    }

    public void CreateParagraph(ImageParameter param)
    {
    }

    public void WriteParagraph(ImageParameter param)
    {
        if (_vobSubWriter == null)
        {
            throw new InvalidOperationException("VobSubWriter is not initialized. Call WriteHeader first.");
        }

        var p = new Paragraph(param.Text, param.StartTime.TotalMilliseconds, param.EndTime.TotalMilliseconds);
        var alignment = MapAlignment(param);

        _vobSubWriter.WriteParagraph(p, param.Bitmap, alignment);
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
        if (_vobSubWriter == null)
        {
            throw new InvalidOperationException("VobSubWriter is not initialized. Call WriteHeader first.");
        }

        _vobSubWriter.WriteIdxFile();
        _vobSubWriter.Dispose();
    }
}