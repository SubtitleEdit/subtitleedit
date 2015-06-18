using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Logic.Forms;

namespace Test.Logic.Forms
{
    /// <summary>
    ///     This is a test class for FormRemoveTextForHearImpairedTest and is intended
    ///     to contain all FormRemoveTextForHearImpairedTest Unit Tests
    /// </summary>
    [TestClass]
    public class RemoveTextForHearImpairedTest
    {

        /// <summary>
        ///     Gets or sets the test context which provides
        ///     information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        private readonly RemoveTextForHI Target = GetRemoveTextForHiLib();
        private static RemoveTextForHI GetRemoveTextForHiLib()
        {
            var hiSettings = new RemoveTextForHISettings();
            return new RemoveTextForHI(hiSettings);
        }

        /// <summary>
        ///     A test for RemoveColon
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "Man over P.A.:\r\nGive back our homes.";
            const string expected = "Give back our homes.";
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest2A()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "GIOVANNI: <i>Number 9: I never look for a scapegoat.</i>";
            const string expected = "<i>I never look for a scapegoat.</i>";
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest2B()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            const string text = "GIOVANNI: <i>Number 9: I never look for a scapegoat.</i>";
            const string expected = "<i>Number 9: I never look for a scapegoat.</i>";
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest3()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "Barry, remember: She cannot\r\nteleport if she cannot see.";
            const string expected = text;
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest4()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "http://subscene.com/u/659433\r\nImproved by: @Ivandrofly";
            const string expected = text;
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest5()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "Okay! Narrator: Hello!";
            const string expected = text;
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest6()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "<i>♪ (THE CAPITOLS: \"COOL JERK\") ♪</i>";
            const string expected = text;
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///     A test for RemoveHIInsideLine
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHiInsideLine()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBetweenParentheses = true;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "Be quiet. (SHUSHING) It's okay.";
            const string expected = "Be quiet. It's okay.";
            string actual = Target.RemoveHearImpairedtagsInsideLine(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///     A test for RemoveHI
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHi1()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBetweenSquares = true;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "- Aw, save it. Storm?\r\n- [Storm]\r\nWe're outta here.";
            const string expected = "- Aw, save it. Storm?\r\n- We're outta here.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///     A test for RemoveHI
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHi2()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBetweenSquares = true;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "[Chuckles,\r\nCoughing]\r\nBut we lived through it.";
            const string expected = "But we lived through it.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///     A test for no removal
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHiNot()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "is the body of a mutant kid\r\non the 6:00 news.";
            const string expected = "is the body of a mutant kid\r\non the 6:00 news.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///     A test for RemoveHI
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHiMultilineItalic()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "<i>NARRATOR:" + Environment.NewLine +
                          "Previously on NCIS</i>";
            const string expected = "<i>Previously on NCIS</i>";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///     A test for RemoveHI
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHiMultilineBold()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "<b>NARRATOR:" + Environment.NewLine +
                          "Previously on NCIS</b>";
            const string expected = "<b>Previously on NCIS</b>";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///     A test for RemoveHI
        /// </summary>
        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHiSecondLineDelay()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "- JOHN: Hey." + Environment.NewLine +
                          "- ...hey.";
            string expected = "- Hey." + Environment.NewLine + "- ...hey.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHiQuotes()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "- Where?!" + Environment.NewLine + "- Ow!";
            const string expected = "Where?!";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHiDouble()
        {

            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBetweenSquares = true;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "[MAN]Where?![MAN]";
            const string expected = "Where?!";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveRemoveNameOfFirstLine()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "HECTOR: Hi." + Environment.NewLine + "-Oh, hey, Hector.";
            string expected = "- Hi." + Environment.NewLine + "- Oh, hey, Hector.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveRemoveNameOfFirstLineBold()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "<b>HECTOR: Hi.</b>";
            const string expected = "<b>Hi.</b>";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "-Ballpark." + Environment.NewLine + "-Hmm.";
            const string expected = "Ballpark.";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections2()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "-Ballpark." + Environment.NewLine + "-Mm-hm.";
            const string expected = "Ballpark.";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections3()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "-Mm-hm." + Environment.NewLine + "-Ballpark.";
            const string expected = "Ballpark.";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections4()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "- Mm-hm." + Environment.NewLine + "- Ballpark.";
            const string expected = "Ballpark.";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections5()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "- Ballpark." + Environment.NewLine + "- Hmm.";
            const string expected = "Ballpark.";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections6A()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "Ballpark, mm-hm.";
            const string expected = "Ballpark.";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections6B()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "Mm-hm, Ballpark.";
            const string expected = "Ballpark.";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections6BItalic()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "<i>Mm-hm, Ballpark.</i>";
            const string expected = "<i>Ballpark.</i>";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections7()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "You like her, huh?";
            const string expected = "You like her?";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections8()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "You like her, huh!";
            const string expected = "You like her!";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections9()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "You like her, huh.";
            const string expected = "You like her.";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections10()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "- You like her, huh." + Environment.NewLine + "- I do";
            string expected = "- You like her." + Environment.NewLine + "- I do";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections10Italic()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "<i>- You like her, huh." + Environment.NewLine + "- I do</i>";
            string expected = "<i>- You like her." + Environment.NewLine + "- I do</i>";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections11()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "- Ballpark, mm-hm." + Environment.NewLine + "- Oh yes!";
            string expected = "- Ballpark." + Environment.NewLine + "- Yes!";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections12()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            const string text = "Well, boy, I'm — Uh —";
            const string expected = "Well, boy, I'm —";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections13()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "- What?" + Environment.NewLine + "- Uh —";
            const string expected = "What?";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonOnlyOnSeparateLine()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = true;
            const string text = "HECTOR: Hi.";
            const string expected = "HECTOR: Hi.";
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfAllUppercase1()
        {
            Target.Settings.RemoveIfAllUppercase = true;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "HECTOR " + Environment.NewLine + "Hi.";
            const string expected = "Hi.";
            string actual = Target.RemoveLineIfAllUppercase(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfAllUppercase2()
        {
            Target.Settings.RemoveIfAllUppercase = true;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "Please, Mr Krook." + Environment.NewLine + "SHOP DOOR BELL CLANGS";
            const string expected = "Please, Mr Krook.";
            string actual = Target.RemoveLineIfAllUppercase(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfAllUppercase3()
        {
            Target.Settings.RemoveIfAllUppercase = true;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBetweenParentheses = true;
            string text = "(<i>GOIN' BACK TO INDIANA</i>" + Environment.NewLine + "CONTINUES PLAYING)";
            const string expected = "";
            string actual = Target.RemoveLineIfAllUppercase(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfParentheses3()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBetweenParentheses = true;
            string text = "(<i>GOIN' BACK TO INDIANA</i>" + Environment.NewLine + "CONTINUES PLAYING)";
            const string expected = "";
            string actual = Target.RemoveHearImpairedTags(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBeforeColonSecondLine()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "- even if it was one week." + Environment.NewLine + "CANNING: Objection.";
            string expected = "- even if it was one week." + Environment.NewLine + "- Objection.";
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBeforeColonOnlyUpper1()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            Target.Settings.ColonSeparateLine = false;
            const string text = "RACHEL <i>&</i> ROSS: Hi there!";
            const string expected = "Hi there!";
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBeforeColonOnlyUpper2()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            Target.Settings.ColonSeparateLine = false;
            const string text = "RACHEL AND ROSS: Hi there!";
            const string expected = "Hi there!";
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBeforeColonOnlyUpper3Negative()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            Target.Settings.ColonSeparateLine = false;
            const string text = "RACHEL and ROSS: Hi there!";
            const string expected = "RACHEL and ROSS: Hi there!";
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextAss1()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBetweenParentheses = true;
            const string text = "{\\an4\\pos(691,748)}(Chuckles) Yes, ok";
            const string expected = "{\\an4\\pos(691,748)}Yes, ok";
            string actual = Target.RemoveHearImpairedTags(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextAss2()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBetweenParentheses = true;
            const string text = "{\\an4\\pos(691,748)}(radio noise)";
            string expected = string.Empty;
            string actual = Target.RemoveHearImpairedTags(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextAss3Colon()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBetweenParentheses = true;
            Target.Settings.RemoveTextBeforeColon = true;
            const string text = "{\\an4\\pos(1335,891)}NIC: Shh!";
            const string expected = "{\\an4\\pos(1335,891)}Shh!";
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBeforeColonTest1()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBetweenParentheses = true;
            Target.Settings.RemoveTextBeforeColon = true;
            const string text = "SKOTT AVFYRADE: 760\r\nFORDON FÖRSTÖRDA: 12";
            const string expected = "760\r\n12";
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveDashBeforeSquareBrackets()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBetweenBrackets = true;
            Target.Settings.RemoveTextBeforeColon = true;
            string text = "- I insist." + Environment.NewLine + "<i>- [ Woman Laughing]</i>";
            const string expected = "I insist.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveDashInRemoveInterjections()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBetweenBrackets = true;
            Target.Settings.RemoveTextBeforeColon = true;
            string text = "- Oh." + Environment.NewLine + "<i>- Yes, sure.</i>";
            const string expected = "<i>Yes, sure.</i>";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveSingleDash()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBetweenBrackets = true;
            Target.Settings.RemoveTextBeforeColon = true;
            string text = "WOMAN: A glass of champagne, please." + Environment.NewLine + "- (Laughter)";
            const string expected = "A glass of champagne, please.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBetweenQuestionMarks()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = false;
            Target.Settings.RemoveTextBeforeColon = false;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            Target.Settings.RemoveTextBetweenBrackets = true;
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.RemoveTextBetweenQuestionMarks = true;
            string text = "? My Paul ?" + Environment.NewLine + "? I give you all ?";
            string expected = string.Empty;
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjectionsItalicFirstLine()
        {
            Target.Settings.RemoveInterjections = true;
            string text = "<i>- Mm-hmm.</i>" + Environment.NewLine + "- In my spare time.";
            const string expected = "In my spare time.";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjectionsItalicSecondLine()
        {

            Target.Settings.RemoveInterjections = true;
            string text = "- In my spare time." + Environment.NewLine + "<i>- Mm-hmm.</i>";
            const string expected = "In my spare time.";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjectionsItalicBothLines()
        {
            Target.Settings.RemoveInterjections = true;
            string text = "<i>- In my spare time.</i>" + Environment.NewLine + "<i>- Mm-hmm.</i>";
            const string expected = "<i>In my spare time.</i>";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjectionsMDash()
        {
            Target.Settings.RemoveInterjections = true;
            const string text = "I'm sorry. I, mm-hmm—";
            const string expected = "I'm sorry. I—";
            string actual = Target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveFirstDashItalics()
        {
            Target.Settings.RemoveTextBetweenBrackets = true;
            string text = "<i>- A man who wants to make his mark..." + Environment.NewLine + "- [ Coughing]</i>";
            const string expected = "<i>A man who wants to make his mark...</i>";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextIfUppercaseNotEmdash()
        {
            Target.Settings.RemoveIfAllUppercase = true;
            string text = "- And you?" + Environment.NewLine + "- I—";
            string expected = "- And you?" + Environment.NewLine + "- I—";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextDontCrashOnEmptyString()
        {
            Target.Settings.RemoveIfAllUppercase = true;
            string text = string.Empty;
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(text, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextKeepMusicSymbolsButRemoveHi()
        {
            Target.Settings.RemoveTextBetweenCustomTags = false;
            Target.Settings.RemoveTextBetweenBrackets = true;
            Target.Settings.RemoveIfTextContains = null;
            const string text = "<i>♪♪[Ambient Electronic]</i>";
            const string expected = "<i>♪♪</i>";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveEmdash()
        {
            Target.Settings.RemoveTextBetweenCustomTags = false;
            Target.Settings.RemoveTextBetweenBrackets = true;
            Target.Settings.RemoveIfTextContains = null;
            Target.Settings.RemoveInterjections = true;
            const string text = "Oh — Oh, that's nice!";
            const string expected = "That's nice!";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextIfUppercaseEmdashRemoveInDialogue()
        {
            Target.Settings.RemoveTextBetweenCustomTags = false;
            Target.Settings.RemoveInterjections = true;
            string text = "- Uh—uh, my God!" + Environment.NewLine + "- Uh, my God.";
            string expected = "- My God!" + Environment.NewLine + "- My God.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextIfUppercaseEmdashRemoveInDialogueWithSpaces()
        {
            Target.Settings.RemoveTextBetweenCustomTags = false;
            Target.Settings.RemoveInterjections = true;
            string text = "- Uh — uh, my God!" + Environment.NewLine + "- Uh, my God.";
            string expected = "- My God!" + Environment.NewLine + "- My God.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextEmdashComma()
        {
            Target.Settings.RemoveTextBetweenCustomTags = false;
            Target.Settings.RemoveInterjections = true;
            string text = "- I just, uh —" + Environment.NewLine + "- What?";
            string expected = "- I just —" + Environment.NewLine + "- What?";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextHiAndColon()
        {
            Target.Settings.RemoveTextBetweenCustomTags = false;
            Target.Settings.RemoveTextBetweenParentheses = true;
            const string text = "I'm trying to! (MASTER): Faster now. evacuate.";
            const string expected = "I'm trying to! Faster now. evacuate.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextDontLeaveDoubleDash()
        {
            Target.Settings.RemoveTextBetweenCustomTags = false;
            Target.Settings.RemoveInterjections = true;
            string text = "- Mr. Harding?" + Environment.NewLine + "Uh--";
            const string expected = "Mr. Harding?";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextDontLeaveDot()
        {

            Target.Settings.RemoveTextBetweenCustomTags = false;
            Target.Settings.RemoveInterjections = true;
            string text = "- Mr. Harding?" + Environment.NewLine + "- Mm-hm. Oh.";
            const string expected = "Mr. Harding?";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextDontLeaveExl()
        {
            Target.Settings.RemoveTextBetweenCustomTags = false;
            Target.Settings.RemoveInterjections = true;
            string text = "-Sit down. Sit down." + Environment.NewLine + "-Oh! Oh!";
            const string expected = "Sit down. Sit down.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRememberDialogueTag()
        {

            Target.Settings.RemoveTextBetweenCustomTags = false;
            Target.Settings.RemoveInterjections = true;
            string text = "Oh." + Environment.NewLine + "-I'm awfully tired.";
            const string expected = "I'm awfully tired.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveLineWithColon()
        {
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "Before:";
            string expected = string.Empty;
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextRemoveLineWithColon2()
        {

            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "COP 1: Call it in, code four. COP 4: Get him out of here.";
            const string expected = "Call it in, code four. Get him out of here.";
            string actual = Target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextSpeakerWithColonPlusLineWithHyphen()
        {
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.RemoveInterjections = false;
            string text = "WOMAN: <i>Mr. Sportello?</i>" + Environment.NewLine + "- Mm-hm.";
            string expected = "<i>- Mr. Sportello?</i>" + Environment.NewLine + "- Mm-hm.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextSpeakerWithColonPlusLineWithHyphenAlsoRemoveInterjections()
        {
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.RemoveInterjections = true;
            string text = "WOMAN: <i>Mr. Sportello?</i>" + Environment.NewLine + "- Mm-hm.";
            string expected = "<i>Mr. Sportello?</i>";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextQuotesInFirstLine()
        {
            Target.Settings.RemoveTextBeforeColon = true;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.RemoveInterjections = true;
            string text = "- \"My father doesn't want me to be him.\"" + Environment.NewLine + "EAMES: Exactly.";
            string expected = "- \"My father doesn't want me to be him.\"" + Environment.NewLine + "- Exactly.";
            string actual = Target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextQuotesInFirstLine2()
        {
            Target.Settings.RemoveIfAllUppercase = false;
            Target.Settings.RemoveInterjections = true;
            Target.Settings.OnlyIfInSeparateLine = false;
            Target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            Target.Settings.ColonSeparateLine = false;
            string text = "- \"Ballpark.\"" + Environment.NewLine + "-Hmm.";
            const string expected = "\"Ballpark.\"";
            string actual = Target.RemoveInterjections(text);
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
