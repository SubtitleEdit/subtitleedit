using System;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Logic.OCR
{
    public class NOcrChar
    {
        public string Text { get; set; }
        public Double Width { get; set; }
        public Double MaxErrorPercent { get; set; }
        public List<NOcrPoint> LinesForeground { get; private set; }
        public List<NOcrPoint> LinesBackground { get; private set; }
        public string Id { get; set; }

        public NOcrChar()
        {
            Id = Guid.NewGuid().ToString();
            LinesForeground = new List<NOcrPoint>();
            LinesBackground = new List<NOcrPoint>();
            Text = string.Empty;
            MaxErrorPercent = 0;
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
