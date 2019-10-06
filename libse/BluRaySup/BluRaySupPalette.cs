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
        private readonly int _size;
        /** Byte buffer for RED info */
        private readonly byte[] _r;
        /** Byte buffer for GREEN info */
        private readonly byte[] _g;
        /** Byte buffer for BLUE info */
        private readonly byte[] _b;
        /** Byte buffer for alpha info */
        private readonly byte[] _a;
        /** Byte buffer for Y (luminance) info */
        private readonly byte[] _y;
        /** Byte buffer for Cb (chrominance blue) info */
        private readonly byte[] _cb;
        /** Byte buffer for Cr (chrominance red) info */
        private readonly byte[] _cr;
        /** Use BT.601 color model instead of BT.709 */
        private readonly bool _useBt601;

        /**
         * Convert YCBCr color info to RGB
         * @param y  8 bit luminance
         * @param cb 8 bit chrominance blue
         * @param cr 8 bit chrominance red
         * @return Integer array with red, blue, green component (in this order)
         */
        public static int[] YCbCr2Rgb(int y, int cb, int cr, bool useBt601)
        {
            var rgb = new int[3];
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
                {
                    rgb[i] = 0;
                }
                else if (rgb[i] > 255)
                {
                    rgb[i] = 255;
                }
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
            var yCbCr = new int[3];
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
                {
                    yCbCr[i] = 16;
                }
                else
                {
                    if (i == 0)
                    {
                        if (yCbCr[i] > 235)
                        {
                            yCbCr[i] = 235;
                        }
                    }
                    else
                    {
                        if (yCbCr[i] > 240)
                        {
                            yCbCr[i] = 240;
                        }
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
            _size = palSize;
            _useBt601 = use601;
            _r = new byte[_size];
            _g = new byte[_size];
            _b = new byte[_size];
            _a = new byte[_size];
            _y = new byte[_size];
            _cb = new byte[_size];
            _cr = new byte[_size];

            // set at least all alpha values to invisible
            var yCbCr = Rgb2YCbCr(0, 0, 0, _useBt601);
            for (int i = 0; i < palSize; i++)
            {
                _y[i] = (byte)yCbCr[0];
                _cb[i] = (byte)yCbCr[1];
                _cr[i] = (byte)yCbCr[2];
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
            _size = red.Length;
            _useBt601 = use601;
            _r = new byte[_size];
            _g = new byte[_size];
            _b = new byte[_size];
            _a = new byte[_size];
            _y = new byte[_size];
            _cb = new byte[_size];
            _cr = new byte[_size];

            for (int i = 0; i < _size; i++)
            {
                _a[i] = alpha[i];
                _r[i] = red[i];
                _g[i] = green[i];
                _b[i] = blue[i];
                var yCbCr = Rgb2YCbCr(_r[i] & 0xff, _g[i] & 0xff, _b[i] & 0xff, _useBt601);
                _y[i] = (byte)yCbCr[0];
                _cb[i] = (byte)yCbCr[1];
                _cr[i] = (byte)yCbCr[2];
            }
        }

        /**
         * Ctor - construct new (independent) palette from existing one
         * @param p Palette to copy values from
         */
        public BluRaySupPalette(BluRaySupPalette p)
        {
            _size = p.GetSize();
            _useBt601 = p.UsesBt601();
            _r = new byte[_size];
            _g = new byte[_size];
            _b = new byte[_size];
            _a = new byte[_size];
            _y = new byte[_size];
            _cb = new byte[_size];
            _cr = new byte[_size];

            for (int i = 0; i < _size; i++)
            {
                _a[i] = p._a[i];
                _r[i] = p._r[i];
                _g[i] = p._g[i];
                _b[i] = p._b[i];
                _y[i] = p._y[i];
                _cb[i] = p._cb[i];
                _cr[i] = p._cr[i];
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
            return ((_a[index] & 0xff) << 24) | ((_r[index] & 0xff) << 16) | ((_g[index] & 0xff) << 8) | (_b[index] & 0xff);
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
            _r[index] = (byte)red;
            _g[index] = (byte)green;
            _b[index] = (byte)blue;
            // create yCbCr
            var yCbCr = Rgb2YCbCr(red, green, blue, _useBt601);
            _y[index] = (byte)yCbCr[0];
            _cb[index] = (byte)yCbCr[1];
            _cr[index] = (byte)yCbCr[2];
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
            _y[index] = (byte)yn;
            _cb[index] = (byte)cbn;
            _cr[index] = (byte)crn;
            // create RGB
            var rgb = YCbCr2Rgb(yn, cbn, crn, _useBt601);
            _r[index] = (byte)rgb[0];
            _g[index] = (byte)rgb[1];
            _b[index] = (byte)rgb[2];
        }

        /**
         * Set alpha channel
         * @param index Palette index
         * @param alpha 8bit alpha channel value
         */
        public void SetAlpha(int index, int alpha)
        {
            _a[index] = (byte)alpha;
        }

        /**
         * Get alpha channel
         * @param index Palette index
         * @return 8bit alpha channel value
         */
        public int GetAlpha(int index)
        {
            return _a[index] & 0xff;
        }

        /**
         * Return byte array of alpha channel components
         * @return Byte array of alpha channel components (don't modify!)
         */
        public byte[] GetAlpha()
        {
            return _a;
        }

        /**
         * Get Integer array containing 8bit red, green, blue components (in this order)
         * @param index Palette index
         * @return Integer array containing 8bit red, green, blue components (in this order)
         */
        public int[] GetRgb(int index)
        {
            var rgb = new int[3];
            rgb[0] = _r[index] & 0xff;
            rgb[1] = _g[index] & 0xff;
            rgb[2] = _b[index] & 0xff;
            return rgb;
        }

        /**
         * Get Integer array containing 8bit Y, Cb, Cr components (in this order)
         * @param index Palette index
         * @return Integer array containing 8bit Y, Cb, Cr components (in this order)
         */
        public int[] GetYCbCr(int index)
        {
            var yCbCr = new int[3];
            yCbCr[0] = _y[index] & 0xff;
            yCbCr[1] = _cb[index] & 0xff;
            yCbCr[2] = _cr[index] & 0xff;
            return yCbCr;
        }

        /**
         * Return byte array of red components
         * @return Byte array of red components (don't modify!)
         */
        public byte[] GetR()
        {
            return _r;
        }

        /**
         * Return byte array of green components
         * @return Byte array of green components (don't modify!)
         */
        public byte[] GetG()
        {
            return _g;
        }

        /**
         * Return byte array of blue components
         * @return Byte array of blue components (don't modify!)
         */
        public byte[] GetB()
        {
            return _b;
        }

        /**
         * Return byte array of Y components
         * @return Byte array of Y components (don't modify!)
         */
        public byte[] GetY()
        {
            return _y;
        }

        /**
         * Return byte array of Cb components
         * @return Byte array of Cb components (don't modify!)
         */
        public byte[] GetCb()
        {
            return _cb;
        }

        /**
         * Return byte array of Cr components
         * @return Byte array of Cr components (don't modify!)
         */
        public byte[] GetCr()
        {
            return _cr;
        }

        /**
         * Get size of palette (number of entries)
         * @return Size of palette (number of entries)
         */
        public int GetSize()
        {
            return _size;
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
            for (int i = 0; i < _size; i++)
            {
                if ((_a[i] & 0xff) < minAlpha)
                {
                    minAlpha = _a[i] & 0xff;
                    transpIdx = i;
                    if (minAlpha == 0)
                    {
                        break;
                    }
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
            return _useBt601;
        }

    }
}
