using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UpdateLanguageFiles
{
    internal class Program
    {
        private static readonly Regex TemplateFileVersionRegex; // e.g.: [assembly: AssemblyVersion("3.4.8.[REVNO]")]
        static Program()
        {
            var options = RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Multiline;
            TemplateFileVersionRegex = new Regex(@"^[ \t]*\[assembly:[ \t]*AssemblyVersion\(""(?<version>[0-9]+\.[0-9]+\.[0-9]+)\.\[REVNO\]""\)\]", options);
        }

        private const string WorkInProgress = "Updating language files...";

        private static void WriteWarning(string message)
        {
            Console.WriteLine();
            Console.WriteLine("WARNING: " + message);
            Console.Write(WorkInProgress);
        }

        private static async Task<int> Main(string[] args)
        {
            var myName = Environment.GetCommandLineArgs()[0];
            myName = Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(myName) ? System.Reflection.Assembly.GetEntryAssembly()?.Location : myName);
            if (args.Length != 2)
            {
                Console.Write("Usage: " + myName + @" <LanguageBaseEnglish> <LanguageDeserializer>
  <LanguageBaseEnglish>  Path to the LanguageBaseEnglish xml file (LanguageBaseEnglish.xml)
  <LanguageDeserializer> Path to the LanguageDeserializer source code file (LanguageDeserializer.cs)
");
                return 1;
            }

            Console.Write(WorkInProgress);

            try
            {
                var noOfChanges = 0;

                var language = new Nikse.SubtitleEdit.Logic.Language { General = { Version = FindVersionNumber() } };
                var languageAsXml = language.GetCurrentLanguageAsXml();
                var oldLanguageAsXml = string.Empty;
                if (File.Exists(args[0]))
                {
                    oldLanguageAsXml = File.ReadAllText(args[0]);
                }

                if (oldLanguageAsXml != languageAsXml)
                {
                    await SaveWithRetry(() => language.Save(args[0]));
                    noOfChanges++;
                    Console.Write(" {0} generated...", Path.GetFileName(args[0]));
                }

                var languageDeserializerContent = LanguageDeserializerGenerator.GenerateCSharpXmlDeserializerForLanguage();
                var oldLanguageDeserializerContent = string.Empty;
                if (File.Exists(args[1]))
                {
                    oldLanguageDeserializerContent = File.ReadAllText(args[1]);
                }

                if (oldLanguageDeserializerContent != languageDeserializerContent)
                {
                    await SaveWithRetry(() => File.WriteAllText(args[1], languageDeserializerContent, Encoding.UTF8));
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
            catch (Exception exception)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR: " + myName + ": " + exception.Message + Environment.NewLine + exception.StackTrace);

                return 2;
            }
        }

        private static async Task SaveWithRetry(Action saveAction)
        {
            const int maxRetries = 10;
            var delayBetweenRetries = TimeSpan.FromMilliseconds(10);

            for (var i = 0; i <= maxRetries; i++)
            {
                try
                {
                    saveAction();
                    return;
                }
                catch
                {
                    if (i == maxRetries)
                    {
                        throw;  // Rethrow the original exception if maximum attempts have been tried
                    }

                    await Task.Delay(delayBetweenRetries).ConfigureAwait(false);
                }
            }
        }
        
        private static string FindVersionNumber()
        {
            var templateFileName = Path.Combine("src", "ui", "Properties", "AssemblyInfo.cs.template");
            var count = 0;
            while (!File.Exists(templateFileName))
            {
                templateFileName = Path.Combine("..", templateFileName);
                
                if (count++ == 5)
                {
                    break;
                }
            }

            if (File.Exists(templateFileName))
            {
                var templateText = File.ReadAllText(templateFileName);
                var versionMatch = TemplateFileVersionRegex.Match(templateText);
                if (versionMatch.Success)
                {
                    return versionMatch.Groups["version"].Value;
                }
                WriteWarning("No valid AssemblyVersion in template file '" + Path.GetFullPath(templateFileName) + "'.");
            }
            else
            {
                WriteWarning("Template file '" + Path.GetFileName(templateFileName) + "' not found.");
            }

            return "unknown";
        }
    }
}
