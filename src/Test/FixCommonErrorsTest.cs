﻿using System;
using System.IO;
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
            Stream strm = asm.GetManifestResourceStream("Test.Hunspellx86.dll");
            if (strm != null)
            {
                var rdr = new StreamReader(strm);
                using (Stream file = File.OpenWrite("Hunspellx86.dll"))
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

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsLine1BadEnding()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "</i>What do i care.</i>");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>What do i care.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsLine1BadEndingDouble()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>To be a life-changing weekend</i>" + Environment.NewLine + "<i>for all of us.");
            target.FixInvalidItalicTags();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>To be a life-changing weekend" + Environment.NewLine + "for all of us.</i>");
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
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "The font is italic!"); // will fail if English dictionary is not found
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

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsNoStartWithLargeAfterThreePeriods()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "- I'll ring her." + Environment.NewLine + "- ...in a lot of trouble.");
            target.FixOcrErrorsViaReplaceList("eng");
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- I'll ring her." + Environment.NewLine + "- ...in a lot of trouble.");
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

        #region Start with uppercase after paragraph
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphMusic1()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "♪ you like to move it...");
            target.FixStartWithUppercaseLetterAfterParagraph();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "♪ You like to move it...");
        }

        #endregion

        #region Fix Spanish question- and exclamation-marks
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishNormalQuestion1()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "Cómo estás?");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¿Cómo estás?");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishNormalExclamationMark1()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "Cómo estás!");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¡Cómo estás!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkDouble()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "¡¡PARA!!");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¡¡PARA!!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkTriple()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "¡¡¡PARA!!!");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¡¡¡PARA!!!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkAndQuestionMark()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "¿Cómo estás?!");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¡¿Cómo estás?!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkAndQuestionMarkManyTagsDoubleExcl()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>Chanchita, ¡¿copias?! Chanchita!!</i>");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>Chanchita, ¡¿copias?! ¡¡Chanchita!!</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkAndQuestionMarkOneOfEach()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "¡Cómo estás?");
            target.FixSpanishInvertedQuestionAndExclamationMarks();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "¿¡Cómo estás!?");
        }

        #endregion

        #region FixHyphens

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash1Italic()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>- Mm-hmm.</i>");
            target.FixHyphens();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>Mm-hmm.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash1Font()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<font color='red'>- Mm-hmm.</font>");
            target.FixHyphens();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<font color='red'>Mm-hmm.</font>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash1()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "- Mm-hmm.");
            target.FixHyphens();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Mm-hmm.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash3()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "- I-I never thought of that.");
            target.FixHyphens();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "I-I never thought of that.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash4()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "- Uh-huh.");
            target.FixHyphens();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Uh-huh.");
        }
        #endregion


        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUppercaseIInsideWords1()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "This is no troubIe!");
            target.FixUppercaseIInsideWords();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This is no trouble!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUppercaseIInsideWords2()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "- I'll ring her." + Environment.NewLine + "- ...In a lot of trouble.");
            target.FixUppercaseIInsideWords();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- I'll ring her." + Environment.NewLine + "- ...In a lot of trouble.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixOcrErrorsNoChange()
        {
            var target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "Yeah, see, that's not mine.");
            target.FixOcrErrorsViaReplaceList("eng");
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "Yeah, see, that's not mine.");
        }


        

    }
}
