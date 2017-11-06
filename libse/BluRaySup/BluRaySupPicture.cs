﻿/*
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
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Core.BluRaySup
{

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

        public int StartTimeForWrite
        {
            get { return (int)((StartTime - 45) * 90.0); }
        }

        /// <summary>
        /// end time in milliseconds
        /// </summary>
        public long EndTime { get; set; }

        public int EndTimeForWrite
        {
            get { return (int)((EndTime - 45) * 90.0); }
        }

        /// <summary>
        /// if true, this is a forced subtitle
        /// </summary>
        public bool IsForced { get; set; }

        /// <summary>
        /// composition number - increased at start and end PCS
        /// </summary>
        public int CompositionNumber { get; set; }

        /// <summary>
        /// objectID used in decoded object
        /// </summary>
        public int ObjectId { get; set; }

        /// <summary>
        /// list of ODS packets containing image info
        /// </summary>
        public List<ImageObject> ImageObjects;

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
        public List<List<PaletteInfo>> Palettes;

        /// <summary>
        /// Create RLE buffer from bitmap
        /// </summary>
        /// <param name="bm">Bitmap to compress</param>
        /// <param name="palette">Palette used for bitmap encoding</param>
        /// <returns>RLE buffer</returns>
        private static byte[] EncodeImage(NikseBitmap bm, Dictionary<Color, int> palette)
        {
            var bytes = new List<Byte>();
            for (int y = 0; y < bm.Height; y++)
            {
                var ofs = y * bm.Width;
                //eol = false;
                int x;
                int len;
                for (x = 0; x < bm.Width; x += len, ofs += len)
                {
                    Color c = bm.GetPixel(x, y);
                    byte color;
                    if (palette.ContainsKey(c))
                        color = (byte)palette[c];
                    else
                        color = FindBestMatch(c, palette);

                    for (len = 1; x + len < bm.Width; len++)
                        if (bm.GetPixel(x + len, y) != c)
                            break;

                    if (len <= 2 && color != 0)
                    {
                        // only a single occurrence -> add color
                        bytes.Add(color);
                        if (len == 2)
                            bytes.Add(color);
                    }
                    else
                    {
                        if (len > 0x3fff)
                            len = 0x3fff;
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
                if (/*!eol &&*/ x == bm.Width)
                {
                    bytes.Add(0); // rle id
                    bytes.Add(0);
                }
            }
            int size = bytes.Count;
            var retval = new byte[size];
            for (int i = 0; i < size; i++)
                retval[i] = bytes[i];
            return retval;
        }

        private static byte FindBestMatch(Color color, Dictionary<Color, int> palette)
        {
            int smallestDiff = 1000;
            int smallestDiffIndex = -1;
            foreach (var kvp in palette)
            {
                int diff = Math.Abs(kvp.Key.A - color.A) + Math.Abs(kvp.Key.R - color.R) + Math.Abs(kvp.Key.G - color.G) + Math.Abs(kvp.Key.B - color.B);
                if (diff < smallestDiff)
                {
                    smallestDiff = diff;
                    smallestDiffIndex = kvp.Value;
                }
            }
            return (byte)smallestDiffIndex;
        }

        private static bool HasCloseColor(Color color, Dictionary<Color, int> palette, int maxDifference)
        {
            foreach (var kvp in palette)
            {
                int difference = Math.Abs(kvp.Key.A - color.A) + Math.Abs(kvp.Key.R - color.R) + Math.Abs(kvp.Key.G - color.G) + Math.Abs(kvp.Key.B - color.B);
                if (difference < maxDifference)
                    return true;
            }
            return false;
        }

        private static Dictionary<Color, int> GetBitmapPalette(NikseBitmap bitmap)
        {
            var pal = new Dictionary<Color, int>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var c = bitmap.GetPixel(x, y);
                    if (c != Color.Transparent)
                    {
                        if (pal.Count < 100)
                        {
                            if (!HasCloseColor(c, pal, 1))
                                pal.Add(c, pal.Count);
                        }
                        else if (pal.Count < 240)
                        {
                            if (!HasCloseColor(c, pal, 5))
                                pal.Add(c, pal.Count);
                        }
                        else if (pal.Count < 254 && !HasCloseColor(c, pal, 25))
                        { 
                            pal.Add(c, pal.Count);
                        }
                    }
                }
            }
            pal.Add(Color.Transparent, pal.Count); // last entry must be transparent
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
                return 0x20;
            if (Math.Abs(fps - Core.FpsPal) < 0.01) // 25
                return 0x30;
            if (Math.Abs(fps - Core.FpsNtsc) < 0.01) // 29.97
                return 0x40;
            if (Math.Abs(fps - Core.FpsPalI) < 0.01) // 50
                return 0x60;
            if (Math.Abs(fps - Core.FpsNtscI) < 0.1) // 59.94
                return 0x70;

            return 0x10; // 23.976
        }

        /// <summary>
        /// Create the binary stream representation of one caption
        /// </summary>
        /// <param name="pic">SubPicture object containing caption info - note that first Composition Number should be 0, then 2, 4, 8, etc.</param>
        /// <param name="bmp">Bitmap</param>
        /// <param name="fps">Frames per second</param>
        /// <param name="bottomMargin">Image bottom margin</param>
        /// <param name="leftOrRightMargin">Image left/right margin</param>
        /// <param name="alignment">Alignment of image</param>
        /// <param name="overridePosition">Position that overrides alignment</param>
        /// <returns>Byte buffer containing the binary stream representation of one caption</returns>
        public static byte[] CreateSupFrame(BluRaySupPicture pic, Bitmap bmp, double fps, int bottomMargin, int leftOrRightMargin, ContentAlignment alignment, Point? overridePosition = null)
        {
            var bm = new NikseBitmap(bmp);
            var colorPalette = GetBitmapPalette(bm);
            var pal = new BluRaySupPalette(colorPalette.Count);
            int k = 0;
            foreach (var kvp in colorPalette)
            {
                pal.SetColor(k, kvp.Key);
                k++;
            }

            byte[] rleBuf = EncodeImage(bm, colorPalette);

            // for some obscure reason, a packet can be a maximum 0xfffc bytes
            // since 13 bytes are needed for the header("PG", PTS, DTS, ID, SIZE)
            // there are only 0xffef bytes available for the packet
            // since the first ODS packet needs an additional 11 bytes for info
            // and the following ODS packets need 4 additional bytes, the
            // first package can store only 0xffe4 RLE buffer bytes and the
            // following packets can store 0xffeb RLE buffer bytes
            int numAddPackets;
            if (rleBuf.Length <= 0xffe4)
                numAddPackets = 0; // no additional packets needed;
            else
                numAddPackets = 1 + (rleBuf.Length - 0xffe4) / 0xffeb;

            // a typical frame consists of 8 packets. It can be enlonged by additional
            // object frames
            int palSize = colorPalette.Count;

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
                0x00,                   // 8: palette_update_flag (0x80), 7bit reserved
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
                0x40                    // 3: first_in_sequence (0x80), last_in_sequence (0x40), 6bits reserved
            };

            int size = packetHeader.Length * (8 + numAddPackets);
            size += headerPcsStart.Length + headerPcsEnd.Length;
            size += 2 * headerWds.Length + headerOdsFirst.Length;
            size += numAddPackets * headerOdsNext.Length;
            size += (2 + palSize * 5) /* PDS */;
            size += rleBuf.Length;

            switch (alignment)
            {
                case ContentAlignment.BottomLeft:
                    pic.WindowXOffset = leftOrRightMargin;
                    pic.WindowYOffset = pic.Height - (bm.Height + bottomMargin);
                    break;
                case ContentAlignment.BottomRight:
                    pic.WindowXOffset = pic.Width - bm.Width - bottomMargin;
                    pic.WindowYOffset = pic.Height - (bm.Height + leftOrRightMargin);
                    break;
                case ContentAlignment.MiddleCenter:
                    pic.WindowXOffset = (pic.Width - bm.Width) / 2;
                    pic.WindowYOffset = (pic.Height - bm.Height) / 2;
                    break;
                case ContentAlignment.MiddleLeft:
                    pic.WindowXOffset = leftOrRightMargin;
                    pic.WindowYOffset = (pic.Height - bm.Height) / 2;
                    break;
                case ContentAlignment.MiddleRight:
                    pic.WindowXOffset = pic.Width - bm.Width - leftOrRightMargin;
                    pic.WindowYOffset = (pic.Height - bm.Height) / 2;
                    break;
                case ContentAlignment.TopCenter:
                    pic.WindowXOffset = (pic.Width - bm.Width) / 2;
                    pic.WindowYOffset = bottomMargin;
                    break;
                case ContentAlignment.TopLeft:
                    pic.WindowXOffset = leftOrRightMargin;
                    pic.WindowYOffset = bottomMargin;
                    break;
                case ContentAlignment.TopRight:
                    pic.WindowXOffset = pic.Width - bm.Width - leftOrRightMargin;
                    pic.WindowYOffset = bottomMargin;
                    break;
                default: // ContentAlignment.BottomCenter:
                    pic.WindowXOffset = (pic.Width - bm.Width) / 2;
                    pic.WindowYOffset = pic.Height - (bm.Height + bottomMargin);
                    break;
            }

            if (overridePosition != null &&
                overridePosition.Value.X >= 0 && overridePosition.Value.X < bm.Width &&
                overridePosition.Value.Y >= 0 && overridePosition.Value.Y < bm.Height)
            {
                pic.WindowXOffset = overridePosition.Value.X;
                pic.WindowYOffset = overridePosition.Value.Y;
            }

            int yOfs = pic.WindowYOffset - Core.CropOfsY;
            if (yOfs < 0)
            {
                yOfs = 0;
            }
            else
            {
                int yMax = pic.Height - pic.WindowHeight - 2 * Core.CropOfsY;
                if (yOfs > yMax)
                    yOfs = yMax;
            }

            int h = pic.Height - 2 * Core.CropOfsY;

            var buf = new byte[size];
            int index = 0;

            int fpsId = GetFpsId(fps);

            /* time (in 90kHz resolution) needed to initialize (clear) the screen buffer
                based on the composition pixel rate of 256e6 bit/s - always rounded up */
            int frameInitTime = (pic.Width * pic.Height * 9 + 3199) / 3200; // better use default height here
            /* time (in 90kHz resolution) needed to initialize (clear) the window area
                based on the composition pixel rate of 256e6 bit/s - always rounded up
                Note: no cropping etc. -> window size == image size */
            int windowInitTime = (bm.Width * bm.Height * 9 + 3199) / 3200;
            /* time (in 90kHz resolution) needed to decode the image
                based on the decoding pixel rate of 128e6 bit/s - always rounded up  */
            int imageDecodeTime = (bm.Width * bm.Height * 9 + 1599) / 1600;
            // write PCS start
            packetHeader[10] = 0x16;                                            // ID
            int dts = pic.StartTimeForWrite - (frameInitTime + windowInitTime + imageDecodeTime); //int dts = pic.StartTimeForWrite - windowInitTime; ???

            ToolBox.SetDWord(packetHeader, 2, pic.StartTimeForWrite);           // PTS
            ToolBox.SetDWord(packetHeader, 6, dts);                             // DTS
            ToolBox.SetWord(packetHeader, 11, headerPcsStart.Length);           // size
            for (int i = 0; i < packetHeader.Length; i++)
                buf[index++] = packetHeader[i];
            ToolBox.SetWord(headerPcsStart, 0, pic.Width);
            ToolBox.SetWord(headerPcsStart, 2, h);                              // cropped height
            ToolBox.SetByte(headerPcsStart, 4, fpsId);
            ToolBox.SetWord(headerPcsStart, 5, pic.CompositionNumber);
            headerPcsStart[14] = (byte)(pic.IsForced ? 0x40 : 0);
            ToolBox.SetWord(headerPcsStart, 15, pic.WindowXOffset);
            ToolBox.SetWord(headerPcsStart, 17, yOfs);
            for (int i = 0; i < headerPcsStart.Length; i++)
                buf[index++] = headerPcsStart[i];

            // write WDS
            packetHeader[10] = 0x17;                                            // ID
            int timestamp = pic.StartTimeForWrite - windowInitTime;
            ToolBox.SetDWord(packetHeader, 2, timestamp);                       // PTS (keep DTS)
            ToolBox.SetWord(packetHeader, 11, headerWds.Length);                // size
            for (int i = 0; i < packetHeader.Length; i++)
                buf[index++] = packetHeader[i];
            ToolBox.SetWord(headerWds, 2, pic.WindowXOffset);
            ToolBox.SetWord(headerWds, 4, yOfs);
            ToolBox.SetWord(headerWds, 6, bm.Width);
            ToolBox.SetWord(headerWds, 8, bm.Height);
            for (int i = 0; i < headerWds.Length; i++)
                buf[index++] = headerWds[i];

            // write PDS
            packetHeader[10] = 0x14;                                            // ID
            ToolBox.SetDWord(packetHeader, 2, dts);                             // PTS (=DTS of PCS/WDS)
            ToolBox.SetDWord(packetHeader, 6, 0);                               // DTS (0)
            ToolBox.SetWord(packetHeader, 11, (2 + palSize * 5));               // size
            for (int i = 0; i < packetHeader.Length; i++)
                buf[index++] = packetHeader[i];
            buf[index++] = 0;
            buf[index++] = 0;
            for (int i = 0; i < palSize; i++)
            {
                buf[index++] = (byte)i;                                         // index
                buf[index++] = pal.GetY()[i];                                   // Y
                buf[index++] = pal.GetCr()[i];                                  // Cr
                buf[index++] = pal.GetCb()[i];                                  // Cb
                buf[index++] = pal.GetAlpha()[i];                               // Alpha
            }

            // write first OBJ
            int bufSize = rleBuf.Length;
            int rleIndex = 0;
            if (bufSize > 0xffe4)
                bufSize = 0xffe4;
            packetHeader[10] = 0x15;                                            // ID
            timestamp = dts + imageDecodeTime;
            ToolBox.SetDWord(packetHeader, 2, timestamp);                       // PTS
            ToolBox.SetDWord(packetHeader, 6, dts);                             // DTS
            ToolBox.SetWord(packetHeader, 11, headerOdsFirst.Length + bufSize); // size
            for (int i = 0; i < packetHeader.Length; i++)
                buf[index++] = packetHeader[i];
            int marker = (int)((numAddPackets == 0) ? 0xC0000000 : 0x80000000);
            ToolBox.SetDWord(headerOdsFirst, 3, marker | (rleBuf.Length + 4));
            ToolBox.SetWord(headerOdsFirst, 7, bm.Width);
            ToolBox.SetWord(headerOdsFirst, 9, bm.Height);
            for (int i = 0; i < headerOdsFirst.Length; i++)
                buf[index++] = headerOdsFirst[i];
            for (int i = 0; i < bufSize; i++)
                buf[index++] = rleBuf[rleIndex++];

            // write additional OBJ packets
            bufSize = rleBuf.Length - bufSize; // remaining bytes to write
            for (int p = 0; p < numAddPackets; p++)
            {
                int psize = bufSize;
                if (psize > 0xffeb)
                    psize = 0xffeb;
                packetHeader[10] = 0x15;                                         // ID (keep DTS & PTS)
                ToolBox.SetWord(packetHeader, 11, headerOdsNext.Length + psize); // size
                for (int i = 0; i < packetHeader.Length; i++)
                    buf[index++] = packetHeader[i];
                for (int i = 0; i < headerOdsNext.Length; i++)
                    buf[index++] = headerOdsNext[i];
                for (int i = 0; i < psize; i++)
                    buf[index++] = rleBuf[rleIndex++];
                bufSize -= psize;
            }

            // write END
            packetHeader[10] = 0x80;                                            // ID
            ToolBox.SetDWord(packetHeader, 6, 0);                               // DTS (0) (keep PTS of ODS)
            ToolBox.SetWord(packetHeader, 11, 0);                               // size
            for (int i = 0; i < packetHeader.Length; i++)
                buf[index++] = packetHeader[i];

            // write PCS end
            packetHeader[10] = 0x16;                                            // ID
            ToolBox.SetDWord(packetHeader, 2, pic.EndTimeForWrite);             // PTS
            dts = pic.EndTimeForWrite - 1; //dts = pic.StartTimeForWrite - 1;
            ToolBox.SetDWord(packetHeader, 6, dts);                             // DTS
            ToolBox.SetWord(packetHeader, 11, headerPcsEnd.Length);             // size
            for (int i = 0; i < packetHeader.Length; i++)
                buf[index++] = packetHeader[i];
            ToolBox.SetWord(headerPcsEnd, 0, pic.Width);
            ToolBox.SetWord(headerPcsEnd, 2, h);                                // cropped height
            ToolBox.SetByte(headerPcsEnd, 4, fpsId);
            ToolBox.SetWord(headerPcsEnd, 5, pic.CompositionNumber + 1);
            for (int i = 0; i < headerPcsEnd.Length; i++)
                buf[index++] = headerPcsEnd[i];

            // write WDS
            packetHeader[10] = 0x17;                                            // ID
            timestamp = pic.EndTimeForWrite - windowInitTime;
            ToolBox.SetDWord(packetHeader, 2, timestamp);                       // PTS (keep DTS of PCS)
            ToolBox.SetWord(packetHeader, 11, headerWds.Length);                // size
            for (int i = 0; i < packetHeader.Length; i++)
                buf[index++] = packetHeader[i];
            ToolBox.SetWord(headerWds, 2, pic.WindowXOffset);
            ToolBox.SetWord(headerWds, 4, yOfs);
            ToolBox.SetWord(headerWds, 6, bm.Width);
            ToolBox.SetWord(headerWds, 8, bm.Height);
            for (int i = 0; i < headerWds.Length; i++)
                buf[index++] = headerWds[i];

            // write END
            packetHeader[10] = 0x80;                                            // ID
            ToolBox.SetDWord(packetHeader, 2, dts);                             // PTS (DTS of end PCS)
            ToolBox.SetDWord(packetHeader, 6, 0);                               // DTS (0)
            ToolBox.SetWord(packetHeader, 11, 0);                               // size
            for (int i = 0; i < packetHeader.Length; i++)
                buf[index++] = packetHeader[i];

            return buf;
        }

    }
}
