using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.TransportStream
{
    public class DvbSubtitle
    {
        public ulong StartMilliseconds { get; set; }
        public ulong EndMilliseconds { get; set; }
        public DvbSubPes Pes { get; set; }

        /// <summary>
        /// Get complete bitmap
        /// </summary>
        /// <returns></returns>
        public Bitmap GetBitmap()
        {
            return null;
        }

        public List<Bitmap> GetBitmaps()
        {
            return null;
        }

    }
}
