using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Forms;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using Nikse.SubtitleEdit.Core;

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

        private static FixCommonErrors GetFixCommonErrorsLib()
        {
            return new FixCommonErrors();
        }

        private static void InitializeFixCommonErrorsLine(FixCommonErrors target, string line)
        {
            var subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph(line, 100, 10000));
            target.Initialize(subtitle, new SubRip(), System.Text.Encoding.UTF8);
        }

        private static void InitializeFixCommonErrorsLine(FixCommonErrors target, string line, string line2)
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
            var buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
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
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is" + Environment.NewLine + "short!");
                target.FixShortLines();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "This is short!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesLong()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This I'm pretty sure is not a" + Environment.NewLine + "short line, that should be merged!!!");
                target.FixShortLines();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "This I'm pretty sure is not a" + Environment.NewLine + "short line, that should be merged!!!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesNormalItalic()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>This is" + Environment.NewLine + "short!</i>");
                target.FixShortLines();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>This is short!</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialog()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Hallo!" + Environment.NewLine + "- Hi");
                target.FixShortLines();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "- Hallo!" + Environment.NewLine + "- Hi");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialogItalic()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>- Hallo!" + Environment.NewLine + "- Hi</i>");
                target.FixShortLines();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>- Hallo!" + Environment.NewLine + "- Hi</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialogItalicTwo()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>- Hallo!</i>" + Environment.NewLine + "<i>- Hi<i>");
                target.FixShortLines();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>- Hallo!</i>" + Environment.NewLine + "<i>- Hi<i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDoNotMergeMusicSymbols()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                string source = "♪ La, la, la ♪" + Environment.NewLine + "♪ La, la, la ♪";
                InitializeFixCommonErrorsLine(target, source);
                target.FixShortLines();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, source);
            }
        }


        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void MergeShortLinesHearingImpaired()
        {
            string input = "[engine roaring]" + Environment.NewLine + "[cluck]";
            string input2 = "<i>engine roaring</i>" + Environment.NewLine + "cluck";
            string expected = input;
            string expected2 = input2;

            var result = FixCommonErrorsHelper.FixShortLines(input);
            var result2 = FixCommonErrorsHelper.FixShortLines(input2);
            Assert.AreEqual(result, expected); Assert.AreEqual(result2, expected2.Replace(Environment.NewLine, " "));
        }



        #endregion Merge short lines

        #region Fix Italics

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBeginOnly()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>Hey!" + Environment.NewLine + "<i>Boy!");
                target.FixInvalidItalicTags();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>Hey!</i>" + Environment.NewLine + "<i>Boy!</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsFirstLineEndMissing()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>(jones) seems their attackers headed north." + Environment.NewLine + "<i>Hi!</i>");
                target.FixInvalidItalicTags();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>(jones) seems their attackers headed north." + Environment.NewLine + "Hi!</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsStartInMiddle()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Seems their <i>attackers headed north.");
                target.FixInvalidItalicTags();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "Seems their attackers headed north.");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsEmptyStart()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i></i>test");
                target.FixInvalidItalicTags();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "test");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsSecondLineMissingEnd()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- And..." + Environment.NewLine + "<i>Awesome it is!");
                target.FixInvalidItalicTags();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "- And..." + Environment.NewLine + "<i>Awesome it is!</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Awesome it is!</i>");
                target.FixInvalidItalicTags();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>Awesome it is!</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Awesome it is!<i></i>");
                target.FixInvalidItalicTags();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "Awesome it is!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Awesome it is!<i>");
                target.FixInvalidItalicTags();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "Awesome it is!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsBadEnding4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Awesome it is!</i><i>");
                target.FixInvalidItalicTags();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "Awesome it is!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsLine1BadEnding()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "</i>What do i care.</i>");
                target.FixInvalidItalicTags();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>What do i care.</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixItalicsLine1BadEndingDouble()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>To be a life-changing weekend</i>" + Environment.NewLine + "<i>for all of us.");
                target.FixInvalidItalicTags();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>To be a life-changing weekend" + Environment.NewLine + "for all of us.</i>");
            }
        }

        #endregion Fix Italics

        #region Fix Missing Periods At End Of Line

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingPeriodsAtEndOfLineNone()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is line one!" + Environment.NewLine + "<i>Boy!</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
                target.FixMissingPeriodsAtEndOfLine();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingPeriodsAtEndOfLineItalicAndMissing()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is line one!" + Environment.NewLine + "<i>Boy</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
                target.FixMissingPeriodsAtEndOfLine();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "This is line one!" + Environment.NewLine + "<i>Boy.</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingPeriodsAtEndOfLineItalicAndMissing2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>This is line one!" + Environment.NewLine + "Boy</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
                target.FixMissingPeriodsAtEndOfLine();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>This is line one!" + Environment.NewLine + "Boy.</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingPeriodsAtEndOfLineWithSpace()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "”... and gently down I laid her. ”");
                target.FixMissingPeriodsAtEndOfLine();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "”... and gently down I laid her. ”");
            }
        }

        #endregion Fix Missing Periods At End Of Line

        #region Fix Hyphens (add dash)

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Hi Joe!" + Environment.NewLine + "- Hi Pete!");
                target.FixHyphensAdd();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Hi Joe!" + Environment.NewLine + "Hi Pete!");
                target.FixHyphensAdd();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash2Italic()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>- Hi Joe!" + Environment.NewLine + "Hi Pete!</i>");
                target.FixHyphensAdd();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>- Hi Joe!" + Environment.NewLine + "- Hi Pete!</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash3NoChange()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
                target.FixHyphensAdd();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDash4NoChange()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Hi!" + Environment.NewLine + "- Hi Pete!");
                target.FixHyphensAdd();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "- Hi!" + Environment.NewLine + "- Hi Pete!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixHyphensAddDashButNotInFirst()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Five-Both?" + Environment.NewLine + "- T... T... Ten...");
                target.FixHyphensAdd();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "- Five-Both?" + Environment.NewLine + "- T... T... Ten...");
            }
        }

        #endregion Fix Hyphens (add dash)

        #region Fix OCR errors

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsSlashMakesTwoWords()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "(laughing/clapping)");
                target.FixOcrErrorsViaReplaceList("eng");
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "(laughing/clapping)");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsSlashIsL() // requires hardcoded rules enabled
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "The font is ita/ic!");
                target.FixOcrErrorsViaReplaceList("eng");
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "The font is italic!"); // will fail if English dictionary is not found
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsDashedWords()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "The clock is 12 a.m.");
                target.FixOcrErrorsViaReplaceList("eng");
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "The clock is 12 a.m.");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixCommonOcrErrorsNoStartWithLargeAfterThreePeriods()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- I'll ring her." + Environment.NewLine + "- ...in a lot of trouble.");
                target.FixOcrErrorsViaReplaceList("eng");
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "- I'll ring her." + Environment.NewLine + "- ...in a lot of trouble.");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixOcrErrorsNoChange()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Yeah, see, that's not mine.");
                target.FixOcrErrorsViaReplaceList("eng");
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "Yeah, see, that's not mine.");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixOcrErrorsViaHardcodedRules1()
        {
            using (var form = new GoToLine())
            {
                Configuration.Settings.Tools.OcrFixUseHardcodedRules = true;
                const string input = "l-l'll see you.";
                var ofe = new Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine("eng", "us_en", form);
                var res = ofe.FixOcrErrorsViaHardcodedRules(input, "Previous line.", new HashSet<string>());
                Assert.AreEqual(res, "I-I'll see you.");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixOcrErrorsViaDoNotFixToUpper()
        {
            using (var form = new FixCommonErrors())
            {

                Configuration.Settings.Tools.OcrFixUseHardcodedRules = true;
                const string input = "i.e., your killer.";
                var ofe = new Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine("eng", "not there", form);
                var res = ofe.FixOcrErrors(input, 1, "Ends with comma,", false, Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine.AutoGuessLevel.Cautious);
                Assert.AreEqual(res, "i.e., your killer.");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixOcrErrorsUrl()
        {
            using (var form = new GoToLine())
            {
                Configuration.Settings.Tools.OcrFixUseHardcodedRules = true;
                const string input = "www.addic7ed.com";
                var ofe = new Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine("eng", "us_en", form);
                var res = ofe.FixOcrErrorsViaHardcodedRules(input, "Previous line.", new HashSet<string>());
                Assert.AreEqual(res, input);
            }
        }

        #endregion Fix OCR errors

        #region Fix missing spaces

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesItalicBegin()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "The<i>Bombshell</i> will gone.");
                target.FixMissingSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "The <i>Bombshell</i> will gone.");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesItalicEnd()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "The <i>Bombshell</i>will gone.");
                target.FixMissingSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "The <i>Bombshell</i> will gone.");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesBeforePeriod1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "It will be okay.It surely will be!");
                target.FixMissingSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "It will be okay. It surely will be!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesBeforePeriod2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "you can't get out.Alright?");
                target.FixMissingSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "you can't get out. Alright?");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesNoChange1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "What did Dr. Gey say?");
                target.FixMissingSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "What did Dr. Gey say?");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesChange2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "To be,or not to be!");
                target.FixMissingSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "To be, or not to be!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesNoChange2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to the O.R. now!");
                target.FixMissingSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "Go to the O.R. now!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesNoChange3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Email niksedk@gmail.Com now!");
                target.FixMissingSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "Email niksedk@gmail.Com now!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesNoChange4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to www.nikse.dk for more info");
                target.FixMissingSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "Go to www.nikse.dk for more info");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixMissingSpacesNoChange5Greek()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Aφαίρεσαν ό,τι αντρικό είχες.");
                target.Language = "el"; // Greek
                target.FixMissingSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "Aφαίρεσαν ό,τι αντρικό είχες.");
            }
        }

        #endregion Fix missing spaces

        #region Fix unneeded spaces

        [TestMethod]
        public void FixUnneededSpaces1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "To be , or not to be!");
                target.FixUnneededSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "To be, or not to be!");
            }
        }

        [TestMethod]
        public void FixUnneededSpaces2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, " To be, or not to be!");
                target.FixUnneededSpaces();
                const string expected = "To be, or not to be!";
                Assert.AreEqual(expected, target.Subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixUnneededSpaces3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "To be , or not to be! ");
                target.FixUnneededSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "To be, or not to be!");
            }
        }

        [TestMethod]
        public void FixUnneededSpaces4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "To be , or not to be! " + Environment.NewLine + " Line two.");
                target.FixUnneededSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "To be, or not to be!" + Environment.NewLine + "Line two.");
            }
        }

        [TestMethod]
        public void FixUnneededSpaces5()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string expected = "\"Foo\" bar.";
                InitializeFixCommonErrorsLine(target, "\"Foo \" bar.", "\" Foo \" bar.");
                target.FixUnneededSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, expected);
                Assert.AreEqual(target.Subtitle.Paragraphs[1].Text, expected);
            }
        }

        [TestMethod]
        public void FixUnneededSpaces6()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string expected = "Foo bar.";
                InitializeFixCommonErrorsLine(target, "Foo \t\tbar.");
                target.FixUnneededSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, expected);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesItalic1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string expected = "Hi <i>bad</i> man!";
                InitializeFixCommonErrorsLine(target, "Hi <i> bad</i> man!");
                target.FixUnneededSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, expected);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesItalic2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string expected = "Hi <i>bad</i> man!";
                InitializeFixCommonErrorsLine(target, "Hi <i>bad </i> man!");
                target.FixUnneededSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, expected);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesFont1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string expected = "Hi <font color='red'>bad</font> man!";
                InitializeFixCommonErrorsLine(target, "Hi <font color='red'> bad</font> man!");
                target.FixUnneededSpaces();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, expected);
            }
        }

        #endregion Fix unneeded spaces

        #region Fix EmptyLines

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEmptyLinesTest1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>\r\nHello world!\r\n</i>");
                target.FixEmptyLines();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>Hello world!</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEmptyLinesTest2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<font color=\"#000000\">\r\nHello world!\r\n</font>");
                target.FixEmptyLines();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<font color=\"#000000\">Hello world!</font>");
            }
        }


        #endregion

        #region Start with uppercase after paragraph

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphMusic1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "♪ you like to move it...");
                target.FixStartWithUppercaseLetterAfterParagraph();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "♪ You like to move it...");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormal1()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("bye.", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "Bye.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormal2()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("<i>bye.</i>", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "<i>Bye.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormal3()
        {
            var prev = new Paragraph("<i>Bye.</i>", 0, 1000);
            var p = new Paragraph("<i>bye.</i>", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "<i>Bye.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormal4()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("bye." + Environment.NewLine + "bye.", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "Bye." + Environment.NewLine + "Bye.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalNoChange1()
        {
            var prev = new Paragraph("Bye,", 0, 1000);
            var p = new Paragraph("bye.", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "bye.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialog1()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("- Moss! Jesus Christ!" + Environment.NewLine + "- what is it?", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "- Moss! Jesus Christ!" + Environment.NewLine + "- What is it?");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialog2()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("<i>- Moss! Jesus Christ!" + Environment.NewLine + "- what is it?</i>", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "<i>- Moss! Jesus Christ!" + Environment.NewLine + "- What is it?</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialog3()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("<i>- Moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- what is it?</i>", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "<i>- Moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- What is it?</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialog4()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("<i>- moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- what is it?</i>", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "<i>- Moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- What is it?</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalNoChange2()
        {
            var prev = new Paragraph("Bye", 0, 1000);
            var p = new Paragraph("bye.", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "bye.");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialogNoChange1()
        {
            var prev = new Paragraph("Bye -", 0, 1000);
            var p = new Paragraph("- moss!", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "- moss!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void StartWithUppercaseAfterParagraphNormalDialogNoChange2()
        {
            var prev = new Paragraph("Bye -", 0, 1000);
            var p = new Paragraph("- moss!" + Environment.NewLine + " - Bye.", 1200, 5000);
            var fixedText = FixCommonErrors.FixStartWithUppercaseLetterAfterParagraph(p, prev, System.Text.Encoding.UTF8, "en");
            Assert.AreEqual(fixedText, "- moss!" + Environment.NewLine + " - Bye.");
        }

        #endregion Start with uppercase after paragraph

        #region Fix Spanish question and exclamation marks

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishNormalQuestion1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Cómo estás?");
                target.FixSpanishInvertedQuestionAndExclamationMarks();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "¿Cómo estás?");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishNormalExclamationMark1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Cómo estás!");
                target.FixSpanishInvertedQuestionAndExclamationMarks();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "¡Cómo estás!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkDouble()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "¡¡PARA!!");
                target.FixSpanishInvertedQuestionAndExclamationMarks();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "¡¡PARA!!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkTriple()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "¡¡¡PARA!!!");
                target.FixSpanishInvertedQuestionAndExclamationMarks();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "¡¡¡PARA!!!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkAndQuestionMark()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "¿Cómo estás?!");
                target.FixSpanishInvertedQuestionAndExclamationMarks();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "¡¿Cómo estás?!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkAndQuestionMarkManyTagsDoubleExcl()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>Chanchita, ¡¿copias?! Chanchita!!</i>");
                target.FixSpanishInvertedQuestionAndExclamationMarks();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>Chanchita, ¡¿copias?! ¡¡Chanchita!!</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSpanishExclamationMarkAndQuestionMarkOneOfEach()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "¡Cómo estás?");
                target.FixSpanishInvertedQuestionAndExclamationMarks();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "¿¡Cómo estás!?");
            }
        }

        #endregion Fix Spanish question and exclamation marks

        #region FixHyphens (remove dash)

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash1Italic()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>- Mm-hmm.</i>");
                target.FixHyphensRemove();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>Mm-hmm.</i>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash1Font()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<font color='red'>- Mm-hmm.</font>");
                target.FixHyphensRemove();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<font color='red'>Mm-hmm.</font>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Mm-hmm.");
                target.FixHyphensRemove();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "Mm-hmm.");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- I-I never thought of that.");
                target.FixHyphensRemove();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "I-I never thought of that.");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixSingleLineDash4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Uh-huh.");
                target.FixHyphensRemove();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "Uh-huh.");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixDashWithPreviousEndsWithDashDash()
        {
            var subtitle = new Subtitle();
            const string t1 = "Hey--";
            string t2 = "- oh, no, no, no, you're gonna" + Environment.NewLine + "need to add the mattress,";
            subtitle.Paragraphs.Add(new Paragraph(t1, 0, 1000));
            subtitle.Paragraphs.Add(new Paragraph(t2, 1000, 4000));
            {
                var result = FixCommonErrorsHelper.FixHyphensRemove(subtitle, 1);
                string target = "oh, no, no, no, you're gonna" + Environment.NewLine + "need to add the mattress,";
                Assert.AreEqual(target, result);
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixDashDontRemoveWhiteSpaceWithItalic()
        {
            var subtitle = new Subtitle();
            const string t1 = "Hey!";
            const string t2 = "- PREVIOUSLY ON<I> HAVEN...</I>";
            subtitle.Paragraphs.Add(new Paragraph(t1, 0, 1000));
            subtitle.Paragraphs.Add(new Paragraph(t2, 1000, 4000));
            {
                var result = FixCommonErrorsHelper.FixHyphensRemove(subtitle, 1);
                const string target = "PREVIOUSLY ON<I> HAVEN...</I>";
                Assert.AreEqual(target, result);
            }
        }

        #endregion FixHyphens (remove dash)

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

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartItalic5()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("WOMAN 2: <i>...24 hours a day at BabyC.</i>");
            Assert.AreEqual(result, "WOMAN 2: <i>24 hours a day at BabyC.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartFont1()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("<font color=\"#000000\">... true but bad.</font>");
            Assert.AreEqual(result, "<font color=\"#000000\">true but bad.</font>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartFont2()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("<font color=\"#000000\"><i>Kurt: ... true but bad.</i></font>");
            Assert.AreEqual(result, "<font color=\"#000000\"><i>Kurt: true but bad.</i></font>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartFont3()
        {
            var result = FixCommonErrorsHelper.FixEllipsesStartHelper("<i><font color=\"#000000\">Kurt: ...true but bad.</font></i>");
            Assert.AreEqual(result, "<i><font color=\"#000000\">Kurt: true but bad.</font></i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartQuote()
        {
            var actual = "\"...Foobar\"";
            const string expected = "\"Foobar\"";
            actual = FixCommonErrorsHelper.FixEllipsesStartHelper(actual);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartQuote2()
        {
            var actual = "\"... Foobar\"";
            const string expected = "\"Foobar\"";
            actual = FixCommonErrorsHelper.FixEllipsesStartHelper(actual);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartQuote3()
        {
            var actual = "\" . . . Foobar\"";
            const string expected = "\"Foobar\"";
            actual = FixCommonErrorsHelper.FixEllipsesStartHelper(actual);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixEllipsesStartDontChange()
        {
            const string input = "- I...";
            string actual = FixCommonErrorsHelper.FixEllipsesStartHelper(input);
            Assert.AreEqual(actual, input);
        }

        #endregion Ellipses start

        #region FixDoubleGreater

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixDoubleGreaterThanTest2()
        {
            const string input1 = "<i>>>Hello world!</i>\r\n<i>>>Hello</i>";
            const string input2 = "<b>>>Hello world!</b>\r\n<i>>>Hello</i>";
            const string input3 = "<u>>>Hello world!</u>\r\n<b>>>Hello</b>";
            const string input4 = "<font color=\"#008040\">>>Hello world!</font>\r\n<font color=\"#008040\">>>Hello</font>";

            const string expected1 = "<i>Hello world!</i>\r\n<i>Hello</i>";
            const string expected2 = "<b>Hello world!</b>\r\n<i>Hello</i>";
            const string expected3 = "<u>Hello world!</u>\r\n<b>Hello</b>";
            const string expected4 = "<font color=\"#008040\">Hello world!</font>\r\n<font color=\"#008040\">Hello</font>";

            var lines1 = input1.SplitToLines();
            var lines2 = input2.SplitToLines();
            var lines3 = input3.SplitToLines();
            var lines4 = input4.SplitToLines();

            for (int i = 0; i < lines1.Length; i++)
            {
                lines1[i] = FixCommonErrorsHelper.FixDoubleGreaterThanHelper(lines1[i]);
                lines2[i] = FixCommonErrorsHelper.FixDoubleGreaterThanHelper(lines2[i]);
                lines3[i] = FixCommonErrorsHelper.FixDoubleGreaterThanHelper(lines3[i]);
                lines4[i] = FixCommonErrorsHelper.FixDoubleGreaterThanHelper(lines4[i]);
            }

            var result1 = string.Join(Environment.NewLine, lines1);
            var result2 = string.Join(Environment.NewLine, lines2);
            var result3 = string.Join(Environment.NewLine, lines3);
            var result4 = string.Join(Environment.NewLine, lines4);

            Assert.AreEqual(result1, expected1);
            Assert.AreEqual(result2, expected2);
            Assert.AreEqual(result3, expected3);
            Assert.AreEqual(result4, expected4);
        }

        #endregion

        #region Fix uppercase I inside words

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUppercaseIInsideWords1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is no troubIe!");
                target.FixUppercaseIInsideWords();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "This is no trouble!");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUppercaseIInsideWords2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- I'll ring her." + Environment.NewLine + "- ...In a lot of trouble.");
                target.FixUppercaseIInsideWords();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "- I'll ring her." + Environment.NewLine + "- ...In a lot of trouble.");
            }
        }

        #endregion Fix uppercase I inside words

        #region Fix dialogs on one line

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixDialogsOnOneLine1()
        {
            const string source = "- I was here, putting our child to sleep-- - Emma.";
            string target = "- I was here, putting our child to sleep--" + Environment.NewLine + "- Emma.";
            string result = FixCommonErrorsHelper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixDialogsOnOneLine2()
        {
            const string source = "- Seriously, though. Are you being bullied? - Nope.";
            string target = "- Seriously, though. Are you being bullied?" + Environment.NewLine + "- Nope.";
            string result = FixCommonErrorsHelper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixDialogsOnOneLine3()
        {
            string source = "- Having sexual relationships" + Environment.NewLine + "with other women. - A'ight.";
            string target = "- Having sexual relationships with other women." + Environment.NewLine + "- A'ight.";
            string result = FixCommonErrorsHelper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }

        #endregion Fix dialogs on one line

        #region FixDoubleDash

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixDoubleDashTest1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                // <font color="#000000"> and <font>
                InitializeFixCommonErrorsLine(target, "<font color=\"#000000\">-- Mm-hmm.</font>");
                target.FixDoubleDash();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<font color=\"#000000\">...Mm-hmm.</font>");
            }
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixDoubleDashTest2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                // <font color="#000000"> and <font>
                InitializeFixCommonErrorsLine(target, "<b>Mm-hmm.</b>\r\n<font color=\"#000000\">-- foobar</font>");
                target.FixDoubleDash();
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<b>Mm-hmm.</b>\r\n<font color=\"#000000\">...foobar</font>");
            }
        }

        #endregion
    }
}
