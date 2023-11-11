using System;
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

        [TestMethod]
        public void FixIt()
        {
            var raw = @"1
00:00:00,100 --> 00:00:04,580
Today on This Old House, I'll tour this
modern home to show how beautiful features

2
00:00:04,581 --> 00:00:07,739
and accessible design
go hand in hand. We're

3
00:00:07,751 --> 00:00:10,920
mixing mortar to patch
the original brick on

4
00:00:10,921 --> 00:00:14,529
this 1960 mid-century
modern. And I'll help the

5
00:00:14,541 --> 00:00:18,160
homeowner Billy build
a DIY ramp for his son at

6
00:00:18,161 --> 00:00:35,230
camp. Hey there, I'm Kevin
O'Connor and welcome back

7
00:00:35,242 --> 00:00:51,010
to our project here in
Lexington, Massachusetts.";

            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, raw.SplitToLines(), null);

            var postProcessor = new AudioToTextPostProcessor("en");
            var fixedSubtitle = postProcessor.Fix(AudioToTextPostProcessor.Engine.Whisper, subtitle, true, false, false, false, false, true);

            Assert.AreEqual(7, fixedSubtitle.Paragraphs.Count);
            Assert.IsTrue(fixedSubtitle.Paragraphs[1].Text.EndsWith("hand.", StringComparison.Ordinal));
            Assert.IsTrue(fixedSubtitle.Paragraphs[2].Text.StartsWith("We're mixing", StringComparison.Ordinal));
            Assert.IsTrue(fixedSubtitle.Paragraphs[4].Text.EndsWith("camp.", StringComparison.Ordinal));
            Assert.IsTrue(fixedSubtitle.Paragraphs[5].Text.StartsWith("Hey there", StringComparison.Ordinal));
        }
    }
}
