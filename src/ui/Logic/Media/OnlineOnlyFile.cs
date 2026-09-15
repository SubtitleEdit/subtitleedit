using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic.Media;

/// <summary>
/// Online-only cloud files - placeholders from Dropbox, iCloud Drive, OneDrive, Google Drive - have
/// their metadata on disk but not their bytes, so the first read waits until the provider has
/// downloaded the whole file (#14912). Only metadata is looked at here, so asking never starts a
/// download.
/// </summary>
public static partial class OnlineOnlyFile
{
    // <sys/stat.h> SF_DATALESS: set by iCloud Drive and File Provider (Dropbox, OneDrive, Google
    // Drive on macOS) on files whose contents have not been downloaded.
    private const uint SfDataless = 0x40000000;

    // Windows cloud files placeholders (OneDrive, Dropbox, iCloud for Windows).
    private const FileAttributes RecallOnOpen = (FileAttributes)0x00040000;
    private const FileAttributes RecallOnDataAccess = (FileAttributes)0x00400000;

    // struct stat with 64-bit inodes: 144 bytes with st_flags at offset 116, on arm64 and x64 alike.
    private const int StatSize = 144;
    private const int StatFlagsOffset = 116;

    public static bool IsOnlineOnly(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return false;
        }

        try
        {
            if (OperatingSystem.IsMacOS())
            {
                return TryGetMacStatFlags(fileName, out var flags) && HasDatalessFlag(flags);
            }

            if (OperatingSystem.IsWindows())
            {
                return IsOnlineOnlyAttributes(File.GetAttributes(fileName));
            }
        }
        catch (Exception)
        {
            // Unknown counts as local - the file then opens just like before.
        }

        return false;
    }

    internal static bool HasDatalessFlag(uint statFlags) => (statFlags & SfDataless) != 0;

    internal static bool IsOnlineOnlyAttributes(FileAttributes attributes) =>
        (attributes & (FileAttributes.Offline | RecallOnOpen | RecallOnDataAccess)) != 0;

    internal static bool TryGetMacStatFlags(string fileName, out uint flags)
    {
        var buffer = new byte[StatSize];
        if (stat64(fileName, buffer) != 0)
        {
            flags = 0;
            return false;
        }

        flags = BitConverter.ToUInt32(buffer, StatFlagsOffset);
        return true;
    }

    // stat64, not stat: on x64 the exported "stat" is the legacy call with 32-bit inodes and another
    // struct layout - the C headers redirect stat to stat$INODE64, a P/Invoke does not. stat64 has
    // the 64-bit-inode layout on both arm64 and x64.
    [LibraryImport("libc", EntryPoint = "stat64", StringMarshalling = StringMarshalling.Utf8)]
    private static partial int stat64(string path, [Out] byte[] buffer);
}
