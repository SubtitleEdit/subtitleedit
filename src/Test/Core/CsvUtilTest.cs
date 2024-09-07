using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.Common;

namespace Test.Core
{
    [TestClass]
    public class CsvUtilTest
    {
        [TestMethod]
        public void Simple_No_Quotes()
        {
            // Arrange
            var csv = "How are you?,I'm fine!,Thank you.";

            // Act
            var result = CsvUtil.CsvSplit(csv, false, out var con);

            // Assert
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("How are you?", result[0]);
            Assert.AreEqual("I'm fine!", result[1]);
            Assert.AreEqual("Thank you.", result[2]);
            Assert.AreEqual(false, con);
        }

        [TestMethod]
        public void Simple_With_Quotes()
        {
            // Arrange
            var csv = "\"How are you?\",\"I'm fine!\",\"Thank you.\"";

            // Act
            var result = CsvUtil.CsvSplit(csv, false, out var con);

            // Assert
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("How are you?", result[0]);
            Assert.AreEqual("I'm fine!", result[1]);
            Assert.AreEqual("Thank you.", result[2]);
            Assert.AreEqual(false, con);
        }

        [TestMethod]
        public void Simple_With_Quotes_And_Comma()
        {
            // Arrange
            var csv = "\"How are you?\",\"I'm fine!\",\"Thank you, my friend.\"";

            // Act
            var result = CsvUtil.CsvSplit(csv, false, out var con);

            // Assert
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("How are you?", result[0]);
            Assert.AreEqual("I'm fine!", result[1]);
            Assert.AreEqual("Thank you, my friend.", result[2]);
            Assert.AreEqual(false, con);
        }

        [TestMethod]
        public void Simple_With_Quotes_And_Escaped_Quote()
        {
            // Arrange
            var csv = "\"How are you?\",\"I'm fine!\",\"Thank \"\"you\"\".\"";

            // Act
            var result = CsvUtil.CsvSplit(csv, false, out var con);

            // Assert
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("How are you?", result[0]);
            Assert.AreEqual("I'm fine!", result[1]);
            Assert.AreEqual("Thank \"you\".", result[2]);
            Assert.AreEqual(false, con);
        }
    }
}
