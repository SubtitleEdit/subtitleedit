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

        #endregion Additional test attributes

        #region SubRip (.srt)

        private static List<string> GetSrtLines(string text)
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
            var target = new SubRip();
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
            var target = new SubRip();
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
            var target = new SubRip();
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
            var target = new SubRip();
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
            var target = new SubRip();
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
            var target = new AdvancedSubStationAlpha();
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
            var target = new AdvancedSubStationAlpha();
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
            var target = new AdvancedSubStationAlpha();
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
            var target = new AdvancedSubStationAlpha();
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
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\fnArial}Font"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font face=\"Arial\">Font</font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssSimpleFontColor1()
        {
            var target = new AdvancedSubStationAlpha();
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
            var target = new AdvancedSubStationAlpha();
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
            var target = new AdvancedSubStationAlpha();
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
            var target = new AdvancedSubStationAlpha();
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
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\1c&HFFFF00&\i1}CYAN{\i0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font color=\"#00ffff\"><i>CYAN</i></font>";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AssFontNameAndSize()
        {
            var target = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetAssLines(@"{\fnViner Hand ITC\fs28}Testing"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font face=\"Viner Hand ITC\" size=\"28\">Testing</font>";
            Assert.AreEqual(expected, actual);
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
            var target = new SubStationAlpha();
            var subtitle = new Subtitle();
            target.LoadSubtitle(subtitle, GetSsaLines(@"{\c&HFFFF00&\i1}CYAN{\i0}"), null);
            string actual = subtitle.Paragraphs[0].Text;
            string expected = "<font color=\"#00ffff\"><i>CYAN</i></font>";
            Assert.AreEqual(expected, actual);
        }

        #endregion Sub Station Alpha (.ssa)

        #region DCinema smpte (.xml)

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void DcinemaSmpteItalic()
        {
            var target = new DCinemaSmpte2010();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<i>Italic</i>", 1000, 5000));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text.Contains("<dcst:Font Italic=\"yes\">Italic</dcst:Font>"));
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
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

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void DcinemaInteropItalic()
        {
            var target = new DCSubtitle();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<i>Italic</i>", 1000, 5000));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text.Contains("<Font Italic=\"yes\""));
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void DcinemaInteropColorAndItalic()
        {
            var target = new DCSubtitle();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<font color=\"#ff0000\"><i>Red</i></font>", 1000, 5000));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text.Contains(" Italic=\"yes\"") && text.Contains(" Color=\"FFFF0000\""));
        }

        #endregion DCinema interop (.xml)

        #region MicroDVD

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdItalic()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<i>Italic</i>", 0, 0));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text == "{0}{0}{Y:i}Italic" || text == "{0}{0}{y:i}Italic");
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdBold()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<b>Bold</b>", 0, 0));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text == "{0}{0}{Y:b}Bold" || text == "{0}{0}{y:b}Bold");
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdUnderline()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<u>Underline</u>", 0, 0));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text == "{0}{0}{Y:u}Underline" || text == "{0}{0}{y:u}Underline");
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdUnderlineItalic()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<u><i>Underline Italic</i></u>", 0, 0));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text == "{0}{0}{Y:u}{Y:i}Underline Italic" || text == "{0}{0}{y:u}{y:i}Underline Italic");
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdItalicUnderline()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("<i><u>Underline Italic</u></i>", 0, 0));
            string text = target.ToText(subtitle, "title");
            Assert.IsTrue(text == "{0}{0}{Y:i}{Y:u}Underline Italic" || text == "{0}{0}{y:i}{y:u}Underline Italic");
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdReadBoldItalic()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            List<string> list = new List<string>();
            list.Add("{0}{0}{y:i,b}Hello!");
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<i><b>Hello!</b></i>");
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdReadFont()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            List<string> list = new List<string>();
            list.Add("{0}{0}{C:$FF0000}Blue");
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<font color=\"#0000FF\">Blue</font>" || text == "<font color=\"blue\">Blue</font>");
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdReadAdvanced()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            List<string> list = new List<string>();
            list.Add("{0}{25}{c:$0000ff}{y:b,u}{f:DeJaVuSans}{s:12}Hello!");
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<font color=\"#ff0000\"><b><u><font face=\"DeJaVuSans\"><font size=\"12\">Hello!</font></font></u></b></font>");
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdReadBoldFirstLineOnly()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            List<string> list = new List<string>();
            list.Add("{0}{0}{y:i}Hello!|Hello!");
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<i>Hello!</i>" + Environment.NewLine + "Hello!");
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdReadBoldSecondLineOnly()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            List<string> list = new List<string>();
            list.Add("{0}{0}Hello!|{y:i}Hello!");
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "Hello!" + Environment.NewLine + "<i>Hello!</i>");
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdReadItalicBothLines()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            List<string> list = new List<string>();
            list.Add("{0}{0}{Y:i}Hello!|Hello!");
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<i>Hello!" + Environment.NewLine + "Hello!</i>" ||
                          text == "<i>Hello!</i>" + Environment.NewLine + "<i>Hello!</i>");
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MicroDvdReadBoldBothLinesItalicFirst()
        {
            var target = new MicroDvd();
            var subtitle = new Subtitle();
            List<string> list = new List<string>();
            list.Add("{0}{0}{Y:b}{y:i}Hello!|Hello!");
            target.LoadSubtitle(subtitle, list, null);
            string text = subtitle.Paragraphs[0].Text;
            Assert.IsTrue(text == "<b><i>Hello!</i>" + Environment.NewLine + "Hello!</b>" ||
                          text == "<b><i>Hello!</i></b>" + Environment.NewLine + "<b>Hello!</b>");
        }

        #endregion MicroDVD

        #region Scenerist SCC

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void CheckTimeCodes()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Line1", 1000, 5000));
            subtitle.Paragraphs.Add(new Paragraph("Line2", 6000, 8000));
            subtitle.Paragraphs.Add(new Paragraph("Line2", 8000, 12000));

            string text = "Scenarist_SCC V1.0\n\n"
                + "00:00:00:00\t94ae 94ae 9420 9420 9476 9476 2080 4ce9 6ee5 3180\n\n"
                + "00:00:00:12\t942c 942c\n\n"
                + "00:00:01:00\t942f 942f\n\n"
                + "00:00:01:04\t94ae 94ae 9420 9420 9476 9476 2080 4ce9 6ee5 3280\n\n"
                + "00:00:02:00\t942c 942c\n\n"
                + "00:00:03:00\t942f 942f\n\n"
                + "00:00:03:04\t94ae 94ae 9420 9420 9476 9476 2080 4ce9 6ee5 b380\n\n"
                + "00:00:04:00\t942c 942c\n\n"
                + "00:00:05:00\t942f 942f\n\n"
                + "00:00:06:00\t942c 942c";

            var expected = new Subtitle();
            expected.Paragraphs.Add(new Paragraph("Line1",
                TimeCode.ParseHHMMSSFFToMilliseconds("00:00:01:00"), TimeCode.ParseHHMMSSFFToMilliseconds("00:00:02:00")));
            expected.Paragraphs.Add(new Paragraph("Line2",
                TimeCode.ParseHHMMSSFFToMilliseconds("00:00:03:00"), TimeCode.ParseHHMMSSFFToMilliseconds("00:00:04:00")));
            expected.Paragraphs.Add(new Paragraph("Line3",
                TimeCode.ParseHHMMSSFFToMilliseconds("00:00:05:00"), TimeCode.ParseHHMMSSFFToMilliseconds("00:00:06:00")));

            var actual = new Subtitle();

            List<string> lines = new List<string>();
            foreach (string line in text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                lines.Add(line);

            var target = new ScenaristClosedCaptions();
            target.LoadSubtitle(actual, lines, null);

            Assert.AreEqual(expected.Paragraphs.Count, actual.Paragraphs.Count);
            for (int i = 0; i < expected.Paragraphs.Count; i++)
            {
                var expectedP = expected.Paragraphs[i];
                var actualP = actual.Paragraphs[i];
                Assert.AreEqual(expectedP.Text, actualP.Text, "text index: " + i);
                Assert.AreEqual(expectedP.StartTime.TotalMilliseconds, actualP.StartTime.TotalMilliseconds, 1, "start index: " + i);
                Assert.AreEqual(expectedP.EndTime.TotalMilliseconds, actualP.EndTime.TotalMilliseconds, 1, "end index: " + i);
            }
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void CheckTimeCodes_RclAndEocOnSameLine()
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph("Line1", 1000, 5000));
            subtitle.Paragraphs.Add(new Paragraph("Line2", 6000, 8000));
            subtitle.Paragraphs.Add(new Paragraph("Line2", 8000, 12000));


            /*
             * This format is present in the 'Clickers.scc' file:
             *   [Timecode]\t{RCL}{RCL}{ENM}{ENM}text{EOC}{EOC} \n\n
             *   
             * The caption text is displayed when EOC is received.  The previous text
             * remains on-screen until the next group is shown.
             * 
             * For this test, the {EOC} command is the 11th or 7th frame in, so we back up
             * that number of frames so the display is on a second boundary.
             */
            string text = "Scenarist_SCC V1.0\n\n"
                          + "00:00:00:14\t9420 9420 94ae 94ae 9476 9476 2080 4ce9 6ee5 3180 942f 942f \n\n"
                          + "00:00:01:14\t94ae 94ae 9420 9420 9476 9476 2080 4ce9 6ee5 3280 942f 942f \n\n"
                          + "00:00:02:14\t94ae 94ae 9420 9420 9476 9476 2080 4ce9 6ee5 b380 942f 942f \n\n"
                          + "00:00:03:18\t9420 9420 94ae 94ae 9470 9470 942f 942f \n\n";

            var expected = new Subtitle();
            expected.Paragraphs.Add(new Paragraph("Line1",
                TimeCode.ParseHHMMSSFFToMilliseconds("00:00:01:00"), TimeCode.ParseHHMMSSFFToMilliseconds("00:00:02:00")));
            expected.Paragraphs.Add(new Paragraph("Line2",
                TimeCode.ParseHHMMSSFFToMilliseconds("00:00:02:00"), TimeCode.ParseHHMMSSFFToMilliseconds("00:00:03:00")));
            expected.Paragraphs.Add(new Paragraph("Line3",
                TimeCode.ParseHHMMSSFFToMilliseconds("00:00:03:00"), TimeCode.ParseHHMMSSFFToMilliseconds("00:00:04:00")));

            var actual = new Subtitle();

            List<string> lines = new List<string>();
            foreach (string line in text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                lines.Add(line);

            var target = new ScenaristClosedCaptions();
            target.LoadSubtitle(actual, lines, null);

            Assert.AreEqual(expected.Paragraphs.Count, actual.Paragraphs.Count);
            for (int i = 0; i < expected.Paragraphs.Count; i++)
            {
                var expectedP = expected.Paragraphs[i];
                var actualP = actual.Paragraphs[i];
                Assert.AreEqual(expectedP.Text, actualP.Text, "text index: " + i);
                Assert.AreEqual(expectedP.StartTime.TotalMilliseconds, actualP.StartTime.TotalMilliseconds, 1, "start index: " + i);
                Assert.AreEqual(expectedP.EndTime.TotalMilliseconds, actualP.EndTime.TotalMilliseconds, 1, "end index: " + i);
            }
        }

        #endregion Scenerist SCC

        #region All subtitle formats

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
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

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
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
        //         [DeploymentItem("SubtitleEdit.exe")]
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
        //                         if (format.FriendlyName != innerFormat.FriendlyName  &&
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
    }
}
