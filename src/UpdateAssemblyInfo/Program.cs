using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace UpdateAssemblyInfo
{
    internal class Program
    {

        private class VersionInfo
        {
            public string Version { get; set; }
            public string RevisionGuid { get; set; }
        }

        private const string DefaultVersion = "1.0.0.0";

        private static void UpdateAssemblyInfo(string templateFileName, VersionInfo newVersionInfo)
        {
            var lines = File.ReadAllLines(templateFileName);
            var sb = new StringBuilder();
            bool change = false;
            foreach (var line in lines)
            {
                var original = line;
                if (newVersionInfo.Version != DefaultVersion)
                { // only replace version info if Git is installed
                    var l = line.Trim();
                    while (l.Contains("  "))
                    {
                        l = l.Replace("  ", " ");
                    }
                    if (l.StartsWith("[assembly: AssemblyVersion", StringComparison.Ordinal) ||
                        l.StartsWith("[assembly:AssemblyVersion", StringComparison.Ordinal) ||
                        l.StartsWith("[assembly: AssemblyFileVersion", StringComparison.Ordinal) ||
                        l.StartsWith("[assembly:AssemblyFileVersion", StringComparison.Ordinal))
                    {
                        int begin = original.IndexOf('"');
                        int end = original.LastIndexOf('"');
                        if (end > begin && begin > 0)
                        {
                            begin++;
                            string oldVersion = original.Substring(begin, end - begin);
                            if (oldVersion != newVersionInfo.Version)
                            {
                                change = true;
                                original = original.Substring(0, begin) + newVersionInfo.Version + original.Remove(0, end);
                            }
                        }
                    }
                    else if (l.StartsWith("[assembly: AssemblyDescription", StringComparison.Ordinal) ||
                             l.StartsWith("[assembly:AssemblyDescription", StringComparison.Ordinal))
                    {
                        int begin = original.IndexOf('"');
                        int end = original.LastIndexOf('"');
                        if (end > begin && begin > 0)
                        {
                            begin++;
                            string oldRevisionGuid = original.Substring(begin, end - begin);
                            if (oldRevisionGuid != newVersionInfo.RevisionGuid)
                            {
                                change = true;
                                original = original.Substring(0, begin) + newVersionInfo.RevisionGuid + original.Remove(0, end);
                            }
                        }
                    }
                }
                sb.AppendLine(original);
            }
            if (change)
            {
                File.WriteAllText(templateFileName.Replace(".template", string.Empty), sb.ToString().Trim());
            }
        }

        private static VersionInfo GetOldVersionNumber(string subtitleEditTemplateFileName)
        {
            var versionInfo = new VersionInfo { Version = DefaultVersion, RevisionGuid = "0" };
            var oldFileName = subtitleEditTemplateFileName.Replace(".template", string.Empty);
            if (File.Exists(oldFileName))
            {
                var lines = File.ReadAllLines(oldFileName);
                foreach (var line in lines)
                {
                    var l = line.Trim();
                    while (l.Contains("  "))
                    {
                        l = l.Replace("  ", " ");
                    }
                    if (l.StartsWith("[assembly: AssemblyVersion", StringComparison.Ordinal) ||
                        l.StartsWith("[assembly:AssemblyVersion", StringComparison.Ordinal) ||
                        l.StartsWith("[assembly: AssemblyFileVersion", StringComparison.Ordinal) ||
                        l.StartsWith("[assembly:AssemblyFileVersion", StringComparison.Ordinal))
                    {
                        int begin = l.IndexOf('"');
                        int end = l.LastIndexOf('"');
                        if (end > begin && begin > 0)
                        {
                            begin++;
                            versionInfo.Version = l.Substring(begin, end - begin);
                        }
                    }
                    else if (l.StartsWith("[assembly: AssemblyDescription", StringComparison.Ordinal) ||
                             l.StartsWith("[assembly:AssemblyDescription", StringComparison.Ordinal))
                    {
                        int begin = l.IndexOf("\"", StringComparison.Ordinal);
                        int end = l.LastIndexOf("\"", StringComparison.Ordinal);
                        if (end > begin && begin > 0)
                        {
                            begin++;
                            versionInfo.RevisionGuid = l.Substring(begin, end - begin);
                        }
                    }
                }
            }
            return versionInfo;
        }

        // e.g.: 3.4.8-226-g7037fef
        private static readonly Regex VersionNumberRegex = new Regex(@"^\d+\.\d+\.\d+\-.+$", RegexOptions.Compiled);

        private static VersionInfo GetNewVersion()
        {
            var versionInfo = new VersionInfo { Version = DefaultVersion, RevisionGuid = "0" };
            var workingFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var clrHash = new CommandLineRunner();
            var clrTags = new CommandLineRunner();
            var gitPath = GetGitPath();
            if (clrHash.RunCommandAndGetOutput(gitPath, "rev-parse --verify HEAD", workingFolder) && clrTags.RunCommandAndGetOutput(gitPath, "describe --tags", workingFolder))
            {
                if (!VersionNumberRegex.IsMatch(clrTags.Result))
                {
                    throw new Exception("Error: Invalid Git version tag (should number.number.number): '" + clrTags.Result + "'");
                }
                versionInfo.RevisionGuid = clrHash.Result;
                versionInfo.Version = clrTags.Result.Substring(0, clrTags.Result.LastIndexOf('-'));
                versionInfo.Version = versionInfo.Version.Replace("-", ".");
            }
            return versionInfo;
        }

        private static int Main(string[] args)
        {
            var myName = Environment.GetCommandLineArgs()[0];
            myName = Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(myName) ? System.Reflection.Assembly.GetEntryAssembly().Location : myName);
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: " + myName + " <se-assmbly-template> <libse-assmbly-template>");
                return 1;
            }

            try
            {
                var seTemplateFileName = Environment.GetCommandLineArgs()[1];
                var libSeTmplateFileName = Environment.GetCommandLineArgs()[2];

                //seTemplateFileName = @"C:\data\SubtitleEdit\subtitleedit\src\Properties\AssemblyInfo.cs.template";
                //libSeTmplateFileName = @"C:\data\SubtitleEdit\subtitleedit\libse\Properties\AssemblyInfo.cs.template";

                Console.Write("Updating version number... ");
                var newVersionInfo = GetNewVersion();
                var oldSeVersion = GetOldVersionNumber(seTemplateFileName);
                var oldLibSeVersion = GetOldVersionNumber(libSeTmplateFileName);

                if (oldSeVersion.RevisionGuid != newVersionInfo.RevisionGuid || oldSeVersion.Version != newVersionInfo.Version ||
                    oldLibSeVersion.RevisionGuid != newVersionInfo.RevisionGuid || oldLibSeVersion.Version != newVersionInfo.Version)
                {
                    if (newVersionInfo.Version == DefaultVersion)
                    {
                        Console.WriteLine("Git not found: AssemblyInfo.cs must be manually updated");
                    }
                    else
                    {
                        Console.WriteLine("updating version number to " + newVersionInfo.Version + " " + newVersionInfo.RevisionGuid);
                    }
                    UpdateAssemblyInfo(seTemplateFileName, newVersionInfo);
                    UpdateAssemblyInfo(libSeTmplateFileName, newVersionInfo);

                    return 0;
                }
                Console.WriteLine("no changes");
                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine("Something bad happened when running " + myName + ": " + exception.Message + Environment.NewLine + exception.StackTrace);
                return 0;
            }
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
            if (string.IsNullOrEmpty(envSystemDrive))
            {
                throw new Exception("Environment.GetEnvironmentVariable('SystemDrive') returned null!");
            }
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
            catch (Exception exception)
            {
                throw new Exception("UpdateAssemblyInfo - GetGitPath: " + exception.Message);
            }

            Console.WriteLine("WARNING: Might not be able to run Git command line tool!");
            return "git";
        }

    }
}