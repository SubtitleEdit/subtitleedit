﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Collections.Generic;

namespace Test.Logic.SubtitleFormats
{
    [TestClass]
    public class LrcTest
    {
        [TestMethod]
        public void TimeCodeRoundingLrc()
        {
            const string input = @"1
00:00:59,999 --> 00:01:02,080
I wasn't in love with him.

2
00:01:02,600 --> 00:01:03,850
I know everyone thought IRT was.";
            var srt = new SubRip();
            var lrc = new Lrc();
            var subtitle = new Subtitle();
            srt.LoadSubtitle(subtitle, new List<string>(input.SplitToLines()), null);
            var text = subtitle.ToText(lrc);

            Assert.IsTrue(text.Contains("[01:00.00]I wasn't in love with him."));
        }
    }
}