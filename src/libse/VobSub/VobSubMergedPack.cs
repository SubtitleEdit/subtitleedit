using System;
using System.Collections.Generic;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Interfaces;
using SkiaSharp;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    public class VobSubMergedPack : IBinaryParagraphWithPosition
    {
        public SubPicture SubPicture { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; set; }
        public int StreamId { get; private set; }
        public IdxParagraph IdxLine { get; private set; }
        public List<SKColor> Palette { get; set; }

        /// <summary>The raw DVD sub picture unit (the merged PES payloads).</summary>
        public byte[] SubPictureData { get; }

        public VobSubMergedPack(byte[] subPictureData, TimeSpan presentationTimestamp, int streamId, IdxParagraph idxLine)
        {
            SubPictureData = subPictureData;
            SubPicture = new SubPicture(subPictureData);
            StartTime = presentationTimestamp;
            StreamId = streamId;
            IdxLine = idxLine;
        }

        public bool IsForced => SubPicture.Forced;

        public SKBitmap GetBitmap()
        {
            if (Palette != null && Palette.Count > 0)
            {
                return SubPicture.GetBitmap(Palette, SKColors.Transparent, SKColors.Black, SKColors.White, SKColors.Black, false, true);
            }

            // No palette (e.g. a .vob without its IFO): white text with a black outline, the way a DVD
            // usually looks (and how a sub/idx with its palette is shown). Which of the three colors is
            // the text, the outline and the anti-aliasing differs between discs, so it is found from the
            // image itself - see GetNoPaletteColors.
            var colors = GetNoPaletteColors();
            return SubPicture.GetBitmap(null, colors[0], colors[1], colors[2], colors[3], false, true);
        }

        private SKColor[] _noPaletteColors;

        /// <summary>
        /// The outline is the color that borders the transparent background the most (relative to
        /// its pixel count) and the text body the one that borders it the least; the text is drawn
        /// white, the outline black and the anti-aliasing between them gray. Most discs use pattern =
        /// text, emphasis 1 = outline, but e.g. some use pattern = outline and emphasis 2 = text - with
        /// fixed colors, text and outline then came out the same color.
        /// </summary>
        private SKColor[] GetNoPaletteColors()
        {
            if (_noPaletteColors != null)
            {
                return _noPaletteColors;
            }

            var markers = new[] { SKColors.Transparent, new SKColor(255, 0, 0), new SKColor(0, 255, 0), new SKColor(0, 0, 255) };
            var counts = new int[4];
            var edges = new int[4];
            using (var bitmap = SubPicture.GetBitmap(null, markers[0], markers[1], markers[2], markers[3], false, true))
            {
                if (bitmap != null)
                {
                    var width = bitmap.Width;
                    var height = bitmap.Height;
                    var pixels = bitmap.Pixels;
                    var index = new int[pixels.Length];
                    for (var i = 0; i < pixels.Length; i++)
                    {
                        index[i] = pixels[i] == markers[1] ? 1 : pixels[i] == markers[2] ? 2 : pixels[i] == markers[3] ? 3 : 0;
                    }

                    for (var y = 0; y < height; y++)
                    {
                        for (var x = 0; x < width; x++)
                        {
                            var i = index[y * width + x];
                            if (i == 0)
                            {
                                continue;
                            }

                            counts[i]++;
                            if (x == 0 || y == 0 || x == width - 1 || y == height - 1 ||
                                index[y * width + x - 1] == 0 || index[y * width + x + 1] == 0 ||
                                index[(y - 1) * width + x] == 0 || index[(y + 1) * width + x] == 0)
                            {
                                edges[i]++;
                            }
                        }
                    }
                }
            }

            var used = new List<int>();
            for (var i = 1; i < 4; i++)
            {
                if (counts[i] > 0)
                {
                    used.Add(i);
                }
            }

            var colors = new[] { SKColors.Transparent, SKColors.White, SKColors.Black, SKColors.Gray };
            if (used.Count >= 2)
            {
                used.Sort((a, b) => ((double)edges[a] / counts[a]).CompareTo((double)edges[b] / counts[b]));
                colors = new[] { SKColors.Transparent, SKColors.Gray, SKColors.Gray, SKColors.Gray };
                colors[used[0]] = SKColors.White; // borders the background the least: text
                colors[used[used.Count - 1]] = SKColors.Black; // borders the background the most: outline
            }

            _noPaletteColors = colors;
            return colors;
        }
        public SKSize GetScreenSize()
        {
            return new SKSize(720, 480);
        }

        /// <summary>
        /// Where the bitmap from <see cref="GetBitmap"/> sits on screen. That bitmap is cropped
        /// to the ink, so this is the display area's origin plus the crop offset - the display
        /// area alone can be the whole frame on some discs, which put every cue at the top-left.
        /// Call after <see cref="GetBitmap"/>, which is what decodes both.
        /// </summary>
        public Position GetPosition()
        {
            var position = SubPicture.ImagePosition;
            return new Position(position.X, position.Y);
        }

        public TimeCode StartTimeCode
        {
            get
            {
                //if (IdxLine != null)
                //{
                //    return new TimeCode(IdxLine.StartTime.TotalMilliseconds);
                //}

                return new TimeCode(StartTime.TotalMilliseconds);
            }
        }

        public TimeCode EndTimeCode
        {
            get
            {
                //if (IdxLine != null)
                //{
                //    return new TimeCode(IdxLine.StartTime.TotalMilliseconds + SubPicture.Delay.TotalMilliseconds);
                //}

                return new TimeCode(StartTime.TotalMilliseconds + SubPicture.Delay.TotalMilliseconds);
            }
        }

    }
}
