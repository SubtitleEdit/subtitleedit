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

using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Core.BluRaySup
{
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
}