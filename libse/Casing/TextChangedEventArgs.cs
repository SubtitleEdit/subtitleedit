using System;

namespace Nikse.SubtitleEdit.Core.Casing
{
    public class TextChangedEventArgs : EventArgs
    {
        public TextChangedEventArgs(Paragraph paragraph, CasingResult casingResult)
        {
            Paragraph = paragraph;
            Before = casingResult.Before;
            After = casingResult.After;
        }

        public Paragraph Paragraph { get; }

        public string Before { get; set; }
        public string After { get; set; }
    }
}
