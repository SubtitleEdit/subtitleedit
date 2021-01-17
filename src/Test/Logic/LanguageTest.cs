using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Core.Common;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Test.Logic
{
    /// <summary>
    /// Summary description for languageTest
    /// </summary>
    [TestClass]
    public class LanguageTest
    {
        /// <summary>
        /// Load a list of currently existing languages
        /// </summary>
        public LanguageTest()
        {
            _list = new List<string>();
            if (Directory.Exists(Path.Combine(Configuration.BaseDirectory, "Languages")))
            {
                string[] versionInfo = Utilities.AssemblyVersion.Split('.');
                string currentVersion = string.Format("{0}.{1}.{2}", versionInfo[0], versionInfo[1], versionInfo[2]);

                foreach (string fileName in Directory.GetFiles(Path.Combine(Configuration.BaseDirectory, "Languages"), "*.xml"))
                {
                    var doc = new XmlDocument();
                    doc.Load(fileName);
                    string version = doc.DocumentElement.SelectSingleNode("General/Version").InnerText;
                    if (version == currentVersion)
                    {
                        string cultureName = Path.GetFileNameWithoutExtension(fileName);
                        _list.Add(cultureName);
                    }
                }
            }
            _list.Sort();
        }

        private List<string> _list; //Store the list of existing languages

        //[TestMethod]
        //public void TestAllLanguageTranslationsExists()
        //{
        //    Language defaultlang = new Language(); //Load the English version
        //    defaultlang.General.TranslatedBy = "Translated by ..."; // to avoid assertion

        //    foreach (String cultureName in _list) //Loop over all language files
        //    {
        //        //Load language
        //        var reader = new StreamReader(Path.Combine(Configuration.BaseDirectory, "Languages") + Path.DirectorySeparatorChar + cultureName + ".xml");
        //        Language lang = Language.Load(reader);

        //        //Loop over all field in language
        //        checkFields(cultureName, defaultlang, lang, defaultlang.GetType().GetFields());

        //        checkProperty(cultureName, defaultlang, lang, defaultlang.GetType().GetProperties());

        //        //If u want to save a kind of fixed lang file
        //        //Disabled the assert fail function for it!
        //      //  lang.Save("Languagesnew\\" + cultureName + ".xml");
        //        reader.Close();
        //    }
        //}

    }

}
