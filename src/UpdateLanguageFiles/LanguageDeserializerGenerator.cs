using System;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    public static class LanguageDeserializerGenerator
    {

        public static string GenerateCSharpXmlDeserializerForLanguage()
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

    public class LanguageDeserializer // NOTE: This class is AUTO-GENERATED!!!!
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
                        if ((name.Length > 0 || string.CompareOrdinal(reader.Name, " + "\"Language\")" + @" != 0) && !reader.IsEmptyElement)
                            name = (name + " + "\"/\" + " + @"reader.Name).TrimStart('/');
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        int idx = name.LastIndexOf(" + "\"/\"" + @", System.StringComparison.Ordinal);
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
            return language;
        }

        private static void SetValue(Language language, XmlReader reader, string name)
        {
            switch (name)
            {");

            var newObjectsString = new StringBuilder();
            sb.AppendLine(SubElementDeserializer(typeof(Language), "language", string.Empty, newObjectsString));
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString().Replace("[NewObjects]", newObjectsString.ToString()).Replace("Nikse.SubtitleEdit.Logic.", string.Empty).Replace("\t", "    ").Replace(" " + Environment.NewLine, Environment.NewLine);
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
                        sb.AppendLine("\t\t\t\tcase \"" + (xmlPath + "/" + fieldInfo.Name).TrimStart('/') + "\":");
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
