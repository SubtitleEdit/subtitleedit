using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;
using System;
using System.Collections.Generic;
using System.IO;

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

        private Subtitle _subtitle;

        private static FixCommonErrors GetFixCommonErrorsLib()
        {
            return new FixCommonErrors();
        }

        private void InitializeFixCommonErrorsLine(FixCommonErrors target, string line)
        {
            _subtitle = new Subtitle();
            _subtitle.Paragraphs.Add(new Paragraph(line, 100, 10000));
            target.Initialize(_subtitle, new SubRip(), System.Text.Encoding.UTF8);
        }

        private void InitializeFixCommonErrorsLine(FixCommonErrors target, string line, string line2)
        {
            _subtitle = new Subtitle();
            _subtitle.Paragraphs.Add(new Paragraph(line, 100, 10000));
            _subtitle.Paragraphs.Add(new Paragraph(line2, 10001, 30000));
            target.Initialize(_subtitle, new SubRip(), System.Text.Encoding.UTF8);
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

            strm = asm.GetManifestResourceStream("Test.Dictionaries.names.xml");
            if (strm != null)
            {
                using (Stream file = File.OpenWrite(Path.Combine("Dictionaries", "names.xml")))
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
        public void FixShortLinesNormal()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is" + Environment.NewLine + "short!");
                new FixShortLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "This is short!");
            }
        }

        [TestMethod]
        public void FixShortLinesLong()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This I'm pretty sure is not a" + Environment.NewLine + "short line, that should be merged!!!");
                new FixShortLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "This I'm pretty sure is not a" + Environment.NewLine + "short line, that should be merged!!!");
            }
        }

        [TestMethod]
        public void FixShortLinesNormalItalic()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>This is" + Environment.NewLine + "short!</i>");
                new FixShortLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>This is short!</i>");
            }
        }

        [TestMethod]
        public void FixShortLinesDialog()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Hallo!" + Environment.NewLine + "- Hi");
                new FixShortLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "- Hallo!" + Environment.NewLine + "- Hi");
            }
        }

        [TestMethod]
        public void FixShortLinesDialogItalic()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>- Hallo!" + Environment.NewLine + "- Hi</i>");
                new FixShortLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>- Hallo!" + Environment.NewLine + "- Hi</i>");
            }
        }

        [TestMethod]
        public void FixShortLinesDialogItalicTwo()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>- Hallo!</i>" + Environment.NewLine + "<i>- Hi<i>");
                new FixShortLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<i>- Hallo!</i>" + Environment.NewLine + "<i>- Hi<i>");
            }
        }

        [TestMethod]
        public void FixShortLinesDoNotMergeMusicSymbols()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                string source = "♪ La, la, la ♪" + Environment.NewLine + "♪ La, la, la ♪";
                InitializeFixCommonErrorsLine(target, source);
                new FixShortLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, source);
            }
        }

        [TestMethod]
        public void MergeShortLinesHearingImpaired()
        {
            string input = "[engine roaring]" + Environment.NewLine + "[cluck]";
            string input2 = "<i>engine roaring</i>" + Environment.NewLine + "cluck";
            string expected = input;
            string expected2 = input2;

            var result = Helper.FixShortLines(input);
            var result2 = Helper.FixShortLines(input2);
            Assert.AreEqual(result, expected); Assert.AreEqual(result2, expected2.Replace(Environment.NewLine, " "));
        }

        #endregion Merge short lines

        #region Fix Italics

        [TestMethod]
        public void FixItalicsBeginOnly()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>Hey!" + Environment.NewLine + "<i>Boy!");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>Hey!</i>" + Environment.NewLine + "<i>Boy!</i>");
            }
        }

        [TestMethod]
        public void FixItalicsFirstLineEndMissing()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>(jones) seems their attackers headed north." + Environment.NewLine + "<i>Hi!</i>");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>(jones) seems their attackers headed north." + Environment.NewLine + "Hi!</i>");
            }
        }

        [TestMethod]
        public void FixItalicsStartInMiddle()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Seems their <i>attackers headed north.");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Seems their attackers headed north.");
            }
        }

        [TestMethod]
        public void FixItalicsEmptyStart()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i></i>test");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "test");
            }
        }

        [TestMethod]
        public void FixItalicsSecondLineMissingEnd()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- And..." + Environment.NewLine + "<i>Awesome it is!");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "- And..." + Environment.NewLine + "<i>Awesome it is!</i>");
            }
        }

        [TestMethod]
        public void FixItalicsBadEnding()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Awesome it is!</i>");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>Awesome it is!</i>");
            }
        }

        [TestMethod]
        public void FixItalicsBadEnding2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Awesome it is!<i></i>");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Awesome it is!");
            }
        }

        [TestMethod]
        public void FixItalicsBadEnding3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Awesome it is!<i>");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Awesome it is!");
            }
        }

        [TestMethod]
        public void FixItalicsBadEnding4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Awesome it is!</i><i>");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Awesome it is!");
            }
        }

        [TestMethod]
        public void FixItalicsLine1BadEnding()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "</i>What do i care.</i>");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>What do i care.</i>");
            }
        }

        [TestMethod]
        public void FixItalicsLine1BadEndingDouble()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>To be a life-changing weekend</i>" + Environment.NewLine + "<i>for all of us.");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>To be a life-changing weekend" + Environment.NewLine + "for all of us.</i>");
            }
        }

        #endregion Fix Italics

        #region Fix Missing Periods At End Of Line

        [TestMethod]
        public void FixMissingPeriodsAtEndOfLineNone()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is line one!" + Environment.NewLine + "<i>Boy!</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
                new FixMissingPeriodsAtEndOfLine().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
            }
        }

        [TestMethod]
        public void FixMissingPeriodsAtEndOfLineItalicAndMissing()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is line one!" + Environment.NewLine + "<i>Boy</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
                new FixMissingPeriodsAtEndOfLine().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "This is line one!" + Environment.NewLine + "<i>Boy.</i>");
            }
        }

        [TestMethod]
        public void FixMissingPeriodsAtEndOfLineItalicAndMissing2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>This is line one!" + Environment.NewLine + "Boy</i>", "This is line one!" + Environment.NewLine + "<i>Boy!</i>");
                new FixMissingPeriodsAtEndOfLine().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>This is line one!" + Environment.NewLine + "Boy.</i>");
            }
        }

        [TestMethod]
        public void FixMissingPeriodsAtEndOfLineWithSpace()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "”... and gently down I laid her. ”");
                new FixMissingPeriodsAtEndOfLine().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "”... and gently down I laid her. ”");
            }
        }

        #endregion Fix Missing Periods At End Of Line

        #region Fix Hyphens (add dash)

        [TestMethod]
        public void FixHyphensAddDash1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Hi Joe!" + Environment.NewLine + "- Hi Pete!");
                new FixHyphensAdd().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
            }
        }

        [TestMethod]
        public void FixHyphensAddDash2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Hi Joe!" + Environment.NewLine + "Hi Pete!");
                new FixHyphensAdd().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
            }
        }

        [TestMethod]
        public void FixHyphensAddDash2Italic()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>- Hi Joe!" + Environment.NewLine + "Hi Pete!</i>");
                new FixHyphensAdd().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>- Hi Joe!" + Environment.NewLine + "- Hi Pete!</i>");
            }
        }

        [TestMethod]
        public void FixHyphensAddDash3NoChange()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
                new FixHyphensAdd().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "- Hi Joe!" + Environment.NewLine + "- Hi Pete!");
            }
        }

        [TestMethod]
        public void FixHyphensAddDash4NoChange()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Hi!" + Environment.NewLine + "- Hi Pete!");
                new FixHyphensAdd().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "- Hi!" + Environment.NewLine + "- Hi Pete!");
            }
        }

        [TestMethod]
        public void FixHyphensAddDashButNotInFirst()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Five-Both?" + Environment.NewLine + "- T... T... Ten...");
                new FixHyphensAdd().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "- Five-Both?" + Environment.NewLine + "- T... T... Ten...");
            }
        }

        [TestMethod]
        public void FixHyphensDontCrash()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                string input = "<i>" + Environment.NewLine + "- So far we don't know</i> <i>much about anything.</i>";
                InitializeFixCommonErrorsLine(target, input);
                new FixHyphensAdd().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, input);
            }
        }

        #endregion Fix Hyphens (add dash)

        #region Fix OCR errors

        [TestMethod]
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
        public void FixOcrErrorsViaHardcodedRules2()
        {
            using (var form = new GoToLine())
            {
                Configuration.Settings.Tools.OcrFixUseHardcodedRules = true;
                const string input = "Foobar\r\n<i>-";
                var ofe = new Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine("eng", "us_en", form);
                var res = ofe.FixOcrErrorsViaHardcodedRules(input, "Previous line.", new HashSet<string>());
                Assert.AreEqual(res, "Foobar\r\n<i>-");
            }
        }

        [TestMethod]
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
        public void FixMissingSpacesItalicBegin()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "The<i>Bombshell</i> will gone.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "The <i>Bombshell</i> will gone.");
            }
        }

        [TestMethod]
        public void FixMissingSpacesItalicEnd()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "The <i>Bombshell</i>will gone.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "The <i>Bombshell</i> will gone.");
            }
        }

        [TestMethod]
        public void FixMissingSpacesBeforePeriod1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "It will be okay.It surely will be!");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "It will be okay. It surely will be!");
            }
        }

        [TestMethod]
        public void FixMissingSpacesBeforePeriod2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "you can't get out.Alright?");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "you can't get out. Alright?");
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "What did Dr. Gey say?");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "What did Dr. Gey say?");
            }
        }

        [TestMethod]
        public void FixMissingSpacesChange2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "To be,or not to be!");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "To be, or not to be!");
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to the O.R. now!");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Go to the O.R. now!");
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Email niksedk@gmail.Com now!");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Email niksedk@gmail.Com now!");
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to www.nikse.dk for more info");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Go to www.nikse.dk for more info");
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange5Greek()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Aφαίρεσαν ό,τι αντρικό είχες.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "el" });  // Greek
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Aφαίρεσαν ό,τι αντρικό είχες.");
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange6()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to nikse.com for more info");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Go to nikse.com for more info");
            }
        }
        [TestMethod]
        public void FixMissingSpacesNoChange7()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to nikse.net for more info");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Go to nikse.net for more info");
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange8()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to nikse.org for more info");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Go to nikse.org for more info");
            }
        }

        [TestMethod]
        public void FixMissingSpacesOneLetterPlusDotDotDot()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "I...want missing spaces.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "en" });
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "I... want missing spaces.");
            }
        }

        [TestMethod]
        public void FixMissingSwedish()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "VD:n tycker det är bra.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "sv" });
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "VD:n tycker det är bra.");
            }
        }

        [TestMethod]
        public void FixMissingSwedish2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "MARCUS:tycker det är bra.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "sv" });
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "MARCUS: tycker det är bra.");
            }
        }

        [TestMethod]
        public void FixMissingFinnish()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "FBI:n");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "fi" });
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "FBI:n");
            }
        }

        [TestMethod]
        public void FixMissingFinnish2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "MARCUS:tycker det är bra.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "fi" });
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "MARCUS: tycker det är bra.");
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
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "To be, or not to be!");
            }
        }

        [TestMethod]
        public void FixUnneededSpaces2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, " To be, or not to be!");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                const string expected = "To be, or not to be!";
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixUnneededSpaces3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "To be , or not to be! ");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "To be, or not to be!");
            }
        }

        [TestMethod]
        public void FixUnneededSpaces4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "To be , or not to be! " + Environment.NewLine + " Line two.");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "To be, or not to be!" + Environment.NewLine + "Line two.");
            }
        }

        [TestMethod]
        public void FixUnneededSpaces5()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string expected = "\"Foo\" bar.";
                InitializeFixCommonErrorsLine(target, "\"Foo \" bar.", "\" Foo \" bar.");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, expected);
                Assert.AreEqual(_subtitle.Paragraphs[1].Text, expected);
            }
        }

        [TestMethod]
        public void FixUnneededSpaces6()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string expected = "Foo bar.";
                InitializeFixCommonErrorsLine(target, "Foo \t\tbar.");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, expected);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesItalic1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string expected = "Hi <i>bad</i> man!";
                InitializeFixCommonErrorsLine(target, "Hi <i> bad</i> man!");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, expected);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesItalic2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string expected = "Hi <i>bad</i> man!";
                InitializeFixCommonErrorsLine(target, "Hi <i>bad </i> man!");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, expected);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesFont1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string expected = "Hi <font color='red'>bad</font> man!";
                InitializeFixCommonErrorsLine(target, "Hi <font color='red'> bad</font> man!");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, expected);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesNewLineAfterItalicAtStart()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                string input = "<i>" + Environment.NewLine + "- So far we don't know much about anything.</i>";
                const string expected = "<i>- So far we don't know much about anything.</i>";
                InitializeFixCommonErrorsLine(target, input);
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, expected);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesNewLineBeforeEndItalicAtEnd()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                string input = "<i>- So far we don't</i> <i>know much about anything." + Environment.NewLine + "</i>";
                const string expected = "<i>- So far we don't</i> <i>know much about anything.</i>";
                InitializeFixCommonErrorsLine(target, input);
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, expected);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesBeforePeriod()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string input = "He's English . He is 21 years of age.";
                const string expected = "He's English. He is 21 years of age.";
                InitializeFixCommonErrorsLine(target, input);
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, expected);
            }
        }

        #endregion Fix unneeded spaces

        #region Fix EmptyLines

        [TestMethod]
        public void FixEmptyLinesTest1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>\r\nHello world!\r\n</i>");
                new FixEmptyLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>Hello world!</i>");
            }
        }

        [TestMethod]
        public void FixEmptyLinesTest2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<font color=\"#000000\">\r\nHello world!\r\n</font>");
                new FixEmptyLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<font color=\"#000000\">Hello world!</font>");
            }
        }

        #endregion Fix EmptyLines

        #region Fix missing periods at end of line

        [TestMethod]
        public void AddPeriodWhereNextLineStartsWithUppercaseLetter()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("The house seemed desolate to me", 0, 1000));
            s.Paragraphs.Add(new Paragraph("I wasn't sure if somebody lived in there...", 3200, 5000));
            new FixMissingPeriodsAtEndOfLine().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(s.Paragraphs[0].Text, "The house seemed desolate to me.");
        }

        [TestMethod]
        public void AddPeriodWhereNextLineStartsWithUppercaseLetter2()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("The house seemed desolate to me and", 0, 1000));
            s.Paragraphs.Add(new Paragraph("I wasn't sure if somebody lived in there...", 1000, 3000));
            new FixMissingPeriodsAtEndOfLine().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(s.Paragraphs[0].Text, "The house seemed desolate to me and");
        }

        #endregion Fix missing periods at end of line

        #region Start with uppercase after paragraph

        [TestMethod]
        public void StartWithUppercaseAfterParagraphMusic1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "♪ you like to move it...");
                new FixStartWithUppercaseLetterAfterParagraph().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "♪ You like to move it...");
            }
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormal1()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("Bye.", 0, 1000));
            s.Paragraphs.Add(new Paragraph("bye.", 1200, 5000));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(s.Paragraphs[1].Text, "Bye.");
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormal2()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("Bye.", 0, 1000));
            s.Paragraphs.Add(new Paragraph("<i>bye.</i>", 1200, 5000));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(s.Paragraphs[1].Text, "<i>Bye.</i>");
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormal3()
        {
            var prev = new Paragraph("<i>Bye.</i>", 0, 1000);
            var p = new Paragraph("<i>bye.</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add((prev));
            s.Paragraphs.Add((p));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(p.Text, "<i>Bye.</i>");
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormal4()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("bye." + Environment.NewLine + "bye.", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add((prev));
            s.Paragraphs.Add((p));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(p.Text, "Bye." + Environment.NewLine + "Bye.");
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormalNoChange1()
        {
            var prev = new Paragraph("Bye,", 0, 1000);
            var p = new Paragraph("bye.", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add((prev));
            s.Paragraphs.Add((p));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(p.Text, "bye.");
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormalDialog1()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("- Moss! Jesus Christ!" + Environment.NewLine + "- what is it?", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add((prev));
            s.Paragraphs.Add((p));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(p.Text, "- Moss! Jesus Christ!" + Environment.NewLine + "- What is it?");
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormalDialog2()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("<i>- Moss! Jesus Christ!" + Environment.NewLine + "- what is it?</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add((prev));
            s.Paragraphs.Add((p));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(p.Text, "<i>- Moss! Jesus Christ!" + Environment.NewLine + "- What is it?</i>");
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormalDialog3()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("<i>- Moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- what is it?</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add((prev));
            s.Paragraphs.Add((p));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(p.Text, "<i>- Moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- What is it?</i>");
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormalDialog4()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("<i>- moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- what is it?</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add((prev));
            s.Paragraphs.Add((p));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(p.Text, "<i>- Moss! Jesus Christ!</i>" + Environment.NewLine + "<i>- What is it?</i>");
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormalNoChange2()
        {
            var prev = new Paragraph("Bye", 0, 1000);
            var p = new Paragraph("bye.", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add((prev));
            s.Paragraphs.Add((p));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(p.Text, "bye.");
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormalDialogNoChange1()
        {
            var prev = new Paragraph("Bye -", 0, 1000);
            var p = new Paragraph("- moss!", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add((prev));
            s.Paragraphs.Add((p));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(p.Text, "- moss!");
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormalDialogNoChange2()
        {
            var prev = new Paragraph("Bye -", 0, 1000);
            var p = new Paragraph("- moss!" + Environment.NewLine + " - Bye.", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add((prev));
            s.Paragraphs.Add((p));
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(p.Text, "- moss!" + Environment.NewLine + " - Bye.");
        }

        #endregion Start with uppercase after paragraph

        #region Fix Spanish question and exclamation marks

        [TestMethod]
        public void FixSpanishNormalQuestion1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Cómo estás?");
                new FixSpanishInvertedQuestionAndExclamationMarks().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "¿Cómo estás?");
            }
        }

        [TestMethod]
        public void FixSpanishNormalExclamationMark1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Cómo estás!");
                new FixSpanishInvertedQuestionAndExclamationMarks().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "¡Cómo estás!");
            }
        }

        [TestMethod]
        public void FixSpanishExclamationMarkDouble()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "¡¡PARA!!");
                new FixSpanishInvertedQuestionAndExclamationMarks().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "¡¡PARA!!");
            }
        }

        [TestMethod]
        public void FixSpanishExclamationMarkTriple()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "¡¡¡PARA!!!");
                new FixSpanishInvertedQuestionAndExclamationMarks().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "¡¡¡PARA!!!");
            }
        }

        [TestMethod]
        public void FixSpanishExclamationMarkAndQuestionMark()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "¿Cómo estás?!");
                new FixSpanishInvertedQuestionAndExclamationMarks().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "¡¿Cómo estás?!");
            }
        }

        [TestMethod]
        public void FixSpanishExclamationMarkAndQuestionMarkManyTagsDoubleExcl()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>Chanchita, ¡¿copias?! Chanchita!!</i>");
                new FixSpanishInvertedQuestionAndExclamationMarks().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>Chanchita, ¡¿copias?! ¡¡Chanchita!!</i>");
            }
        }

        [TestMethod]
        public void FixSpanishExclamationMarkAndQuestionMarkOneOfEach()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "¡Cómo estás?");
                new FixSpanishInvertedQuestionAndExclamationMarks().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "¿¡Cómo estás!?");
            }
        }

        #endregion Fix Spanish question and exclamation marks

        #region FixHyphens (remove dash)

        [TestMethod]
        public void FixSingleLineDash1Italic()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>- Mm-hmm.</i>");
                new FixHyphensRemove().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>Mm-hmm.</i>");
            }
        }

        [TestMethod]
        public void FixSingleLineDash1Font()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<font color='red'>- Mm-hmm.</font>");
                new FixHyphensRemove().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<font color='red'>Mm-hmm.</font>");
            }
        }

        [TestMethod]
        public void FixSingleLineDash1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Mm-hmm.");
                new FixHyphensRemove().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Mm-hmm.");
            }
        }

        [TestMethod]
        public void FixSingleLineDash3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- I-I never thought of that.");
                new FixHyphensRemove().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "I-I never thought of that.");
            }
        }

        [TestMethod]
        public void FixSingleLineDash4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Uh-huh.");
                new FixHyphensRemove().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Uh-huh.");
            }
        }

        [TestMethod]
        public void FixDashWithPreviousEndsWithDashDash()
        {
            var subtitle = new Subtitle();
            const string t1 = "Hey--";
            string t2 = "- oh, no, no, no, you're gonna" + Environment.NewLine + "need to add the mattress,";
            subtitle.Paragraphs.Add(new Paragraph(t1, 0, 1000));
            subtitle.Paragraphs.Add(new Paragraph(t2, 1000, 4000));
            {
                var result = Helper.FixHyphensRemove(subtitle, 1);
                string target = "oh, no, no, no, you're gonna" + Environment.NewLine + "need to add the mattress,";
                Assert.AreEqual(target, result);
            }
        }

        [TestMethod]
        public void FixDashDontRemoveWhiteSpaceWithItalic()
        {
            var subtitle = new Subtitle();
            const string t1 = "Hey!";
            const string t2 = "- PREVIOUSLY ON<I> HAVEN...</I>";
            subtitle.Paragraphs.Add(new Paragraph(t1, 0, 1000));
            subtitle.Paragraphs.Add(new Paragraph(t2, 1000, 4000));
            {
                var result = Helper.FixHyphensRemove(subtitle, 1);
                const string target = "PREVIOUSLY ON<I> HAVEN...</I>";
                Assert.AreEqual(target, result);
            }
        }

        #endregion FixHyphens (remove dash)

        #region Ellipses start

        [TestMethod]
        public void FixEllipsesStartNormal1()
        {
            var result = Helper.FixEllipsesStartHelper("...But that is true.");
            Assert.AreEqual(result, "But that is true.");
        }

        [TestMethod]
        public void FixEllipsesStartNormal2()
        {
            var result = Helper.FixEllipsesStartHelper("... But that is true.");
            Assert.AreEqual(result, "But that is true.");
        }

        [TestMethod]
        public void FixEllipsesStartNormal3()
        {
            var result = Helper.FixEllipsesStartHelper("Kurt: ... true but bad.");
            Assert.AreEqual(result, "Kurt: true but bad.");
        }

        [TestMethod]
        public void FixEllipsesStartNormal4()
        {
            var result = Helper.FixEllipsesStartHelper("Kurt: ... true but bad.");
            Assert.AreEqual(result, "Kurt: true but bad.");
        }

        [TestMethod]
        public void FixEllipsesStartItalic1()
        {
            var result = Helper.FixEllipsesStartHelper("<i>...But that is true.</i>");
            Assert.AreEqual(result, "<i>But that is true.</i>");
        }

        [TestMethod]
        public void FixEllipsesStartItalic2()
        {
            var result = Helper.FixEllipsesStartHelper("<i>... But that is true.</i>");
            Assert.AreEqual(result, "<i>But that is true.</i>");
        }

        [TestMethod]
        public void FixEllipsesStartItalic3()
        {
            var result = Helper.FixEllipsesStartHelper("<i>Kurt: ... true but bad.</i>");
            Assert.AreEqual(result, "<i>Kurt: true but bad.</i>");
        }

        [TestMethod]
        public void FixEllipsesStartItalic4()
        {
            var result = Helper.FixEllipsesStartHelper("<i>Kurt: ... true but bad.</i>");
            Assert.AreEqual(result, "<i>Kurt: true but bad.</i>");
        }

        [TestMethod]
        public void FixEllipsesStartItalic5()
        {
            var result = Helper.FixEllipsesStartHelper("WOMAN 2: <i>...24 hours a day at BabyC.</i>");
            Assert.AreEqual(result, "WOMAN 2: <i>24 hours a day at BabyC.</i>");
        }

        [TestMethod]
        public void FixEllipsesStartFont1()
        {
            var result = Helper.FixEllipsesStartHelper("<font color=\"#000000\">... true but bad.</font>");
            Assert.AreEqual(result, "<font color=\"#000000\">true but bad.</font>");
        }

        [TestMethod]
        public void FixEllipsesStartFont2()
        {
            var result = Helper.FixEllipsesStartHelper("<font color=\"#000000\"><i>Kurt: ... true but bad.</i></font>");
            Assert.AreEqual(result, "<font color=\"#000000\"><i>Kurt: true but bad.</i></font>");
        }

        [TestMethod]
        public void FixEllipsesStartFont3()
        {
            var result = Helper.FixEllipsesStartHelper("<i><font color=\"#000000\">Kurt: ...true but bad.</font></i>");
            Assert.AreEqual(result, "<i><font color=\"#000000\">Kurt: true but bad.</font></i>");
        }

        [TestMethod]
        public void FixEllipsesStartQuote()
        {
            var actual = "\"...Foobar\"";
            const string expected = "\"Foobar\"";
            actual = Helper.FixEllipsesStartHelper(actual);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void FixEllipsesStartQuote2()
        {
            var actual = "\"... Foobar\"";
            const string expected = "\"Foobar\"";
            actual = Helper.FixEllipsesStartHelper(actual);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void FixEllipsesStartQuote3()
        {
            var actual = "\" . . . Foobar\"";
            const string expected = "\"Foobar\"";
            actual = Helper.FixEllipsesStartHelper(actual);
            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void FixEllipsesStartDontChange()
        {
            const string input = "- I...";
            string actual = Helper.FixEllipsesStartHelper(input);
            Assert.AreEqual(actual, input);
        }

        #endregion Ellipses start

        #region FixDoubleGreater

        [TestMethod]
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
                lines1[i] = Helper.FixDoubleGreaterThanHelper(lines1[i]);
                lines2[i] = Helper.FixDoubleGreaterThanHelper(lines2[i]);
                lines3[i] = Helper.FixDoubleGreaterThanHelper(lines3[i]);
                lines4[i] = Helper.FixDoubleGreaterThanHelper(lines4[i]);
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

        #endregion FixDoubleGreater

        #region Fix uppercase I inside words

        [TestMethod]
        public void FixUppercaseIInsideWords1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is no troubIe!");
                new FixUppercaseIInsideWords().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "This is no trouble!");
            }
        }

        [TestMethod]
        public void FixUppercaseIInsideWords2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- I'll ring her." + Environment.NewLine + "- ...In a lot of trouble.");
                new FixUppercaseIInsideWords().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "- I'll ring her." + Environment.NewLine + "- ...In a lot of trouble.");
            }
        }

        #endregion Fix uppercase I inside words

        #region Fix dialogs on one line

        [TestMethod]
        public void FixDialogsOnOneLine1()
        {
            const string source = "- I was here, putting our child to sleep-- - Emma.";
            string target = "- I was here, putting our child to sleep--" + Environment.NewLine + "- Emma.";
            string result = Helper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }

        [TestMethod]
        public void FixDialogsOnOneLine2()
        {
            const string source = "- Seriously, though. Are you being bullied? - Nope.";
            string target = "- Seriously, though. Are you being bullied?" + Environment.NewLine + "- Nope.";
            string result = Helper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }

        [TestMethod]
        public void FixDialogsOnOneLine3()
        {
            string source = "- Having sexual relationships" + Environment.NewLine + "with other women. - A'ight.";
            string target = "- Having sexual relationships with other women." + Environment.NewLine + "- A'ight.";
            string result = Helper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }

        [TestMethod]
        public void FixDialogsOnOneLine4()
        {
            string source = "- Haiman, say: \"I love you.\" - So," + Environment.NewLine + "what are you up to? Another question!";
            string target = "- Haiman, say: \"I love you.\"" + Environment.NewLine + "- So, what are you up to? Another question!"; 
            string result = Helper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }        

        #endregion Fix dialogs on one line

        #region FixDoubleDash

        [TestMethod]
        public void FixDoubleDashTest1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                // <font color="#000000"> and <font>
                InitializeFixCommonErrorsLine(target, "<font color=\"#000000\">-- Mm-hmm.</font>");
                new FixDoubleDash().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<font color=\"#000000\">...Mm-hmm.</font>");
            }
        }

        [TestMethod]
        public void FixDoubleDashTest2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                // <font color="#000000"> and <font>
                InitializeFixCommonErrorsLine(target, "<b>Mm-hmm.</b>\r\n<font color=\"#000000\">-- foobar</font>");
                new FixDoubleDash().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<b>Mm-hmm.</b>\r\n<font color=\"#000000\">...foobar</font>");
            }
        }

        #endregion FixDoubleDash

        #region Start with upppercase after colon

        [TestMethod]
        public void StartWithUppercaseAfterColon1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "John: <i>hi!</i>");
                new FixStartWithUppercaseLetterAfterColon().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("John: <i>Hi!</i>", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void StartWithUppercaseAfterColon2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "John: <i>hi!</i>. Harry: maybe!");
                new FixStartWithUppercaseLetterAfterColon().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("John: <i>Hi!</i>. Harry: Maybe!", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void StartWithUppercaseAfterColon3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "John: <b>hi!</b>. Harry: <u>maybe!</u>");
                new FixStartWithUppercaseLetterAfterColon().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("John: <b>Hi!</b>. Harry: <u>Maybe!</u>", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void StartWithUppercaseAfterColon4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "John: <font color=\"#ffff80\">hello world.</font>");
                new FixStartWithUppercaseLetterAfterColon().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("John: <font color=\"#ffff80\">Hello world.</font>", _subtitle.Paragraphs[0].Text);
            }
        }

        #endregion Start with upppercase after colon

        #region Fix Music Notation

        [TestMethod]
        public void FixMusicNotation1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "John: <font color=\"#ffff80\">Hello world.</font>");
                new FixMusicNotation().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("John: <font color=\"#ffff80\">Hello world.</font>", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMusicNotation2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "# Hello world. #");
                new FixMusicNotation().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(string.Format("{0} Hello world. {0}", Configuration.Settings.Tools.MusicSymbol), _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMusicNotation3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "# Hello world. #");
                Configuration.Settings.Tools.MusicSymbol = "♫";
                new FixMusicNotation().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(string.Format("{0} Hello world. {0}", Configuration.Settings.Tools.MusicSymbol), _subtitle.Paragraphs[0].Text);
            }
        }

        #endregion Fix Music Notation

        #region FixFrenchLApostrophe

        [TestMethod]
        public void FixFrenchLApostrophe1()
        {
            var res = Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine.FixFrenchLApostrophe("L'Axxxx and l'axxxx", " L'", "Bye.");
            res = Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine.FixFrenchLApostrophe(res, " l'", "Bye.");
            Assert.AreEqual("L'Axxxx and l'axxxx", res);
        }

        [TestMethod]
        public void FixFrenchLApostrophe2()
        {
            var res = Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine.FixFrenchLApostrophe("l'Axxxx and L'axxxx", " L'", "Bye.");
            res = Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine.FixFrenchLApostrophe(res, " l'", "Bye.");
            Assert.AreEqual("L'Axxxx and l'axxxx", res);
        }

        [TestMethod]
        public void FixFrenchLApostrophe3()
        {
            var res = Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine.FixFrenchLApostrophe("l'Axxxx." + Environment.NewLine + "l'axxxx", " L'", "Bye.");
            res = Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine.FixFrenchLApostrophe(res, " l'", "Bye.");
            Assert.AreEqual("L'Axxxx." + Environment.NewLine + "L'axxxx", res);
        }

        [TestMethod]
        public void FixFrenchLApostrophe4()
        {
            var res = Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine.FixFrenchLApostrophe("l'Axxxx and" + Environment.NewLine + "L'axxxx", " L'", "Bye.");
            res = Nikse.SubtitleEdit.Logic.Ocr.OcrFixEngine.FixFrenchLApostrophe(res, " l'", "Bye.");
            Assert.AreEqual("L'Axxxx and" + Environment.NewLine + "l'axxxx", res);
        }

        #endregion FixFrenchLApostrophe

        #region Start with uppercase letter after period inside paragraph.

        [TestMethod]
        public void FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest1()
        {
            const string ExpectedOuput = "<i>- Foobar! - What is it?</i>";
            var p = new Paragraph("<i>- Foobar! - what is it?</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(ExpectedOuput, p.Text);
        }

        [TestMethod]
        public void FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest2()
        {
            const string ExpectedOuput = "<i>- Foobar... what is it?</i>";
            var p = new Paragraph(ExpectedOuput, 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(ExpectedOuput, p.Text);
        }

        [TestMethod]
        public void FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest3()
        {
            const string ExpectedOuput = "<i>- Foobar??? What is it?</i>";
            var p = new Paragraph("<i>- Foobar??? what is it?</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(ExpectedOuput, p.Text);
        }

        [TestMethod]
        public void FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest4()
        {
            const string ExpectedOuput = "<i>- Foobar??? 'Cause.</i>";
            var p = new Paragraph("<i>- Foobar??? 'cause.</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(ExpectedOuput, p.Text);
        }

        [TestMethod]
        public void FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest5()
        {
            const string ExpectedOuput = "<i>- Foobar??? I. Lower</i>";
            var p = new Paragraph("<i>- Foobar??? i. lower</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(ExpectedOuput, p.Text);
        }

        #endregion

        #region Fix unneeded periods after [?!]

        [TestMethod]
        public void FixUnneededPeriodsTest1()
        {
            string processedText = FixUnneededPeriods.RemoveDotAfterPunctuation("Foobar?.\r\nFoobar!.\r\nFoobar");
            Assert.AreEqual("Foobar?\r\nFoobar!\r\nFoobar", processedText);
        }

        [TestMethod]
        public void FixUnneededPeriodsTest2()
        {
            string processedText = FixUnneededPeriods.RemoveDotAfterPunctuation("Foobar?.");
            Assert.AreEqual("Foobar?", processedText);

            processedText = FixUnneededPeriods.RemoveDotAfterPunctuation("Foobar!.");
            Assert.AreEqual("Foobar!", processedText);
        }

        [TestMethod]
        public void FixUnneededPeriodsTest3()
        {
            string processedText = FixUnneededPeriods.RemoveDotAfterPunctuation("Foobar?. Foobar!.... Foobar");
            Assert.AreEqual("Foobar? Foobar! Foobar", processedText);
        }

        #endregion
    }
}
