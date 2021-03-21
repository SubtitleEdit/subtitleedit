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

            var commands = new List<ICommand>
            {
                new DefineWindow(), 
                new SetWindowAttributes(), 
                new SetPenAttributes(), 
                new SetPenColor(),
            };

            foreach (var line in text.SplitToLines())
            {
                commands.Add(new SetPenLocation());
                commands.Add(new SetText(line));
            }

            var bytes = new List<byte>();
            foreach (var command in commands)
            {
                bytes.AddRange(command.GetBytes());
            }

            var smpte291M = new Smpte291M(counter, 20, bytes.ToArray());
            var resultBytes = smpte291M.GetBytes();
            var hex = ByteArrayToHexString(resultBytes);
            return new[] { hex };
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
