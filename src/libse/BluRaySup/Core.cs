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

        /** Frames per seconds for 24p (23.976) */
        public const double Fps24P = 24000.0 / 1001;
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
        * Get: use of BT.601 color model instead of BT.709.
        * @return True if BT.601 is used
        */
        public static bool UsesBt601()
        {
            return UseBt601;
        }

        public static int CropOfsY { get; set; }
    }
}
