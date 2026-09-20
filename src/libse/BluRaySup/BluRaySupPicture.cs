/*
 * Copyright 2009 Volker Oth (0xdeadbeef)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * NOTE: Converted to C# and modified by Nikse.dk@gmail.com
 * NOTE: For more info see http://blog.thescorpius.com/index.php/2017/07/15/presentation-graphic-stream-sup-files-bluray-subtitle-format/
 */

using Nikse.SubtitleEdit.Core.Common;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.BluRaySup
{
    public enum BluRayContentAlignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
    }

    public class BluRaySupPicture
    {
        /// <summary>
        /// Composition objects one display set can show at the same time - the PCS has room for
        /// two, each in a window of its own.
        /// </summary>
        public const int MaxCompositionObjects = 2;

        /// <summary>
        /// Palette entries a display set can define. Index 255 is never defined: a decoder keeps
        /// an undefined entry fully transparent, and that is the index transparent pixels are
        /// encoded with (<see cref="TransparentIndex"/>).
        /// </summary>
        public const int MaxPaletteEntries = 255;

        /// <summary>
        /// Colours <see cref="GetBitmapPalette"/> collects for one bitmap, before the transparent
        /// entry it always adds at the end.
        /// </summary>
        private const int MaxPaletteColors = 254;

        private const byte TransparentIndex = 0xff;

        /// <summary>
        /// screen width
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// screen height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// start time in milliseconds
        /// </summary>
        public long StartTime { get; set; }

        public long StartTimeForWrite => (long)Math.Round(StartTime * 90.0, MidpointRounding.AwayFromZero);

        /// <summary>
        /// end time in milliseconds
        /// </summary>
        public long EndTime { get; set; }

        public long EndTimeForWrite => (long)Math.Round(EndTime * 90.0, MidpointRounding.AwayFromZero);

        /// <summary>
        /// if true, this is a forced subtitle
        /// </summary>
        public bool IsForced { get; set; }

        /// <summary>
        /// composition number - increased at start and end PCS
        /// </summary>
        public int CompositionNumber { get; set; }

        /// <summary>
        /// Set by <see cref="CreateSupFrame(BluRaySupPicture, IList{BluRaySupCompositionObject}, double, bool)"/>:
        /// the composition number the display set following the ones it wrote should use.
        /// </summary>
        public int NextCompositionNumber { get; set; }

        /// <summary>
        /// The caption as it went into the stream, set by
        /// <see cref="CreateSupFrame(BluRaySupPicture, SKBitmap, SKColor, double, int, int, BluRayContentAlignment, BluRayPoint)"/>.
        /// A caller that lets the bitmap go can still compose the caption with others from this.
        /// </summary>
        public BluRaySupEncodedBitmap EncodedBitmap { get; set; }

        /// <summary>
        /// width of subtitle window (might be larger than image)
        /// </summary>
        public int WindowWidth { get; set; }

        /// <summary>
        /// height of subtitle window (might be larger than image)
        /// </summary>
        public int WindowHeight { get; set; }

        /// <summary>
        /// upper left corner of subtitle window x
        /// </summary>
        public int WindowXOffset { get; set; }

        /// <summary>
        /// upper left corner of subtitle window y
        /// </summary>
        public int WindowYOffset { get; set; }

        /// <summary>
        /// FPS type (e.g. 0x10 = 24p)
        /// </summary>
        public int FramesPerSecondType { get; set; }

        /// <summary>
        /// List of (list of) palette info - there are up to 8 palettes per epoch, each can be updated several times
        /// </summary>
        public List<List<PaletteInfo>> Palettes { get; set; } = new List<List<PaletteInfo>>();

        /// <summary>
        /// Alpha levels for a fade in/out, in presentation order. A step at <see cref="StartTime"/>
        /// sets the alpha the caption appears with; every later step becomes a palette update
        /// display set. Empty (the default) writes the caption fully opaque, as before.
        /// </summary>
        public List<BluRaySupFadeStep> FadeSteps { get; set; } = new List<BluRaySupFadeStep>();

        /// <summary>
        /// Create RLE buffer from bitmap
        /// </summary>
        /// <param name="bm">Bitmap to compress</param>
        /// <param name="palette">Palette used for bitmap encoding</param>
        /// <param name="indexOffset">Where <paramref name="palette"/> starts in the palette the display set defines - the RLE references that one, so every index is shifted by this</param>
        /// <returns>RLE buffer</returns>
        private static byte[] EncodeImage(SKBitmap bm, List<SKColor> palette, int indexOffset)
        {
            var lookup = new Dictionary<SKColor, int>();
            for (var i = 0; i < palette.Count; i++)
            {
                var color = palette[i];
                if (!lookup.ContainsKey(color))
                {
                    lookup.Add(color, i);
                }
            }

            // Cap on the approximate-match memo below. A caption repeats its anti-aliasing
            // colours on every row, but a photographic source can hold millions of distinct
            // ones - past the cap the linear scan simply runs again, as it always did.
            const int maxCachedColors = 1 << 16;

            // Transparent pixels use the one index no palette defines, whatever range of the
            // palette this bitmap has.
            const byte transparentColor = TransparentIndex;
            var bytes = new List<byte>(bm.Width * 2);
            var reader = new BitmapRowReader(bm);
            var row = new SKColor[bm.Width];
            for (var y = 0; y < bm.Height; y++)
            {
                reader.ReadRow(y, row);
                int x;
                int len;
                for (x = 0; x < bm.Width; x += len)
                {
                    var c = row[x];

                    byte color;
                    if (c.Alpha == 0)
                    {
                        color = transparentColor;
                    }
                    else if (lookup.TryGetValue(c, out var intC))
                    {
                        color = (byte)(intC + indexOffset);
                    }
                    else
                    {
                        // FindBestMatch is a linear scan over the (up to 255 entry) palette and
                        // the palette does not change while encoding, so the answer for a color
                        // is fixed. Anti-aliased edges hit this for the same handful of colors
                        // on every row of the caption; remember them in the same lookup.
                        var best = FindBestMatch(c, palette);
                        if (lookup.Count < maxCachedColors)
                        {
                            lookup[c] = best;
                        }

                        color = (byte)(best + indexOffset);
                    }

                    for (len = 1; x + len < bm.Width; len++)
                    {
                        if (row[x + len] != c)
                        {
                            break;
                        }
                    }

                    if (len <= 2 && color != 0)
                    {
                        // only a single occurrence -> add color
                        bytes.Add(color);
                        if (len == 2)
                        {
                            bytes.Add(color);
                        }
                    }
                    else
                    {
                        if (len > 0x3fff)
                        {
                            len = 0x3fff;
                        }

                        bytes.Add(0); // rle id
                        // commented out due to bug in SupRip
                        /*if (color == 0 && x+len == bm.Width)
                        {
                            bytes.Add(0);
                            eol = true;
                        }
                        else */
                        if (color == 0 && len < 0x40)
                        {
                            // 00 xx -> xx times 0
                            bytes.Add((byte)len);
                        }
                        else if (color == 0)
                        {
                            // 00 4x xx -> xxx zeroes
                            bytes.Add((byte)(0x40 | (len >> 8)));
                            bytes.Add((byte)len);
                        }
                        else if (len < 0x40)
                        {
                            // 00 8x cc -> x times value cc
                            bytes.Add((byte)(0x80 | len));
                            bytes.Add(color);
                        }
                        else
                        {
                            // 00 cx yy cc -> xyy times value cc
                            bytes.Add((byte)(0xc0 | (len >> 8)));
                            bytes.Add((byte)len);
                            bytes.Add(color);
                        }
                    }
                }
                if (x == bm.Width)
                {
                    bytes.Add(0); // rle id
                    bytes.Add(0);
                }
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Hands out whole rows of pixels without a SkiaSharp interop call per pixel. Only color
        /// types whose bytes <see cref="SKBitmap.GetPixel"/> returns unchanged are read directly:
        /// premultiplied pixels get un-premultiplied on the way out of GetPixel, and other layouts
        /// are not four plain BGRA/RGBA bytes, so both keep using GetPixel.
        /// </summary>
        private sealed class BitmapRowReader
        {
            private readonly SKBitmap _bitmap;
            private readonly int _redOffset;
            private readonly int _blueOffset;
            private readonly bool _direct;

            public BitmapRowReader(SKBitmap bitmap)
            {
                _bitmap = bitmap;
                // Alpha is byte 3 and green byte 1 in both layouts; only red and blue swap.
                if (bitmap.AlphaType != SKAlphaType.Premul && bitmap.GetPixels() != IntPtr.Zero)
                {
                    if (bitmap.ColorType == SKColorType.Bgra8888)
                    {
                        _redOffset = 2;
                        _blueOffset = 0;
                        _direct = true;
                    }
                    else if (bitmap.ColorType == SKColorType.Rgba8888)
                    {
                        _redOffset = 0;
                        _blueOffset = 2;
                        _direct = true;
                    }
                }
            }

            public unsafe void ReadRow(int y, SKColor[] row)
            {
                if (!_direct)
                {
                    for (var x = 0; x < row.Length; x++)
                    {
                        row[x] = _bitmap.GetPixel(x, y);
                    }

                    return;
                }

                var rowBytes = _bitmap.RowBytes;
                var line = new ReadOnlySpan<byte>((byte*)_bitmap.GetPixels().ToPointer() + (long)y * rowBytes, rowBytes);
                for (var x = 0; x < row.Length; x++)
                {
                    var i = x * 4;
                    row[x] = new SKColor(line[i + _redOffset], line[i + 1], line[i + _blueOffset], line[i + 3]);
                }
            }
        }

        private static byte FindBestMatch(SKColor color, List<SKColor> palette)
        {
            var smallestDiff = 1000;
            var smallestDiffIndex = -1;
            var max = palette.Count;
            for (var i = 0; i < max; i++)
            {
                var c = palette[i];
                var diff = Math.Abs(c.Alpha - color.Alpha) + Math.Abs(c.Red - color.Red) + Math.Abs(c.Green - color.Green) + Math.Abs(c.Blue - color.Blue);
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    smallestDiffIndex = i;
                }
            }

            return (byte)smallestDiffIndex;
        }

        private static bool HasCloseColor(SKColor color, List<SKColor> palette, int maxDifference)
        {
            var max = palette.Count;
            for (var i = 0; i < max; i++)
            {
                var c = palette[i];
                var difference = Math.Abs(c.Alpha - color.Alpha) + Math.Abs(c.Red - color.Red) + Math.Abs(c.Green - color.Green) + Math.Abs(c.Blue - color.Blue);
                if (difference < maxDifference)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The colours of the bitmap, <paramref name="fontColor"/> first and a transparent entry
        /// last, at most <paramref name="maxColors"/> of them before the transparent one. A
        /// bitmap with more distinct colours than that is reduced by merging close ones.
        /// </summary>
        private static List<SKColor> GetBitmapPalette(SKBitmap bitmap, SKColor fontColor, int maxColors)
        {
            var pal = new List<SKColor>(255);
            var lookup = new HashSet<SKColor>(255);
            var reader = new BitmapRowReader(bitmap);
            var row = new SKColor[bitmap.Width];

            // Add font color as first entry
            pal.Add(fontColor);
            lookup.Add(fontColor);

            // first we try with exact colors
            for (var y = 0; y < bitmap.Height; y++)
            {
                reader.ReadRow(y, row);
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var c = row[x];
                    if (c.Alpha > 0)
                    {
                        if (lookup.Contains(c))
                        {
                            // exact color already exists
                        }
                        else
                        {
                            pal.Add(c);
                            lookup.Add(c);
                        }

                        if (pal.Count >= maxColors)
                        {
                            break;
                        }
                    }
                }
                if (pal.Count >= maxColors)
                {
                    break;
                }
            }

            if (pal.Count < maxColors)
            {
                pal.Add(SKColors.Transparent); // last entry must be transparent
                return pal;
            }


            // get close colors (image has probably been processed in SE)
            // The tolerance widens as the palette fills: 1 for the first part, 5 for the next,
            // 25 for the rest.
            var exactLimit = maxColors * 100 / MaxPaletteColors;
            var closeLimit = maxColors * 240 / MaxPaletteColors;
            pal = new List<SKColor>();
            lookup = new HashSet<SKColor>();
            pal.Add(fontColor);
            lookup.Add(fontColor);

            // Colors already known to have a close palette entry. HasCloseColor is a linear scan
            // over the palette, and this pass reached it for every pixel of every anti-aliased
            // edge - a 1920x200 caption ran it hundreds of thousands of times over a palette
            // growing towards 254 entries. A rejection can never be undone: the palette only
            // grows, and the tolerance only widens as it does (1 -> 5 -> 25), so a color that
            // had a close entry once still has one later. Rejections take no palette slot, so
            // caching one can never skip an insertion.
            var rejected = new HashSet<SKColor>();
            for (var y = 0; y < bitmap.Height; y++)
            {
                reader.ReadRow(y, row);
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var c = row[x];
                    if (c.Alpha > 0)
                    {
                        if (lookup.Contains(c))
                        {
                            // exact color already exists
                        }
                        else if (rejected.Contains(c))
                        {
                            // a close enough color already exists
                        }
                        else if (pal.Count < exactLimit)
                        {
                            if (!HasCloseColor(c, pal, 1))
                            {
                                pal.Add(c);
                                lookup.Add(c);
                            }
                            else
                            {
                                rejected.Add(c);
                            }
                        }
                        else if (pal.Count < closeLimit)
                        {
                            if (!HasCloseColor(c, pal, 5))
                            {
                                pal.Add(c);
                                lookup.Add(c);
                            }
                            else
                            {
                                rejected.Add(c);
                            }
                        }
                        else if (pal.Count < maxColors)
                        {
                            if (!HasCloseColor(c, pal, 25))
                            {
                                pal.Add(c);
                                lookup.Add(c);
                            }
                            else
                            {
                                rejected.Add(c);
                            }
                        }
                    }
                }

                // Every branch above requires pal.Count < maxColors, so a full palette freezes the
                // result - the rest of the image only cost scans that could not change it.
                if (pal.Count >= maxColors)
                {
                    break;
                }
            }

            pal.Add(SKColors.Transparent); // last entry must be transparent
            return pal;
        }

        /// <summary>
        /// Get ID for given frame rate
        /// </summary>
        /// <param name="fps">frame rate</param>
        /// <returns>byte ID for the given frame rate</returns>
        private static int GetFpsId(double fps)
        {
            if (Math.Abs(fps - Core.Fps24Hz) < 0.01) // 24
            {
                return 0x20;
            }

            if (Math.Abs(fps - Core.FpsPal) < 0.01) // 25
            {
                return 0x30;
            }

            if (Math.Abs(fps - Core.FpsNtsc) < 0.01) // 29.97
            {
                return 0x40;
            }

            if (Math.Abs(fps - 30.0) < 0.01) // 30
            {
                return 0x50;
            }

            if (Math.Abs(fps - Core.FpsPalI) < 0.01) // 50
            {
                return 0x60;
            }

            if (Math.Abs(fps - Core.FpsNtscI) < 0.1) // 59.94
            {
                return 0x70;
            }

            return 0x10; // 23.976
        }

        private static long _lastEndTimeForWrite = -1000;

        // PG segment types
        private const byte PdsSegment = 0x14;
        private const byte OdsSegment = 0x15;
        private const byte PcsSegment = 0x16;
        private const byte WdsSegment = 0x17;
        private const byte EndSegment = 0x80;

        // composition_state of a PCS
        private const byte CompositionStateNormal = 0x00;
        private const byte CompositionStateEpochStart = 0x80;

        // For some obscure reason, a packet can be a maximum 0xfffc bytes. Since 13 bytes are
        // needed for the header ("PG", PTS, DTS, ID, SIZE) there are only 0xffef bytes available
        // for the packet; the first ODS packet needs an additional 11 bytes for info and the
        // following ODS packets 4 additional bytes, so the first package can store only 0xffe4
        // RLE buffer bytes and the following packets 0xffeb RLE buffer bytes.
        private const int FirstOdsPayload = 0xffe4;
        private const int NextOdsPayload = 0xffeb;

        /// <summary>
        /// Splits fade steps into the alpha the object appears with (part of the epoch's own
        /// palette) and the steps that follow it as palette update display sets. Steps outside
        /// the display set, and steps that do not change the alpha, are dropped - each one would
        /// cost a display set for nothing.
        /// </summary>
        private static List<BluRaySupFadeStep> GetFadeSteps(IList<BluRaySupFadeStep> fadeSteps, long startTime, long endTime, out int startAlphaPercent)
        {
            startAlphaPercent = 100;
            var updates = new List<BluRaySupFadeStep>();
            if (fadeSteps == null || fadeSteps.Count == 0)
            {
                return updates;
            }

            var sorted = new List<BluRaySupFadeStep>(fadeSteps);
            sorted.Sort((a, b) => a.TimeMs.CompareTo(b.TimeMs));

            var lastAlpha = 100;
            var lastTime = long.MinValue;
            foreach (var step in sorted)
            {
                var alpha = Math.Min(100, Math.Max(0, step.AlphaPercent));
                if (step.TimeMs <= startTime)
                {
                    startAlphaPercent = alpha;
                    lastAlpha = alpha;
                    continue;
                }

                if (step.TimeMs >= endTime || alpha == lastAlpha || step.TimeMs == lastTime)
                {
                    continue;
                }

                updates.Add(new BluRaySupFadeStep(step.TimeMs, alpha));
                lastAlpha = alpha;
                lastTime = step.TimeMs;
            }

            return updates;
        }

        /// <summary>
        /// One composition object of a display set, encoded against its own range of the shared
        /// palette.
        /// </summary>
        private sealed class EncodedObject
        {
            /// <summary>
            /// The object's colours, ending with the transparent entry
            /// <see cref="GetBitmapPalette"/> always adds.
            /// </summary>
            public List<SKColor> Palette;

            /// <summary>
            /// Index of the first of those colours in the shared palette.
            /// </summary>
            public int PaletteOffset;

            public byte[] Rle;
            public int Width;
            public int Height;
            public int X;
            public int Y;
            public bool IsForced;
            public int StartAlphaPercent;
            public List<BluRaySupFadeStep> Updates;
        }

        /// <summary>
        /// The alpha of every composition object at one palette update display set.
        /// </summary>
        private sealed class PaletteUpdate
        {
            public long TimeMs;
            public int[] AlphaPercents;

            public long TimeForWrite => (long)Math.Round(TimeMs * 90.0, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Interleaves the fade steps of all objects into one schedule. A palette update carries
        /// the whole palette, so at every step time the alpha of every object is needed - the
        /// ones without a step of their own keep the alpha they had.
        /// </summary>
        private static List<PaletteUpdate> MergeFadeSteps(IList<EncodedObject> objects)
        {
            var times = new SortedSet<long>();
            foreach (var obj in objects)
            {
                foreach (var step in obj.Updates)
                {
                    times.Add(step.TimeMs);
                }
            }

            var current = new int[objects.Count];
            for (var k = 0; k < objects.Count; k++)
            {
                current[k] = objects[k].StartAlphaPercent;
            }

            var updates = new List<PaletteUpdate>(times.Count);
            foreach (var time in times)
            {
                for (var k = 0; k < objects.Count; k++)
                {
                    foreach (var step in objects[k].Updates)
                    {
                        if (step.TimeMs == time)
                        {
                            current[k] = step.AlphaPercent;
                        }
                    }
                }

                updates.Add(new PaletteUpdate { TimeMs = time, AlphaPercents = (int[])current.Clone() });
            }

            return updates;
        }

        private static int ClampY(BluRaySupPicture pic, int y)
        {
            var yOfs = y - Core.CropOfsY;
            if (yOfs < 0)
            {
                return 0;
            }

            var yMax = pic.Height - pic.WindowHeight - 2 * Core.CropOfsY;
            return yOfs > yMax ? yMax : yOfs;
        }

        /// <summary>
        /// Builds the palettes and RLE data of the objects. Every object of a display set reads
        /// from the one palette the PCS names, so each gets a range of it: the palettes are laid
        /// end to end and the RLE of an object references its own range. That also keeps the
        /// fades apart - a palette update scales the entries of one object and leaves the
        /// others alone.
        /// </summary>
        private static List<EncodedObject> EncodeObjects(BluRaySupPicture pic, IList<BluRaySupCompositionObject> objects)
        {
            var palettes = new List<List<SKColor>>(objects.Count);
            var total = 0;
            foreach (var obj in objects)
            {
                var palette = GetBitmapPalette(obj.Bitmap, obj.FontColor, MaxPaletteColors);
                palettes.Add(palette);
                total += palette.Count;
            }

            if (total > MaxPaletteEntries)
            {
                // Together they do not fit; give every object an equal share instead.
                var maxColors = MaxPaletteEntries / objects.Count - 1;
                for (var k = 0; k < objects.Count; k++)
                {
                    palettes[k] = GetBitmapPalette(objects[k].Bitmap, objects[k].FontColor, maxColors);
                }
            }

            var encoded = new List<EncodedObject>(objects.Count);
            var offset = 0;
            for (var k = 0; k < objects.Count; k++)
            {
                var obj = objects[k];
                var updates = GetFadeSteps(obj.FadeSteps, pic.StartTime, pic.EndTime, out var startAlphaPercent);
                encoded.Add(new EncodedObject
                {
                    Palette = palettes[k],
                    PaletteOffset = offset,
                    Rle = EncodeImage(obj.Bitmap, palettes[k], offset),
                    Width = obj.Bitmap.Width,
                    Height = obj.Bitmap.Height,
                    X = obj.X,
                    Y = ClampY(pic, obj.Y),
                    IsForced = obj.IsForced,
                    StartAlphaPercent = startAlphaPercent,
                    Updates = updates,
                });
                offset += palettes[k].Count;
            }

            return encoded;
        }

        /// <summary>
        /// Presentation Composition Segment: the frame, the composition number and state, and
        /// which objects are shown where. A PCS with no objects clears the screen.
        /// </summary>
        private static byte[] BuildPcs(int width, int height, int fpsId, int compositionNumber, byte compositionState, bool paletteUpdate, IList<EncodedObject> objects)
        {
            var buf = new byte[11 + objects.Count * 8];
            ToolBox.SetWord(buf, 0, width);                                 // video_width
            ToolBox.SetWord(buf, 2, height);                                // video_height (cropped)
            ToolBox.SetByte(buf, 4, fpsId);                                 // hi nibble: frame_rate, lo nibble: reserved
            ToolBox.SetWord(buf, 5, compositionNumber);                     // composition_number
            buf[7] = compositionState;                                      // 0x80: epoch start, 0x40: acquisition point, 0x00: normal
            buf[8] = (byte)(paletteUpdate ? 0x80 : 0x00);                   // palette_update_flag, 7 bit reserved
            buf[9] = 0;                                                     // palette_id_ref (0..7)
            buf[10] = (byte)objects.Count;                                  // number_of_composition_objects (0..2)
            for (var k = 0; k < objects.Count; k++)
            {
                var obj = objects[k];
                var i = 11 + k * 8;
                ToolBox.SetWord(buf, i, k);                                 // object_id_ref
                buf[i + 2] = (byte)k;                                       // window_id_ref (0..1)
                buf[i + 3] = (byte)(obj.IsForced ? 0x40 : 0x00);            // object_cropped_flag: 0x80, forced_on_flag: 0x40, 6 bit reserved
                ToolBox.SetWord(buf, i + 4, obj.X);                         // composition_object_horizontal_position
                ToolBox.SetWord(buf, i + 6, obj.Y);                         // composition_object_vertical_position
            }

            return buf;
        }

        /// <summary>
        /// Window Definition Segment: one window per object, sized to it.
        /// </summary>
        private static byte[] BuildWds(IList<EncodedObject> objects)
        {
            var buf = new byte[1 + objects.Count * 9];
            buf[0] = (byte)objects.Count;                                   // number_of_windows (0..2)
            for (var k = 0; k < objects.Count; k++)
            {
                var obj = objects[k];
                var i = 1 + k * 9;
                buf[i] = (byte)k;                                           // window_id
                ToolBox.SetWord(buf, i + 1, obj.X);                         // window_horizontal_position
                ToolBox.SetWord(buf, i + 3, obj.Y);                         // window_vertical_position
                ToolBox.SetWord(buf, i + 5, obj.Width);                     // window_width
                ToolBox.SetWord(buf, i + 7, obj.Height);                    // window_height
            }

            return buf;
        }

        /// <summary>
        /// Palette Definition Segment with the alpha of every entry scaled by the percentage of
        /// the object the entry belongs to - the whole of a Blu-ray fade is these numbers
        /// changing between display sets.
        /// </summary>
        private static byte[] BuildPds(BluRaySupPalette pal, int palSize, int paletteVersion, IList<EncodedObject> objects, int[] alphaPercents)
        {
            var entryAlphaPercent = new int[palSize];
            for (var k = 0; k < objects.Count; k++)
            {
                var obj = objects[k];
                for (var i = 0; i < obj.Palette.Count; i++)
                {
                    entryAlphaPercent[obj.PaletteOffset + i] = alphaPercents[k];
                }
            }

            var buf = new byte[2 + palSize * 5];
            var index = 0;
            buf[index++] = 0;                                               // palette_id
            buf[index++] = (byte)paletteVersion;                            // palette_version_number
            var alpha = pal.GetAlpha();
            for (var i = 0; i < palSize; i++)
            {
                buf[index++] = (byte)i;                                     // palette_entry_id
                buf[index++] = pal.GetY()[i];                               // Y
                buf[index++] = pal.GetCr()[i];                              // Cr
                buf[index++] = pal.GetCb()[i];                              // Cb
                buf[index++] = (byte)(alpha[i] * entryAlphaPercent[i] / 100); // T (alpha)
            }

            return buf;
        }

        /// <summary>
        /// Object Definition Segments of one object: the RLE data, split over as many segments
        /// as the 16 bit segment length requires. Only the final fragment carries
        /// last_in_sequence.
        /// </summary>
        private static void WriteOds(SegmentWriter writer, long pts, int objectId, EncodedObject obj)
        {
            var rle = obj.Rle;
            var numAddPackets = 0;
            if (rle.Length > FirstOdsPayload)
            {
                // round up, but without an extra empty packet when the rest divides evenly
                numAddPackets = (rle.Length - FirstOdsPayload + NextOdsPayload - 1) / NextOdsPayload;
            }

            var first = new byte[11];
            ToolBox.SetWord(first, 0, objectId);                            // object_id
            first[2] = 0;                                                   // object_version_number
            var marker = numAddPackets == 0 ? 0xC0000000 : 0x80000000;      // first_in_sequence (0x80), last_in_sequence (0x40)
            ToolBox.SetDWord(first, 3, (uint)(marker | (uint)(rle.Length + 4))); // flags + object_data_length (RLE plus its 4 byte size info)
            ToolBox.SetWord(first, 7, obj.Width);                           // object_width
            ToolBox.SetWord(first, 9, obj.Height);                          // object_height

            var bufSize = Math.Min(rle.Length, FirstOdsPayload);
            writer.Write(OdsSegment, pts, first, rle, 0, bufSize);

            var rleIndex = bufSize;
            var remaining = rle.Length - bufSize;
            for (var p = 0; p < numAddPackets; p++)
            {
                var packetSize = Math.Min(remaining, NextOdsPayload);
                var next = new byte[4];
                ToolBox.SetWord(next, 0, objectId);                         // object_id
                next[2] = 0;                                                // object_version_number
                next[3] = (byte)(p == numAddPackets - 1 ? 0x40 : 0x00);     // last_in_sequence on the final fragment only
                writer.Write(OdsSegment, pts, next, rle, rleIndex, packetSize);
                rleIndex += packetSize;
                remaining -= packetSize;
            }
        }

        /// <summary>
        /// Writes PG segments: "PG", PTS, DTS, type and size, then the payload.
        /// <para>
        /// Timestamps: every segment of a display set carries the display set's presentation
        /// time as PTS, and DTS is left at 0 ("unset"). This mirrors how .sup files extracted
        /// from retail discs look, and is what the common consumers expect: ffmpeg's sup demuxer
        /// explicitly treats DTS 0 as unset, SupMover shifts the PTS of every segment (so they
        /// must all carry the real presentation time), and muxers like tsMuxeR re-derive decode
        /// timing from the PCS when authoring an m2ts. The previous writer put decode-model
        /// values into DTS and zeroed the ODS/END PTS, producing segments with DTS > PTS
        /// (issue #10219).
        /// </para>
        /// </summary>
        private sealed class SegmentWriter
        {
            private readonly System.IO.MemoryStream _stream = new System.IO.MemoryStream();
            private readonly byte[] _header =
            {
                0x50, 0x47,             // 0:  "PG"
                0x00, 0x00, 0x00, 0x00, // 2:  PTS - presentation time stamp
                0x00, 0x00, 0x00, 0x00, // 6:  DTS - decoding time stamp
                0x00,                   // 10: segment_type
                0x00, 0x00              // 11: segment_length (bytes following till next PG)
            };

            public void Write(byte type, long pts, byte[] payload)
            {
                Write(type, pts, payload, null, 0, 0);
            }

            public void Write(byte type, long pts, byte[] payload, byte[] data, int dataOffset, int dataLength)
            {
                ToolBox.SetDWord(_header, 2, (uint)pts);
                ToolBox.SetDWord(_header, 6, 0);
                _header[10] = type;
                ToolBox.SetWord(_header, 11, payload.Length + dataLength);
                _stream.Write(_header, 0, _header.Length);
                _stream.Write(payload, 0, payload.Length);
                if (data != null)
                {
                    _stream.Write(data, dataOffset, dataLength);
                }
            }

            public byte[] ToArray()
            {
                return _stream.ToArray();
            }
        }

        /// <summary>
        /// Create the binary stream representation of one caption
        /// </summary>
        /// <param name="pic">SubPicture object containing caption info - note that first Composition Number should be 0, then 2, 4, 8, etc.</param>
        /// <param name="bmp">Bitmap</param>
        /// <param name="fontColor">Font color used to build the bitmap palette</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="bottomMargin">Image bottom margin</param>
        /// <param name="leftOrRightMargin">Image left/right margin</param>
        /// <param name="alignment">Alignment of image</param>
        /// <param name="overridePosition">Position that overrides alignment</param>
        /// <returns>Byte buffer containing the binary stream representation of one caption</returns>
        public static byte[] CreateSupFrame(BluRaySupPicture pic, SKBitmap bmp, SKColor fontColor, double fps, int bottomMargin, int leftOrRightMargin, BluRayContentAlignment alignment, BluRayPoint overridePosition = null)
        {
            switch (alignment)
            {
                case BluRayContentAlignment.BottomLeft:
                    pic.WindowXOffset = leftOrRightMargin;
                    pic.WindowYOffset = pic.Height - (bmp.Height + bottomMargin);
                    break;
                case BluRayContentAlignment.BottomRight:
                    pic.WindowXOffset = pic.Width - bmp.Width - leftOrRightMargin;
                    pic.WindowYOffset = pic.Height - (bmp.Height + bottomMargin);
                    break;
                case BluRayContentAlignment.MiddleCenter:
                    pic.WindowXOffset = (pic.Width - bmp.Width) / 2;
                    pic.WindowYOffset = (pic.Height - bmp.Height) / 2;
                    break;
                case BluRayContentAlignment.MiddleLeft:
                    pic.WindowXOffset = leftOrRightMargin;
                    pic.WindowYOffset = (pic.Height - bmp.Height) / 2;
                    break;
                case BluRayContentAlignment.MiddleRight:
                    pic.WindowXOffset = pic.Width - bmp.Width - leftOrRightMargin;
                    pic.WindowYOffset = (pic.Height - bmp.Height) / 2;
                    break;
                case BluRayContentAlignment.TopCenter:
                    pic.WindowXOffset = (pic.Width - bmp.Width) / 2;
                    pic.WindowYOffset = bottomMargin;
                    break;
                case BluRayContentAlignment.TopLeft:
                    pic.WindowXOffset = leftOrRightMargin;
                    pic.WindowYOffset = bottomMargin;
                    break;
                case BluRayContentAlignment.TopRight:
                    pic.WindowXOffset = pic.Width - bmp.Width - leftOrRightMargin;
                    pic.WindowYOffset = bottomMargin;
                    break;
                default: // ContentAlignment.BottomCenter:
                    pic.WindowXOffset = (pic.Width - bmp.Width) / 2;
                    pic.WindowYOffset = pic.Height - (bmp.Height + bottomMargin);
                    break;
            }

            if (overridePosition != null &&
                overridePosition.X >= 0 && overridePosition.X < pic.Width &&
                overridePosition.Y >= 0 && overridePosition.Y < pic.Height)
            {
                pic.WindowXOffset = overridePosition.X;
                pic.WindowYOffset = overridePosition.Y;
            }

            var obj = new BluRaySupCompositionObject
            {
                Bitmap = bmp,
                FontColor = fontColor,
                X = pic.WindowXOffset,
                Y = pic.WindowYOffset,
                IsForced = pic.IsForced,
                FadeSteps = pic.FadeSteps,
            };

            var buffer = CreateSupFrame(pic, new[] { obj }, fps, true, out var encoded);
            pic.EncodedBitmap = new BluRaySupEncodedBitmap(encoded[0].Palette, encoded[0].Rle, encoded[0].Width, encoded[0].Height);
            return buffer;
        }

        /// <summary>
        /// Create the binary stream representation of one display set showing
        /// <paramref name="objects"/> together - up to <see cref="MaxCompositionObjects"/> of
        /// them, each in a window of its own, sharing one palette. The display set is an epoch
        /// start at <see cref="StartTime"/>; the fades of the objects follow as palette update
        /// display sets and, when <paramref name="writeClear"/> is set, a screen clear at
        /// <see cref="EndTime"/> ends it. Leave <paramref name="writeClear"/> off when another
        /// epoch starts right at <see cref="EndTime"/> - an epoch start replaces everything on
        /// screen, so a clear before it would only cost a display set.
        /// </summary>
        /// <param name="pic">Frame size, times, forced flag and the composition number of the first display set. <see cref="NextCompositionNumber"/> is set on return.</param>
        /// <param name="objects">The captions to show, one or two. Their windows may not overlap.</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="writeClear">Whether to end with a display set that clears the screen</param>
        /// <returns>Byte buffer containing the binary stream representation of the display sets</returns>
        public static byte[] CreateSupFrame(BluRaySupPicture pic, IList<BluRaySupCompositionObject> objects, double fps, bool writeClear = true)
        {
            return CreateSupFrame(pic, objects, fps, writeClear, out _);
        }

        private static byte[] CreateSupFrame(BluRaySupPicture pic, IList<BluRaySupCompositionObject> objects, double fps, bool writeClear, out List<EncodedObject> encoded)
        {
            if (objects == null || objects.Count == 0)
            {
                throw new ArgumentException("A display set needs at least one composition object", nameof(objects));
            }

            if (objects.Count > MaxCompositionObjects)
            {
                throw new ArgumentException($"A Blu-ray display set can compose at most {MaxCompositionObjects} objects", nameof(objects));
            }

            encoded = EncodeObjects(pic, objects);
            var palSize = 0;
            foreach (var obj in encoded)
            {
                palSize += obj.Palette.Count;
            }

            var pal = new BluRaySupPalette(palSize);
            foreach (var obj in encoded)
            {
                for (var i = 0; i < obj.Palette.Count; i++)
                {
                    pal.SetColor(obj.PaletteOffset + i, obj.Palette[i]);
                }
            }

            var h = pic.Height - 2 * Core.CropOfsY;
            var fpsId = GetFpsId(fps);
            var writer = new SegmentWriter();

            var pts = pic.StartTimeForWrite;
            if (Configuration.Settings.Tools.ExportBluRayRemoveSmallGaps && Math.Abs(_lastEndTimeForWrite - pts) < 100)
            {
                pts = _lastEndTimeForWrite + 1;
            }

            _lastEndTimeForWrite = pic.EndTimeForWrite;
            var endPts = pic.EndTimeForWrite;
            var compositionNumber = pic.CompositionNumber;

            // The caption: PCS (also called the Control Segment), WDS, PDS, the ODS of every
            // object, END.
            writer.Write(PcsSegment, pts, BuildPcs(pic.Width, h, fpsId, compositionNumber, CompositionStateEpochStart, false, encoded));
            writer.Write(WdsSegment, pts, BuildWds(encoded));
            var startAlphaPercents = new int[encoded.Count];
            for (var k = 0; k < encoded.Count; k++)
            {
                startAlphaPercents[k] = encoded[k].StartAlphaPercent;
            }

            writer.Write(PdsSegment, pts, BuildPds(pal, palSize, 0, encoded, startAlphaPercents));
            for (var k = 0; k < encoded.Count; k++)
            {
                WriteOds(writer, pts, k, encoded[k]);
            }

            writer.Write(EndSegment, pts, Array.Empty<byte>());

            // The fade steps - one palette update display set each (PCS + PDS + END). The PCS
            // repeats the composition of the epoch start with palette_update_flag set, which
            // tells the decoder to keep the objects it already has and only take the new palette.
            var updates = MergeFadeSteps(encoded);
            for (var step = 0; step < updates.Count; step++)
            {
                // A step may only be scheduled after the caption is up and before it is taken
                // down; the start PTS can have been nudged by the small gap removal above.
                var stepPts = Math.Min(Math.Max(updates[step].TimeForWrite, pts + 1), endPts - 1);

                compositionNumber++;
                writer.Write(PcsSegment, stepPts, BuildPcs(pic.Width, h, fpsId, compositionNumber, CompositionStateNormal, true, encoded));

                // The palette version has to move for the decoder to take the update; it is a
                // byte, so it wraps on captions with more than 255 steps.
                writer.Write(PdsSegment, stepPts, BuildPds(pal, palSize, (step + 1) & 0xff, encoded, updates[step].AlphaPercents));
                writer.Write(EndSegment, stepPts, Array.Empty<byte>());
            }

            if (writeClear)
            {
                // PCS with no objects, the windows again, END
                compositionNumber++;
                writer.Write(PcsSegment, endPts, BuildPcs(pic.Width, h, fpsId, compositionNumber, CompositionStateNormal, false, Array.Empty<EncodedObject>()));
                writer.Write(WdsSegment, endPts, BuildWds(encoded));
                writer.Write(EndSegment, endPts, Array.Empty<byte>());
            }

            pic.NextCompositionNumber = compositionNumber + 1;
            return writer.ToArray();
        }
    }
}
