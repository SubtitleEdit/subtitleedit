using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Mp4.Boxes
{
    public class Tkhd : Box
    {

        public readonly uint Width;
        public readonly uint Height;

        public Tkhd(FileStream fs, ulong maximumLength)
        {
            buffer = new byte[38];
            int bytesRead = fs.Read(buffer, 0, buffer.Length);
            if (bytesRead < buffer.Length)
                return;

            //System.Windows.Forms.MessageBox.Show(Helper.GetUInt(buffer, 04).ToString());
            //System.Windows.Forms.MessageBox.Show(Helper.GetUInt(buffer, 06).ToString());
            //System.Windows.Forms.MessageBox.Show(Helper.GetUInt(buffer, 08).ToString());
            //System.Windows.Forms.MessageBox.Show(Helper.GetUInt(buffer, 10).ToString());
            //System.Windows.Forms.MessageBox.Show(Helper.GetUInt(buffer, 12).ToString());
            //System.Windows.Forms.MessageBox.Show(Helper.GetUInt(buffer, 14).ToString());
            //System.Windows.Forms.MessageBox.Show(Helper.GetUInt(buffer, 16).ToString());
            //System.Windows.Forms.MessageBox.Show(Helper.GetUInt(buffer, 19).ToString());
            //System.Windows.Forms.MessageBox.Show(Helper.GetUInt(buffer, 20).ToString());
            //System.Windows.Forms.MessageBox.Show(Helper.GetUInt(buffer, 22).ToString());
            //System.Windows.Forms.MessageBox.Show(Helper.GetUInt(buffer, 24).ToString());

        }
    }
}
