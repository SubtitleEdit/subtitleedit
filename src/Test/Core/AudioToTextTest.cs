using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.AudioToText;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Test.Core
{
    [TestClass]
    public class AudioToTextTest
    {
        [TestMethod]
        public void AudioToTextPostProcessorAutoBalance()
        {
            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, @"1
00:01:04,000 --> 00:01:07,693
It's very important to understand that the
world is not just a place witch accepts our

2
00:01:07,717 --> 00:01:08,940
norms.".SplitToLines(), null);

            var postProcessor = new AudioToTextPostProcessor("en");
            var fixedSubtitle = postProcessor.Fix(AudioToTextPostProcessor.Engine.Whisper, subtitle, true, false, true, false, false, false);
            Assert.AreEqual(2, fixedSubtitle.Paragraphs.Count);
            Assert.IsTrue(fixedSubtitle.Paragraphs[1].Text.Length > 30);
        }

        [TestMethod]
        public void AudioToTextPostProcessorAutoBalance2()
        {
            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, @"1
00:01:04,000 --> 00:01:07,693
It's very important to understand that the
world just accepts our

2
00:01:07,717 --> 00:01:08,940
norms.".SplitToLines(), null);

            var postProcessor = new AudioToTextPostProcessor("en");
            var fixedSubtitle = postProcessor.Fix(AudioToTextPostProcessor.Engine.Whisper, subtitle, true, false, true, false, false, false);
            Assert.AreEqual(1, fixedSubtitle.Paragraphs.Count);
            Assert.IsTrue(fixedSubtitle.Paragraphs[0].Text.CountCharacters(false) == 71);
        }

        [TestMethod]
        public void AudioToTextPostProcessorAutoBalance3()
        {
            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, @"1
00:01:04,000 --> 00:01:07,140
Playing at a different venue almost every night, waking up in a different hotel

2
00:01:07,140 --> 00:01:08,000
room every morning.".SplitToLines(), null);

            var postProcessor = new AudioToTextPostProcessor("en");
            var fixedSubtitle = postProcessor.Fix(AudioToTextPostProcessor.Engine.Whisper, subtitle, true, false, true, false, false, false);
            Assert.AreEqual(2, fixedSubtitle.Paragraphs.Count);
            Assert.IsTrue(fixedSubtitle.Paragraphs[0].Text.EndsWith(","));
        }
    }
}
