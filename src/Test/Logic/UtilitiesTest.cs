using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Forms.FixCommonErrors;
using System;

namespace Test.Logic
{
    [TestClass]
    public class UtilitiesTest
    {
        [TestMethod]
        public void AutoBreakLine1()
        {
            const int maxLength = 43;
            var s = Utilities.AutoBreakLine("You have a private health insurance and life insurance." + Environment.NewLine + "A digital clone included.", 5, 33, string.Empty);
            var arr = s.Replace(Environment.NewLine, "\n").Split('\n');
            Assert.AreEqual(2, arr.Length);
            Assert.IsFalse(arr[0].Length > maxLength);
            Assert.IsFalse(arr[1].Length > maxLength);
        }

        [TestMethod]
        public void AutoBreakLine2()
        {
            var s = Utilities.AutoBreakLine("We're gonna lose him." + Environment.NewLine + "He's left him four signals in the last week.", 5, 33, string.Empty);
            Assert.IsFalse(s == "We're gonna lose him." + Environment.NewLine + "He's left him four signals in the last week.");
        }

        [TestMethod]
        public void AutoBreakLine3()
        {
            string s1 = "- We're gonna lose him." + Environment.NewLine + "- He's left him four signals in the last week.";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void AutoBreakLine4()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            const string s1 = "- Seriously, though. Are you being bullied? - Nope.";
            string s2 = Utilities.AutoBreakLine(s1);
            string target = "- Seriously, though. Are you being bullied?" + Environment.NewLine + "- Nope.";
            Assert.AreEqual(target, s2);
        }

        [TestMethod]
        public void AutoBreakLine5()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            const string s1 = "<i>30 years ago I'd found</i> The Book of the Dead.";
            var s2 = Utilities.AutoBreakLine(s1);
            var expected = "<i>30 years ago I'd found</i>" + Environment.NewLine + "The Book of the Dead.";
            Assert.AreEqual(expected, s2);
        }

        [TestMethod]
        public void AutoBreakLine5DoNoBreakAtPeriod()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            const string s1 = "Oh, snap, we're still saying the same thing. This is amazing!";
            string s2 = Utilities.AutoBreakLine(s1);
            string target = "Oh, snap, we're still saying the" + Environment.NewLine + "same thing. This is amazing!";
            Assert.AreEqual(target, s2);
        }

        [TestMethod]
        public void AutoBreakLineDoNotBreakAfterDashDash()
        {
            Configuration.Settings.General.SubtitleLineMaximumLength = 43;
            string s1 = "- That's hilarious, I don't--" + Environment.NewLine + "- Are the cheeks turning nice and pink?";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        public void AutoBreakLineDialog1()
        {
            const string s1 = "- Qu'est ce qui se passe ? - Je veux voir ce qu'ils veulent être.";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.AreEqual("- Qu'est ce qui se passe ?" + Environment.NewLine + "- Je veux voir ce qu'ils veulent être.", s2);
        }

        [TestMethod]
        public void AutoBreakLineDialog2()
        {
            const string s1 = "- Je veux voir ce qu'ils veulent être. - Qu'est ce qui se passe ?";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.AreEqual("- Je veux voir ce qu'ils veulent être." + Environment.NewLine + "- Qu'est ce qui se passe ?", s2);
        }

        [TestMethod]
        public void UnBreakLine1()
        {
            string s = Utilities.UnbreakLine("Hallo!" + Environment.NewLine + "Hallo!");
            Assert.AreEqual("Hallo! Hallo!", s);
        }

        [TestMethod]
        public void UnBreakLine2()
        {
            string s = Utilities.UnbreakLine("Hallo!\nHallo!");
            Assert.AreEqual("Hallo! Hallo!", s);
        }

        [TestMethod]
        public void UnBreakLine3()
        {
            string s = Utilities.UnbreakLine("Hallo!\r\nHallo!");
            Assert.AreEqual("Hallo! Hallo!", s);
        }

        [TestMethod]
        public void UnBreakLine4()
        {
            string s = Utilities.UnbreakLine("Hallo! \nHallo!");
            Assert.AreEqual("Hallo! Hallo!", s);
        }

        [TestMethod]
        public void FixInvalidItalicTags2()
        {
            const string s1 = "Gledaj prema kameri i rici <i>zdravo!";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "Gledaj prema kameri i rici zdravo!");
        }

        [TestMethod]
        public void FixInvalidItalicTags3()
        {
            string s1 = "<i>Line 1.</i>" + Environment.NewLine + "<i>Line 2.";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>Line 1." + Environment.NewLine + "Line 2.</i>");
        }

        [TestMethod]
        public void FixInvalidItalicTags4()
        {
            string s1 = "It <i>is</i> a telegram," + Environment.NewLine + "it <i>is</i> ordering an advance,";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        public void FixInvalidItalicTags5()
        {
            string s1 = "- <i>It is a telegram?</i>" + Environment.NewLine + "<i>- It is.</i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>- It is a telegram?" + Environment.NewLine + "- It is.</i>");
        }

        [TestMethod]
        public void FixInvalidItalicTags6()
        {
            string s1 = "- <i>Text1!</i>" + Environment.NewLine + "- <i>Text2.</i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>- Text1!" + Environment.NewLine + "- Text2.</i>");
        }

        [TestMethod]
        public void FixInvalidItalicTags7()
        {
            string s1 = "<i>- You think they're they gone?<i>" + Environment.NewLine + "<i>- That can't be.</i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>- You think they're they gone?" + Environment.NewLine + "- That can't be.</i>");
        }

        [TestMethod]
        public void FixInvalidItalicTags8()
        {
            string s1 = "<i>- You think they're they gone?</i>" + Environment.NewLine + "<i>- That can't be.<i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>- You think they're they gone?" + Environment.NewLine + "- That can't be.</i>");
        }

        [TestMethod]
        public void FixInvalidItalicTags9()
        {
            const string s1 = "FALCONE:<i> I didn't think</i>\r\n<i>it was going to be you,</i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "FALCONE: <i>I didn't think\r\nit was going to be you,</i>");
        }

        [TestMethod]
        public void FixInvalidItalicTags10()
        {
            const string s1 = "< I>Hallo!</I>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>Hallo!</i>");
        }

        [TestMethod]
        public void FixInvalidItalicTags11()
        {
            const string s1 = "< I >Hallo!< /I>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>Hallo!</i>");
        }

        [TestMethod]
        public void FixInvalidItalicTags12()
        {
            const string s1 = "< I >Hallo!<I/>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>Hallo!</i>");
        }

        [TestMethod]
        public void FixInvalidItalicTags13()
        {
            var s1 = "<i>Hallo!</i>" + Environment.NewLine + "<i>Hallo!</i>" + Environment.NewLine + "<i>Hallo!</i>";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        public void FixInvalidItalicTags14()
        {
            var s1 = "<i>Hallo!<i/>" + Environment.NewLine + "<i>Hallo!<i/>" + Environment.NewLine + "<i>Hallo!";
            string s2 = HtmlUtil.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>Hallo!" + Environment.NewLine + "Hallo!" + Environment.NewLine + "Hallo!</i>");
        }

        [TestMethod]
        public void FixUnneededSpacesDoubleSpace1()
        {
            const string s1 = "This is  a test";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "This is a test");
        }

        [TestMethod]
        public void FixUnneededSpacesDoubleSpace2()
        {
            const string s1 = "This is a test  ";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "This is a test");
        }

        [TestMethod]
        public void FixUnneededSpacesItalics1()
        {
            const string s1 = "<i> This is a test</i>";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "<i>This is a test</i>");
        }

        [TestMethod]
        public void FixUnneededSpacesItalics2()
        {
            const string s1 = "<i>This is a test </i>";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "<i>This is a test</i>");
        }

        [TestMethod]
        public void FixUnneededSpacesHyphen1()
        {
            const string s1 = "This is a low- budget job";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "This is a low-budget job");
        }

        [TestMethod]
        public void FixUnneededSpacesHyphen2()
        {
            const string s1 = "This is a low- budget job";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "This is a low-budget job");
        }

        [TestMethod]
        public void FixUnneededSpacesHyphenDoNotChange1()
        {
            const string s1 = "This is it - and he likes it!";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        public void FixUnneededSpacesHyphenDoNotChange2()
        {
            const string s1 = "What are your long- and altitude stats?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        public void FixUnneededSpacesHyphenDoNotChange3()
        {
            const string s1 = "Did you buy that first- or second-handed?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        public void FixUnneededSpacesHyphenDoNotChangeDutch1()
        {
            const string s1 = "Wat zijn je voor- en familienaam?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "nl");
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        public void FixUnneededSpacesHyphenDoNotChangeDutch2()
        {
            const string s1 = "Was het in het voor- of najaar?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "nl");
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        public void FixUnneededSpacesDialogDotDotDotLine1()
        {
            string s = Utilities.RemoveUnneededSpaces("- ... Careful", "en");
            Assert.AreEqual(s, "- ...Careful");
        }

        [TestMethod]
        public void FixUnneededSpacesDialogDotDotDotLine2()
        {
            string s = Utilities.RemoveUnneededSpaces("- Hi!" + Environment.NewLine + "- ... Careful", "en");
            Assert.AreEqual(s, "- Hi!" + Environment.NewLine + "- ...Careful");
        }

        [TestMethod]
        public void FixUnneededSpacesFontTag1()
        {
            string s = Utilities.RemoveUnneededSpaces("<font color=\"#808080\"> (PEOPLE SPEAKING INDISTINCTLY) </font>", "en");
            Assert.AreEqual(s, "<font color=\"#808080\">(PEOPLE SPEAKING INDISTINCTLY)</font>");
        }

        [TestMethod]
        public void FixUnneededSpacesFontTag2()
        {
            string s = Utilities.RemoveUnneededSpaces("Foobar\r\n<font color=\"#808080\"> (PEOPLE SPEAKING INDISTINCTLY) </font>", "en");
            Assert.AreEqual(s, "Foobar\r\n<font color=\"#808080\">(PEOPLE SPEAKING INDISTINCTLY)</font>");
        }

        [TestMethod]
        public void FixUnneededSpacesFontTag3()
        {
            string s = Utilities.RemoveUnneededSpaces("<FONT COLOR=\"#808080\">- Foobar! </FONT>\r\n<font color=\"#808080\"> (PEOPLE SPEAKING INDISTINCTLY) </font>", "en");
            Assert.AreEqual(s, "<font color=\"#808080\">- Foobar!</font>\r\n<font color=\"#808080\">(PEOPLE SPEAKING INDISTINCTLY)</font>");
        }

        [TestMethod]
        public void RemoveUnneededSpacesAfterQuote()
        {
            const string lang = "en";

            // variant 1
            string s = Utilities.RemoveUnneededSpaces("\" In five years the Corleone family\r\nwill be completely legitimate.\"", lang);
            Assert.AreEqual("\"In five years the Corleone family\r\nwill be completely legitimate.\"", s);

            // variant 2
            s = Utilities.RemoveUnneededSpaces("Foobar? \" Foobar\".", lang);
            Assert.AreEqual("Foobar? \"Foobar\".", s);
        }

        [TestMethod]
        public void CountTagInTextStringOneLetterString()
        {
            int count = Utilities.CountTagInText("HHH", "H");
            Assert.AreEqual(count, 3);
        }

        [TestMethod]
        public void CountTagInTextStringNotThere()
        {
            int count = Utilities.CountTagInText("HHH", "B");
            Assert.AreEqual(count, 0);
        }

        [TestMethod]
        public void CountTagInTextCharNormal()
        {
            int count = Utilities.CountTagInText("HHH", 'H');
            Assert.AreEqual(count, 3);
        }

        [TestMethod]
        public void CountTagInTextCharNotThere()
        {
            int count = Utilities.CountTagInText("HHH", 'B');
            Assert.AreEqual(count, 0);
        }

        [TestMethod]
        public void FixHyphensAddTest()
        {
            string test1 = "<font color=\"#008080\">- Foobar." + Environment.NewLine + "Foobar.</font>";
            string expected1 = "<font color=\"#008080\">- Foobar." + Environment.NewLine + "- Foobar.</font>";

            string test2 = "<i>Foobar.</i>" + Environment.NewLine + "- Foobar.";
            var expected2 = "<i>- Foobar.</i>" + Environment.NewLine + "- Foobar.";

            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph(test1, 0000, 11111));
            sub.Paragraphs.Add(new Paragraph(test2, 0000, 11111));

            string output1 = Helper.FixHyphensAdd(sub, 0, "en");
            string output2 = Helper.FixHyphensAdd(sub, 1, "en");

            Assert.AreEqual(output1, expected1);
            Assert.AreEqual(output2, expected2);
        }

        [TestMethod]
        public void FixHyphensAddTestAssTag()
        {
            string test1 = "{\\an5}- At least I was going back to Hawaii." + Environment.NewLine + "Woody.";
            string expected1 = "{\\an5}- At least I was going back to Hawaii." + Environment.NewLine + "- Woody.";
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph(test1, 0000, 11111));

            string output1 = Helper.FixHyphensAdd(sub, 0, "en");

            Assert.AreEqual(output1, expected1);
        }

        [TestMethod]
        public void FixHyphensAddTestItalic()
        {
            string test1 = "<i>- At least I was going back to Hawaii.</i>" + Environment.NewLine + "<i>Woody.</i>";
            string expected1 = "<i>- At least I was going back to Hawaii.</i>" + Environment.NewLine + "<i>- Woody.</i>";
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph(test1, 0000, 11111));

            string output1 = Helper.FixHyphensAdd(sub, 0, "en");

            Assert.AreEqual(output1, expected1);
        }

        [TestMethod]
        public void RemoveLineBreaks1()
        {
            string result = Utilities.RemoveLineBreaks("Hey" + Environment.NewLine + "you!");
            Assert.AreEqual(result, "Hey you!");
        }

        [TestMethod]
        public void RemoveLineBreaks2()
        {
            string result = Utilities.RemoveLineBreaks("<i>Foobar " + Environment.NewLine + "</i> foobar.");
            Assert.AreEqual(result, "<i>Foobar</i> foobar.");
        }

        [TestMethod]
        public void RemoveLineBreaks3()
        {
            string result = Utilities.RemoveLineBreaks("<i>Foobar " + Environment.NewLine + "</i>foobar.");
            Assert.AreEqual(result, "<i>Foobar</i> foobar.");
        }

        [TestMethod]
        public void RemoveLineBreaks4()
        {
            string result = Utilities.RemoveLineBreaks("<i>Hey</i>" + Environment.NewLine + "<i>you!</i>");
            Assert.AreEqual(result, "<i>Hey you!</i>");
        }

        [TestMethod]
        public void RemoveLineBreaks5()
        {
            string result = Utilities.RemoveLineBreaks("<i>Foobar" + Environment.NewLine + "</i>");
            Assert.AreEqual(result, "<i>Foobar</i>");
        }

        [TestMethod]
        public void IsValidRegexOk1()
        {
            Assert.IsTrue(Utilities.IsValidRegex(@"^(?![\s\S])"));
        }

        [TestMethod]
        public void IsValidRegexOk2()
        {
            Assert.IsTrue(Utilities.IsValidRegex(@"\d+"));
        }

        [TestMethod]
        public void IsValidRegexBad1()
        {
            Assert.IsFalse(Utilities.IsValidRegex(@"[\s\S(\()()(()\)"));
        }

        [TestMethod]
        public void ReverseNumbers1()
        {
            Assert.AreEqual(Utilities.ReverseNumbers("Hallo 009"), "Hallo 900");
        }

        [TestMethod]
        public void ReverseNumbers2()
        {
            Assert.AreEqual(Utilities.ReverseNumbers("Hallo 009 001 Bye"), "Hallo 900 100 Bye");
        }

        [TestMethod]
        public void ReverseStartAndEndingForRightToLeft1()
        {
            Assert.AreEqual(Utilities.ReverseStartAndEndingForRightToLeft("-I have a big head."), ".I have a big head-");
        }

        [TestMethod]
        public void ReverseStartAndEndingForRightToLeft2()
        {
            Assert.AreEqual(Utilities.ReverseStartAndEndingForRightToLeft("~So do I?"), "?So do I~");
        }

        [TestMethod]
        public void ReverseStartAndEndingForRightToLeft3()
        {
            Assert.AreEqual(Utilities.ReverseStartAndEndingForRightToLeft("+I do too!"), "!I do too+");
        }

        [TestMethod]
        public void ReverseStartAndEndingForRightToLeft4()
        {
            Assert.AreEqual(Utilities.ReverseStartAndEndingForRightToLeft("(Mom)" + Environment.NewLine + "What are you doing here?"), "(Mom)" + Environment.NewLine + "?What are you doing here");
        }

        [TestMethod]
        public void ReverseStartAndEndingForRightToLeft5()
        {
            Assert.AreEqual(Utilities.ReverseStartAndEndingForRightToLeft("{\\an8}+I do too!"), "{\\an8}!I do too+");
        }

        [TestMethod]
        public void ReverseStartAndEndingForRightToLeft6()
        {
            Assert.AreEqual(Utilities.ReverseStartAndEndingForRightToLeft("-I have a big head." + Environment.NewLine + "~So do I?" + Environment.NewLine + "+I do too!"),
                                                                          ".I have a big head-" + Environment.NewLine + "?So do I~" + Environment.NewLine + "!I do too+");
        }

    }
}
