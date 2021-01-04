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
            LinesForeground = new List<NOcrPoint>(old.LinesForeground.Count);
            LinesBackground = new List<NOcrPoint>(old.LinesBackground.Count);
            Text = old.Text;
            Width = old.Width;
            Height = old.Height;
            MarginTop = old.MarginTop;
            Italic = old.Italic;
            foreach (var p in old.LinesForeground)
            {
                LinesForeground.Add(new NOcrPoint(new Point(p.Start.X, p.Start.Y), new Point(p.End.X, p.End.Y)));
            }

            foreach (var p in old.LinesBackground)
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

        public bool IsSensitive => Text == "O" || Text == "o" || Text == "0" || Text == "'" || Text == "-" || Text == ":" || Text == "\"";

        public NOcrChar(Stream stream, bool isVersion2)
        {
            try
            {
                if (isVersion2)
                {
                    var buffer = new byte[4];
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read < buffer.Length)
                    {
                        LoadedOk = false;
                        return;
                    }

                    var isShort = (buffer[0] & 0b0001_0000) > 0;
                    Italic = (buffer[0] & 0b0010_0000) > 0;

                    if (isShort)
                    {
                        ExpandCount = buffer[0] & 0b0000_1111;
                        Width = buffer[1];
                        Height = buffer[2];
                        MarginTop = buffer[3];
                    }
                    else
                    {
                        ExpandCount = stream.ReadByte();
                        Width = stream.ReadByte() << 8 | stream.ReadByte();
                        Height = stream.ReadByte() << 8 | stream.ReadByte();
                        MarginTop = stream.ReadByte() << 8 | stream.ReadByte();
                    }

                    int textLen = stream.ReadByte();
                    if (textLen > 0)
                    {
                        buffer = new byte[textLen];
                        stream.Read(buffer, 0, buffer.Length);
                        Text = System.Text.Encoding.UTF8.GetString(buffer);
                    }

                    if (isShort)
                    {
                        LinesForeground = ReadPointsBytes(stream);
                        LinesBackground = ReadPointsBytes(stream);
                    }
                    else
                    {
                        LinesForeground = ReadPoints(stream);
                        LinesBackground = ReadPoints(stream);
                    }

                    LoadedOk = true;
                }
                else
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
            }
            catch
            {
                LoadedOk = false;
            }
        }

        private static List<NOcrPoint> ReadPoints(Stream stream)
        {
            int length = stream.ReadByte() << 8 | stream.ReadByte();
            var list = new List<NOcrPoint>(length);
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

        private static List<NOcrPoint> ReadPointsBytes(Stream stream)
        {
            int length = stream.ReadByte();
            var list = new List<NOcrPoint>(length);
            var buffer = new byte[4];
            for (int i = 0; i < length; i++)
            {
                stream.Read(buffer, 0, buffer.Length);
                var point = new NOcrPoint
                {
                    Start = new Point(buffer[0], buffer[1]),
                    End = new Point(buffer[2], buffer[3])
                };
                list.Add(point);
            }
            return list;
        }

        internal void Save(Stream stream)
        {
            if (IsAllByteValues())
            {
                SaveOneBytes(stream);
            }
            else
            {
                SaveTwoBytes(stream);
            }
        }

        private bool IsAllByteValues()
        {
            return Width <= byte.MaxValue && Height <= byte.MaxValue && ExpandCount < 16 &&
                   LinesBackground.Count <= byte.MaxValue && LinesForeground.Count <= byte.MaxValue &&
                   IsAllPointByteValues(LinesForeground) && IsAllPointByteValues(LinesForeground);
        }

        private static bool IsAllPointByteValues(List<NOcrPoint> lines)
        {
            for (var index = 0; index < lines.Count; index++)
            {
                var point = lines[index];
                if (point.Start.X > byte.MaxValue || point.Start.Y > byte.MaxValue ||
                    point.End.X > byte.MaxValue || point.End.Y > byte.MaxValue)
                {
                    return false;
                }
            }

            return true;
        }

        private void SaveOneBytes(Stream stream)
        {
            var flags = 0b0001_0000;

            if (Italic)
            {
                flags |= 0b0010_0000;
            }

            if (ExpandCount > 0)
            {
                flags |= (byte)ExpandCount;
            }

            stream.WriteByte((byte)flags);

            stream.WriteByte((byte)Width);
            stream.WriteByte((byte)Height);
            stream.WriteByte((byte)MarginTop);

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
            WritePointsAsOneByte(stream, LinesForeground);
            WritePointsAsOneByte(stream, LinesBackground);
        }

        private void SaveTwoBytes(Stream stream)
        {
            var flags = 0b0000_0000;

            if (Italic)
            {
                flags |= 0b0010_0000;
            }

            stream.WriteByte((byte)flags);
            stream.WriteByte((byte)ExpandCount);

            WriteInt16(stream, (ushort)Width);
            WriteInt16(stream, (ushort)Height);
            WriteInt16(stream, (ushort)MarginTop);

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

        private static void WritePointsAsOneByte(Stream stream, List<NOcrPoint> points)
        {
            stream.WriteByte((byte)points.Count);
            foreach (var nOcrPoint in points)
            {
                stream.WriteByte((byte)nOcrPoint.Start.X);
                stream.WriteByte((byte)nOcrPoint.Start.Y);
                stream.WriteByte((byte)nOcrPoint.End.X);
                stream.WriteByte((byte)nOcrPoint.End.Y);
            }
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
