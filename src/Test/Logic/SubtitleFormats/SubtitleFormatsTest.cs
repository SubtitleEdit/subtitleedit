using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Test.Logic.SubtitleFormats
{
    /// <summary>
    ///This is a test class for subtitle formats and is intended
    ///to contain all subtitle formats Unit Tests
    ///</summary>
    [TestClass]
    public class SubtitleFormatsTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        //
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //

        #endregion Additional test attributes

        #region SubRip (.srt)

        private static List<string> GetSrtLines(string text)
        {
            return new List<string>(text.SplitToLines());
        }

        [TestMethod]
        public void SrtCoordinates()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"1
00:00:02,001 --> 00:00:16,001  X1:000 X2:000 Y1:050 Y2:100
Let us have some! Let us have some!";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "Let us have some! Let us have some!";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SrtNoLineNumbers()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"00:00:03,000 --> 00:00:08,000
Line1.

00:00:08,000 --> 00:00:09,920
Line 2.";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            Assert.AreEqual(2, subtitle.Paragraphs.Count);
        }

        [TestMethod]
        public void SrtDotsInsteadOfCommas()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"2
00:00:04.501 --> 00:00:08.500
Dots instead of commas";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "Dots instead of commas";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SrtTwoLiner()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"2
00:00:04.501 --> 00:00:08.500
Line 1
Line 2";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "Line 1" + Environment.NewLine + "Line 2";
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void SrtThreeLiner()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"2
00:00:04.501 --> 00:00:08.500
Line 1
Line 2
Line 3";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "Line 1" + Environment.NewLine + "Line 2" + Environment.NewLine + "Line 3";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SrtKeepBlankLines()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            string subText = "Now go on!" + Environment.NewLine + Environment.NewLine + "Now go on!";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            var text = target.ToText(subtitle, "title");

            var outSubtitle = new Subtitle();
            target.LoadSubtitle(outSubtitle, text.SplitToLines(), null);
            Assert.IsTrue(outSubtitle.Paragraphs[0].Text == subText);
        }

        [TestMethod]
        public void SrtDifficultLines1()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"303
00:16:22,417 --> 00:16:24,417
sky bots?

304
00:16:24,417 --> 00:16:27,042
how do you think i did


all the stuff
for the show?

305
00:16:27,042 --> 00:16:29,417
you think i went myself?";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            Assert.AreEqual(3, subtitle.Paragraphs.Count);
            const string expected = @"how do you think i did


all the stuff
for the show?";
            Assert.AreEqual(expected, subtitle.Paragraphs[1].Text);
        }

        [TestMethod]
        public void SrtLineNumbers()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"1
00:01:10,040 --> 00:01:13,360 
K-A-R...

2
00:01:16,840 --> 00:01:19,080 
... A-N-T...

3
00:01:21,280 --> 00:01:23,720 
... Æ-N-E.

4
00:01:27,120 --> 00:01:29,680 
Karantæne.

5
00:01:29,840 --> 00:01:32,360 
K-A-R...";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            Assert.AreEqual(5, subtitle.Paragraphs.Count);
            Assert.AreEqual("K-A-R...", subtitle.Paragraphs[0].Text);
        }

        [TestMethod]
        public void SrtSpacesPre()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"1
 00:01:10,040 --> 00:01:13,360 
K-A-R...

 2
00:01:16,840 --> 00:01:19,080 
... A-N-T...

3
 00:01:21,280 --> 00:01:23,720 
... Æ-N-E.

4
00:01:27,120 --> 00:01:29,680 
Karantæne.

5
00:01:29,840 --> 00:01:32,360 
K-A-R...";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            Assert.AreEqual(5, subtitle.Paragraphs.Count);
            Assert.AreEqual("K-A-R...", subtitle.Paragraphs[0].Text);
        }

        [TestMethod]
        public void SrtDifficultLines2()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"1
01:38:18,534 --> 01:38:20,067
 
6530

2
01:39:17,534 --> 01:39:19,400
ppp
";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            Assert.AreEqual(2, subtitle.Paragraphs.Count);
            const string expected = @" 
6530";
            Assert.AreEqual(expected.Trim(), subtitle.Paragraphs[0].Text.Trim());
        }


        [TestMethod]
        public void SrtDifficultLines3()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"1
00:06:25,218 --> 00:06:27,420
<font color='white' face='monospace' size='1c'>
    We have detected a new signal.
   </font>

2
00:06:32,225 --> 00:06:34,526
<font color='white' face='monospace' size='1c'>




    Where is it this time?
   </font>

3
00:06:34,560 --> 00:06:37,096
<font color='white' face='monospace' size='1c'>
    Outside of Federation space.
   </font>";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            Assert.AreEqual(3, subtitle.Paragraphs.Count);
            Assert.AreEqual(@"<font color='white' face='monospace' size='1c'>
    We have detected a new signal.
   </font>", subtitle.Paragraphs[0].Text); Assert.AreEqual(@"<font color='white' face='monospace' size='1c'>




    Where is it this time?
   </font>", subtitle.Paragraphs[1].Text);
        }

        [TestMethod]
        public void SrtBadTimeCode1()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"1
00:00:16.583 --> 00:00:19.833
1941

2
00:00:21.333 --> 00:00:24.083
Half the world is at war...

3
00:00:26.500 --> 00:00:28.875
Germany has taken most of Europe...";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            Assert.AreEqual(3, subtitle.Paragraphs.Count);
            Assert.AreEqual("1941", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("Half the world is at war...", subtitle.Paragraphs[1].Text);
            Assert.AreEqual("Germany has taken most of Europe...", subtitle.Paragraphs[2].Text);
        }

        [TestMethod]
        public void SrtMissingNewLinesAndBadTimeCodes()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"1
00: 00: 12,083  -->  00: 00: 15,726
text1
 2
00: 00: 15,750  -->  00: 00: 28,892
text2
3
00: 00: 28,916  -->  00: 00: 33,726
text3a
text3b";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            Assert.AreEqual(3, subtitle.Paragraphs.Count);
            Assert.AreEqual("text1", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("text2", subtitle.Paragraphs[1].Text);
            Assert.AreEqual("text3a" + Environment.NewLine + "text3b", subtitle.Paragraphs[2].Text);
        }

        [TestMethod]
        public void MissingLineNumberAndEmptyLines()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"00:00:00,000 --> 00:00:10,000
abc

00:00:11,000 --> 00:00:20,000

00:00:21,000 --> 00:00:30,000
def";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            Assert.AreEqual(3, subtitle.Paragraphs.Count);
            Assert.AreEqual("abc", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("", subtitle.Paragraphs[1].Text);
            Assert.AreEqual("def", subtitle.Paragraphs[2].Text);
            Assert.AreEqual(30, subtitle.Paragraphs[2].EndTime.TotalSeconds);
        }

        [TestMethod]
        public void TestBadFileWithMissingNewLineAndBadNumbers()
        {
            var target = new SubRip();
            var subtitle = new Subtitle();
            const string text = @"
00:21:29,998 --> 00:21:32,960
LONDYN 8 maja 1945.

Jeszcze 13 lat temu,

-Kwadrans po 12... Mamo.
67

00:21:58,986 --> 00:22:01,947
GŁÓWNY
012 235 4526
model 5732.
93
00:22:03,991 --> 00:22:05,784
- Za wasz
- Za wasz
115
00:22:18,931 --> 00:22:20,674
- How do you do, Mr. Van Cleve?
- How do you do, Mr. Van Cleve?

124

00:22:21,175 --> 00:22:23,121
10:15 PM.
That's it, you're staying in bed.

312
00:24:21,175 --> 00:24:23,121
REŻYSERIA
611
00:27:21,175 --> 00:27:23,121
ZA ŁATWO, PRAGNIE MŁODOŚCI";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            Assert.AreEqual(7, subtitle.Paragraphs.Count);
            Assert.AreEqual("LONDYN 8 maja 1945." + Environment.NewLine + Environment.NewLine + "Jeszcze 13 lat temu," + Environment.NewLine + Environment.NewLine + "-Kwadrans po 12... Mamo.", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("GŁÓWNY" + Environment.NewLine + "012 235 4526" + Environment.NewLine + "model 5732.", subtitle.Paragraphs[1].Text);
            Assert.AreEqual("- Za wasz" + Environment.NewLine + "- Za wasz", subtitle.Paragraphs[2].Text);
            Assert.AreEqual("- How do you do, Mr. Van Cleve?" + Environment.NewLine + "- How do you do, Mr. Van Cleve?", subtitle.Paragraphs[3].Text);
            Assert.AreEqual("10:15 PM." + Environment.NewLine + "That's it, you're staying in bed.", subtitle.Paragraphs[4].Text);
            Assert.AreEqual("REŻYSERIA", subtitle.Paragraphs[5].Text);
            Assert.AreEqual("ZA ŁATWO, PRAGNIE MŁODOŚCI", subtitle.Paragraphs[6].Text);

            Assert.AreEqual("00:21:29,998", subtitle.Paragraphs[0].StartTime.ToString(false));
            Assert.AreEqual("00:21:32,960", subtitle.Paragraphs[0].EndTime.ToString(false));

            Assert.AreEqual("00:21:58,986", subtitle.Paragraphs[1].StartTime.ToString(false));
            Assert.AreEqual("00:22:01,947", subtitle.Paragraphs[1].EndTime.ToString(false));

            Assert.AreEqual("00:22:03,991", subtitle.Paragraphs[2].StartTime.ToString(false));
            Assert.AreEqual("00:22:05,784", subtitle.Paragraphs[2].EndTime.ToString(false));

            Assert.AreEqual("00:27:21,175", subtitle.Paragraphs[6].StartTime.ToString(false));
            Assert.AreEqual("00:27:23,121", subtitle.Paragraphs[6].EndTime.ToString(false));

            Assert.IsTrue(target.Errors.Contains("Line 8 -"));
            Assert.IsTrue(target.Errors.Contains("Line 14 -"));
            Assert.IsTrue(target.Errors.Contains("Line 18 -"));
            Assert.IsTrue(target.Errors.Contains("Line 32 -"));
        }

        #endregion SubRip (.srt)

        #region Advanced Sub Station alpha (.ass)

        private static List<string> GetAssLines(string lineOneText)
        {
            var text =
@"[Script Info]
Title:
Original Script: swk
Update Details:
ScriptType: v4.00+
Collisions: Normal
PlayDepth: 0
PlayResX: 1920
PlayResY: 1080
Timer: 100,0000

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Default,Arial,73,&H00FFFFFF,&H0000FFFF,&H00000000,&H4B404040,0,0,0,0,100,100,0,0,1,2,0,2,20,20,100,0
Style: Rahmen,Arial,73,&H00FFFFFF,&H00FFFFFF,&H00000000,&H4B404040,0,0,0,0,100,100,0,0,3,20,0,2,20,20,100,0
Style: Rahmen2,Arial,73,&H00FFFFFF,&H00FFFFFF,&H00000000,&H4B404040,0,0,0,0,100,100,0,0,3,20,0,2,20,20,50,0
Style: links,Arial,73,&H00FFFFFF,&H0000FFFF,&H00000000,&H4B404040,0,0,0,0,100,100,0,0,1,2,0,1,200,20,100,0
Style: rechts,Arial,73,&H00FFFFFF,&H0000FFFF,&H00000000,&H4B404040,0,0,0,0,100,100,0,0,1,2,0,3,20,200,100,0

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:16.84,0:00:18.16,rechts,,0000,0000,0000,," + lineOneText;
            return new List<string>(text.SplitToLines());
        }

        [TestMethod]
        public void AssSimpleItalic()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\i1}Italic{\i0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<i>Italic</i>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssSimpleBold()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\b1}Bold{\b0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<b>Bold</b>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssSimpleUnderline()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\u1}Underline{\u0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<u>Underline</u>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssSimpleFontSize()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\fs28}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font size=\"28\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssSimpleFontSizeMultiple()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\fs1}T{\fs2}E{\fs3}S{\fs4}T"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font size=\"1\">T</font><font size=\"2\">E</font><font size=\"3\">S</font><font size=\"4\">T</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssSimpleFontSizeMultipleToText()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<font size=\"1\">T</font><font size=\"2\">E</font><font size=\"3\">S</font><font size=\"4\">T</font>", 0, 2000));
            var result = target.ToText(subtitle, "");
            Assert.IsTrue(result.Contains(@"{\fs1}T{\fs2}E{\fs3}S{\fs4}T"));
        }

        [TestMethod]
        public void AssSimpleFontName()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\fnArial}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font face=\"Arial\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssFontNameWithSpace()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\fnArial Bold}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font face=\"Arial Bold\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssFontNameWithSpaceOutput()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("<font face=\"Arial Bold\">Previously...</font> :)", 0, 2000));
            var text = new AdvancedSubStationAlpha().ToText(s, string.Empty);
            Assert.IsTrue(text.Contains("{\\fnArial Bold}Previously...{\\fn} :)"));
        }

        [TestMethod]
        public void AssSimpleFontNameMultiple()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\fnArial}Font1{\fnTahoma}Font2"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font face=\"Arial\">Font1</font><font face=\"Tahoma\">Font2</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssSimpleFontColor1()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\c&HFF&}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font color=\"#ff0000\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssSimpleFontColor2()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\c&HFF00&}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font color=\"#00ff00\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssSimpleFontColor3()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\c&HFF0000&}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font color=\"#0000ff\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssSimpleFontColor4()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\c&HFFFFFF&}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font color=\"#ffffff\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssSimpleFontColorAndItalic()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\1c&HFFFF00&\i1}CYAN{\i0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font color=\"#00ffff\"><i>CYAN</i></font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssFontNameAndSize()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\fnViner Hand ITC\fs28}Testing"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font face=\"Viner Hand ITC\" size=\"28\">Testing</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssSizeAndOtherTags()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\fs20\pos(1,1)\blur5}Bla-bla-bla"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = @"{\fs20\pos(1,1)\blur5}Bla-bla-bla";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AssFontTagPlusDefault()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("<font color=\"#ff0000\">Previously...</font> :)", 0, 2000));
            var text = new AdvancedSubStationAlpha().ToText(s, string.Empty);
            Assert.IsTrue(text.Contains("{\\c&H0000ff&}Previously...{\\c} :)"));
        }

        [TestMethod]
        public void AssFontEventsLast()
        {
            var text = @"[Script Info]
; test

[Aegisub Project Garbage]
Last Style Storage: Default

[V4+ Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding
Style: Segoe Script Red shadow alpha 160,Segoe Script,77,&H006EBAB4,&H0300FFFF,&H00000000,&HA00000FF,0,0,0,0,100,100,0,0,1,5,5,2,170,170,29,1

[Fonts]
fontname: AGENCYR_0.TTF
!!%

[Events]
Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: 0,0:00:01.80,0:00:04.93,Segoe Script Red shadow alpha 160,,0,0,0,,Die met de zuurstof\Ngezichtsbehandeling? Geweldig!
Dialogue: 0,0:00:05.02,0:00:07.94,Segoe Script Red shadow alpha 160,,0,0,0,,Dit wordt de trip van ons leven.";
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, text.SplitToLines(), null);
            var output = new AdvancedSubStationAlpha().ToText(subtitle, string.Empty);
            Assert.IsTrue(output.Contains("[Events]"));
            Assert.AreEqual(2, subtitle.Paragraphs.Count);
        }


        #endregion Advanced Sub Station alpha (.ass)

        #region Sub Station Alpha (.ssa)

        private static List<string> GetSsaLines(string lineOneText)
        {
            var text =
@"[Script Info]
; This is a Sub Station Alpha v4 script.
Title: TEST
ScriptType: v4.00
Collisions: Normal
PlayDepth: 0

[V4 Styles]
Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
Style: Default,Arial,20,16777215,65535,65535,-2147483640,-1,0,1,3,0,2,30,30,30,0,0

[Events]
Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
Dialogue: Marked=0,0:00:01.00,0:00:03.00,Default,NTP,0000,0000,0000,!Effect," + lineOneText;
            return new List<string>(text.SplitToLines());
        }

        [TestMethod]
        public void SsaSimpleFontColorAndItalic()
        {
            var target = new SubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetSsaLines(@"{\c&HFFFF00&\i1}CYAN{\i0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "<font color=\"#00ffff\"><i>CYAN</i></font>";
            Assert.AreEqual(expected, actual);
        }

        #endregion Sub Station Alpha (.ssa)

        #region DCinema smpte (.xml)

        [TestMethod]
        public void DcinemaSmpteItalic()
        {
            var target = new DCinemaSmpte2010();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<i>Italic</i>", 1000, 5000));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text.Contains("<dcst:Font Italic=\"yes\">Italic</dcst:Font>"));
        }

        [TestMethod]
        public void DcinemaSmpteColorAndItalic()
        {
            var target = new DCinemaSmpte2010();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<font color=\"#ff0000\"><i>Red</i></font>", 1000, 5000));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text.Contains("<dcst:Font Italic=\"yes\" Color=\"FFFF0000\">Red</dcst:Font>"));
        }

        #endregion DCinema smpte (.xml)

        #region DCinema interop (.xml)

        [TestMethod]
        public void DcinemaInteropItalic()
        {
            var target = new DCinemaInterop();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<i>Italic</i>", 1000, 5000));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text.Contains("<Font Italic=\"yes\""));
        }

        [TestMethod]
        public void DcinemaInteropColorAndItalic()
        {
            var target = new DCinemaInterop();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<font color=\"#ff0000\"><i>Red</i></font>", 1000, 5000));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text.Contains(" Italic=\"yes\"") && text.Contains(" Color=\"FFFF0000\""));
        }

        #endregion DCinema interop (.xml)

        #region MicroDVD

        [TestMethod]
        public void MicroDvdItalic()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<i>Italic</i>", 0, 0));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text == "{0}{0}{Y:i}Italic" || text == "{0}{0}{y:i}Italic");
        }

        [TestMethod]
        public void MicroDvdBold()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<b>Bold</b>", 0, 0));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text == "{0}{0}{Y:b}Bold" || text == "{0}{0}{y:b}Bold");
        }

        [TestMethod]
        public void MicroDvdUnderline()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<u>Underline</u>", 0, 0));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text == "{0}{0}{Y:u}Underline" || text == "{0}{0}{y:u}Underline");
        }

        [TestMethod]
        public void MicroDvdUnderlineItalic()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<u><i>Underline Italic</i></u>", 0, 0));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text == "{0}{0}{Y:u}{Y:i}Underline Italic" || text == "{0}{0}{y:u}{y:i}Underline Italic");
        }

        [TestMethod]
        public void MicroDvdItalicUnderline()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<i><u>Underline Italic</u></i>", 0, 0));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text == "{0}{0}{Y:i}{Y:u}Underline Italic" || text == "{0}{0}{y:i}{y:u}Underline Italic");
        }

        [TestMethod]
        public void MicroDvdReadBoldItalic()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            var list = new List<string> { "{0}{0}{y:i,b}Hello!" };
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<i><b>Hello!</b></i>");
        }

        [TestMethod]
        public void MicroDvdReadFont()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            var list = new List<string> { "{0}{0}{C:$FF0000}Blue" };
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<font color=\"#0000FF\">Blue</font>" || text == "<font color=\"blue\">Blue</font>");
        }

        [TestMethod]
        public void MicroDvdReadAdvanced()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            var list = new List<string> { "{0}{25}{c:$0000ff}{y:b,u}{f:DeJaVuSans}{s:12}Hello!" };
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<font color=\"#ff0000\"><b><u><font face=\"DeJaVuSans\"><font size=\"12\">Hello!</font></font></u></b></font>");
        }

        [TestMethod]
        public void MicroDvdReadBoldFirstLineOnly()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            var list = new List<string> { "{0}{0}{y:i}Hello!|Hello!" };
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<i>Hello!</i>" + Environment.NewLine + "Hello!");
        }

        [TestMethod]
        public void MicroDvdReadBoldSecondLineOnly()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            var list = new List<string> { "{0}{0}Hello!|{y:i}Hello!" };
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "Hello!" + Environment.NewLine + "<i>Hello!</i>");
        }

        [TestMethod]
        public void MicroDvdReadItalicBothLines()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            var list = new List<string> { "{0}{0}{Y:i}Hello!|Hello!" };
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<i>Hello!" + Environment.NewLine + "Hello!</i>" ||
                          text == "<i>Hello!</i>" + Environment.NewLine + "<i>Hello!</i>");
        }

        [TestMethod]
        public void MicroDvdReadBoldBothLinesItalicFirst()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            var list = new List<string> { "{0}{0}{Y:b}{y:i}Hello!|Hello!" };
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<b><i>Hello!</i>" + Environment.NewLine + "Hello!</b>" ||
                          text == "<b><i>Hello!</i></b>" + Environment.NewLine + "<b>Hello!</b>");
        }

        #endregion MicroDVD

        #region Sami (.smi)

        [TestMethod]
        public void SamiKeepBlankLines()
        {
            var target = new Sami();
            var subtitle = new Subtitle();
            string subText = "Now go on!" + Environment.NewLine + Environment.NewLine + "Now go on!";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            var text = target.ToText(subtitle, "title");

            var outSubtitle = new Subtitle();
            target.LoadSubtitle(outSubtitle, text.SplitToLines(), null);
            Assert.IsTrue(outSubtitle.Paragraphs[0].Text == subText);
        }

        [TestMethod]
        public void SamiItalic()
        {
            var target = new Sami();
            var subtitle = new Subtitle();
            const string subText = "<i>Now go on!<i>";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            subtitle.Paragraphs.Add(new Paragraph(subText, 1000, 1999));
            var text = target.ToText(subtitle, "title");

            var outSubtitle = new Subtitle();
            target.LoadSubtitle(outSubtitle, text.SplitToLines(), null);
            Assert.IsTrue(outSubtitle.Paragraphs[0].Text == subText);
            Assert.IsTrue(outSubtitle.Paragraphs[1].Text == subText);
        }

        [TestMethod]
        public void SamiFont()
        {
            var target = new Sami();
            var subtitle = new Subtitle();
            const string subText = "<font color=\"#00ff00\">We have secured the first building!</font>";
            subtitle.Paragraphs.Add(new Paragraph(subText, 0, 999));
            subtitle.Paragraphs.Add(new Paragraph(subText, 1000, 1999));
            var text = target.ToText(subtitle, "title");

            var outSubtitle = new Subtitle();
            target.LoadSubtitle(outSubtitle, text.SplitToLines(), null);
            Assert.IsTrue(outSubtitle.Paragraphs[0].Text == subText);
            Assert.IsTrue(outSubtitle.Paragraphs[1].Text == subText);
        }

        #endregion Sami (.smi)

        #region Scenerist SCC

        [TestMethod]
        public void CheckTimeCodes()
        {
            var target = new ScenaristClosedCaptions();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Line1", 1000, 5000));
            subtitle.Paragraphs.Add(new Paragraph("Line2", 6000, 8000));
            subtitle.Paragraphs.Add(new Paragraph("Line2", 8000, 12000));

            const string text = @"Scenarist_SCC V1.0

01:00:00:24 9420 94f8 5bc1 4c45 5254 20d3 4fd5 cec4 49ce c75d 9420 942c 942f 9420 94f2 4f43 544f cec1 d554 d320 544f 20d9 4fd5 5220 d354 c154 494f ced3 a180

01:00:02:22 9420 942c 942f

01:00:04:25 9420 94f4 c2c1 52ce c143 4c45 d3a1

01:00:05:10 942c

01:00:06:14 9420 942c 942f

01:00:07:06 9420 9476 cb57 c1da 4949 a180

01:00:08:24 9420 942c 942f 9420 9476 d045 d34f a180

01:00:10:03 9420 942c 942f

01:00:11:29 942c

01:00:37:12 9420 9470 4558 d04c 4f52 45a1

01:00:39:00 9420 942c 942f 9420 9476 5245 d343 d545 a180

01:00:40:00 9420 942c 942f 9420 947a d052 4f54 4543 54a1

01:00:41:00 9420 942c 942f

01:00:41:10 9420 9154 4f43 544f cec1 d554 d3a1

01:00:42:28 9420 942c 942f

01:00:44:26 942c

01:00:47:03 9420 9152 a254 c845 204f 4354 4fce c1d5 54d3 20c1 cec4 91f2 54c8 4520 c749 c1ce 5420 cb45 4cd0 2046 4f52 45d3 54ae a280

01:00:49:00 9420 942c 942f

01:00:52:04 942c";

            var sub2 = new Subtitle();

            var lines = new List<string>();
            foreach (string line in text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                lines.Add(line);

            target.LoadSubtitle(sub2, lines, null);

            var copy = new Subtitle(sub2);
            for (int i = 0; i < copy.Paragraphs.Count; i++)
            {
                sub2.Paragraphs[i].StartTime.TotalMilliseconds += 1000;
                sub2.Paragraphs[i].EndTime.TotalMilliseconds += 1000;
            }
            for (int i = 0; i < copy.Paragraphs.Count; i++)
            {
                Assert.IsTrue(Math.Abs(copy.Paragraphs[i].StartTime.TotalMilliseconds + 1000 - sub2.Paragraphs[i].StartTime.TotalMilliseconds) < 0.001);
                Assert.IsTrue(Math.Abs(copy.Paragraphs[i].EndTime.TotalMilliseconds + 1000 - sub2.Paragraphs[i].EndTime.TotalMilliseconds) < 0.001);
            }
        }

        #endregion Scenerist SCC

        #region All subtitle formats

        [TestMethod]
        public void LineCount()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Line 1", 0, 3000));
            subtitle.Paragraphs.Add(new Paragraph("Line 2", 4000, 7000));
            subtitle.Paragraphs.Add(new Paragraph("Line 3", 8000, 11000));
            subtitle.Paragraphs.Add(new Paragraph("Line 4", 12000, 15000));

            int expected = subtitle.Paragraphs.Count;
            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
            {
                if (format.GetType() != typeof(JsonType6) && format.IsTextBased)
                {
                    format.BatchMode = true;
                    string text = format.ToText(subtitle, "test");
                    var list = new List<string>();
                    foreach (string line in text.Replace("\r\n", "\n").Split('\n'))
                        list.Add(line);
                    var s2 = new Subtitle();
                    format.LoadSubtitle(s2, list, null);
                    int actual = s2.Paragraphs.Count;
                    Assert.AreEqual(expected, actual, format.FriendlyName);
                }
            }
        }

        [TestMethod]
        public void LineContent()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Line 1", 0, 3000));
            subtitle.Paragraphs.Add(new Paragraph("Line 2", 4000, 7000));
            subtitle.Paragraphs.Add(new Paragraph("Line 3", 8000, 11000));
            subtitle.Paragraphs.Add(new Paragraph("Line 4", 12000, 15000));

            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
            {
                if (format.IsTextBased)
                {
                    format.BatchMode = true;
                    string text = format.ToText(subtitle, "test");
                    var list = new List<string>();
                    foreach (string line in text.Replace("\r\n", "\n").Split('\n'))
                        list.Add(line);
                    var s2 = new Subtitle();
                    format.LoadSubtitle(s2, list, null);

                    if (s2.Paragraphs.Count == 4)
                    {
                        Assert.AreEqual(subtitle.Paragraphs[0].Text, s2.Paragraphs[0].Text, format.FriendlyName);
                        Assert.AreEqual(subtitle.Paragraphs[3].Text, s2.Paragraphs[3].Text, format.FriendlyName);
                    }
                }
            }
        }

        //         [TestMethod()]
        //         public void FormatReload()
        //         {
        //             var subtitle = new Subtitle();
        //             subtitle.Paragraphs.Add(new Paragraph("Line 1", 0, 3000));
        //             subtitle.Paragraphs.Add(new Paragraph("Line 2", 4000, 7000));
        //             subtitle.Paragraphs.Add(new Paragraph("Line 3", 8000, 11000));
        //             subtitle.Paragraphs.Add(new Paragraph("Line 4", 12000, 15000));

        //             StringBuilder sb = new StringBuilder();
        //             foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
        //             {
        //                 string text = format.ToText(subtitle, "test");
        //                 var list = new List<string>();
        //                 foreach (string line in text.Replace("\r\n", "\n").Split('\n'))
        //                     list.Add(line);

        //                 foreach (SubtitleFormat innerFormat in SubtitleFormat.AllSubtitleFormats)
        //                 {
        //                     if (innerFormat.IsMine(list, null))
        //                     {
        //                         if (format.FriendlyName != innerFormat.FriendlyName &&
        //                             !format.FriendlyName.Contains("Final Cut"))
        //                         {
        ////                             Assert.AreEqual(format.FriendlyName, innerFormat.FriendlyName, text);
        //                             sb.AppendLine(innerFormat.FriendlyName + " takes " + format.FriendlyName);
        //                         }
        //                         break;
        //                     }
        //                 }
        //             }
        ////             System.Windows.Forms.MessageBox.Show(sb.ToString());
        //         }

        #endregion All subtitle formats

        #region ToUtf8XmlString

        [TestMethod]
        public void ToUtf8XmlStringDefault()
        {
            var doc = new XmlDocument();
            doc.LoadXml("<root><tag>test</tag></root>");
            string xml = SubtitleFormat.ToUtf8XmlString(doc);
            Assert.IsTrue(xml.Contains("utf-8"));
        }

        [TestMethod]
        public void ToUtf8XmlStringNoHeader()
        {
            var doc = new XmlDocument();
            doc.LoadXml("<root><tag>test</tag></root>");
            string xml = SubtitleFormat.ToUtf8XmlString(doc, true);
            Assert.IsTrue(!xml.Contains("utf-8"));
        }

        [TestMethod]
        public void ToUtf8XmlStringStillXml()
        {
            var doc = new XmlDocument();
            doc.LoadXml("<root><tag>ø</tag></root>");
            string xml = SubtitleFormat.ToUtf8XmlString(doc, true);

            doc.LoadXml(xml);
            Assert.IsTrue(doc.InnerXml.Contains("ø"));
        }

        #endregion ToUtf8XmlString

        #region UTX
        [TestMethod]
        public void LoadTestStartingWithCardinal()
        {
            string s = "#Every satellite...#\r\n#0:02:06.14,0:02:08.08";
            var target = new Utx();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, s.SplitToLines(), string.Empty);
            string actual = subtitle.Paragraphs[0].Text;
            Assert.AreEqual("#Every satellite...#", actual);
        }
        #endregion

        #region Unknown format
        [TestMethod]
        public void TestUnknownFormatAndFrameRate()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                sb.AppendLine(i.ToString(CultureInfo.InvariantCulture));
                sb.AppendLine(Utilities.LowercaseLetters);
                sb.AppendLine();
            }
            var lines = sb.ToString().SplitToLines();
            Configuration.Settings.General.CurrentFrameRate = 27;
            foreach (var format in SubtitleFormat.AllSubtitleFormats)
            {
                if (format.IsMine(lines, null))
                {
                    Assert.Fail("This format should not be recognized: " + format.GetType());
                }
                if (Math.Abs(Configuration.Settings.General.CurrentFrameRate - 27) > 0.01)
                {
                    Assert.Fail("Frame rate changed in 'IsMine': " + format.GetType());
                }
            }
        }
        #endregion

        #region Phoenix Subtitle

        [TestMethod]
        public void PhoenixSubtitleTest()
        {
            var phxSub = new PhoenixSubtitle();
            var subtitle = new Subtitle();
            const string text = @"2447,   2513, ""You should come to the Drama Club, too.""
2513,   2594, ""Yeah. The Drama Club is worried|that you haven't been coming.""
2603,   2675, """"I see. Sorry, ""I'll drop"" by next time.""""";

            // Test text.
            phxSub.LoadSubtitle(subtitle, new List<string>(text.SplitToLines()), null);
            Assert.AreEqual("You should come to the Drama Club, too.", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("Yeah. The Drama Club is worried\r\nthat you haven't been coming.", subtitle.Paragraphs[1].Text);

            // Test frames.
            Assert.AreEqual(SubtitleFormat.FramesToMilliseconds(2447), subtitle.Paragraphs[0].StartTime.TotalMilliseconds);
            Assert.AreEqual(SubtitleFormat.FramesToMilliseconds(2513), subtitle.Paragraphs[0].EndTime.TotalMilliseconds);

            // Test total lines.
            Assert.AreEqual(2, subtitle.Paragraphs[1].NumberOfLines);

            // Test quote inside/beginning of text.
            Assert.AreEqual("\"I see. Sorry, \"I'll drop\" by next time.\"", subtitle.Paragraphs[2].Text);
        }

        #endregion

        #region MacSub

        [TestMethod]
        public void MacSubTest()
        {
            const string input = @"/3035
Every satellite...
/3077
/3082
every constellation...
/3133
/3138
""souvenirs of space walks
and astronauts.“...""
/3205";
            var macSub = new MacSub();
            var subtitle = new Subtitle();
            macSub.LoadSubtitle(subtitle, new List<string>(input.SplitToLines()), null);

            // Test text.
            Assert.AreEqual("Every satellite...", subtitle.Paragraphs[0].Text);
            // Test line count.
            Assert.AreEqual(2, subtitle.Paragraphs[2].NumberOfLines);
            // Test frame.
            Assert.AreEqual(3082, SubtitleFormat.MillisecondsToFrames(subtitle.Paragraphs[1].StartTime.TotalMilliseconds));
        }

        #endregion

        #region TimedText

        [TestMethod]
        public void TimedTextNameSpaceTt()
        {
            var target = new TimedText10();
            var subtitle = new Subtitle();
            string raw = @"
<?xml version='1.0' encoding='UTF-8'?>
<tt:tt xmlns:tt='http://www.w3.org/ns/ttml' xmlns:ttp='http://www.w3.org/ns/ttml#parameter' xmlns:tts='http://www.w3.org/ns/ttml#styling' xmlns:ebuttm='urn:ebu:tt:metadata' xmlns:ebutts='urn:ebu:tt:style' ttp:timeBase='media' xml:lang='ca-ES' ttp:cellResolution='40 25'>
  <tt:head>
    <tt:metadata>
      <ebuttm:documentMetadata>
        <ebuttm:conformsToStandard>urn:ebu:tt:distribution:2014-01</ebuttm:conformsToStandard>
        <ebuttm:documentCountryOfOrigin>es</ebuttm:documentCountryOfOrigin>
      </ebuttm:documentMetadata>
    </tt:metadata>
  </tt:head>
  <tt:body>
    <tt:div>
      <tt:p xml:id='p1' region='r4' begin='00:00:04.400' end='00:00:08.520'>
        <tt:span style='ss2'>Hallo world.</tt:span>
      </tt:p>
    </tt:div>
  </tt:body>
</tt:tt>".Replace("'", "\"");
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);
            string actual = subtitle.Paragraphs[0].Text;
            const string expected = "Hallo world.";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TimedTextKeepDivs()
        {
            var target = new TimedText10();
            var subtitle = new Subtitle();
            string raw = @"
<?xml version='1.0' encoding='utf-8'?>
<tt xmlns='http://www.w3.org/ns/ttml' xmlns:tts='http://www.w3.org/ns/ttml#styling' xml:lang='en'>
  <head>
  </head>
  <body>
    <div>
      <p begin='00:00:01.000' id='p1' end='00:00:03.079'>Test.</p>
    </div>
    <div>
      <p begin='00:00:51.699' id='p2' end='00:00:54.040'>Now.</p>
      <p begin='00:00:54.064' id='p3' end='00:00:55.429'>We all alive.</p>
    </div>
    <div>
      <p begin='00:01:29.731' id='p4' end='00:01:32.447'>Here is what's left.</p>
    </div>
  </body>
</tt>".Replace("'", "\"");
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);
            var actual = target.ToText(subtitle, string.Empty);
            Assert.AreEqual(3, Utilities.CountTagInText(actual, "</div>"));
        }

        [TestMethod]
        public void TimedTextKeepReadNewLineAndAmpersand()
        {
            var target = new TimedText10();
            var subtitle = new Subtitle();
            var input = "You have got to do" + Environment.NewLine +
                        "what Johnson & Johnson did.";
            subtitle.Paragraphs.Add(new Paragraph(input, 0, 3000));
            var raw = target.ToText(subtitle, string.Empty);
            var subtitleNew = new Subtitle();
            target.LoadSubtitle(subtitleNew, raw.SplitToLines(), null);
            Assert.AreEqual(input, subtitleNew.Paragraphs[0].Text);
        }

        [TestMethod]
        public void TimedTextKeepSpaces()
        {
            var target = new TimedText10();
            var subtitle = new Subtitle();
            string raw = @"
<?xml version = '1.0' encoding='UTF-8' standalone='no'?>
<tt xmlns:tt='http://www.w3.org/ns/ttml' xmlns:ttm='http://www.w3.org/ns/ttml#metadata' xmlns:ttp='http://www.w3.org/ns/ttml#parameter' xmlns:tts='http://www.w3.org/ns/ttml#styling' ttp:tickRate='10000000' ttp:timeBase='media' xmlns='http://www.w3.org/ns/ttml'>
    <head>
        <ttp:profile use='http://netflix.com/ttml/profile/dfxp-ls-sdh'/>
        <styling>
            <style tts:color='white' tts:fontSize='100%' tts:fontWeight='normal' tts:textAlign='center' xml:id='normal'/>
            <style tts:color='white' tts:fontSize='100%' tts:fontStyle='italic' tts:fontWeight='normal' tts:textAlign='center' xml:id='normal_1'/>
        </styling>
        <layout>
            <region tts:displayAlign='after' tts:extent='80.00% 40.00%' tts:origin='10.00% 50.00%' xml:id='bottom'/>
            <region tts:displayAlign='before' tts:extent='80.00% 40.00%' tts:origin='10.00% 10.00%' xml:id='top'/>
        </layout>
    </head>
    <body region='bottom'>
        <div xml:space='preserve'>
            <p begin='1116782332t' end='1134466666t' style='normal' xml:id='subtitle2'><span style='normal_1'>AAA</span> <span style='normal_1'>BBB</span></p>
        </div>
    </body>
</tt>".Replace("'", "\"");
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);
            var actual = subtitle.Paragraphs.First().Text;
            Assert.AreEqual("AAA BBB", actual);
        }

        #endregion

        #region JacoSub
        [TestMethod]
        public void JacoSubSubtitleTest()
        {
            var jacobSub = new JacoSub();
            var subtitle = new Subtitle();
            const string text = @"1:55:52.16 1:55:53.20 D [Billy] That might have been my fault.
1:55:53.20 1:55:55.13 D That might have been my fault,\nI'm so sorry.";

            // Test text.
            jacobSub.LoadSubtitle(subtitle, new List<string>(text.SplitToLines()), null);
            Assert.AreEqual("[Billy] That might have been my fault.", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("That might have been my fault," + Environment.NewLine + "I'm so sorry.", subtitle.Paragraphs[1].Text);

            // Test time code.
            double expectedTotalMilliseconds = new TimeCode(1, 55, 52, SubtitleFormat.FramesToMilliseconds(16)).TotalMilliseconds;
            Assert.AreEqual(expectedTotalMilliseconds, subtitle.Paragraphs[0].StartTime.TotalMilliseconds);

            // Test total lines.
            Assert.AreEqual(2, subtitle.Paragraphs[1].NumberOfLines);
        }

        [TestMethod]
        public void JacoSubSubtitleTestItalicAndBold()
        {
            var jacobSub = new JacoSub();
            var subtitle = new Subtitle();
            const string text = @"1:55:52.16 1:55:53.20 D \BBilly\b That might have been my fault.
1:55:53.20 1:55:55.13 D That might have been my \Ifault\i.
1:55:53.20 1:55:55.13 D That might have been my \Ifault\N.
1:55:53.20 1:55:55.13 D That might have been \Bmy \Ifault\i\b.";

            jacobSub.LoadSubtitle(subtitle, new List<string>(text.SplitToLines()), null);

            Assert.AreEqual("<b>Billy</b> That might have been my fault.", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("That might have been my <i>fault</i>.", subtitle.Paragraphs[1].Text);
            Assert.AreEqual("That might have been my <i>fault</i>.", subtitle.Paragraphs[2].Text);
            Assert.AreEqual("That might have been <b>my <i>fault</i></b>.", subtitle.Paragraphs[3].Text);
        }

        [TestMethod]
        public void JacoSubSubtitleTestCommentRemoved()
        {
            var jacobSub = new JacoSub();
            var subtitle = new Subtitle();
            const string text = @"1:55:52.16 1:55:53.20 D Billy{billy is an actor} that might have been my fault.
1:55:53.20 1:55:55.13 D Test.";

            jacobSub.LoadSubtitle(subtitle, new List<string>(text.SplitToLines()), null);

            Assert.AreEqual("Billy that might have been my fault.", subtitle.Paragraphs[0].Text);
        }
        #endregion

        #region LambdaCap
        [TestMethod]
        public void LambdaCapTestItalic()
        {
            Configuration.Settings.General.CurrentFrameRate = 25;
            var lambdaCap = new LambdaCap();
            var subtitle = new Subtitle();
            const string text = "Lambda字幕V4 DF0+1 SCENE\"和文標準\"" + @" 

1	00000000/00000300	Line 1 with ＠斜３［italic］＠ word.
2	00000900/00001200	Line 1
				Line 2";

            lambdaCap.LoadSubtitle(subtitle, new List<string>(text.SplitToLines()), null);

            Assert.AreEqual("Line 1 with <i>italic</i> word.", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("Line 1" + Environment.NewLine + "Line 2", subtitle.Paragraphs[1].Text);
            Assert.AreEqual(3000, subtitle.Paragraphs[0].EndTime.TotalMilliseconds);
            Assert.AreEqual(2, subtitle.Paragraphs.Count);
        }
        #endregion

        #region WebVTT

        [TestMethod]
        public void WebVttFontColor()
        {
            var target = new WebVTT();
            var subtitle = new Subtitle();
            string raw = @"
WEBVTT

00:00:54.440 --> 00:00:58.920 align:middle line:-4
Hi, I'm Keith Lemon.

00:00:58.960 --> 00:01:03.280 align:middle line:-3
<c.yellow>AUDIENCE: Aww!</c>";
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);
            string actual = subtitle.Paragraphs[1].Text;
            const string expected = "<font color=\"yellow\">AUDIENCE: Aww!</font>";
            Assert.AreEqual(expected, actual);

            var webVtt = subtitle.ToText(target);
            Assert.IsTrue(webVtt.Contains("<c.yellow>AUDIENCE: Aww!</c>"));
        }

        public void WebVttFontColorHex()
        {
            var target = new WebVTT();
            var subtitle = new Subtitle();
            string raw = @"WEBVTT

00:00:54.440 --> 00:00:58.920 align:middle line:-4
Hi, I'm Keith Lemon.

00:00:58.960 --> 00:01:03.280 align:middle line:-3
<c.color008000>AUDIENCE: Aww!</c>";
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);

            Assert.AreEqual("<font color=\"#r008000\">AUDIENCE: Aww!</font>", subtitle.Paragraphs[1].Text);
        }

        [TestMethod]
        public void WebVttFontColorHex2()
        {
            var target = new WebVTT();
            var subtitle = new Subtitle();
            string raw = @"
WEBVTT

00:00:54.440 --> 00:00:58.920
<font color='#008000'>Text1</c>

00:00:58.960 --> 00:01:03.280 align:middle line:-3
<font color='#FF0000'>Text2</c>".Replace("'", "\"");
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);
            var webVtt = subtitle.ToText(target);

            Assert.IsTrue(webVtt.Contains("<c.color008000>Text1</c>"));
            Assert.IsTrue(webVtt.Contains("<c.red>Text2</c>"));
        }

        [TestMethod]
        public void WebVttSpaceBeforeTimeCode()
        {
            var target = new WebVTT();
            var subtitle = new Subtitle();
            string raw = @"WEBVTT

 00:39:32.240 --> 00:39:37.640 align:middle
Jag måste ge mig av.
Hemskt ledsen.

00:39:48.960 --> 00:39:51.120 align:middle

VÄLKOMMEN TILL TEXAS

00:40:15.520 --> 00:40:19.640 align:middle
-Hej, Martin.
-Hej, pappa.";
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);
            Assert.AreEqual(3, subtitle.Paragraphs.Count);
        }

        [TestMethod]
        public void WebVttFontBlankLine()
        {
            var target = new WebVTT();
            var subtitle = new Subtitle();
            string raw = @"WEBVTT

00:39:32.240 --> 00:39:37.640 align:middle
Jag måste ge mig av.
Hemskt ledsen.

00:39:48.960 --> 00:39:51.120 align:middle

VÄLKOMMEN TILL TEXAS

00:40:15.520 --> 00:40:19.640 align:middle
-Hej, Martin.
-Hej, pappa.";
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);
            string actual = subtitle.Paragraphs[1].Text;
            string expected = Environment.NewLine + "VÄLKOMMEN TILL TEXAS";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void WebVttFontColor2()
        {
            var target = new WebVTT();
            var subtitle = new Subtitle();
            string raw = @"WEBVTT

        00:02:00.000 --> 00:02:05.000
        <c.yellow.bg_blue>This is yellow text</c>

        00:04:00.000 --> 00:04:05.000
        <c.yellow.bg_blue.magenta.bg_black>This is magenta text</c>

        00:08:00.000 --> 00:09:05.000
        <c.color008000>This is hex colored</c>";
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);
            target.RemoveNativeFormatting(subtitle, new SubRip());
            Assert.AreEqual("<font color=\"yellow\">This is yellow text</font>", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("<font color=\"magenta\">This is magenta text</font>", subtitle.Paragraphs[1].Text);
            Assert.AreEqual("<font color=\"#008000\">This is hex colored</font>", subtitle.Paragraphs[2].Text);
        }

        [TestMethod]
        public void WebVttEscapeEncoding()
        {
            var target = new WebVTT();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<i>R&D</i>", 0, 0));
            subtitle.Paragraphs.Add(new Paragraph("i<5", 0, 0));
            subtitle.Paragraphs.Add(new Paragraph("i>6", 0, 0));
            subtitle.Paragraphs.Add(new Paragraph("<v Viggo>Hallo", 0, 0));
            subtitle.Paragraphs.Add(new Paragraph("&rlm;<c.arabic>مسلسلات NETFLIX ألاصلية</c.arabic>", 0, 0));
            var raw = subtitle.ToText(target);
            Assert.IsTrue(raw.Contains("<i>R&amp;D</i>"));
            Assert.IsTrue(raw.Contains("i&lt;5"));
            Assert.IsTrue(raw.Contains("i&gt;6"));
            Assert.IsTrue(raw.Contains("<v Viggo>Hallo"));
            Assert.IsTrue(raw.Contains("&rlm;<c.arabic>مسلسلات NETFLIX ألاصلية</c.arabic>"));
        }

        [TestMethod]
        public void WebVttEscapeDecoding()
        {
            var target = new WebVTT();
            var subtitle = new Subtitle();
            string raw = @"WEBVTT

00:00:00.000 --> 00:00:00.000
<i>R&amp;D</i>

00:00:00.000 --> 00:00:00.000
i&lt;5

00:00:00.000 --> 00:00:00.000
i&gt;6

00:00:00.000 --> 00:00:00.000
<v Viggo>Hallo

00:00:00.000 --> 00:00:00.000
&rlm;<c.arabic>مسلسلات NETFLIX ألاصلية</c.arabic>";
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);
            Assert.AreEqual("<i>R&D</i>", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("i<5", subtitle.Paragraphs[1].Text);
            Assert.AreEqual("i>6", subtitle.Paragraphs[2].Text);
            Assert.AreEqual("<v Viggo>Hallo", subtitle.Paragraphs[3].Text);
            Assert.AreEqual("&rlm;<c.arabic>مسلسلات NETFLIX ألاصلية</c.arabic>", subtitle.Paragraphs[4].Text);
        }

        [TestMethod]
        public void WebVttXTimestampHeader()
        {
            var target = new WebVTT();
            var subtitle = new Subtitle();
            string raw = @"WEBVTT
X-TIMESTAMP-MAP=MPEGTS:900000,LOCAL:00:00:00.000

00:00:16.360 --> 00:00:20.840
Line1

00:00:30.000 --> 00:00:35.000
Line2";
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);
            Assert.AreEqual("Line1", subtitle.Paragraphs[0].Text);
            Assert.IsTrue(Math.Abs(26360 - subtitle.Paragraphs[0].StartTime.TotalMilliseconds) < 0.01);
            Assert.IsTrue(Math.Abs(30840 - subtitle.Paragraphs[0].EndTime.TotalMilliseconds) < 0.01);
            Assert.AreEqual("Line2", subtitle.Paragraphs[1].Text);
            Assert.IsTrue(Math.Abs(40000 - subtitle.Paragraphs[1].StartTime.TotalMilliseconds) < 0.01);
        }

        [TestMethod]
        public void WebVttXTimestampHeader2()
        {
            var target = new WebVTT();
            var subtitle = new Subtitle();
            string raw = @"WEBVTT
X-TIMESTAMP-MAP=MPEGTS:900000,LOCAL:00:00:10.000

00:00:16.360 --> 00:00:20.840
Line1

00:00:30.000 --> 00:00:35.000
Line2";
            target.LoadSubtitle(subtitle, raw.SplitToLines(), null);
            Assert.AreEqual("Line1", subtitle.Paragraphs[0].Text);
            Assert.IsTrue(Math.Abs(16360 - subtitle.Paragraphs[0].StartTime.TotalMilliseconds) < 0.01);
            Assert.IsTrue(Math.Abs(20840 - subtitle.Paragraphs[0].EndTime.TotalMilliseconds) < 0.01);
            Assert.AreEqual("Line2", subtitle.Paragraphs[1].Text);
            Assert.IsTrue(Math.Abs(30000 - subtitle.Paragraphs[1].StartTime.TotalMilliseconds) < 0.01);
        }

        #endregion

        #region DvdStudioGraphics

        private const string DvdStudioGraphicsAsString = @"$SetFilePathToken = <<Graphic>>
01:00:42:00 , 01:00:46:22 , <<Graphic>>file.0001.tif
01:09:02:13 , 01:09:06:12 , <<Graphic>>file.0002.tif
01:09:06:13 , 01:09:10:02 , <<Graphic>>file.0003.tif
01:09:34:23 , 01:09:37:22 , <<Graphic>>file.0004.tif
01:36:15:02 , 01:36:21:01 , <<Graphic>>file.0005.tif
01:37:35:00 , 01:37:40:11 , <<Graphic>>file.0006.tif
01:38:34:17 , 01:38:37:14 , <<Graphic>>file.0007.tif";

        [TestMethod]
        public void DvdStudioProSpaceGraphicTestText()
        {
            var format = new DvdStudioProSpaceGraphic();
            var subtitle = new Subtitle();
            format.LoadSubtitle(subtitle, new List<string>(DvdStudioGraphicsAsString.SplitToLines()), null);
            Assert.AreEqual("file.0001.tif", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("file.0007.tif", subtitle.Paragraphs[subtitle.Paragraphs.Count - 1].Text);
        }

        [TestMethod]
        public void DvdStudioProSpaceGraphicShouldNotBeLoaded()
        {
            var lines = DvdStudioGraphicsAsString.SplitToLines();
            foreach (var format in SubtitleFormat.AllSubtitleFormats)
            {
                if (format.IsMine(lines, null))
                {
                    Assert.Fail("'DvdStudioProSpaceGraphic' should not be recognized by " + format.FriendlyName);
                }
            }
        }

        #endregion

        #region Structured Titles

        [TestMethod]
        public void StructuredTitlesLoad()
        {
            var format = new StructuredTitles();
            var subtitle = new Subtitle();
            format.LoadSubtitle(subtitle, (@"Structured titles
0001 : 00:00:03:18,00:00:05:22,1
80 80 81
C1N03 Top

0002 : 00:00:12:04,00:00:14:04,11
80 80 81
C1N03 <Italic>

0003 : 00:00:14:06,00:00:16:05,10
80 80 81
C1N03 Line 1
C1N03 Line 2").SplitToLines(), null);
            Assert.AreEqual("{\\an8}Top", subtitle.Paragraphs[0].Text);
            Assert.AreEqual("<i>Italic</i>", subtitle.Paragraphs[1].Text);
        }

        [TestMethod]
        public void StructuredTitlesSave()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("{\\an8}Top", 0, 2000));
            sub.Paragraphs.Add(new Paragraph("<i>Italic</i>", 3000, 5000));
            var format = new StructuredTitles();
            var txt = format.ToText(sub, null);
            Assert.IsTrue(txt.Contains(",1" + Environment.NewLine));
            Assert.IsFalse(txt.Contains("{\\an8}"));
            Assert.IsTrue(txt.Contains("<Italic>"));
        }

        #endregion
    }
}
