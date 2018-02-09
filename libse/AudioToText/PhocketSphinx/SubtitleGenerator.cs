using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nikse.SubtitleEdit.Core.Forms;

namespace Nikse.SubtitleEdit.Core.AudioToText.PhocketSphinx
{
    public class SubtitleGenerator
    {
        private readonly List<ResultText> _resultTexts;

        public SubtitleGenerator(List<ResultText> resultTexts)
        {
            _resultTexts = resultTexts;
        }

        public Subtitle Generate()
        {
            var subtitle = new Subtitle();
            var currentList = new List<ResultText>();
            foreach (var resultText in _resultTexts)
            {
                 subtitle.Paragraphs.Add(new Paragraph(resultText.Text, resultText.Start * 1000.0, resultText.End * 1000.0));  
            }
            //SplitLongLinesHelper.SplitLongLinesInSubtitle()
            return subtitle;
        }
    }
}
