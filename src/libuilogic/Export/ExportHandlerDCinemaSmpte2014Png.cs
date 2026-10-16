using Nikse.SubtitleEdit.Core.Common;
using System.Globalization;
using System.Text;
using System.Xml;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.UiLogic.Export;

public class ExportHandlerDCinemaSmpte2014Png : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.DCinemaPng;
    public virtual string Extension => "";
    public virtual bool UseFileName => false;
    public virtual string Title => string.Format("Export to {0}", "DCinema SMPTE 2014/png");
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

        // Nothing sets the FrameRate property, so it stayed at the 23.976 default and every
        // index.xml declared EditRate/TimeCodeRate 23 - not a valid D-Cinema rate, and out of step
        // with the TimeIn/TimeOut values, which come from the real current frame rate. The Dost and
        // FCP handlers already take it from the image parameters; this one was missed.
        if (imageParameter.FramesPerSecond > 0)
        {
            FrameRate = imageParameter.FramesPerSecond;
        }

        PrepareOutput(fileOrFolderName);
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
        SaveImage(uuidString, param.Bitmap.ToPngArray());

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
        // The export's 3D depth: stereoscopic cinema places the image in depth by its Z-position.
        if (param.Depth3D == 0)
        {
            _sb.AppendLine("<Image Vposition=\"" + vPos + "\" Hposition=\"" + hPos + "\" Valign=\"" + verticalAlignment + "\" Halign=\"" + horizontalAlignment + "\">urn:uuid:" +
                          uuidString + "</Image>");
        }
        else
        {
            _sb.AppendLine("<Image Vposition=\"" + vPos + "\" Hposition=\"" + hPos + "\" Zposition=\"" + param.Depth3D.ToString(System.Globalization.CultureInfo.InvariantCulture) +
                          "\" Valign=\"" + verticalAlignment + "\" Halign=\"" + horizontalAlignment + "\">urn:uuid:" + uuidString + "</Image>");
        }

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
            // xs:dateTime without fractions or zone - DCP-o-matic's verifier warns on "o" (Bv2.1)
            "  <IssueDate>" + DateTime.Now.ToString("s") + "</IssueDate>" + Environment.NewLine +
            "  <ReelNumber>1</ReelNumber>" + Environment.NewLine +
            "  <Language>en</Language>" + Environment.NewLine +
            "  <EditRate>[FRAMERATE] 1</EditRate>" + Environment.NewLine +
            "  <TimeCodeRate>[FRAMERATE]</TimeCodeRate>" + Environment.NewLine +
            "  <StartTime>00:00:00:00</StartTime>" + Environment.NewLine +
            "  <SubtitleList>" + Environment.NewLine +
            _sb +
            "  </SubtitleList>" + Environment.NewLine +
            "</SubtitleReel>";

        // Round, do not truncate: (int)23.976 is 23, which no D-Cinema player accepts.
        xml = xml.Replace("[FRAMERATE]", ((int)Math.Round(FrameRate, MidpointRounding.AwayFromZero)).ToString(CultureInfo.InvariantCulture));

        doc.LoadXml(xml);
        // "UTF-8" like the text D-Cinema formats - ClairMeta rejects the lowercase declaration
        SaveXml(SubtitleFormat.ToUtf8XmlString(doc).Replace("encoding=\"utf-8\"", "encoding=\"UTF-8\""));
    }

    /// <summary>
    /// The PNGs and index.xml go into a folder; see <see cref="ExportHandlerDCinemaSmpte2014Mxf"/>
    /// for the same subtitles wrapped in one MXF track file.
    /// </summary>
    protected virtual void PrepareOutput(string fileOrFolderName)
    {
        _folderName = fileOrFolderName;
        if (!Directory.Exists(_folderName))
        {
            Directory.CreateDirectory(_folderName);
        }
    }

    protected virtual void SaveImage(string uuid, byte[] png)
    {
        File.WriteAllBytes(Path.Combine(_folderName, uuid + ".png"), png);
    }

    protected virtual void SaveXml(string xml)
    {
        File.WriteAllText(Path.Combine(_folderName, "index.xml"), xml);
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