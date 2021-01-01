using Nikse.SubtitleEdit.Logic;
using System;
using System.Text;

namespace UpdateLanguageFiles
{
    public static class LanguageDeserializerGenerator
    {

        public static string GenerateCSharpXmlDeserializerForLanguage()
        {
            var sb = new StringBuilder(@"using System.IO;
using System.Text;
using System.Xml;

// !!! THIS FILE IS AUTO-GENERATED!!!
// !!! THIS FILE IS AUTO-GENERATED!!!
// !!! THIS FILE IS AUTO-GENERATED!!!

namespace Nikse.SubtitleEdit.Logic
{

    public class LanguageDeserializer // NOTE: This class is AUTO-GENERATED!!!!
    {

        public static Language CustomDeserializeLanguage(string fileName)
        {
            var name = new StringBuilder(100, 1000);
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var language = new Language();

            using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings {
                   IgnoreWhitespace = true, IgnoreProcessingInstructions = true, IgnoreComments = true,
                   DtdProcessing = DtdProcessing.Ignore, CheckCharacters = false, CloseInput = true }))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (!reader.IsEmptyElement && reader.Depth > 0)
                        {
                            name.Append('/').Append(reader.Name);
                        }
                        else if (reader.Depth == 0)
                        {
                            language.Name = reader[""Name""];
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        if (name.Length > 0)
                        {
                            name.Length -= reader.Name.Length + 1;
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        SetValue(language, reader, name.ToString(1, name.Length - 1));
                    }
                }
            }
            return language;
        }

        private static void SetValue(Language language, XmlReader reader, string name)
        {
            switch (name)
            {
");

            sb.AppendLine(SubElementDeserializer(typeof(Language), string.Empty));
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString().Replace("Nikse.SubtitleEdit.Logic.", string.Empty).Replace("\t", "    ").Replace(" " + Environment.NewLine, Environment.NewLine);
        }

        private static string SubElementDeserializer(Type classType, string xmlPath)
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
                        sb.AppendLine("\t\t\t\t\tlanguage." + (xmlPath.Replace('/', '.') + "." + fieldInfo.Name).TrimStart('.') + " = reader.Value;");
                        sb.AppendLine("\t\t\t\t\tbreak;");
                    }
                }
                foreach (var fieldInfo in fields)
                {
                    if (fieldInfo.FieldType.Name != "String" && fieldInfo.FieldType.FullName.Contains("LanguageStructure"))
                    {
                        sb.AppendLine(SubElementDeserializer(fieldInfo.FieldType, xmlPath + "/" + fieldInfo.Name + "/").TrimEnd());
                    }
                }
            }
            else
            {
                foreach (var prp in properties)
                {
                    if (prp.PropertyType.Name == "String")
                    {
                        sb.AppendLine("\t\t\t\tcase \"" + (xmlPath + "/" + prp.Name).TrimStart('/') + "\":");
                        sb.AppendLine("\t\t\t\t\tlanguage." + (xmlPath.Replace('/', '.') + "." + prp.Name).TrimStart('.') + " = reader.Value;");
                        sb.AppendLine("\t\t\t\t\tbreak;");
                    }
                }
                foreach (var prp in properties)
                {
                    if (prp.PropertyType.Name != "String" && prp.PropertyType.FullName.Contains("LanguageStructure"))
                    {
                        sb.AppendLine(SubElementDeserializer(prp.PropertyType, xmlPath + "/" + prp.Name + "/").TrimEnd());
                    }
                }
            }
            return sb.ToString();
        }
    }
}
