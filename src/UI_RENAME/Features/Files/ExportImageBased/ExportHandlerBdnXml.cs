using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerBdnXml : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.BdnXml;
    public string Extension => "";
    public bool UseFileName => false;
    public string Title => string.Format(Se.Language.General.ExportToX, "BDN XML");

    private int _width;
    private int _height;
    private StringBuilder _sb = new StringBuilder();
    private Paragraph? _first;
    private Paragraph? _last;
    private string _folderName = string.Empty;
    private int _imagesSavedCount = 0;
    private double _frameRate = 23.976;

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _width = imageParameter.ScreenWidth;
        _height = imageParameter.ScreenHeight;

        _folderName = fileOrFolderName;
        if (!Directory.Exists(_folderName))
        {
            Directory.CreateDirectory(_folderName);
        }

        _sb.Clear();
        _imagesSavedCount = 0;
    }

    public void CreateParagraph(ImageParameter param)
    {

    }

    public void WriteParagraph(ImageParameter param)
    {
        if (_first == null)
        {
            _first = new Paragraph(param.Text, param.StartTime.TotalMilliseconds, param.EndTime.TotalMilliseconds);
        }
        _last = new Paragraph(param.Text, param.StartTime.TotalMilliseconds, param.EndTime.TotalMilliseconds);
        _imagesSavedCount++;

        var numberString = $"{_imagesSavedCount:0000}";
        var fileName = Path.Combine(_folderName, numberString + ".png");

        File.WriteAllBytes(fileName, param.Bitmap.ToPngArray());

        _sb.AppendLine("<Event InTC=\"" + ToHHMMSSFF(new TimeCode(param.StartTime)) + "\" OutTC=\"" +
                                ToHHMMSSFF(new TimeCode(param.EndTime)) + "\" Forced=\"" + param.IsForced.ToString().ToLowerInvariant() + "\">");

        var x = (_width - param.Bitmap.Width) / 2;
        var y = _height - (param.Bitmap.Height + param.BottomTopMargin);
        var border = 0;
        switch (param.Alignment)
        {
            case ExportAlignment.BottomLeft:
                x = border;
                y = _height - (param.Bitmap.Height + param.BottomTopMargin);
                break;
            case ExportAlignment.BottomRight:
                x = _height - param.Bitmap.Width - border;
                y = _height - (param.Bitmap.Height + param.BottomTopMargin);
                break;
            case ExportAlignment.MiddleCenter:
                x = (_width - param.Bitmap.Width) / 2;
                y = (_height - param.Bitmap.Height) / 2;
                break;
            case ExportAlignment.MiddleLeft:
                x = border;
                y = (_height - param.Bitmap.Height) / 2;
                break;
            case ExportAlignment.MiddleRight:
                x = _width - param.Bitmap.Width - border;
                y = (_height - param.Bitmap.Height) / 2;
                break;
            case ExportAlignment.TopCenter:
                x = (_width - param.Bitmap.Width) / 2;
                y = border;
                break;
            case ExportAlignment.TopLeft:
                x = border;
                y = border;
                break;
            case ExportAlignment.TopRight:
                x = _width - param.Bitmap.Width - border;
                y = border;
                break;
        }

        if (param.OverridePosition.HasValue &&
            param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.Bitmap.Width &&
            param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.Bitmap.Height)
        {
            x = param.OverridePosition.Value.X;
            y = param.OverridePosition.Value.Y;
        }

        _sb.AppendLine("  <Graphic Width=\"" + param.Bitmap.Width.ToString(CultureInfo.InvariantCulture) + "\" Height=\"" +
                      param.Bitmap.Height.ToString(CultureInfo.InvariantCulture) + "\" X=\"" + x.ToString(CultureInfo.InvariantCulture) + "\" Y=\"" + y.ToString(CultureInfo.InvariantCulture) +
                      "\">" + numberString + ".png</Graphic>");
        _sb.AppendLine("</Event>");
    }

    public void WriteFooter()
    {
        var videoFormat = "1080p";
        if (_width == 1920 && _height == 1080)
        {
            videoFormat = "1080p";
        }
        else if (_width == 1280 && _height == 720)
        {
            videoFormat = "720p";
        }
        else if (_width == 848 && _height == 480)
        {
            videoFormat = "480p";
        }
        else if (_width > 0 && _height > 0)
        {
            videoFormat = _width + "x" + _height;
        }

        if (_first == null)
        {
            _first = new Paragraph(string.Empty, 0, 0);
        }

        if (_last == null)
        {
            _last = new Paragraph(string.Empty, 0, 0);
        }

        var doc = new XmlDocument();
        doc.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
                    "<BDN Version=\"0.93\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:noNamespaceSchemaLocation=\"BD-03-006-0093b BDN File Format.xsd\">" + Environment.NewLine +
                    "<Description>" + Environment.NewLine +
                    "<Name Title=\"subtitle_exp\" Content=\"\"/>" + Environment.NewLine +
                    "<Language Code=\"eng\"/>" + Environment.NewLine +
                    "<Format VideoFormat=\"" + videoFormat + "\" FrameRate=\"" + _frameRate.ToString(CultureInfo.InvariantCulture) + "\" DropFrame=\"false\"/>" + Environment.NewLine +
                    "<Events Type=\"Graphic\" FirstEventInTC=\"" + ToHHMMSSFF(_first.StartTime) + "\" LastEventOutTC=\"" + ToHHMMSSFF(_last.EndTime) + "\" NumberofEvents=\"" + _imagesSavedCount.ToString(CultureInfo.InvariantCulture) + "\"/>" + Environment.NewLine +
                    "</Description>" + Environment.NewLine +
                    "<Events>" + Environment.NewLine +
                    "</Events>" + Environment.NewLine +
                    "</BDN>");
        if (doc == null || doc.DocumentElement == null)
        {
            throw new InvalidOperationException("BDN XML document could not be created.");
        }

        XmlNode? events = doc.DocumentElement.SelectSingleNode("Events");
        doc.PreserveWhitespace = true;

        if (events == null)
        {
            throw new InvalidOperationException("BDN XML Events node not found.");
        }
        events.InnerXml = _sb.ToString();
        FileUtil.WriteAllTextWithDefaultUtf8(Path.Combine(_folderName, "index.xml"), FormatUtf8Xml(doc));
    }

    private static string FormatUtf8Xml(XmlDocument doc)
    {
        var sb = new StringBuilder();
        using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 }))
        {
            doc.Save(writer);
        }
        return sb.ToString().Replace(" encoding=\"utf-16\"", " encoding=\"utf-8\"").Trim(); // "replace hack" due to missing encoding (encoding only works if it's the only parameter...)
    }

    private string ToHHMMSSFF(TimeCode timeCode)
    {
        var ms = timeCode.TotalMilliseconds;
        if (!Configuration.Settings.General.CurrentVideoIsSmpte && ((decimal)_frameRate) % 1 != 0)
        {
            ms /= 1.001;
        }

        var tc = new TimeCode(ms);
        return $"{tc.Hours:00}:{tc.Minutes:00}:{tc.Seconds:00}:{MillisecondsToFramesMaxFrameRate(tc.Milliseconds):00}";
    }

    private int MillisecondsToFramesMaxFrameRate(double milliseconds)
    {
        int frames = (int)Math.Round(milliseconds / (TimeCode.BaseUnit / _frameRate));
        if (frames >= _frameRate)
        {
            frames = (int)(_frameRate - 0.01);
        }

        return frames;
    }
}