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
    public static class Core
    {

        /** Use BT.601 color model instead of BT.709 */
        private const bool UseBt601 = false;

        /** Flag that defines whether to swap Cr/Cb components when loading a SUP */
        private const bool SwapCrCb = false;

        /** Alpha threshold for cropping */
        private const int AlphaCrop = 14;

        /** Two equal captions are merged of they are closer than 200ms (0.2*90000 = 18000) */
        private const int MergePtSdiff = 18000;

        /** Frames per seconds for 24p (23.976) */
        public const double Fps24P = 24000.0 / 1001;
        /** Frames per seconds for wrong 24P (23.975) */
        public const double Fps23975 = 23.975;
        /** Frames per seconds for 24Hz (24.0) */
        public const double Fps24Hz = 24.0;
        /** Frames per seconds for PAL progressive (25.0) */
        public const double FpsPal = 25.0;
        /** Frames per seconds for NTSC progressive (29.97) */
        public const double FpsNtsc = 30000.0 / 1001;
        /** Frames per seconds for PAL interlaced (50.0) */
        public const double FpsPalI = 50.0;
        /** Frames per seconds for NTSC interlaced (59.94) */
        public const double FpsNtscI = 60000.0 / 1001;

        /**
         * Get maximum time difference for merging captions.
         * @return Maximum time difference for merging captions
         */
        public static int GetMergePtSdiff()
        {
            return MergePtSdiff;
        }

        /**
        * Get: use of BT.601 color model instead of BT.709.
        * @return True if BT.601 is used
        */
        public static bool UsesBt601()
        {
            return UseBt601;
        }

        /**
         * Get flag that defines whether to swap Cr/Cb components when loading a SUP.
         * @return True: swap cr/cb
         */
        public static bool GetSwapCrCb()
        {
            return SwapCrCb;
        }

        /**
         * Get alpha threshold for cropping.
         * @return Alpha threshold for cropping
         */
        public static int GetAlphaCrop()
        {
            return AlphaCrop;
        }

        public static int CropOfsY { get; set; }

        public const double FpsTrg = FpsPal;
    }
}
