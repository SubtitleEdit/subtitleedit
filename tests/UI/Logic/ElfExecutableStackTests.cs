using System.Buffers.Binary;
using Nikse.SubtitleEdit.Logic;

namespace Tests.Logic;

/// <summary>
/// Covers the PT_GNU_STACK patcher that keeps Purfview's Faster-Whisper-XXL loadable on
/// glibc 2.41+, where dlopen no longer grants a shared object an executable stack.
/// </summary>
public class ElfExecutableStackTests
{
    private const uint PtLoad = 1;
    private const uint PtGnuStack = 0x6474e551;

    [Fact]
    public void ClearsTheExecuteBitOnAnElf64Library()
    {
        var elf = BuildElf64(gnuStackFlags: 7); // RWE - what libctranslate2 ships with

        Assert.True(ElfHelper.ClearExecutableStack(elf));
        Assert.Equal(6u, ReadElf64GnuStackFlags(elf)); // RW
    }

    [Fact]
    public void LeavesTheOtherProgramHeadersAlone()
    {
        var elf = BuildElf64(gnuStackFlags: 7);

        ElfHelper.ClearExecutableStack(elf);

        // The PT_LOAD segment before it still has its execute bit - only the stack was touched.
        elf.Position = 64 + 4;
        var loadFlags = new byte[4];
        elf.ReadExactly(loadFlags);
        Assert.Equal(5u, BinaryPrimitives.ReadUInt32LittleEndian(loadFlags));
    }

    [Fact]
    public void IsIdempotent()
    {
        var elf = BuildElf64(gnuStackFlags: 7);

        Assert.True(ElfHelper.ClearExecutableStack(elf));
        Assert.False(ElfHelper.ClearExecutableStack(elf));
        Assert.Equal(6u, ReadElf64GnuStackFlags(elf));
    }

    [Fact]
    public void LeavesALibraryThatNeverAskedForAnExecutableStack()
    {
        var elf = BuildElf64(gnuStackFlags: 6); // RW - the normal case

        Assert.False(ElfHelper.ClearExecutableStack(elf));
        Assert.Equal(6u, ReadElf64GnuStackFlags(elf));
    }

    [Fact]
    public void ClearsTheExecuteBitOnAnElf32Library()
    {
        var elf = BuildElf32(gnuStackFlags: 7);

        Assert.True(ElfHelper.ClearExecutableStack(elf));

        elf.Position = 52 + 24;
        var flags = new byte[4];
        elf.ReadExactly(flags);
        Assert.Equal(6u, BinaryPrimitives.ReadUInt32LittleEndian(flags));
    }

    [Fact]
    public void IgnoresABigEndianElf()
    {
        var elf = BuildElf64(gnuStackFlags: 7);
        elf.Position = 5;
        elf.WriteByte(2); // ELFDATA2MSB - the offsets below would be wrong, so do not touch it

        Assert.False(ElfHelper.ClearExecutableStack(elf));
    }

    [Fact]
    public void IgnoresFilesThatAreNotElf()
    {
        // "*.so*" also matches things like "notes.sound", and a downloaded folder can hold
        // anything, so a non-ELF must be rejected rather than patched at a guessed offset.
        var notElf = new MemoryStream(new byte[512]);
        Assert.False(ElfHelper.ClearExecutableStack(notElf));

        var text = new MemoryStream("MZ this is a windows binary"u8.ToArray());
        Assert.False(ElfHelper.ClearExecutableStack(text));
    }

    [Fact]
    public void IgnoresATruncatedElf()
    {
        var full = BuildElf64(gnuStackFlags: 7).ToArray();

        // Header intact, program headers cut off mid-way.
        var truncated = new MemoryStream(full[..(full.Length - 40)]);
        Assert.False(ElfHelper.ClearExecutableStack(truncated));

        var headerOnly = new MemoryStream(full[..20]);
        Assert.False(ElfHelper.ClearExecutableStack(headerOnly));
    }

    /// <summary>
    /// A 64-bit little-endian ELF with two program headers: a PT_LOAD (R+X) and a PT_GNU_STACK
    /// with the given flags. Only the fields the patcher reads are filled in.
    /// </summary>
    private static MemoryStream BuildElf64(uint gnuStackFlags)
    {
        const int phOffset = 64;
        const int phEntSize = 56;
        var bytes = new byte[phOffset + 2 * phEntSize];

        bytes[0] = 0x7F;
        bytes[1] = (byte)'E';
        bytes[2] = (byte)'L';
        bytes[3] = (byte)'F';
        bytes[4] = 2; // ELFCLASS64
        bytes[5] = 1; // ELFDATA2LSB
        bytes[6] = 1; // EV_CURRENT

        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(32, 8), phOffset);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(54, 2), phEntSize);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(56, 2), 2);

        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(phOffset, 4), PtLoad);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(phOffset + 4, 4), 5); // R+X

        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(phOffset + phEntSize, 4), PtGnuStack);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(phOffset + phEntSize + 4, 4), gnuStackFlags);

        return new MemoryStream(bytes, 0, bytes.Length, writable: true, publiclyVisible: true);
    }

    /// <summary>
    /// The 32-bit layout, where p_flags sits at the end of the entry instead of right after
    /// p_type.
    /// </summary>
    private static MemoryStream BuildElf32(uint gnuStackFlags)
    {
        const int phOffset = 52;
        const int phEntSize = 32;
        var bytes = new byte[phOffset + phEntSize];

        bytes[0] = 0x7F;
        bytes[1] = (byte)'E';
        bytes[2] = (byte)'L';
        bytes[3] = (byte)'F';
        bytes[4] = 1; // ELFCLASS32
        bytes[5] = 1; // ELFDATA2LSB
        bytes[6] = 1;

        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(28, 4), phOffset);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(42, 2), phEntSize);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(44, 2), 1);

        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(phOffset, 4), PtGnuStack);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(phOffset + 24, 4), gnuStackFlags);

        return new MemoryStream(bytes, 0, bytes.Length, writable: true, publiclyVisible: true);
    }

    private static uint ReadElf64GnuStackFlags(MemoryStream elf)
    {
        elf.Position = 64 + 56 + 4;
        var flags = new byte[4];
        elf.ReadExactly(flags);
        return BinaryPrimitives.ReadUInt32LittleEndian(flags);
    }
}
