using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using SkiaSharp;
using System.Globalization;
using System.Text;

namespace Nikse.SubtitleEdit.UiLogic.Export;

public class ExportHandlerFcp : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.Fcp;
    public string Extension => "";
    public bool UseFileName => false;
    public string Title => string.Format("Export to {0}", "Final Cut Pro + image");
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

        // Nothing sets the FrameRate property, so the chosen frame rate only ever reached this
        // handler through the image parameters - and every time code in the xmeml was written at
        // the 25 fps default. Take it from the parameters, like the BDN XML handler does.
        if (imageParameter.FramesPerSecond > 0)
        {
            FrameRate = imageParameter.FramesPerSecond;
        }
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
            <pixelaspectratio>square</pixelaspectratio>
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

        // "Full frame" writes the subtitle onto a frame-sized image, so every png can be dropped
        // on the timeline at 0,0 and still land where Subtitle Edit placed it.
        SKBitmap? fullFrameBitmap = null;
        if (param.IsFullFrame)
        {
            fullFrameBitmap = FullFrameImage.Create(param);
        }

        try
        {
            File.WriteAllBytes(targetImageFileName, (fullFrameBitmap ?? param.Bitmap).ToPngArray());
        }
        finally
        {
            fullFrameBitmap?.Dispose();
        }

        var (timeBase, ntsc) = GetTimeBaseAndNtsc();

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
      <ntsc>[NTSC]</ntsc>
      <timebase>[TIMEBASE]</timebase>
    </rate>
    <timecode>
      <rate>
        <ntsc>[NTSC]</ntsc>
        <timebase>[TIMEBASE]</timebase>
      </rate>
      <string>00:00:00:00</string>
      <frame>0</frame>
      <source>source</source>
      <displayformat>[DISPLAYFORMAT]</displayformat>
    </timecode>
    <in>0</in>
    <out>[OUT]</out>
    <media>
      <video>
        <format>
          <samplecharacteristics>
            <rate>
              <timebase>[TIMEBASE]</timebase>
              <ntsc>[NTSC]</ntsc>
            </rate>
            <width>[WIDTH]</width>
            <height>[HEIGHT]</height>
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
        // The clip items in _sb already have their own placeholders filled in, so these only
        // reach the sequence around them - the old code replaced whole "<timebase>25</timebase>"
        // elements, which reached into the clip items too.
        var (timeBase, ntsc) = GetTimeBaseAndNtsc();
        s = s.Replace("[TIMEBASE]", timeBase.ToString(CultureInfo.InvariantCulture));
        s = s.Replace("[NTSC]", ntsc);
        s = s.Replace("[DISPLAYFORMAT]", ntsc == "TRUE" ? "DF" : "NDF"); //Non Drop Frame or Drop Frame

        // The sequence was hardcoded to 1920x1080 no matter which resolution was exported.
        s = s.Replace("[WIDTH]", _width.ToString(CultureInfo.InvariantCulture));
        s = s.Replace("[HEIGHT]", _height.ToString(CultureInfo.InvariantCulture));

        var sequenceEnd = 0;
        if (_imagesSavedCount > 0)
        {
            sequenceEnd = SubtitleFormat.MillisecondsToFrames(_endTime.TotalMilliseconds, FrameRate) + 1;
        }

        s = s.Replace("[OUT]", sequenceEnd.ToString(CultureInfo.InvariantCulture));

        //if (comboBoxLanguage.Text == "NTSC")
        //{
        //    s = s.Replace("<ntsc>FALSE</ntsc>", "<ntsc>TRUE</ntsc>");
        //}

        //if (comboBoxImageFormat.Text.Contains("8-bit"))
        //{
        //    s = s.Replace("<colordepth>32</colordepth>", "<colordepth>8</colordepth>");
        //}

        var fileName = Path.Combine(_folderName, "fcpxml_export.xml");
        File.WriteAllText(fileName, s);
    }

    /// <summary>
    /// The xmeml time base (a whole number of frames per second) and NTSC flag for
    /// <see cref="FrameRate"/> - 29.97 is the 30 time base with NTSC pull-down, and so on.
    /// </summary>
    private (int TimeBase, string Ntsc) GetTimeBaseAndNtsc()
    {
        if (Math.Abs(FrameRate - 29.97) < 0.01)
        {
            return (30, "TRUE");
        }

        if (Math.Abs(FrameRate - 23.976) < 0.01)
        {
            return (24, "TRUE");
        }

        if (Math.Abs(FrameRate - 59.94) < 0.01)
        {
            return (60, "TRUE");
        }

        return ((int)Math.Round(FrameRate, MidpointRounding.AwayFromZero), "FALSE");
    }
}