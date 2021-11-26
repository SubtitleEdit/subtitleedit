using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Xml;

namespace Test.Logic.Dictionaries
{
    [TestClass]
    public class LanguageFileTest
    {
        [TestMethod]
        public void TestLanguageFiles()
        {
            var idx = Environment.CurrentDirectory.IndexOf(Path.DirectorySeparatorChar + "subtitleedit" + Path.DirectorySeparatorChar);
            var baseDir = Environment.CurrentDirectory.Substring(0, idx);
            var languageDir = Path.Combine(baseDir, "subtitleedit", "src", "ui", "Languages");
            var files = Directory.GetFiles(languageDir, "*.xml");

            foreach (var fileName in files)
            {
                var xml = new XmlDocument();
                try
                {
                    xml.Load(fileName);

                    var cultureName = xml.DocumentElement.SelectSingleNode("General/CultureName").InnerText;
                    var fileNameStart = Path.GetFileName(fileName).Substring(0, cultureName.Length);
                    Assert.AreEqual(fileNameStart, cultureName);
                }
                catch (Exception exception)
                {
                    throw new Exception($"Unable to load language file {fileName} : {exception.Message}");
                }
            }
        }
    }
}
