using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    public static class LanguageDeserializerGenerator
    {

        public static string GenerateCSharpXmlDeserializerForLanguage_VIA_XML_DOCUMENT_WHICH_IS_SLOW()
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"using System.IO;
using System.Xml;

// !!! THIS FILE IS AUTO-GENERATED!!!
// !!! THIS FILE IS AUTO-GENERATED!!!
// !!! THIS FILE IS AUTO-GENERATED!!!

namespace Nikse.SubtitleEdit.Logic
{

    public class LanguageDeserializer
    {

        public static Language CustomDeserializeLanguage(string fileName)
        {
            var doc = new XmlDocument();
            doc.PreserveWhitespace = true;

            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            doc.Load(stream);
            stream.Close();

            XmlNode node = doc.DocumentElement;
            XmlNode subNode;
            var language = new Language();
");
            sb.AppendLine(GenerateCSharpXmlDeserializer(typeof(Language), "language", string.Empty));
            sb.AppendLine();
            sb.AppendLine("\t\t\treturn language;");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            System.IO.File.WriteAllText(@"C:\Data\subtitleedit\subtitleedit\src\Logic\LanguageDeserializer.cs", sb.ToString());
            return sb.ToString();
        }

        private static string GenerateCSharpXmlDeserializer(Type classType, string currentName, string xmlPath)
        {
            xmlPath = xmlPath.Trim('/');

            var sb = new StringBuilder();

            var properties = classType.GetProperties();
            if (properties.Length == 0)
            {
                var fields = classType.GetFields();
                foreach (var fieldInfo in fields)
                {
                    if (fieldInfo.FieldType.Name == "String")
                    {
                        sb.AppendLine("\t\t\t\tsubNode = node.SelectSingleNode(\"" + fieldInfo.Name + "\");");
                        sb.AppendLine("\t\t\t\tif (subNode != null)");
                        sb.AppendLine("\t\t\t\t" + currentName + "." + fieldInfo.Name + " = subNode.InnerText;");
                    }
                }
                foreach (var fieldInfo in fields)
                {
                    if (fieldInfo.FieldType.Name != "String" && fieldInfo.FieldType.FullName.Contains("LanguageStructure"))
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t" + currentName + "." + fieldInfo.Name + " = new " + fieldInfo.FieldType.FullName.Replace("+", ".") + "();");
                        sb.AppendLine("\t\t\tnode = doc.DocumentElement.SelectSingleNode(\"" + fieldInfo.Name + "\");");
                        sb.AppendLine("\t\t\tif (node != null)");
                        sb.AppendLine("\t\t\t{");
                        sb.AppendLine(GenerateCSharpXmlDeserializer(fieldInfo.FieldType, currentName + "." + fieldInfo.Name, xmlPath + "/" + fieldInfo.Name + "/"));
                        sb.AppendLine("\t\t\t}");
                    }
                }
            }
            else
            {
                foreach (var prp in properties)
                {
                    if (prp.PropertyType.Name == "String")
                    {
                        sb.AppendLine("\t\t\tsubNode = node.SelectSingleNode(\"" + prp.Name + "\");");
                        sb.AppendLine("\t\t\tif (subNode != null)");
                        sb.AppendLine("\t\t\t\t" + currentName + "." + prp.Name + " = subNode.InnerText;");
                    }
                }
                foreach (var prp in properties)
                {
                    if (prp.PropertyType.Name != "String" && prp.PropertyType.FullName.Contains("LanguageStructure"))
                    {
                        sb.AppendLine();
                        sb.AppendLine("\t\t\t" + currentName + "." + prp.Name + " = new " + prp.PropertyType.FullName.Replace("+", ".") + "();");
                        sb.AppendLine("\t\t\tnode = doc.DocumentElement.SelectSingleNode(\"" + xmlPath + "/" + prp.Name + "\");");
                        sb.AppendLine("\t\t\tif (node != null)");
                        sb.AppendLine("\t\t\t{");                        
                        sb.AppendLine(GenerateCSharpXmlDeserializer(prp.PropertyType, currentName + "." + prp.Name, xmlPath + "/" + prp.Name + "/"));
                        sb.AppendLine("\t\t\t}");
                    }
                }
            }
            return sb.ToString();
        }


        public static void GenerateCSharpXmlDeserializerForLanguage()
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"using System.IO;
using System.Xml;
using Nikse.SubtitleEdit.Logic;

// !!! THIS FILE IS AUTO-GENERATED!!!
// !!! THIS FILE IS AUTO-GENERATED!!!
// !!! THIS FILE IS AUTO-GENERATED!!!

namespace Nikse.SubtitleEdit.Logic
{

    public class LanguageDeserializer // NOTE: This class is AUTO-GENERATED!!!! (Choose language + press ctrl+alt+shift+C to generate)
    {

        public static Language CustomDeserializeLanguage(string fileName)
        {
            string name = string.Empty;
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var language = new Language();
            [NewObjects]
            using (XmlReader reader = XmlReader.Create(stream))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (name.Length > 0 || reader.Name != " + "\"Language\"" + @")
                          name = (name + " + "\"/\" + " + @"reader.Name).TrimStart('/');
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        int idx = name.LastIndexOf(" +"\"/\"" + @");
                        if (idx > 0)
                            name = name.Remove(idx);
                        else
                            name = string.Empty;
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        SetValue(language, reader, name);
                    }
                }
            }
            stream.Close();
            return language;
        }

        private static void SetValue(Language language, XmlReader reader, string name)
        {
            switch (name)
            {");

            var language = new Language();
            var newObjectsString = new StringBuilder();
            sb.AppendLine(SubElementDeserializer(typeof(Language), "language", string.Empty, newObjectsString));
            sb.AppendLine();
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            System.IO.File.WriteAllText(@"C:\Data\subtitleedit\subtitleedit\src\Logic\LanguageDeserializer.cs", sb.ToString().Replace("[NewObjects]", newObjectsString.ToString()));
        }

        private static string SubElementDeserializer(Type classType, string currentName, string xmlPath, StringBuilder newObjectsString)
        {
            xmlPath = xmlPath.Trim('/');

            var sb = new StringBuilder();

            var properties = classType.GetProperties();
            if (properties.Length == 0)
            {
                var fields = classType.GetFields();
                foreach (var fieldInfo in fields)
                {
                    if (fieldInfo.FieldType.Name == "String")
                    {
                        sb.AppendLine("\t\t\t\tcase \"" + (xmlPath + "/" + fieldInfo.Name).TrimStart('/') + "\": ");
                        sb.AppendLine("\t\t\t\t\tlanguage." + (xmlPath.Replace("/", ".") + ".").TrimStart('.') + fieldInfo.Name + " = reader.Value;");
                        sb.AppendLine("\t\t\t\t\tbreak;");
                    }
                }
                foreach (var fieldInfo in fields)
                {
                    if (fieldInfo.FieldType.Name != "String" && fieldInfo.FieldType.FullName.Contains("LanguageStructure"))
                    {
                        newObjectsString.AppendLine("\t\t\t" + currentName + "." + fieldInfo.Name + " = new " + fieldInfo.FieldType.FullName.Replace("+", ".") + "();");
                        sb.AppendLine(SubElementDeserializer(fieldInfo.FieldType, currentName + "." + fieldInfo.Name, xmlPath + "/" + fieldInfo.Name + "/", newObjectsString).TrimEnd());
                    }
                }
            }
            else
            {
                foreach (var prp in properties)
                {
                    if (prp.PropertyType.Name == "String")
                    {
                        sb.AppendLine("\t\t\t\tcase \"" + xmlPath + "/" + prp.Name + "\":");
                        sb.AppendLine("\t\t\t\t\tlanguage." + xmlPath.Replace("/", ".") + "." + prp.Name + " = reader.Value;");
                        sb.AppendLine("\t\t\t\t\tbreak;");
                    }
                }
                foreach (var prp in properties)
                {
                    if (prp.PropertyType.Name != "String" && prp.PropertyType.FullName.Contains("LanguageStructure"))
                    {
                        newObjectsString.AppendLine("\t\t\t" + currentName + "." + prp.Name + " = new " + prp.PropertyType.FullName.Replace("+", ".") + "();");
                        sb.AppendLine(SubElementDeserializer(prp.PropertyType, currentName + "." + prp.Name, xmlPath + "/" + prp.Name + "/", newObjectsString).TrimEnd());
                    }
                }
            }
            return sb.ToString();
        }

    }
}

