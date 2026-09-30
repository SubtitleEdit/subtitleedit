using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.VobSub
{
    /// <summary>
    /// Reads DVD .sup files made of "SP" packets (see <see cref="SpHeader"/>), e.g. demuxed
    /// with SubRip/Subtitle Processor or written by <see cref="DvdSupWriter"/>.
    /// </summary>
    public static class SpDvdSupParser
    {
        public static List<SpHeader> Parse(string fileName)
        {
            var list = new List<SpHeader>();
            using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var buffer = new byte[SpHeader.SpHeaderLength];
                var bytesRead = fs.Read(buffer, 0, buffer.Length);
                var header = new SpHeader(buffer);

                while (header.Identifier == "SP" && bytesRead > 0 && header.NextBlockPosition > 4)
                {
                    buffer = new byte[header.NextBlockPosition];
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                    if (bytesRead == buffer.Length)
                    {
                        header.AddPicture(buffer);
                        list.Add(header);
                    }

                    buffer = new byte[SpHeader.SpHeaderLength];
                    bytesRead = fs.Read(buffer, 0, buffer.Length);
                    while (bytesRead == buffer.Length && Encoding.ASCII.GetString(buffer, 0, 2) != "SP")
                    {
                        fs.Seek(fs.Position - buffer.Length + 1, SeekOrigin.Begin);
                        bytesRead = fs.Read(buffer, 0, buffer.Length);
                    }

                    header = new SpHeader(buffer);
                }
            }

            return list;
        }
    }
}
