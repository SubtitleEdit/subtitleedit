using Nikse.SubtitleEdit.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Nikse.SubtitleEdit.Logic;

namespace Test
{        
    /// <summary>
    ///This is a test class for FixCommonErrors and is intended
    ///to contain all FixCommonErrorsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FixCommonErrorsTest
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

        private static void InitializeFixCommonErrorsLine(FixCommonErrors_Accessor target, string line)
        {
            Subtitle subtitle = new Subtitle();
            subtitle.Paragraphs.Add(new Paragraph(line, 100, 10000));
            target.Initialize(subtitle);
            target.ButtonFixClick(null, null);
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

        #region Merge short lines  
        /// <summary>
        ///A test for Merge short lines
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesNormal()
        {
            FixCommonErrors_Accessor target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "This is" + Environment.NewLine + "short!");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This is short!");
        }

        /// <summary>
        ///A test for Merge short lines
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesLong()
        {
            FixCommonErrors_Accessor target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "This I'm pretty sure is not a" + Environment.NewLine + "short line, that should be merged!!!");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "This I'm pretty sure is not a" + Environment.NewLine + "short line, that should be merged!!!");
        }

        /// <summary>
        ///A test for Merge short lines
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesNormalItalic()
        {
            FixCommonErrors_Accessor target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>This is" + Environment.NewLine + "short!</i>");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>This is short!</i>");
        }

        /// <summary>
        ///A test for Merge short lines
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialogue()
        {
            FixCommonErrors_Accessor target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "- Hallo!" + Environment.NewLine + "- Hi");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "- Hallo!" + Environment.NewLine + "- Hi");
        }

        /// <summary>
        ///A test for Merge short lines
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialogueItalic()
        {
            FixCommonErrors_Accessor target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>- Hallo!" + Environment.NewLine + "- Hi</i>");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>- Hallo!" + Environment.NewLine + "- Hi</i>");
        }


        /// <summary>
        ///A test for Merge short lines
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void FixShortLinesDialogueItalicTwo()
        {
            FixCommonErrors_Accessor target = new FixCommonErrors_Accessor();
            InitializeFixCommonErrorsLine(target, "<i>- Hallo!</i>" + Environment.NewLine + "<i>- Hi<i>");
            target.FixShortLines();
            Assert.AreEqual(target._subtitle.Paragraphs[0].Text, "<i>- Hallo!</i>" + Environment.NewLine + "<i>- Hi<i>");
        }
        #endregion
    }
}
