using Nikse.SubtitleEdit.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor(); // TODO: Initialize to an appropriate value
            string text = "Man over P.A.:\r\nGive back our homes.";
            string expected = "Give back our homes.";
            string actual = target.RemoveColon(text);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveHIInsideLine
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SubtitleEdit.exe")]
        public void RemoveHIInsideLine()
        {
            FormRemoveTextForHearImpaired_Accessor target = new FormRemoveTextForHearImpaired_Accessor(); // TODO: Initialize to an appropriate value
            string text = "Be quiet. (SHUSHING) It's okay.";
            string expected = "Be quiet. It's okay.";
            string actual = target.RemoveHearImpairedtagsInsideLine(text);
            Assert.AreEqual(expected, actual);
        }

    }
}
