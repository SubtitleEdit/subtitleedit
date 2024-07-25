using System;
using Nikse.SubtitleEdit.Core.Common.TextLengthCalculator;

namespace Nikse.SubtitleEdit.Core.Common
{
    public class Paragraph 
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public TimeCode StartTime { get; set; }

        public TimeCode EndTime { get; set; }

        public TimeCode Duration => new TimeCode(EndTime.TotalMilliseconds - StartTime.TotalMilliseconds);
        public double DurationTotalMilliseconds => EndTime.TotalMilliseconds - StartTime.TotalMilliseconds;
        public double DurationTotalSeconds => (EndTime.TotalMilliseconds - StartTime.TotalMilliseconds) / TimeCode.BaseUnit;

        public bool Forced { get; set; }

        /// <summary>
        /// Extra info (style name for ASSA).
        /// </summary>
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

        /// <summary>
        /// Adjusts the start and end times of a subtitle paragraph.
        /// </summary>
        /// <param name="factor">The factor by which to multiply the start and end times.</param>
        /// <param name="adjustmentInSeconds">The number of seconds to adjust the start and end times by.</param>
        /// <remarks>
        /// This method adjusts the start and end times of a subtitle paragraph by multiplying them with the given factor
        /// and then adding the adjustmentInSeconds value. It does not modify the paragraph if the start time is
        /// TimeCode.MaxTime. The adjustment is performed by updating the TotalMilliseconds property of the paragraph's
        /// StartTime and EndTime objects.
        /// </remarks>
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

                return 60.0 / DurationTotalSeconds * Text.CountWords();
            }
        }

        /// <summary>
        /// Calculates the number of characters per second in a paragraph.
        /// </summary>
        /// <returns>The number of characters per second.</returns>
        /// <remarks>
        /// This method calculates the number of characters per second in a paragraph by dividing the total number of
        /// characters by the duration of the paragraph in seconds. If the duration is less than 1 millisecond, a default
        /// value of 999 is returned.
        /// </remarks>
        public double GetCharactersPerSecond()
        {
            if (DurationTotalMilliseconds < 1)
            {
                return 999;
            }

            return (double)Text.CountCharacters(true) / DurationTotalSeconds;
        }

        /// <summary>
        /// Calculates the number of characters per second in a paragraph.
        /// </summary>
        /// <param name="numberOfCharacters">The total number of characters in the paragraph.</param>
        /// <returns>The number of characters per second.</returns>
        /// <remarks>
        /// This method calculates the number of characters per second in a paragraph by dividing the total number of
        /// characters by the duration of the paragraph in seconds. If the duration is less than 1 millisecond, a default
        /// value of 999 is returned.
        /// </remarks>
        public double GetCharactersPerSecond(double numberOfCharacters)
        {
            if (DurationTotalMilliseconds < 1)
            {
                return 999;
            }

            return numberOfCharacters / DurationTotalSeconds;
        }

        /// <summary>
        /// Calculates the number of characters per second in a paragraph using the default <see cref="ICalcLength"/> implementation.
        /// </summary>
        /// <returns>The number of characters per second.</returns>
        /// <remarks>
        /// This method calculates the number of characters per second in a paragraph by dividing the total number of
        /// characters by the duration of the paragraph in seconds. If the duration is less than 1 millisecond, a default
        /// value of 999 is returned.
        /// </remarks>
        public double GetCharactersPerSecond(ICalcLength calc)
        {
            if (DurationTotalMilliseconds < 1)
            {
                return 999;
            }

            return (double)calc.CountCharacters(Text, true) / DurationTotalSeconds;
        }
    }
}
