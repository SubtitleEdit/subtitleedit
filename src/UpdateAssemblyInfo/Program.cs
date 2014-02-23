using System;
using System.IO;

namespace UpdateAssemblyInfo
{
    class Program
    {

        private static string GetGitPath()
        {
            string p = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), @"Git\bin\git.exe");
            if (File.Exists(p))
                return p;

            p = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), @"Git\bin\git.exe");
            if (File.Exists(p))
                return p;

            p = @"C:\Program Files\Git\bin\git.exe";
            if (File.Exists(p))
                return p;

            p = @"C:\Program Files (x86)\Git\bin\git.exe";
            if (File.Exists(p))
                return p;

            p = @"C:\Git\bin\git.exe";
            if (File.Exists(p))
                return p;

            return "git";
        }

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
                Console.WriteLine("UpdateAssemblyDescription 0.9");
                Console.WriteLine("UpdateAssemblyDescription <template with [GITHASH]> <target file>");
                File.WriteAllText(errorFileName, "Wrong number of arguments: " + args.Length);
                return 1;
            }

            string workingFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string template = args[0];
            string target = args[1];

            var clrHash = new CommandLineRunner();
            var rcGitHash = clrHash.RunCommandAndGetOutput(GetGitPath(), "rev-parse --verify HEAD", workingFolder);
            var clrTags = new CommandLineRunner();
            if (rcGitHash && clrTags.RunCommandAndGetOutput(GetGitPath(), "describe --tags", workingFolder))
            {
                try
                {
                    DoUpdateAssembly("[GITHASH]", clrHash.Result, template, target);
                    if (!clrTags.Result.Contains("-"))
                        clrTags.Result += "-0";
                    DoUpdateAssembly("[REVNO]", clrTags.Result.Split('-')[1] , target, target);
                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    File.WriteAllText(errorFileName, e.Message);
                }
            }
            else
            {
                try
                {
                    // allow for compile without git
                    DoUpdateAssembly("[GITHASH]", string.Empty, template, target);
                    DoUpdateAssembly("[REVNO]", "0", target, target);
                }
                catch
                {
                }

                Console.WriteLine("Error running Git");
                Console.WriteLine(" - git folder: " + workingFolder);
                Console.WriteLine(" - template: " + template);
                Console.WriteLine(" - target: " + target);
                File.WriteAllText(errorFileName, " - git folder: " + workingFolder + Environment.NewLine +
                                                 " - template: " + template + Environment.NewLine +
                                                 " - target: " + target);
            }
            return 1;
        }

    }
}
