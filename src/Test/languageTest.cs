using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Logic;
using System.Xml;

namespace Test
{
    /// <summary>
    /// Summary description for languageTest
    /// </summary>
    [TestClass]
    public class languageTest
    {
        /// <summary>
        /// Load a list of currently existing languages
        /// </summary>
        public languageTest()
        {
            _list = new List<string>();
            if (Directory.Exists(Path.Combine(Configuration.BaseDirectory, "Languages")))
            {
                string[] versionInfo = Utilities.AssemblyVersion.Split('.');
                string currentVersion = String.Format("{0}.{1}", versionInfo[0], versionInfo[1]);
                if (versionInfo.Length >= 3 && versionInfo[2] != "0")
                    currentVersion += "." + versionInfo[2];

                foreach (string fileName in Directory.GetFiles(Path.Combine(Configuration.BaseDirectory, "Languages"), "*.xml"))
                {
                    XmlDocument doc = new XmlDocument();
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

        [TestMethod]
        public void TestAllLanguageTranslationsExists()
        {
            Language defaultlang = new Language(); //Load the English version
            defaultlang.General.TranslatedBy = "Translated by ..."; // to avoid assertion

            foreach (String cultureName in _list) //Loop over all language files
            {
                //Load language
                var reader = new System.IO.StreamReader(Path.Combine(Configuration.BaseDirectory, "Languages") + Path.DirectorySeparatorChar + cultureName + ".xml");
                Language lang = Language.Load(reader);

                //Loop over all field in language
                checkFields(cultureName, defaultlang, lang, defaultlang.GetType().GetFields());

                checkProperty(cultureName, defaultlang, lang, defaultlang.GetType().GetProperties());

                //If u want to save a kind of fixed lang file
                //Disabled the assert fail function for it!
              //  lang.Save("Languagesnew\\" + cultureName + ".xml");
                reader.Close();
            }
        }

        private void checkFields(string cultureName, object completeLang, object cultureLang, FieldInfo[] fields)
        {
            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.IsPublic && fieldInfo.FieldType.Namespace.Equals("Nikse.SubtitleEdit.Logic")) {
                    object completeLangatt = fieldInfo.GetValue(completeLang);
                    object cultureLangatt = fieldInfo.GetValue(cultureLang);

                    if ((cultureLangatt == null) || (completeLangatt == null))
                    {
                        Assert.Fail(fieldInfo.Name + " is mssing");
                    }
                    //Console.Out.WriteLine("Field: " + fieldInfo.Name + " checked of type:" + fieldInfo.FieldType.FullName);
                    if (!fieldInfo.FieldType.FullName.Equals("System.String"))
                    {
                        checkFields(cultureName, completeLang, cultureLang, fieldInfo.FieldType.GetFields());
                        checkProperty(cultureName, completeLangatt, cultureLangatt, fieldInfo.FieldType.GetProperties());
                    }
                    else
                    {
                        Assert.Fail("no expecting a string here");
                    }
                }
            }
        }

        private void checkProperty(string cultureName, object completeLang, object cultureLang, PropertyInfo[] properties)
        {
            foreach (PropertyInfo propertie in properties)
            {
                if (propertie.CanRead && propertie.Name != "HelpFile")
                {
                    object completeLangValue = propertie.GetValue(completeLang, null);
                    object cultureLangValue = propertie.GetValue(cultureLang, null);
                    //If the translated version is null there is a error (also the english version is not allowed to be null)
                    if ((cultureLangValue == null) || (completeLangValue == null) || (String.IsNullOrWhiteSpace(completeLangValue.ToString())))
                    {
                        Assert.Fail(propertie.Name + " is mssing in language " + cultureName);
                        propertie.SetValue(cultureLang, completeLangValue, null);
                        //Console.Out.WriteLine(propertie.Name + " inserted");
                    }
                    //Console.Out.WriteLine("propertie: " + propertie.Name + " checked of type:" + propertie.PropertyType.FullName);
                    if (!propertie.PropertyType.FullName.Equals("System.String"))
                    {
                        checkFields(cultureName, completeLangValue, cultureLangValue, propertie.PropertyType.GetFields());
                        checkProperty(cultureName, completeLangValue, cultureLangValue, propertie.PropertyType.GetProperties());
                    }
                }
            }
        }
    }

}
