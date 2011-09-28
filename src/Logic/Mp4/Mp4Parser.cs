using System.Collections.Generic;
using System.IO;
using Nikse.SubtitleEdit.Logic.Mp4.Boxes;
using System;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Mp4
{

    /// <summary>
    /// http://wiki.multimedia.cx/index.php?title=QuickTime_container
    /// </summary>
    public class Mp4Parser
    {

        


        public string FileName { get; private set; }
        public Moov Moov { get; private set; }

        public List<Trak> GetSubtitleTracks()
        {
            var list = new List<Trak>();
            if (Moov != null && Moov.Tracks != null)
            {
                foreach (var trak in Moov.Tracks)
                { 
                    if (trak.Mdia != null && trak.Mdia.HandlerType == "sbtl" && trak.Mdia.Minf != null && trak.Mdia.Minf.Stbl != null)
                    {
                        list.Add(trak);
                    }
                }
            }
            return list;
        }

        public Mp4Parser(string fileName)
        {
            FileName = fileName;
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            ParseMp4(fs);
            fs.Close();
        }

        public Mp4Parser(FileStream fs)
        {
            FileName = null;
            ParseMp4(fs);
        }

        private void ParseMp4(FileStream fs)
        {
            int count = 0;
            var buffer = new byte[8];
            long pos = 0;
            int bytesRead = 8;
            while (bytesRead == 8)
            {
                fs.Seek(pos, SeekOrigin.Begin);
                bytesRead = fs.Read(buffer, 0, 8);
                var size = Helper.GetUInt(buffer, 0);
                string name = Helper.GetString(buffer, 4, 4);
                pos = fs.Position + size - 8;
                if (name == "moov")
                {
                    Moov = new Moov(fs, pos);                 
                }
                count++;
                if (count > 100)
                    break;
            }
            fs.Close();
        }

    }
}
