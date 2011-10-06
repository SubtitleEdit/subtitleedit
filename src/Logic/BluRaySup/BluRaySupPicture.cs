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
                    if (alpha >= alphaOld)
                    {
                        if (alpha < Core.GetAlphaCrop())
                        {// to not mess with scaling algorithms, make transparent color black
                            y = 16;
                            cr = 128;
                            cb = 128;
                        }
                        palette.SetAlpha(palIndex, alpha);
                    }
                    else fadeOut = true;

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
                return new Bitmap(1,1);

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
            if (color > 0 && x < bmp.Width && y < bmp.Height)
                bmp.SetPixel(x, y, Color.FromArgb(palette.GetArgb(color)));
        }


    }

    public class ImageObject
    {
        /// <summary>
        /// list of ODS packets containing image info
        /// </summary>
        public List<ImageObjectFragment> Fragments;

        /// <summary>
        /// palette identifier
        /// </summary>
        public int PaletteId { get; set; }

        /// <summary>
        /// overall size of RLE buffer (might be spread over several packages)
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        /// with of subtitle image
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// height of subtitle image
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// upper left corner of subtitle x
        /// </summary>
        public int XOffset { get; set; }

        /// <summary>
        /// upper left corner of subtitle y
        /// </summary>
        public int YOffset { get; set; }
    }


    /// <summary>
    /// contains offset and size of one fragment containing (parts of the) RLE buffer
    /// </summary>
    public class ImageObjectFragment
    {
        /// <summary>
        /// size of this part of the RLE buffer
        /// </summary>
        public int ImagePacketSize { get; set; }

        /// <summary>
        /// Buffer for raw image data fragment
        /// </summary>
        public byte[] ImageBuffer { get; set; }
    }


    /// <summary>
    /// contains offset and size of one update of a palette
    /// </summary>
    public class PaletteInfo
    {
        /// <summary>
        /// number of palette entries
        /// </summary>
        public int PaletteSize { get; set; }

        /// <summary>
        /// raw palette data
        /// </summary>
        public byte[] PaletteBuffer { get; set; }

        public PaletteInfo()
        {
        }

        public PaletteInfo(PaletteInfo paletteInfo)
        {
            PaletteSize = paletteInfo.PaletteSize;
            PaletteBuffer = new byte[paletteInfo.PaletteBuffer.Length];
            Buffer.BlockCopy(paletteInfo.PaletteBuffer, 0, PaletteBuffer, 0, PaletteBuffer.Length);
        }
    }


}
