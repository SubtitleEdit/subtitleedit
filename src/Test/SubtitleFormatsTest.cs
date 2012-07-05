using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

namespace Test
{
    /// <summary>
    ///This is a test class for subtitle formats and is intended
    ///to contain all subtitle formats Unit Tests
    ///</summary>
    [TestClass()]
    public class SubtitleFormatsTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

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
        #endregion

        #region Subrip (.srt)


        private List<string> GetSrtLines(string text)
        {
            var lines = new List<string>();
            string[] arr = text.Replace(Environment.NewLine, "\r").Replace("\n", "\r").Split('\r');
            foreach (string line in arr)
                lines.Add(line);
            return lines;
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void SrtCoordinates()
        {
            var target = new SubRip_Accessor();
            var subtitle = new Subtitle();
            string text =
@"1
00:00:02,001 --> 00:00:16,001  X1:000 X2:000 Y1:050 Y2:100
Let us have some! Let us have some!";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "Let us have some! Let us have some!";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void SrtNoLineNumbers()
        {
            var target = new SubRip_Accessor();
            var subtitle = new Subtitle();
            string text =
@"00:00:03,000 --> 00:00:08,000
Line1.

00:00:08,000 --> 00:00:09,920
Line 2.";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            string actual = subtitle.Paragraphs.Count.ToString();
            string expected = "2";
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void SrtDotsInsteadOfCommas()
        {
            var target = new SubRip_Accessor();
            var subtitle = new Subtitle();
            string text =
@"2
00:00:04.501 --> 00:00:08.500
Dots instead of commas";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "Dots instead of commas";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void SrtTwoLiner()
        {
            var target = new SubRip_Accessor();
            var subtitle = new Subtitle();
            string text =
@"2
00:00:04.501 --> 00:00:08.500
Line 1
Line 2";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "Line 1" + Environment.NewLine + "Line 2";
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void SrtThreeLiner()
        {
            var target = new SubRip_Accessor();
            var subtitle = new Subtitle();
            string text =
@"2
00:00:04.501 --> 00:00:08.500
Line 1
Line 2
Line 3";
            target.LoadSubtitle(subtitle, GetSrtLines(text), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "Line 1" + Environment.NewLine + "Line 2" + Environment.NewLine + "Line 3";
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Advanced Sub Station alpha (.ass)


        private List<string> GetAssLines(string lineOneText)
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
            var lines = new List<string>();
            string[] arr = text.Replace(Environment.NewLine, "\r").Replace("\n", "\r").Split('\r');
            foreach (string line in arr)
                lines.Add(line);
            return lines;
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssSimpleItalic()
        {
            var target = new AdvancedSubStationAlpha_Accessor();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\i1}Italic{\i0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<i>Italic</i>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssSimpleBold()
        {
            var target = new AdvancedSubStationAlpha_Accessor();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\b1}Bold{\b0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<b>Bold</b>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssSimpleUnderline()
        {
            var target = new AdvancedSubStationAlpha_Accessor();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\u1}Underline{\u0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<u>Underline</u>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssSimpleFontSize()
        {
            var target = new AdvancedSubStationAlpha_Accessor();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\fs28}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font size=\"28\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssSimpleFontName()
        {
            var target = new AdvancedSubStationAlpha_Accessor();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\fnArial}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font name=\"Arial\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssSimpleFontColor1()
        {
            var target = new AdvancedSubStationAlpha_Accessor();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\c&HFF&}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font color=\"#ff0000\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssSimpleFontColor2()
        {
            var target = new AdvancedSubStationAlpha_Accessor();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\c&HFF00&}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font color=\"#00ff00\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssSimpleFontColor3()
        {
            var target = new AdvancedSubStationAlpha_Accessor();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\c&HFF0000&}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font color=\"#0000ff\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssSimpleFontColor4()
        {
            var target = new AdvancedSubStationAlpha_Accessor();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\c&HFFFFFF&}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font color=\"#ffffff\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssSimpleFontColorAndItalic()
        {
            var target = new AdvancedSubStationAlpha_Accessor();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\1c&HFFFF00&\i1}CYAN{\i0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font color=\"#00ffff\"><i>CYAN</i></font>";
            Assert.AreEqual(expected, actual);
        }

        #endregion


        #region Sub Station Alpha (.ssa)

        private List<string> GetSsaLines(string lineOneText)
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
            var lines = new List<string>();
            string[] arr = text.Replace(Environment.NewLine, "\r").Replace("\n", "\r").Split('\r');
            foreach (string line in arr)
                lines.Add(line);
            return lines;
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void SsaSimpleFontColorAndItalic()
        {
            var target = new SubStationAlpha_Accessor();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetSsaLines(@"{\c&HFFFF00&\i1}CYAN{\i0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font color=\"#00ffff\"><i>CYAN</i></font>";
            Assert.AreEqual(expected, actual);
        }

        #endregion


        #region DCinema smpte (.xml)

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void DcinemaSmpteItalic()
        {
            var target = new DCinemaSmpte_Accessor();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<i>Italic</i>", 1000, 5000));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text.Contains("<dcst:Font Italic=\"yes\">Italic</dcst:Font>"));
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void DcinemaSmpteColorAndItalic()
        {
            var target = new DCinemaSmpte_Accessor();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<font color=\"#ff0000\"><i>Red</i></font>", 1000, 5000));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text.Contains("<dcst:Font Italic=\"yes\" Color=\"FFFF0000\">Red</dcst:Font>"));
        }

        #endregion


        #region DCinema interop (.xml)

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void DcinemaInteropItalic()
        {
            var target = new DCSubtitle_Accessor();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<i>Italic</i>", 1000, 5000));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text.Contains("<Font Italic=\"yes\">Italic</Font>"));
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void DcinemaInteropColorAndItalic()
        {
            var target = new DCSubtitle_Accessor();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<font color=\"#ff0000\"><i>Red</i></font>", 1000, 5000));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text.Contains("<Font Italic=\"yes\" Color=\"FFFF0000\">Red</Font>"));
        }

        #endregion

    }
}
