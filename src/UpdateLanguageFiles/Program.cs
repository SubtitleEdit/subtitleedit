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
                Nikse.SubtitleEdit.Logic.Configuration.Settings.Language.Save(args[0]);
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

    }
}
