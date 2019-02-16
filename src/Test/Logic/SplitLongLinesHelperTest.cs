using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Forms;

namespace Test.Logic
{
    /// <summary>
    /// Summary description for SplitLongLinesHelperTest
    /// </summary>
    [TestClass]
    public class SplitLongLinesHelperTest
    {
        private int _maxLineLength;

        private readonly Subtitle _subtitle;

        public SplitLongLinesHelperTest()
        {
            _maxLineLength = Configuration.Settings.General.SubtitleLineMaximumLength;

            _subtitle = new Subtitle()
            {
                Paragraphs =
                {
                    new Paragraph { Text = "We have never been to Asia, nor have we visited Africa."},
                    new Paragraph { Text = "We have never\r\nbeen to Asia, nor\r\nhave we visited Africa."},
                    new Paragraph { Text = "- Foobar.\r\n- Foobar"},
                    new Paragraph { Text = "- Sometimes, all you need to do is completely make an ass?\r\n- Of yourself and laugh it off to realise that life isn’t so bad after all."},
                    new Paragraph { Text = "Sometimes, all you need to do is completely make an ass\r\nof yourself and laugh it off to realise that life isn’t so bad after all."},
                }
            };

            // build timing
            for (int i = 0; i < _subtitle.Paragraphs.Count; i++)
            {
                var p = _subtitle.Paragraphs[i];
                if (i == 0)
                {
                    p.EndTime.TotalMilliseconds = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                }
                else
                {
                    p.StartTime.TotalMilliseconds = _subtitle.Paragraphs[i - 1].EndTime.TotalMilliseconds +
                        Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                    p.EndTime.TotalMilliseconds = Utilities.GetOptimalDisplayMilliseconds(p.Text);
                }
            }
        }

        [TestMethod]
        public void SplitLongLinesInSubtitleTest()
        {
            var procSubtitle = SplitLongLinesHelper.SplitLongLinesInSubtitle(_subtitle, _maxLineLength * 2, _maxLineLength);

            Assert.AreEqual("We have never been to Asia,\r\nnor have we visited Africa.", procSubtitle.Paragraphs[0].Text);
            Assert.AreEqual("We have never been to Asia,\r\nnor have we visited Africa.", procSubtitle.Paragraphs[1].Text);
            Assert.AreEqual(_subtitle.Paragraphs[2].Text, procSubtitle.Paragraphs[2].Text);

            Assert.AreNotEqual(_subtitle.Paragraphs.Count, procSubtitle.Paragraphs.Count);

            // too long (dialog)
            Assert.AreNotEqual("Sometimes, all you need to do is completely make an ass?", procSubtitle.Paragraphs[3].Text);
            Assert.AreNotEqual("Of yourself and laugh it off to realise that life isn’t so bad after all.", procSubtitle.Paragraphs[4].Text);

            // too long
            Assert.AreNotEqual("Sometimes, all you need to do is completely make an ass?", procSubtitle.Paragraphs[5].Text);
            Assert.AreNotEqual("Of yourself and laugh it off to realise that life isn’t so bad after all.", procSubtitle.Paragraphs[5].Text);
        }
    }
}
