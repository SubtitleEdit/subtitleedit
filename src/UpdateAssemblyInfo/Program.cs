﻿using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace UpdateAssemblyInfo
{
    internal class Program
    {
        private static readonly Regex LongGitTagRegex; // e.g.: 3.4.8-226-g7037fef
        private static readonly Regex LongVersionRegex; // e.g.: 3.4.8.226
        private static readonly Regex ShortVersionRegex; // e.g.: 3.4.8 (w/o build number)
        private static readonly Regex TemplateFileVersionRegex; // e.g.: [assembly: AssemblyVersion("3.4.8.[REVNO]")]
        private static readonly Regex AssemblyInfoFileVersionRegex; // e.g.: [assembly: AssemblyVersion("3.4.8.226")]
        private static readonly Regex AssemblyInfoFileRevisionGuidRegex; // e.g.: [assembly: AssemblyDescription("0e82e5769c9b235383991082c0a0bba96d20c69d")]
        static Program()
        {
            var options = RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;
            LongGitTagRegex = new Regex(@"\A(?<major>[0-9]+)\.(?<minor>[0-9]+)\.(?<maintenance>[0-9]+)-(?<build>[0-9]+)-g[0-9a-z]+\z", options);
            LongVersionRegex = new Regex(@"\A(?<major>[0-9]+)\.(?<minor>[0-9]+)\.(?<maintenance>[0-9]+)\.(?<build>[0-9]+)\z", options);
            ShortVersionRegex = new Regex(@"\A(?<major>[0-9]+)\.(?<minor>[0-9]+)\.(?<maintenance>[0-9]+)\z", options);
            options |= RegexOptions.Multiline;
            TemplateFileVersionRegex = new Regex(@"^[ \t]*\[assembly:[ \t]*AssemblyVersion\(""(?<version>[0-9]+\.[0-9]+\.[0-9]+)\.\[REVNO\]""\)\]", options);
            AssemblyInfoFileVersionRegex = new Regex(@"^[ \t]*\[assembly:[ \t]*AssemblyVersion\(""(?<version>[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)""\)\]", options);
            AssemblyInfoFileRevisionGuidRegex = new Regex(@"^[ \t]*\[assembly:[ \t]*AssemblyDescription\(""(?<guid>[0-9A-Za-z]*)""\)\]", options);
        }

        private const int UnknownBuild = 9999;

        private class VersionInfo : IComparable<VersionInfo>, IEquatable<VersionInfo>
        {
            public int Major { get; private set; }
            public int Minor { get; private set; }
            public int Maintenance { get; private set; }
            public int Build { get; private set; }
            public string RevisionGuid { get; private set; }

            public string FullVersion
            {
                get
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:D}.{1:D}.{2:D}.{3:D} {4}", Major, Minor, Maintenance, Build, RevisionGuid).TrimEnd();
                }
            }

            public string ShortVersion
            {
                get
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0:D}.{1:D}.{2:D}", Major, Minor, Maintenance);
                }
            }

            public VersionInfo()
            {
                RevisionGuid = string.Empty;
            }

            public VersionInfo(string version, string guid = null)
            {
                var match = LongGitTagRegex.Match(version);
                if (!match.Success)
                    match = LongVersionRegex.Match(version);
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
                    match = ShortVersionRegex.Match(version);
                if (!match.Success)
                    throw new ArgumentException("Invalid version identifier: '" + version + "'");
                Major = int.Parse(match.Groups["major"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
                Minor = int.Parse(match.Groups["minor"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
                Maintenance = int.Parse(match.Groups["maintenance"].Value, NumberStyles.None, CultureInfo.InvariantCulture);
            }

            public int CompareTo(VersionInfo vi)
            {
                int cmp = 1;
                if (!object.ReferenceEquals(vi, null))
                {
                    cmp = Major.CompareTo(vi.Major);
                    if (cmp == 0)
                        cmp = Minor.CompareTo(vi.Minor);
                    if (cmp == 0)
                        cmp = Maintenance.CompareTo(vi.Maintenance);
                }
                return cmp;
            }

            public bool Equals(VersionInfo vi)
            {
                return object.ReferenceEquals(vi, null) ? false : Major.Equals(vi.Major) && Minor.Equals(vi.Minor) && Maintenance.Equals(vi.Maintenance) && Build.Equals(vi.Build) && RevisionGuid.Equals(vi.RevisionGuid, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as VersionInfo);
            }

            public override int GetHashCode()
            {
                return FullVersion.GetHashCode();
            }

            public static bool operator == (VersionInfo vi1, VersionInfo vi2)
            {
                return object.ReferenceEquals(vi2, null) ? object.ReferenceEquals(vi1, null) : vi2.Equals(vi1);
            }

            public static bool operator != (VersionInfo vi1, VersionInfo vi2)
            {
                return object.ReferenceEquals(vi2, null) ? !object.ReferenceEquals(vi1, null) : !vi2.Equals(vi1);
            }

            public static bool operator > (VersionInfo vi1, VersionInfo vi2)
            {
                return object.ReferenceEquals(vi1, null) ? false : vi1.CompareTo(vi2) > 0;
            }

            public static bool operator < (VersionInfo vi1, VersionInfo vi2)
            {
                return object.ReferenceEquals(vi2, null) ? false : vi2.CompareTo(vi1) > 0;
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
                    throw new Exception("Malformed template file: '" + templateFileName + "'");
                }
                var index = templateMatch.Groups["version"].Index;
                var length = templateMatch.Groups["version"].Length;
                templateText = templateText.Remove(index, length).Insert(index, newVersion.ShortVersion);
                File.WriteAllText(templateFileName, templateText, Encoding.UTF8);
            }

            var assemblyInfoText = templateText.Replace("[REVNO]", newVersion.Build.ToString(CultureInfo.InvariantCulture)).Replace("[GITHASH]", newVersion.RevisionGuid);
            var assemblyInfoFileName = templateFileName.Replace(".template", string.Empty);
            File.WriteAllText(assemblyInfoFileName, assemblyInfoText, Encoding.UTF8);
        }

        private static void GetRepositoryVersions(out VersionInfo currentRepositoryVersion, out VersionInfo latestRepositoryVersion)
        {
            var workingDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            var clrHash = new CommandLineRunner();
            var clrTags = new CommandLineRunner();
            var gitPath = GetGitPath();
            if (clrHash.RunCommandAndGetOutput(gitPath, "rev-parse --verify HEAD", workingDirectory) && clrTags.RunCommandAndGetOutput(gitPath, "describe --long --tags", workingDirectory))
            {
                if (!LongGitTagRegex.IsMatch(clrTags.Result))
                {
                    throw new Exception("Invalid Git version tag: '" + clrTags.Result + "' (major.minor.maintenance-build expected)");
                }
                currentRepositoryVersion = new VersionInfo(clrTags.Result, clrHash.Result);
            }
            else
            {
                currentRepositoryVersion = new VersionInfo(); // no git repository
            }
            if (clrHash.RunCommandAndGetOutput(gitPath, "rev-parse --verify refs/heads/master", workingDirectory) && clrTags.RunCommandAndGetOutput(gitPath, "describe --long --tags refs/heads/master", workingDirectory))
            {
                if (!LongGitTagRegex.IsMatch(clrTags.Result))
                {
                    throw new Exception("Invalid Git version tag: '" + clrTags.Result + "' (major.minor.maintenance-build expected)");
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
            }
            return new VersionInfo();
        }

        private static VersionInfo GetTemplateVersion(string templateFileName)
        {
            var templateText = File.ReadAllText(templateFileName);
            var versionMatch = TemplateFileVersionRegex.Match(templateText);
            if (!versionMatch.Success)
            {
                throw new Exception("Malformed template file: '" + templateFileName + "'");
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

        private static int Main(string[] args)
        {
            var myName = Environment.GetCommandLineArgs()[0];
            myName = Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(myName) ? System.Reflection.Assembly.GetEntryAssembly().Location : myName);
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
                VersionInfo currentRepositoryVersion;
                VersionInfo latestRepositoryVersion;
                VersionInfo currentVersion;
                VersionInfo newVersion;

                GetRepositoryVersions(out currentRepositoryVersion, out latestRepositoryVersion);
                currentVersion = GetCurrentVersion(seTemplateFileName);
                var updateTemplateFile = false;
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
                WriteWarning(@"Environment.GetEnvironmentVariable(""SystemDrive"") returned null!");
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
                if (string.IsNullOrWhiteSpace(envSystemDrive) || !cRoot.StartsWith(envSystemDrive, StringComparison.OrdinalIgnoreCase))
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
                WriteWarning(@"System.IO.DriveInfo(""C"") exception: " + exception.Message);
            }

            WriteWarning("Might not be able to run Git command line tool!");
            return "git";
        }

    }
}