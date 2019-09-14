using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core;

namespace Test.Core
{
    [TestClass]
    public class SeJsonParserTest
    {
        [TestMethod]
        public void Simple()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("{ \"content\" : \"Joe\"}", "content");
            Assert.AreEqual("Joe", result.First());
        }

        [TestMethod]
        public void SimpleQuote()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("{ \"content\" : \"Joe \\\"is\\\" best\"}", "content");
            Assert.AreEqual("Joe \\\"is\\\" best", result.First());
        }

        [TestMethod]
        public void SimpleArray()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("[{ \"content\" : \"Joe1\"},{ \"content\" : \"Joe2\"}]", "content");
            Assert.AreEqual("Joe1", result[0]);
            Assert.AreEqual("Joe2", result[1]);
        }

        [TestMethod]
        public void Complex()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("{" + Environment.NewLine +
                                                      "\"name\":\"John\"," + Environment.NewLine +
                                                      "\"age\":30," + Environment.NewLine +
                                                      "\"cars\": [" + Environment.NewLine +
                                                      "{ \"name\":\"Ford\", \"content\":\"Fiesta\"  }," + Environment.NewLine +
                                                      "{ \"name\":\"BMW\", \"content\": \"X3\"}," + Environment.NewLine +
                                                      "{ \"name\":\"Fiat\", \"content\": \"500\" } ]}", "content");
            Assert.AreEqual("Fiesta", result[0]);
            Assert.AreEqual("X3", result[1]);
            Assert.AreEqual("500", result[2]);
        }

        [TestMethod]
        public void SimpleNumber()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("[{ \"content\" : 10 }]", "content");
            Assert.AreEqual("10", result[0]);
        }

        [TestMethod]
        public void SimpleBoolTrue()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("[{ \"content\" : true }]", "content");
            Assert.AreEqual("true", result[0]);
        }

        [TestMethod]
        public void SimpleBoolFalse()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("[{ \"content\" : false }]", "content");
            Assert.AreEqual("false", result[0]);
        }

        [TestMethod]
        public void SimpleNull()
        {
            var parser = new SeJsonParser();
            var result = parser.GetAllTagsByNameAsStrings("[{ \"content\" : null }]", "content");
            Assert.AreEqual("null", result[0]);
        }
    }
}
