using System;

namespace Nikse.SubtitleEdit.Core
{
    /// <summary>
    /// Fourier transform
    ///
    /// *****************************************************************************
    /// *
    /// * Copyright (c) 2002, Wilhelm Kurz.  All Rights Reserved.
    /// * wkurz@foni.net
    /// *
    /// * This file is provided for demonstration and educational uses only.
    /// * Permission to use, copy, modify and distribute this file for
    /// * any purpose and without fee is hereby granted.
    /// *
    /// *****************************************************************************
    /// Converted/optimized by Nikse from vb code: http://www.wilhelm-kurz-software.de/dynaplot/applicationnotes/spectrogram.htm
    /// </summary>
    internal class Fourier
    {
        public const double W0Hanning = 0.5;
        public const double W0Hamming = 0.54;
        public const double W0Blackman = 0.42;
        private const double Pi = 3.14159265358979;

        private double[] cosarray;
        private double[] sinarray;
        private bool _forward;
        private int _arraySize;
        private int _ldArraysize = 0;

        public Fourier(int arraySize, bool forward)
        {
            _arraySize = arraySize;
            _forward = forward;
            cosarray = new double[arraySize];
            sinarray = new double[arraySize];

            double sign = 1.0;
            if (forward)
                sign = -1.0;

            double phase0 = 2.0 * Pi / arraySize;
            for (int i = 0; i <= arraySize - 1; i++)
            {
                sinarray[i] = sign * Math.Sin(phase0 * i);
                cosarray[i] = Math.Cos(phase0 * i);
            }

            int j = _arraySize;
            while (j != 1)
            {
                _ldArraysize++;
                j /= 2;
            }
        }

        public void MagnitudeSpectrum(double[] real, double[] imag, double w0, double[] magnitude)
        {
            int i;
            magnitude[0] = Math.Sqrt(SquareSum(real[0], imag[0]));
            for (i = 1; i <= (_arraySize / 2 - 1); i++)
                magnitude[i] = (Math.Sqrt(SquareSum(real[i], imag[i]) + SquareSum(real[_arraySize - i], imag[_arraySize - i]))) / w0;
        }

        public static double Hanning(int n, int j)
        {
            return W0Hanning - 0.5 * Math.Cos(2.0 * Pi * j / n);
        }

        public static double Hamming(int n, int j)
        {
            return W0Hamming - 0.46 * Math.Cos(2.0 * Pi * j / n);
        }

        public static double Blackman(int n, int j)
        {
            return W0Blackman - 0.5 * Math.Cos(2.0 * Pi * j / n) + 0.08 * Math.Cos(4.0 * Pi * j / n);
        }

        private static void Swap(ref double a, ref double b)
        {
            double temp = a;
            a = b;
            b = temp;
        }

        private static double SquareSum(double a, double b)
        {
            return a * a + b * b;
        }

        public void FourierTransform(double[] real, double[] imag)
        {
            int i;
            if (_forward)
            {
                for (i = 0; i <= _arraySize - 1; i++)
                {
                    real[i] /= _arraySize;
                    imag[i] /= _arraySize;
                }
            }

            int k;
            int j = 0;
            for (i = 0; i <= _arraySize - 2; i++)
            {
                if (i < j)
                {
                    Swap(ref real[i], ref real[j]);
                    Swap(ref imag[i], ref imag[j]);
                }
                k = _arraySize / 2;
                while (k <= j)
                {
                    j -= k;
                    k /= 2;
                }
                j += k;
            }

            int a = 2;
            int b = 1;
            for (int count = 1; count <= _ldArraysize; count++)
            {
                int c0 = _arraySize / a;
                int c1 = 0;
                for (k = 0; k <= b - 1; k++)
                {
                    i = k;
                    while (i < _arraySize)
                    {
                        int arg = i + b;
                        double prodreal;
                        double prodimag;
                        if (k == 0)
                        {
                            prodreal = real[arg];
                            prodimag = imag[arg];
                        }
                        else
                        {
                            prodreal = real[arg] * cosarray[c1] - imag[arg] * sinarray[c1];
                            prodimag = real[arg] * sinarray[c1] + imag[arg] * cosarray[c1];
                        }
                        real[arg] = real[i] - prodreal;
                        imag[arg] = imag[i] - prodimag;
                        real[i] += prodreal;
                        imag[i] += prodimag;
                        i += a;
                    }
                    c1 += c0;
                }
                a *= 2;
                b *= 2;
            }
        }

    }
}
