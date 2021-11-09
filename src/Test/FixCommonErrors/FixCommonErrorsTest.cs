using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms;
using System;
using System.Collections.Generic;
using System.IO;

namespace Test.FixCommonErrors
{
    /// <summary>
    /// This is a test class for FixCommonErrors and is intended
    /// to contain all FixCommonErrorsTest Unit Tests
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

        private static Nikse.SubtitleEdit.Forms.FixCommonErrors GetFixCommonErrorsLib()
        {
            return new Nikse.SubtitleEdit.Forms.FixCommonErrors();
        }

        private void InitializeFixCommonErrorsLine(Nikse.SubtitleEdit.Forms.FixCommonErrors target, string line)
        {
            _subtitle = new Subtitle();
            _subtitle.Paragraphs.Add(new Paragraph(line, 100, 10000));
            target.Initialize(_subtitle, new SubRip(), System.Text.Encoding.UTF8);
        }

        private void InitializeFixCommonErrorsLine(Nikse.SubtitleEdit.Forms.FixCommonErrors target, string line, string line2)
        {
            _subtitle = new Subtitle();
            _subtitle.Paragraphs.Add(new Paragraph(line, 100, 10000));
            _subtitle.Paragraphs.Add(new Paragraph(line2, 10001, 30000));
            target.Initialize(_subtitle, new SubRip(), System.Text.Encoding.UTF8);
        }

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

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            var dictionaryFolder = Configuration.DictionariesDirectory;
            if (!Directory.Exists(dictionaryFolder))
            {
                Directory.CreateDirectory(dictionaryFolder);
            }

            var asm = System.Reflection.Assembly.GetExecutingAssembly();
            var stream = asm.GetManifestResourceStream("Test.Dictionaries.en_US.aff");
            WriteStream(stream, Path.Combine(dictionaryFolder, "en_US.aff"));

            stream = asm.GetManifestResourceStream("Test.Dictionaries.en_US.dic");
            WriteStream(stream, Path.Combine(dictionaryFolder, "en_US.dic"));

            stream = asm.GetManifestResourceStream("Test.Dictionaries.names.xml");
            WriteStream(stream, Path.Combine(dictionaryFolder, "names.xml"));
        }

        private static readonly object WriteStreamLock = new Object();

        private static void WriteStream(Stream stream, string fileName)
        {
            lock (WriteStreamLock)
            {
                if (stream != null)
                {
                    if (File.Exists(fileName))
                    {
                        try
                        {
                            File.Delete(fileName);
                        }
                        catch
                        {
                            return;
                        }
                    }
                    using (Stream file = File.OpenWrite(fileName))
                    {
                        CopyStream(stream, file);
                    }
                }
            }
        }

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
        public void FixShortLinesWithDash()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Any progress" + Environment.NewLine + "on the e-mail?");
                new FixShortLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Any progress on the e-mail?");
            }
        }

        [TestMethod]
        public void FixShortLinesWithDash2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Any progress" + Environment.NewLine + "- None.");
                new FixShortLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "- Any progress" + Environment.NewLine + "- None.");
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

            var result = Helper.FixShortLines(input, "en");
            var result2 = Helper.FixShortLines(input2, "en");
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

        [TestMethod]
        public void FixItalicsLine1BadEndingCasing1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>Test</I>");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>Test</i>", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixItalicsLine1BadEndingCasing2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<I>Test</i>");
                new FixInvalidItalicTags().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>Test</i>", _subtitle.Paragraphs[0].Text);
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
        public void FixCommonOcrErrorsEndBoldTag()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<b>This is a good ship.</b>");
                target.FixOcrErrorsViaReplaceList("eng");
                Assert.AreEqual(target.Subtitle.Paragraphs[0].Text, "<b>This is a good ship.</b>");
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
            using (var form = new Nikse.SubtitleEdit.Forms.FixCommonErrors())
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

        [TestMethod]
        public void FixCommonOcrErrorsStartEllipsis()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "…but never could.");
                target.FixOcrErrorsViaReplaceList("eng");
                Assert.AreEqual("…but never could.", target.Subtitle.Paragraphs[0].Text);
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
                Assert.AreEqual("The <i>Bombshell</i> will gone.", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesItalicEnd()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "The <i>Bombshell</i>will gone.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("The <i>Bombshell</i> will gone.", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesBeforePeriod1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "It will be okay.It surely will be!");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("It will be okay. It surely will be!", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesBeforePeriod2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "you can't get out.Alright?");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("you can't get out. Alright?", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "What did Dr. Gey say?");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("What did Dr. Gey say?", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesChange2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "To be,or not to be!");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("To be, or not to be!", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to the O.R. now!");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Go to the O.R. now!", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Email niksedk@gmail.Com now!");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Email niksedk@gmail.Com now!", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to www.nikse.dk for more info");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Go to www.nikse.dk for more info", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange5Greek()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Aφαίρεσαν ό,τι αντρικό είχες.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "el" });  // Greek
                Assert.AreEqual("Aφαίρεσαν ό,τι αντρικό είχες.", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange6()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to nikse.com for more info");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Go to nikse.com for more info", _subtitle.Paragraphs[0].Text);
            }
        }
        [TestMethod]
        public void FixMissingSpacesNoChange7()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to nikse.net for more info");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Go to nikse.net for more info", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesNoChange8()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Go to nikse.org for more info");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Go to nikse.org for more info", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesOneLetterPlusDotDotDot()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "I...want missing spaces.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "en" });
                Assert.AreEqual("I... want missing spaces.", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesAfterQuestionMark()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Long made a plea deal?Did you know?");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "en" });
                Assert.AreEqual("Long made a plea deal? Did you know?", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesAfterExclamationMark()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Long made a plea deal!Did you know?");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "en" });
                Assert.AreEqual("Long made a plea deal! Did you know?", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesAfterThreeDotsBeforeDollarSign()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Give me...$20!");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "en" });
                Assert.AreEqual("Give me... $20!", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSwedish()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "VD:n tycker det är bra.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "sv" });
                Assert.AreEqual("VD:n tycker det är bra.", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSwedish2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "MARCUS:tycker det är bra.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "sv" });
                Assert.AreEqual("MARCUS: tycker det är bra.", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingFinnish()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "FBI:n");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "fi" });
                Assert.AreEqual("FBI:n", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingFinnish2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "MARCUS:tycker det är bra.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "fi" });
                Assert.AreEqual("MARCUS: tycker det är bra.", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesMusicSymbolsFirst()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string input = "♪Dream, little one ♪";
                const string expected = "♪ Dream, little one ♪";
                InitializeFixCommonErrorsLine(target, input);
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesMusicSymbolsLast()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string input = "♪ Dream, little one♪";
                const string expected = "♪ Dream, little one ♪";
                InitializeFixCommonErrorsLine(target, input);
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesMusicSymbolsItalicFirst()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string input = "<i>♪Dream, little one ♪</i>";
                const string expected = "<i>♪ Dream, little one ♪</i>";
                InitializeFixCommonErrorsLine(target, input);
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesMusicSymbolsItalicLast()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string input = "<i>♪ Dream, little one♪</i>";
                const string expected = "<i>♪ Dream, little one ♪</i>";
                InitializeFixCommonErrorsLine(target, input);
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesMusicTwoLines()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                string input = "♪ Dream, little one♪" + Environment.NewLine +
                               "♪ Dream, little one♪";
                string expected = "♪ Dream, little one ♪" + Environment.NewLine +
                                  "♪ Dream, little one ♪";
                InitializeFixCommonErrorsLine(target, input);
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesMusicTwoLinesItalic()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                string input = "<i>♪Dream, little one♪</i>" + Environment.NewLine +
                               "<i>♪Dream, little one♪</i>";
                string expected = "<i>♪ Dream, little one ♪</i>" + Environment.NewLine +
                                  "<i>♪ Dream, little one ♪</i>";
                InitializeFixCommonErrorsLine(target, input);
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesMusicDoNotChange()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                string input = "♪♫ Dream, little one ♫♪";
                string expected = input;
                InitializeFixCommonErrorsLine(target, input);
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesMusicDoNotChange2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                string input = "♪♪ ! ♪♪" + Environment.NewLine +
                               "♪";
                string expected = input;
                InitializeFixCommonErrorsLine(target, input);
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesMusicHashtags()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string input = "#Dream, little one#";
                const string expected = "# Dream, little one #";
                InitializeFixCommonErrorsLine(target, input);
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesDontRemoveHashTag()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string input = "#Dream";
                const string expected = "#Dream";
                InitializeFixCommonErrorsLine(target, input);
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesQuestionMarkAndNumber()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string input = "Why don't we make it?20.";
                const string expected = "Why don't we make it? 20.";
                InitializeFixCommonErrorsLine(target, input);
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(expected, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesDialogThreeLines()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "-Person one speaks" + Environment.NewLine +
                    "and continues speaking some." + Environment.NewLine +
                    "-The other person speaks and there will be no fix executed.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("- Person one speaks" + Environment.NewLine +
                    "and continues speaking some." + Environment.NewLine +
                    "- The other person speaks and there will be no fix executed.", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesDialogThreeLines2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "-Person one speaks." + Environment.NewLine +
                                                      "-The other person starts speaking and continues" + Environment.NewLine +
                                                      "to speak loudly for a little white.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("- Person one speaks." + Environment.NewLine +
                                "- The other person starts speaking and continues" + Environment.NewLine +
                                "to speak loudly for a little white.", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesStartEllipsisDoNotTouch()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "...I'm fine.");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("...I'm fine.", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesStartEllipsisDoNotTouch2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "...\"litigious need not apply.\"");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("...\"litigious need not apply.\"", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesMusicSymbolInItalicDoNotTouch()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>♪</i>");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>♪</i>", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesArabic()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "aaa\u060Caaa؟aaa.aaa");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "ar" });
                Assert.AreEqual("aaa\u060C aaa؟ aaa. aaa", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMissingSpacesArabic2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "aaa\u060C\"aaa؟\"aaa.\"aaa");
                new FixMissingSpaces().Fix(_subtitle, new EmptyFixCallback { Language = "ar" });
                Assert.AreEqual("aaa\u060C\"aaa؟\"aaa.\"aaa", _subtitle.Paragraphs[0].Text);
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
        public void FixUnneededSpaces7()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>\"pescado. \"</i>");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>\"pescado.\"</i>");
            }
        }

        [TestMethod]
        public void FixUnneededSpaces8()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>\" pescado.\"</i>");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>\"pescado.\"</i>");
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

        [TestMethod]
        public void FixUnneededSpacesBeforeEndTag1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>I happen to have" + Environment.NewLine + " the blood of an ancient family .</i>");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>I happen to have" + Environment.NewLine + "the blood of an ancient family.</i>", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesBeforeEndTag1b()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>I happen to have" + Environment.NewLine + " the blood of an ancient family !</i>");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>I happen to have" + Environment.NewLine + "the blood of an ancient family!</i>", _subtitle.Paragraphs[0].Text);
            }
        }


        [TestMethod]
        public void FixUnneededSpacesBeforeEndTag1c()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>I happen to have" + Environment.NewLine + " the blood of an ancient family ?</i>");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>I happen to have" + Environment.NewLine + "the blood of an ancient family?</i>", _subtitle.Paragraphs[0].Text);
            }
        }


        [TestMethod]
        public void FixUnneededSpacesBeforeEndTag2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>I happen to have to. </i>" + Environment.NewLine + "Right?");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>I happen to have to.</i>" + Environment.NewLine + "Right?", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesBeforeEndTag2a()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>I happen to have to! </i>" + Environment.NewLine + "Right?");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>I happen to have to!</i>" + Environment.NewLine + "Right?", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixUnneededSpacesBeforeEndTag2b()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>I happen to have to? </i>" + Environment.NewLine + "Right?");
                new FixUnneededSpaces().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>I happen to have to?</i>" + Environment.NewLine + "Right?", _subtitle.Paragraphs[0].Text);
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

        public void FixEmptyLinesTest3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "\\U202C");
                new FixEmptyLines().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(0, _subtitle.Paragraphs.Count);
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
            s.Paragraphs.Add(prev);
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("bye.", p.Text);
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormalDialogNoChange1()
        {
            var prev = new Paragraph("Bye -", 0, 1000);
            var p = new Paragraph("- moss!", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(prev);
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("- moss!", p.Text);
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphNormalDialogNoChange2()
        {
            var prev = new Paragraph("Bye -", 0, 1000);
            var p = new Paragraph("- moss!" + Environment.NewLine + " - Bye.", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(prev);
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("- moss!" + Environment.NewLine + " - Bye.", p.Text);
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphDashAfterPeriod()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("Bye." + Environment.NewLine + "- bye.", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(prev);
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("Bye." + Environment.NewLine + "- Bye.", p.Text);
        }

        [TestMethod]
        public void StartWithUppercaseAfterParagraphDashAfterPeriod2()
        {
            var prev = new Paragraph("Bye.", 0, 1000);
            var p = new Paragraph("Bye." + Environment.NewLine + "<i>- bye.</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(prev);
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("Bye." + Environment.NewLine + "<i>- Bye.</i>", p.Text);
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

        [TestMethod]
        public void FixSpanishExclamationMarkAndQuestionMarkReplaceLastPeriod()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "¡Cómo estás.");
                new FixSpanishInvertedQuestionAndExclamationMarks().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "¡Cómo estás!");
            }
        }

        [TestMethod]
        public void FixSpanishExclamationMarkAndQuestionMarkThreePeriods()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "¿Cómo estás...");
                new FixSpanishInvertedQuestionAndExclamationMarks().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "¿Cómo estás...?");
            }
        }

        [TestMethod]
        public void FixSpanishExclamationMarkAndQuestionMarkEllipsis()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "¿Cómo estás…");
                new FixSpanishInvertedQuestionAndExclamationMarks().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "¿Cómo estás…?");
            }
        }

        [TestMethod]
        public void FixSpanishExclamationMarkAndQuestionMarkMultiLineNoChange()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                _subtitle = new Subtitle();
                _subtitle.Paragraphs.Add(new Paragraph("¿Debo preguntar...", 7000, 10000));
                _subtitle.Paragraphs.Add(new Paragraph("...otra vez?", 10100, 12000));
                target.Initialize(_subtitle, new SubRip(), System.Text.Encoding.UTF8);
                new FixSpanishInvertedQuestionAndExclamationMarks().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "¿Debo preguntar...");
                Assert.AreEqual(_subtitle.Paragraphs[1].Text, "...otra vez?");
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
                new FixHyphensRemoveDashSingleLine().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>Mm-hmm.</i>");
            }
        }

        [TestMethod]
        public void FixSingleLineDash1Font()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<font color='red'>- Mm-hmm.</font>");
                new FixHyphensRemoveDashSingleLine().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<font color='red'>Mm-hmm.</font>");
            }
        }

        [TestMethod]
        public void FixSingleLineDash1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Mm-hmm.");
                new FixHyphensRemoveDashSingleLine().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Mm-hmm.");
            }
        }

        [TestMethod]
        public void FixSingleLineDash3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- I-I never thought of that.");
                new FixHyphensRemoveDashSingleLine().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "I-I never thought of that.");
            }
        }

        [TestMethod]
        public void FixSingleLineDash4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- Uh-huh.");
                new FixHyphensRemoveDashSingleLine().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "Uh-huh.");
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

            for (int i = 0; i < lines1.Count; i++)
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

        [TestMethod]
        public void FixUppercaseIInsideWords3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>FIight DU 720 from StockhoIm...</i>");
                new FixUppercaseIInsideWords().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>Flight DU 720 from Stockholm...</i>");
            }
        }

        [TestMethod]
        public void FixUppercaseIInsideWords4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>FIight DU 720 from BraziI...</i>");
                new FixUppercaseIInsideWords().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>Flight DU 720 from Brazil...</i>");
            }
        }

        [TestMethod]
        public void FixUppercaseIInsideWords5()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>FIight DU 720 from McIvan BraziI...</i>");
                new FixUppercaseIInsideWords().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "<i>Flight DU 720 from McIvan Brazil...</i>");
            }
        }

        [TestMethod]
        public void FixUppercaseIInsideWords6_AfterCurlyBracket()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "{MAN}Isabella");
                new FixUppercaseIInsideWords().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "{MAN}Isabella");
            }
        }

        [TestMethod]
        public void FixUppercaseIInsideWords_Dont_Change()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "I've had multiple MRIs today.");
                new FixUppercaseIInsideWords().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(_subtitle.Paragraphs[0].Text, "I've had multiple MRIs today.");
            }
        }

        [TestMethod]
        public void FixUppercaseIInsideWords_Dont_Change_Starting_I()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Ioannises had a nice day.");
                new FixUppercaseIInsideWords().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Ioannises had a nice day.", _subtitle.Paragraphs[0].Text);
            }
        }

        #endregion Fix uppercase I inside words

        #region Fix dialogs on one line

        [TestMethod]
        public void FixDialogsOnOneLine1()
        {
            const string source = "- I was here, putting our child to sleep-- - Emma.";
            var target = "- I was here, putting our child to sleep--" + Environment.NewLine + "- Emma.";
            var result = Helper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }

        [TestMethod]
        public void FixDialogsOnOneLine2()
        {
            const string source = "- Seriously, though. Are you being bullied? - Nope.";
            var target = "- Seriously, though. Are you being bullied?" + Environment.NewLine + "- Nope.";
            var result = Helper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }

        [TestMethod]
        public void FixDialogsOnOneLine3()
        {
            var source = "- Having sexual relationships" + Environment.NewLine + "with other women. - A'ight.";
            var target = "- Having sexual relationships with other women." + Environment.NewLine + "- A'ight.";
            var result = Helper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }

        [TestMethod]
        public void FixDialogsOnOneLine4()
        {
            var source = "- Haiman, say: \"I love you.\" - So," + Environment.NewLine + "what are you up to? Another question!";
            var target = "- Haiman, say: \"I love you.\"" + Environment.NewLine + "- So, what are you up to? Another question!";
            var result = Helper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }

        [TestMethod]
        public void FixDialogsOnOneLine5()
        {
            var source = "- [Gunshot] - [Scream]";
            var target = "- [Gunshot]" + Environment.NewLine + "- [Scream]";
            var result = Helper.FixDialogsOnOneLine(source, "en");
            Assert.AreEqual(result, target);
        }

        [TestMethod]
        public void FixDialogsOnOneLine6()
        {
            var source = "- Where did she get the tip? <i>- I don't know.</i>";
            var target = "- Where did she get the tip?" + Environment.NewLine + "<i>- I don't know.</i>";
            var result = Helper.FixDialogsOnOneLine(source, "en");
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

        [TestMethod]
        public void FixMusicNotationQuestionMarks()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "? Hello world?");
                Configuration.Settings.Tools.MusicSymbol = "♫";
                new FixMusicNotation().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("♫ Hello world ♫", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMusicNotationQuestionMarksNarrator()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Foobar: ? Hello world?");
                Configuration.Settings.Tools.MusicSymbol = "♫";
                new FixMusicNotation().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(string.Format("Foobar: {0} Hello world {0}", Configuration.Settings.Tools.MusicSymbol), _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMusicNotationNoHashtags()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string s = "Lär mig #människor";
                InitializeFixCommonErrorsLine(target, s);
                Configuration.Settings.Tools.MusicSymbol = "♫";
                new FixMusicNotation().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(s, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMusicNotationNoHashtagsMultiple()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                const string s = "Lär mig #människor, #metoo, #timesup";
                InitializeFixCommonErrorsLine(target, s);
                Configuration.Settings.Tools.MusicSymbol = "♫";
                new FixMusicNotation().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(s, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMusicNotationQuestionDoNotChange()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                var input = "- (whispers): like this..." + Environment.NewLine +
                            "- Like what?";
                InitializeFixCommonErrorsLine(target, input);
                Configuration.Settings.Tools.MusicSymbol = "♫";
                new FixMusicNotation().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual(input, _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMusicNotationNoChange()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "?");
                new FixMusicNotation().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("?", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMusicNotationNoChange2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "??");
                Configuration.Settings.Tools.MusicSymbol = "♫";
                new FixMusicNotation().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("♫♫", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void FixMusicNotationNoChange3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "");
                new FixMusicNotation().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("", _subtitle.Paragraphs[0].Text);
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
            const string expectedOutput = "<i>- Foobar! - What is it?</i>";
            var p = new Paragraph("<i>- Foobar! - what is it?</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(expectedOutput, p.Text);
        }

        [TestMethod]
        public void FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest2()
        {
            const string expectedOutput = "<i>- Foobar... what is it?</i>";
            var p = new Paragraph(expectedOutput, 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(expectedOutput, p.Text);
        }

        [TestMethod]
        public void FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest3()
        {
            const string expectedOutput = "<i>- Foobar??? What is it?</i>";
            var p = new Paragraph("<i>- Foobar??? what is it?</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(expectedOutput, p.Text);
        }

        [TestMethod]
        public void FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest4()
        {
            const string expectedOutput = "<i>- Foobar??? 'Cause.</i>";
            var p = new Paragraph("<i>- Foobar??? 'cause.</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(expectedOutput, p.Text);
        }

        [TestMethod]
        public void FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest5()
        {
            const string expectedOutput = "<i>- Foobar??? I. Lower</i>";
            var p = new Paragraph("<i>- Foobar??? i. lower</i>", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(expectedOutput, p.Text);
        }

        [TestMethod]
        public void FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest6()
        {
            var s = new Subtitle();
            var p = new Paragraph("And I was... i was just tired.", 1200, 5000);
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("And I was... I was just tired.", p.Text);
        }

        [TestMethod]
        public void FixStartWithUppercaseLetterAfterPeriodInsideParagraphTest7()
        {
            var s = new Subtitle();
            var p = new Paragraph("And I was... he was just tired.", 1200, 5000);
            s.Paragraphs.Add(p);
            new FixStartWithUppercaseLetterAfterPeriodInsideParagraph().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("And I was... he was just tired.", p.Text);
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

        [TestMethod]
        public void FixUnneededPeriodsTestKorean()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("- 안녕하세요." + Environment.NewLine + "- 반갑습니다.", 0, 1000));
            var fup = new FixUnneededPeriods();
            fup.Fix(sub, new EmptyFixCallback { Language = "ko" });
            Assert.AreEqual("- 안녕하세요" + Environment.NewLine + "- 반갑습니다", sub.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixUnneededPeriodsTestKoreanItalic()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("<i>- 안녕하세요." + Environment.NewLine + "- 반갑습니다.</i>", 0, 1000));
            var fup = new FixUnneededPeriods();
            fup.Fix(sub, new EmptyFixCallback { Language = "ko" });
            Assert.AreEqual("<i>- 안녕하세요" + Environment.NewLine + "- 반갑습니다</i>", sub.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixUnneededPeriodsTestKoreanDoNotChange()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("안녕하세요...", 0, 1000));
            var fup = new FixUnneededPeriods();
            fup.Fix(sub, new EmptyFixCallback { Language = "ko" });
            Assert.AreEqual("안녕하세요...", sub.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixCommas1()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("Hi,, how are you?", 0, 1000));
            var fc = new FixCommas();
            fc.Fix(sub, new EmptyFixCallback());
            Assert.AreEqual("Hi, how are you?", sub.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixCommas2()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("Hi, , how are you?", 0, 1000));
            var fc = new FixCommas();
            fc.Fix(sub, new EmptyFixCallback());
            Assert.AreEqual("Hi, how are you?", sub.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixCommas3()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("Hi,,,", 0, 1000));
            var fc = new FixCommas();
            fc.Fix(sub, new EmptyFixCallback());
            Assert.AreEqual("Hi...", sub.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixCommas4()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("Hi,!", 0, 1000));
            var fc = new FixCommas();
            fc.Fix(sub, new EmptyFixCallback());
            Assert.AreEqual("Hi!", sub.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixCommas5()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("Hi,,, are you okay?", 0, 1000));
            var fc = new FixCommas();
            fc.Fix(sub, new EmptyFixCallback());
            Assert.AreEqual("Hi... are you okay?", sub.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixCommasArabic()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("مرحبا ، ، مرحبا", 0, 1000));
            var fc = new FixCommas();
            fc.Fix(sub, new EmptyFixCallback { Language = "ar" });
            Assert.AreEqual("مرحبا، مرحبا", sub.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixCommas6()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("[Hi,]", 0, 1000));
            var fc = new FixCommas();
            fc.Fix(sub, new EmptyFixCallback());
            Assert.AreEqual("[Hi]", sub.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixCommas7()
        {
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph("(Hi,)", 0, 1000));
            var fc = new FixCommas();
            fc.Fix(sub, new EmptyFixCallback());
            Assert.AreEqual("(Hi)", sub.Paragraphs[0].Text);
        }

        #endregion

        #region Fix Danish letter "i"

        [TestMethod]
        public void FixDanishLetterITest1()
        {
            const string expectedOutput = "Det må I undskylde.";
            var p = new Paragraph("Det må i undskylde.", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixDanishLetterI().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(expectedOutput, p.Text);
        }

        [TestMethod]
        public void FixDanishLetterITest2()
        {
            const string expectedOutput = "Det må I selv om.";
            var p = new Paragraph("Det må i selv om.", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixDanishLetterI().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(expectedOutput, p.Text);
        }

        [TestMethod]
        public void FixDanishLetterITest3()
        {
            const string expectedOutput = "I dag skal I hilse på";
            var p = new Paragraph("I dag skal i hilse på", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixDanishLetterI().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(expectedOutput, p.Text);
        }

        [TestMethod]
        public void FixDanishLetterITestNegative1()
        {
            const string expectedOutput = "Der vil jeg ikke gå i skole.";
            var p = new Paragraph(expectedOutput, 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new FixDanishLetterI().Fix(s, new EmptyFixCallback());
            Assert.AreEqual(expectedOutput, p.Text);
        }

        #endregion

        #region AddMissingQuotes

        [TestMethod]
        public void AddMissingQuotesMissingStart()
        {
            var p = new Paragraph("Bad Ape!\"", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new AddMissingQuotes().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("\"Bad Ape!\"", p.Text);
        }

        [TestMethod]
        public void AddMissingQuotesMissingEnd()
        {
            var p = new Paragraph("\"Bad Ape!", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new AddMissingQuotes().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("\"Bad Ape!\"", p.Text);
        }

        [TestMethod]
        public void AddMissingQuotesMissingMultiple()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("\"Bad Ape!", 1200, 5000));
            s.Paragraphs.Add(new Paragraph("\"Bad Ape!", 11200, 15000));
            s.Paragraphs.Add(new Paragraph("\"Bad Ape!", 21200, 25000));
            new AddMissingQuotes().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("\"Bad Ape!\"", s.Paragraphs[0].Text);
            Assert.AreEqual("\"Bad Ape!\"", s.Paragraphs[1].Text);
            Assert.AreEqual("\"Bad Ape!\"", s.Paragraphs[2].Text);
        }

        [TestMethod]
        public void AddMissingQuotesOtherQuote()
        {
            var p = new Paragraph("\"Bad Ape!”", 1200, 5000);
            var s = new Subtitle();
            s.Paragraphs.Add(p);
            new AddMissingQuotes().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("\"Bad Ape!\"", p.Text);
        }

        [TestMethod]
        public void AddMissingQuotesMissingMultiple2()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("\"Dear Juilliard School of Music,", 2000, 5000));
            s.Paragraphs.Add(new Paragraph("I was baffled by your restraining order" + Environment.NewLine + "of October the 13th.", 5100, 8000));
            s.Paragraphs.Add(new Paragraph("What did I do?\"", 8100, 1500));
            new AddMissingQuotes().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("\"Dear Juilliard School of Music,", s.Paragraphs[0].Text);
            Assert.AreEqual("I was baffled by your restraining order" + Environment.NewLine + "of October the 13th.", s.Paragraphs[1].Text);
            Assert.AreEqual("What did I do?\"", s.Paragraphs[2].Text);
        }

        [TestMethod]
        public void AddMissingQuotesMissingMultipleDontTouch()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("And Caesar says, \"If I wanted two,", 2000, 5000));
            s.Paragraphs.Add(new Paragraph("I would have ordered two.\"", 5100, 8000));
            new AddMissingQuotes().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("And Caesar says, \"If I wanted two,", s.Paragraphs[0].Text);
            Assert.AreEqual("I would have ordered two.\"", s.Paragraphs[1].Text);
        }

        [TestMethod]
        public void AddMissingQuotesMissingMultiple3lines()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("\"Painter left job unfinished," + Environment.NewLine + "spilled paint on porch.", 2000, 5000));
            s.Paragraphs.Add(new Paragraph("Do not hire this person.", 5100, 8000));
            s.Paragraphs.Add(new Paragraph("Zero stars.\"", 8100, 1200));
            new AddMissingQuotes().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("\"Painter left job unfinished," + Environment.NewLine + "spilled paint on porch.", s.Paragraphs[0].Text);
            Assert.AreEqual("Do not hire this person.", s.Paragraphs[1].Text);
            Assert.AreEqual("Zero stars.\"", s.Paragraphs[2].Text);
        }

        [TestMethod]
        public void AddMissingQuotesMissingMultiple4lines()
        {
            var s = new Subtitle();
            s.Paragraphs.Add(new Paragraph("\"Painter left job unfinished," + Environment.NewLine + "spilled paint on porch.", 2000, 5000));
            s.Paragraphs.Add(new Paragraph("Do not hire this person.", 5100, 8000));
            s.Paragraphs.Add(new Paragraph("Shrug emoji, poop emoji," + Environment.NewLine + "thumbs-down emoji.", 8100, 15000));
            s.Paragraphs.Add(new Paragraph("Zero stars.\"", 15100, 1800));
            new AddMissingQuotes().Fix(s, new EmptyFixCallback());
            Assert.AreEqual("\"Painter left job unfinished," + Environment.NewLine + "spilled paint on porch.", s.Paragraphs[0].Text);
            Assert.AreEqual("Do not hire this person.", s.Paragraphs[1].Text);
            Assert.AreEqual("Shrug emoji, poop emoji," + Environment.NewLine + "thumbs-down emoji.", s.Paragraphs[2].Text);
            Assert.AreEqual("Zero stars.\"", s.Paragraphs[3].Text);
        }

        #endregion

        #region Fix continuation style

        [TestMethod]
        public void FixUnnecessaryLeadingDots1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test...", "...and we need to do it.");
                new FixUnnecessaryLeadingDots().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("and we need to do it.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixUnnecessaryLeadingDots2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test.", "...coming in mid-sentence.");
                new FixUnnecessaryLeadingDots().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test.", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...coming in mid-sentence.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixUnnecessaryLeadingDots3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "{\an8}<i>This is a test...</i>" + Environment.NewLine + " " + Environment.NewLine + "_", "{\an8}<i>...and we need to do it.</i>" + Environment.NewLine + " " + Environment.NewLine + "_");
                new FixUnnecessaryLeadingDots().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("{\an8}<i>This is a test...</i>" + Environment.NewLine + " " + Environment.NewLine + "_", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("{\an8}<i>and we need to do it.</i>" + Environment.NewLine + " " + Environment.NewLine + "_", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test...", "...but we need to do it.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test,", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("but we need to do it.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "NO ENTRY", "...but we need to do it.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("NO ENTRY", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...but we need to do it.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle2A()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "10 LANDMINES", "...but we need to do it.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("10 LANDMINES", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...but we need to do it.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle2B()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "No wait...", "NO ENTRY");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("No wait...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("NO ENTRY", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle2C()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "No wait...", "10 LANDMINES");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("No wait...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("10 LANDMINES", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test...", "...but we need to do it.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.OnlyTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("but we need to do it.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle4()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test,", "but we need to do it.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...but we need to do it.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle5()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "no entry", "...but we need to do it.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("no entry", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...but we need to do it.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle6()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>la la la</i>", "...but we need to do it.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>la la la</i>", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...but we need to do it.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle7()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test,", "Mark should do it.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...Mark should do it.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle8()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test...", "...Mark can do.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("Mark can do.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle9()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "{\an8}<i>'This is a test...</i>" + Environment.NewLine + " " + Environment.NewLine + "_", "{\an8}<i>...Mark can do.'</i>" + Environment.NewLine + " " + Environment.NewLine + "_");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("{\an8}<i>'This is a test</i>" + Environment.NewLine + " " + Environment.NewLine + "_", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("{\an8}<i>Mark can do.'</i>" + Environment.NewLine + " " + Environment.NewLine + "_", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle10()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "I'm just testin'", "and you can join.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("I'm just testin'...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...and you can join.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle11()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "I am <i>testing</i>", "this right now.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("I am <i>testing</i>...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...this right now.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle13()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "I am testing -", "- this right now.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("I am testing...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...this right now.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle14()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "I am testing...", "...this right now.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDash;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("I am testing -", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("- this right now.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle15()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "I am testing...", "- ...this right now." + Environment.NewLine + "- You kidding me?");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDash;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("I am testing -", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("- this right now." + Environment.NewLine + "- You kidding me?", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle16()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "I am testing", "- this right now." + Environment.NewLine + "- You kidding me?");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("I am testing...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("- ...this right now." + Environment.NewLine + "- You kidding me?", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle17()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "I am testing", "- this right now." + Environment.NewLine + "- You kidding me?");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDash;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("I am testing -", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("- this right now." + Environment.NewLine + "- You kidding me?", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle18()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test", "to see if it works.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...to see if it works.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle19()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test", "To see if it works.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("To see if it works.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle20()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>This is a test</i>", "to see if it works.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>This is a test...</i>", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...to see if it works.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle21()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a <i>test</i>", "to see if it works.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a <i>test</i>...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...to see if it works.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle22()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a <i>test</i>", "To see if it works.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a <i>test</i>", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("To see if it works.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle23()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test with a comma,", "but it should stay here.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test with a comma,", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("but it should stay here.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle23B()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test with a comma, -", "- but it should stay here.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test with a comma,", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("but it should stay here.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle24()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is a test with a comma, -", "- but it should be removed.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDash;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is a test with a comma -", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("- but it should be removed.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle25()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>This is a test...</i>" + Environment.NewLine + "_", "<i>but I should do it.</i>" + Environment.NewLine + "_");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>This is a test,</i>" + Environment.NewLine + "_", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("<i>but I should do it.</i>" + Environment.NewLine + "_", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle26()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "-Test." + Environment.NewLine + "-(CHATTER)", "Hello.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("-Test." + Environment.NewLine + "-(CHATTER)", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("Hello.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle27()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "-LACEY: Teleportation." + Environment.NewLine + "-(PHONE CHIMES)", "-(PHONE CHIMES)" + Environment.NewLine + "-Time travel.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("-LACEY: Teleportation." + Environment.NewLine + "-(PHONE CHIMES)", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("-(PHONE CHIMES)" + Environment.NewLine + "-Time travel.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle28()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "It's okay, you fucked up", "(PHONE CHIMES)");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("It's okay, you fucked up", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("(PHONE CHIMES)", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle29()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "(PHONE CHIMES)", "It's okay, you fucked up");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("(PHONE CHIMES)", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("It's okay, you fucked up", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle30()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "We might as well...", "-(PHONE CHIMES)" + Environment.NewLine + "-...just eat Dominos...");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("We might as well...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("-(PHONE CHIMES)" + Environment.NewLine + "-...just eat Dominos...", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle31()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test,", "test,");
                _subtitle.Paragraphs.Add(new Paragraph("test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...test...", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("...test.", _subtitle.Paragraphs[2].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle31B()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test,", "test,");
                _subtitle.Paragraphs.Add(new Paragraph("test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test,", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("test,", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("test.", _subtitle.Paragraphs[2].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle31C()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test,", "test,");
                _subtitle.Paragraphs.Add(new Paragraph("test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.NoneLeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test,", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("test...", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("...test.", _subtitle.Paragraphs[2].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle31D()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test,", "test,");
                _subtitle.Paragraphs.Add(new Paragraph("test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.NoneTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test,", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("test...", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("test.", _subtitle.Paragraphs[2].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle32()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test...", "test...");
                _subtitle.Paragraphs.Add(new Paragraph("test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...test...", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("...test.", _subtitle.Paragraphs[2].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle32B()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test...", "test...");
                _subtitle.Paragraphs.Add(new Paragraph("test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("test", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("test.", _subtitle.Paragraphs[2].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle32C()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test...", "test...");
                _subtitle.Paragraphs.Add(new Paragraph("test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.NoneLeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("test...", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("...test.", _subtitle.Paragraphs[2].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle32D()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test...", "test...");
                _subtitle.Paragraphs.Add(new Paragraph("test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.NoneTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("test...", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("test.", _subtitle.Paragraphs[2].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle33()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test...", "test...");
                _subtitle.Paragraphs.Add(new Paragraph("Test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...test...", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("Test.", _subtitle.Paragraphs[2].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle33B()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test...", "test...");
                _subtitle.Paragraphs.Add(new Paragraph("Test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.None;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("test...", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("Test.", _subtitle.Paragraphs[2].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle33C()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test...", "test...");
                _subtitle.Paragraphs.Add(new Paragraph("Test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.NoneLeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("test...", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("Test.", _subtitle.Paragraphs[2].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle33D()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "Test...", "test...");
                _subtitle.Paragraphs.Add(new Paragraph("Test.", 40000, 50000));
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.NoneTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("Test", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("test...", _subtitle.Paragraphs[1].Text);
                Assert.AreEqual("Test.", _subtitle.Paragraphs[2].Text);
            }
        }
        [TestMethod]
        public void FixContinuationStyle34()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is what you call a 'test'...", "and you can join.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is what you call a 'test'...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...and you can join.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle34B()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is what you call a 'test'", "and you can join.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is what you call a 'test'...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...and you can join.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle34C()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is what you call a 'test,'", "and you can join.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is what you call a 'test'...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...and you can join.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle34D()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "This is what you call a 'test...'", "and you can join.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("This is what you call a 'test'...", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("...and you can join.", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle34E()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "'This is what you call a test'", "'and you can join.'");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("'This is what you call a test...'", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("'...and you can join.'", _subtitle.Paragraphs[1].Text);
            }
        }

        [TestMethod]
        public void FixContinuationStyle35()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "They wanted to test", "But they couldn't.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new FixContinuationStyle().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("They wanted to test", _subtitle.Paragraphs[0].Text);
                Assert.AreEqual("But they couldn't.", _subtitle.Paragraphs[1].Text);
            }
        }

        #endregion Fix continuation style


        [TestMethod]
        public void RemoveDialogFirstLineInNonDialogs1()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- They wanted to test!");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new RemoveDialogFirstLineInNonDialogs().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("They wanted to test!", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void RemoveDialogFirstLineInNonDialogs2()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "<i>- They wanted to test!</i>");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new RemoveDialogFirstLineInNonDialogs().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("<i>They wanted to test!</i>", _subtitle.Paragraphs[0].Text);
            }
        }

        [TestMethod]
        public void RemoveDialogFirstLineInNonDialogs3()
        {
            using (var target = GetFixCommonErrorsLib())
            {
                InitializeFixCommonErrorsLine(target, "- They wanted to test!" + Environment.NewLine + "But not Kal-El.");
                Configuration.Settings.General.ContinuationStyle = ContinuationStyle.LeadingTrailingDots;
                new RemoveDialogFirstLineInNonDialogs().Fix(_subtitle, new EmptyFixCallback());
                Assert.AreEqual("They wanted to test!" + Environment.NewLine + "But not Kal-El.", _subtitle.Paragraphs[0].Text);
            }
        }
    }
}
