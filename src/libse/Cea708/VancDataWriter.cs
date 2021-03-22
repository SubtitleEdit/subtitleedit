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
            var commands = new List<ICommand>
            {
                new HideWindows(true),
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

        public static string[] GenerateLinesFromText(string text, int counter)
        {
            //TODO: chunk in max 32 bytes chunks (do not split commands)
            var results = new List<string>();
            var bytes = new List<byte>();
            var lines = text.SplitToLines();
            var commands = new List<ICommand>
            {
                new DefineWindow(lines.Count),
                new SetWindowAttributes(SetWindowAttributes.JustifyCenter),
                new SetPenAttributes(false),
                new SetPenColor(),
            };
            foreach (var command in commands)
            {
                bytes.AddRange(command.GetBytes());
            }
            commands.Clear();

            foreach (var line in lines)
            {
                var c1 = new SetPenLocation();
                if (c1.GetBytes().Length + bytes.Count > 32)
                {
                    counter = FlushCommands(counter, bytes, results);
                }
                bytes.AddRange(c1.GetBytes());

                var c2 = new SetText(line);
                if (c2.GetBytes().Length + bytes.Count > 32)
                {
                    counter = FlushCommands(counter, bytes, results);
                }
                bytes.AddRange(c2.GetBytes());
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
