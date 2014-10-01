using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Forms;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;

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

        private static Nikse.SubtitleEdit.Forms.FixCommonErrors GetFixCommonErrorsLib()
        {
            return new Nikse.SubtitleEdit.Forms.FixCommonErrors();
        }

        private static void InitializeFixCommonErrorsLine(Nikse.SubtitleEdit.Forms.FixCommonErrors target, string line)
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph(line, 100, 10000));
            target.Initialize(subtitle, new SubRip(), System.Text.Encoding.UTF8);
        }

        private static void InitializeFixCommonErrorsLine(Nikse.SubtitleEdit.Forms.FixCommonErrors target, string line, string line2)
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph(line, 100, 10000));
            subtitle.Paragraphs.Add(new Paragraph(line2, 10001, 30000));
            target.Initialize(subtitle, new SubRip(), System.Text.Encoding.UTF8);
        }

        #region Additional test attributes

        //
        //You can use the following additional attributes as you write your tests:
        //

        /// <summary>
        /// Copies the contents of input to output. Doesn't close either stream.
        /// </summary>
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

            if (!Directory.Exists("Directories"))
                Directory.CreateDirectory("Dictionaries");
            var strm = asm.GetManifestResourceStream("Test.Dictionaries.en_US.aff");
            if (strm != null)
            {
                using (Stream file = File.OpenWrite(Path.Combine("Dictionaries", "en_US.aff")))
                {
                    CopyStream(strm, file);
                }
            }

            strm = asm.GetManifestResourceStream("Test.Dictionaries.en_US.dic");
            if (strm != null)
            {
                using (Stream file = File.OpenWrite(Path.Combine("Dictionaries", "en_US.dic")))
                {
                    CopyStream(strm, file);
                }
            }

            strm = asm.GetManifestResourceStream("Test.Dictionaries.names_etc.xml");
            if (strm != null)
            {
                using (Stream file = File.OpenWrite(Path.Combine("Dictionaries", "names_etc.xml")))
                {
                    CopyStream(strm, file);
                }
            }

        }

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

        #region Merge short lines

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesNormal()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "This is" + Environment.NewLine + "short!");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This is short!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesLong()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "This I'm pretty sure is not a" + Environment.NewLine + "short line, that should be merged!!!");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This I'm pretty sure is not a" + Environment.NewLine + "short line, that should be merged!!!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesNormalItalic()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<i>This is" + Environment.NewLine + "short!</i>");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>This is short!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialog()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "- Hallo!" + Environment.NewLine + "- Hi");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Hallo!" + Environment.NewLine + "- Hi");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialogItalic()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<i>- Hallo!" + Environment.NewLine + "- Hi</i>");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>- Hallo!" + Environment.NewLine + "- Hi</i>");
        }

        /// <summary>
        ///A test for Merge short lines
        ///</summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialogItalicTwo()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<i>- Hallo!</i>" + Environment.NewLine + "<i>- Hi<i>");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>- Hallo!</i>" + Environment.NewLine + "<i>- Hi<i>");
        }

        #endregion Merge short lines

        #region Fix Italics

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBeginOnly()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<i>Hey!" + Environment.NewLine + "<i>Boy!");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>Hey!</i>" + Environment.NewLine + "<i>Boy!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsFirstLineEndMissing()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<i>(jones) seems their attackers headed north." + Environment.NewLine + "<i>Hi!</i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>(jones) seems their attackers headed north." + Environment.NewLine + "Hi!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsStartInMiddle()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Seems their <i>attackers headed north.");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Seems their attackers headed north.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsEmptyStart()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<i></i>test");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "test");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsSecondLineMissingEnd()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "- And..." + Environment.NewLine + "<i>Awesome it is!");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- And..." + Environment.NewLine + "<i>Awesome it is!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Awesome it is!</i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>Awesome it is!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding2()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Awesome it is!<i></i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Awesome it is!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding3()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Awesome it is!<i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Awesome it is!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding4()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Awesome it is!</i><i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Awesome it is!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsLine1BadEnding()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "</i>What do i care.</i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>What do i care.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsLine1BadEndingDouble()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<i>To be a life-changing weekend</i>" + Environment.NewLine + "<i>for all of us.");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>To be a life-changing weekend" + Environment.NewLine + "for all of us.</i>");
        }

        #endregion Fix Italics

        #region Fix Missing Periods At End Of Line

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingPeriodsAtEndOfLineNone()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "This is line one!" + Environment.NewLine + "<i>Boy!</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
            target.FixMissingPeriodsAtEndOfLine();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingPeriodsAtEndOfLineItalicAndMissing()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "This is line one!" + Environment.NewLine + "<i>Boy</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
            target.FixMissingPeriodsAtEndOfLine();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This is line one!" + Environment.NewLine + "<i>Boy.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingPeriodsAtEndOfLineItalicAndMissing2()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<i>This is line one!" + Environment.NewLine + "Boy</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
            target.FixMissingPeriodsAtEndOfLine();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>This is line one!" + Environment.NewLine + "Boy.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingPeriodsAtEndOfLineWithSpace()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "”... and gently down I laid her. ”");
            target.FixMissingPeriodsAtEndOfLine();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "”... and gently down I laid her. ”");
        }

        #endregion Fix Missing Periods At End Of Line

        #region Fix Hyphens (add dash)

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash1()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Hi Joe!" + Environment.NewLine + "- Hi Pete!");
            target.FixHyphensAdd();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash2()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "- Hi Joe!" + Environment.NewLine + "Hi Pete!");
            target.FixHyphensAdd();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash2Italic()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<i>- Hi Joe!" + Environment.NewLine + "Hi Pete!</i>");
            target.FixHyphensAdd();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>- Hi Joe!" + Environment.NewLine + "- Hi Pete!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash3NoChange()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
            target.FixHyphensAdd();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash4NoChange()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "- Hi!" + Environment.NewLine + "- Hi Pete!");
            target.FixHyphensAdd();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Hi!" + Environment.NewLine + "- Hi Pete!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDashButNotInFirst()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Five-Both?" + Environment.NewLine + "- T... T... Ten...");
            target.FixHyphensAdd();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Five-Both?" + Environment.NewLine + "- T... T... Ten...");
        }

        #endregion Fix Hyphens (add dash)

        #region Fix OCR errors

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsSlashMakesTwoWords()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "(laughing/clapping)");
            target.FixOcrErrorsViaReplaceList("eng");
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "(laughing/clapping)");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsSlashIsL() // requires hardcoded rules enabled
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "The font is ita/ic!");
            target.FixOcrErrorsViaReplaceList("eng");
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "The font is italic!"); // will fail if English dictionary is not found
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsDashedWords()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "The clock is 12 a.m.");
            target.FixOcrErrorsViaReplaceList("eng");
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "The clock is 12 a.m.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsNoStartWithLargeAfterThreePeriods()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "- I'll ring her." + Environment.NewLine + "- ...in a lot of trouble.");
            target.FixOcrErrorsViaReplaceList("eng");
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- I'll ring her." + Environment.NewLine + "- ...in a lot of trouble.");
        }

        #endregion Fix OCR errors

        #region Fix missingspaces

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesItalicBegin()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "The<i>Bombshell</i> will gone.");
            target.FixMissingSpaces();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "The <i>Bombshell</i> will gone.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesItalicEnd()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "The <i>Bombshell</i>will gone.");
            target.FixMissingSpaces();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "The <i>Bombshell</i> will gone.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesBeforePeriod1()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "It will be okay.It surely will be!");
            target.FixMissingSpaces();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "It will be okay. It surely will be!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesBeforePeriod2()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "you can't get out.Alright?");
            target.FixMissingSpaces();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "you can't get out. Alright?");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesNoChange1()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "What did Dr. Gey say?");
            target.FixMissingSpaces();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "What did Dr. Gey say?");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesNoChange2()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Go to the O.R. now!");
            target.FixMissingSpaces();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Go to the O.R. now!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesNoChange3()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Email niksedk@gmail.Com now!");
            target.FixMissingSpaces();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Email niksedk@gmail.Com now!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesNoChange4()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Go to www.Nikse.Dk for more info");
            target.FixMissingSpaces();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Go to www.Nikse.Dk for more info");
        }

        #endregion Fix missingspaces

        #region Start with uppercase after paragraph

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphMusic1()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "♪ you like to move it...");
            target.FixStartWithUppercaseLetterAfterParagraph();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "♪ You like to move it...");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormal1()
        {
            Paragraph prev = new Paragraph("Bye.", 0, 1000);
            Paragraph p = new Paragraph("bye.", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "Bye.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormal2()
        {
            Paragraph prev = new Paragraph("Bye.", 0, 1000);
            Paragraph p = new Paragraph("<i>bye.</i>", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "<i>Bye.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormal3()
        {
            Paragraph prev = new Paragraph("<i>Bye.</i>", 0, 1000);
            Paragraph p = new Paragraph("<i>bye.</i>", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "<i>Bye.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormal4()
        {
            Paragraph prev = new Paragraph("Bye.", 0, 1000);
            Paragraph p = new Paragraph("bye." + Environment.NewLine + "bye.", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "Bye." + Environment.NewLine + "Bye.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalNoChange1()
        {
            Paragraph prev = new Paragraph("Bye,", 0, 1000);
            Paragraph p = new Paragraph("bye.", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "bye.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialog1()
        {
            Paragraph prev = new Paragraph("Bye.", 0, 1000);
            Paragraph p = new Paragraph("- Moss! Jesus Christ!" + Environment.NewLine + "- what is it?", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "- Moss! Jesus Christ!" + Environment.NewLine + "- What is it?");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialog2()
        {
            Paragraph prev = new Paragraph("Bye.", 0, 1000);
            Paragraph p = new Paragraph("<i>- Moss! Jesus Christ!" + Environment.NewLine + "- what is it?</i>", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "<i>- Moss! Jesus Christ!" + Environment.NewLine + "- What is it?</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialog3()
        {
            Paragraph prev = new Paragraph("Bye.", 0, 1000);
            Paragraph p = new Paragraph("<i>- Moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- what is it?</i>", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "<i>- Moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- What is it?</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialog4()
        {
            Paragraph prev = new Paragraph("Bye.", 0, 1000);
            Paragraph p = new Paragraph("<i>- moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- what is it?</i>", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "<i>- Moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- What is it?</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalNoChange2()
        {
            Paragraph prev = new Paragraph("Bye", 0, 1000);
            Paragraph p = new Paragraph("bye.", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "bye.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialogNoChange1()
        {
            Paragraph prev = new Paragraph("Bye -", 0, 1000);
            Paragraph p = new Paragraph("- moss!", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "- moss!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialogNoChange2()
        {
            Paragraph prev = new Paragraph("Bye -", 0, 1000);
            Paragraph p = new Paragraph("- moss!" + Environment.NewLine + " - Bye.", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "- moss!" + Environment.NewLine + " - Bye.");
        }

        #endregion Start with uppercase after paragraph

        #region Fix Spanish question- and exclamation-marks

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishNormalQuestion1()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Cómo estás?");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¿Cómo estás?");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishNormalExclamationMark1()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Cómo estás!");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¡Cómo estás!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkDouble()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "¡¡PARA!!");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¡¡PARA!!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkTriple()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "¡¡¡PARA!!!");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¡¡¡PARA!!!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkAndQuestionMark()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "¿Cómo estás?!");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¡¿Cómo estás?!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkAndQuestionMarkManyTagsDoubleExcl()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<i>Chanchita, ¡¿copias?! Chanchita!!</i>");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>Chanchita, ¡¿copias?! ¡¡Chanchita!!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkAndQuestionMarkOneOfEach()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "¡Cómo estás?");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¿¡Cómo estás!?");
        }

        #endregion Fix Spanish question- and exclamation-marks

        #region FixHyphens

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash1Italic()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<i>- Mm-hmm.</i>");
            target.FixHyphens();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>Mm-hmm.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash1Font()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "<font color='red'>- Mm-hmm.</font>");
            target.FixHyphens();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<font color='red'>Mm-hmm.</font>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash1()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "- Mm-hmm.");
            target.FixHyphens();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Mm-hmm.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash3()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "- I-I never thought of that.");
            target.FixHyphens();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "I-I never thought of that.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash4()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "- Uh-huh.");
            target.FixHyphens();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Uh-huh.");
        }

        #endregion FixHyphens

        #region Ellipses start

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartNormal1()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("...But that is true.");
            Assert.AreEqual(result, "But that is true.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartNormal2()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("... But that is true.");
            Assert.AreEqual(result, "But that is true.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartNormal3()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("Kurt: ... true but bad.");
            Assert.AreEqual(result, "Kurt: true but bad.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartNormal4()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("Kurt: ... true but bad.");
            Assert.AreEqual(result, "Kurt: true but bad.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartItalic1()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("<i>...But that is true.</i>");
            Assert.AreEqual(result, "<i>But that is true.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartItalic2()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("<i>... But that is true.</i>");
            Assert.AreEqual(result, "<i>But that is true.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartItalic3()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("<i>Kurt: ... true but bad.</i>");
            Assert.AreEqual(result, "<i>Kurt: true but bad.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartItalic4()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("<i>Kurt: ... true but bad.</i>");
            Assert.AreEqual(result, "<i>Kurt: true but bad.</i>");
        }

        #endregion Ellipses start

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUppercaseIInsideWords1()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "This is no troubIe!");
            target.FixUppercaseIInsideWords();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This is no trouble!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUppercaseIInsideWords2()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "- I'll ring her." + Environment.NewLine + "- ...In a lot of trouble.");
            target.FixUppercaseIInsideWords();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- I'll ring her." + Environment.NewLine + "- ...In a lot of trouble.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixOcrErrorsNoChange()
        {
            var target = GetFixCommonErrorsLib();
            InitializeFixCommonErrorsLine(target, "Yeah, see, that's not mine.");
            target.FixOcrErrorsViaReplaceList("eng");
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Yeah, see, that's not mine.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixOcrErrorsViaHardcodedRules1()
        {
            var form = new GoToLine();
            Configuration.Settings.Tools.OcrFixUseHardcodedRules = true;
            //string input = "i'I'll see you.";
            string input = "l-l'll see you.";
            Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine ofe = new Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine("eng", "us_en", form);
            var res = ofe.FixOcrErrorsViaHardcodedRules(input, "Previous line.", new HashSet<string>());
            Assert.AreEqual(res, "I-I'll see you.");
        }

    }
}
