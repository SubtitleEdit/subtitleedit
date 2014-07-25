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
            var s = Utilities.AutoBreakLine("You have a private health insurance and life insurance." + Environment.NewLine + "A digital clone included.", 5, maxLength, 33, string.Empty);
            var arr = s.Replace(Environment.NewLine, "\n").Split('\n');
            Assert.AreEqual(2, arr.Length);
            Assert.IsFalse(arr[0].Length > maxLength);
            Assert.IsFalse(arr[1].Length > maxLength);
        }

        [TestMethod]
        public void AutoBreakLine2()
        {
            const int maxLength = 43;
            var s = Utilities.AutoBreakLine("We're gonna lose him." + Environment.NewLine + "He's left him four signals in the last week.", 5, maxLength, 33, string.Empty);
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



    }
}
