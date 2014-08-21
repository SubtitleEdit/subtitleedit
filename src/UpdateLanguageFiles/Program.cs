using System;
using System.IO;

namespace UpdateLanguageFiles
{
    class Program
    {       

        private static void DoUpdateAssembly(string source, string gitHash, string template, string target)
        {
            string templateData = File.ReadAllText(template);
            string fixedData = templateData.Replace(source, gitHash);
            File.WriteAllText(target, fixedData);
        }

        static int Main(string[] args)
        {
            string errorFileName = System.Reflection.Assembly.GetEntryAssembly().Location.Replace(".exe", "_error.txt");
            if (args.Length != 2)
            {
                Console.WriteLine("UpdateLanguageFiles - wrong number of arguments: " + args.Length);
                File.WriteAllText(errorFileName, "Wrong number of arguments: " + args.Length);
                return 1;
            }

            Console.WriteLine("Updating language files...");

            try
            {
                var language = new Nikse.SubtitleEdit.Logic.Language();
                language.General.Version = FindVersionNumber();
                language.Save(args[0]);

                Nikse.SubtitleEdit.Logic.LanguageDeserializerGenerator.GenerateCSharpXmlDeserializerForLanguage(args[1]);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateLanguageFiles - " + ex.Message);
                File.WriteAllText(errorFileName, ex.Message);
                return 2;
            }
        }

        private static string FindVersionNumber()
        {
            var fileName = @"src\Properties\AssemblyInfo.cs.template";
            if (!File.Exists(fileName))
                fileName = @"..\..\src\Properties\AssemblyInfo.cs.template";
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
