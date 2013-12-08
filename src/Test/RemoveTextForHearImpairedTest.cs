using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Forms;
using System;

namespace Test
{


    /// <summary>
    ///This is a test class for FormRemoveTextForHearImpairedTest and is intended
    ///to contain all FormRemoveTextForHearImpairedTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RemoveTextForHearImpairedTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
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
        #endregion

        /// <summary>
        ///A test for RemoveColon
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest()
        {
            var target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            string text = "Man over P.A.:\r\nGive back our homes.";
            string expected = "Give back our homes.";
            string actual = target.RemoveColon(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest2a()
        {
            var target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            string text = "GIOVANNI: <i>Number 9: I never look for a scapegoat.</i>";
            string expected = "<i>I never look for a scapegoat.</i>";
            string actual = target.RemoveColon(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonTest2b()
        {
            var target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = true;
            string text = "GIOVANNI: <i>Number 9: I never look for a scapegoat.</i>";
            string expected = "<i>Number 9: I never look for a scapegoat.</i>";
            string actual = target.RemoveColon(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }

        

        /// <summary>
        ///A test for RemoveHIInsideLine
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHIInsideLine()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBetweenParentheses.Checked = true;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "Be quiet. (SHUSHING) It's okay.";
            string expected = "Be quiet. It's okay.";
            string actual = target.RemoveHearImpairedtagsInsideLine(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveHI
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHI1()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBetweenSquares.Checked = true;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "- Aw, save it. Storm?\r\n- [Storm]\r\nWe're outta here.";
            string expected = "- Aw, save it. Storm?\r\n- We're outta here.";
            string actual = target.RemoveTextFromHearImpaired(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveHI
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHI2()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBetweenSquares.Checked = true;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "[Chuckles,\r\nCoughing]\r\nBut we lived through it.";
            string expected = "But we lived through it.";
            string actual = target.RemoveTextFromHearImpaired(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for no removal
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHINot()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "is the body of a mutant kid\r\non the 6:00 news.";
            string expected = "is the body of a mutant kid\r\non the 6:00 news.";
            string actual = target.RemoveTextFromHearImpaired(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }


        /// <summary>
        ///A test for RemoveHI
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHIMultilineItalic()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "<i>NARRATOR:" + Environment.NewLine +
                          "Previously on NCIS</i>";
            string expected = "<i>Previously on NCIS</i>";
            string actual = target.RemoveTextFromHearImpaired(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveHI
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHIMultilineBold()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "<b>NARRATOR:" + Environment.NewLine +
                          "Previously on NCIS</b>";
            string expected = "<b>Previously on NCIS</b>";
            string actual = target.RemoveTextFromHearImpaired(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveHI
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHISecondLineDelay()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "- JOHN: Hey." + Environment.NewLine +
                          "- ...hey.";
            string expected = "- Hey."+ Environment.NewLine +"- ...hey.";
            string actual = target.RemoveTextFromHearImpaired(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHIQuotes()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "- Where?!" + Environment.NewLine + "- Ow!";
            string expected = "Where?!";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHIDouble()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBetweenSquares.Checked = true;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "[MAN]Where?![MAN]";
            string expected = "Where?!";
            string actual = target.RemoveTextFromHearImpaired(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveRemoveNameOfFirstLine()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "HECTOR: Hi." + Environment.NewLine + "-Oh, hey, Hector.";
            string expected = "- Hi." + Environment.NewLine + "- Oh, hey, Hector.";
            string actual = target.RemoveTextFromHearImpaired(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveRemoveNameOfFirstLineBold()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "<b>HECTOR: Hi.</b>";
            string expected = "<b>Hi.</b>";
            string actual = target.RemoveTextFromHearImpaired(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "-Ballpark." + Environment.NewLine + "-Hmm.";
            string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections2()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "-Ballpark." + Environment.NewLine + "-Mm-hm.";
            string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections3()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "-Mm-hm." + Environment.NewLine + "-Ballpark.";
            string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections4()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "- Mm-hm." + Environment.NewLine + "- Ballpark.";
            string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections5()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "- Ballpark." + Environment.NewLine + "- Hmm.";
            string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections6a()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "Ballpark, mm-hm.";
            string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections6b()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "Mm-hm, Ballpark.";
            string expected = "Ballpark.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections6bItalic()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "<i>Mm-hm, Ballpark.</i>";
            string expected = "<i>Ballpark.</i>";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections7()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "You like her, huh?";
            string expected = "You like her?";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections8()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "You like her, huh!";
            string expected = "You like her!";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections9()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "You like her, huh.";
            string expected = "You like her.";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections10()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "- You like her, huh." + Environment.NewLine + "- I do";
            string expected = "- You like her." + Environment.NewLine + "- I do";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections10Italic()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "<i>- You like her, huh." + Environment.NewLine + "- I do</i>";
            string expected = "<i>- You like her." + Environment.NewLine + "- I do</i>";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveInterjections11()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "- Ballpark, mm-hm." + Environment.NewLine + "- Oh yes!";
            string expected = "- Ballpark." + Environment.NewLine + "- Yes!";
            string actual = target.RemoveInterjections(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveColonOnlyOnSeparateLine()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = true;
            string text = "HECTOR: Hi.";
            string expected = "HECTOR: Hi.";
            string actual = target.RemoveColon(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfAllUppercase1()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = true;
            target.checkBoxRemoveInterjections.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "HECTOR " + Environment.NewLine + "Hi.";
            string expected = "Hi.";
            string actual = target.RemoveLineIfAllUppercase(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfAllUppercase2()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = true;
            target.checkBoxRemoveInterjections.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "Please, Mr Krook." + Environment.NewLine + "SHOP DOOR BELL CLANGS";
            string expected = "Please, Mr Krook.";
            string actual = target.RemoveLineIfAllUppercase(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfAllUppercase3()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = true;
            target.checkBoxRemoveInterjections.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            target.checkBoxRemoveTextBetweenParentheses.Checked = true;
            string text = "(<i>GOIN' BACK TO INDIANA</i>" + Environment.NewLine + "CONTINUES PLAYING)";
            string expected = "";
            string actual = target.RemoveLineIfAllUppercase(text);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveLineIfParentheses3()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = false;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            target.checkBoxRemoveTextBetweenParentheses.Checked = true;
            string text = "(<i>GOIN' BACK TO INDIANA</i>" + Environment.NewLine + "CONTINUES PLAYING)";
            string expected = "";
            string actual = target.RemoveHearImpairedTags(text);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveTextBeforeColonSecondLine()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor();
            target.checkBoxRemoveIfAllUppercase.Checked = false;
            target.checkBoxRemoveInterjections.Checked = false;
            target.checkBoxRemoveTextBeforeColon.Checked = true;
            target.checkBoxOnlyIfInSeparateLine.Checked = false;
            target.checkBoxRemoveTextBeforeColonOnlyUppercase.Checked = false;
            target.checkBoxColonSeparateLine.Checked = false;
            string text = "- even if it was one week." + Environment.NewLine  + "CANNING: Objection.";
            string expected = "- even if it was one week." + Environment.NewLine + "- Objection.";
            string actual = target.RemoveColon(text, string.Empty);
            Assert.AreEqual(expected, actual);
        }

    }
}
