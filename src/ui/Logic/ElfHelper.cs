using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Buffers.Binary;
using System.IO;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Minimal ELF program-header patching for downloaded Linux binaries.
/// <para>
/// glibc 2.41 stopped making the stack executable when a shared object asks for it
/// (<c>PT_GNU_STACK</c> with the execute bit set): <c>dlopen</c> now fails outright with
/// "cannot enable executable stack as shared object requires: Invalid argument". Some engines SE
/// downloads still carry that flag, so on a distro with glibc 2.41+ (Fedora 42, Arch,
/// Ubuntu 25.10) they die the moment the library is loaded. Clearing the bit is enough - the
/// libraries do not actually run code off the stack.
/// </para>
/// </summary>
public static class ElfHelper
{
    // Program header types/flags from elf.h.
    private const uint PtGnuStack = 0x6474e551;
    private const uint PfX = 0x1;

    private const int ElfClass32 = 1;
    private const int ElfClass64 = 2;
    private const int ElfDataLittleEndian = 1;

    // Offsets of p_flags inside a program header entry - it moves between the two ELF classes.
    private const int PFlagsOffset32 = 24;
    private const int PFlagsOffset64 = 4;

    private const int MinPhEntSize32 = 32;
    private const int MinPhEntSize64 = 56;

    // A sane upper bound on e_phnum, so a corrupt/truncated file cannot spin the loop.
    private const int MaxProgramHeaders = 4096;

    /// <summary>
    /// Clears the executable-stack flag on every shared library below <paramref name="folder"/>
    /// and returns how many files were changed. Anything that is not a little-endian ELF, or has
    /// no executable-stack request, is left untouched; unreadable files are skipped.
    /// </summary>
    public static int ClearExecutableStackInFolder(string folder)
    {
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder))
        {
            return 0;
        }

        var patched = 0;
        try
        {
            // "*.so*" also catches versioned names like "libctranslate2-d3638643.so.4.4.0".
            // Anything that slips through the pattern but is not an ELF is rejected by the
            // header check in ClearExecutableStack.
            foreach (var fileName in Directory.EnumerateFiles(folder, "*.so*", SearchOption.AllDirectories))
            {
                try
                {
                    if (ClearExecutableStack(fileName))
                    {
                        patched++;
                    }
                }
                catch (Exception ex)
                {
                    Se.LogError(ex, $"ElfHelper: could not clear the executable-stack flag on '{fileName}'");
                }
            }
        }
        catch (Exception ex)
        {
            Se.LogError(ex, $"ElfHelper: could not scan '{folder}' for shared libraries");
        }

        return patched;
    }

    /// <summary>
    /// Clears <c>PF_X</c> on the <c>PT_GNU_STACK</c> program header of an ELF file. Returns true
    /// only when the file was actually rewritten, i.e. it is a little-endian ELF that did request
    /// an executable stack.
    /// </summary>
    public static bool ClearExecutableStack(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        return ClearExecutableStack(stream);
    }

    /// <summary>
    /// Stream overload - exposed for testing with a synthetic ELF.
    /// </summary>
    internal static bool ClearExecutableStack(Stream stream)
    {
        Span<byte> header = stackalloc byte[64];
        stream.Position = 0;
        if (!ReadExactly(stream, header))
        {
            return false;
        }

        if (header[0] != 0x7F || header[1] != (byte)'E' || header[2] != (byte)'L' || header[3] != (byte)'F')
        {
            return false;
        }

        var elfClass = header[4];
        // Only little-endian ELF is handled: every platform SE ships Linux binaries for is
        // little-endian, and guessing at a big-endian layout would risk corrupting the file.
        if (header[5] != ElfDataLittleEndian || (elfClass != ElfClass32 && elfClass != ElfClass64))
        {
            return false;
        }

        var is64 = elfClass == ElfClass64;
        var phOffset = is64
            ? (long)BinaryPrimitives.ReadUInt64LittleEndian(header.Slice(32, 8))
            : BinaryPrimitives.ReadUInt32LittleEndian(header.Slice(28, 4));
        var phEntSize = is64
            ? BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(54, 2))
            : BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(42, 2));
        var phCount = is64
            ? BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(56, 2))
            : BinaryPrimitives.ReadUInt16LittleEndian(header.Slice(44, 2));

        var minEntSize = is64 ? MinPhEntSize64 : MinPhEntSize32;
        if (phOffset <= 0 || phCount == 0 || phCount > MaxProgramHeaders || phEntSize < minEntSize)
        {
            return false;
        }

        var pFlagsOffset = is64 ? PFlagsOffset64 : PFlagsOffset32;
        Span<byte> entry = stackalloc byte[8];
        for (var i = 0; i < phCount; i++)
        {
            var entryOffset = phOffset + (long)i * phEntSize;
            if (entryOffset + minEntSize > stream.Length)
            {
                return false;
            }

            stream.Position = entryOffset;
            if (!ReadExactly(stream, entry.Slice(0, 4)))
            {
                return false;
            }

            if (BinaryPrimitives.ReadUInt32LittleEndian(entry.Slice(0, 4)) != PtGnuStack)
            {
                continue;
            }

            stream.Position = entryOffset + pFlagsOffset;
            if (!ReadExactly(stream, entry.Slice(0, 4)))
            {
                return false;
            }

            var flags = BinaryPrimitives.ReadUInt32LittleEndian(entry.Slice(0, 4));
            if ((flags & PfX) == 0)
            {
                return false;
            }

            BinaryPrimitives.WriteUInt32LittleEndian(entry.Slice(0, 4), flags & ~PfX);
            stream.Position = entryOffset + pFlagsOffset;
            stream.Write(entry.Slice(0, 4));
            stream.Flush();
            return true;
        }

        return false;
    }

    private static bool ReadExactly(Stream stream, Span<byte> buffer)
    {
        var read = 0;
        while (read < buffer.Length)
        {
            var count = stream.Read(buffer.Slice(read));
            if (count <= 0)
            {
                return false;
            }

            read += count;
        }

        return true;
    }
}
