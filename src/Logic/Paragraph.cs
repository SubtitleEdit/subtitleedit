using System;

namespace Nikse.SubtitleEdit.Logic
{
    public class Paragraph
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public TimeCode StartTime { get; set; }

        public TimeCode EndTime { get; set; }

        public TimeCode Duration
        {
            get
            {
                var timeCode = new TimeCode(EndTime.TimeSpan);
                timeCode.AddTime(- StartTime.TotalMilliseconds);
                return timeCode;
            }
        }

        public int StartFrame { get; set; }

        public int EndFrame { get; set; }

        public bool Forced { get; set; }

        public string Extra { get; set; }

        public Paragraph()
        {
            StartTime = new TimeCode(TimeSpan.FromSeconds(0));
            EndTime = new TimeCode(TimeSpan.FromSeconds(0));
            Text = string.Empty;
        }

        public Paragraph(TimeCode startTime, TimeCode endTime, string text)
        {
            StartTime = startTime;
            EndTime = endTime;
            Text = text;
        }

        public Paragraph(Paragraph paragraph)
        {
            Number = paragraph.Number;
            Text = paragraph.Text;
            StartTime = new TimeCode(paragraph.StartTime.TimeSpan);
            EndTime = new TimeCode(paragraph.EndTime.TimeSpan);
            StartFrame = paragraph.StartFrame;
            EndFrame = paragraph.EndFrame;
            Forced = paragraph.Forced;
            Extra = paragraph.Extra;
        }

        public Paragraph(int startFrame, int endFrame, string text)
        {
            StartTime = new TimeCode(0, 0, 0, 0);
            EndTime = new TimeCode(0, 0, 0, 0);
            StartFrame = startFrame;
            EndFrame = endFrame;
            Text = text;
        }

        public Paragraph(string text, double startTotalMilliseconds, double endTotalMilliseconds)
        {
            StartTime = new TimeCode(TimeSpan.FromMilliseconds(startTotalMilliseconds));
            EndTime = new TimeCode(TimeSpan.FromMilliseconds(endTotalMilliseconds));
            Text = text;
        }

        internal void Adjust(double factor, double adjust)
        {
            double seconds = StartTime.TimeSpan.TotalSeconds * factor + adjust;
            StartTime.TimeSpan = TimeSpan.FromSeconds(seconds);

            seconds = EndTime.TimeSpan.TotalSeconds * factor + adjust;
            EndTime.TimeSpan = TimeSpan.FromSeconds(seconds);
        }

        public void CalculateFrameNumbersFromTimeCodes(double frameRate)
        {
            StartFrame = (int) Math.Round((StartTime.TotalMilliseconds / 1000.0 * frameRate));
            EndFrame = (int) Math.Round((EndTime.TotalMilliseconds / 1000.0 * frameRate));
        }

        public void CalculateTimeCodesFromFrameNumbers(double frameRate)
        {
           StartTime.TotalMilliseconds = StartFrame * (1000.0 / frameRate);
           EndTime.TotalMilliseconds = EndFrame * (1000.0 / frameRate);
        }

        public override string ToString()
        {
            return StartTime + " --> " + EndTime + " " + Text;
        }

        public int NumberOfLines
        {
            get
            {
                if (string.IsNullOrEmpty(Text))
                    return 0;
                return Text.Length - Text.Replace(Environment.NewLine, string.Empty).Length;
            }
        }
    }
}
