using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace UpdateAssemblyInfo
{
    internal class Program
    {
        private static readonly Regex LongGitTagRegex; // e.g.: 3.4.8-226-g7037fef
        private static readonly Regex ShortGitTagRegex; // e.g.: 3.4-226-g7037fef
        private static readonly Regex LongVersionRegex; // e.g.: 3.4.8.226
        private static readonly Regex ShortVersionRegex; // e.g.: 3.4.8 (w/o build number)
        private static readonly Regex TemplateFileVersionRegex; // e.g.: [assembly: AssemblyVersion("3.4.8.[REVNO]")]
        private static readonly Regex AssemblyInfoFileVersionRegex; // e.g.: [assembly: AssemblyVersion("3.4.8.226")]
        private static readonly Regex TemplateFileCopyrightRegex; // e.g.: [assembly: AssemblyCopyright("Copyright 2001-2019, Nikse")]
        private static readonly Regex AssemblyInfoFileRevisionGuidRegex; // e.g.: [assembly: AssemblyDescription("0e82e5769c9b235383991082c0a0bba96d20c69d")]

        static Program()
        {
            var options = RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;
            LongGitTagRegex = new Regex(@"\A(?<major>[0-9]+)\.(?<minor>[0-9]+)\.(?<maintenance>[0-9]+)-(?<build>[0-9]+)-g[0-9a-z]+\z", options);
            ShortGitTagRegex = new Regex(@"\A(?<major>[0-9]+)\.(?<minor>[0-9]+)-(?<build>[0-9]+)-g[0-9a-z]+\z", options);
            LongVersionRegex = new Regex(@"\A(?<major>[0-9]+)\.(?<minor>[0-9]+)\.(?<maintenance>[0-9]+)\.(?<build>[0-9]+)\z", options);
            ShortVersionRegex = new Regex(@"\A(?<major>[0-9]+)\.(?<minor>[0-9]+)\.(?<maintenance>[0-9]+)\z", options);
            options |= RegexOptions.Multiline;
            TemplateFileVersionRegex = new Regex(@"^[ \t]*\[assembly:[ \t]*AssemblyVersion\(""(?<version>[0-9]+\.[0-9]+\.[0-9]+)\.\[REVNO\]""\)\]", options);
            AssemblyInfoFileVersionRegex = new Regex(@"^[ \t]*\[assembly:[ \t]*AssemblyVersion\(""(?<version>[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)""\)\]", options);
            TemplateFileCopyrightRegex = new Regex(@"^[ \t]*\[assembly:[ \t]*AssemblyCopyright\(""Copyright 2001-(?<year>[2-9][0-9]{3})[^""]+""\)\]", options);
            AssemblyInfoFileRevisionGuidRegex = new Regex(@"^[ \t]*\[assembly:[ \t]*AssemblyDescription\(""(?<guid>[0-9A-Za-z]*)""\)\]", options);
        }

        private const int UnknownBuild = 9999;

        private class VersionInfo : IComparable<VersionInfo>, IEquatable<VersionInfo>
        {
            private int Major { get; }
            private int Minor { get; }
            private int Maintenance { get; }
            public int Build { get; set; }
            public string RevisionGuid { get; }

            public string FullVersion => string.Format(CultureInfo.InvariantCulture, "{0:D}.{1:D}.{2:D}.{3:D} {4}", Major, Minor, Maintenance, Build, RevisionGuid).TrimEnd();
            public string NormalVersion => string.Format(CultureInfo.InvariantCulture, "{0:D}.{1:D}.{2:D}.{3:D}", Major, Minor, Maintenance, Build).TrimEnd();
            public string ShortVersion => string.Format(CultureInfo.InvariantCulture, "{0:D}.{1:D}.{2:D}", Major, Minor, Maintenance);
            public string MajorMinor => string.Format(CultureInfo.InvariantCulture, "{0:D}.{1:D}", Major, Minor);

            public VersionInfo()
            {
                RevisionGuid = string.Empty;
            }

            public VersionInfo(string version, string guid = null)
            {
                var match = LongGitTagRegex.Match(version);
                if (!match.Success)
                {
                    match = LongVersionRegex.Match(version);
                }

                if (!match.Success)
                {
                    match = ShortGitTagRegex.Match(version);
                }

                if (!match.Success || string.IsNullOrWhiteSpace(guid))
                {
                    Build = UnknownBuild;
                    RevisionGuid = string.Empty;
                }
                else
                {
                    Build = int.Parse(match.Groups["build"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
                    RevisionGuid = guid.Trim().ToLowerInvariant();
                }

                if (!match.Success)
                {
                    match = ShortVersionRegex.Match(version);
                }

                if (!match.Success)
                {
                    throw new ArgumentException($"Invalid version identifier: '{version}'");
                }

                Major = int.Parse(match.Groups["major"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
                Minor = int.Parse(match.Groups["minor"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
                Maintenance = match.Groups["maintenance"].Success ? int.Parse(match.Groups["maintenance"].Value, NumberStyles.None, CultureInfo.InvariantCulture) : 0;
            }

            public int CompareTo(VersionInfo vi)
            {
                var cmp = 1;
                if (!ReferenceEquals(vi, null))
                {
                    cmp = Major.CompareTo(vi.Major);
                    if (cmp == 0)
                    {
                        cmp = Minor.CompareTo(vi.Minor);
                    }
                    if (cmp == 0)
                    {
                        cmp = Maintenance.CompareTo(vi.Maintenance);
                    }
                }

                return cmp;
            }

            public bool Equals(VersionInfo vi)
            {
                return !ReferenceEquals(vi, null) && Major.Equals(vi.Major) && Minor.Equals(vi.Minor) && Maintenance.Equals(vi.Maintenance) && Build.Equals(vi.Build) && RevisionGuid.Equals(vi.RevisionGuid, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as VersionInfo);
            }

            public override int GetHashCode()
            {
                return FullVersion.GetHashCode();
            }

            public static bool operator ==(VersionInfo vi1, VersionInfo vi2)
            {
                return vi2?.Equals(vi1) ?? ReferenceEquals(vi1, null);
            }

            public static bool operator !=(VersionInfo vi1, VersionInfo vi2)
            {
                return !vi2?.Equals(vi1) ?? !ReferenceEquals(vi1, null);
            }

            public static bool operator >(VersionInfo vi1, VersionInfo vi2)
            {
                return !ReferenceEquals(vi1, null) && vi1.CompareTo(vi2) > 0;
            }

            public static bool operator <(VersionInfo vi1, VersionInfo vi2)
            {
                return !ReferenceEquals(vi2, null) && vi2.CompareTo(vi1) > 0;
            }
        }

        private static void UpdateTranslations(string languagesFolderName, VersionInfo newVersion, VersionInfo oldVersion)
        {
            // Valid translation file name formats:
            //   <language-id> "-" <script-id> "-" <region-id> ".xml"  (e.g., sr-Cyrl-RS.xml)
            //   <language-id> "-" <script-id> ".xml"  (e.g., zh-Hans.xml)
            //   <language-id> "-" <region-id> ".xml"  (e.g., nb-NO.xml)
            var fileNamePattern = $@"[\{Path.AltDirectorySeparatorChar}\{Path.DirectorySeparatorChar}][a-z]{{2,3}}-[A-Z][A-Za-z-]+\.xml\z";
            var fileNameRegex = new Regex(fileNamePattern, RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
            var translation = new XmlDocument { XmlResolver = null };

            foreach (var fileName in Directory.EnumerateFiles(languagesFolderName).Where(fn => fileNameRegex.IsMatch(fn)))
            {
                translation.Load(fileName);

                if (translation.DocumentElement?.SelectSingleNode("General/Version") is XmlElement node && node.InnerText.Trim() == oldVersion.ShortVersion)
                {
                    node.InnerText = newVersion.ShortVersion;
                }

                translation.Save(fileName);
            }
        }

        private static void UpdateTmx14ToolVersion(string tmx14FileName, VersionInfo newVersion, VersionInfo oldVersion)
        {
            if (newVersion.MajorMinor != oldVersion.MajorMinor)
            {
                var headerPattern = @"<header +creationtool=\\""Subtitle Edit\\"" +creationtoolversion=\\""(?<version>[^\\]+)\\"" ";
                var headerRegex = new Regex(headerPattern, RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

                var tmx14Text = File.ReadAllText(tmx14FileName);
                var tmx14Match = headerRegex.Match(tmx14Text);
                if (!tmx14Match.Success)
                {
                    throw new FormatException($"Cannot find creationtoolversion attribute in '{tmx14FileName}'");
                }
                var index = tmx14Match.Groups["version"].Index;
                var length = tmx14Match.Groups["version"].Length;
                tmx14Text = tmx14Text.Remove(index, length).Insert(index, newVersion.MajorMinor);

                SaveWithRetry(tmx14FileName, tmx14Text);
            }
        }

        private static void UpdateAssemblyInfo(string templateFileName, VersionInfo newVersion, bool updateTemplateFile = false)
        {
            var templateText = File.ReadAllText(templateFileName).TrimEnd();
            if (updateTemplateFile)
            {
                var templateMatch = TemplateFileVersionRegex.Match(templateText);
                if (!templateMatch.Success)
                {
                    throw new FormatException($"Malformed template file: '{templateFileName}' (missing AssemblyVersion)");
                }
                var index = templateMatch.Groups["version"].Index;
                var length = templateMatch.Groups["version"].Length;
                templateText = templateText.Remove(index, length).Insert(index, newVersion.ShortVersion);

                templateMatch = TemplateFileCopyrightRegex.Match(templateText);
                if (!templateMatch.Success)
                {
                    throw new FormatException($"Malformed template file: '{templateFileName}' (missing AssemblyCopyright)");
                }
                index = templateMatch.Groups["year"].Index;
                length = templateMatch.Groups["year"].Length;
                templateText = templateText.Remove(index, length).Insert(index, DateTime.UtcNow.Year.ToString("D4", CultureInfo.InvariantCulture));

                SaveWithRetry(templateFileName, templateText);
            }

            var assemblyInfoText = templateText.Replace("[REVNO]", newVersion.Build.ToString(CultureInfo.InvariantCulture)).Replace("[GITHASH]", newVersion.RevisionGuid);
            var assemblyInfoFileName = templateFileName.Replace(".template", string.Empty);
            SaveWithRetry(assemblyInfoFileName, assemblyInfoText);
        }

        private static void SaveWithRetry(string fileName, string content)
        {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    File.WriteAllText(fileName, content, Encoding.UTF8);
                    return;
                }
                catch
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
            File.WriteAllText(fileName, content, Encoding.UTF8);
        }

        private static void GetRepositoryVersions(out VersionInfo currentRepositoryVersion, out VersionInfo latestRepositoryVersion)
        {
            var workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            var clrHash = new CommandLineRunner();
            var clrTags = new CommandLineRunner();
            var gitPath = GetGitPath();
            if (clrHash.RunCommandAndGetOutput(gitPath, "rev-parse --verify HEAD", workingDirectory) && clrTags.RunCommandAndGetOutput(gitPath, "describe --long --tags", workingDirectory))
            {
                if (!LongGitTagRegex.IsMatch(clrTags.Result) && !ShortGitTagRegex.IsMatch(clrTags.Result))
                {
                    throw new FormatException($"Invalid Git version tag: '{clrTags.Result}' (major.minor.maintenance-build expected)");
                }
                currentRepositoryVersion = new VersionInfo(clrTags.Result, clrHash.Result);
            }
            else
            {
                currentRepositoryVersion = new VersionInfo(); // no git repository
            }
            if (clrHash.RunCommandAndGetOutput(gitPath, "rev-parse --verify refs/heads/main", workingDirectory) && clrTags.RunCommandAndGetOutput(gitPath, "describe --long --tags refs/heads/main", workingDirectory))
            {
                if (!LongGitTagRegex.IsMatch(clrTags.Result) && !ShortGitTagRegex.IsMatch(clrTags.Result))
                {
                    throw new FormatException("Invalid Git version tag: '{clrTags.Result}' (major.minor.maintenance-build expected)");
                }
                latestRepositoryVersion = new VersionInfo(clrTags.Result, clrHash.Result);
            }
            else
            {
                latestRepositoryVersion = new VersionInfo(); // no git repository
            }
        }

        private static VersionInfo GetCurrentVersion(string templateFileName)
        {
            var assemblyInfoFileName = templateFileName.Replace(".template", string.Empty);
            try
            {
                var assemblyInfoText = File.ReadAllText(assemblyInfoFileName);
                var versionMatch = AssemblyInfoFileVersionRegex.Match(assemblyInfoText);
                var revisionMatch = AssemblyInfoFileRevisionGuidRegex.Match(assemblyInfoText);
                if (versionMatch.Success && revisionMatch.Success)
                {
                    return new VersionInfo(versionMatch.Groups["version"].Value, revisionMatch.Groups["guid"].Value);
                }
            }
            catch
            {
                // ignored
            }

            return new VersionInfo();
        }

        private static VersionInfo GetTemplateVersion(string templateFileName)
        {
            var templateText = File.ReadAllText(templateFileName);
            var versionMatch = TemplateFileVersionRegex.Match(templateText);
            if (!versionMatch.Success)
            {
                throw new FormatException($"Malformed template file: '{templateFileName}' (missing AssemblyVersion)");
            }
            return new VersionInfo(versionMatch.Groups["version"].Value);
        }

        private const string WorkInProgress = "Updating version number...";

        private static void WriteWarning(string message)
        {
            Console.WriteLine();
            Console.WriteLine("WARNING: " + message);
            Console.Write(WorkInProgress);
        }

        private static bool IsRCVersion()
        {
            var workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            var clrTags = new CommandLineRunner();
            var gitPath = GetGitPath();
            if (clrTags.RunCommandAndGetOutput(gitPath, "describe --long --tags", workingDirectory))
            {
                if (clrTags.Result.Contains("RC"))
                {
                    return true;
                }
            }

            return false;
        }

        //debug: "..\..\ui\Properties\AssemblyInfo.cs.template" "..\..\src\libse\Properties\AssemblyInfo.cs.template"
        private static int Main(string[] args)
        {
            var isRC = IsRCVersion();

            var myName = Environment.GetCommandLineArgs()[0];
            myName = Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(myName) ? System.Reflection.Assembly.GetEntryAssembly()?.Location : myName);

            if (!isRC && args.Length == 1 && Environment.GetCommandLineArgs()[1] == "winget")
            {
                GetRepositoryVersions(out var currentRepositoryVersion, out var latestRepositoryVersion);
                UpdateWinGet(latestRepositoryVersion);
                return 0;
            }

            if (args.Length != 2)
            {
                Console.WriteLine("Usage: " + myName + " <se-assmbly-template> <libse-assmbly-template>");
                return 1;
            }

            Console.Write(WorkInProgress);

            try
            {
                var seTemplateFileName = Environment.GetCommandLineArgs()[1];
                var libSeTemplateFileName = Environment.GetCommandLineArgs()[2];
                var currentVersion = GetCurrentVersion(seTemplateFileName);
                var latestRepositoryVersion = GetCurrentVersion(seTemplateFileName);
                var currentRepositoryVersion = GetCurrentVersion(seTemplateFileName);
                if (!isRC)
                {
                    GetRepositoryVersions(out currentRepositoryVersion, out latestRepositoryVersion);
                }

                var updateTemplateFile = false;
                VersionInfo newVersion;
                if (latestRepositoryVersion.RevisionGuid.Length > 0 && currentVersion > latestRepositoryVersion && latestRepositoryVersion == currentRepositoryVersion)
                {
                    // version sequence has been bumped for new release: must update template file too
                    Console.Write(" new release...");
                    updateTemplateFile = true;
                    // build number and revision guid will be unknown until 'git commit' and 'git tag' have been run
                    newVersion = new VersionInfo(currentVersion.ShortVersion);
                }
                else if (currentRepositoryVersion.RevisionGuid.Length == 0)
                {
                    // last resort when building from source tarball - unknown build number and revision guid
                    newVersion = GetTemplateVersion(seTemplateFileName);
                }
                else
                {
                    newVersion = currentRepositoryVersion;
                }

                if (newVersion != currentVersion)
                {
                    Console.WriteLine(" updating version number to " + newVersion.FullVersion);
                    if (updateTemplateFile)
                    {
                        var oldVersion = GetTemplateVersion(seTemplateFileName);
                        var languagesFolderName = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(seTemplateFileName)) ?? throw new InvalidOperationException(), "Languages");
                        UpdateTranslations(languagesFolderName, newVersion, oldVersion);
                        var tmx14FileName = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(libSeTemplateFileName)) ?? throw new InvalidOperationException(), "SubtitleFormats", "Tmx14.cs");
                        UpdateTmx14ToolVersion(tmx14FileName, newVersion, oldVersion);
                    }

                    UpdateAssemblyInfo(libSeTemplateFileName, newVersion, updateTemplateFile);
                    UpdateAssemblyInfo(seTemplateFileName, newVersion, updateTemplateFile);
                }
                else
                {
                    Console.WriteLine(" no changes");
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

        private static void UpdateWinGet(VersionInfo version)
        {
            if (version.Build != 0)
            {
                return;
            }

            var workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            var dir = Path.Combine(workingDirectory, "winget");
            if (!Directory.Exists(dir))
            {
                dir = Path.Combine(workingDirectory, "..", "winget");
            }
            if (!Directory.Exists(dir))
            {
                dir = Path.Combine(workingDirectory, "..", "..", "winget");
            }
            if (!Directory.Exists(dir))
            {
                dir = Path.Combine(workingDirectory, "..", "..", "..", "winget");
            }
            if (!Directory.Exists(dir))
            {
                dir = Path.Combine(workingDirectory, "..", "..", "..", "..", "winget");
            }

            if (!Directory.Exists(dir))
            {
                return;
            }

            var sha256Hash = ComputeSha256HashForInstaller(version);

            var installer = Path.Combine(dir, "Nikse.SubtitleEdit.installer.yaml");
            var yaml = File.ReadAllText(installer);
            yaml = SetVariable(yaml, "PackageVersion", version.NormalVersion); //PackageVersion: 3.6.7.0
            yaml = SetVariable(yaml, "ReleaseDate", DateTime.Now.ToString("yyyy-MM-dd")); //ReleaseDate: 2022-08-13
            yaml = SetVariable(yaml, "InstallerUrl", $"https://github.com/SubtitleEdit/subtitleedit/releases/download/{version.ShortVersion}/SubtitleEdit-{version.ShortVersion}-Setup.exe"); //InstallerUrl: https://github.com/SubtitleEdit/subtitleedit/releases/download/3.6.7/SubtitleEdit-3.6.7-Setup.exe
            yaml = SetVariable(yaml, "InstallerSha256", sha256Hash); //InstallerSha256: 66F2BEFD07E2295EE606BC02A4EAACB1E0D2DEBE42B4D167AE45C5CC76F5E9A3
            File.WriteAllText(installer, yaml.TrimEnd()+ Environment.NewLine + Environment.NewLine);

            var locale = Path.Combine(dir, "Nikse.SubtitleEdit.locale.en-US.yaml");
            yaml = File.ReadAllText(locale);
            yaml = SetVariable(yaml, "PackageVersion", version.NormalVersion); // PackageVersion: 3.6.7.0
            File.WriteAllText(locale, yaml.TrimEnd() + Environment.NewLine + Environment.NewLine);

            var main = Path.Combine(dir, "Nikse.SubtitleEdit.yaml");
            yaml = File.ReadAllText(main);
            yaml = SetVariable(yaml, "PackageVersion", version.NormalVersion); // PackageVersion: 3.6.7.0
            File.WriteAllText(main, yaml.TrimEnd() + Environment.NewLine + Environment.NewLine);

            Console.WriteLine("Winget updated.");
        }

        static string ComputeSha256HashForInstaller(VersionInfo version)
        {
            var exe = $"SubtitleEdit-{version.ShortVersion}-Setup.exe";
            if (!File.Exists(exe))
            {
                exe = @"C:\git\subtitleedit\" + exe;
            }

            if (!File.Exists(exe))
            {
                return "TODO: fix sha 256 hash";
            }

            using (var sha256Hash = SHA256.Create())
            {
                var bytes = sha256Hash.ComputeHash(File.ReadAllBytes(exe));
                var builder = new StringBuilder();
                for (var i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("X2"));
                }

                return builder.ToString();
            }
        }

        private static string SetVariable(string txt, string targetVariable, string targetValue)
        {
            var sb = new StringBuilder();
            foreach (var line in txt.Split('\n'))
            {
                var s = line.Trim('\r');
                var searchStr = $"{targetVariable}:";
                if (s.TrimStart().StartsWith(searchStr.TrimStart(), StringComparison.OrdinalIgnoreCase))
                {
                    var start = line.Substring(0, line.IndexOf(searchStr, StringComparison.OrdinalIgnoreCase) + searchStr.Length);
                    sb.AppendLine($"{start} {targetValue}");
                }
                else
                {
                    sb.AppendLine(s);
                }
            }

            return sb.ToString();
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
                    {
                        return path;
                    }
                }
            }

            var gitPath = Path.Combine("Git", "bin", "git.exe");

            var envProgramFiles = Environment.GetEnvironmentVariable("ProgramFiles");
            if (!string.IsNullOrWhiteSpace(envProgramFiles))
            {
                var path = Path.Combine(envProgramFiles, gitPath);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            envProgramFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            if (!string.IsNullOrWhiteSpace(envProgramFiles))
            {
                var path = Path.Combine(envProgramFiles, gitPath);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            var envSystemDrive = Environment.GetEnvironmentVariable("SystemDrive");
            if (string.IsNullOrEmpty(envSystemDrive))
            {
                WriteWarning(@"Environment.GetEnvironmentVariable(""SystemDrive"") returned null!");
            }
            if (!string.IsNullOrWhiteSpace(envSystemDrive))
            {
                var path = Path.Combine(envSystemDrive, "Program Files", gitPath);
                if (File.Exists(path))
                {
                    return path;
                }

                path = Path.Combine(envSystemDrive, "Program Files (x86)", gitPath);
                if (File.Exists(path))
                {
                    return path;
                }

                path = Path.Combine(envSystemDrive, gitPath);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            try
            {
                var cRoot = new DriveInfo("C").RootDirectory.FullName;
                if (string.IsNullOrWhiteSpace(envSystemDrive) || !cRoot.StartsWith(envSystemDrive, StringComparison.OrdinalIgnoreCase))
                {
                    var path = Path.Combine(cRoot, "Program Files", gitPath);
                    if (File.Exists(path))
                    {
                        return path;
                    }

                    path = Path.Combine(cRoot, "Program Files (x86)", gitPath);
                    if (File.Exists(path))
                    {
                        return path;
                    }

                    path = Path.Combine(cRoot, gitPath);
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }
            catch (Exception exception)
            {
                WriteWarning(@"System.IO.DriveInfo(""C"") exception: " + exception.Message);
            }

            WriteWarning("Might not be able to run Git command line tool!");
            return "git";
        }
    }
}
