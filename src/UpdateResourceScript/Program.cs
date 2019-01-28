using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace UpdateResourceScript
{
    internal static class ExtensionMethods
    {
        public static string Escape(this string s)
        {
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public static StringBuilder Append(this StringBuilder sb, string name, string value)
        {
            return sb.AppendFormat("\nVALUE \"{0}\", \"{1}\"", name, value.Escape());
        }
    }

    internal class Program
    {
        private class VersionInfoResource
        {
            public string StringFileInfo { get; }
            public string ProductVersion { get; }
            public string FileVersion { get; }
            public string FileFlags { get; }

            public VersionInfoResource(string assemblyFileName)
            {
                var fileInfo = FileVersionInfo.GetVersionInfo(assemblyFileName);
                if (fileInfo.OriginalFilename == null)
                {
                    throw new Exception("File '" + assemblyFileName + "' is not an assembly file");
                }

                // Fixed-info properties
                FileVersion = string.Format(CultureInfo.InvariantCulture, "{0:D}, {1:D}, {2:D}, {3:D}", fileInfo.FileMajorPart, fileInfo.FileMinorPart, fileInfo.FileBuildPart, fileInfo.FilePrivatePart);
                ProductVersion = string.Format(CultureInfo.InvariantCulture, "{0:D}, {1:D}, {2:D}, {3:D}", fileInfo.ProductMajorPart, fileInfo.ProductMinorPart, fileInfo.ProductBuildPart, fileInfo.ProductPrivatePart);

                var flags = new HashSet<string>();
                var block = new StringBuilder();
                // Required string-information-block values
                block.Append("VALUE \"FileDescription\", TargetAssemblyDescription"); // TargetAssemblyDescription is a resource-script macro
                block.Append("\nVALUE \"OriginalFilename\", TargetAssemblyFileName"); // TargetAssemblyFileName is a resource-script macro
                block.Append("FileVersion", fileInfo.FileVersion);
                block.Append("ProductVersion", fileInfo.ProductVersion);
                block.Append("ProductName", fileInfo.ProductName);
                block.Append("CompanyName", fileInfo.CompanyName);
                // Optional string-information-block values
                block.Append("\nVALUE \"InternalName\", TargetAssemblyFileName"); // Consistent with VS behaviour (differs from MSDN)
                if (fileInfo.LegalTrademarks != null)
                {
                    block.Append("LegalTrademarks", fileInfo.LegalTrademarks);
                }

                if (fileInfo.LegalCopyright != null)
                {
                    block.Append("LegalCopyright", fileInfo.LegalCopyright);
                }

                if (fileInfo.Comments != null)
                {
                    block.Append("Comments", fileInfo.Comments);
                }
                // Flagged string-information-block values
                if (fileInfo.IsSpecialBuild && fileInfo.SpecialBuild != null)
                {
                    block.Append("SpecialBuild", fileInfo.SpecialBuild);
                    flags.Add("VS_FF_SPECIALBUILD");
                }
                if (fileInfo.IsPrivateBuild && fileInfo.PrivateBuild != null)
                {
                    block.Append("PrivateBuild", fileInfo.PrivateBuild);
                    flags.Add("VS_FF_PRIVATEBUILD");
                }
                StringFileInfo = block.Replace("\n", Environment.NewLine + new String(' ', 3 * 4 /* Indentation */)).ToString();

                if (fileInfo.IsPreRelease)
                {
                    flags.Add("VS_FF_PRERELEASE");
                }

                FileFlags = (flags.Count > 0) ? "(" + string.Join("|", flags) + ")" : "0L";
            }
        }

        private const string WorkInProgress = "Updating win32 resource script...";

        private static int Main(string[] args)
        {
            var myName = Environment.GetCommandLineArgs()[0];
            myName = Path.GetFileNameWithoutExtension(string.IsNullOrWhiteSpace(myName) ? System.Reflection.Assembly.GetEntryAssembly().Location : myName);
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: " + myName + " <resource-script-template> <assembly-file>");
                return 1;
            }

            Console.Write(WorkInProgress);

            try
            {
                var resourceScriptTemplateFileName = Environment.GetCommandLineArgs()[1];
                var resourceScriptFileName = resourceScriptTemplateFileName.Replace(".template", string.Empty);
                var assemblyFileName = Environment.GetCommandLineArgs()[2];

                var info = new VersionInfoResource(assemblyFileName);
                var text = File.ReadAllText(resourceScriptTemplateFileName).
                            Replace("[vi:FileFlags]", info.FileFlags).
                            Replace("[vi:FileVersion]", info.FileVersion).
                            Replace("[vi:ProductVersion]", info.ProductVersion).
                            Replace("[vi:StringFileInfo]", info.StringFileInfo);
                File.WriteAllText(resourceScriptFileName, text, Encoding.Unicode);
                Console.WriteLine(" done");
                return 0;
            }
            catch (Exception exception)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR: " + myName + ": " + exception.Message + Environment.NewLine + exception.StackTrace);
                return 2;
            }
        }

    }
}
