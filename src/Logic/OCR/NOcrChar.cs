using System;
using System.Collections.Generic;
using System.Drawing;

namespace Nikse.SubtitleEdit.Logic.OCR
{
    public class NOcrChar
    {
        public string Text { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int MarginTop { get; set; }
        public bool Italic { get; set; }
        public List<NOcrPoint> LinesForeground { get; private set; }
        public List<NOcrPoint> LinesBackground { get; private set; }
        public string Id { get; set; }
        public int ExpandCount { get; set; }

        public Double WidthPercent
        {
            get
            {
                return Height * 100 / Width;
            }
        }

        public NOcrChar()
        {
            Id = Guid.NewGuid().ToString();
            LinesForeground = new List<NOcrPoint>();
            LinesBackground = new List<NOcrPoint>();
            Text = string.Empty;
        }

        public NOcrChar(NOcrChar old)
        {
            Id = Guid.NewGuid().ToString();
            LinesForeground = new List<NOcrPoint>();
            LinesBackground = new List<NOcrPoint>();
            Text = old.Text;
            Width = old.Width;
            Height = old.Height;
            MarginTop = old.MarginTop;
            Italic = old.Italic;
            foreach (NOcrPoint p in old.LinesForeground)
                LinesForeground.Add(new NOcrPoint(new Point(p.Start.X, p.Start.Y), new Point(p.End.X, p.End.Y)));
            foreach (NOcrPoint p in old.LinesBackground)
                LinesBackground.Add(new NOcrPoint(new Point(p.Start.X, p.Start.Y), new Point(p.End.X, p.End.Y)));
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

        public bool IsSensitive
        {
            get
            {
                return Text == "." || Text == "," || Text == "'" || Text == "-" || Text == "\"";
            }
        }

    }
}
