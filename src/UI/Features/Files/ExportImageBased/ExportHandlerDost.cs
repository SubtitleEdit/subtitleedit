using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerDost : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.Dost;
    public string Extension => "";
    public bool UseFileName => false;
    public string Title => string.Format(Se.Language.General.ExportToX, "Dost/image");
    public double FrameRate { get; set; } = 23.976;

    private string _folderName = string.Empty; 
    private int _count = 0;
    private StringBuilder _sb = new StringBuilder();

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _folderName = fileOrFolderName;
        if (!Directory.Exists(_folderName))
        {
            Directory.CreateDirectory(_folderName);
        }

        _sb.Clear();
        _count = 0;
    }

    public void CreateParagraph(ImageParameter param)
    {

    }

    public void WriteParagraph(ImageParameter param)
    {
        _count++;
        string numberString = string.Format("{0:0000}", _count);
        var fileName = Path.Combine(_folderName, Path.GetFileNameWithoutExtension(_folderName).Replace(" ", "_")) + "_" + numberString + ".png";
        File.WriteAllBytes(fileName, param.Bitmap.ToPngArray());

        const string paragraphWriteFormat = "{0}\t{1}\t{2}\t{4}\t{5}\t{3}\t0\t0";

        int top = param.ScreenHeight - (param.Bitmap.Height + param.BottomTopMargin);
        int left = (param.ScreenWidth - param.Bitmap.Width) / 2;
        if (param.Alignment == ExportAlignment.BottomLeft || param.Alignment == ExportAlignment.MiddleLeft || param.Alignment == ExportAlignment.TopLeft)
        {
            left = param.LeftRightMargin;
        }
        else if (param.Alignment == ExportAlignment.BottomRight || param.Alignment == ExportAlignment.MiddleRight || param.Alignment == ExportAlignment.TopRight)
        {
            left = param.ScreenWidth - param.Bitmap.Width - param.LeftRightMargin;
        }

        if (param.Alignment == ExportAlignment.TopLeft || param.Alignment == ExportAlignment.TopCenter || param.Alignment == ExportAlignment.TopRight)
        {
            top = param.BottomTopMargin;
        }

        if (param.Alignment == ExportAlignment.MiddleLeft || param.Alignment == ExportAlignment.MiddleCenter || param.Alignment == ExportAlignment.MiddleRight)
        {
            top = param.ScreenHeight - (param.Bitmap.Height / 2);
        }

        if (param.OverridePosition.HasValue &&
            param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.Bitmap.Width &&
            param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.Bitmap.Height)
        {
            left = param.OverridePosition.Value.X;
            top = param.OverridePosition.Value.Y;
        }

        string startTime = ToHHMMSSFF(new TimeCode(param.StartTime));
        string endTime = ToHHMMSSFF(new TimeCode(param.EndTime));
        _sb.AppendLine(string.Format(paragraphWriteFormat, numberString, startTime, endTime, Path.GetFileName(fileName), left, top));
    }

    public void WriteFooter()
    {
        var header = @"$FORMAT=480
$VERSION=1.2
$ULEAD=TRUE
$DROP=[DROPVALUE]" + Environment.NewLine + Environment.NewLine +
"NO\tINTIME\t\tOUTTIME\t\tXPOS\tYPOS\tFILENAME\tFADEIN\tFADEOUT";

        var dropValue = "30000";
        header = header.Replace("[DROPVALUE]", dropValue);
        File.WriteAllText(Path.Combine(_folderName, "index.dost"), header + Environment.NewLine + _sb.ToString());
    }

    private int MillisecondsToFramesMaxFrameRate(double milliseconds)
    {
        int frames = (int)Math.Round(milliseconds / (TimeCode.BaseUnit / FrameRate));
        if (frames >= FrameRate)
        {
            frames = (int)(FrameRate - 0.01);
        }

        return frames;
    }

    private string ToHHMMSSFF(TimeCode timeCode)
    {
        var ms = timeCode.TotalMilliseconds;
        if (!Configuration.Settings.General.CurrentVideoIsSmpte && ((decimal)FrameRate) % 1 != 0)
        {
            ms /= 1.001;
        }

        var tc = new TimeCode(ms);
        return $"{tc.Hours:00}:{tc.Minutes:00}:{tc.Seconds:00}:{MillisecondsToFramesMaxFrameRate(tc.Milliseconds):00}";
    }

}