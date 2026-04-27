using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerDCinemaSmpte2014Png : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.DCinemaPng;
    public string Extension => "";
    public bool UseFileName => false;
    public string Title => string.Format(Se.Language.General.ExportToX, "DCinema SMPTE 2014/png");
    public double FrameRate { get; set; } = 23.976;

    private int _width;
    private int _height;
    private StringBuilder _sb = new StringBuilder();
    private string _folderName = string.Empty;
    private int _imagesSavedCount = 0;

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
        // string numberString = $"{i:0000}";
        _imagesSavedCount++;
        string uuidString = Guid.NewGuid().ToString().RemoveChar('-').Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
        string fileName = Path.Combine(_folderName, uuidString + ".png");
        File.WriteAllBytes(fileName, param.Bitmap.ToPngArray());

        string verticalAlignment = "bottom";
        string horizontalAlignment = "center";
        string vPos = "9.7";
        string hPos = "0";

        switch (param.Alignment)
        {
            case ExportAlignment.BottomLeft:
                verticalAlignment = "bottom";
                horizontalAlignment = "left";
                hPos = "10";
                break;
            case ExportAlignment.BottomRight:
                verticalAlignment = "bottom";
                horizontalAlignment = "right";
                hPos = "10";
                break;
            case ExportAlignment.MiddleCenter:
                verticalAlignment = "center";
                vPos = "0";
                break;
            case ExportAlignment.MiddleLeft:
                verticalAlignment = "center";
                horizontalAlignment = "left";
                hPos = "10";
                vPos = "0";
                break;
            case ExportAlignment.MiddleRight:
                verticalAlignment = "center";
                horizontalAlignment = "right";
                hPos = "10";
                vPos = "0";
                break;
            case ExportAlignment.TopCenter:
                verticalAlignment = "top";
                break;
            case ExportAlignment.TopLeft:
                verticalAlignment = "top";
                horizontalAlignment = "left";
                hPos = "10";
                break;
            case ExportAlignment.TopRight:
                verticalAlignment = "top";
                horizontalAlignment = "right";
                hPos = "10";
                break;
        }

        _sb.AppendLine("<Subtitle SpotNumber=\"" + _imagesSavedCount + "\" FadeUpTime=\"" + "00:00:00:00" + "\" FadeDownTime=\"" + "00:00:00:00" + "\" TimeIn=\"" +
                      new TimeCode(param.StartTime).ToHHMMSSFF() + "\" TimeOut=\"" + new TimeCode( param.EndTime).ToHHMMSSFF() + "\">");
        // if (param.Depth3D == 0)
        {
            _sb.AppendLine("<Image Vposition=\"" + vPos + "\" Hposition=\"" + hPos + "\" Valign=\"" + verticalAlignment + "\" Halign=\"" + horizontalAlignment + "\">urn:uuid:" +
                          uuidString + "</Image>");
        }
        // else
        // {
        //     _sb.AppendLine("<Image Vposition=\"" + vPos + "\" Hposition=\"" + hPos + "\" Zposition=\"" + param.Depth3D + "\" Valign=\"" + verticalAlignment + "\" Halign=\"" +
        //                   horizontalAlignment + "\">urn:uuid:" + uuidString + "</Image>");
        // }

       _sb.AppendLine("</Subtitle>");
    }

    public void WriteFooter()
    {
        var doc = new XmlDocument();
        var guid = Guid.NewGuid().ToString().RemoveChar('-').Insert(8, "-").Insert(13, "-").Insert(18, "-").Insert(23, "-");
        var xml =
            "<SubtitleReel xmlns=\"http://www.smpte-ra.org/schemas/428-7/2014/DCST\">" + Environment.NewLine +
            "  <Id>urn:uuid:" + guid + "</Id>" + Environment.NewLine +
            "  <ContentTitleText>Movie Title</ContentTitleText>" + Environment.NewLine +
            "  <AnnotationText>This is a subtitle file</AnnotationText>" + Environment.NewLine +
            "  <IssueDate>" + DateTime.Now.ToString("o") + "</IssueDate>" + Environment.NewLine +
            "  <ReelNumber>1</ReelNumber>" + Environment.NewLine +
            "  <Language>en</Language>" + Environment.NewLine +
            "  <EditRate>[FRAMERATE] 1</EditRate>" + Environment.NewLine +
            "  <TimeCodeRate>[FRAMERATE]</TimeCodeRate>" + Environment.NewLine +
            "  <StartTime>00:00:00:00</StartTime>" + Environment.NewLine +
            "  <SubtitleList>" + Environment.NewLine +
            _sb +
            "  </SubtitleList>" + Environment.NewLine +
            "</SubtitleReel>";

        xml = xml.Replace("[FRAMERATE]", ((int)FrameRate).ToString(CultureInfo.InvariantCulture));

        doc.LoadXml(xml);
        var fName = Path.Combine(_folderName, "index.xml");

        File.WriteAllText(fName, SubtitleFormat.ToUtf8XmlString(doc));
    }

    private static string FormatUtf8Xml(XmlDocument doc)
    {
        var sb = new StringBuilder();
        using (var writer = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8 }))
        {
            doc.Save(writer);
        }

        return
            sb.ToString().Replace(" encoding=\"utf-16\"", " encoding=\"utf-8\"")
                .Trim(); // "replace hack" due to missing encoding (encoding only works if it's the only parameter...)
    }
}