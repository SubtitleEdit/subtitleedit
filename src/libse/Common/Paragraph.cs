using Nikse.SubtitleEdit.Core.Translate.Processor;
using System;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class Paragraph : ITranslationBaseUnit
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public TimeCode StartTime { get; set; }

        public TimeCode EndTime { get; set; }

        public TimeCode Duration => new TimeCode(EndTime.TotalMilliseconds - StartTime.TotalMilliseconds);

        public bool Forced { get; set; }

        public string Extra { get; set; }

        public bool IsComment { get; set; }

        public string Actor { get; set; }
        public string Region { get; set; }

        public string MarginL { get; set; }
        public string MarginR { get; set; }
        public string MarginV { get; set; }

        public string Effect { get; set; }

        public int Layer { get; set; }

        public string Id { get; }

        public string Language { get; set; }

        public string Style { get; set; }

        public bool NewSection { get; set; }

        public string Bookmark { get; set; }

        public bool IsDefault => Math.Abs(StartTime.TotalMilliseconds) < 0.01 && Math.Abs(EndTime.TotalMilliseconds) < 0.01 && string.IsNullOrEmpty(Text);

        private static string GenerateId()
        {
            return Guid.NewGuid().ToString();
        }

        public Paragraph() : this(new TimeCode(), new TimeCode(), string.Empty)
        {
        }

        public Paragraph(TimeCode startTime, TimeCode endTime, string text)
        {
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
            Id = GenerateId();
        }

        public Paragraph(Paragraph paragraph, bool generateNewId = true)
        {
            Number = paragraph.Number;
            Text = paragraph.Text;
            StartTime = new TimeCode(paragraph.StartTime.TotalMilliseconds);
            EndTime = new TimeCode(paragraph.EndTime.TotalMilliseconds);
            Forced = paragraph.Forced;
            Extra = paragraph.Extra;
            IsComment = paragraph.IsComment;
            Actor = paragraph.Actor;
            Region = paragraph.Region;
            MarginL = paragraph.MarginL;
            MarginR = paragraph.MarginR;
            MarginV = paragraph.MarginV;
            Effect = paragraph.Effect;
            Layer = paragraph.Layer;
            Id = generateNewId ? GenerateId() : paragraph.Id;
            Language = paragraph.Language;
            Style = paragraph.Style;
            NewSection = paragraph.NewSection;
            Bookmark = paragraph.Bookmark;
        }

        public Paragraph(string text, double startTotalMilliseconds, double endTotalMilliseconds)
            : this(new TimeCode(startTotalMilliseconds), new TimeCode(endTotalMilliseconds), text)
        {
        }

        public void Adjust(double factor, double adjustmentInSeconds)
        {
            if (StartTime.IsMaxTime)
            {
                return;
            }

            StartTime.TotalMilliseconds = StartTime.TotalMilliseconds * factor + adjustmentInSeconds * TimeCode.BaseUnit;
            EndTime.TotalMilliseconds = EndTime.TotalMilliseconds * factor + adjustmentInSeconds * TimeCode.BaseUnit;
        }

        public override string ToString()
        {
            return $"{StartTime} --> {EndTime} {Text}";
        }

        public int NumberOfLines => Utilities.GetNumberOfLines(Text);

        public double WordsPerMinute
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                {
                    return 0;
                }

                return 60.0 / Duration.TotalSeconds * Text.CountWords();
            }
        }
    }
}
