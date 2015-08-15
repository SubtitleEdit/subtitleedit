using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Test
{
    [TestClass]
    public class XmlDictionariesTest
    {
        [TestMethod]
        [DeploymentItem("..\\Dictionaries")]
        public void DictionariesValidXml()
        {
            var xmlFileNames = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.xml");
            foreach (var xmlFileName in xmlFileNames)
            {
                TestXmlWellFormedness(xmlFileName);    
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
            if (exception.LineNumber >= 0 && exception.LineNumber < lines.Count())
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
