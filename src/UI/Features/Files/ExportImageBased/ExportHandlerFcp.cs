using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerFcp : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.Fcp;
    public string Extension => "";
    public bool UseFileName => false;
    public string Title => string.Format(Se.Language.General.ExportToX, "FCP/image");
    public double FrameRate { get; set; } = 25.0;

    private string _folderName = string.Empty;
    private string _prefix = string.Empty;
    private StringBuilder _sb = new StringBuilder();
    private int _imagesSavedCount = 0;
    private TimeSpan _endTime;
    private int _width = 1920;
    private int _height = 1080;

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _folderName = fileOrFolderName;
        if (!Directory.Exists(_folderName))
        {
            Directory.CreateDirectory(_folderName);
        }

        _prefix = Guid.NewGuid().ToString();
        _sb.Clear();
        _imagesSavedCount = 0;
        _width = imageParameter.ScreenWidth;
        _height = imageParameter.ScreenHeight;
    }

    public void CreateParagraph(ImageParameter param)
    {

    }

    public void WriteParagraph(ImageParameter param)
    {
        _endTime = param.EndTime;
        _imagesSavedCount++;

        var numberString = string.Format(_prefix + "{0:0000}", _imagesSavedCount).RemoveChar(' ');
        var fileNameShort = numberString + ".png";
        var targetImageFileName = Path.Combine(_folderName, fileNameShort);
        var fileNameNoPath = Path.GetFileName(fileNameShort);
        var fileNameNoExt = Path.GetFileNameWithoutExtension(fileNameNoPath);
        var pathUrl = "file://localhost/" + targetImageFileName.Replace("\\", "/").Replace(" ", "%20");
        //if (!checkBoxFcpFullPathUrl.Checked)
        //{
        pathUrl = fileNameShort;
        //}

        var template = "          <clipitem id=\"" + System.Security.SecurityElement.Escape(fileNameNoPath) + "\">" + Environment.NewLine +
@"            <name>" + System.Security.SecurityElement.Escape(fileNameNoPath) + @"</name>
            <duration>[DURATION]</duration>
            <rate>
              <timebase>[TIMEBASE]</timebase>
              <ntsc>[NTSC]</ntsc>
            </rate>
            <in>[IN]</in>
            <out>[OUT]</out>
            <start>[START]</start>
            <end>[END]</end>
            <pixelaspectratio>" + param.ScreenWidth + "x" + param.ScreenHeight + @"</pixelaspectratio>
            <stillframe>TRUE</stillframe>
            <anamorphic>FALSE</anamorphic>
            <alphatype>straight</alphatype>
            <masterclipid>" + System.Security.SecurityElement.Escape(fileNameNoPath) + @"1</masterclipid>" + Environment.NewLine +
                          "            <file id=\"" + fileNameNoExt + "\">" + @"
              <name>" + System.Security.SecurityElement.Escape(fileNameNoPath) + @"</name>
              <pathurl>" + pathUrl + @"</pathurl>
              <rate>
                <timebase>[TIMEBASE]</timebase>
                <ntsc>[NTSC]</ntsc>
              </rate>
              <duration>[DURATION]</duration>
              <width>" + param.ScreenWidth + @"</width>
              <height>" + param.ScreenHeight + @"</height>
              <media>
                <video>
                  <duration>[DURATION]</duration>
                  <stillframe>TRUE</stillframe>
                  <samplecharacteristics>
                    <width>" + param.ScreenWidth + @"</width>
                    <height>" + param.ScreenHeight + @"</height>
                  </samplecharacteristics>
                </video>
              </media>
            </file>
            <sourcetrack>
              <mediatype>video</mediatype>
            </sourcetrack>
            <fielddominance>none</fielddominance>
          </clipitem>";

        var outBitmap = param.Bitmap;
        //if (checkBoxFullFrameImage.Checked)
        //{
        //    var nbmp = new NikseBitmap(param.Bitmap);
        //    nbmp.ReplaceTransparentWith(panelFullFrameBackground.BackColor);
        //    using (var bmp = nbmp.GetBitmap())
        //    {
        //        int top = param.ScreenHeight - (param.Bitmap.Height + param.BottomMargin);
        //        int left = (param.ScreenWidth - param.Bitmap.Width) / 2;

        //        var b = new NikseBitmap(param.ScreenWidth, param.ScreenHeight);
        //        {
        //            b.Fill(panelFullFrameBackground.BackColor);
        //            outBitmap = b.GetBitmap();
        //            {
        //                if (param.Alignment == ContentAlignment.BottomLeft || param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.TopLeft)
        //                {
        //                    left = param.LeftMargin;
        //                }
        //                else if (param.Alignment == ContentAlignment.BottomRight || param.Alignment == ContentAlignment.MiddleRight || param.Alignment == ContentAlignment.TopRight)
        //                {
        //                    left = param.ScreenWidth - param.Bitmap.Width - param.RightMargin;
        //                }

        //                if (param.Alignment == ContentAlignment.TopLeft || param.Alignment == ContentAlignment.TopCenter || param.Alignment == ContentAlignment.TopRight)
        //                {
        //                    top = param.BottomMargin;
        //                }

        //                if (param.Alignment == ContentAlignment.MiddleLeft || param.Alignment == ContentAlignment.MiddleCenter || param.Alignment == ContentAlignment.MiddleRight)
        //                {
        //                    top = (param.ScreenHeight - param.Bitmap.Height) / 2;
        //                }

        //                if (param.OverridePosition.HasValue &&
        //                    param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.Bitmap.Width &&
        //                    param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.Bitmap.Height)
        //                {
        //                    left = param.OverridePosition.Value.X;
        //                    top = param.OverridePosition.Value.Y;
        //                }

        //                using (var g = Graphics.FromImage(outBitmap))
        //                {
        //                    g.DrawImage(bmp, left, top);
        //                    g.Dispose();
        //                }
        //            }
        //        }
        //    }
        //}


        File.WriteAllBytes(targetImageFileName, param.Bitmap.ToPngArray());

        //if (comboBoxImageFormat.Text == "8-bit png")
        //{
        //    foreach (var encoder in ImageCodecInfo.GetImageEncoders())
        //    {
        //        if (encoder.FormatID == ImageFormat.Png.Guid)
        //        {
        //            var parameters = new EncoderParameters();
        //            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, 8);

        //            var nbmp = new NikseBitmap(outBitmap);
        //            var b = nbmp.ConvertTo8BitsPerPixel();
        //            b.Save(targetImageFileName, encoder, parameters);
        //            b.Dispose();

        //            break;
        //        }
        //    }
        //}
        //else
        //{
        //    SaveImage(outBitmap, targetImageFileName, ImageFormat);
        //}

        var timeBase = 25;
        var ntsc = "FALSE";
        //if (comboBoxLanguage.SelectedItem.ToString().Equals("NTSC", StringComparison.Ordinal))
        //{
        //    ntsc = "TRUE";
        //}

        //if (Math.Abs(param.FramesPerSeconds - 29.97) < 0.01)
        //{
        //    timeBase = 30;
        //    ntsc = "TRUE";
        //}
        //else if (Math.Abs(param.FramesPerSeconds - 23.976) < 0.01)
        //{
        //    timeBase = 24;
        //    ntsc = "TRUE";
        //}
        //else if (Math.Abs(param.FramesPerSeconds - 59.94) < 0.01)
        //{
        //    timeBase = 60;
        //    ntsc = "TRUE";
        //}

        var duration = SubtitleFormat.MillisecondsToFrames(param.EndTime.TotalMilliseconds - param.StartTime.TotalMilliseconds, FrameRate);
        var start = SubtitleFormat.MillisecondsToFrames(param.StartTime.TotalMilliseconds, FrameRate);
        var end = SubtitleFormat.MillisecondsToFrames(param.EndTime.TotalMilliseconds, FrameRate);

        template = template.Replace("[DURATION]", duration.ToString(CultureInfo.InvariantCulture));
        template = template.Replace("[IN]", start.ToString(CultureInfo.InvariantCulture));
        template = template.Replace("[OUT]", end.ToString(CultureInfo.InvariantCulture));
        template = template.Replace("[START]", start.ToString(CultureInfo.InvariantCulture));
        template = template.Replace("[END]", end.ToString(CultureInfo.InvariantCulture));
        template = template.Replace("[TIMEBASE]", timeBase.ToString(CultureInfo.InvariantCulture));
        template = template.Replace("[NTSC]", ntsc);
        _sb.AppendLine(template);
    }

    public void WriteFooter()
    {
        var duration = 0;
        if (_imagesSavedCount > 0)
        {
            duration = (int)Math.Round(_endTime.TotalSeconds * FrameRate);
        }

        var s = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + Environment.NewLine +
                   "<!DOCTYPE xmeml[]>" + Environment.NewLine +
                   "<xmeml version=\"4\">" + Environment.NewLine +
                   "  <sequence id=\"" + System.Security.SecurityElement.Escape(_prefix) + "\">" + Environment.NewLine +
                   "    <updatebehavior>add</updatebehavior>" + Environment.NewLine +
                   "    <name>" + System.Security.SecurityElement.Escape(_prefix) + @"</name>
    <duration>" + duration.ToString(CultureInfo.InvariantCulture) + @"</duration>
    <rate>
      <ntsc>FALSE</ntsc>
      <timebase>25</timebase>
    </rate>
    <timecode>
      <rate>
        <ntsc>FALSE</ntsc>
        <timebase>25</timebase>
      </rate>
      <string>00:00:00:00</string>
      <frame>0</frame>
      <source>source</source>
      <displayformat>NDF</displayformat>
    </timecode>
    <in>0</in>
    <out>[OUT]</out>
    <media>
      <video>
        <format>
          <samplecharacteristics>
            <rate>
              <timebase>25</timebase>
              <ntsc>FALSE</ntsc>
            </rate>
            <width>1920</width>
            <height>1080</height>
            <anamorphic>FALSE</anamorphic>
            <pixelaspectratio>square</pixelaspectratio>
            <fielddominance>none</fielddominance>
            <colordepth>32</colordepth>
          </samplecharacteristics>
        </format>
        <track>
          <enabled>TRUE</enabled>
          <locked>FALSE</locked>
        </track>
        <track>
" + _sb + @"   <enabled>TRUE</enabled>
          <locked>FALSE</locked>
        </track>
      </video>
      <audio>
        <track>
          <enabled>TRUE</enabled>
          <locked>FALSE</locked>
          <outputchannelindex>1</outputchannelindex>
        </track>
        <track>
          <enabled>TRUE</enabled>
          <locked>FALSE</locked>
          <outputchannelindex>2</outputchannelindex>
        </track>
        <track>
          <enabled>TRUE</enabled>
          <locked>FALSE</locked>
          <outputchannelindex>3</outputchannelindex>
        </track>
        <track>
          <enabled>TRUE</enabled>
          <locked>FALSE</locked>
          <outputchannelindex>4</outputchannelindex>
        </track>
      </audio>
    </media>
    <ismasterclip>FALSE</ismasterclip>
  </sequence>
</xmeml>";
        if (FrameRate == 29.97)
        {
            s = s.Replace("<displayformat>NDF</displayformat>", "<displayformat>DF</displayformat>"); //Non Drop Frame or Drop Frame
            s = s.Replace("<timebase>25</timebase>", "<timebase>30</timebase>");
            s = s.Replace("<ntsc>FALSE</ntsc>", "<ntsc>TRUE</ntsc>");
        }
        else if (FrameRate == 23.976)
        {
            s = s.Replace("<displayformat>NDF</displayformat>", "<displayformat>DF</displayformat>"); //Non Drop Frame or Drop Frame
            s = s.Replace("<timebase>25</timebase>", "<timebase>24</timebase>");
            s = s.Replace("<ntsc>FALSE</ntsc>", "<ntsc>TRUE</ntsc>");
        }
        else if (FrameRate == 59.94)
        {
            s = s.Replace("<displayformat>NDF</displayformat>", "<displayformat>DF</displayformat>"); //Non Drop Frame or Drop Frame
            s = s.Replace("<timebase>25</timebase>", "<timebase>60</timebase>");
            s = s.Replace("<ntsc>FALSE</ntsc>", "<ntsc>TRUE</ntsc>");
        }
        else
        {
            s = s.Replace("<timebase>25</timebase>", "<timebase>" + FrameRate.ToString(CultureInfo.InvariantCulture) + "</timebase>");
        }

        if (_imagesSavedCount > 0)
        {
            var end = SubtitleFormat.MillisecondsToFrames(_endTime.TotalMilliseconds, FrameRate);
            end++;
            s = s.Replace("[OUT]", end.ToString(CultureInfo.InvariantCulture));
        }

        //if (comboBoxLanguage.Text == "NTSC")
        //{
        //    s = s.Replace("<ntsc>FALSE</ntsc>", "<ntsc>TRUE</ntsc>");
        //}

        s = s.Replace("<width>1920</width>", "<width>" + _width.ToString(CultureInfo.InvariantCulture) + "</width>");
        s = s.Replace("<height>1080</height>", "<height>" + _height.ToString(CultureInfo.InvariantCulture) + "</height>");

        //if (comboBoxImageFormat.Text.Contains("8-bit"))
        //{
        //    s = s.Replace("<colordepth>32</colordepth>", "<colordepth>8</colordepth>");
        //}

        var fileName = Path.Combine(_folderName, "fcpxml_export.xml");
        File.WriteAllText(fileName, s);
    }
}