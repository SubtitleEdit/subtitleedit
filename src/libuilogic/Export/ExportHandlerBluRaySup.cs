using Nikse.SubtitleEdit.Core.BluRaySup;

namespace Nikse.SubtitleEdit.UiLogic.Export;

public class ExportHandlerBluRaySup : IExportHandler
{
    public ExportImageType ExportImageType => ExportImageType.BluRaySup;
    public string Extension => ".sup";
    public bool UseFileName => true;
    public string Title => string.Format("Export to {0}", "Blu-ray sup");

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

    /// <summary>
    /// Composition numbers a single subtitle may use: one display set for the caption, one for
    /// the screen clear and up to <see cref="ExportFade.MaxSteps"/> palette updates in between.
    /// Every subtitle gets a block of its own - the numbers must keep climbing through the file
    /// (they wrap at 16 bits, which is expected), gaps in them do no harm.
    /// </summary>
    private const int CompositionNumbersPerParagraph = ExportFade.MaxSteps + 2;

    private static void MakeBluRaySupImage(ImageParameter param)
    {
        var startTime = (long)Math.Round(param.StartTime.TotalMilliseconds, MidpointRounding.AwayFromZero);
        var endTime = (long)Math.Round(param.EndTime.TotalMilliseconds, MidpointRounding.AwayFromZero);
        var brSub = new BluRaySupPicture
        {
            StartTime = startTime,
            EndTime = endTime,
            Width = param.ScreenWidth,
            Height = param.ScreenHeight,
            IsForced = param.IsForced,
            CompositionNumber = (param.Index + 1) * CompositionNumbersPerParagraph,

            // "{\fad(..)}" - Blu-ray fades by re-sending the palette with the alpha scaled, so
            // the subtitle is encoded once and each step costs a palette, not a bitmap.
            FadeSteps = ExportFade.CreateSteps(param.FadeKeyframes, startTime, endTime, param.FramesPerSecond),
        };
        if (param.IsFullFrame)
        {
            // The image already covers the frame, so it goes in at 0,0 with no margins - the
            // alignment and margins were used to place the text inside it.
            using var fullSize = FullFrameImage.Create(param);
            param.Buffer = BluRaySupPicture.CreateSupFrame(brSub, fullSize, param.FontColor, param.FramesPerSecond, 0, 0, BluRayContentAlignment.BottomCenter);
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