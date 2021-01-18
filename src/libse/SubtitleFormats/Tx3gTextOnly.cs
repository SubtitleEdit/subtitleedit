using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    public class Tx3GTextOnly : SubtitleFormat
    {
        public override string Extension => ".tx3g";

        public override string Name => "tx3g";

        public override string ToText(Subtitle subtitle, string title)
        {
            throw new NotImplementedException();
        }

        public static int GetUInt(byte[] buffer, int index)
        {
            return (buffer[index] << 24) + (buffer[index + 1] << 16) + (buffer[index + 2] << 8) + buffer[index + 3];
        }

        public override void LoadSubtitle(Subtitle subtitle, List<string> lines, string fileName)
        {
            subtitle.Paragraphs.Clear();
            if (!string.IsNullOrEmpty(fileName) && fileName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase) && File.Exists(fileName))
            {
                var buffer = FileUtil.ReadAllBytesShared(fileName);
                int i = 0;
                while (i  + 4 < buffer.Length)
                {
                    var boxLength = GetUInt(buffer, i);
                    if (boxLength > 500 || i + 4 + boxLength > buffer.Length)
                    {
                        break;
                    }

                    if (boxLength > 10 && buffer[i + 4] == 0x73 && buffer[i + 4 + 1] == 0x74 && buffer[i + 4 + 2] == 0x79 && buffer[i + 4 + 3] == 0x6C && buffer[i + 4 + 4] == 0) // styl + 0
                    {
                        i += boxLength; // "styl" mp4 box
                    }
                    else
                    {
                        var text = Encoding.UTF8.GetString(buffer, i + 4, boxLength);
                        text = string.Join(Environment.NewLine, text.SplitToLines());
                        subtitle.Paragraphs.Add(new Paragraph { Text = text });
                        i += boxLength + 4;
                    }
                }
                subtitle.Renumber();
            }
        }

    }
}
