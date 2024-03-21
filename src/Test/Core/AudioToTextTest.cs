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

        [TestMethod]
        public void TryForWholeSentences1()
        {
            var raw = @"12
00:00:25,500 --> 00:00:27,060
Oh, my... Bob, right?

13
00:00:28,560 --> 00:00:29,220
Could be fun.

14
00:00:29,660 --> 00:00:32,580
We could get to know each other a
little, maybe loosen things up around

15
00:00:32,580 --> 00:00:33,060
here?

16
00:00:33,680 --> 00:00:39,160
I've worked with this lot before,
and, erm... Yeah, this is as loose as they

17
00:00:39,160 --> 00:00:40,300
get.

18
00:00:46,120 --> 00:00:46,340
Hmm.

19
00:00:48,160 --> 00:00:49,120
What's the about that, Bob's?

20
00:00:49,120 --> 00:00:49,860
Oh, no.

21
00:00:50,580 --> 00:00:50,700
Yep.

22
00:00:51,240 --> 00:00:52,600
I felt that soon as I said it.

23
00:00:54,860 --> 00:00:56,340
Right, I'm headed out.

24
00:00:57,460 --> 00:00:58,780
Everyone have a great day, yeah?

25
00:00:58,780 --> 00:00:59,600
Yeah.

26
00:01:00,600 --> 00:01:01,880
Wait.

27
00:01:01,880 --> 00:01:02,480
Wait.";

            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, raw.SplitToLines(), null);

            var fixedSubtitle = AudioToTextPostProcessor.TryForWholeSentences(subtitle, "en", 42);

            Assert.AreEqual(14, fixedSubtitle.Paragraphs.Count);
            Assert.AreEqual("We could get to know each other a little, maybe loosen things up around here?", Utilities.UnbreakLine(fixedSubtitle.Paragraphs[2].Text));
            Assert.AreEqual("I've worked with this lot before, and, erm... Yeah, this is as loose as they get.", Utilities.UnbreakLine(fixedSubtitle.Paragraphs[3].Text));
            Assert.AreEqual("Hmm.", fixedSubtitle.Paragraphs[4].Text);
            Assert.AreEqual("What's the about that, Bob's?", fixedSubtitle.Paragraphs[5].Text);
        }

        [TestMethod]
        public void TryForWholeSentences2()
        {
            var raw = @"1
00:00:26,500 --> 00:00:27,060
Yes, I think this could indeed be very good. But also

2
00:00:28,560 --> 00:00:29,220
that could be fun indeed my friend.";

            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, raw.SplitToLines(), null);

            var fixedSubtitle = AudioToTextPostProcessor.TryForWholeSentences(subtitle, "en", 42);

            Assert.AreEqual(2, fixedSubtitle.Paragraphs.Count);
            Assert.AreEqual("Yes, I think this could indeed be very good.", Utilities.UnbreakLine(fixedSubtitle.Paragraphs[0].Text));
            Assert.AreEqual("But also that could be fun indeed my friend.", Utilities.UnbreakLine(fixedSubtitle.Paragraphs[1].Text));
        }

        [TestMethod]
        public void TryForWholeSentences3()
        {
            var raw = @"1
00:04:23,780 --> 00:04:27,340
In each of the commercials that I'm in,
I'm the one who simply can't go on without

2
00:04:27,340 --> 00:04:27,780
the product.";

            var subtitle = new Subtitle();
            new SubRip().LoadSubtitle(subtitle, raw.SplitToLines(), null);

            var fixedSubtitle = AudioToTextPostProcessor.TryForWholeSentences(subtitle, "en", 42);

            Assert.AreEqual(2, fixedSubtitle.Paragraphs.Count);
        }
    }
}
