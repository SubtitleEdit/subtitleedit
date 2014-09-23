using System;
using System.IO;

namespace UpdateAssemblyInfo
{
    internal class Program
    {

        private static string GetGitPath()
        {
            string p = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles"), @"Git\bin\git.exe");
            if (File.Exists(p))
                return p;

            // This variable doesn't get set on every Windows configuration. (@alfaproject vm for example)
            var programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            if (programFiles != null)
            {
                p = Path.Combine(programFiles, @"Git\bin\git.exe");
                if (File.Exists(p))
                    return p;
            }

            p = @"C:\Program Files\Git\bin\git.exe";
            if (File.Exists(p))
                return p;

            p = @"C:\Program Files (x86)\Git\bin\git.exe";
            if (File.Exists(p))
                return p;

            p = @"C:\Git\bin\git.exe";
            if (File.Exists(p))
                return p;

            Console.WriteLine("Warning: Might not be able to find Git command line tool!");
            return "git";
        }

        private static void DoUpdateAssembly(string source, string gitHash, string template, string target)
        {
            string templateData = File.ReadAllText(template);
            string fixedData = templateData.Replace(source, gitHash);
            File.WriteAllText(target, fixedData);
        }

        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("UpdateAssemblyInfo 1.0");
                Console.WriteLine("UpdateAssemblyInfo <template with [GITHASH]> <target file>");
                Console.WriteLine("Wrong number of arguments: " + args.Length);
                return 1;
            }

            Console.WriteLine("Updating assembly info...");
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
                    DoUpdateAssembly("[REVNO]", clrTags.Result.Split('-')[1], target, target);
                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
            else
            {
                try
                {
                    // allow to compile without git
                    Console.WriteLine("Warning: Could not run Git - build number will be 9999!");
                    DoUpdateAssembly("[GITHASH]", string.Empty, template, target);
                    DoUpdateAssembly("[REVNO]", "9999", target, target);
                    return 0;
                }
                catch
                {
                    Console.WriteLine("Error running Git");
                    Console.WriteLine(" - Git folder: " + workingFolder);
                    Console.WriteLine(" - Template: " + template);
                    Console.WriteLine(" - Target: " + target);
                }
            }
            return 1;
        }

    }
}
