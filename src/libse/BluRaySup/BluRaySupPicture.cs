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
        /// <returns>RLE buffer</returns>
        private static byte[] EncodeImage(SKBitmap bm, List<SKColor> palette)
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

            var transparentColor = (byte)palette[palette.Count - 1];
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
                        color = (byte)intC;
                    }
                    else
                    {
                        // FindBestMatch is a linear scan over the (up to 255 entry) palette and
                        // the palette does not change while encoding, so the answer for a color
                        // is fixed. Anti-aliased edges hit this for the same handful of colors
                        // on every row of the caption; remember them in the same lookup.
                        color = FindBestMatch(c, palette);
                        if (lookup.Count < maxCachedColors)
                        {
                            lookup[c] = color;
                        }
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

        private static List<SKColor> GetBitmapPalette(SKBitmap bitmap, SKColor fontColor)
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

                        if (pal.Count >= 254)
                        {
                            break;
                        }
                    }
                }
                if (pal.Count >= 254)
                {
                    break;
                }
            }

            if (pal.Count < 254)
            {
                pal.Add(SKColors.Transparent); // last entry must be transparent
                return pal;
            }


            // get close colors (image has probably been processed in SE)
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
                        else if (pal.Count < 100)
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
                        else if (pal.Count < 240)
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
                        else if (pal.Count < 254)
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

                // Every branch above requires pal.Count < 254, so a full palette freezes the
                // result - the rest of the image only cost scans that could not change it.
                if (pal.Count >= 254)
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

        /// <summary>
        /// Splits <see cref="FadeSteps"/> into the alpha the caption appears with (part of the
        /// epoch's own palette) and the steps that follow it as palette update display sets.
        /// Steps outside the caption, and steps that do not change the alpha, are dropped - each
        /// one would cost a display set for nothing.
        /// </summary>
        private static List<BluRaySupFadeStep> GetFadeSteps(BluRaySupPicture pic, out int startAlphaPercent)
        {
            startAlphaPercent = 100;
            var updates = new List<BluRaySupFadeStep>();
            if (pic.FadeSteps == null || pic.FadeSteps.Count == 0)
            {
                return updates;
            }

            var sorted = new List<BluRaySupFadeStep>(pic.FadeSteps);
            sorted.Sort((a, b) => a.TimeMs.CompareTo(b.TimeMs));

            var lastAlpha = 100;
            var lastTime = long.MinValue;
            foreach (var step in sorted)
            {
                var alpha = Math.Min(100, Math.Max(0, step.AlphaPercent));
                if (step.TimeMs <= pic.StartTime)
                {
                    startAlphaPercent = alpha;
                    lastAlpha = alpha;
                    continue;
                }

                if (step.TimeMs >= pic.EndTime || alpha == lastAlpha || step.TimeMs == lastTime)
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
        /// Writes a Palette Definition Segment with every alpha scaled by
        /// <paramref name="alphaPercent"/> - the whole of a Blu-ray fade is this one number
        /// changing between display sets.
        /// </summary>
        private static int WritePds(byte[] buf, int index, byte[] packetHeader, BluRaySupPalette pal, int palSize, int paletteVersion, int alphaPercent)
        {
            packetHeader[10] = 0x14;                                      // ID (keep PTS & DTS)
            ToolBox.SetWord(packetHeader, 11, 2 + palSize * 5);      // size
            for (var i = 0; i < packetHeader.Length; i++)
            {
                buf[index++] = packetHeader[i];
            }

            buf[index++] = 0;                                             // palette_id
            buf[index++] = (byte)paletteVersion;                          // palette_version_number
            var alpha = pal.GetAlpha();
            for (var i = 0; i < palSize; i++)
            {
                buf[index++] = (byte)i;                                   // index
                buf[index++] = pal.GetY()[i];                             // Y
                buf[index++] = pal.GetCr()[i];                            // Cr
                buf[index++] = pal.GetCb()[i];                            // Cb
                buf[index++] = (byte)(alpha[i] * alphaPercent / 100);     // Alpha
            }

            return index;
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
            var bm = bmp.Copy();
            var colorPalette = GetBitmapPalette(bm, fontColor);
            var pal = new BluRaySupPalette(colorPalette.Count);
            for (var i = 0; i < colorPalette.Count; i++)
            {
                pal.SetColor(i, colorPalette[i]);
            }

            var rleBuf = EncodeImage(bm, colorPalette);

            // for some obscure reason, a packet can be a maximum 0xfffc bytes
            // since 13 bytes are needed for the header("PG", PTS, DTS, ID, SIZE)
            // there are only 0xffef bytes available for the packet
            // since the first ODS packet needs an additional 11 bytes for info
            // and the following ODS packets need 4 additional bytes, the
            // first package can store only 0xffe4 RLE buffer bytes and the
            // following packets can store 0xffeb RLE buffer bytes
            int numAddPackets;
            if (rleBuf.Length <= 0xffe4)
            {
                numAddPackets = 0; // no additional packets needed
            }
            else
            {
                // round up, but without an extra empty packet when the rest divides evenly
                numAddPackets = (rleBuf.Length - 0xffe4 + 0xffeb - 1) / 0xffeb;
            }

            // a typical frame consists of 8 packets. It can be elongated by additional object frames
            var palSize = colorPalette.Count;

            var packetHeader = new byte[]
            {
                0x50, 0x47,             // 0:  "PG"
                0x00, 0x00, 0x00, 0x00, // 2:  PTS - presentation time stamp
                0x00, 0x00, 0x00, 0x00, // 6:  DTS - decoding time stamp
                0x00,                   // 10: segment_type
                0x00, 0x00              // 11: segment_length (bytes following till next PG)
            };
            var headerPcsStart = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, // 0: video_width, video_height
                0x10,                   // 4: hi nibble: frame_rate (0x10=24p), lo nibble: reserved
                0x00, 0x00,             // 5: composition_number (increased by start and end header)
                0x80,                   // 7: composition_state (0x80: epoch start)
                                        //      0x00: Normal
                                        //      0x40: Acquisition Point
                                        //      0x80: Epoch Start
                0x00,                   // 8: palette_update_flag (0x80==true, 0x00==false), 7bit reserved
                0x00,                   // 9: palette_id_ref (0..7)
                0x01,                   // 10: number_of_composition_objects (0..2)
                0x00, 0x00,             // 11: 16bit object_id_ref
                0x00,                   // 13: window_id_ref (0..1)
                0x00,                   // 14: object_cropped_flag: 0x80, forced_on_flag = 0x040, 6bit reserved
                0x00, 0x00, 0x00, 0x00  // 15: composition_object_horizontal_position, composition_object_vertical_position
            };
            var headerPcsEnd = new byte[]
            {
                0x00, 0x00, 0x00, 0x00, // 0: video_width, video_height
                0x10,                   // 4: hi nibble: frame_rate (0x10=24p), lo nibble: reserved
                0x00, 0x00,             // 5: composition_number (increased by start and end header)
                0x00,                   // 7: composition_state (0x00: normal)
                0x00,                   // 8: palette_update_flag (0x80), 7bit reserved
                0x00,                   // 9: palette_id_ref (0..7)
                0x00                    // 10: number_of_composition_objects (0..2)
            };
            var headerWds = new byte[]
            {
                0x01,                   // 0 : number of windows (currently assumed 1, 0..2 is legal)
                0x00,                   // 1 : window id (0..1)
                0x00, 0x00, 0x00, 0x00, // 2 : x-ofs, y-ofs
                0x00, 0x00, 0x00, 0x00  // 6 : width, height
            };
            var headerOdsFirst = new byte[]
            {
                0x00, 0x00,             // 0: object_id
                0x00,                   // 2: object_version_number
                0xC0,                   // 3: first_in_sequence (0x80), last_in_sequence (0x40), 6bits reserved
                0x00, 0x00, 0x00,       // 4: object_data_length - full RLE buffer length (including 4 bytes size info)
                0x00, 0x00, 0x00, 0x00  // 7: object_width, object_height
            };
            var headerOdsNext = new byte[]
            {
                0x00, 0x00,             // 0: object_id
                0x00,                   // 2: object_version_number
                0x00                    // 3: first_in_sequence (0x80), last_in_sequence (0x40), 6bits reserved
                                        //    set per packet below - only the final one is last_in_sequence
            };

            // Fade steps ride along as palette update display sets (PCS + PDS + END) between the
            // caption and the screen clear - same object, only the palette alpha changes.
            var fadeSteps = GetFadeSteps(pic, out var startAlphaPercent);

            var size = packetHeader.Length * (8 + numAddPackets + fadeSteps.Count * 3);
            size += headerPcsStart.Length + headerPcsEnd.Length;
            size += 2 * headerWds.Length + headerOdsFirst.Length;
            size += numAddPackets * headerOdsNext.Length;
            size += (2 + palSize * 5) /* PDS */;
            size += fadeSteps.Count * (headerPcsStart.Length + 2 + palSize * 5);
            size += rleBuf.Length;

            switch (alignment)
            {
                case BluRayContentAlignment.BottomLeft:
                    pic.WindowXOffset = leftOrRightMargin;
                    pic.WindowYOffset = pic.Height - (bm.Height + bottomMargin);
                    break;
                case BluRayContentAlignment.BottomRight:
                    pic.WindowXOffset = pic.Width - bm.Width - leftOrRightMargin;
                    pic.WindowYOffset = pic.Height - (bm.Height + bottomMargin);
                    break;
                case BluRayContentAlignment.MiddleCenter:
                    pic.WindowXOffset = (pic.Width - bm.Width) / 2;
                    pic.WindowYOffset = (pic.Height - bm.Height) / 2;
                    break;
                case BluRayContentAlignment.MiddleLeft:
                    pic.WindowXOffset = leftOrRightMargin;
                    pic.WindowYOffset = (pic.Height - bm.Height) / 2;
                    break;
                case BluRayContentAlignment.MiddleRight:
                    pic.WindowXOffset = pic.Width - bm.Width - leftOrRightMargin;
                    pic.WindowYOffset = (pic.Height - bm.Height) / 2;
                    break;
                case BluRayContentAlignment.TopCenter:
                    pic.WindowXOffset = (pic.Width - bm.Width) / 2;
                    pic.WindowYOffset = bottomMargin;
                    break;
                case BluRayContentAlignment.TopLeft:
                    pic.WindowXOffset = leftOrRightMargin;
                    pic.WindowYOffset = bottomMargin;
                    break;
                case BluRayContentAlignment.TopRight:
                    pic.WindowXOffset = pic.Width - bm.Width - leftOrRightMargin;
                    pic.WindowYOffset = bottomMargin;
                    break;
                default: // ContentAlignment.BottomCenter:
                    pic.WindowXOffset = (pic.Width - bm.Width) / 2;
                    pic.WindowYOffset = pic.Height - (bm.Height + bottomMargin);
                    break;
            }

            if (overridePosition != null &&
                overridePosition.X >= 0 && overridePosition.X < pic.Width &&
                overridePosition.Y >= 0 && overridePosition.Y < pic.Height)
            {
                pic.WindowXOffset = overridePosition.X;
                pic.WindowYOffset = overridePosition.Y;
            }

            var yOfs = pic.WindowYOffset - Core.CropOfsY;
            if (yOfs < 0)
            {
                yOfs = 0;
            }
            else
            {
                var yMax = pic.Height - pic.WindowHeight - 2 * Core.CropOfsY;
                if (yOfs > yMax)
                {
                    yOfs = yMax;
                }
            }

            var h = pic.Height - 2 * Core.CropOfsY;

            var buf = new byte[size];
            var index = 0;

            var fpsId = GetFpsId(fps);

            // Timestamps: every segment of a display set carries the display set's
            // presentation time as PTS, and DTS is left at 0 ("unset"). This mirrors how
            // .sup files extracted from retail discs look, and is what the common consumers
            // expect: ffmpeg's sup demuxer explicitly treats DTS 0 as unset, SupMover shifts
            // the PTS of every segment (so they must all carry the real presentation time),
            // and muxers like tsMuxeR re-derive decode timing from the PCS when authoring
            // an m2ts. The previous writer put decode-model values into DTS and zeroed the
            // ODS/END PTS, producing segments with DTS > PTS (issue #10219).

            // write PCS start - Presentation Composition Segment (also called the Control Segment)
            packetHeader[10] = 0x16; // ID

            var pts = pic.StartTimeForWrite;
            if (Configuration.Settings.Tools.ExportBluRayRemoveSmallGaps && Math.Abs(_lastEndTimeForWrite - pts) < 100)
            {
                pts = _lastEndTimeForWrite + 1;
            }

            _lastEndTimeForWrite = pic.EndTimeForWrite;

            ToolBox.SetDWord(packetHeader, 2, (uint)pts);                     // PTS
            ToolBox.SetDWord(packetHeader, 6, 0);                             // DTS (0 = unset)
            ToolBox.SetWord(packetHeader, 11, headerPcsStart.Length);     // size
            for (var i = 0; i < packetHeader.Length; i++)
            {
                buf[index++] = packetHeader[i];
            }

            ToolBox.SetWord(headerPcsStart, 0, pic.Width);
            ToolBox.SetWord(headerPcsStart, 2, h);                      // cropped height
            ToolBox.SetByte(headerPcsStart, 4, fpsId);
            ToolBox.SetWord(headerPcsStart, 5, pic.CompositionNumber);
            headerPcsStart[14] = (byte)(pic.IsForced ? 0x40 : 0);
            ToolBox.SetWord(headerPcsStart, 15, pic.WindowXOffset);
            ToolBox.SetWord(headerPcsStart, 17, yOfs);
            for (var i = 0; i < headerPcsStart.Length; i++)
            {
                buf[index++] = headerPcsStart[i];
            }

            // write WDS
            packetHeader[10] = 0x17;                                            // ID (keep PTS & DTS)
            ToolBox.SetWord(packetHeader, 11, headerWds.Length);       // size
            for (var i = 0; i < packetHeader.Length; i++)
            {
                buf[index++] = packetHeader[i];
            }

            ToolBox.SetWord(headerWds, 2, pic.WindowXOffset);
            ToolBox.SetWord(headerWds, 4, yOfs);
            ToolBox.SetWord(headerWds, 6, bm.Width);
            ToolBox.SetWord(headerWds, 8, bm.Height);
            for (var i = 0; i < headerWds.Length; i++)
            {
                buf[index++] = headerWds[i];
            }

            // write PDS - Palette Definition Segment
            index = WritePds(buf, index, packetHeader, pal, palSize, 0, startAlphaPercent);

            // write first OBJ
            var bufSize = rleBuf.Length;
            var rleIndex = 0;
            if (bufSize > 0xffe4)
            {
                bufSize = 0xffe4;
            }

            packetHeader[10] = 0x15;                                                    // ID (keep PTS & DTS)
            ToolBox.SetWord(packetHeader, 11, headerOdsFirst.Length + bufSize); // size
            for (var i = 0; i < packetHeader.Length; i++)
            {
                buf[index++] = packetHeader[i];
            }

            var marker = (int)((numAddPackets == 0) ? 0xC0000000 : 0x80000000);
            ToolBox.SetDWord(headerOdsFirst, 3, (uint)(marker | (rleBuf.Length + 4)));
            ToolBox.SetWord(headerOdsFirst, 7, bm.Width);
            ToolBox.SetWord(headerOdsFirst, 9, bm.Height);
            for (var i = 0; i < headerOdsFirst.Length; i++)
            {
                buf[index++] = headerOdsFirst[i];
            }

            Buffer.BlockCopy(rleBuf, rleIndex, buf, index, bufSize);
            index += bufSize;
            rleIndex += bufSize;

            // write additional OBJ packets
            bufSize = rleBuf.Length - bufSize; // remaining bytes to write
            for (var p = 0; p < numAddPackets; p++)
            {
                var psize = bufSize;
                if (psize > 0xffeb)
                {
                    psize = 0xffeb;
                }

                packetHeader[10] = 0x15;                                         // ID (keep DTS & PTS)
                ToolBox.SetWord(packetHeader, 11, headerOdsNext.Length + psize); // size
                for (var i = 0; i < packetHeader.Length; i++)
                {
                    buf[index++] = packetHeader[i];
                }

                // only the final fragment carries last_in_sequence - middle ones must be 0x00
                headerOdsNext[3] = (byte)(p == numAddPackets - 1 ? 0x40 : 0x00);
                for (var i = 0; i < headerOdsNext.Length; i++)
                {
                    buf[index++] = headerOdsNext[i];
                }

                for (var i = 0; i < psize; i++)
                {
                    buf[index++] = rleBuf[rleIndex++];
                }

                bufSize -= psize;
            }

            // write END
            packetHeader[10] = 0x80;                                             // ID (keep PTS & DTS)
            ToolBox.SetWord(packetHeader, 11, 0);                       // size
            for (var i = 0; i < packetHeader.Length; i++)
            {
                buf[index++] = packetHeader[i];
            }

            // write the fade steps - one palette update display set each (PCS + PDS + END). The
            // PCS repeats the composition of the epoch start with palette_update_flag set, which
            // tells the decoder to keep the object it already has and only take the new palette.
            var endPts = pic.EndTimeForWrite;
            var compositionNumber = pic.CompositionNumber;
            headerPcsStart[7] = 0x00;                                            // composition_state: normal case
            headerPcsStart[8] = 0x80;                                            // palette_update_flag
            for (var step = 0; step < fadeSteps.Count; step++)
            {
                // A step may only be scheduled after the caption is up and before it is taken
                // down; the start PTS can have been nudged by the small gap removal above.
                var stepPts = Math.Min(Math.Max(fadeSteps[step].TimeForWrite, pts + 1), endPts - 1);

                compositionNumber++;
                packetHeader[10] = 0x16;                                         // ID
                ToolBox.SetDWord(packetHeader, 2, (uint)stepPts);           // PTS
                ToolBox.SetDWord(packetHeader, 6, 0);                       // DTS (0 = unset)
                ToolBox.SetWord(packetHeader, 11, headerPcsStart.Length);   // size
                for (var i = 0; i < packetHeader.Length; i++)
                {
                    buf[index++] = packetHeader[i];
                }

                ToolBox.SetWord(headerPcsStart, 5, compositionNumber);
                for (var i = 0; i < headerPcsStart.Length; i++)
                {
                    buf[index++] = headerPcsStart[i];
                }

                // The palette version has to move for the decoder to take the update; it is a
                // byte, so it wraps on captions with more than 255 steps.
                index = WritePds(buf, index, packetHeader, pal, palSize, (step + 1) & 0xff, fadeSteps[step].AlphaPercent);

                packetHeader[10] = 0x80;                                         // END (keep PTS & DTS)
                ToolBox.SetWord(packetHeader, 11, 0);                       // size
                for (var i = 0; i < packetHeader.Length; i++)
                {
                    buf[index++] = packetHeader[i];
                }
            }

            // write PCS end
            packetHeader[10] = 0x16;                                            // ID
            ToolBox.SetDWord(packetHeader, 2, (uint)pic.EndTimeForWrite);        // PTS
            ToolBox.SetDWord(packetHeader, 6, 0);                          // DTS (0 = unset)
            ToolBox.SetWord(packetHeader, 11, headerPcsEnd.Length);     // size
            for (var i = 0; i < packetHeader.Length; i++)
            {
                buf[index++] = packetHeader[i];
            }

            ToolBox.SetWord(headerPcsEnd, 0, pic.Width);
            ToolBox.SetWord(headerPcsEnd, 2, h);                                // cropped height
            ToolBox.SetByte(headerPcsEnd, 4, fpsId);
            ToolBox.SetWord(headerPcsEnd, 5, compositionNumber + 1);
            for (var i = 0; i < headerPcsEnd.Length; i++)
            {
                buf[index++] = headerPcsEnd[i];
            }

            // write WDS - Window Definition Segment
            packetHeader[10] = 0x17;                                                     // ID (keep PTS & DTS)
            ToolBox.SetWord(packetHeader, 11, headerWds.Length);                // size
            for (var i = 0; i < packetHeader.Length; i++)
            {
                buf[index++] = packetHeader[i];
            }

            ToolBox.SetWord(headerWds, 2, pic.WindowXOffset);
            ToolBox.SetWord(headerWds, 4, yOfs);
            ToolBox.SetWord(headerWds, 6, bm.Width);
            ToolBox.SetWord(headerWds, 8, bm.Height);
            for (var i = 0; i < headerWds.Length; i++)
            {
                buf[index++] = headerWds[i];
            }

            // write END
            packetHeader[10] = 0x80;                                            // ID (keep PTS & DTS)
            ToolBox.SetWord(packetHeader, 11, 0);                       // size
            for (var i = 0; i < packetHeader.Length; i++)
            {
                buf[index++] = packetHeader[i];
            }

            return buf;
        }
    }
}
