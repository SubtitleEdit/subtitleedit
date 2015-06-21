using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Logic.Forms;

namespace Test.Logic.Forms
{
    /// <summary>
    /// This is a test class for FormRemoveTextForHearImpairedTest and is intended
    /// to contain all FormRemoveTextForHearImpairedTest Unit Tests
    /// </summary>
    [TestClass]
    public class RemoveTextForHearImpairedTest
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private static RemoveTextForHI GetRemoveTextForHiLib()
        {
            var hiSettings = new RemoveTextForHISettings();
            return new RemoveTextForHI(hiSettings);
        }

        /// <summary>
        /// A test for RemoveColon
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "Man over P.A.:\r\nGive back our homes.";
            const string expected = "Give back our homes.";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest2A()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "GIOVANNI: <i>Number 9: I never look for a scapegoat.</i>";
            const string expected = "<i>I never look for a scapegoat.</i>";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest2B()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            const string text = "GIOVANNI: <i>Number 9: I never look for a scapegoat.</i>";
            const string expected = "<i>Number 9: I never look for a scapegoat.</i>";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest3()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "Barry, remember: She cannot\r\nteleport if she cannot see.";
            const string expected = text;
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest4()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "http://subscene.com/u/659433\r\nImproved by: @Ivandrofly";
            const string expected = text;
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest5()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "Okay! Narrator: Hello!";
            const string expected = text;
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest6()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "<i>♪ (THE CAPITOLS: \"COOL JERK\") ♪</i>";
            const string expected = text;
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for RemoveHIInsideLine
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHIInsideLine()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "Be quiet. (SHUSHING) It's okay.";
            const string expected = "Be quiet. It's okay.";
            string actual = target.RemoveHearImpairedtagsInsideLine(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for RemoveHI
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHI1()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "- Aw, save it. Storm?\r\n- [Storm]\r\nWe're outta here.";
            const string expected = "- Aw, save it. Storm?\r\n- We're outta here.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for RemoveHI
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHI2()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "[Chuckles,\r\nCoughing]\r\nBut we lived through it.";
            const string expected = "But we lived through it.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for no removal
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHINot()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "is the body of a mutant kid\r\non the 6:00 news.";
            const string expected = "is the body of a mutant kid\r\non the 6:00 news.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for RemoveHI
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHIMultilineItalic()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "<i>NARRATOR:" + Environment.NewLine +
                          "Previously on NCIS</i>";
            const string expected = "<i>Previously on NCIS</i>";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for RemoveHI
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHIMultilineBold()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "<b>NARRATOR:" + Environment.NewLine +
                          "Previously on NCIS</b>";
            const string expected = "<b>Previously on NCIS</b>";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for RemoveHI
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHISecondLineDelay()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "- JOHN: Hey." + Environment.NewLine + "- ...hey.";
            string expected = "- Hey." + Environment.NewLine + "- ...hey.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHIQuotes()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "- Where?!" + Environment.NewLine + "- Ow!";
            const string expected = "Where?!";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHIDouble()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "[MAN]Where?![MAN]";
            const string expected = "Where?!";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveRemoveNameOfFirstLine()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "HECTOR: Hi." + Environment.NewLine + "-Oh, hey, Hector.";
            string expected = "- Hi." + Environment.NewLine + "- Oh, hey, Hector.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveRemoveNameOfFirstLineBold()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "<b>HECTOR: Hi.</b>";
            const string expected = "<b>Hi.</b>";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "-Ballpark." + Environment.NewLine + "-Hmm.";
            const string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections2()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "-Ballpark." + Environment.NewLine + "-Mm-hm.";
            const string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections3()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "-Mm-hm." + Environment.NewLine + "-Ballpark.";
            const string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections4()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "- Mm-hm." + Environment.NewLine + "- Ballpark.";
            const string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections5()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "- Ballpark." + Environment.NewLine + "- Hmm.";
            const string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections6A()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "Ballpark, mm-hm.";
            const string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections6B()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "Mm-hm, Ballpark.";
            const string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections6BItalic()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "<i>Mm-hm, Ballpark.</i>";
            const string expected = "<i>Ballpark.</i>";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections7()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "You like her, huh?";
            const string expected = "You like her?";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections8()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "You like her, huh!";
            const string expected = "You like her!";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections9()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "You like her, huh.";
            const string expected = "You like her.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections10()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "- You like her, huh." + Environment.NewLine + "- I do";
            string expected = "- You like her." + Environment.NewLine + "- I do";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections10Italic()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "<i>- You like her, huh." + Environment.NewLine + "- I do</i>";
            string expected = "<i>- You like her." + Environment.NewLine + "- I do</i>";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections11()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "- Ballpark, mm-hm." + Environment.NewLine + "- Oh yes!";
            string expected = "- Ballpark." + Environment.NewLine + "- Yes!";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections12()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "Well, boy, I'm — Uh —";
            const string expected = "Well, boy, I'm —";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections13()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "- What?" + Environment.NewLine + "- Uh —";
            const string expected = "What?";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonOnlyOnSeparateLine()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = true;
            const string text = "HECTOR: Hi.";
            const string expected = "HECTOR: Hi.";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfAllUppercase1()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = true;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "HECTOR " + Environment.NewLine + "Hi.";
            const string expected = "Hi.";
            string actual = target.RemoveLineIfAllUppercase(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfAllUppercase2()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = true;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "Please, Mr Krook." + Environment.NewLine + "SHOP DOOR BELL CLANGS";
            const string expected = "Please, Mr Krook.";
            string actual = target.RemoveLineIfAllUppercase(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfAllUppercase3()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = true;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            string text = "(<i>GOIN' BACK TO INDIANA</i>" + Environment.NewLine + "CONTINUES PLAYING)";
            const string expected = "";
            string actual = target.RemoveLineIfAllUppercase(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfParentheses3()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            string text = "(<i>GOIN' BACK TO INDIANA</i>" + Environment.NewLine + "CONTINUES PLAYING)";
            const string expected = "";
            string actual = target.RemoveHearImpairedTags(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBeforeColonSecondLine()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "- even if it was one week." + Environment.NewLine + "CANNING: Objection.";
            string expected = "- even if it was one week." + Environment.NewLine + "- Objection.";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBeforeColonOnlyUpper1()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            target.Settings.ColonSeparateLine = false;
            const string text = "RACHEL <i>&</i> ROSS: Hi there!";
            const string expected = "Hi there!";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBeforeColonOnlyUpper2()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            target.Settings.ColonSeparateLine = false;
            const string text = "RACHEL AND ROSS: Hi there!";
            const string expected = "Hi there!";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBeforeColonOnlyUpper3Negative()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            target.Settings.ColonSeparateLine = false;
            const string text = "RACHEL and ROSS: Hi there!";
            const string expected = "RACHEL and ROSS: Hi there!";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextAss1()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            const string text = "{\\an4\\pos(691,748)}(Chuckles) Yes, ok";
            const string expected = "{\\an4\\pos(691,748)}Yes, ok";
            string actual = target.RemoveHearImpairedTags(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextAss2()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            const string text = "{\\an4\\pos(691,748)}(radio noise)";
            string expected = string.Empty;
            string actual = target.RemoveHearImpairedTags(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextAss3Colon()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            target.Settings.RemoveTextBeforeColon = true;
            const string text = "{\\an4\\pos(1335,891)}NIC: Shh!";
            const string expected = "{\\an4\\pos(1335,891)}Shh!";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBeforeColonTest1()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            target.Settings.RemoveTextBeforeColon = true;
            const string text = "SKOTT AVFYRADE: 760\r\nFORDON FÖRSTÖRDA: 12";
            const string expected = "760\r\n12";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveDashBeforeSquareBrackets()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveTextBeforeColon = true;
            string text = "- I insist." + Environment.NewLine + "<i>- [ Woman Laughing]</i>";
            const string expected = "I insist.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveDashInRemoveInterjections()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveTextBeforeColon = true;
            string text = "- Oh." + Environment.NewLine + "<i>- Yes, sure.</i>";
            const string expected = "<i>Yes, sure.</i>";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveSingleDash()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveTextBeforeColon = true;
            string text = "WOMAN: A glass of champagne, please." + Environment.NewLine + "- (Laughter)";
            const string expected = "A glass of champagne, please.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBetweenQuestionMarks()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBetweenQuestionMarks = true;
            string text = "? My Paul ?" + Environment.NewLine + "? I give you all ?";
            string expected = string.Empty;
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjectionsItalicFirstLine()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveInterjections = true;
            string text = "<i>- Mm-hmm.</i>" + Environment.NewLine + "- In my spare time.";
            const string expected = "In my spare time.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjectionsItalicSecondLine()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveInterjections = true;
            string text = "- In my spare time." + Environment.NewLine + "<i>- Mm-hmm.</i>";
            const string expected = "In my spare time.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjectionsItalicBothLines()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveInterjections = true;
            string text = "<i>- In my spare time.</i>" + Environment.NewLine + "<i>- Mm-hmm.</i>";
            const string expected = "<i>In my spare time.</i>";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjectionsMDash()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveInterjections = true;
            const string text = "I'm sorry. I, mm-hmm—";
            const string expected = "I'm sorry. I—";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveFirstDashItalics()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            string text = "<i>- A man who wants to make his mark..." + Environment.NewLine + "- [ Coughing]</i>";
            const string expected = "<i>A man who wants to make his mark...</i>";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextIfUppercaseNotEmdash()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = true;
            string text = "- And you?" + Environment.NewLine + "- I—";
            string expected = "- And you?" + Environment.NewLine + "- I—";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextDontCrashOnEmptyString()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = true;
            string text = string.Empty;
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(text, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextKeepMusicSymbolsButRemoveHI()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveIfTextContains = null;
            const string text = "<i>♪♪[Ambient Electronic]</i>";
            const string expected = "<i>♪♪</i>";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveEmdash()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveIfTextContains = null;
            target.Settings.RemoveInterjections = true;
            const string text = "Oh — Oh, that's nice!";
            const string expected = "That's nice!";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextIfUppercaseEmdashRemoveInDialogue()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "- Uh—uh, my God!" + Environment.NewLine + "- Uh, my God.";
            string expected = "- My God!" + Environment.NewLine + "- My God.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextIfUppercaseEmdashRemoveInDialogueWithSpaces()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "- Uh — uh, my God!" + Environment.NewLine + "- Uh, my God.";
            string expected = "- My God!" + Environment.NewLine + "- My God.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextEmdashComma()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "- I just, uh —" + Environment.NewLine + "- What?";
            string expected = "- I just —" + Environment.NewLine + "- What?";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextHiAndColon()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            const string text = "I'm trying to! (MASTER): Faster now. evacuate.";
            const string expected = "I'm trying to! Faster now. evacuate.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextDontLeaveDoubleDash()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "- Mr. Harding?" + Environment.NewLine + "Uh--";
            const string expected = "Mr. Harding?";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextDontLeaveDot()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "- Mr. Harding?" + Environment.NewLine + "- Mm-hm. Oh.";
            const string expected = "Mr. Harding?";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextDontLeaveExl()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "-Sit down. Sit down." + Environment.NewLine + "-Oh! Oh!";
            const string expected = "Sit down. Sit down.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRememberDialogueTag()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "Oh." + Environment.NewLine + "-I'm awfully tired.";
            const string expected = "I'm awfully tired.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveLineWithColon()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "Before:";
            string expected = string.Empty;
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveLineWithColon2()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "COP 1: Call it in, code four. COP 4: Get him out of here.";
            const string expected = "Call it in, code four. Get him out of here.";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextSpeakerWithColonPlusLineWithHyphen()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = false;
            string text = "WOMAN: <i>Mr. Sportello?</i>" + Environment.NewLine + "- Mm-hm.";
            string expected = "<i>- Mr. Sportello?</i>" + Environment.NewLine + "- Mm-hm.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextSpeakerWithColonPlusLineWithHyphenAlsoRemoveInterjections()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = true;
            string text = "WOMAN: <i>Mr. Sportello?</i>" + Environment.NewLine + "- Mm-hm.";
            string expected = "<i>Mr. Sportello?</i>";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextQuotesInFirstLine()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = true;
            string text = "- \"My father doesn't want me to be him.\"" + Environment.NewLine + "EAMES: Exactly.";
            string expected = "- \"My father doesn't want me to be him.\"" + Environment.NewLine + "- Exactly.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextQuotesInFirstLine2()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            string text = "- \"Ballpark.\"" + Environment.NewLine + "-Hmm.";
            const string expected = "\"Ballpark.\"";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiInDialogue1()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            string text = "-[gurgling]" + Environment.NewLine + "-Mom?";
            const string expected = "Mom?";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiInDialogue2()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            string text = "-[Ronnie laughs]" + Environment.NewLine + "-[sighs] Wow, Ronnie.";
            const string expected = "Wow, Ronnie.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiRemoveFontTag()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            const string text = "<font color=\"#808080\">[Whistling]</font> Hallo everybody!";
            const string expected = "Hallo everybody!";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiRemoveFontTag2()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            const string text = "♪ <font color=\"#000000\">[LIGHT SWITCH CLICKS]</font>";
            const string expected = "♪";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiRemoveFontTag3()
        {
            RemoveTextForHI target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            const string text = "Foobar <font color=\"#808080\">[CHAINS RATTLING]</font> Foobar";
            const string expected = "Foobar Foobar";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        #region Additional test attributes

        //
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
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
    }
}
