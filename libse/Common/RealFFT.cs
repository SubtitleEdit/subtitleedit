// Copyright(C) 1996-2001 Takuya Ooura
// C# port by J.D. Purcell
// You may use, copy, modify this code for any purpose and without fee.

using System;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class RealFFT
    {
        private readonly int _length;
        private readonly int[] _ip;
        private readonly double[] _w;

        public readonly double ForwardScaleFactor;
        public readonly double ReverseScaleFactor;

        public RealFFT(int length)
        {
            if (length < 2 || (length & (length - 1)) != 0)
            {
                throw new ArgumentException("length", "FFT length must be at least 2 and a power of 2.");
            }

            _length = length;
            _ip = new int[2 + (1 << (Convert.ToInt32(Math.Log(Math.Max(length / 4, 1), 2)) / 2))];
            _w = new double[length / 2];

            ForwardScaleFactor = length;
            ReverseScaleFactor = 0.5;
        }

        public void ComputeForward(double[] buff)
        {
            Compute(buff, false);
        }

        public void ComputeReverse(double[] buff)
        {
            Compute(buff, true);
        }

        private void Compute(double[] buff, bool reverse)
        {
            if (buff.Length < _length)
            {
                throw new ArgumentException("buff", "Buffer length must be greater than or equal to the FFT length.");
            }

            rdft(_length, reverse, buff, _ip, _w);
        }

        private static void rdft(int n, bool rev, double[] a, int[] ip, double[] w)
        {
            int nw, nc;
            double xi;

            nw = ip[0];
            if (n > (nw << 2))
            {
                nw = n >> 2;
                makewt(nw, ip, w);
            }
            nc = ip[1];
            if (n > (nc << 2))
            {
                nc = n >> 2;
                makect(nc, ip, w, nw);
            }
            if (!rev)
            {
                if (n > 4)
                {
                    bitrv2(n, ip, a);
                    cftfsub(n, a, w);
                    rftfsub(n, a, nc, w, nw);
                }
                else if (n == 4)
                {
                    cftfsub(n, a, w);
                }
                xi = a[0] - a[1];
                a[0] += a[1];
                a[1] = xi;
            }
            else
            {
                a[1] = 0.5 * (a[0] - a[1]);
                a[0] -= a[1];
                if (n > 4)
                {
                    rftbsub(n, a, nc, w, nw);
                    bitrv2(n, ip, a);
                    cftbsub(n, a, w);
                }
                else if (n == 4)
                {
                    cftfsub(n, a, w);
                }
            }
        }

        /* -------- initializing routines -------- */

        private static void makewt(int nw, int[] ip, double[] w)
        {
            int j, nwh;
            double delta, x, y;

            ip[0] = nw;
            ip[1] = 1;
            if (nw > 2)
            {
                nwh = nw >> 1;
                delta = Math.Atan(1.0) / nwh;
                w[0] = 1;
                w[1] = 0;
                w[nwh] = Math.Cos(delta * nwh);
                w[nwh + 1] = w[nwh];
                if (nwh > 2)
                {
                    for (j = 2; j < nwh; j += 2)
                    {
                        x = Math.Cos(delta * j);
                        y = Math.Sin(delta * j);
                        w[j] = x;
                        w[j + 1] = y;
                        w[nw - j] = y;
                        w[nw - j + 1] = x;
                    }
                    bitrv2(nw, ip, w);
                }
            }
        }

        private static void makect(int nc, int[] ip, double[] c, int nw)
        {
            int j, nch;
            double delta;

            ip[1] = nc;
            if (nc > 1)
            {
                nch = nc >> 1;
                delta = Math.Atan(1.0) / nch;
                c[nw] = Math.Cos(delta * nch);
                c[nw + nch] = 0.5 * c[nw];
                for (j = 1; j < nch; j++)
                {
                    c[nw + j] = 0.5 * Math.Cos(delta * j);
                    c[nw + nc - j] = 0.5 * Math.Sin(delta * j);
                }
            }
        }

        /* -------- child routines -------- */

        private static void bitrv2(int n, int[] ip, double[] a)
        {
            int j, j1, k, k1, l, m, m2;
            double xr, xi, yr, yi;

            ip[2] = 0;
            l = n;
            m = 1;
            while ((m << 3) < l)
            {
                l >>= 1;
                for (j = 0; j < m; j++)
                {
                    ip[m + j + 2] = ip[j + 2] + l;
                }
                m <<= 1;
            }
            m2 = 2 * m;
            if ((m << 3) == l)
            {
                for (k = 0; k < m; k++)
                {
                    for (j = 0; j < k; j++)
                    {
                        j1 = 2 * j + ip[k + 2];
                        k1 = 2 * k + ip[j + 2];
                        xr = a[j1];
                        xi = a[j1 + 1];
                        yr = a[k1];
                        yi = a[k1 + 1];
                        a[j1] = yr;
                        a[j1 + 1] = yi;
                        a[k1] = xr;
                        a[k1 + 1] = xi;
                        j1 += m2;
                        k1 += 2 * m2;
                        xr = a[j1];
                        xi = a[j1 + 1];
                        yr = a[k1];
                        yi = a[k1 + 1];
                        a[j1] = yr;
                        a[j1 + 1] = yi;
                        a[k1] = xr;
                        a[k1 + 1] = xi;
                        j1 += m2;
                        k1 -= m2;
                        xr = a[j1];
                        xi = a[j1 + 1];
                        yr = a[k1];
                        yi = a[k1 + 1];
                        a[j1] = yr;
                        a[j1 + 1] = yi;
                        a[k1] = xr;
                        a[k1 + 1] = xi;
                        j1 += m2;
                        k1 += 2 * m2;
                        xr = a[j1];
                        xi = a[j1 + 1];
                        yr = a[k1];
                        yi = a[k1 + 1];
                        a[j1] = yr;
                        a[j1 + 1] = yi;
                        a[k1] = xr;
                        a[k1 + 1] = xi;
                    }
                    j1 = 2 * k + m2 + ip[k + 2];
                    k1 = j1 + m2;
                    xr = a[j1];
                    xi = a[j1 + 1];
                    yr = a[k1];
                    yi = a[k1 + 1];
                    a[j1] = yr;
                    a[j1 + 1] = yi;
                    a[k1] = xr;
                    a[k1 + 1] = xi;
                }
            }
            else
            {
                for (k = 1; k < m; k++)
                {
                    for (j = 0; j < k; j++)
                    {
                        j1 = 2 * j + ip[k + 2];
                        k1 = 2 * k + ip[j + 2];
                        xr = a[j1];
                        xi = a[j1 + 1];
                        yr = a[k1];
                        yi = a[k1 + 1];
                        a[j1] = yr;
                        a[j1 + 1] = yi;
                        a[k1] = xr;
                        a[k1 + 1] = xi;
                        j1 += m2;
                        k1 += m2;
                        xr = a[j1];
                        xi = a[j1 + 1];
                        yr = a[k1];
                        yi = a[k1 + 1];
                        a[j1] = yr;
                        a[j1 + 1] = yi;
                        a[k1] = xr;
                        a[k1 + 1] = xi;
                    }
                }
            }
        }

        private static void cftfsub(int n, double[] a, double[] w)
        {
            int j, j1, j2, j3, l;
            double x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i;

            l = 2;
            if (n > 8)
            {
                cft1st(n, a, w);
                l = 8;
                while ((l << 2) < n)
                {
                    cftmdl(n, l, a, w);
                    l <<= 2;
                }
            }
            if ((l << 2) == n)
            {
                for (j = 0; j < l; j += 2)
                {
                    j1 = j + l;
                    j2 = j1 + l;
                    j3 = j2 + l;
                    x0r = a[j] + a[j1];
                    x0i = a[j + 1] + a[j1 + 1];
                    x1r = a[j] - a[j1];
                    x1i = a[j + 1] - a[j1 + 1];
                    x2r = a[j2] + a[j3];
                    x2i = a[j2 + 1] + a[j3 + 1];
                    x3r = a[j2] - a[j3];
                    x3i = a[j2 + 1] - a[j3 + 1];
                    a[j] = x0r + x2r;
                    a[j + 1] = x0i + x2i;
                    a[j2] = x0r - x2r;
                    a[j2 + 1] = x0i - x2i;
                    a[j1] = x1r - x3i;
                    a[j1 + 1] = x1i + x3r;
                    a[j3] = x1r + x3i;
                    a[j3 + 1] = x1i - x3r;
                }
            }
            else
            {
                for (j = 0; j < l; j += 2)
                {
                    j1 = j + l;
                    x0r = a[j] - a[j1];
                    x0i = a[j + 1] - a[j1 + 1];
                    a[j] += a[j1];
                    a[j + 1] += a[j1 + 1];
                    a[j1] = x0r;
                    a[j1 + 1] = x0i;
                }
            }
        }

        private static void cftbsub(int n, double[] a, double[] w)
        {
            int j, j1, j2, j3, l;
            double x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i;

            l = 2;
            if (n > 8)
            {
                cft1st(n, a, w);
                l = 8;
                while ((l << 2) < n)
                {
                    cftmdl(n, l, a, w);
                    l <<= 2;
                }
            }
            if ((l << 2) == n)
            {
                for (j = 0; j < l; j += 2)
                {
                    j1 = j + l;
                    j2 = j1 + l;
                    j3 = j2 + l;
                    x0r = a[j] + a[j1];
                    x0i = -a[j + 1] - a[j1 + 1];
                    x1r = a[j] - a[j1];
                    x1i = -a[j + 1] + a[j1 + 1];
                    x2r = a[j2] + a[j3];
                    x2i = a[j2 + 1] + a[j3 + 1];
                    x3r = a[j2] - a[j3];
                    x3i = a[j2 + 1] - a[j3 + 1];
                    a[j] = x0r + x2r;
                    a[j + 1] = x0i - x2i;
                    a[j2] = x0r - x2r;
                    a[j2 + 1] = x0i + x2i;
                    a[j1] = x1r - x3i;
                    a[j1 + 1] = x1i - x3r;
                    a[j3] = x1r + x3i;
                    a[j3 + 1] = x1i + x3r;
                }
            }
            else
            {
                for (j = 0; j < l; j += 2)
                {
                    j1 = j + l;
                    x0r = a[j] - a[j1];
                    x0i = -a[j + 1] + a[j1 + 1];
                    a[j] += a[j1];
                    a[j + 1] = -a[j + 1] - a[j1 + 1];
                    a[j1] = x0r;
                    a[j1 + 1] = x0i;
                }
            }
        }

        private static void cft1st(int n, double[] a, double[] w)
        {
            int j, k1, k2;
            double wk1r, wk1i, wk2r, wk2i, wk3r, wk3i;
            double x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i;

            x0r = a[0] + a[2];
            x0i = a[1] + a[3];
            x1r = a[0] - a[2];
            x1i = a[1] - a[3];
            x2r = a[4] + a[6];
            x2i = a[5] + a[7];
            x3r = a[4] - a[6];
            x3i = a[5] - a[7];
            a[0] = x0r + x2r;
            a[1] = x0i + x2i;
            a[4] = x0r - x2r;
            a[5] = x0i - x2i;
            a[2] = x1r - x3i;
            a[3] = x1i + x3r;
            a[6] = x1r + x3i;
            a[7] = x1i - x3r;
            wk1r = w[2];
            x0r = a[8] + a[10];
            x0i = a[9] + a[11];
            x1r = a[8] - a[10];
            x1i = a[9] - a[11];
            x2r = a[12] + a[14];
            x2i = a[13] + a[15];
            x3r = a[12] - a[14];
            x3i = a[13] - a[15];
            a[8] = x0r + x2r;
            a[9] = x0i + x2i;
            a[12] = x2i - x0i;
            a[13] = x0r - x2r;
            x0r = x1r - x3i;
            x0i = x1i + x3r;
            a[10] = wk1r * (x0r - x0i);
            a[11] = wk1r * (x0r + x0i);
            x0r = x3i + x1r;
            x0i = x3r - x1i;
            a[14] = wk1r * (x0i - x0r);
            a[15] = wk1r * (x0i + x0r);
            k1 = 0;
            for (j = 16; j < n; j += 16)
            {
                k1 += 2;
                k2 = 2 * k1;
                wk2r = w[k1];
                wk2i = w[k1 + 1];
                wk1r = w[k2];
                wk1i = w[k2 + 1];
                wk3r = wk1r - 2 * wk2i * wk1i;
                wk3i = 2 * wk2i * wk1r - wk1i;
                x0r = a[j] + a[j + 2];
                x0i = a[j + 1] + a[j + 3];
                x1r = a[j] - a[j + 2];
                x1i = a[j + 1] - a[j + 3];
                x2r = a[j + 4] + a[j + 6];
                x2i = a[j + 5] + a[j + 7];
                x3r = a[j + 4] - a[j + 6];
                x3i = a[j + 5] - a[j + 7];
                a[j] = x0r + x2r;
                a[j + 1] = x0i + x2i;
                x0r -= x2r;
                x0i -= x2i;
                a[j + 4] = wk2r * x0r - wk2i * x0i;
                a[j + 5] = wk2r * x0i + wk2i * x0r;
                x0r = x1r - x3i;
                x0i = x1i + x3r;
                a[j + 2] = wk1r * x0r - wk1i * x0i;
                a[j + 3] = wk1r * x0i + wk1i * x0r;
                x0r = x1r + x3i;
                x0i = x1i - x3r;
                a[j + 6] = wk3r * x0r - wk3i * x0i;
                a[j + 7] = wk3r * x0i + wk3i * x0r;
                wk1r = w[k2 + 2];
                wk1i = w[k2 + 3];
                wk3r = wk1r - 2 * wk2r * wk1i;
                wk3i = 2 * wk2r * wk1r - wk1i;
                x0r = a[j + 8] + a[j + 10];
                x0i = a[j + 9] + a[j + 11];
                x1r = a[j + 8] - a[j + 10];
                x1i = a[j + 9] - a[j + 11];
                x2r = a[j + 12] + a[j + 14];
                x2i = a[j + 13] + a[j + 15];
                x3r = a[j + 12] - a[j + 14];
                x3i = a[j + 13] - a[j + 15];
                a[j + 8] = x0r + x2r;
                a[j + 9] = x0i + x2i;
                x0r -= x2r;
                x0i -= x2i;
                a[j + 12] = -wk2i * x0r - wk2r * x0i;
                a[j + 13] = -wk2i * x0i + wk2r * x0r;
                x0r = x1r - x3i;
                x0i = x1i + x3r;
                a[j + 10] = wk1r * x0r - wk1i * x0i;
                a[j + 11] = wk1r * x0i + wk1i * x0r;
                x0r = x1r + x3i;
                x0i = x1i - x3r;
                a[j + 14] = wk3r * x0r - wk3i * x0i;
                a[j + 15] = wk3r * x0i + wk3i * x0r;
            }
        }

        private static void cftmdl(int n, int l, double[] a, double[] w)
        {
            int j, j1, j2, j3, k, k1, k2, m, m2;
            double wk1r, wk1i, wk2r, wk2i, wk3r, wk3i;
            double x0r, x0i, x1r, x1i, x2r, x2i, x3r, x3i;

            m = l << 2;
            for (j = 0; j < l; j += 2)
            {
                j1 = j + l;
                j2 = j1 + l;
                j3 = j2 + l;
                x0r = a[j] + a[j1];
                x0i = a[j + 1] + a[j1 + 1];
                x1r = a[j] - a[j1];
                x1i = a[j + 1] - a[j1 + 1];
                x2r = a[j2] + a[j3];
                x2i = a[j2 + 1] + a[j3 + 1];
                x3r = a[j2] - a[j3];
                x3i = a[j2 + 1] - a[j3 + 1];
                a[j] = x0r + x2r;
                a[j + 1] = x0i + x2i;
                a[j2] = x0r - x2r;
                a[j2 + 1] = x0i - x2i;
                a[j1] = x1r - x3i;
                a[j1 + 1] = x1i + x3r;
                a[j3] = x1r + x3i;
                a[j3 + 1] = x1i - x3r;
            }
            wk1r = w[2];
            for (j = m; j < l + m; j += 2)
            {
                j1 = j + l;
                j2 = j1 + l;
                j3 = j2 + l;
                x0r = a[j] + a[j1];
                x0i = a[j + 1] + a[j1 + 1];
                x1r = a[j] - a[j1];
                x1i = a[j + 1] - a[j1 + 1];
                x2r = a[j2] + a[j3];
                x2i = a[j2 + 1] + a[j3 + 1];
                x3r = a[j2] - a[j3];
                x3i = a[j2 + 1] - a[j3 + 1];
                a[j] = x0r + x2r;
                a[j + 1] = x0i + x2i;
                a[j2] = x2i - x0i;
                a[j2 + 1] = x0r - x2r;
                x0r = x1r - x3i;
                x0i = x1i + x3r;
                a[j1] = wk1r * (x0r - x0i);
                a[j1 + 1] = wk1r * (x0r + x0i);
                x0r = x3i + x1r;
                x0i = x3r - x1i;
                a[j3] = wk1r * (x0i - x0r);
                a[j3 + 1] = wk1r * (x0i + x0r);
            }
            k1 = 0;
            m2 = 2 * m;
            for (k = m2; k < n; k += m2)
            {
                k1 += 2;
                k2 = 2 * k1;
                wk2r = w[k1];
                wk2i = w[k1 + 1];
                wk1r = w[k2];
                wk1i = w[k2 + 1];
                wk3r = wk1r - 2 * wk2i * wk1i;
                wk3i = 2 * wk2i * wk1r - wk1i;
                for (j = k; j < l + k; j += 2)
                {
                    j1 = j + l;
                    j2 = j1 + l;
                    j3 = j2 + l;
                    x0r = a[j] + a[j1];
                    x0i = a[j + 1] + a[j1 + 1];
                    x1r = a[j] - a[j1];
                    x1i = a[j + 1] - a[j1 + 1];
                    x2r = a[j2] + a[j3];
                    x2i = a[j2 + 1] + a[j3 + 1];
                    x3r = a[j2] - a[j3];
                    x3i = a[j2 + 1] - a[j3 + 1];
                    a[j] = x0r + x2r;
                    a[j + 1] = x0i + x2i;
                    x0r -= x2r;
                    x0i -= x2i;
                    a[j2] = wk2r * x0r - wk2i * x0i;
                    a[j2 + 1] = wk2r * x0i + wk2i * x0r;
                    x0r = x1r - x3i;
                    x0i = x1i + x3r;
                    a[j1] = wk1r * x0r - wk1i * x0i;
                    a[j1 + 1] = wk1r * x0i + wk1i * x0r;
                    x0r = x1r + x3i;
                    x0i = x1i - x3r;
                    a[j3] = wk3r * x0r - wk3i * x0i;
                    a[j3 + 1] = wk3r * x0i + wk3i * x0r;
                }
                wk1r = w[k2 + 2];
                wk1i = w[k2 + 3];
                wk3r = wk1r - 2 * wk2r * wk1i;
                wk3i = 2 * wk2r * wk1r - wk1i;
                for (j = k + m; j < l + (k + m); j += 2)
                {
                    j1 = j + l;
                    j2 = j1 + l;
                    j3 = j2 + l;
                    x0r = a[j] + a[j1];
                    x0i = a[j + 1] + a[j1 + 1];
                    x1r = a[j] - a[j1];
                    x1i = a[j + 1] - a[j1 + 1];
                    x2r = a[j2] + a[j3];
                    x2i = a[j2 + 1] + a[j3 + 1];
                    x3r = a[j2] - a[j3];
                    x3i = a[j2 + 1] - a[j3 + 1];
                    a[j] = x0r + x2r;
                    a[j + 1] = x0i + x2i;
                    x0r -= x2r;
                    x0i -= x2i;
                    a[j2] = -wk2i * x0r - wk2r * x0i;
                    a[j2 + 1] = -wk2i * x0i + wk2r * x0r;
                    x0r = x1r - x3i;
                    x0i = x1i + x3r;
                    a[j1] = wk1r * x0r - wk1i * x0i;
                    a[j1 + 1] = wk1r * x0i + wk1i * x0r;
                    x0r = x1r + x3i;
                    x0i = x1i - x3r;
                    a[j3] = wk3r * x0r - wk3i * x0i;
                    a[j3 + 1] = wk3r * x0i + wk3i * x0r;
                }
            }
        }

        private static void rftfsub(int n, double[] a, int nc, double[] c, int nw)
        {
            int j, k, kk, ks, m;
            double wkr, wki, xr, xi, yr, yi;

            m = n >> 1;
            ks = 2 * nc / m;
            kk = 0;
            for (j = 2; j < m; j += 2)
            {
                k = n - j;
                kk += ks;
                wkr = 0.5 - c[nw + nc - kk];
                wki = c[nw + kk];
                xr = a[j] - a[k];
                xi = a[j + 1] + a[k + 1];
                yr = wkr * xr - wki * xi;
                yi = wkr * xi + wki * xr;
                a[j] -= yr;
                a[j + 1] -= yi;
                a[k] += yr;
                a[k + 1] -= yi;
            }
        }

        private static void rftbsub(int n, double[] a, int nc, double[] c, int nw)
        {
            int j, k, kk, ks, m;
            double wkr, wki, xr, xi, yr, yi;

            a[1] = -a[1];
            m = n >> 1;
            ks = 2 * nc / m;
            kk = 0;
            for (j = 2; j < m; j += 2)
            {
                k = n - j;
                kk += ks;
                wkr = 0.5 - c[nw + nc - kk];
                wki = c[nw + kk];
                xr = a[j] - a[k];
                xi = a[j + 1] + a[k + 1];
                yr = wkr * xr + wki * xi;
                yi = wkr * xi - wki * xr;
                a[j] -= yr;
                a[j + 1] = yi - a[j + 1];
                a[k] += yr;
                a[k + 1] = yi - a[k + 1];
            }
            a[m + 1] = -a[m + 1];
        }
    }
}
