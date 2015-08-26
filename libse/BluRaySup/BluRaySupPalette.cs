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

namespace Nikse.SubtitleEdit.Core.BluRaySup
{
    public class BluRaySupPalette
    {
        /** Number of palette entries */
        private int size;
        /** Byte buffer for RED info */
        private byte[] r;
        /** Byte buffer for GREEN info */
        private byte[] g;
        /** Byte buffer for BLUE info */
        private byte[] b;
        /** Byte buffer for alpha info */
        private byte[] a;
        /** Byte buffer for Y (luminance) info */
        private byte[] y;
        /** Byte buffer for Cb (chrominance blue) info */
        private byte[] cb;
        /** Byte buffer for Cr (chrominance red) info */
        private byte[] cr;
        /** Use BT.601 color model instead of BT.709 */
        private bool useBT601;

        /**
         * Convert YCBCr color info to RGB
         * @param y  8 bit luminance
         * @param cb 8 bit chrominance blue
         * @param cr 8 bit chrominance red
         * @return Integer array with red, blue, green component (in this order)
         */
        public static int[] YCbCr2Rgb(int y, int cb, int cr, bool useBt601)
        {
            int[] rgb = new int[3];
            double r, g, b;

            y -= 16;
            cb -= 128;
            cr -= 128;

            var y1 = y * 1.164383562;
            if (useBt601)
            {
                /* BT.601 for YCbCr 16..235 -> RGB 0..255 (PC) */
                r = y1 + cr * 1.596026317;
                g = y1 - cr * 0.8129674985 - cb * 0.3917615979;
                b = y1 + cb * 2.017232218;
            }
            else
            {
                /* BT.709 for YCbCr 16..235 -> RGB 0..255 (PC) */
                r = y1 + cr * 1.792741071;
                g = y1 - cr * 0.5329093286 - cb * 0.2132486143;
                b = y1 + cb * 2.112401786;
            }
            rgb[0] = (int)(r + 0.5);
            rgb[1] = (int)(g + 0.5);
            rgb[2] = (int)(b + 0.5);
            for (int i = 0; i < 3; i++)
            {
                if (rgb[i] < 0)
                    rgb[i] = 0;
                else if (rgb[i] > 255)
                    rgb[i] = 255;
            }
            return rgb;
        }

        /**
         * Convert RGB color info to YCBCr
         * @param r 8 bit red component
         * @param g 8 bit green component
         * @param b 8 bit blue component
         * @return Integer array with luminance (Y), chrominance blue (Cb), chrominance red (Cr) (in this order)
         */
        private static int[] Rgb2YCbCr(int r, int g, int b, bool useBt601)
        {
            int[] yCbCr = new int[3];
            double y, cb, cr;

            if (useBt601)
            {
                /* BT.601 for RGB 0..255 (PC) -> YCbCr 16..235 */
                y = r * 0.299 * 219 / 255 + g * 0.587 * 219 / 255 + b * 0.114 * 219 / 255;
                cb = -r * 0.168736 * 224 / 255 - g * 0.331264 * 224 / 255 + b * 0.5 * 224 / 255;
                cr = r * 0.5 * 224 / 255 - g * 0.418688 * 224 / 255 - b * 0.081312 * 224 / 255;
            }
            else
            {
                /* BT.709 for RGB 0..255 (PC) -> YCbCr 16..235 */
                y = r * 0.2126 * 219 / 255 + g * 0.7152 * 219 / 255 + b * 0.0722 * 219 / 255;
                cb = -r * 0.2126 / 1.8556 * 224 / 255 - g * 0.7152 / 1.8556 * 224 / 255 + b * 0.5 * 224 / 255;
                cr = r * 0.5 * 224 / 255 - g * 0.7152 / 1.5748 * 224 / 255 - b * 0.0722 / 1.5748 * 224 / 255;
            }
            yCbCr[0] = 16 + (int)(y + 0.5);
            yCbCr[1] = 128 + (int)(cb + 0.5);
            yCbCr[2] = 128 + (int)(cr + 0.5);
            for (int i = 0; i < 3; i++)
            {
                if (yCbCr[i] < 16)
                    yCbCr[i] = 16;
                else
                {
                    if (i == 0)
                    {
                        if (yCbCr[i] > 235)
                            yCbCr[i] = 235;
                    }
                    else
                    {
                        if (yCbCr[i] > 240)
                            yCbCr[i] = 240;
                    }
                }
            }
            return yCbCr;
        }

        /**
         * Ctor - initializes palette with transparent black (RGBA: 0x00000000)
         * @param palSize Number of palette entries
         * @param use601  Use BT.601 instead of BT.709
         */
        public BluRaySupPalette(int palSize, bool use601)
        {
            size = palSize;
            useBT601 = use601;
            r = new byte[size];
            g = new byte[size];
            b = new byte[size];
            a = new byte[size];
            y = new byte[size];
            cb = new byte[size];
            cr = new byte[size];

            // set at least all alpha values to invisible
            int[] yCbCr = Rgb2YCbCr(0, 0, 0, useBT601);
            for (int i = 0; i < palSize; i++)
            {
                a[i] = 0;
                r[i] = 0;
                g[i] = 0;
                b[i] = 0;
                y[i] = (byte)yCbCr[0];
                cb[i] = (byte)yCbCr[1];
                cr[i] = (byte)yCbCr[2];
            }
        }

        /**
         * Ctor - initializes palette with transparent black (RGBA: 0x00000000)
         * @param palSize Number of palette entries
         */
        public BluRaySupPalette(int palSize)
            : this(palSize, false)
        {
        }

        /**
         * Ctor - construct palette from red, green blue and alpha buffers
         * @param red    Byte buffer containing the red components
         * @param green  Byte buffer containing the green components
         * @param blue   Byte buffer containing the blue components
         * @param alpha  Byte buffer containing the alpha components
         * @param use601 Use BT.601 instead of BT.709
         */
        public BluRaySupPalette(byte[] red, byte[] green, byte[] blue, byte[] alpha, bool use601)
        {
            size = red.Length;
            useBT601 = use601;
            r = new byte[size];
            g = new byte[size];
            b = new byte[size];
            a = new byte[size];
            y = new byte[size];
            cb = new byte[size];
            cr = new byte[size];

            for (int i = 0; i < size; i++)
            {
                a[i] = alpha[i];
                r[i] = red[i];
                g[i] = green[i];
                b[i] = blue[i];
                var yCbCr = Rgb2YCbCr(r[i] & 0xff, g[i] & 0xff, b[i] & 0xff, useBT601);
                y[i] = (byte)yCbCr[0];
                cb[i] = (byte)yCbCr[1];
                cr[i] = (byte)yCbCr[2];
            }
        }

        /**
         * Ctor - construct palette from red, green blue and alpha buffers
         * @param red   Byte buffer containing the red components
         * @param green Byte buffer containing the green components
         * @param blue  Byte buffer containing the blue components
         * @param alpha Byte buffer containing the alpha components
         */
        public BluRaySupPalette(byte[] red, byte[] green, byte[] blue, byte[] alpha)
            : this(red, green, blue, alpha, false)
        {
        }

        /**
         * Ctor - construct new (independent) palette from existing one
         * @param p Palette to copy values from
         */
        public BluRaySupPalette(BluRaySupPalette p)
        {
            size = p.GetSize();
            useBT601 = p.UsesBt601();
            r = new byte[size];
            g = new byte[size];
            b = new byte[size];
            a = new byte[size];
            y = new byte[size];
            cb = new byte[size];
            cr = new byte[size];

            for (int i = 0; i < size; i++)
            {
                a[i] = p.a[i];
                r[i] = p.r[i];
                g[i] = p.g[i];
                b[i] = p.b[i];
                y[i] = p.y[i];
                cb[i] = p.cb[i];
                cr[i] = p.cr[i];
            }
        }

        /**
         * Set palette index "index" to color "c" in ARGB format
         * @param index Palette index
         * @param c Color in ARGB format
         */
        public void SetArgb(int index, int c)
        {
            int a1 = (c >> 24) & 0xff;
            int r1 = (c >> 16) & 0xff;
            int g1 = (c >> 8) & 0xff;
            int b1 = c & 0xff;
            SetRgb(index, r1, g1, b1);
            SetAlpha(index, a1);
        }

        /**
         * Return palette entry at index as Integer in ARGB format
         * @param index Palette index
         * @return Palette entry at index as Integer in ARGB format
         */
        public int GetArgb(int index)
        {
            return ((a[index] & 0xff) << 24) | ((r[index] & 0xff) << 16) | ((g[index] & 0xff) << 8) | (b[index] & 0xff);
        }

        internal void SetColor(int index, System.Drawing.Color color)
        {
            SetRgb(index, color.R, color.G, color.B);
            SetAlpha(index, color.A);
        }

        /**
         * Set palette entry (RGB mode)
         * @param index Palette index
         * @param red   8bit red component
         * @param green 8bit green component
         * @param blue  8bit blue component
         */
        public void SetRgb(int index, int red, int green, int blue)
        {
            r[index] = (byte)red;
            g[index] = (byte)green;
            b[index] = (byte)blue;
            // create yCbCr
            int[] yCbCr = Rgb2YCbCr(red, green, blue, useBT601);
            y[index] = (byte)yCbCr[0];
            cb[index] = (byte)yCbCr[1];
            cr[index] = (byte)yCbCr[2];
        }

        /**
         * Set palette entry (YCbCr mode)
         * @param index Palette index
         * @param yn    8bit Y component
         * @param cbn   8bit Cb component
         * @param crn   8bit Cr component
         */
        public void SetYCbCr(int index, int yn, int cbn, int crn)
        {
            y[index] = (byte)yn;
            cb[index] = (byte)cbn;
            cr[index] = (byte)crn;
            // create RGB
            int[] rgb = YCbCr2Rgb(yn, cbn, crn, useBT601);
            r[index] = (byte)rgb[0];
            g[index] = (byte)rgb[1];
            b[index] = (byte)rgb[2];
        }

        /**
         * Set alpha channel
         * @param index Palette index
         * @param alpha 8bit alpha channel value
         */
        public void SetAlpha(int index, int alpha)
        {
            a[index] = (byte)alpha;
        }

        /**
         * Get alpha channel
         * @param index Palette index
         * @return 8bit alpha channel value
         */
        public int GetAlpha(int index)
        {
            return a[index] & 0xff;
        }

        /**
         * Return byte array of alpha channel components
         * @return Byte array of alpha channel components (don't modify!)
         */
        public byte[] GetAlpha()
        {
            return a;
        }

        /**
         * Get Integer array containing 8bit red, green, blue components (in this order)
         * @param index Palette index
         * @return Integer array containing 8bit red, green, blue components (in this order)
         */
        public int[] GetRgb(int index)
        {
            int[] rgb = new int[3];
            rgb[0] = r[index] & 0xff;
            rgb[1] = g[index] & 0xff;
            rgb[2] = b[index] & 0xff;
            return rgb;
        }

        /**
         * Get Integer array containing 8bit Y, Cb, Cr components (in this order)
         * @param index Palette index
         * @return Integer array containing 8bit Y, Cb, Cr components (in this order)
         */
        public int[] GetYCbCr(int index)
        {
            int[] yCbCr = new int[3];
            yCbCr[0] = y[index] & 0xff;
            yCbCr[1] = cb[index] & 0xff;
            yCbCr[2] = cr[index] & 0xff;
            return yCbCr;
        }

        /**
         * Return byte array of red components
         * @return Byte array of red components (don't modify!)
         */
        public byte[] GetR()
        {
            return r;
        }

        /**
         * Return byte array of green components
         * @return Byte array of green components (don't modify!)
         */
        public byte[] GetG()
        {
            return g;
        }

        /**
         * Return byte array of blue components
         * @return Byte array of blue components (don't modify!)
         */
        public byte[] GetB()
        {
            return b;
        }

        /**
         * Return byte array of Y components
         * @return Byte array of Y components (don't modify!)
         */
        public byte[] GetY()
        {
            return y;
        }

        /**
         * Return byte array of Cb components
         * @return Byte array of Cb components (don't modify!)
         */
        public byte[] GetCb()
        {
            return cb;
        }

        /**
         * Return byte array of Cr components
         * @return Byte array of Cr components (don't modify!)
         */
        public byte[] GetCr()
        {
            return cr;
        }

        /**
         * Get size of palette (number of entries)
         * @return Size of palette (number of entries)
         */
        public int GetSize()
        {
            return size;
        }

        /**
         * Return index of most transparent palette entry or the index of the first completely transparent color
         * @return Index of most transparent palette entry or the index of the first completely transparent color
         */
        public int GetTransparentIndex()
        {
            // find (most) transparent index in palette
            int transpIdx = 0;
            int minAlpha = 0x100;
            for (int i = 0; i < size; i++)
            {
                if ((a[i] & 0xff) < minAlpha)
                {
                    minAlpha = a[i] & 0xff;
                    transpIdx = i;
                    if (minAlpha == 0)
                        break;
                }
            }
            return transpIdx;
        }

        /**
         * Get: use of BT.601 color model instead of BT.709
         * @return True if BT.601 is used
         */
        public bool UsesBt601()
        {
            return useBT601;
        }

    }
}