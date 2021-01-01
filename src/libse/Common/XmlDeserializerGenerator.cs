//using System;
//using System.Text;

//namespace Nikse.SubtitleEdit.Core.Common
//{
//    public static class XmlDeserializerGenerator
//    {

//        public static string GenerateCSharpXmlDeserializerForLanguageStructure()
//        {
//            var sb = new StringBuilder();
//            sb.AppendLine(@"using System.IO;
//using System.Xml;

//namespace Nikse.SubtitleEdit.Logic
//{

//    public class LanguageDeserializer
//    {

//        public static Language CustomDeserializeLanguage(string fileName)
//        {
//            var doc = new XmlDocument();
//            doc.PreserveWhitespace = true;

//            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
//            doc.Load(stream);
//            stream.Close();

//            XmlNode node = doc.DocumentElement;
//            XmlNode subNode;
//            var language = new Language();
//");
//            sb.AppendLine(GenerateCSharpXmlDeserializer(typeof(Language), "language", string.Empty));
//            sb.AppendLine();
//            sb.AppendLine("\t\t\treturn language;");
//            sb.AppendLine("\t\t}");
//            sb.AppendLine("\t}");
//            sb.AppendLine("}");
//            return sb.ToString();
//        }

//        private static string GenerateCSharpXmlDeserializer(Type classType, string currentName, string xmlPath)
//        {
//            xmlPath = xmlPath.Trim('/');

//            var sb = new StringBuilder();

//            var properties = classType.GetProperties();
//            if (properties.Length == 0)
//            {
//                var fields = classType.GetFields();
//                foreach (var fieldInfo in fields)
//                {
//                    if (fieldInfo.FieldType.Name == "String")
//                    {
//                        sb.AppendLine("\t\t\tsubNode = node.SelectSingleNode(\"" + fieldInfo.Name + "\");");
//                        sb.AppendLine("\t\t\tif (subNode != null)");
//                        sb.AppendLine("\t\t\t\t" + currentName + "." + fieldInfo.Name + " = subNode.InnerText;");
//                    }
//                }
//                foreach (var fieldInfo in fields)
//                {
//                    if (fieldInfo.FieldType.Name != "String" && fieldInfo.FieldType.FullName.Contains("LanguageStructure"))
//                    {
//                        sb.AppendLine();
//                        sb.AppendLine("\t\t\t" + currentName + "." + fieldInfo.Name + " = new " + fieldInfo.FieldType.FullName.Replace("+", ".") + "();");
//                        sb.AppendLine("\t\t\tnode = doc.DocumentElement.SelectSingleNode(\"" + fieldInfo.Name + "\");");
//                        sb.AppendLine("\t\t\tif (node != null)");
//                        sb.AppendLine("\t\t\t{");
//                        sb.AppendLine(GenerateCSharpXmlDeserializer(fieldInfo.FieldType, currentName + "." + fieldInfo.Name, xmlPath + "/" + fieldInfo.Name + "/"));
//                        sb.AppendLine("\t\t\t}");
//                    }
//                }
//            }
//            else
//            {
//                foreach (var prp in properties)
//                {
//                    if (prp.PropertyType.Name == "String")
//                    {
//                        sb.AppendLine("\t\t\tsubNode = node.SelectSingleNode(\"" + prp.Name + "\");");
//                        sb.AppendLine("\t\t\tif (subNode != null)");
//                        sb.AppendLine("\t\t\t\t" + currentName + "." + prp.Name + " = subNode.InnerText;");
//                    }
//                }
//                foreach (var prp in properties)
//                {
//                    if (prp.PropertyType.Name != "String" && prp.PropertyType.FullName.Contains("LanguageStructure"))
//                    {
//                        sb.AppendLine();
//                        sb.AppendLine("\t\t\t" + currentName + "." + prp.Name + " = new " + prp.PropertyType.FullName.Replace("+", ".") + "();");
//                        sb.AppendLine("\t\t\tnode = doc.DocumentElement.SelectSingleNode(\"" + xmlPath + "/" + prp.Name + "\");");
//                        sb.AppendLine("\t\t\tif (node != null)");
//                        sb.AppendLine("\t\t\t{");
//                        sb.AppendLine(GenerateCSharpXmlDeserializer(prp.PropertyType, currentName + "." + prp.Name, xmlPath + "/" + prp.Name + "/"));
//                        sb.AppendLine("\t\t\t}");
//                    }
//                }
//            }
//            return sb.ToString();
//        }

//    }
//}
