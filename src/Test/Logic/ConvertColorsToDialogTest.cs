using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Logic
{
    [TestClass]
    public class ConvertColorsToDialogTest
    {
        [TestMethod]
        public void TestDialog1()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<font color=\"#ffff00\">That was really delicious.</font>" + Environment.NewLine + 
                                                                              "I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- That was really delicious." + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestDialog2()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("That's it!" + Environment.NewLine + 
                                                                              "<font color=\"#ffff00\">..sped to victory.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- That's it!" + Environment.NewLine + "- ..sped to victory.", result);
        }

        [TestMethod]
        public void TestDialog2Alt()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashSecondLineWithoutSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("That's it!" + Environment.NewLine +
                                                                              "<font color=\"#ffff00\">..sped to victory.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("That's it!" + Environment.NewLine + "-..sped to victory.", result);
        }

        [TestMethod]
        public void TestDialog3()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<font color=\"#ff0000\">That's it!</font>" + Environment.NewLine + 
                                                                              "<font color=\"#ffff00\">..sped to victory.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- That's it!" + Environment.NewLine + "- ..sped to victory.", result);
        }

        [TestMethod]
        public void TestDialog3Alt()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashSecondLineWithoutSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<font color=\"#ff0000\">That's it!</font>" + Environment.NewLine +
                                                                              "<font color=\"#ffff00\">..sped to victory.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("That's it!" + Environment.NewLine + "-..sped to victory.", result);
        }

        [TestMethod]
        public void TestDialog4()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("We are going to Ibiza." + Environment.NewLine +
                                                                              "<font color=\"#ffff00\">To where?</font> Ibiza. <font color=\"#ffff00\">Okay.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- We are going to Ibiza." + Environment.NewLine + "- To where? - Ibiza. - Okay.", result);
        }

        [TestMethod]
        public void TestDialog4Alt()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashSecondLineWithoutSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("We are going to Ibiza." + Environment.NewLine +
                                                                              "<font color=\"#ffff00\">To where?</font> Ibiza. <font color=\"#ffff00\">Okay.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("We are going to Ibiza." + Environment.NewLine + "-To where? -Ibiza. -Okay.", result);
        }

        [TestMethod]
        public void TestDialogAddNewLinesOff()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashSecondLineWithoutSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("Keep on going. <font color=\"#ffff00\">Okay.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("Keep on going. -Okay.", result);
        }

        [TestMethod]
        public void TestDialogAddNewLinesOn()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashSecondLineWithoutSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("Keep on going. <font color=\"#ffff00\">Okay.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, true, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("Keep on going." + Environment.NewLine + "-Okay.", result);
        }

        [TestMethod]
        public void TestDialogThreeLines1()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<font color=\"#00ffff\">Got a sinking feeling.</font>" + Environment.NewLine +
                                                                              "<font color=\"#00ff00\">\"Evacuate from</font>" + Environment.NewLine +
                                                                              "<font color=\"#00ff00\">an underwater chopper.\"</font> ", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- Got a sinking feeling." + Environment.NewLine + "- \"Evacuate from" + Environment.NewLine + "an underwater chopper.\"", result);
        }

        [TestMethod]
        public void TestDialogThreeLines2()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<font color=\"#00ffff\">Got a sinking feeling</font>" + Environment.NewLine +
                                                                              "<font color=\"#00ffff\">about all of this.</font>" + Environment.NewLine +
                                                                              "<font color=\"#00ff00\">Don't worry.</font> ", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- Got a sinking feeling" + Environment.NewLine + "about all of this." + Environment.NewLine + "- Don't worry.", result);
        }

        [TestMethod]
        public void TestDialogThreeLines2ReBreak()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<font color=\"#00ffff\">Got a sinking feeling</font>" + Environment.NewLine +
                                                                              "<font color=\"#00ffff\">about all of this.</font>" + Environment.NewLine +
                                                                              "<font color=\"#00ff00\">Don't worry.</font> ", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, true);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- Got a sinking feeling about all of this." + Environment.NewLine + "- Don't worry.", result);
        }

        [TestMethod]
        public void TestNoChange1()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<font color=\"#ffff00\">That's it!</font>" + Environment.NewLine +
                                                                              "<font color=\"#ffff00\">..sped to victory.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("That's it!" + Environment.NewLine + "..sped to victory.", result);
        }

        [TestMethod]
        public void TestNoChange2()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<font color=\"#ffff00\">That's it!</font> <font color=\"#ffff00\">Sped to victory.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("That's it! Sped to victory.", result);
        }

        [TestMethod]
        public void TestNoChange3()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<font color=\"#ffff00\">- That was really delicious.</font>" + Environment.NewLine +
                                                                              "- I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, false, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("<font color=\"#ffff00\">- That was really delicious.</font>" + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestNoChange3NoColor()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<font color=\"#ffff00\">- That was really delicious.</font>" + Environment.NewLine +
                                                                              "- I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- That was really delicious." + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestNoChange4()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("- <font color=\"#ffff00\">That was really delicious.</font>" + Environment.NewLine +
                                                                              "- I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, false, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- <font color=\"#ffff00\">That was really delicious.</font>" + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestNoChange4NoColor()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("- <font color=\"#ffff00\">That was really delicious.</font>" + Environment.NewLine +
                                                                              "- I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- That was really delicious." + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestNoChange5()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("- No, don't touch that-- - <font color=\"#ffff00\">That was stupid.</font>" + Environment.NewLine +
                                                                              "- I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, false, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- No, don't touch that-- - <font color=\"#ffff00\">That was stupid.</font>" + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestNoChange5NoColor()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("- No, don't touch that-- - <font color=\"#ffff00\">That was stupid.</font>" + Environment.NewLine +
                                                                              "- I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- No, don't touch that-- - That was stupid." + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestDialogInterruption()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("No, don't touch that-- <font color=\"#ffff00\">That was stupid.</font>" + Environment.NewLine +
                                                                              "I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, false, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- No, don't touch that-- - <font color=\"#ffff00\">That was stupid.</font>" + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestDialogInterruptionNoColor()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("No, don't touch that-- <font color=\"#ffff00\">That was stupid.</font>" + Environment.NewLine +
                                                                              "I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- No, don't touch that-- - That was stupid." + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestPositioning()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("{\\an8}<font color=\"#ffff00\">That was really delicious.</font>" + Environment.NewLine +
                                                                              "I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("{\\an8}- That was really delicious." + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestPositioning2()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("{\\an8}That was really delicious." + Environment.NewLine +
                                                                              "<font color=\"#ffff00\">I know.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("{\\an8}- That was really delicious." + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestPositioning3()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashSecondLineWithoutSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("{\\an8}<font color=\"#ffff00\">That was really delicious.</font>" + Environment.NewLine +
                                                                              "I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("{\\an8}That was really delicious." + Environment.NewLine + "-I know.", result);
        }

        [TestMethod]
        public void TestPositioning4()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashSecondLineWithoutSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("{\\an8}That was really delicious." + Environment.NewLine +
                                                                              "<font color=\"#ffff00\">I know.</font>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("{\\an8}That was really delicious." + Environment.NewLine + "-I know.", result);
        }

        [TestMethod]
        public void TestVttDialog1()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("<c.cyan>That was really delicious.</c>" + Environment.NewLine +
                                                                              "I know.", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- That was really delicious." + Environment.NewLine + "- I know.", result);
        }

        [TestMethod]
        public void TestVttDialog2()
        {
            Configuration.Settings.General.DialogStyle = Nikse.SubtitleEdit.Core.Enums.DialogType.DashBothLinesWithSpace;

            var subtitle = new Subtitle(new List<Paragraph>() { new Paragraph("That's it!" + Environment.NewLine +
                                                                              "<c.cyan>..sped to victory.</c>", 0, 2000) });

            ConvertColorsToDialogUtils.ConvertColorsToDialogInSubtitle(subtitle, true, false, false);
            var result = subtitle.Paragraphs.First().Text;

            Assert.AreEqual("- That's it!" + Environment.NewLine + "- ..sped to victory.", result);
        }
    }
}
