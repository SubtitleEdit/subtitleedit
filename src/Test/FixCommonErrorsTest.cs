using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Test
{
    /// <summary>
    ///This is a test class for FixCommonErrors and is intended
    ///to contain all FixCommonErrorsTest Unit Tests
    ///</summary>
    [TestClass]
    public class FixCommonErrorsTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        private static void InitializeFixCommonErrorsLine(FixCommonErrors_Accessor target, string line)
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph(line, 100, 10000));
            target.Initialize(subtitle, new Nikse.SubtitleEdit.Logic.SubtitleFormats.SubRip(), System.Text.Encoding.UTF8);
        }

        private static void InitializeFixCommonErrorsLine(FixCommonErrors_Accessor target, string line, string line2)
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph(line, 100, 10000));
            subtitle.Paragraphs.Add(new Paragraph(line2, 10001, 30000));
            target.Initialize(subtitle, new Nikse.SubtitleEdit.Logic.SubtitleFormats.SubRip(), System.Text.Encoding.UTF8);
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

        #region Merge short lines
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesNormal()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "This is" + Environment.NewLine + "short!");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This is short!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesLong()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "This I'm pretty sure is not a" + Environment.NewLine + "short line, that should be merged!!!");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This I'm pretty sure is not a" + Environment.NewLine + "short line, that should be merged!!!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesNormalItalic()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>This is" + Environment.NewLine + "short!</i>");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>This is short!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialogue()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "- Hallo!" + Environment.NewLine + "- Hi");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Hallo!" + Environment.NewLine + "- Hi");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialogueItalic()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>- Hallo!" + Environment.NewLine + "- Hi</i>");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>- Hallo!" + Environment.NewLine + "- Hi</i>");
        }


        /// <summary>
        ///A test for Merge short lines
        ///</summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialogueItalicTwo()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>- Hallo!</i>" + Environment.NewLine + "<i>- Hi<i>");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>- Hallo!</i>" + Environment.NewLine + "<i>- Hi<i>");
        }
        #endregion

        #region Fix Italics
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBeginOnly()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>Hey!" + Environment.NewLine + "<i>Boy!");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>Hey!</i>" + Environment.NewLine + "<i>Boy!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsFirstLineEndMissing()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>(jones) seems their attackers headed north." + Environment.NewLine + "<i>Hi!</i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>(jones) seems their attackers headed north." + Environment.NewLine + "Hi!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsStartInMiddle()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "Seems their <i>attackers headed north.");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Seems their attackers headed north.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsEmptyStart()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i></i>test");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "test");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsSecondLineMissingEnd()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "- And..." + Environment.NewLine + "<i>Awesome it is!");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- And..." + Environment.NewLine + "<i>Awesome it is!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "Awesome it is!</i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Awesome it is!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding2()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "Awesome it is!<i></i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Awesome it is!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding3()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "Awesome it is!<i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Awesome it is!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding4()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "Awesome it is!</i><i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Awesome it is!");
        }
        #endregion

        #region Fix Missing Periods At End Of Line
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingPeriodsAtEndOfLineNone()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "This is line one!" + Environment.NewLine + "<i>Boy!</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
            target.FixMissingPeriodsAtEndOfLine();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingPeriodsAtEndOfLineItalicAndMissing()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "This is line one!" + Environment.NewLine + "<i>Boy</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
            target.FixMissingPeriodsAtEndOfLine();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This is line one!" + Environment.NewLine + "<i>Boy.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingPeriodsAtEndOfLineItalicAndMissing2()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>This is line one!" + Environment.NewLine + "Boy</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
            target.FixMissingPeriodsAtEndOfLine();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>This is line one!" + Environment.NewLine + "Boy.</i>");
        }
        #endregion

        #region Fix Hyphens (add dash)
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash1()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "Hi Joe!" + Environment.NewLine + "- Hi Pete!");
            target.FixHyphensAdd();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash2()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "- Hi Joe!" + Environment.NewLine + "Hi Pete!");
            target.FixHyphensAdd();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash2italic()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>- Hi Joe!" + Environment.NewLine + "Hi Pete!</i>");
            target.FixHyphensAdd();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>- Hi Joe!" + Environment.NewLine + "- Hi Pete!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash3NoChange()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
            target.FixHyphensAdd();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash4NoChange()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "- Hi!" + Environment.NewLine + "- Hi Pete!");
            target.FixHyphensAdd();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Hi!" + Environment.NewLine + "- Hi Pete!");
        }

        #endregion

        #region Fix OCR errors
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsSlashMakesTwoWords()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "(laughing/clapping)");
            target.FixOcrErrorsViaReplaceList("eng");
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "(laughing/clapping)");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsSlashIsL()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "The font is ita/ic!");
            target.FixOcrErrorsViaReplaceList("eng");
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "The font is italic!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsDashedWords()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "The clock is 12 a.m.");
            target.FixOcrErrorsViaReplaceList("eng");
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "The clock is 12 a.m.");
        }
        #endregion


        #region Fix missingspaces
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesItalicBegin()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "The<i>Bombshell</i> will gone.");
            target.FixMissingSpaces();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "The <i>Bombshell</i> will gone.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesItalicEnd()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "The <i>Bombshell</i>will gone.");
            target.FixMissingSpaces();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "The <i>Bombshell</i> will gone.");
        }

        #endregion
    }
}
