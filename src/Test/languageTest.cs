using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nikse.SubtitleEdit.Logic;

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
                foreach (string fileName in Directory.GetFiles(Path.Combine(Configuration.BaseDirectory, "Languages"), "*.xml"))
                {
                    string cultureName = Path.GetFileNameWithoutExtension(fileName);
                    _list.Add(cultureName);
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


            _list.Remove("ar-EG");
            _list.Remove("bg-BG");
            _list.Remove("ca-ES");
  //          _list.Remove("cs-CZ");
            _list.Remove("de-DE");
            _list.Remove("el-GR");
            _list.Remove("es-ES");
 //           _list.Remove("eu-ES");
            _list.Remove("fa-IR");
            _list.Remove("fr-FR");
            _list.Remove("hr-HR");
            _list.Remove("hu-HU");
            _list.Remove("it-IT");
            _list.Remove("ja-JP");
            _list.Remove("ko-KR");
      //      _list.Remove("nl-NL");
//            _list.Remove("pl-PL");
//            _list.Remove("pt-BR");
//            _list.Remove("pt-PT");
            _list.Remove("ro-RO");
            _list.Remove("ru-RU");
            _list.Remove("sr-Cyrl-RS");
            _list.Remove("sr-Latn-RS");
            _list.Remove("tr-TR");
            _list.Remove("xxx");
            _list.Remove("xxx");
            _list.Remove("sv-SE");

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
