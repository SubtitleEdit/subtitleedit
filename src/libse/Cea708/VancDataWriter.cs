using Nikse.SubtitleEdit.Core.Cea708.Commands;
using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Cea708
{
    public static class VancDataWriter
    {
        public static string GenerateTextInit(int counter)
        {
            var commands = new List<ICea708Command>
            {
                new ToggleWindows(true),
                new HideWindows(true),
                new DeleteWindows(true),
            };
            var bytes = new List<byte>();
            foreach (var command in commands)
            {
                bytes.AddRange(command.GetBytes());
            }

            var smpte291M = new Smpte291M(counter, 20, bytes.ToArray());
            var resultBytes = smpte291M.GetBytes();
            var hex = ByteArrayToHexString(resultBytes);
            return hex;
        }

        public static string GenerateEmpty(int counter)
        {
            var bytes = new List<byte>();
            var smpte291M = new Smpte291M(counter, 20, bytes.ToArray());
            //smpte291M.CcDataSectionCcData.CcData[2].Valid = false;
            //smpte291M.CcDataSectionCcData.CcData[2].Type = 2;
            //smpte291M.CcDataSectionCcData.CcData[2].Data1 = 0;
            //smpte291M.CcDataSectionCcData.CcData[2].Data2 = 0;
            //smpte291M.CcDataSectionCcData.CcData[3].Valid = false;
            var resultBytes = smpte291M.GetBytes();
            var hex = ByteArrayToHexString(resultBytes);
            return hex;
        }

        public static string[] GenerateLinesFromText(string input, int counter)
        {
            //TODO: improve italic support
            var text = Utilities.RemoveSsaTags(input);
            text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagFont, HtmlUtil.TagBold);
            var results = new List<string>();
            var bytes = new List<byte>();
            var italic = text.StartsWith("<i>");
            text = HtmlUtil.RemoveOpenCloseTags(text, HtmlUtil.TagItalic);
            var lines = text.SplitToLines();
            var commands = new List<ICea708Command>
            {
                new DefineWindow(lines.Count),
                new SetWindowAttributes(SetWindowAttributes.JustifyCenter),
                new SetPenAttributes(italic),
                new SetPenColor(),
            };
            foreach (var command in commands)
            {
                bytes.AddRange(command.GetBytes());
            }
            commands.Clear();

            var row = 0;
            foreach (var line in lines)
            {
                // Each line goes on its own row - the reader turns a row increase into a line break.
                var c1 = new SetPenLocation { Row = row };
                row++;
                if (c1.GetBytes().Length + bytes.Count > 32)
                {
                    counter = FlushCommands(counter, bytes, results);
                }
                bytes.AddRange(c1.GetBytes());

                // Split the line across packets instead of appending it whole: a cc_data
                // section holds at most 16 byte-pairs, so a single SetText longer than the
                // remaining room made CcDataSection throw "Too many bytes for CCData!" and the
                // whole save failed on any line of ~34 characters or more.
                var remaining = line;
                while (remaining.Length > 0)
                {
                    var room = 32 - bytes.Count;
                    if (room <= 0)
                    {
                        counter = FlushCommands(counter, bytes, results);
                        room = 32 - bytes.Count;
                    }

                    // Count whole characters: a G2 character encodes as EXT1 + code, and
                    // splitting that pair apart would emit a stray escape.
                    var take = 0;
                    var used = 0;
                    while (take < remaining.Length)
                    {
                        var charLength = new SetText(remaining[take].ToString()).GetBytes().Length;
                        if (used + charLength > room)
                        {
                            break;
                        }

                        used += charLength;
                        take++;
                    }

                    if (take == 0)
                    {
                        counter = FlushCommands(counter, bytes, results);
                        continue;
                    }

                    bytes.AddRange(new SetText(remaining.Substring(0, take)).GetBytes());
                    remaining = remaining.Substring(take);
                    if (remaining.Length > 0)
                    {
                        counter = FlushCommands(counter, bytes, results);
                    }
                }
            }

            FlushCommands(counter, bytes, results);
            return results.ToArray();
        }

        private static int FlushCommands(int counter, List<byte> bytes, List<string> results)
        {
            var smpte291M = new Smpte291M(counter, 20, bytes.ToArray());
            counter++;
            var resultBytes = smpte291M.GetBytes();
            var hex = ByteArrayToHexString(resultBytes);
            results.Add(hex);
            bytes.Clear();
            return counter;
        }

        public static string ByteArrayToHexString(byte[] bytes)
        {
            var hex = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                hex.AppendFormat("{0:X2}", b);
            }

            return hex.ToString();
        }
    }
}
