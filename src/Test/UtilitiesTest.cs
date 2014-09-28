using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Logic;

namespace Test
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
        [DeploymentItem("SubtitleEdit.exe")]
        public void AutoBreakLine3()
        {
            string s1 = "- We're gonna lose him." + Environment.NewLine + "- He's left him four signals in the last week.";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.AreEqual(s1, s2);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AutoBreakLineDialog1()
        {
            string s1 = "- Qu'est ce qui se passe ? - Je veux voir ce qu'ils veulent être.";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.AreEqual("- Qu'est ce qui se passe ?" + Environment.NewLine + "- Je veux voir ce qu'ils veulent être.", s2);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void AutoBreakLineDialog2()
        {
            string s1 = "- Je veux voir ce qu'ils veulent être. - Qu'est ce qui se passe ?";
            string s2 = Utilities.AutoBreakLine(s1);
            Assert.AreEqual("- Je veux voir ce qu'ils veulent être." + Environment.NewLine + "- Qu'est ce qui se passe ?", s2);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixInvalidItalicTags2()
        {
            string s1 = "Gledaj prema kameri i rici <i>zdravo!";
            string s2 = Utilities.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "Gledaj prema kameri i rici zdravo!");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixInvalidItalicTags3()
        {
            string s1 = "<i>Line 1.</i>" + Environment.NewLine + "<i>Line 2.";
            string s2 = Utilities.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>Line 1." + Environment.NewLine + "Line 2.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixInvalidItalicTags4()
        {
            string s1 = "It <i>is</i> a telegram," + Environment.NewLine + "it <i>is</i> ordering an advance,";
            string s2 = Utilities.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixInvalidItalicTags5()
        {
            string s1 = "- <i>It is a telegram?</i>" + Environment.NewLine + "<i>- It is.</i>";
            string s2 = Utilities.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>- It is a telegram?" + Environment.NewLine + "- It is.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixInvalidItalicTags6()
        {
            string s1 = "- <i>Text1!</i>" + Environment.NewLine + "- <i>Text2.</i>";
            string s2 = Utilities.FixInvalidItalicTags(s1);
            Assert.AreEqual(s2, "<i>- Text1!" + Environment.NewLine + "- Text2.</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesDoubleSpace1()
        {
            string s1 = "This is  a test";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "This is a test");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesDoubleSpace2()
        {
            string s1 = "This is a test  ";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "This is a test");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesItalics1()
        {
            string s1 = "<i> This is a test</i>";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "<i>This is a test</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesItalics2()
        {
            string s1 = "<i>This is a test </i>";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "<i>This is a test</i>");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesHyphen1()
        {
            string s1 = "This is a low- budget job";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "This is a low-budget job");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesHyphen2()
        {
            string s1 = "This is a low- budget job";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, "This is a low-budget job");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesHyphenDoNotChange1()
        {
            string s1 = "This is it - and he likes it!";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesHyphenDoNotChange2()
        {
            string s1 = "What are your long- and altitude stats?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesHyphenDoNotChange3()
        {
            string s1 = "Did you buy that first- or second-handed?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "en");
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesHyphenDoNotChangeDutch1()
        {
            string s1 = "Wat zijn je voor- en familienaam?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "nl");
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesHyphenDoNotChangeDutch2()
        {
            string s1 = "Was het in het voor- of najaar?";
            string s2 = Utilities.RemoveUnneededSpaces(s1, "nl");
            Assert.AreEqual(s2, s1);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesDialogDotDotDotLine1()
        {
            string s = Utilities.RemoveUnneededSpaces("- ... Careful", "en");
            Assert.AreEqual(s, "- ...Careful");
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixUnneededSpacesDialogDotDotDotLine2()
        {
            string s = Utilities.RemoveUnneededSpaces("- Hi!" + Environment.NewLine + "- ... Careful", "en");
            Assert.AreEqual(s, "- Hi!" + Environment.NewLine + "- ...Careful");
        }

    }
}
