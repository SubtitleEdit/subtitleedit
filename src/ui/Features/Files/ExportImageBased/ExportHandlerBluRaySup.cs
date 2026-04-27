using Nikse.SubtitleEdit.Core.BluRaySup;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.IO;

namespace Nikse.SubtitleEdit.Features.Files.ExportImageBased;

public class ExportHandlerBluRaySup : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.BluRaySup;
    public string Extension => ".sup";
    public bool UseFileName => true;
    public string Title => string.Format(Se.Language.General.ExportToX, "Blu-ray sup");

    private int _width;
    private int _height;
    private FileStream? _fileStream;

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _width = imageParameter.ScreenWidth;
        _height = imageParameter.ScreenHeight;
        _fileStream = new FileStream(fileOrFolderName, FileMode.Create);
    }

    public void CreateParagraph(ImageParameter param)
    {
        MakeBluRaySupImage(param);
    }

    public void WriteParagraph(ImageParameter param)
    {
        _fileStream!.Write(param.Buffer, 0, param.Buffer.Length);
    }

    public void WriteFooter()
    {
        _fileStream!.Close();
    }

    private static void MakeBluRaySupImage(ImageParameter param)
    {
        var brSub = new BluRaySupPicture
        {
            StartTime = (long)Math.Round(param.StartTime.TotalMilliseconds, MidpointRounding.AwayFromZero),
            EndTime = (long)Math.Round(param.EndTime.TotalMilliseconds, MidpointRounding.AwayFromZero),
            Width = param.ScreenWidth,
            Height = param.ScreenHeight,
            IsForced = param.IsForced,
            CompositionNumber = (param.Index + 1) * 2,
        };
        if (param.IsFullFrame)
        {
            var nbmp = new NikseBitmap(param.Bitmap);
            nbmp.ReplaceTransparentWith(param.BackgroundColor);
            using (var bmp = nbmp.GetBitmap())
            {
                var top = param.ScreenHeight - (param.Bitmap.Height + param.BottomTopMargin);
                var left = (param.ScreenWidth - param.Bitmap.Width) / 2;

                var b = new NikseBitmap(param.ScreenWidth, param.ScreenHeight);
                {
                    b.Fill(param.BackgroundColor);
                    using (var fullSize = b.GetBitmap())
                    {
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

                        if (param.OverridePosition != null &&
                            param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.ScreenWidth &&
                            param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.ScreenHeight)
                        {
                            left = param.OverridePosition.Value.X;
                            top = param.OverridePosition.Value.Y;
                        }

                        //using (var g = Graphics.FromImage(fullSize))
                        //{
                        //    g.DrawImage(bmp, left, top);
                        //    g.Dispose();
                        //}
                        param.Buffer = BluRaySupPicture.CreateSupFrame(brSub, fullSize, param.FontColor, param.FramesPerSecond, 0, 0, BluRayContentAlignment.BottomCenter);
                    }
                }
            }
        }
        else
        {
            if (param.OverridePosition != null &&
                param.OverridePosition.Value.X >= 0 && param.OverridePosition.Value.X < param.ScreenWidth &&
                param.OverridePosition.Value.Y >= 0 && param.OverridePosition.Value.Y < param.ScreenHeight)
            {
                param.LeftRightMargin = param.OverridePosition.Value.X;
                param.BottomTopMargin = param.ScreenHeight - param.OverridePosition.Value.Y - param.Bitmap.Height;
            }

            var margin = param.LeftRightMargin;

            param.Buffer = BluRaySupPicture.CreateSupFrame(
                brSub,
                param.Bitmap,
                param.FontColor,
                param.FramesPerSecond,
                param.BottomTopMargin,
                margin,
                param.BluRayContentAlignment,
                param.OverridePosition.HasValue ? new BluRayPoint(param.OverridePosition.Value.X, param.OverridePosition.Value.Y) : null);
        }
    }
}