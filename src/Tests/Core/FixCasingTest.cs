using System;
using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;

namespace Tests.Core
{
    [TestClass]
    public class HtmFixCasingTestlUtilTest
    {
        [TestMethod]
        public void FixStutter1()
        {
            var fc = new FixCasing("en");
            var s = new Subtitle(new List<Paragraph> { new Paragraph("W-what time is it? W-what time is it?", 0, 2000) });
            fc.FixMakeLowercase = false;
            fc.FixMakeUppercase = false;
            fc.FixNormal = true;
            fc.Fix(s);
            Assert.AreEqual("W-What time is it? W-What time is it?", s.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixStutter2()
        {
            var fc = new FixCasing("en");
            var s = new Subtitle(new List<Paragraph> { new Paragraph("W-w-what time is it? W-w-what time is it?", 0, 2000) });
            fc.FixMakeLowercase = false;
            fc.FixMakeUppercase = false;
            fc.FixNormal = true;
            fc.Fix(s);
            Assert.AreEqual("W-W-What time is it? W-W-What time is it?", s.Paragraphs[0].Text);
        }

        [TestMethod]
        public void FixCasingWithStandardText()
        {
            var fixCasing = new FixCasing("en");
            var output = fixCasing.Fix("this is for www.nikse.dk. thank you.", new List<string>(), false, true, true, "Bye.");
            Assert.AreEqual("This is for www.nikse.dk. Thank you.", output);
        }
        
        [TestMethod]
        public void FixCasingWithExclamationMark()
        {
            var fixCasing = new FixCasing("en");
            var output = fixCasing.Fix("this is for www.nikse.dk! thank you.", new List<string>(), false, true, true, "Bye.");
            Assert.AreEqual("This is for www.nikse.dk! Thank you.", output);
        }

        [TestMethod]
        public void FixCasingForWebAddress()
        {
            var fixCasing = new FixCasing("en");
            var output = fixCasing.Fix("www.nikse.dk", new List<string>(), false, true, true, "Bye.");
            Assert.AreEqual("www.nikse.dk", output);
        }

        [TestMethod]
        public void FixCasingForDialogue()
        {
            var fixCasing = new FixCasing("en");
            var output = fixCasing.Fix("- hi joe!" + Environment.NewLine + "- hi jane.", new List<string>(), false, true, true, "Bye.");
            Assert.AreEqual("- Hi joe!" + Environment.NewLine + "- Hi jane.", output);
        }

        [TestMethod]
        public void FixCasingForDialogueWithNames()
        {
            var fixCasing = new FixCasing("en");
            var output = fixCasing.Fix("- hi joe!" + Environment.NewLine + "- hi jane.", new List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.AreEqual("- Hi Joe!" + Environment.NewLine + "- Hi Jane.", output);
        }

        [TestMethod]
        public void FixCasingForNewsreelNarrator()
        {
            var fixCasing = new FixCasing("en");
            var output = fixCasing.Fix("[ newsreel narrator ] ominous clouds of war.", new List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.AreEqual("[ Newsreel narrator ] Ominous clouds of war.", output);
        }

        [TestMethod]
        public void FixCasingForCharacterDialogue()
        {
            var fixCasing = new FixCasing("en");
            var output = fixCasing.Fix("andy: dad!", new List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.AreEqual("Andy: Dad!", output);
        }

        [TestMethod]
        public void FixCasingForCharacterDialogueWithExclamations()
        {
            var fixCasing = new FixCasing("en");
            var output = fixCasing.Fix("- quit! wait outside!" + Environment.NewLine + "- girl: miss, i've got a headache.", new List<string> { "Joe", "Jane" }, true, true, true, "Bye.");
            Assert.AreEqual("- Quit! Wait outside!" + Environment.NewLine + "- Girl: Miss, i've got a headache.", output);
        }

        [TestMethod]
        public void FixCasingForCharacterDialogueWithQuotes()
        {
            var fixCasing = new FixCasing("en");
            var output = fixCasing.Fix("Uh, “thor and doctor jones”", new List<string> { "Thor", "Jones" }, true, true, true, "Bye.");
            Assert.AreEqual("Uh, “Thor and doctor Jones”", output);
        }

        [TestMethod]
        public void FixCasingForEllipsis()
        {
            var fixCasing = new FixCasing("en");
            var output = fixCasing.Fix("…but never could.", new List<string>(), true, true, true, "Bye.");
            Assert.AreEqual("…but never could.", output);
        }

    }
}
