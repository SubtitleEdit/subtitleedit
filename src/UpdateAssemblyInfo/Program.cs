using System;
using System.IO;
using System.Text;

namespace UpdateAssemblyInfo
{
    internal class Program
    {

        private class Template
        {

            private string templateFile;
            private string templateText;

            public Template(string path)
            {
                templateFile = path;
            }

            public void Replace(string source, string replacement)
            {
                if (templateText == null)
                {
                    templateText = File.ReadAllText(templateFile);
                }
                templateText = templateText.Replace(source, replacement);
            }

            public void Save(string target)
            {
                File.WriteAllText(target, templateText, Encoding.UTF8);
            }

        }

        private static int Main(string[] args)
        {
            var myName = Environment.GetCommandLineArgs()[0];
            myName = Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(myName)
                   ? System.Reflection.Assembly.GetEntryAssembly().Location
                   : myName);

            if (args.Length != 2)
            {
                Console.Write("Usage: " + myName + @" <template> <target>
  <template> Path to the template file with [GITHASH] and [REVNO]
  <target>   Path to the target file (AssemblyInfo.cs)
");
                return 1;
            }

            Console.WriteLine("Updating assembly info...");
            var workingFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var templateFile = args[0];
            var targetFile = args[1];
            var template = new Template(templateFile);

            var clrHash = new CommandLineRunner();
            var clrTags = new CommandLineRunner();
            var gitPath = GetGitPath();
            string exceptionMessage;
            if (clrHash.RunCommandAndGetOutput(gitPath, "rev-parse --verify HEAD", workingFolder) &&
                clrTags.RunCommandAndGetOutput(gitPath, "describe --tags", workingFolder))
            {
                try
                {
                    template.Replace("[GITHASH]", clrHash.Result);
                    if (clrTags.Result.IndexOf('-') < 0)
                        clrTags.Result += "-0";
                    template.Replace("[REVNO]", clrTags.Result.Split('-')[1]);
                    template.Save(targetFile);
                    return 0;
                }
                catch (Exception ex)
                {
                    exceptionMessage = ex.Message;
                }
            }
            else
            {
                try
                {
                    // allow to compile without git
                    Console.WriteLine("WARNING: Could not run Git - build number will be 9999!");
                    template.Replace("[GITHASH]", string.Empty);
                    template.Replace("[REVNO]", "9999");
                    template.Save(targetFile);
                    return 0;
                }
                catch (Exception ex)
                {
                    exceptionMessage = ex.Message;
                }
            }
            Console.WriteLine(myName + ": Could not update AssemblyInfo: " + exceptionMessage);
            Console.WriteLine(" - Git folder: " + workingFolder);
            Console.WriteLine(" - Template: " + templateFile);
            Console.WriteLine(" - Target: " + targetFile);
            return 1;
        }

        private static string GetGitPath()
        {
            var envPath = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrWhiteSpace(envPath))
            {
                foreach (var p in envPath.Split(Path.PathSeparator))
                {
                    var path = Path.Combine(p, "git.exe");
                    if (File.Exists(path))
                        return path;
                }
            }

            var gitPath = Path.Combine("Git", "bin", "git.exe");

            var envProgramFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            if (!string.IsNullOrWhiteSpace(envProgramFiles))
            {
                var path = Path.Combine(envProgramFiles, gitPath);
                if (File.Exists(path))
                    return path;
            }

            envProgramFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            if (!string.IsNullOrWhiteSpace(envProgramFiles))
            {
                var path = Path.Combine(envProgramFiles, gitPath);
                if (File.Exists(path))
                    return path;
            }

            var envSystemDrive = Environment.GetEnvironmentVariable("SystemDrive");
            if (!string.IsNullOrWhiteSpace(envSystemDrive))
            {
                var path = Path.Combine(envSystemDrive, "Program Files", gitPath);
                if (File.Exists(path))
                    return path;

                path = Path.Combine(envSystemDrive, "Program Files (x86)", gitPath);
                if (File.Exists(path))
                    return path;

                path = Path.Combine(envSystemDrive, gitPath);
                if (File.Exists(path))
                    return path;
            }

            try
            {
                var cRoot = new DriveInfo("C").RootDirectory.FullName;
                if (!cRoot.StartsWith(envSystemDrive, StringComparison.OrdinalIgnoreCase))
                {
                    var path = Path.Combine(cRoot, "Program Files", gitPath);
                    if (File.Exists(path))
                        return path;

                    path = Path.Combine(cRoot, "Program Files (x86)", gitPath);
                    if (File.Exists(path))
                        return path;

                    path = Path.Combine(cRoot, gitPath);
                    if (File.Exists(path))
                        return path;
                }
            }
            catch { }

            Console.WriteLine("WARNING: Might not be able to run Git command line tool!");
            return "git";
        }

    }
}
