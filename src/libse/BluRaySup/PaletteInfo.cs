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
