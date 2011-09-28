using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Mvhd
    {

        public Mvhd(FileStream fs, long maximumLength)
        {
            var buffer = new byte[8];
            long pos = fs.Position;            
        }

    }
}
