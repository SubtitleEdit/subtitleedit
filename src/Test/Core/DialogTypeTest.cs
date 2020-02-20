using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Enums;

namespace Test.Core
{
    [TestClass]
    public class DialogTypeTest
    {
        [TestMethod]
        public void FixSpacesDashBothLinesWithSpace1()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
            var result = splitMerge.FixSpaces($"-How are you?{Environment.NewLine}-I'm fine.");
            Assert.AreEqual($"- How are you?{Environment.NewLine}- I'm fine.", result);
        }

        [TestMethod]
        public void FixSpacesDashBothLinesWithSpace2()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
            var result = splitMerge.FixSpaces($"- How are you?{Environment.NewLine}- I'm fine.");
            Assert.AreEqual($"- How are you?{Environment.NewLine}- I'm fine.", result);
        }

        [TestMethod]
        public void FixSpacesDashBothLinesWithSpace3()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
            var result = splitMerge.FixSpaces($"{{\\an8}}-How are you?{Environment.NewLine}{{\\an8}}-I'm fine.");
            Assert.AreEqual($"{{\\an8}}- How are you?{Environment.NewLine}{{\\an8}}- I'm fine.", result);
        }

        [TestMethod]
        public void FixSpacesDashBothLinesWithSpace4()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
            var result = splitMerge.FixSpaces($"<i>-How are you?</i>{Environment.NewLine}<i>-I'm fine.</i>");
            Assert.AreEqual($"<i>- How are you?</i>{Environment.NewLine}<i>- I'm fine.</i>", result);
        }



        [TestMethod]
        public void FixSpacesDashBothLinesWithoutSpace1()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithoutSpace };
            var result = splitMerge.FixSpaces($"- How are you?{Environment.NewLine}- I'm fine.'");
            Assert.AreEqual($"-How are you?{Environment.NewLine}-I'm fine.'", result);
        }

        [TestMethod]
        public void FixSpacesDashBothLinesWithoutSpace2()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithoutSpace };
            var result = splitMerge.FixSpaces($"-How are you?{Environment.NewLine}-I'm fine.'");
            Assert.AreEqual($"-How are you?{Environment.NewLine}-I'm fine.'", result);
        }



        [TestMethod]
        public void FixSpacesDashSecondLineWithSpace1()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithSpace };
            var result = splitMerge.FixSpaces($"How are you?{Environment.NewLine}-I'm fine.'");
            Assert.AreEqual($"How are you?{Environment.NewLine}- I'm fine.'", result);
        }

        [TestMethod]
        public void FixSpacesDashSecondLineWithSpace2()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithSpace };
            var result = splitMerge.FixSpaces($"How are you?{Environment.NewLine}- I'm fine.'");
            Assert.AreEqual($"How are you?{Environment.NewLine}- I'm fine.'", result);
        }


        [TestMethod]
        public void FixSpacesDashSecondLineWithoutSpace1()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithoutSpace };
            var result = splitMerge.FixSpaces($"How are you?{Environment.NewLine}- I'm fine.'");
            Assert.AreEqual($"How are you?{Environment.NewLine}-I'm fine.'", result);
        }

        [TestMethod]
        public void FixSpacesDashSecondLineWithoutSpace2()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithoutSpace };
            var result = splitMerge.FixSpaces($"How are you?{Environment.NewLine}-I'm fine.'");
            Assert.AreEqual($"How are you?{Environment.NewLine}-I'm fine.'", result);
        }



        [TestMethod]
        public void FixDashesDashBothLinesWithSpace1()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
            var result = splitMerge.FixDashes($"How are you?{Environment.NewLine}- I'm fine.");
            Assert.AreEqual($"- How are you?{Environment.NewLine}- I'm fine.", result);
        }


        [TestMethod]
        public void FixDashesDashBothLinesWithoutSpace1()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithoutSpace };
            var result = splitMerge.FixDashes($"How are you?{Environment.NewLine}-I'm fine.");
            Assert.AreEqual($"-How are you?{Environment.NewLine}-I'm fine.", result);
        }

        [TestMethod]
        public void FixDashesDashSecondLineWithSpace1()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithSpace };
            var result = splitMerge.FixDashes($"- How are you?{Environment.NewLine}- I'm fine.");
            Assert.AreEqual($"How are you?{Environment.NewLine}- I'm fine.", result);
        }

        [TestMethod]
        public void FixDashesDashSecondLineWithoutSpace1()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashSecondLineWithoutSpace };
            var result = splitMerge.FixDashes($"-How are you?{Environment.NewLine}-I'm fine.");
            Assert.AreEqual($"How are you?{Environment.NewLine}-I'm fine.", result);
        }



        [TestMethod]
        public void FixHyphensAddDash1()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
            var result = splitMerge.FixDashes($"Hi Joe!{Environment.NewLine}- Hi Pete!");
            Assert.AreEqual($"- Hi Joe!{Environment.NewLine}- Hi Pete!", result);
        }

        [TestMethod]
        public void FixHyphensWithQuotes()
        {
            var splitMerge = new DialogSplitMerge { DialogStyle = DialogType.DashBothLinesWithSpace };
            var result = splitMerge.FixDashes($"\"Into The Woods.\"{Environment.NewLine}- \"Sweeney Todd.\"");
            Assert.AreEqual($"- \"Into The Woods.\"{Environment.NewLine}- \"Sweeney Todd.\"", result);
        }
    }
}
