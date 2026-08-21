using HarfBuzzSharp;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Nikse.SubtitleEdit.Logic;

/// <summary>
/// Trims a TrueType font to the glyphs a subtitle actually uses, so embedded [Fonts]
/// attachments stay small (a full CJK font is easily 5-10 MB; a subtitle uses a few
/// hundred glyphs). The font is NOT re-indexed: every table except glyf/loca is kept
/// byte-identical and unused glyphs merely get their outlines emptied, so glyph IDs,
/// cmap, GSUB/GPOS shaping, kerning and the family name all keep working unchanged in
/// libass and VSFilter. The used-glyph set is computed by shaping the actual subtitle
/// text with HarfBuzz - the same shaping path libass uses - so ligatures and contextual
/// forms (Arabic, Indic) are included exactly.
/// </summary>
public static class FontTrimmer
{
    public enum TrimSkipReason
    {
        None,

        /// <summary>CFF/PostScript outlines ('OTTO') - only glyf-flavored TrueType is supported.</summary>
        NotTrueType,

        /// <summary>TrueType collections ('ttcf') are not supported.</summary>
        FontCollection,

        /// <summary>The font could not be parsed (or required tables are missing).</summary>
        CouldNotParse,

        /// <summary>Trimming would not make the font smaller (e.g. already trimmed).</summary>
        NoSavings,
    }

    public sealed class TrimResult
    {
        /// <summary>The trimmed font, or the original bytes when trimming was skipped.</summary>
        public byte[] Bytes { get; }
        public bool Trimmed { get; }
        public TrimSkipReason SkipReason { get; }
        public int TotalGlyphs { get; }
        public int KeptGlyphs { get; }

        internal TrimResult(byte[] bytes, bool trimmed, TrimSkipReason skipReason, int totalGlyphs, int keptGlyphs)
        {
            Bytes = bytes;
            Trimmed = trimmed;
            SkipReason = skipReason;
            TotalGlyphs = totalGlyphs;
            KeptGlyphs = keptGlyphs;
        }
    }

    /// <summary>User-facing text for a skip reason (used by the attachments window and the font collector).</summary>
    public static string GetSkipReasonDisplay(TrimSkipReason reason) => reason switch
    {
        TrimSkipReason.NotTrueType => Se.Language.Assa.TrimFontsReasonNotTrueType,
        TrimSkipReason.FontCollection => Se.Language.Assa.TrimFontsReasonFontCollection,
        TrimSkipReason.NoSavings => Se.Language.Assa.TrimFontsReasonNoSavings,
        _ => Se.Language.Assa.TrimFontsReasonCouldNotParse,
    };

    private const uint TagHead = 0x68656164; // 'head'
    private const uint TagMaxp = 0x6D617870; // 'maxp'
    private const uint TagLoca = 0x6C6F6361; // 'loca'
    private const uint TagGlyf = 0x676C7966; // 'glyf'
    private const uint TagDsig = 0x44534947; // 'DSIG' - a digital signature is invalid after any change

    /// <summary>
    /// Trims <paramref name="fontBytes"/> to the glyphs needed to render
    /// <paramref name="usedLines"/> (visible text only - override tags already removed).
    /// Never throws: on any problem the original bytes are returned with a skip reason.
    /// </summary>
    public static TrimResult Trim(byte[] fontBytes, IReadOnlyCollection<string> usedLines)
    {
        try
        {
            return TrimInner(fontBytes, usedLines);
        }
        catch (Exception exception)
        {
            Se.LogError(exception, "FontTrimmer could not trim font");
            return new TrimResult(fontBytes, false, TrimSkipReason.CouldNotParse, 0, 0);
        }
    }

    private static TrimResult TrimInner(byte[] font, IReadOnlyCollection<string> usedLines)
    {
        TrimResult Skip(TrimSkipReason reason) => new(font, false, reason, 0, 0);

        if (font.Length < 12)
        {
            return Skip(TrimSkipReason.CouldNotParse);
        }

        var version = ReadU32(font, 0);
        if (version == 0x4F54544F) // 'OTTO'
        {
            return Skip(TrimSkipReason.NotTrueType);
        }

        if (version == 0x74746366) // 'ttcf'
        {
            return Skip(TrimSkipReason.FontCollection);
        }

        if (version != 0x00010000 && version != 0x74727565) // 1.0 / 'true'
        {
            return Skip(TrimSkipReason.CouldNotParse);
        }

        // sfnt table directory
        int numTables = ReadU16(font, 4);
        if (numTables == 0 || 12 + numTables * 16 > font.Length)
        {
            return Skip(TrimSkipReason.CouldNotParse);
        }

        var tables = new List<(uint Tag, int Offset, int Length)>(numTables);
        for (var i = 0; i < numTables; i++)
        {
            var p = 12 + i * 16;
            var tag = ReadU32(font, p);
            var offset = (int)ReadU32(font, p + 8);
            var length = (int)ReadU32(font, p + 12);
            if (offset < 0 || length < 0 || offset > font.Length || font.Length - offset < length)
            {
                return Skip(TrimSkipReason.CouldNotParse);
            }

            tables.Add((tag, offset, length));
        }

        var head = tables.FindIndex(t => t.Tag == TagHead);
        var maxp = tables.FindIndex(t => t.Tag == TagMaxp);
        var loca = tables.FindIndex(t => t.Tag == TagLoca);
        var glyf = tables.FindIndex(t => t.Tag == TagGlyf);
        if (head < 0 || maxp < 0 || loca < 0 || glyf < 0 ||
            tables[head].Length < 54 || tables[maxp].Length < 6)
        {
            return Skip(TrimSkipReason.CouldNotParse);
        }

        int indexToLocFormat = ReadU16(font, tables[head].Offset + 50);
        int numGlyphs = ReadU16(font, tables[maxp].Offset + 4);
        if (indexToLocFormat > 1 || numGlyphs == 0)
        {
            return Skip(TrimSkipReason.CouldNotParse);
        }

        var glyphOffsets = ReadLoca(font, tables[loca], indexToLocFormat, numGlyphs);
        if (glyphOffsets == null || glyphOffsets[numGlyphs] > (uint)tables[glyf].Length)
        {
            return Skip(TrimSkipReason.CouldNotParse);
        }

        var used = GetUsedGlyphs(font, usedLines);
        AddCompositeComponents(font, tables[glyf].Offset, glyphOffsets, numGlyphs, used);

        var keptCount = 0;
        foreach (var g in used)
        {
            if (g < (uint)numGlyphs)
            {
                keptCount++;
            }
        }

        if (keptCount >= numGlyphs)
        {
            return new TrimResult(font, false, TrimSkipReason.NoSavings, numGlyphs, numGlyphs);
        }

        // New glyf: kept glyphs copied verbatim, unused glyphs become empty (loca[i] == loca[i+1]).
        // Entries stay 2-byte aligned, which also keeps a short loca valid (offsets are stored / 2).
        var newGlyf = new MemoryStream();
        var newOffsets = new uint[numGlyphs + 1];
        for (var g = 0; g < numGlyphs; g++)
        {
            newOffsets[g] = (uint)newGlyf.Length;
            if (used.Contains((uint)g))
            {
                var start = tables[glyf].Offset + (int)glyphOffsets[g];
                var length = (int)(glyphOffsets[g + 1] - glyphOffsets[g]);
                newGlyf.Write(font, start, length);
                if (length % 2 == 1)
                {
                    newGlyf.WriteByte(0);
                }
            }
        }

        newOffsets[numGlyphs] = (uint)newGlyf.Length;

        var newLoca = WriteLoca(newOffsets, indexToLocFormat);
        var trimmed = RebuildFont(font, tables, head, loca, glyf, newLoca, newGlyf.ToArray());
        if (trimmed.Length >= font.Length)
        {
            return new TrimResult(font, false, TrimSkipReason.NoSavings, numGlyphs, keptCount);
        }

        return new TrimResult(trimmed, true, TrimSkipReason.None, numGlyphs, keptCount);
    }

    /// <summary>
    /// The glyph IDs needed to render the given text lines with this font: glyph 0
    /// (.notdef), the shaped output of every line (ligatures and contextual forms shape to
    /// the same glyph IDs libass gets from HarfBuzz), and each character's nominal cmap
    /// glyph as a safety net for runs a renderer might shape differently.
    /// </summary>
    private static HashSet<uint> GetUsedGlyphs(byte[] fontBytes, IReadOnlyCollection<string> usedLines)
    {
        var used = new HashSet<uint> { 0 };

        var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
        using var blob = new Blob(handle.AddrOfPinnedObject(), fontBytes.Length, MemoryMode.ReadOnly, () => handle.Free());
        using var face = new Face(blob, 0);
        using var font = new HarfBuzzSharp.Font(face);

        var seenLines = new HashSet<string>(StringComparer.Ordinal);
        var seenCodepoints = new HashSet<int>();
        foreach (var line in usedLines)
        {
            if (string.IsNullOrEmpty(line) || !seenLines.Add(line))
            {
                continue;
            }

            // Shape script by script so a mixed line (e.g. Latin + Arabic) gives every
            // script its own shaper - one buffer would shape it all as the first script.
            // Each run is shaped horizontally and vertically: a font used by a vertical
            // "@" style gets its glyphs substituted via the 'vert' feature (rotated
            // brackets, vertical kana), and those alternates must survive the trim too.
            foreach (var run in SplitToScriptRuns(line))
            {
                AddShapedGlyphs(font, run, used, vertical: false);
                AddShapedGlyphs(font, run, used, vertical: true);
            }

            for (var i = 0; i < line.Length; i += char.IsSurrogatePair(line, i) ? 2 : 1)
            {
                // A lone surrogate (possible in text decoded from a corrupt file) has no
                // code point - skip it instead of letting ConvertToUtf32 throw, which
                // would abort trimming for every font.
                if (char.IsSurrogate(line[i]) && !char.IsSurrogatePair(line, i))
                {
                    continue;
                }

                var codepoint = char.ConvertToUtf32(line, i);
                if (seenCodepoints.Add(codepoint) && font.TryGetGlyph(codepoint, out var glyph))
                {
                    used.Add(glyph);
                }
            }
        }

        return used;
    }

    private static void AddShapedGlyphs(HarfBuzzSharp.Font font, string run, HashSet<uint> used, bool vertical)
    {
        using var buffer = new HarfBuzzSharp.Buffer();
        buffer.AddUtf16(run);
        buffer.GuessSegmentProperties();
        if (vertical)
        {
            buffer.Direction = Direction.TopToBottom;
        }

        font.Shape(buffer);
        foreach (var info in buffer.GlyphInfos)
        {
            used.Add(info.Codepoint);
        }
    }

    /// <summary>
    /// Splits a line into runs of one script each. Combining marks, joiners (ZWJ/ZWNJ)
    /// and other neutral characters stay with the run they follow, so Arabic joining and
    /// Indic conjunct formation still see them.
    /// </summary>
    internal static IEnumerable<string> SplitToScriptRuns(string text)
    {
        var runStart = 0;
        var runBucket = -1;
        for (var i = 0; i < text.Length;)
        {
            var next = i + (char.IsSurrogatePair(text, i) ? 2 : 1);
            // A lone surrogate has no code point - treat it as neutral so it stays with
            // the current run (HarfBuzz replaces it while shaping).
            var bucket = char.IsSurrogate(text[i]) && !char.IsSurrogatePair(text, i)
                ? -1
                : GetScriptBucket(char.ConvertToUtf32(text, i));
            if (bucket >= 0)
            {
                if (runBucket < 0)
                {
                    runBucket = bucket;
                }
                else if (bucket != runBucket)
                {
                    yield return text.Substring(runStart, i - runStart);
                    runStart = i;
                    runBucket = bucket;
                }
            }

            i = next;
        }

        if (runStart < text.Length)
        {
            yield return text.Substring(runStart);
        }
    }

    /// <summary>
    /// A coarse per-script bucket for the scripts where shaping substitutes glyphs
    /// (contextual forms, conjuncts), or -1 for characters that should stay with the
    /// current run (neutrals, combining marks, joiners).
    /// </summary>
    private static int GetScriptBucket(int cp)
    {
        if (cp == 0x200C || cp == 0x200D) // ZWNJ / ZWJ - shaping controls, keep in run
        {
            return -1;
        }

        if (cp >= 0x0300 && cp <= 0x036F) // combining marks, keep in run
        {
            return -1;
        }

        if (cp < 0x0370)
        {
            // Latin letters start/continue a Latin run; digits, punctuation and spaces
            // are neutral and stay with whatever run they follow.
            return char.IsLetter((char)cp) ? 0 : -1;
        }

        if (cp >= 0x0590 && cp <= 0x05FF || cp >= 0xFB1D && cp <= 0xFB4F)
        {
            return 2; // Hebrew
        }

        if (cp >= 0x0600 && cp <= 0x06FF || cp >= 0x0750 && cp <= 0x077F ||
            cp >= 0x08A0 && cp <= 0x08FF || cp >= 0xFB50 && cp <= 0xFDFF ||
            cp >= 0xFE70 && cp <= 0xFEFF)
        {
            return 3; // Arabic
        }

        if (cp >= 0x0900 && cp <= 0x0DFF)
        {
            return 4 + ((cp - 0x0900) >> 7); // Indic blocks: Devanagari..Sinhala, one bucket each
        }

        if (cp >= 0x0E00 && cp <= 0x0EFF)
        {
            return 20 + ((cp - 0x0E00) >> 7); // Thai, Lao
        }

        if (cp >= 0x1000 && cp <= 0x109F)
        {
            return 22; // Myanmar
        }

        if (cp >= 0x1780 && cp <= 0x17FF)
        {
            return 23; // Khmer
        }

        return 1; // everything else (Greek, Cyrillic, CJK, ...) - no cross-script runs
    }

    /// <summary>Adds the component glyphs of kept composite glyphs (worklist, nested composites included).</summary>
    private static void AddCompositeComponents(byte[] font, int glyfOffset, uint[] glyphOffsets, int numGlyphs, HashSet<uint> used)
    {
        var queue = new Queue<uint>(used);
        while (queue.Count > 0)
        {
            var g = queue.Dequeue();
            if (g >= (uint)numGlyphs || glyphOffsets[g + 1] - glyphOffsets[g] < 10)
            {
                continue;
            }

            var start = glyfOffset + (int)glyphOffsets[g];
            var end = glyfOffset + (int)glyphOffsets[g + 1];
            var numberOfContours = (short)ReadU16(font, start);
            if (numberOfContours >= 0)
            {
                continue; // simple glyph
            }

            var pos = start + 10;
            while (pos + 4 <= end)
            {
                var flags = ReadU16(font, pos);
                var componentIndex = ReadU16(font, pos + 2);
                if (used.Add(componentIndex))
                {
                    queue.Enqueue(componentIndex);
                }

                pos += 4;
                pos += (flags & 0x0001) != 0 ? 4 : 2; // ARG_1_AND_2_ARE_WORDS
                if ((flags & 0x0008) != 0) // WE_HAVE_A_SCALE
                {
                    pos += 2;
                }
                else if ((flags & 0x0040) != 0) // WE_HAVE_AN_X_AND_Y_SCALE
                {
                    pos += 4;
                }
                else if ((flags & 0x0080) != 0) // WE_HAVE_A_TWO_BY_TWO
                {
                    pos += 8;
                }

                if ((flags & 0x0020) == 0) // MORE_COMPONENTS
                {
                    break;
                }
            }
        }
    }

    private static uint[]? ReadLoca(byte[] font, (uint Tag, int Offset, int Length) loca, int indexToLocFormat, int numGlyphs)
    {
        var entrySize = indexToLocFormat == 0 ? 2 : 4;
        if (loca.Length < (numGlyphs + 1) * entrySize)
        {
            return null;
        }

        var offsets = new uint[numGlyphs + 1];
        for (var i = 0; i <= numGlyphs; i++)
        {
            offsets[i] = indexToLocFormat == 0
                ? (uint)ReadU16(font, loca.Offset + i * 2) * 2
                : ReadU32(font, loca.Offset + i * 4);
            if (i > 0 && offsets[i] < offsets[i - 1])
            {
                return null;
            }
        }

        return offsets;
    }

    private static byte[] WriteLoca(uint[] offsets, int indexToLocFormat)
    {
        var result = new byte[offsets.Length * (indexToLocFormat == 0 ? 2 : 4)];
        for (var i = 0; i < offsets.Length; i++)
        {
            if (indexToLocFormat == 0)
            {
                WriteU16(result, i * 2, (ushort)(offsets[i] / 2));
            }
            else
            {
                WriteU32(result, i * 4, offsets[i]);
            }
        }

        return result;
    }

    /// <summary>
    /// Rebuilds the sfnt file with the new glyf/loca tables. All other tables are copied
    /// verbatim in their original order; only offsets, lengths, checksums and the whole-file
    /// checksum adjustment change. A DSIG table is dropped - the signature is invalid anyway
    /// once the font is modified.
    /// </summary>
    private static byte[] RebuildFont(
        byte[] font,
        List<(uint Tag, int Offset, int Length)> tables,
        int headIndex, int locaIndex, int glyfIndex,
        byte[] newLoca, byte[] newGlyf)
    {
        var kept = new List<(uint Tag, byte[] Data, int OriginalOffset)>();
        for (var i = 0; i < tables.Count; i++)
        {
            var (tag, offset, length) = tables[i];
            if (tag == TagDsig)
            {
                continue;
            }

            byte[] data;
            if (i == locaIndex)
            {
                data = newLoca;
            }
            else if (i == glyfIndex)
            {
                data = newGlyf;
            }
            else
            {
                data = new byte[length];
                Array.Copy(font, offset, data, 0, length);
            }

            if (tag == TagHead)
            {
                WriteU32(data, 8, 0); // checkSumAdjustment is computed over the whole file below
            }

            kept.Add((tag, data, offset));
        }

        // Table data keeps the original file order; the directory must be sorted by tag.
        var dataOrder = new List<int>();
        for (var i = 0; i < kept.Count; i++)
        {
            dataOrder.Add(i);
        }

        dataOrder.Sort((a, b) => kept[a].OriginalOffset.CompareTo(kept[b].OriginalOffset));

        var directoryOrder = new List<int>(dataOrder);
        directoryOrder.Sort((a, b) => kept[a].Tag.CompareTo(kept[b].Tag));

        var numTables = kept.Count;
        var headerSize = 12 + numTables * 16;
        var newOffsets = new int[numTables];
        var totalSize = headerSize;
        foreach (var i in dataOrder)
        {
            newOffsets[i] = totalSize;
            totalSize += (kept[i].Data.Length + 3) & ~3; // tables are 4-byte aligned, zero padded
        }

        var result = new byte[totalSize];
        WriteU32(result, 0, ReadU32(font, 0)); // sfnt version
        WriteU16(result, 4, (ushort)numTables);
        var entrySelector = 0;
        while (1 << (entrySelector + 1) <= numTables)
        {
            entrySelector++;
        }

        var searchRange = (ushort)((1 << entrySelector) * 16);
        WriteU16(result, 6, searchRange);
        WriteU16(result, 8, (ushort)entrySelector);
        WriteU16(result, 10, (ushort)(numTables * 16 - searchRange));

        var record = 12;
        foreach (var i in directoryOrder)
        {
            WriteU32(result, record, kept[i].Tag);
            WriteU32(result, record + 4, TableChecksum(kept[i].Data));
            WriteU32(result, record + 8, (uint)newOffsets[i]);
            WriteU32(result, record + 12, (uint)kept[i].Data.Length);
            record += 16;
        }

        for (var i = 0; i < numTables; i++)
        {
            Array.Copy(kept[i].Data, 0, result, newOffsets[i], kept[i].Data.Length);
        }

        // Whole-file checksum -> head.checkSumAdjustment (currently zeroed in the head data).
        var headOffset = 0;
        for (var i = 0; i < numTables; i++)
        {
            if (kept[i].Tag == TagHead)
            {
                headOffset = newOffsets[i];
            }
        }

        WriteU32(result, headOffset + 8, 0xB1B0AFBA - TableChecksum(result));
        return result;
    }

    private static uint TableChecksum(byte[] data)
    {
        uint sum = 0;
        var whole = data.Length & ~3;
        for (var i = 0; i < whole; i += 4)
        {
            sum += ReadU32(data, i);
        }

        if (whole < data.Length)
        {
            uint last = 0;
            for (var i = whole; i < data.Length; i++)
            {
                last = last << 8 | data[i];
            }

            sum += last << (8 * (4 - (data.Length - whole)));
        }

        return sum;
    }

    private static uint ReadU32(byte[] d, int p) => (uint)(d[p] << 24 | d[p + 1] << 16 | d[p + 2] << 8 | d[p + 3]);

    private static ushort ReadU16(byte[] d, int p) => (ushort)(d[p] << 8 | d[p + 1]);

    private static void WriteU32(byte[] d, int p, uint v)
    {
        d[p] = (byte)(v >> 24);
        d[p + 1] = (byte)(v >> 16);
        d[p + 2] = (byte)(v >> 8);
        d[p + 3] = (byte)v;
    }

    private static void WriteU16(byte[] d, int p, ushort v)
    {
        d[p] = (byte)(v >> 8);
        d[p + 1] = (byte)v;
    }
}
