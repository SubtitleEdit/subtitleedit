using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.OCR
{
    public class NOcrChar
    {
        public string Text { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int MarginTop { get; set; }
        public List<NOcrPoint> LinesForeground { get; private set; }
        public List<NOcrPoint> LinesBackground { get; private set; }
        public string Id { get; set; }

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

        public NOcrChar(string text) : this()
        {
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
