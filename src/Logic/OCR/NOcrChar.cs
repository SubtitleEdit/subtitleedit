using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Nikse.SubtitleEdit.Logic.Ocr
{
    public class NOcrChar
    {
        public string Text { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int MarginTop { get; set; }
        public bool Italic { get; set; }
        public List<NOcrPoint> LinesForeground { get; }
        public List<NOcrPoint> LinesBackground { get; }
        public int ExpandCount { get; set; }
        public bool LoadedOk { get; }

        public double WidthPercent => Height * 100.0 / Width;

        public NOcrChar()
        {
            LinesForeground = new List<NOcrPoint>();
            LinesBackground = new List<NOcrPoint>();
            Text = string.Empty;
        }

        public NOcrChar(NOcrChar old)
        {
            LinesForeground = new List<NOcrPoint>();
            LinesBackground = new List<NOcrPoint>();
            Text = old.Text;
            Width = old.Width;
            Height = old.Height;
            MarginTop = old.MarginTop;
            Italic = old.Italic;
            foreach (NOcrPoint p in old.LinesForeground)
            {
                LinesForeground.Add(new NOcrPoint(new Point(p.Start.X, p.Start.Y), new Point(p.End.X, p.End.Y)));
            }

            foreach (NOcrPoint p in old.LinesBackground)
            {
                LinesBackground.Add(new NOcrPoint(new Point(p.Start.X, p.Start.Y), new Point(p.End.X, p.End.Y)));
            }
        }

        public NOcrChar(string text)
            : this()
        {
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }

        public bool IsSensitive => Text == "." || Text == "," || Text == "'" || Text == "-" || Text == ":" || Text == "\"";

        public NOcrChar(Stream stream)
        {
            try
            {
                var buffer = new byte[9];
                int read = stream.Read(buffer, 0, buffer.Length);
                if (read < buffer.Length)
                {
                    LoadedOk = false;
                    return;
                }

                Width = buffer[0] << 8 | buffer[1];
                Height = buffer[2] << 8 | buffer[3];

                MarginTop = buffer[4] << 8 | buffer[5];

                Italic = buffer[6] != 0;

                ExpandCount = buffer[7];

                int textLen = buffer[8];
                if (textLen > 0)
                {
                    buffer = new byte[textLen];
                    stream.Read(buffer, 0, buffer.Length);
                    Text = System.Text.Encoding.UTF8.GetString(buffer);
                }
                LinesForeground = ReadPoints(stream);
                LinesBackground = ReadPoints(stream);

                LoadedOk = true;
            }
            catch
            {
                LoadedOk = false;
            }
        }

        private static List<NOcrPoint> ReadPoints(Stream stream)
        {
            var list = new List<NOcrPoint>();
            int length = stream.ReadByte() << 8 | stream.ReadByte();
            var buffer = new byte[8];
            for (int i = 0; i < length; i++)
            {
                stream.Read(buffer, 0, buffer.Length);
                var point = new NOcrPoint
                {
                    Start = new Point(buffer[0] << 8 | buffer[1], buffer[2] << 8 | buffer[3]),
                    End = new Point(buffer[4] << 8 | buffer[5], buffer[6] << 8 | buffer[7])
                };
                list.Add(point);
            }
            return list;
        }

        internal void Save(Stream stream)
        {
            WriteInt16(stream, (ushort)Width);
            WriteInt16(stream, (ushort)Height);

            WriteInt16(stream, (ushort)MarginTop);

            stream.WriteByte(Convert.ToByte(Italic));
            stream.WriteByte(Convert.ToByte(ExpandCount));

            if (Text == null)
            {
                stream.WriteByte(0);
            }
            else
            {
                var textBuffer = System.Text.Encoding.UTF8.GetBytes(Text);
                stream.WriteByte((byte)textBuffer.Length);
                stream.Write(textBuffer, 0, textBuffer.Length);
            }
            WritePoints(stream, LinesForeground);
            WritePoints(stream, LinesBackground);
        }

        private static void WritePoints(Stream stream, List<NOcrPoint> points)
        {
            WriteInt16(stream, (ushort)points.Count);
            foreach (var nOcrPoint in points)
            {
                WriteInt16(stream, (ushort)nOcrPoint.Start.X);
                WriteInt16(stream, (ushort)nOcrPoint.Start.Y);
                WriteInt16(stream, (ushort)nOcrPoint.End.X);
                WriteInt16(stream, (ushort)nOcrPoint.End.Y);
            }
        }

        private static void WriteInt16(Stream stream, ushort val)
        {
            var buffer = new byte[2];
            buffer[0] = (byte)((val & 0xFF00) >> 8);
            buffer[1] = (byte)(val & 0x00FF);
            stream.Write(buffer, 0, buffer.Length);
        }

    }
}
