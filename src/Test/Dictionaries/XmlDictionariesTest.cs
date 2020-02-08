using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Dictionaries
{
    [TestClass]
    public class XmlDictionariesTest
    {
        private const string DictionaryFolder = @"..\..\..\..\Dictionaries";

        [TestMethod]
        [DeploymentItem(DictionaryFolder)]
        public void DictionariesValidXml()
        {
            var xmlFileNames = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.xml");
            Assert.IsTrue(xmlFileNames.Length > 0);
            foreach (var xmlFileName in xmlFileNames)
            {
                TestXmlWellFormedness(xmlFileName);
            }
        }

        [TestMethod]
        [DeploymentItem(DictionaryFolder)]
        public void DictionariesValidReplaceListRegEx()
        {
            var xmlFileNames = Directory.GetFiles(Directory.GetCurrentDirectory(), "???_OCRFixReplaceList.xml");
            foreach (var xmlFileName in xmlFileNames)
            {
                try
                {
                    var doc = XDocument.Load(xmlFileName);
                    var regExList = from regEx in doc.Descendants("RegularExpressions").DescendantNodes() select regEx;
                    foreach (var xNode in regExList)
                    {
                        var e = xNode as XElement;
                        if (e != null && e.NodeType == XmlNodeType.Element && e.Attribute("find") != null)
                        {
                            var regExPattern = e.Attribute("find").Value;
                            try
                            {
                                new Regex(regExPattern);
                            }
                            catch (Exception exception)
                            {
                                string msg = Path.GetFileName(xmlFileName) + " has an invalid RegEx find expression: " + regExPattern + Environment.NewLine +
                                             exception.Message;
                                Assert.Fail(msg);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private static void TestXmlWellFormedness(string fileName)
        {
            try
            {
                XDocument.Load(fileName);
            }
            catch (XmlException exception)
            {
                var msg = BuildXmlErrorMessage(fileName, exception);
                Assert.Fail(msg);
            }
        }

        private static string BuildXmlErrorMessage(string fileName, XmlException exception)
        {
            string msg = Path.GetFileName(fileName) + " is not wel-formed." + Environment.NewLine +
                         "Line " + exception.LineNumber + " at position " + exception.LinePosition + Environment.NewLine +
                         exception.Message;

            var lines = File.ReadAllLines(fileName);
            if (exception.LineNumber >= 0 && exception.LineNumber < lines.Length)
            {
                msg += Environment.NewLine + Environment.NewLine;
                if (exception.LineNumber > 1)
                    msg += (exception.LineNumber - 1) + ":  " + lines[exception.LineNumber - 2] + Environment.NewLine;
                if (exception.LineNumber > 0)
                    msg += (exception.LineNumber) + ":  " + lines[exception.LineNumber - 1] + Environment.NewLine;
                msg += (exception.LineNumber + 1) + ":  " + lines[exception.LineNumber];
            }
            return msg;
        }

    }
}
