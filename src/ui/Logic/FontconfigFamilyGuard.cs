using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Keeps Skia's fontconfig font manager from hanging forever at startup on Linux.
///
/// Skia (SkFontMgr_fontconfig::GetFamilyNames, still in SkiaSharp 3.119) walks each font's family
/// names with <c>FcPatternGetString(font, FC_FAMILY, id++)</c> until fontconfig answers
/// <c>FcResultNoId</c>. For a font that has no family entry at all fontconfig answers
/// <c>FcResultNoMatch</c> instead, which Skia treats as "skip this id" - so the loop never ends.
/// The main thread then spins at 100% CPU inside SKFontManager.Default before any window exists:
/// no window, no output, no error-log.txt, for both the tarball and the Flatpak.
///
/// fontconfig's scanner falls back to the file name when a font has no family, so such fonts come
/// from a stale cache or a <c>&lt;match target="scan"&gt;</c> rule that deletes the family. This
/// guard finds them before Skia does and hides exactly those files via a generated config file
/// that includes the normal one plus <c>&lt;rejectfont&gt;</c> entries. Call <see cref="Apply"/>
/// once, before Avalonia initializes Skia.
/// </summary>
public static partial class FontconfigFamilyGuard
{
    private const string FontconfigLibrary = "libfontconfig.so.1";
    private const int FcSetSystem = 0;
    private const int FcResultMatch = 0;

    [StructLayout(LayoutKind.Sequential)]
    private struct FcFontSet
    {
        public int NFont;
        public int SFont;
        public IntPtr Fonts;
    }

    [LibraryImport(FontconfigLibrary)]
    private static partial IntPtr FcInitLoadConfigAndFonts();

    [LibraryImport(FontconfigLibrary)]
    private static partial IntPtr FcConfigGetFonts(IntPtr config, int set);

    [LibraryImport(FontconfigLibrary, StringMarshalling = StringMarshalling.Utf8)]
    private static partial int FcPatternGetString(IntPtr pattern, string obj, int n, out IntPtr value);

    [LibraryImport(FontconfigLibrary)]
    private static partial IntPtr FcConfigFilename(IntPtr name);

    [LibraryImport(FontconfigLibrary)]
    private static partial void FcStrFree(IntPtr s);

    [LibraryImport(FontconfigLibrary)]
    private static partial void FcConfigDestroy(IntPtr config);

    [LibraryImport("libc", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
    private static partial int setenv(string name, string value, int overwrite);

    public static void Apply()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        // Escape hatch in case the generated config ever misbehaves on an exotic setup.
        if (Environment.GetEnvironmentVariable("SE_DISABLE_FONTCONFIG_GUARD") == "1")
        {
            return;
        }

        try
        {
            var config = FcInitLoadConfigAndFonts();
            if (config == IntPtr.Zero)
            {
                return;
            }

            List<string> familylessFiles;
            string? baseConfigFile;
            try
            {
                familylessFiles = FindFamilylessFontFiles(config);
                if (familylessFiles.Count == 0)
                {
                    return;
                }

                baseConfigFile = GetDefaultConfigFileName();
            }
            finally
            {
                FcConfigDestroy(config);
            }

            var guardConfigFile = Path.Combine(Se.DataFolder, "fontconfig-guard.conf");
            File.WriteAllText(guardConfigFile, BuildConfig(baseConfigFile, familylessFiles), new UTF8Encoding(false));

            // Skia reads FONTCONFIG_FILE through native getenv; managed SetEnvironmentVariable
            // never reaches the native environment, so set both.
            Environment.SetEnvironmentVariable("FONTCONFIG_FILE", guardConfigFile);
            setenv("FONTCONFIG_FILE", guardConfigFile, 1);

            Se.LogError("Fonts without a family name were hidden from Subtitle Edit, as they make the font system hang at startup:" +
                        Environment.NewLine + string.Join(Environment.NewLine, familylessFiles) +
                        Environment.NewLine + "Removing these fonts, or running 'fc-cache -r', may fix them for other apps too.");
        }
        catch (Exception exception)
        {
            // No fontconfig (DllNotFoundException) or an unexpected layout - carry on as before.
            Se.LogError(exception, "Fontconfig family guard failed");
        }
    }

    private static List<string> FindFamilylessFontFiles(IntPtr config)
    {
        var result = new List<string>();
        var fontSetPtr = FcConfigGetFonts(config, FcSetSystem);
        if (fontSetPtr == IntPtr.Zero)
        {
            return result;
        }

        var fontSet = Marshal.PtrToStructure<FcFontSet>(fontSetPtr);
        for (var i = 0; i < fontSet.NFont; i++)
        {
            var pattern = Marshal.ReadIntPtr(fontSet.Fonts, i * IntPtr.Size);
            if (pattern == IntPtr.Zero || FcPatternGetString(pattern, "family", 0, out _) == FcResultMatch)
            {
                continue;
            }

            if (FcPatternGetString(pattern, "file", 0, out var filePtr) == FcResultMatch)
            {
                var file = Marshal.PtrToStringUTF8(filePtr);
                if (!string.IsNullOrEmpty(file) && !result.Contains(file))
                {
                    result.Add(file);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// The config file fontconfig would load by default (honors a user's own FONTCONFIG_FILE),
    /// or null when there is none and fontconfig runs on its built-in fallback.
    /// </summary>
    private static string? GetDefaultConfigFileName()
    {
        var namePtr = FcConfigFilename(IntPtr.Zero);
        if (namePtr == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            return Marshal.PtrToStringUTF8(namePtr);
        }
        finally
        {
            FcStrFree(namePtr);
        }
    }

    private static string BuildConfig(string? baseConfigFile, IEnumerable<string> rejectedFiles)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\"?>");
        sb.AppendLine("<!DOCTYPE fontconfig SYSTEM \"urn:fontconfig:fonts.dtd\">");
        sb.AppendLine("<!-- Generated by Subtitle Edit at startup - see FontconfigFamilyGuard. -->");
        sb.AppendLine("<fontconfig>");
        if (!string.IsNullOrEmpty(baseConfigFile))
        {
            sb.AppendLine($"  <include ignore_missing=\"yes\">{SecurityElement.Escape(baseConfigFile)}</include>");
        }
        else
        {
            // Same as fontconfig's built-in fallback config (FcInitFallbackConfig).
            sb.AppendLine("  <dir>/usr/share/fonts</dir>");
            sb.AppendLine("  <dir prefix=\"xdg\">fonts</dir>");
            sb.AppendLine("  <cachedir>/var/cache/fontconfig</cachedir>");
            sb.AppendLine("  <cachedir prefix=\"xdg\">fontconfig</cachedir>");
            sb.AppendLine("  <include ignore_missing=\"yes\">/etc/fonts/conf.d</include>");
            sb.AppendLine("  <include ignore_missing=\"yes\" prefix=\"xdg\">fontconfig/conf.d</include>");
            sb.AppendLine("  <include ignore_missing=\"yes\" prefix=\"xdg\">fontconfig/fonts.conf</include>");
        }

        sb.AppendLine("  <selectfont>");
        sb.AppendLine("    <rejectfont>");
        foreach (var file in rejectedFiles)
        {
            sb.AppendLine($"      <glob>{SecurityElement.Escape(file)}</glob>");
        }

        sb.AppendLine("    </rejectfont>");
        sb.AppendLine("  </selectfont>");
        sb.AppendLine("</fontconfig>");
        return sb.ToString();
    }
}
