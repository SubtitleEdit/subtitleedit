using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Enums;
using Nikse.SubtitleEdit.Core.Forms;
using System;

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
            var hiSettings = new RemoveTextForHISettings(new Subtitle());
            return new RemoveTextForHI(hiSettings);
        }

        private static InterjectionRemoveContext GetRemoveInterjectionContext(string text, bool onlyInSeparatedLine)
        {
            return new InterjectionRemoveContext
            {
                OnlySeparatedLines = onlyInSeparatedLine,
                Interjections = RemoveTextForHI.GetInterjectionList(),
                Text = text,
            };
        }

        /// <summary>
        /// A test for RemoveColon
        /// </summary>
        [TestMethod]
        public void RemoveColonTest()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveColonTest2A()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveColonTest2B()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveColonTest3()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveColonTest4()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveColonTest5()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveColonTest6()
        {
            var target = GetRemoveTextForHiLib();
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

        [TestMethod]
        public void RemoveColonTest7()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            string text = "Kelly has an eating" + Environment.NewLine + "disorder? Michael: Yes.";
            string expected = "- Kelly has an eating" + Environment.NewLine + "disorder? - Yes.";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveColonTest7A()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            string text = "Kelly has an eating" + Environment.NewLine + "disorder? MICHAEL: Yes.";
            string expected = "- Kelly has an eating" + Environment.NewLine + "disorder? - Yes.";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveColonTest8()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            string text = "That's really great, you" + Environment.NewLine + "guys. RYAN: Don't vaccinate it.";
            string expected = "- That's really great, you" + Environment.NewLine + "guys. - Don't vaccinate it.";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveColonTest9()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            string text = "<i>- JOHN: Hvordan går det?</i>" + Environment.NewLine + "<i>-Marry: Det går fint!</i>";
            string expected = "<i>- Hvordan går det?</i>" + Environment.NewLine + "<i>- Det går fint!</i>";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveColonTest10()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            string text = "- Rich: Hm-mm." + Environment.NewLine + "- Sam: Yes, and it was,";
            string expected = "- Hm-mm." + Environment.NewLine + "- Yes, and it was,";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveColonTest11()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = true;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            string text = "- He's got the clap." + Environment.NewLine + "- SAM: (gasps) What?";
            string expected = "- He's got the clap." + Environment.NewLine + "- What?";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for RemoveHIInsideLine
        /// </summary>
        [TestMethod]
        public void RemoveHIInsideLine()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            const string text = "Be quiet. (SHUSHING) It's okay.";
            const string expected = "Be quiet. It's okay.";
            string actual = target.RemoveHearingImpairedTagsInsideLine(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// A test for RemoveHI
        /// </summary>
        [TestMethod]
        public void RemoveHI1()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveHI2()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveHINot()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveHIMultilineItalic()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveHIMultilineBold()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveHISecondLineDelay()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveHIQuotes()
        {
            const string expected = "Where?!";
            string text = "- Where?!" + Environment.NewLine + "- Ow!";
            string actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, onlyInSeparatedLine: false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveHIDouble()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveRemoveNameOfFirstLine()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveRemoveNameOfFirstLineBold()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveInterjections()
        {
            var text = "-Ballpark." + Environment.NewLine + "-Hmm.";
            const string expected = "Ballpark.";
            string actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections2()
        {
            var text = $"-Ballpark.{Environment.NewLine}-Mm-hm.";
            const string expected = "Ballpark.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections3()
        {
            var text = $"-Mm-hm.{Environment.NewLine}-Ballpark.";
            const string expected = "Ballpark.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections4()
        {
            var text = "- Mm-hm." + Environment.NewLine + "- Ballpark.";
            const string expected = "Ballpark.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections5()
        {
            var text = $"- Ballpark.{Environment.NewLine}- Hmm.";
            const string expected = "Ballpark.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections6A()
        {
            const string text = "Ballpark, mm-hm.";
            const string expected = "Ballpark.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections6B()
        {
            const string text = "Mm-hm, Ballpark.";
            const string expected = "Ballpark.";

            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections6BItalic()
        {
            const string text = "<i>Mm-hm, Ballpark.</i>";
            const string expected = "<i>Ballpark.</i>";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections7()
        {
            const string text = "You like her, huh?";
            const string expected = "You like her?";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections8()
        {
            const string text = "You like her, huh!";
            const string expected = "You like her!";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections9()
        {
            const string text = "You like her, huh.";
            const string expected = "You like her.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections10()
        {
            var text = $"- You like her, huh.{Environment.NewLine}- I do";
            var expected = $"- You like her.{Environment.NewLine}- I do";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections10Italic()
        {
            string text = "<i>- You like her, huh." + Environment.NewLine + "- I do</i>";
            string expected = "<i>- You like her." + Environment.NewLine + "- I do</i>";

            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections11()
        {
            var text = $"- Ballpark, mm-hm.{Environment.NewLine}- Oh yes!";
            var expected = $"- Ballpark.{Environment.NewLine}- Yes!";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections12()
        {
            const string text = "Well, boy, I'm — Uh —";
            const string expected = "Well, boy, I'm —";

            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections13()
        {
            string text = "- What?" + Environment.NewLine + "- Uh —";
            const string expected = "What?";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections14A()
        {
            string text = "Hey! Uh...";
            const string expected = "Hey!";

            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections14B()
        {
            var text = $"Hey! Uh...{Environment.NewLine}Bye.";
            var expected = $"Hey!{Environment.NewLine}Bye.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections15A()
        {
            string text = "I think that..." + Environment.NewLine + "Uh... Hey!";
            string expected = "I think that..." + Environment.NewLine + "Hey!";

            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjections15B()
        {
            var text = $"I think that...{Environment.NewLine}<i>Uh... Hey!</i>";
            var expected = $"I think that...{Environment.NewLine}<i>Hey!</i>";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveColonOnlyOnSeparateLine()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveColonOnlyOnSeparateLineDoRemove()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            target.Settings.ColonSeparateLine = false;
            const string text = "MICHAEL: How are you?";
            const string expected = "How are you?";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveLineIfAllUppercase1()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveLineIfAllUppercase2()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveLineIfAllUppercase3()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveLineIfParentheses3()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveTextBeforeColonSecondLine()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveTextBeforeColonOnlyUpper1()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveTextBeforeColonOnlyUpper2()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveTextBeforeColonOnlyUpper3Negative()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveTextAss1()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveTextAss2()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveTextAss3Colon()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveTextBeforeColonTest1()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            target.Settings.RemoveTextBeforeColon = true;
            string text = "SKOTT AVFYRADE: 760." + Environment.NewLine + "FORDON FÖRSTÖRDA: 12.";
            string expected = "- 760." + Environment.NewLine + "- 12.";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextRemoveDashBeforeSquareBrackets()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveTextRemoveDashInRemoveInterjections()
        {
            string text = "- Oh." + Environment.NewLine + "<i>- Yes, sure.</i>";
            const string expected = "<i>Yes, sure.</i>";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextRemoveSingleDash()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveTextBetweenQuestionMarks()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveInterjectionsItalicFirstLine()
        {
            string text = "<i>- Mm-hmm.</i>" + Environment.NewLine + "- In my spare time.";
            const string expected = "In my spare time.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjectionsItalicSecondLine()
        {
            var text = $"- In my spare time.{Environment.NewLine}<i>- Mm-hmm.</i>";
            const string expected = "In my spare time.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjectionsItalicBothLines()
        {
            var text = $"<i>- In my spare time.</i>{Environment.NewLine}<i>- Mm-hmm.</i>";
            const string expected = "<i>In my spare time.</i>";

            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjectionsMDash()
        {
            const string text = "I'm sorry. I, mm-hmm—";
            const string expected = "I'm sorry. I—";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjectionsOnlyOnSeparateLine1()
        {
            const string text = "I'm sorry. Mm-hmm.";
            const string expected = text;
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, true));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjectionsOnlyOnSeparateLine2()
        {
            string text = $"I\'m sorry.{Environment.NewLine}Mm-hmm.";
            const string expected = "I'm sorry.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, true));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveFirstDashItalics()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            string text = "<i>- A man who wants to make his mark..." + Environment.NewLine + "- [ Coughing]</i>";
            const string expected = "<i>A man who wants to make his mark...</i>";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextIfUppercaseNotEmdash()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = true;
            string text = "- And you?" + Environment.NewLine + "- I—";
            string expected = "- And you?" + Environment.NewLine + "- I—";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextDontCrashOnEmptyString()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = true;
            string text = string.Empty;
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(text, actual);
        }

        [TestMethod]
        public void RemoveTextKeepMusicSymbolsButRemoveHI()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveIfTextContains = null;
            const string text = "<i>♪♪[Ambient Electronic]</i>";
            const string expected = "<i>♪♪</i>";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextRemoveEmdash()
        {
            var target = GetRemoveTextForHiLib();
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
        public void RemoveTextIfUppercaseEmdashRemoveInDialogue()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "- Uh—uh, my God!" + Environment.NewLine + "- Uh, my God.";
            string expected = "- My God!" + Environment.NewLine + "- My God.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextIfUppercaseEmdashRemoveInDialogueWithSpaces()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "- Uh — uh, my God!" + Environment.NewLine + "- Uh, my God.";
            string expected = "- My God!" + Environment.NewLine + "- My God.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextEmdashComma()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "- I just, uh —" + Environment.NewLine + "- What?";
            string expected = "- I just —" + Environment.NewLine + "- What?";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextHiAndColon()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            const string text = "I'm trying to! (MASTER): Faster now. evacuate.";
            const string expected = "I'm trying to! Faster now. evacuate.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextDontLeaveDoubleDash()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "- Mr. Harding?" + Environment.NewLine + "Uh--";
            const string expected = "Mr. Harding?";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextDontLeaveDot()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "- Mr. Harding?" + Environment.NewLine + "- Mm-hm. Oh.";
            const string expected = "Mr. Harding?";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextDontLeaveExl()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "-Sit down. Sit down." + Environment.NewLine + "-Oh! Oh!";
            const string expected = "Sit down. Sit down.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextRememberDialogueTag()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            string text = "Oh." + Environment.NewLine + "-I'm awfully tired.";
            const string expected = "I'm awfully tired.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextRemoveLineWithColon()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "Before:";
            string expected = string.Empty;
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextRemoveLineWithColon2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            const string text = "COP 1: Call it in, code four. COP 4: Get him out of here.";
            const string expected = "Call it in, code four. Get him out of here.";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextSpeakerWithColonPlusLineWithHyphen()
        {
            var target = GetRemoveTextForHiLib();
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
            var target = GetRemoveTextForHiLib();
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
            var target = GetRemoveTextForHiLib();
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

            string text = "- \"Ballpark.\"" + Environment.NewLine + "-Hmm.";
            const string expected = "\"Ballpark.\"";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, true));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiInDialogue1()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            string text = "-[gurgling]" + Environment.NewLine + "-Mom?";
            const string expected = "Mom?";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiInDialogue2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            string text = "-[Ronnie laughs]" + Environment.NewLine + "-[sighs] Wow, Ronnie.";
            const string expected = "Wow, Ronnie.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiRemoveFontTag()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            const string text = "<font color=\"#808080\">[Whistling]</font> Hallo everybody!";
            const string expected = "Hallo everybody!";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiRemoveFontTag2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            const string text = "♪ <font color=\"#000000\">[LIGHT SWITCH CLICKS]</font>";
            const string expected = "♪";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiRemoveFontTag3()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            const string text = "Foobar <font color=\"#808080\">[CHAINS RATTLING]</font> Foobar";
            const string expected = "Foobar Foobar";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiRemoveInterjections()
        {
            string text = "<i>- Here it is." + Environment.NewLine + "- Ahh!</i>";
            const string expected = "<i>Here it is.</i>";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, true));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiRemoveFirstLine()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveTextBetweenCustomTags = true;
            target.Settings.CustomStart = "♪";
            target.Settings.CustomEnd = "♪";
            target.Settings.RemoveTextBetweenBrackets = true;
            string text = "<i>- ♪♪[Continues ]</i>" + Environment.NewLine + "- It's pretty strong stuff.";
            const string expected = "It's pretty strong stuff.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiMultiLineCustomTags()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveTextBetweenCustomTags = true;
            target.Settings.CustomStart = "♪";
            target.Settings.CustomEnd = "♪";
            target.Settings.RemoveTextBetweenBrackets = true;
            string text = "♪ Trotting down the paddock" + Environment.NewLine + "on a bright, sunny day ♪♪";
            string expected = string.Empty;
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiMultiLineCustomTagsItalic()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveTextBetweenCustomTags = true;
            target.Settings.CustomStart = "♪";
            target.Settings.CustomEnd = "♪";
            target.Settings.RemoveTextBetweenBrackets = true;
            string text = "<i>♪ Trotting down the paddock" + Environment.NewLine + "on a bright, sunny day ♪♪</i>";
            string expected = string.Empty;
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiMultiLineCustomTagsInDialoque()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveTextBetweenCustomTags = true;
            target.Settings.CustomStart = "♪";
            target.Settings.CustomEnd = "♪";
            target.Settings.RemoveTextBetweenBrackets = true;
            string text = "- ♪ Honey, honey, yeah ♪" + Environment.NewLine + "- ♪ Heard it through|the grapevine ♪";
            string expected = string.Empty;
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiSecondLineItalicAdvanced()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.RemoveTextBetweenCustomTags = true;
            target.Settings.CustomStart = "♪";
            target.Settings.CustomEnd = "♪";
            target.Settings.RemoveTextBetweenBrackets = true;
            Configuration.Settings.General.DialogStyle = DialogType.DashBothLinesWithSpace;
            string text = "The meal is ready. Let's go!" + Environment.NewLine + "<i>- [Nick]</i> J. T. Lancer!";
            string expected = "- The meal is ready. Let's go!" + Environment.NewLine + "- J. T. Lancer!";
            var sub = new Subtitle();
            sub.Paragraphs.Add(new Paragraph(text, 0, 2000));
            string actual = target.RemoveTextFromHearImpaired(text, sub, 0);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiInterjectionsEndDash()
        {
            const string text = "Oh. Oh, yeah. Ahh —";
            const string expected = "Yeah —";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiRemoveFirstBlankLineAlsoItalics()
        {
            string text = "<i>Ow. Ow." + Environment.NewLine + "Ow, my head.</i>";
            const string expected = "<i>My head.</i>";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiDialogAddDashFirstLine()
        {
            var target = GetRemoveTextForHiLib();
            string text = "RECORDING: <i>Have you lost someone?</i>" + Environment.NewLine + "- What?";
            string expected = "<i>- Have you lost someone?</i>" + Environment.NewLine + "- What?";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveTextForHiDialogMusicSymbolsFirstLine()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            string actual = target.RemoveColon("- ♪ To defeat ♪" + Environment.NewLine + "- Referee: Salute.");
            Assert.AreEqual("- ♪ To defeat ♪" + Environment.NewLine + "- Salute.", actual);
        }

        [TestMethod]
        public void RemoveTextForHiDialogMusicSymbolsFirstLine2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            string actual = target.RemoveColon("- ♪ To defeat ♪" + Environment.NewLine + "- Referee: Salute.");
            Assert.AreEqual("- ♪ To defeat ♪" + Environment.NewLine + "- Referee: Salute.", actual);
        }

        [TestMethod]
        public void RemoveTextForHiDialogMusicSymbolsFirstLine3()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveTextBetweenCustomTags = true;
            target.Settings.CustomStart = "♪";
            target.Settings.CustomEnd = "♪";
            string actual = target.RemoveTextFromHearImpaired("- ♪ To defeat ♪" + Environment.NewLine + "- Referee: Salute.");
            Assert.AreEqual("Salute.", actual);
        }

        [TestMethod]
        public void RemoveInterjectionKeepDotDotDot()
        {
            string text = "She uh..." + Environment.NewLine + "she disappeared.";
            string expected = "She..." + Environment.NewLine + "she disappeared.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjectionKeepEndingQuestionMark()
        {
            var target = GetRemoveTextForHiLib();
            const string text = "So you mean that oh?";
            const string expected = "So you mean that?";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjectionKeepEndingEx()
        {
            const string text = "So you mean that oh!";
            const string expected = "So you mean that!";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext(text, false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveRemoveUppercaseLineNegativeOnlyNumbers()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.RemoveIfAllUppercase = true;

            string text = "Let's count!" + Environment.NewLine + "1.. 2... 3!";
            string expected = "Let's count!" + Environment.NewLine + "1.. 2... 3!";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveRemoveUppercase()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.RemoveIfAllUppercase = true;

            const string text = "ENGINE STARTING";
            const string expected = "";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveAdvanced1()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            target.Settings.RemoveIfAllUppercase = false;

            string text = "- NORA: <i>Sir?</i>" + Environment.NewLine + "- (CAR DOOR CLOSES)";
            string expected = "<i>Sir?</i>";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveAdvanced2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            target.Settings.RemoveIfAllUppercase = false;

            string text = "- Well, received, technically." + Environment.NewLine + "- KEVIN: <i>Mmm-hmm.</i>";
            string expected = "- Well, received, technically." + Environment.NewLine + "<i>- Mmm-hmm.</i>";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod]
        public void RemoveAdvanced3()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            target.Settings.RemoveInterjections = true;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            target.Settings.RemoveIfAllUppercase = false;

            string text = "- Well, received, technically." + Environment.NewLine + "- KEVIN: <i>Mmm-hmm.</i>";
            string expected = "Well, received, technically.";
            string actual = target.RemoveTextFromHearImpaired(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveColonTestDash()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveIfAllUppercase = false;
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.OnlyIfInSeparateLine = false;
            target.Settings.ColonSeparateLine = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            string text = "- I have a theory, captain--" + Environment.NewLine + "UHURA: Captain Kirk.";
            string expected = "- I have a theory, captain--" + Environment.NewLine + "- Captain Kirk.";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjectionDotDotDot()
        {
            string expected = "Alright.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext("Oh... alright.", false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjectionDotDotDotItalic()
        {
            string expected = "<i>Alright.</i>";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext("<i>Oh... alright.</i>", false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjectionDotDotDotSecondLineDialog()
        {
            var target = GetRemoveTextForHiLib();
            var expected = $"- OK.{Environment.NewLine}- Alright.";
            var actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext($"- OK.{Environment.NewLine}- Oh... alright.", false));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveInterjectionDialogSecondLineEmDash()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenParentheses = true;
            target.Settings.RemoveInterjections = true;
            string expected = "- How many, sir?" + Environment.NewLine + "- 275.";
            string actual = target.RemoveTextFromHearImpaired("- How many, sir?" + Environment.NewLine + "- Uh — (clears throat) 275.");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveColonAfterHITags()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenParentheses = false;
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveInterjections = false;
            string actual = target.RemoveTextFromHearImpaired("[scoffs]: Nice try.");
            Assert.AreEqual("Nice try.", actual);
        }

        [TestMethod]
        public void RemoveSecondLineDialogAndItalicAndMusic()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenCustomTags = true;
            target.Settings.CustomStart = "♪";
            target.Settings.CustomEnd = "♪";
            string actual = target.RemoveTextFromHearImpaired("- Ferguson, Kaz..." + Environment.NewLine + "- <i>♪ [Ominous tone plays] ♪</i>");
            Assert.AreEqual("Ferguson, Kaz...", actual);
        }

        [TestMethod]
        public void RemoveSecondLineDialogAndItalicAndMusic2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            string actual = target.RemoveTextFromHearImpaired("- Ferguson, Kaz..." + Environment.NewLine + "- <i>♪ [Ominous tone plays] ♪</i>");
            Assert.AreEqual("Ferguson, Kaz...", actual);
        }


        [TestMethod]
        public void RemoveFirstLineOfTwoColons1()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            string actual = target.RemoveTextFromHearImpaired("KIRK:" + Environment.NewLine + "<i>Captain's log, stardate 1514. 1:</i>");
            Assert.AreEqual("<i>Captain's log, stardate 1514. 1:</i>", actual);
        }

        [TestMethod]
        public void RemoveFirstLineOfTwoColons2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            string actual = target.RemoveTextFromHearImpaired("KIRK:" + Environment.NewLine + "Captain's log, stardate 1514. 1:");
            Assert.AreEqual("Captain's log, stardate 1514. 1:", actual);
        }

        [TestMethod]
        public void DontRemoveDoubleDashInSecondLine()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            string actual = target.RemoveTextFromHearImpaired("BALOK[OVER RADIO]:" + Environment.NewLine + "<i>--and trespassed into our star systems.</i>");
            Assert.AreEqual("<i>--and trespassed into our star systems.</i>", actual);
        }

        [TestMethod]
        public void DontRemoveDoubleDashInSecondLine2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            string actual = target.RemoveTextFromHearImpaired("BALOK[OVER RADIO]:" + Environment.NewLine + "--and trespassed into our star systems.");
            Assert.AreEqual("--and trespassed into our star systems.", actual);
        }

        [TestMethod]
        public void DontRemoveDoubleDashInSecondLine3()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            string actual = target.RemoveTextFromHearImpaired("BALOK[OVER RADIO]:" + Environment.NewLine + "<i>—and trespassed into our star systems.</i>");
            Assert.AreEqual("<i>—and trespassed into our star systems.</i>", actual);
        }

        [TestMethod]
        public void DontRemoveDialogueDashInSecondLineNoSpace()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBetweenSquares = true;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveTextBetweenCustomTags = false;
            string actual = target.RemoveTextFromHearImpaired("<i> -JOHN: Hvordan går det?</i>" + Environment.NewLine + "<i>-Marry: Det går fint!</i>");
            Assert.AreEqual("<i>- Hvordan går det?</i>" + Environment.NewLine + "<i>- Det går fint!</i>", actual);
        }

        [TestMethod]
        public void StartWithUppercaseLetterAfterSemicolon()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            string actual = target.RemoveColon("Robert: what do you think about this new car?");
            Assert.AreEqual("What do you think about this new car?", actual);
        }

        [TestMethod]
        public void StartWithUppercaseLetterAfterSemicolonWithItalic()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            string actual = target.RemoveColon("<i>Robert: what do you think about this new car?</i>");
            Assert.AreEqual("<i>What do you think about this new car?</i>", actual);
        }

        [TestMethod]
        public void StartWithUppercaseLetterAfterSemicolonSecondLine()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            string actual = target.RemoveColon("Hallo!" + Environment.NewLine + "Robert: what do you think about this new car?");
            Assert.IsTrue(actual.Contains("What do you think about this new car?"));
        }

        [TestMethod]
        public void RemoveColonNameAfterEllipsisInsideLine()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.NameList.Add("JEREMY");
            string actual = target.RemoveColon("- Step out of here... JEREMY: Not world." + Environment.NewLine + "It's a British record.");
            Assert.AreEqual("- Step out of here... Not world." + Environment.NewLine + "- It's a British record.", actual);
        }

        [TestMethod]
        public void RemoveNameAndCustom()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveTextBetweenCustomTags = true;
            target.Settings.CustomStart = "♪";
            target.Settings.CustomEnd = "♪";
            string actual = target.RemoveTextFromHearImpaired("♪ Give me hope and" + Environment.NewLine + "give me patience ♪");
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void RemoveTextBetweenParenthesesNested1()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            string actual = target.RemoveTextFromHearImpaired("(Not, it's not (a secret).)");
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void RemoveTextBetweenParenthesesNested2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.RemoveTextBetweenParentheses = true;
            string actual = target.RemoveTextFromHearImpaired("(MAN) Not, it's... (WOMAN) not.");
            Assert.AreEqual("Not, it's... not.", actual);
        }

        [TestMethod]
        public void RemoveTextBetweenBeforeColonFontTag()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBetweenParentheses = true;
            string actual = target.RemoveColon("<font color=\"#404040\">MAN:</font> Gotta be ready before nightfall.");
            Assert.AreEqual("Gotta be ready before nightfall.", actual);
        }

        [TestMethod]
        public void RemoveTextBetweenBeforeColonItalicTag()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBetweenParentheses = true;
            string actual = target.RemoveColon("<i>MAN:</i> Gotta be ready before nightfall.");
            Assert.AreEqual("Gotta be ready before nightfall.", actual);
        }

        [TestMethod]
        public void RemoveTextBetweenBeforeColonDoNotTouch()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBetweenParentheses = true;
            var source = "{\\an8}But I know of something" + Environment.NewLine +
                         "<i>that could:</i>";
            string actual = target.RemoveColon(source);
            Assert.AreEqual(source, actual);
        }

        [TestMethod]
        public void RemoveTextBetweenBeforeColonMakeEmpty()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            target.Settings.OnlyIfInSeparateLine = false;
            string actual = target.RemoveColon("<i>SAURON:</i>");
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void RemoveTextBetweenBeforeColonMakeEmpty2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.OnlyIfInSeparateLine = false;
            string actual = target.RemoveColon("<i>SAURON:</i>");
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void RemoveTextBetweenBeforeColonMakeEmpty3()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            target.Settings.OnlyIfInSeparateLine = true;
            string actual = target.RemoveColon("<i>SAURON:</i>");
            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void RemoveHiSecondLineHiDialog()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            target.Settings.OnlyIfInSeparateLine = true;
            string actual = target.RemoveTextFromHearImpaired("<i>-Era stato avveritito.</i>" + Environment.NewLine + "-(PARLA IN SPANOLO)");
            Assert.AreEqual("<i>Era stato avveritito.</i>", actual);
        }

        [TestMethod]
        public void RemoveHiRemoveInLine()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            target.Settings.OnlyIfInSeparateLine = false;
            string actual = target.RemoveTextFromHearImpaired("Senti, [si schiarisce la voce]," + Environment.NewLine + "dille semplicemente che ti e mancata.");
            Assert.AreEqual("Senti," + Environment.NewLine + "dille semplicemente che ti e mancata.", actual);
        }

        [TestMethod]
        public void RemoveInterjectionsRemovePeriod()
        {
            string actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext("Hey. Uh.", onlyInSeparatedLine: false));
            Assert.AreEqual("Hey.", actual);
        }

        [TestMethod]
        public void RemoveTextForHi()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            string actual = target.RemoveTextFromHearImpaired("Spoken text." + Environment.NewLine + "<i>- [hearing impaired text] Spoken text.</i>");
            Assert.AreEqual("- Spoken text." + Environment.NewLine + "<i>- Spoken text.</i>", actual);
        }

        [TestMethod]
        public void RemoveTextForHiSecondLineDifficult()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = true;
            target.Settings.RemoveInterjections = false;
            string actual = target.RemoveTextFromHearImpaired("- [chuckles]" + Environment.NewLine + "- MRS. TRYON: Mr. Wylie!");
            Assert.AreEqual("Mr. Wylie!", actual);
        }

        [TestMethod]
        public void RemoveTextForHiSecondLineDifficult2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = false;
            string actual = target.RemoveTextFromHearImpaired("- [chuckles]" + Environment.NewLine + "- MRS. TRYON: Mr. Wylie!");
            Assert.AreEqual("Mr. Wylie!", actual);
        }


        [TestMethod]
        public void RemoveTextForHiBracketDoubleLineDoRemove()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.OnlyIfInSeparateLine = false;
            string actual = target.RemoveTextFromHearImpaired("[man] Aren't you a little old" + Environment.NewLine + "to be playing with dolls, Michael?");
            Assert.AreEqual("Aren't you a little old" + Environment.NewLine + "to be playing with dolls, Michael?", actual);
        }

        [TestMethod]
        public void RemoveTextForHiBracketDoubleLineDoNotRemove()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.OnlyIfInSeparateLine = true;
            string actual = target.RemoveTextFromHearImpaired("[man] Aren't you a little old" + Environment.NewLine + "to be playing with dolls, Michael?");
            Assert.AreEqual("[man] Aren't you a little old" + Environment.NewLine + "to be playing with dolls, Michael?", actual);
        }

        [TestMethod]
        public void RemoveTextForHiDialogHearingImpairedSecondLine()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.OnlyIfInSeparateLine = false;
            string actual = target.RemoveTextFromHearImpaired("- You're weird!" + Environment.NewLine + "- [sigh]");
            Assert.AreEqual("You're weird!", actual);
        }

        [TestMethod]
        public void RemoveTextForHiAlternateHyphenU2010()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = false;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenBrackets = true;
            target.Settings.OnlyIfInSeparateLine = false;
            string actual = target.RemoveTextFromHearImpaired("\u2010 You're weird!" + Environment.NewLine + "\u2010 [sigh]");
            Assert.AreEqual("You're weird!", actual);
        }

        [TestMethod]
        public void RemoveTextForHiThreeLinesDialog1()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.OnlyIfInSeparateLine = false;
            string actual = target.RemoveColon("- I mean, sometimes" + Environment.NewLine + "a leaf is just..." + Environment.NewLine + "- SAMANTHA: Uganda");
            Assert.AreEqual("- I mean, sometimes" + Environment.NewLine + "a leaf is just..." + Environment.NewLine + "- Uganda", actual);
        }

        [TestMethod]
        public void RemoveTextForHiThreeLinesDialog2()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.OnlyIfInSeparateLine = false;
            string actual = target.RemoveColon("- DAVID: I'm Artemis." + Environment.NewLine + "- LARRY: Not supposed" + Environment.NewLine + "to be purple.");
            Assert.AreEqual("- I'm Artemis." + Environment.NewLine + "- Not supposed" + Environment.NewLine + "to be purple.", actual);
        }

        [TestMethod]
        public void RemoveTextForHiThreeLinesDialog3()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.OnlyIfInSeparateLine = false;
            string actual = target.RemoveColon("<i>- and many were imm..." + Environment.NewLine + "- WOMAN: We believe" + Environment.NewLine + "the future of food...</i>");
            Assert.AreEqual("<i>- and many were imm..." + Environment.NewLine + "- We believe" + Environment.NewLine + "the future of food...</i>", actual);
        }

        [TestMethod]
        public void RemoveTextForHiSecondDialogEmpty()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.OnlyIfInSeparateLine = false;
            string actual = target.RemoveTextFromHearImpaired("- Oh. No." + Environment.NewLine + "-");
            Assert.AreEqual("Oh. No.", actual);
        }

        [TestMethod]
        public void RemoveTextForHiFirstDialogEmpty()
        {
            var target = GetRemoveTextForHiLib();
            target.Settings.RemoveTextBeforeColon = true;
            target.Settings.RemoveTextBeforeColonOnlyUppercase = false;
            target.Settings.RemoveInterjections = false;
            target.Settings.RemoveTextBetweenBrackets = false;
            target.Settings.OnlyIfInSeparateLine = false;
            string actual = target.RemoveTextFromHearImpaired("-" + Environment.NewLine + "- Oh. No.");
            Assert.AreEqual("Oh. No.", actual);
        }

        [TestMethod]
        public void RemoveInterjectionsAfterComma()
        {
            string actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext("Hey, ahhhh.", onlyInSeparatedLine: false));
            Assert.AreEqual("Hey.", actual);
        }

        [TestMethod]
        public void RemoveInterjectionsAfterCommaUpper()
        {
            string actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext("Hey, Ah.", onlyInSeparatedLine: false));
            Assert.AreEqual("Hey.", actual);
        }

        [TestMethod]
        public void RemoveInterjectionsAfterComma2()
        {
            string actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext("Hey, ahhhh!", onlyInSeparatedLine: false));
            Assert.AreEqual("Hey!", actual);
        }

        [TestMethod]
        public void RemoveInterjectionsAfterComma3()
        {
            string actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext("Hey, ahhhh?", onlyInSeparatedLine: false));
            Assert.AreEqual("Hey?", actual);
        }

        [TestMethod]
        public void RemoveInterjectionsAfterComma4()
        {
            string actual = new RemoveInterjection().Invoke(GetRemoveInterjectionContext("Hey, ahhhh!?", onlyInSeparatedLine: false));
            Assert.AreEqual("Hey!?", actual);
        }
    }
}
