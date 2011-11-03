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
using System.Text;

namespace Nikse.SubtitleEdit.Logic.BluRaySup
{
    public static class ToolBox
    {

        /// <summary>
        /// Convert bytes to a C-style hex string with leading zeroes
        /// </summary>
        public static string ToHex(byte[] buffer, int index, int digits)
        {
            var sb = new StringBuilder();
            for (int i = index; i < index + digits; i++)
            {
                string s = string.Format("{0:X}", buffer[i]);
                if (s.Length < 2)
                    sb.Append("0");
                sb.Append(s);
            }
            return "0x" + sb;
        }

        /// <summary>
        /// Convert a long integer to a C-style hex string with leading zeroes
        /// </summary>
        public static string ToHex(int number, int index, int digits)
        {
            string s = string.Format("{0:X}", number);
            if (s.Length < digits)
                s.PadLeft(digits, '0');
            return "0x" + s;
        }


        /// <summary>
        /// Convert an integer to a string with leading zeroes
        /// </summary>
        /// <param name="i">Integer value to convert</param>
        /// <param name="digits">Number of digits to display - note that a 32bit number can have only 10 digits</param>
        /// <returns>String version of integer with trailing zeroes</returns>
        public static string ZeroTrim(int i, int digits)
        {
            string s = i.ToString();
            return s.PadLeft(digits, '0');
        }

        /**
         * Convert time in milliseconds to array containing hours, minutes, seconds and milliseconds
         * @param ms Time in milliseconds
         * @return Array containing hours, minutes, seconds and milliseconds (in this order)
         */
        public static int[] MillisecondsToTime(long ms)
        {
            int[] time = new int[4];
            // time[0] = hours
            time[0] = (int)(ms / (60 * 60 * 1000));
            ms -= time[0] * 60 * 60 * 1000;
            // time[1] = minutes
            time[1] = (int)(ms / (60 * 1000));
            ms -= time[1] * 60 * 1000;
            // time[2] = seconds
            time[2] = (int)(ms / 1000);
            ms -= time[2] * 1000;
            time[3] = (int)ms;
            return time;
        }

        /// <summary>
        /// Convert time in 90kHz ticks to string hh:mm:ss.ms
        /// </summary>
        /// <param name="pts">Time in 90kHz resolution</param>
        /// <returns>String in format hh:mm:ss:ms</returns>
        public static string PtsToTimeString(long pts)
        {
            int[] time = MillisecondsToTime((pts + 45) / 90);
            return ZeroTrim(time[0], 2) + ":" + ZeroTrim(time[1], 2) + ":" + ZeroTrim(time[2], 2) + "." + ZeroTrim(time[3], 3);
        }

        /// <summary>
        /// Write (big endian) double word to buffer[index] (index points at most significant byte)
        /// </summary>
        /// <param name="buffer">Byte array</param>
        /// <param name="index">Index to write to</param>
        /// <param name="val">Integer value of double word to write</param>
        public static void SetDWord(byte[] buffer, int index, int val)
        {
            buffer[index] = (byte)(val >> 24);
            buffer[index + 1] = (byte)(val >> 16);
            buffer[index + 2] = (byte)(val >> 8);
            buffer[index + 3] = (byte)(val);
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
            buffer[index + 1] = (byte)(val);
        }    

        /// <summary>
        /// Write byte to buffer[index]
        /// </summary>
        /// <param name="buffer">Byte array</param>
        /// <param name="index">Index to write to</param>
        /// <param name="val">Integer value of byte to write</param>
        public static void SetByte(byte[] buffer, int index, int val)
        {
            buffer[index] = (byte)(val);
        }

    }
}