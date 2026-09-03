using Nikse.SubtitleEdit.Core.BluRaySup;
using SkiaSharp;
using System.Collections.Concurrent;

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

    /// <summary>
    /// What <see cref="CreateParagraph"/> made of each subtitle: where it sits in the frame and
    /// when it is shown. Only subtitles in here can be composed with others - one that skipped
    /// CreateParagraph carries a ready-made buffer (a track copied out of a container, say) and
    /// is written as it is.
    /// </summary>
    private readonly ConcurrentDictionary<ImageParameter, BluRaySupPicture> _pictures = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Subtitles waiting to be written because they overlap in time. A Blu-ray shows one
    /// display set at a time, so overlapping subtitles have to go into the same display sets
    /// to be seen together (issue #14456) - the group is written when the next subtitle does
    /// not overlap it, or at the end.
    /// </summary>
    private readonly List<ImageParameter> _pending = [];
    private long _pendingStartMs;
    private long _pendingEndMs;

    public void WriteHeader(string fileOrFolderName, ImageParameter imageParameter)
    {
        _width = imageParameter.ScreenWidth;
        _height = imageParameter.ScreenHeight;
        _pictures.Clear();
        _pending.Clear();
        _fileStream = new FileStream(fileOrFolderName, FileMode.Create);
    }

    public void CreateParagraph(ImageParameter param)
    {
        _pictures[param] = MakeBluRaySupImage(param);
    }

    public void WriteParagraph(ImageParameter param)
    {
        if (!_pictures.TryGetValue(param, out var picture))
        {
            FlushPending();
            _fileStream!.Write(param.Buffer, 0, param.Buffer.Length);
            return;
        }

        var overlapsPending = picture.StartTime < _pendingEndMs && picture.EndTime > _pendingStartMs;
        if (_pending.Count > 0 && !overlapsPending)
        {
            FlushPending();
        }

        if (_pending.Count == 0)
        {
            _pendingStartMs = picture.StartTime;
            _pendingEndMs = picture.EndTime;
        }
        else
        {
            _pendingStartMs = Math.Min(_pendingStartMs, picture.StartTime);
            _pendingEndMs = Math.Max(_pendingEndMs, picture.EndTime);
        }

        _pending.Add(param);
    }

    public void WriteFooter()
    {
        FlushPending();
        _fileStream!.Close();
    }

    private void FlushPending()
    {
        if (_pending.Count == 0)
        {
            return;
        }

        try
        {
            if (_pending.Count == 1)
            {
                // Nothing to compose - the display sets CreateParagraph made are the file.
                var buffer = _pending[0].Buffer;
                _fileStream!.Write(buffer, 0, buffer.Length);
            }
            else
            {
                WriteOverlapping(_pending);
            }
        }
        finally
        {
            _pending.Clear();
        }
    }

    /// <summary>
    /// A subtitle as it sits in the frame, ready to be composed with others.
    /// </summary>
    private sealed class PlacedCaption
    {
        public required SKBitmap Bitmap { get; init; }
        public bool OwnsBitmap { get; init; }

        /// <summary>
        /// Made by <see cref="Merge"/> rather than taken from a subtitle - and so is its bitmap.
        /// </summary>
        public bool IsMerged { get; init; }

        public int X { get; init; }
        public int Y { get; init; }
        public SKColor FontColor { get; init; }
        public bool IsForced { get; init; }
        public List<BluRaySupFadeStep> FadeSteps { get; init; } = [];

        public int Right => X + Bitmap.Width;
        public int Bottom => Y + Bitmap.Height;

        public bool Intersects(PlacedCaption other)
        {
            return X < other.Right && other.X < Right && Y < other.Bottom && other.Y < Bottom;
        }

        public long UnionArea(PlacedCaption other)
        {
            return (long)(Math.Max(Right, other.Right) - Math.Min(X, other.X)) *
                   (Math.Max(Bottom, other.Bottom) - Math.Min(Y, other.Y));
        }

        public BluRaySupCompositionObject ToCompositionObject()
        {
            return new BluRaySupCompositionObject
            {
                Bitmap = Bitmap,
                FontColor = FontColor,
                X = X,
                Y = Y,
                IsForced = IsForced,
                FadeSteps = FadeSteps,
            };
        }

        public void Dispose()
        {
            if (OwnsBitmap)
            {
                Bitmap.Dispose();
            }
        }
    }

    /// <summary>
    /// Writes subtitles that overlap in time. The timeline is cut wherever one of them starts
    /// or ends, and every slice becomes an epoch showing all the subtitles on screen in it -
    /// which the decoder takes as one picture, so this is what makes two lines show up at the
    /// same time. Only the last slice clears the screen; before that, the next epoch start
    /// replaces the previous one.
    /// </summary>
    private void WriteOverlapping(List<ImageParameter> cues)
    {
        var pictures = cues.Select(c => _pictures[c]).ToList();
        var times = pictures.SelectMany(p => new[] { p.StartTime, p.EndTime }).Distinct().OrderBy(t => t).ToList();
        var compositionNumber = pictures[0].CompositionNumber;
        var fps = cues[0].FramesPerSecond;

        var captions = new List<PlacedCaption>(cues.Count);
        try
        {
            for (var i = 0; i < cues.Count; i++)
            {
                captions.Add(Place(cues[i], pictures[i]));
            }

            for (var t = 0; t + 1 < times.Count; t++)
            {
                var start = times[t];
                var end = times[t + 1];
                var visible = new List<PlacedCaption>();
                for (var i = 0; i < cues.Count; i++)
                {
                    if (pictures[i].StartTime <= start && pictures[i].EndTime > start)
                    {
                        visible.Add(captions[i]);
                    }
                }

                if (visible.Count == 0)
                {
                    continue;
                }

                // Nothing on screen after this slice means it has to clear; otherwise the next
                // slice's epoch start takes over.
                var showsMore = pictures.Any(p => p.StartTime <= end && p.EndTime > end);

                var composed = Compose(visible);
                try
                {
                    var objects = composed.Select(c => c.ToCompositionObject()).ToList();
                    var picture = new BluRaySupPicture
                    {
                        StartTime = start,
                        EndTime = end,
                        Width = _width,
                        Height = _height,
                        IsForced = objects.Any(o => o.IsForced),
                        CompositionNumber = compositionNumber,
                    };

                    var buffer = BluRaySupPicture.CreateSupFrame(picture, objects, fps, writeClear: !showsMore);
                    _fileStream!.Write(buffer, 0, buffer.Length);
                    compositionNumber = picture.NextCompositionNumber;
                }
                finally
                {
                    foreach (var caption in composed.Where(c => c.IsMerged))
                    {
                        caption.Dispose();
                    }
                }
            }
        }
        finally
        {
            foreach (var caption in captions)
            {
                caption.Dispose();
            }
        }
    }

    private static PlacedCaption Place(ImageParameter cue, BluRaySupPicture picture)
    {
        // Not the cue's bitmap: callers may dispose that right after WriteParagraph (seconv
        // and the container track exports do), and the full frame image was let go as soon as
        // it was encoded. The encoded caption on the picture has the same pixels.
        return new PlacedCaption
        {
            Bitmap = picture.EncodedBitmap.ToBitmap(),
            OwnsBitmap = true,
            X = picture.WindowXOffset,
            Y = picture.WindowYOffset,
            FontColor = cue.FontColor,
            IsForced = cue.IsForced,
            FadeSteps = picture.FadeSteps,
        };
    }

    /// <summary>
    /// Turns the subtitles on screen into the objects of one display set. Windows may not
    /// overlap, so subtitles drawn over each other become one object - the later one on top,
    /// as the export preview shows them - and a display set holds at most two objects, so past
    /// that the closest pair share one.
    /// </summary>
    private static List<PlacedCaption> Compose(List<PlacedCaption> visible)
    {
        var list = visible.OrderBy(c => c.Y).ThenBy(c => c.X).ToList();

        bool merged;
        do
        {
            merged = false;
            for (var i = 0; i < list.Count && !merged; i++)
            {
                for (var j = i + 1; j < list.Count; j++)
                {
                    if (list[i].Intersects(list[j]))
                    {
                        list[i] = Merge(list[i], list[j]);
                        list.RemoveAt(j);
                        merged = true;
                        break;
                    }
                }
            }
        } while (merged);

        while (list.Count > BluRaySupPicture.MaxCompositionObjects)
        {
            var bestI = 0;
            var bestJ = 1;
            var bestArea = long.MaxValue;
            for (var i = 0; i < list.Count; i++)
            {
                for (var j = i + 1; j < list.Count; j++)
                {
                    var area = list[i].UnionArea(list[j]);
                    if (area < bestArea)
                    {
                        bestArea = area;
                        bestI = i;
                        bestJ = j;
                    }
                }
            }

            list[bestI] = Merge(list[bestI], list[bestJ]);
            list.RemoveAt(bestJ);
        }

        return list;
    }

    /// <summary>
    /// One caption covering both, <paramref name="b"/> drawn over <paramref name="a"/>. It fades
    /// as <paramref name="a"/> does - a merged object has one palette range, so it cannot fade
    /// its parts apart.
    /// </summary>
    private static PlacedCaption Merge(PlacedCaption a, PlacedCaption b)
    {
        var x = Math.Min(a.X, b.X);
        var y = Math.Min(a.Y, b.Y);
        var width = Math.Max(a.Right, b.Right) - x;
        var height = Math.Max(a.Bottom, b.Bottom) - y;

        var bitmap = new SKBitmap(Math.Max(1, width), Math.Max(1, height), false);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(a.Bitmap, a.X - x, a.Y - y);
            canvas.DrawBitmap(b.Bitmap, b.X - x, b.Y - y);
        }

        var merged = new PlacedCaption
        {
            Bitmap = bitmap,
            OwnsBitmap = true,
            IsMerged = true,
            X = x,
            Y = y,
            FontColor = a.FontColor,
            IsForced = a.IsForced || b.IsForced,
            FadeSteps = a.FadeSteps,
        };

        if (a.IsMerged)
        {
            a.Dispose();
        }

        if (b.IsMerged)
        {
            b.Dispose();
        }

        return merged;
    }

    /// <summary>
    /// Composition numbers a single subtitle may use: one display set for the caption, one for
    /// the screen clear and up to <see cref="ExportFade.MaxSteps"/> palette updates in between.
    /// Every subtitle gets a block of its own - the numbers must keep climbing through the file
    /// (they wrap at 16 bits, which is expected), gaps in them do no harm. A group of
    /// overlapping subtitles counts on from the block of its first one; it can run past its
    /// blocks when every line fades, and a number then repeats far apart in the file, which no
    /// decoder minds.
    /// </summary>
    private const int CompositionNumbersPerParagraph = ExportFade.MaxSteps + 2;

    private static BluRaySupPicture MakeBluRaySupImage(ImageParameter param)
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

        return brSub;
    }
}
