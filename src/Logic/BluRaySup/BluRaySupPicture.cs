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
 */

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Logic.BluRaySup
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

        /// <summary>
        /// end time in milliseconds
        /// </summary>
        public long EndTime { get; set; }

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

        /** list of (list of) palette info - there are up to 8 palettes per epoch, each can be updated several times */
        public List<List<PaletteInfo>> Palettes;

        private static byte[] packetHeader =
        {
            0x50, 0x47,             // 0:  "PG"
            0x00, 0x00, 0x00, 0x00, // 2:  PTS - presentation time stamp
            0x00, 0x00, 0x00, 0x00, // 6:  DTS - decoding time stamp
            0x00,                   // 10: segment_type
            0x00, 0x00,             // 11: segment_length (bytes following till next PG)
        };

        private static byte[] headerPCSStart =
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

        private static byte[] headerPCSEnd =
        {
            0x00, 0x00, 0x00, 0x00, // 0: video_width, video_height
            0x10,                   // 4: hi nibble: frame_rate (0x10=24p), lo nibble: reserved
            0x00, 0x00,             // 5: composition_number (increased by start and end header)
            0x00,                   // 7: composition_state (0x00: normal)
            0x00,                   // 8: palette_update_flag (0x80), 7bit reserved
            0x00,                   // 9: palette_id_ref (0..7)
            0x00,                   // 10: number_of_composition_objects (0..2)
        };

        private static byte[] headerODSFirst =
        {
            0x00, 0x00,             // 0: object_id
            0x00,                   // 2: object_version_number
            0xC0,                   // 3: first_in_sequence (0x80), last_in_sequence (0x40), 6bits reserved
            0x00, 0x00, 0x00,       // 4: object_data_length - full RLE buffer length (including 4 bytes size info)
            0x00, 0x00, 0x00, 0x00, // 7: object_width, object_height
        };

        private static byte[] headerODSNext =
        {
            0x00, 0x00,             // 0: object_id
            0x00,                   // 2: object_version_number
            0x40,                   // 3: first_in_sequence (0x80), last_in_sequence (0x40), 6bits reserved
        };

        private static byte[] headerWDS =
        {
            0x01,                   // 0 : number of windows (currently assumed 1, 0..2 is legal)
            0x00,                   // 1 : window id (0..1)
            0x00, 0x00, 0x00, 0x00, // 2 : x-ofs, y-ofs
            0x00, 0x00, 0x00, 0x00  // 6 : width, height
        };

        public BluRaySupPicture(BluRaySupPicture subPicture)
        {
            Width = subPicture.Width;
            Height = subPicture.Height;
            StartTime = subPicture.StartTime;
            EndTime = subPicture.EndTime;
            IsForced = subPicture.IsForced;
            CompositionNumber = subPicture.CompositionNumber;

            ObjectId = subPicture.ObjectId;
            ImageObjects = new List<ImageObject>();
            foreach (ImageObject io in subPicture.ImageObjects)
                ImageObjects.Add(io);
            WindowWidth = subPicture.WindowWidth;
            WindowHeight = subPicture.WindowHeight;
            WindowXOffset = subPicture.WindowXOffset;
            WindowYOffset = subPicture.WindowYOffset;
            FramesPerSecondType = subPicture.FramesPerSecondType;
            Palettes = new List<List<PaletteInfo>>();
            foreach (List<PaletteInfo> palette in subPicture.Palettes)
            {
                List<PaletteInfo> p = new List<PaletteInfo>();
                foreach (PaletteInfo pi in palette)
                {
                    p.Add(new PaletteInfo(pi));
                }
                Palettes.Add(p);
            }
        }

        public BluRaySupPicture()
        {
        }

        public ImageObject GetImageObject(int index)
        {
            return ImageObjects[index];
        }

        internal ImageObject ObjectIdImage
        {
            get
            {
                if (ImageObjects == null)
                {
                    System.Diagnostics.Debug.WriteLine("Invalid Blu-ray SupPicture - ImageObjects is null - BluRaySupPictures.cs: internal ImageObject ObjectIdImage");
                    return null;
                }
                else if (ObjectId < ImageObjects.Count)
                {
                    return ImageObjects[ObjectId];
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Invalid Blu-ray SupPicture index - BluRaySupPictures.cs: internal ImageObject ObjectIdImage");
                    return ImageObjects[ImageObjects.Count - 1];
                }
            }
        }

        /// <summary>
        /// decode palette from the input stream
        /// </summary>
        /// <param name="pic">SubPicture object containing info about the current caption</param>
        /// <returns>Palette object</returns>
        public BluRaySupPalette DecodePalette(BluRaySupPalette defaultPalette)
        {
            BluRaySupPicture pic = this;
            bool fadeOut = false;
            if (pic.Palettes.Count == 0 || pic.ObjectIdImage.PaletteId >= pic.Palettes.Count)
            {
                System.Diagnostics.Debug.Print("Palette not found in objectID=" + pic.ObjectId + " PaletteId=" + pic.ObjectIdImage.PaletteId + "!");
                if (defaultPalette == null)
                    return new BluRaySupPalette(256, Core.UsesBt601());
                else
                    return new BluRaySupPalette(defaultPalette);
            }
            List<PaletteInfo> pl = pic.Palettes[pic.ObjectIdImage.PaletteId];
            BluRaySupPalette palette = new BluRaySupPalette(256, Core.UsesBt601());
            // by definition, index 0xff is always completely transparent
            // also all entries must be fully transparent after initialization

            for (int j = 0; j < pl.Count; j++)
            {
                PaletteInfo p = pl[j];
                int index = 0;

                for (int i = 0; i < p.PaletteSize; i++)
                {
                    // each palette entry consists of 5 bytes
                    int palIndex = p.PaletteBuffer[index];
                    int y = p.PaletteBuffer[++index];
                    int cr, cb;
                    if (Core.GetSwapCrCb())
                    {
                        cb = p.PaletteBuffer[++index];
                        cr = p.PaletteBuffer[++index];
                    }
                    else
                    {
                        cr = p.PaletteBuffer[++index];
                        cb = p.PaletteBuffer[++index];
                    }
                    int alpha = p.PaletteBuffer[++index];

                    int alphaOld = palette.GetAlpha(palIndex);
                    // avoid fading out
                    if (alpha >= alphaOld || alpha == 0)
                    {
                        if (alpha < Core.GetAlphaCrop())
                        {// to not mess with scaling algorithms, make transparent color black
                            y = 16;
                            cr = 128;
                            cb = 128;
                        }
                        palette.SetAlpha(palIndex, alpha);
                    }
                    else
                    {
                        fadeOut = true;
                    }

                    palette.SetYCbCr(palIndex, y, cb, cr);
                    index++;
                }
            }
            if (fadeOut)
                System.Diagnostics.Debug.Print("fade out detected -> patched palette\n");
            return palette;
        }

        /// <summary>
        /// Decode caption from the input stream
        /// </summary>
        /// <param name="pic">SubPicture object containing info about the caption</param>
        /// <param name="transIdx">index of the transparent color</param>
        /// <returns>bitmap of the decoded caption</returns>
        public Bitmap DecodeImage(BluRaySupPalette defaultPalette)
        {
            if (ObjectIdImage == null)
                return new Bitmap(1, 1);

            int w = ObjectIdImage.Width;
            int h = ObjectIdImage.Height;

            if (w > Width || h > Height)
                throw new Exception("Subpicture too large: " + w + "x" + h);

            //Bitmap bm = new Bitmap(w, h);
            FastBitmap bm = new FastBitmap(new Bitmap(w, h));
            bm.LockImage();
            BluRaySupPalette pal = DecodePalette(defaultPalette);

            int index = 0;
            int ofs = 0;
            int xpos = 0;

            // just for multi-packet support, copy all of the image data in one common buffer
            byte[] buf = new byte[ObjectIdImage.BufferSize];
            foreach (ImageObjectFragment fragment in ObjectIdImage.Fragments)
            {
                Buffer.BlockCopy(fragment.ImageBuffer, 0, buf, index, fragment.ImagePacketSize);
                index += fragment.ImagePacketSize;
            }


            index = 0;
            do
            {
                int b = buf[index++] & 0xff;
                if (b == 0)
                {
                    b = buf[index++] & 0xff;
                    if (b == 0)
                    {
                        // next line
                        ofs = (ofs / w) * w;
                        if (xpos < w)
                            ofs += w;
                        xpos = 0;
                    }
                    else
                    {
                        int size;
                        if ((b & 0xC0) == 0x40)
                        {
                            // 00 4x xx -> xxx zeroes
                            size = ((b - 0x40) << 8) + (buf[index++] & 0xff);
                            for (int i = 0; i < size; i++)
                                PutPixel(bm, ofs++, 0, pal);
                            xpos += size;
                        }
                        else if ((b & 0xC0) == 0x80)
                        {
                            // 00 8x yy -> x times value y
                            size = (b - 0x80);
                            b = buf[index++] & 0xff;
                            for (int i = 0; i < size; i++)
                                PutPixel(bm, ofs++, b, pal);
                            xpos += size;
                        }
                        else if ((b & 0xC0) != 0)
                        {
                            // 00 cx yy zz -> xyy times value z
                            size = ((b - 0xC0) << 8) + (buf[index++] & 0xff);
                            b = buf[index++] & 0xff;
                            for (int i = 0; i < size; i++)
                                PutPixel(bm, ofs++, b, pal);
                            xpos += size;
                        }
                        else
                        {
                            // 00 xx -> xx times 0
                            for (int i = 0; i < b; i++)
                                PutPixel(bm, ofs++, 0, pal);
                            xpos += b;
                        }
                    }
                }
                else
                {
                    PutPixel(bm, ofs++, b, pal);
                    xpos++;
                }
            } while (index < buf.Length);

            bm.UnlockImage();
            return bm.GetBitmap();
        }

        private static void PutPixel(FastBitmap bmp, int index, int color, BluRaySupPalette palette)
        {
            int x = index % bmp.Width;
            int y = index / bmp.Width;
            if (color >= 0 && x < bmp.Width && y < bmp.Height)
                bmp.SetPixel(x, y, Color.FromArgb(palette.GetArgb(color)));
        }

        /// <summary>
        /// Create RLE buffer from bitmap
        /// </summary>
        /// <param name="bm">bitmap to compress</param>
        /// <returns>RLE buffer</returns>
        private static byte[] EncodeImage(Bitmap bm, Dictionary<Color, int> palette)
        {
            List<Byte> bytes = new List<Byte>();
            byte color = 0;
            int ofs = 0;
            int len = 0;
            //boolean eol;            
            for (int y = 0; y < bm.Height; y++)
            {
                ofs = y * bm.Width;
                //eol = false;
                int x;
                for (x = 0; x < bm.Width; x += len, ofs += len)
                {
                    Color c = bm.GetPixel(x, y);
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
            byte[] retval = new byte[size];
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

        private static Dictionary<Color, int> GetBitmapPalette(Bitmap bitmap)
        {
            Dictionary<Color, int> pal = new Dictionary<Color, int>();
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color c = bitmap.GetPixel(x, y);
                    if (!pal.ContainsKey(c) && c != Color.Transparent && pal.Count < 255)
                        pal.Add(c, pal.Count);
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
        private static int getFpsId(double fps)
        {
            if (fps == Core.Fps24Hz)
                return 0x20;
            if (fps == Core.FpsPal)
                return 0x30;
            if (fps == Core.FpsNtsc)
                return 0x40;
            if (fps == Core.FpsPalI)
                return 0x60;
            if (fps == Core.FpsNtscI)
                return 0x70;
            // assume FPS_24P (also for FPS_23_975)
            return 0x10;
        }

        /// <summary>
        /// Create the binary stream representation of one caption
        /// </summary>
        /// <param name="pic">SubPicture object containing caption info</param>
        /// <param name="bm">bitmap</param>
        /// <param name="pal">palette</param>
        /// <returns>byte buffer containing the binary stream representation of one caption</returns>
        public static byte[] CreateSupFrame(BluRaySupPicture pic, Bitmap bm)
        {
            int size = 0;
            var colorPalette = GetBitmapPalette(bm);
            BluRaySupPalette pal = new BluRaySupPalette(colorPalette.Count);
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

            size = packetHeader.Length * (8 + numAddPackets);
            size += headerPCSStart.Length + headerPCSEnd.Length;
            size += 2 * headerWDS.Length + headerODSFirst.Length;
            size += numAddPackets * headerODSNext.Length;
            size += (2 + palSize * 5) /* PDS */;
            size += rleBuf.Length;

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

            byte[] buf = new byte[size];
                int index = 0;

                int fpsId = getFpsId(Core.fpsTrg);

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
                packetHeader[10] = 0x16;											// ID
                int dts = (int)pic.StartTime - (frameInitTime + windowInitTime);
                ToolBox.SetDWord(packetHeader, 2, (int)pic.StartTime);				// PTS
                ToolBox.SetDWord(packetHeader, 6, dts);								// DTS
                ToolBox.SetWord(packetHeader, 11, headerPCSStart.Length);			// size
                for (int i = 0; i < packetHeader.Length; i++)
                    buf[index++] = packetHeader[i];
                ToolBox.SetWord(headerPCSStart, 0, pic.Width);
                ToolBox.SetWord(headerPCSStart, 2, h);								// cropped height
                ToolBox.SetByte(headerPCSStart, 4, fpsId);
                ToolBox.SetWord(headerPCSStart, 5, pic.CompositionNumber);
                headerPCSStart[14] = (byte)(pic.IsForced ? (byte)0x40 : 0);
                ToolBox.SetWord(headerPCSStart, 15, pic.WindowXOffset);
                ToolBox.SetWord(headerPCSStart, 17, yOfs);
                for (int i = 0; i < headerPCSStart.Length; i++)
                    buf[index++] = headerPCSStart[i];

                // write WDS
                packetHeader[10] = 0x17;											// ID
                int timeStamp = (int)pic.StartTime - windowInitTime;
                ToolBox.SetDWord(packetHeader, 2, timeStamp);						// PTS (keep DTS)
                ToolBox.SetWord(packetHeader, 11, headerWDS.Length);				// size
                for (int i = 0; i < packetHeader.Length; i++)
                    buf[index++] = packetHeader[i];
                ToolBox.SetWord(headerWDS, 2, pic.WindowXOffset);
                ToolBox.SetWord(headerWDS, 4, yOfs);
                ToolBox.SetWord(headerWDS, 6, bm.Width);
                ToolBox.SetWord(headerWDS, 8, bm.Height);
                for (int i = 0; i < headerWDS.Length; i++)
                    buf[index++] = headerWDS[i];

                // write PDS
                packetHeader[10] = 0x14;											// ID
                ToolBox.SetDWord(packetHeader, 2, dts);								// PTS (=DTS of PCS/WDS)
                ToolBox.SetDWord(packetHeader, 6, 0);								// DTS (0)
                ToolBox.SetWord(packetHeader, 11, (2 + palSize * 5));				// size
                for (int i = 0; i < packetHeader.Length; i++)
                    buf[index++] = packetHeader[i];
                buf[index++] = 0;
                buf[index++] = 0;
                for (int i = 0; i < palSize; i++)
                {
                    buf[index++] = (byte)i;											// index
                    buf[index++] = pal.GetY()[i];									// Y
                    buf[index++] = pal.GetCr()[i];									// Cr
                    buf[index++] = pal.GetCb()[i];									// Cb
                    buf[index++] = pal.GetAlpha()[i];								// Alpha
                }

                // write first OBJ
                int bufSize = rleBuf.Length;
                int rleIndex = 0;
                if (bufSize > 0xffe4)
                    bufSize = 0xffe4;
                packetHeader[10] = 0x15;											// ID
                timeStamp = dts + imageDecodeTime;
                ToolBox.SetDWord(packetHeader, 2, timeStamp);						// PTS
                ToolBox.SetDWord(packetHeader, 6, dts);								// DTS
                ToolBox.SetWord(packetHeader, 11, headerODSFirst.Length + bufSize);	// size
                for (int i = 0; i < packetHeader.Length; i++)
                    buf[index++] = packetHeader[i];
                int marker = (int)((numAddPackets == 0) ? 0xC0000000 : 0x80000000);
                ToolBox.SetDWord(headerODSFirst, 3, marker | (rleBuf.Length + 4));
                ToolBox.SetWord(headerODSFirst, 7, bm.Width);
                ToolBox.SetWord(headerODSFirst, 9, bm.Height);
                for (int i = 0; i < headerODSFirst.Length; i++)
                    buf[index++] = headerODSFirst[i];
                for (int i = 0; i < bufSize; i++)
                    buf[index++] = rleBuf[rleIndex++];

                // write additional OBJ packets
                bufSize = rleBuf.Length - bufSize; // remaining bytes to write
                for (int p = 0; p < numAddPackets; p++)
                {
                    int psize = bufSize;
                    if (psize > 0xffeb)
                        psize = 0xffeb;
                    packetHeader[10] = 0x15;                                            // ID (keep DTS & PTS)
                    ToolBox.SetWord(packetHeader, 11, headerODSNext.Length + psize);    // size
                    for (int i = 0; i < packetHeader.Length; i++)
                        buf[index++] = packetHeader[i];
                    for (int i = 0; i < headerODSNext.Length; i++)
                        buf[index++] = headerODSNext[i];
                    for (int i = 0; i < psize; i++)
                        buf[index++] = rleBuf[rleIndex++];
                    bufSize -= psize;
                }

                // write END
                packetHeader[10] = (byte)0x80;										// ID
                ToolBox.SetDWord(packetHeader, 6, 0);								// DTS (0) (keep PTS of ODS)
                ToolBox.SetWord(packetHeader, 11, 0);								// size
                for (int i = 0; i < packetHeader.Length; i++)
                    buf[index++] = packetHeader[i];

                // write PCS end
                packetHeader[10] = 0x16;											// ID
                ToolBox.SetDWord(packetHeader, 2, (int)pic.EndTime);				// PTS
                dts = (int)pic.StartTime - 1;
                ToolBox.SetDWord(packetHeader, 6, dts);								// DTS
                ToolBox.SetWord(packetHeader, 11, headerPCSEnd.Length);				// size
                for (int i = 0; i < packetHeader.Length; i++)
                    buf[index++] = packetHeader[i];
                ToolBox.SetWord(headerPCSEnd, 0, pic.Width);
                ToolBox.SetWord(headerPCSEnd, 2, h);                                // cropped height
                ToolBox.SetByte(headerPCSEnd, 4, fpsId);
                ToolBox.SetWord(headerPCSEnd, 5, pic.CompositionNumber + 1);
                for (int i = 0; i < headerPCSEnd.Length; i++)
                    buf[index++] = headerPCSEnd[i];

                // write WDS
                packetHeader[10] = 0x17;											// ID
                timeStamp = (int)pic.EndTime - windowInitTime;
                ToolBox.SetDWord(packetHeader, 2, timeStamp);						// PTS (keep DTS of PCS)
                ToolBox.SetWord(packetHeader, 11, headerWDS.Length);				// size
                for (int i = 0; i < packetHeader.Length; i++)
                    buf[index++] = packetHeader[i];
                ToolBox.SetWord(headerWDS, 2, pic.WindowXOffset);
                ToolBox.SetWord(headerWDS, 4, yOfs);
                ToolBox.SetWord(headerWDS, 6, bm.Width);
                ToolBox.SetWord(headerWDS, 8, bm.Height);
                for (int i = 0; i < headerWDS.Length; i++)
                    buf[index++] = headerWDS[i];

                // write END
                packetHeader[10] = (byte)0x80;										// ID
                ToolBox.SetDWord(packetHeader, 2, dts);								// PTS (DTS of end PCS)
                ToolBox.SetDWord(packetHeader, 6, 0);								// DTS (0)
                ToolBox.SetWord(packetHeader, 11, 0);								// size
                for (int i = 0; i < packetHeader.Length; i++)
                    buf[index++] = packetHeader[i];

            return buf;
        }

    }
}
