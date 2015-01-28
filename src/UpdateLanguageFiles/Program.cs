using System;
using System.IO;
using System.Text;

namespace UpdateLanguageFiles
{
    internal class Program
    {

        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("UpdateLanguageFiles - wrong number of arguments: " + args.Length);
                return 1;
            }

            Console.Write("Updating language files...");

            try
            {
                int noOfChanges = 0;
                var language = new Nikse.SubtitleEdit.Logic.Language();
                language.General.Version = FindVersionNumber();
                string currentLanguageAsXml = language.GetCurrentLanguageAsXml();
                string oldLanguageAsXml = File.ReadAllText(args[0]);
                if (oldLanguageAsXml != currentLanguageAsXml)
                {
                    language.Save(args[0]);
                    noOfChanges++;
                    Console.Write(" {0} generated...", Path.GetFileName(args[0]));
                }

                string languageDeserializerContent = Nikse.SubtitleEdit.Logic.LanguageDeserializerGenerator.GenerateCSharpXmlDeserializerForLanguage();
                string languageDeserializerContentOld = string.Empty;
                if (File.Exists(args[1]))
                    languageDeserializerContentOld = File.ReadAllText(args[1]);
                if (languageDeserializerContent != languageDeserializerContentOld)
                {
                    File.WriteAllText(args[1], languageDeserializerContent, Encoding.UTF8);
                    noOfChanges++;
                    Console.Write(" {0} generated...", Path.GetFileName(args[1]));
                }

                if (noOfChanges == 0)
                {
                    Console.WriteLine(" no changes");
                }
                else
                {
                    Console.WriteLine();
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("UpdateLanguageFiles - " + ex.Message);
                return 2;
            }
        }

        private static string FindVersionNumber()
        {
            var fileName = @"src\Properties\AssemblyInfo.cs.template";
            if (!File.Exists(fileName))
                fileName = @"..\src\Properties\AssemblyInfo.cs.template";
            if (!File.Exists(fileName))
                fileName = @"..\..\src\Properties\AssemblyInfo.cs.template";
            if (!File.Exists(fileName))
                fileName = @"..\..\..\src\Properties\AssemblyInfo.cs.template";
            if (!File.Exists(fileName))
                fileName = @"..\..\..\..\src\Properties\AssemblyInfo.cs.template";
            if (!File.Exists(fileName))
                fileName = @"..\..\..\..\..\src\Properties\AssemblyInfo.cs.template";

            if (File.Exists(fileName))
            {
                string text = File.ReadAllText(fileName);
                string tag = "[assembly: AssemblyVersion(\"";
                int start = text.IndexOf(tag);
                if (start > 0)
                {
                    var arr = text.Substring(start + tag.Length, 8).Split('.');
                    if (arr.Length > 2)
                        return string.Format("{0}.{1}.{2}", arr[0], arr[1], arr[2]);
                }
            }
            return "unknown";
        }

    }
}
