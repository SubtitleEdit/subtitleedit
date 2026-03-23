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

namespace Nikse.SubtitleEdit.Core.BluRaySup
{
    public static class ToolBox
    {
        /**
         * Convert time in milliseconds to array containing hours, minutes, seconds and milliseconds
         * @param ms Time in milliseconds
         * @return Array containing hours, minutes, seconds and milliseconds (in this order)
         */
        public static long[] MillisecondsToTime(double ms)
        {
            var time = new long[4];
            // time[0] = hours
            time[0] = (long)Math.Round(ms / (60 * 60 * 1000), MidpointRounding.AwayFromZero);
            ms -= time[0] * 60.0 * 60.0 * 1000.0;
            // time[1] = minutes
            time[1] = (long)Math.Round(ms / (60.0 * 1000.0), MidpointRounding.AwayFromZero);
            ms -= time[1] * 60 * 1000;
            // time[2] = seconds
            time[2] = (long)Math.Round(ms / 1000.0, MidpointRounding.AwayFromZero);
            ms -= time[2] * 1000.0;
            time[3] = (long)Math.Round(ms, MidpointRounding.AwayFromZero);
            return time;
        }

        /// <summary>
        /// Convert time in 90kHz ticks to string hh:mm:ss.ms
        /// </summary>
        /// <param name="pts">Time in 90kHz resolution</param>
        /// <returns>String in format hh:mm:ss:ms</returns>
        public static string PtsToTimeString(long pts)
        {
            var time = MillisecondsToTime(pts / 90.0);
            return $@"{time[0]:D2}:{time[1]:D2}:{time[2]:D2}.{time[3]:D3}";
        }

        /// <summary>
        /// Write (big endian) double word to buffer[index] (index points at most significant byte)
        /// </summary>
        /// <param name="buffer">Byte array</param>
        /// <param name="index">Index to write to</param>
        /// <param name="val">Integer value of double word to write</param>
        public static void SetDWord(byte[] buffer, int index, uint val)
        {
            if (val > 4_294_967_295)
            {
                throw new ArgumentException("val");
            }

            buffer[index] = (byte)(val >> 24);
            buffer[index + 1] = (byte)(val >> 16);
            buffer[index + 2] = (byte)(val >> 8);
            buffer[index + 3] = (byte)val;
        }

        /// <summary>
        /// Write (big endian) word to buffer[index] (index points at most significant byte)
        /// </summary>
        /// <param name="buffer">Byte array</param>
        /// <param name="index">index Index to write to</param>
        /// <param name="val">val Integer value of word to write</param>
        public static void SetWord(byte[] buffer, int index, int val)
        {
            buffer[index] = (byte)(val >> 8);
            buffer[index + 1] = (byte)val;
        }

        /// <summary>
        /// Write byte to buffer[index]
        /// </summary>
        /// <param name="buffer">Byte array</param>
        /// <param name="index">Index to write to</param>
        /// <param name="val">Integer value of byte to write</param>
        public static void SetByte(byte[] buffer, int index, int val)
        {
            buffer[index] = (byte)val;
        }
    }
}
