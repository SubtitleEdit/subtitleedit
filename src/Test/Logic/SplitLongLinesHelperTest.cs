using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Forms;
using System;

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
                if (i > 0)
                {
                    p.StartTime.TotalMilliseconds = _subtitle.Paragraphs[i - 1].EndTime.TotalMilliseconds +
                        Configuration.Settings.General.MinimumMillisecondsBetweenLines;
                }
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + Utilities.GetOptimalDisplayMilliseconds(p.Text);
            }
        }

        [TestMethod]
        public void SplitLongLinesInSubtitleTest()
        {
            Configuration.Settings.Tools.AutoBreakUsePixelWidth = false;
            var procSubtitle = SplitLongLinesHelper.SplitLongLinesInSubtitle(_subtitle, _maxLineLength * 2, _maxLineLength);

            Assert.AreEqual("We have never been to Asia,\r\nnor have we visited Africa.", procSubtitle.Paragraphs[0].Text);
            Assert.AreEqual("We have never\r\nbeen to Asia, nor\r\nhave we visited Africa.", procSubtitle.Paragraphs[1].Text);
            Assert.AreEqual(_subtitle.Paragraphs[2].Text, procSubtitle.Paragraphs[2].Text);

            Assert.AreNotEqual(_subtitle.Paragraphs.Count, procSubtitle.Paragraphs.Count);

            // too long (dialog)
            Assert.AreEqual(Utilities.AutoBreakLine("Sometimes, all you need to do is completely make an ass?", "en"), procSubtitle.Paragraphs[3].Text);
            Assert.AreEqual(Utilities.AutoBreakLine("Of yourself and laugh it off to realise that life isn’t so bad after all.", "en"), procSubtitle.Paragraphs[4].Text);

            // too long
            Assert.AreEqual("Sometimes, all you need to do is\r\ncompletely make an ass of yourself", procSubtitle.Paragraphs[5].Text);
            Assert.AreEqual("and laugh it off to realise that\r\nlife isn’t so bad after all.", procSubtitle.Paragraphs[6].Text);

            // timing test
            if (procSubtitle.Paragraphs[5].Duration.TotalMilliseconds > procSubtitle.Paragraphs[6].Duration.TotalMilliseconds)
            {
                Assert.IsTrue(procSubtitle.Paragraphs[5].Text.Length > procSubtitle.Paragraphs[6].Text.Length);
            }
            if (procSubtitle.Paragraphs[5].Duration.TotalMilliseconds < procSubtitle.Paragraphs[6].Duration.TotalMilliseconds)
            {
                Assert.IsTrue(procSubtitle.Paragraphs[5].Text.Length < procSubtitle.Paragraphs[6].Text.Length);
            }
        }

        [TestMethod]
        public void SplitLongLinesInSubtitleTest2()
        {
            Configuration.Settings.Tools.AutoBreakUsePixelWidth = false;
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("Hi Joes, how are you feeling after the match yesterday?" + Environment.NewLine + "I know you must be pretty smashed up.", 0, 5000));
            var procSubtitle = SplitLongLinesHelper.SplitLongLinesInSubtitle(sub, _maxLineLength * 2, _maxLineLength);
            Assert.AreEqual("Hi Joes, how are you feeling" + Environment.NewLine + "after the match yesterday?", procSubtitle.Paragraphs[0].Text);
            Assert.AreEqual("I know you must be" + Environment.NewLine + "pretty smashed up.", procSubtitle.Paragraphs[1].Text);
        }

        [TestMethod]
        public void MillisecondsPerCharTest()
        {
            string text = Utilities.AutoBreakLine("The waves were crashing on the\r\nshore; it was a lovely sight.");
            double optimalDuration = Utilities.GetOptimalDisplayMilliseconds(text);
            double displayCharLen = (HtmlUtil.RemoveHtmlTags(text, true).Length - ((Utilities.GetNumberOfLines(text) - 1) * Environment.NewLine.Length));
            double msPerChar = optimalDuration / displayCharLen;

            const double tolerance = .0001;
            double diff = Math.Abs(optimalDuration - (displayCharLen * msPerChar));
            Assert.IsTrue(diff < tolerance);
        }

    }
}
