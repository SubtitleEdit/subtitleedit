using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{
    /// <summary>
    /// ARIB STD-B24 caption text decoder (Japanese ISDB broadcast captions, also used by ARIB B-36 files).
    /// Implements the 8-bit character coding from ARIB STD-B24 part 1 chapter 7: G0-G3 code set
    /// designation/invocation, C0/C1 control sets and the character set conversion tables.
    /// See https://www.arib.or.jp/english/html/overview/doc/6-STD-B24v5_2-1p3-E1.pdf
    /// and the reference implementation https://github.com/xqq/libaribcaption
    /// </summary>
    public class AribB24Decoder
    {
        public enum AribProfile
        {
            /// <summary>Full-seg broadcast captions (data_component_id 0x0008)</summary>
            ProfileA,

            /// <summary>One-seg (mobile) captions (data_component_id 0x0012)</summary>
            ProfileC,

            /// <summary>ABNT NBR 15606-1 Latin captions (Brazil/Philippines ISDB)</summary>
            Latin,
        }

        private enum GraphicSetKind
        {
            Kanji,
            Alphanumeric,
            LatinExtension,
            LatinSpecial,
            Hiragana,
            Katakana,
            Mosaic,
            JisX0201Katakana,
            Drcs,
            Macro,
        }

        private readonly struct CodeSet
        {
            public readonly GraphicSetKind Kind;
            public readonly int Bytes;

            public CodeSet(GraphicSetKind kind, int bytes)
            {
                Kind = kind;
                Bytes = bytes;
            }
        }

        private static readonly CodeSet KanjiSet = new CodeSet(GraphicSetKind.Kanji, 2);
        private static readonly CodeSet AlphanumericSet = new CodeSet(GraphicSetKind.Alphanumeric, 1);
        private static readonly CodeSet LatinExtensionSet = new CodeSet(GraphicSetKind.LatinExtension, 1);
        private static readonly CodeSet LatinSpecialSet = new CodeSet(GraphicSetKind.LatinSpecial, 1);
        private static readonly CodeSet HiraganaSet = new CodeSet(GraphicSetKind.Hiragana, 1);
        private static readonly CodeSet KatakanaSet = new CodeSet(GraphicSetKind.Katakana, 1);
        private static readonly CodeSet MosaicSet = new CodeSet(GraphicSetKind.Mosaic, 1);
        private static readonly CodeSet JisX0201KatakanaSet = new CodeSet(GraphicSetKind.JisX0201Katakana, 1);
        private static readonly CodeSet DrcsOneByteSet = new CodeSet(GraphicSetKind.Drcs, 1);
        private static readonly CodeSet DrcsTwoByteSet = new CodeSet(GraphicSetKind.Drcs, 2);
        private static readonly CodeSet MacroSet = new CodeSet(GraphicSetKind.Macro, 1);

        private const string GetaMark = "〓"; // shown for characters that cannot be mapped (DRCS etc.)

        private readonly AribProfile _profile;
        private readonly CodeSet[] _g = new CodeSet[4];
        private int _glIndex;
        private int _grIndex;
        private float _horizontalScale;
        private float _verticalScale;
        private readonly StringBuilder _text = new StringBuilder();

        public AribB24Decoder(AribProfile profile = AribProfile.ProfileA)
        {
            _profile = profile;
            Reset();
        }

        public void Reset()
        {
            if (_profile == AribProfile.Latin)
            {
                _g[0] = AlphanumericSet;
                _g[1] = AlphanumericSet;
                _g[2] = LatinExtensionSet;
                _g[3] = LatinSpecialSet;
                _horizontalScale = 0.5f; // Latin defaults to middle size
                _verticalScale = 1.0f;
            }
            else if (_profile == AribProfile.ProfileC)
            {
                _g[0] = DrcsOneByteSet;
                _g[1] = AlphanumericSet;
                _g[2] = KanjiSet;
                _g[3] = MacroSet;
                _horizontalScale = 1.0f;
                _verticalScale = 1.0f;
            }
            else
            {
                _g[0] = KanjiSet;
                _g[1] = AlphanumericSet;
                _g[2] = HiraganaSet;
                _g[3] = MacroSet;
                _horizontalScale = 1.0f;
                _verticalScale = 1.0f;
            }

            _glIndex = 0;
            _grIndex = 2;
            _text.Clear();
        }

        /// <summary>
        /// Decode a caption statement body (or part of one - state is kept between calls until <see cref="Reset"/>).
        /// </summary>
        /// <returns>The text decoded so far</returns>
        public string Decode(byte[] buffer, int index, int length)
        {
            var end = index + length;
            if (end > buffer.Length)
            {
                end = buffer.Length;
            }

            DecodeInternal(buffer, index, end, 0);
            return GetText();
        }

        /// <summary>
        /// Decode ARIB STD-B24 encoded text (one-shot, full-seg profile) - used by the ARIB B-36 format reader.
        /// </summary>
        public static string AribToString(byte[] buffer, int index, int length)
        {
            var decoder = new AribB24Decoder();
            return decoder.Decode(buffer, index, length);
        }

        private string GetText()
        {
            var sb = new StringBuilder(_text.Length);
            foreach (var line in _text.ToString().SplitToLines())
            {
                var s = line.Trim();
                if (s.Length > 0)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(Environment.NewLine);
                    }

                    sb.Append(s);
                }
            }

            return sb.ToString();
        }

        private void DecodeInternal(byte[] buffer, int index, int end, int recursionDepth)
        {
            var pos = index;
            while (pos < end)
            {
                var b = buffer[pos];
                if (b <= 0x20)
                {
                    pos += HandleC0(buffer, pos, end, recursionDepth);
                }
                else if (b < 0x7f)
                {
                    pos += HandleGraphicCharacter(buffer, pos, end, _g[_glIndex], recursionDepth);
                }
                else if (b <= 0xa0)
                {
                    pos += HandleC1(buffer, pos, end);
                }
                else if (b < 0xff)
                {
                    pos += HandleGraphicCharacter(buffer, pos, end, _g[_grIndex], recursionDepth);
                }
                else
                {
                    pos++;
                }
            }
        }

        private void AppendNewline()
        {
            if (_text.Length > 0 && _text[_text.Length - 1] != '\n')
            {
                _text.Append('\n');
            }
        }

        private bool IsMiddleSize => _horizontalScale * 2f == _verticalScale;

        private int HandleC0(byte[] buffer, int pos, int end, int recursionDepth)
        {
            switch (buffer[pos])
            {
                case 0x08: // APB - active position backward
                    return 1;
                case 0x09: // APF - active position forward
                    _text.Append(' ');
                    return 1;
                case 0x0a: // APD - active position down
                case 0x0d: // APR - active position return
                    AppendNewline();
                    return 1;
                case 0x0b: // APU - active position up
                    return 1;
                case 0x0c: // CS - clear screen
                    _text.Clear();
                    return 1;
                case 0x0e: // LS1 - locking shift 1
                    _glIndex = 1;
                    return 1;
                case 0x0f: // LS0 - locking shift 0
                    _glIndex = 0;
                    return 1;
                case 0x16: // PAPF - parameterized active position forward
                    _text.Append(' ');
                    return 2;
                case 0x19: // SS2 - single shift 2
                    if (pos + 1 < end)
                    {
                        return 1 + HandleGraphicCharacter(buffer, pos + 1, end, _g[2], recursionDepth);
                    }

                    return 1;
                case 0x1b: // ESC
                    return 1 + HandleEsc(buffer, pos + 1, end);
                case 0x1c: // APS - active position set (2 parameter bytes)
                    AppendNewline();
                    return 3;
                case 0x1d: // SS3 - single shift 3
                    if (pos + 1 < end)
                    {
                        return 1 + HandleGraphicCharacter(buffer, pos + 1, end, _g[3], recursionDepth);
                    }

                    return 1;
                case 0x20: // SP - space
                    _text.Append(' ');
                    return 1;
                default: // NUL, BEL, CAN, RS, US...
                    return 1;
            }
        }

        private int HandleEsc(byte[] buffer, int pos, int end)
        {
            if (pos >= end)
            {
                return 0;
            }

            var b = buffer[pos];
            switch (b)
            {
                case 0x6e: // LS2
                    _glIndex = 2;
                    return 1;
                case 0x6f: // LS3
                    _glIndex = 3;
                    return 1;
                case 0x7e: // LS1R
                    _grIndex = 1;
                    return 1;
                case 0x7d: // LS2R
                    _grIndex = 2;
                    return 1;
                case 0x7c: // LS3R
                    _grIndex = 3;
                    return 1;
            }

            if (b == 0x24) // designate two-byte set
            {
                if (pos + 1 >= end)
                {
                    return 1;
                }

                var b2 = buffer[pos + 1];
                if (b2 >= 0x28 && b2 <= 0x2b)
                {
                    if (pos + 2 >= end)
                    {
                        return 2;
                    }

                    var gIndex = b2 - 0x28;
                    if (buffer[pos + 2] == 0x20) // two-byte DRCS
                    {
                        if (pos + 3 < end)
                        {
                            DesignateDrcs(gIndex, buffer[pos + 3], true);
                        }

                        return 4;
                    }

                    Designate(gIndex, buffer[pos + 2], true);
                    return 3;
                }

                Designate(0, b2, true);
                return 2;
            }

            if (b >= 0x28 && b <= 0x2b) // designate one-byte set
            {
                if (pos + 1 >= end)
                {
                    return 1;
                }

                var gIndex = b - 0x28;
                if (buffer[pos + 1] == 0x20) // one-byte DRCS
                {
                    if (pos + 2 < end)
                    {
                        DesignateDrcs(gIndex, buffer[pos + 2], false);
                    }

                    return 3;
                }

                Designate(gIndex, buffer[pos + 1], false);
                return 2;
            }

            return 1;
        }

        private void Designate(int gIndex, byte finalByte, bool twoByte)
        {
            switch (finalByte)
            {
                case 0x42: // kanji
                case 0x39: // JIS X 0213:2004 plane 1
                case 0x3a: // JIS X 0213:2004 plane 2
                case 0x3b: // ARIB additional symbols
                    _g[gIndex] = KanjiSet;
                    break;
                case 0x4a: // alphanumeric
                    _g[gIndex] = AlphanumericSet;
                    break;
                case 0x4b: // Latin extension (ABNT NBR 15606-1)
                    _g[gIndex] = LatinExtensionSet;
                    break;
                case 0x4c: // Latin special (ABNT NBR 15606-1)
                    _g[gIndex] = LatinSpecialSet;
                    break;
                case 0x30: // hiragana
                case 0x37: // proportional hiragana
                    _g[gIndex] = HiraganaSet;
                    break;
                case 0x31: // katakana
                case 0x38: // proportional katakana
                    _g[gIndex] = KatakanaSet;
                    break;
                case 0x36: // proportional alphanumeric
                    _g[gIndex] = AlphanumericSet;
                    break;
                case 0x49: // JIS X 0201 katakana
                    _g[gIndex] = JisX0201KatakanaSet;
                    break;
                case 0x32: // mosaic A
                case 0x33: // mosaic B
                case 0x34: // mosaic C
                case 0x35: // mosaic D
                    _g[gIndex] = MosaicSet;
                    break;
                default:
                    _g[gIndex] = twoByte ? KanjiSet : AlphanumericSet;
                    break;
            }
        }

        private void DesignateDrcs(int gIndex, byte finalByte, bool twoByte)
        {
            if (finalByte == 0x70) // macro set
            {
                _g[gIndex] = MacroSet;
            }
            else if (finalByte == 0x40) // DRCS-0 is a two-byte set
            {
                _g[gIndex] = DrcsTwoByteSet;
            }
            else
            {
                _g[gIndex] = twoByte ? DrcsTwoByteSet : DrcsOneByteSet;
            }
        }

        private int HandleC1(byte[] buffer, int pos, int end)
        {
            switch (buffer[pos])
            {
                case 0x88: // SSZ - small size
                    _horizontalScale = 0.5f;
                    _verticalScale = 0.5f;
                    return 1;
                case 0x89: // MSZ - middle size (half width)
                    _horizontalScale = 0.5f;
                    _verticalScale = 1.0f;
                    return 1;
                case 0x8a: // NSZ - normal size
                    _horizontalScale = 1.0f;
                    _verticalScale = 1.0f;
                    return 1;
                case 0x8b: // SZX - character size controls (1 parameter byte)
                    if (pos + 1 < end)
                    {
                        switch (buffer[pos + 1])
                        {
                            case 0x41: // double height
                                _verticalScale = 2.0f;
                                break;
                            case 0x44: // double width
                                _horizontalScale = 2.0f;
                                break;
                            case 0x45: // double height and width
                                _horizontalScale = 2.0f;
                                _verticalScale = 2.0f;
                                break;
                        }
                    }

                    return 2;
                case 0x90: // COL - color controls
                    if (pos + 1 < end && buffer[pos + 1] == 0x20)
                    {
                        return 3; // palette selection has an extra parameter byte
                    }

                    return 2;
                case 0x91: // FLC - flashing control
                case 0x93: // POL - pattern polarity
                case 0x94: // WMM - writing mode modification
                case 0x97: // HLC - highlight character block
                case 0x98: // RPC - repeat character
                    return 2;
                case 0x92: // CDC - conceal display controls
                    if (pos + 1 < end && buffer[pos + 1] == 0x20)
                    {
                        return 3;
                    }

                    return 2;
                case 0x9b: // CSI - control sequence introducer
                    return 1 + HandleCsi(buffer, pos + 1, end);
                case 0x9d: // TIME - time controls (2 parameter bytes)
                    return 3;
                default: // BKF-WHF colors, MACRO, SPL, STL...
                    return 1;
            }
        }

        private static int HandleCsi(byte[] buffer, int pos, int end)
        {
            // parameter bytes (0x30-0x39, separator 0x3B) followed by
            // an intermediate byte 0x20 and a final byte
            var i = pos;
            while (i < end && buffer[i] != 0x20)
            {
                i++;
            }

            i++; // the final byte after the 0x20 intermediate
            return i - pos + 1;
        }

        private int HandleGraphicCharacter(byte[] buffer, int pos, int end, CodeSet codeSet, int recursionDepth)
        {
            var b1 = (byte)(buffer[pos] & 0x7f);
            if (b1 < 0x21 || b1 > 0x7e)
            {
                return 1;
            }

            if (codeSet.Bytes == 2)
            {
                if (pos + 1 >= end)
                {
                    return 1;
                }

                var b2 = (byte)(buffer[pos + 1] & 0x7f);
                if (b2 < 0x21 || b2 > 0x7e)
                {
                    return 2;
                }

                if (codeSet.Kind == GraphicSetKind.Kanji)
                {
                    AppendKanji(b1, b2);
                }
                else if (codeSet.Kind == GraphicSetKind.Drcs)
                {
                    _text.Append(GetaMark);
                }

                return 2;
            }

            switch (codeSet.Kind)
            {
                case GraphicSetKind.Alphanumeric:
                    if (_profile == AribProfile.Latin)
                    {
                        AppendFromTable(AribB24Tables.AlphanumericTable_Latin, b1);
                    }
                    else if (IsMiddleSize)
                    {
                        AppendFromTable(AribB24Tables.AlphanumericTable_Halfwidth, b1);
                    }
                    else
                    {
                        AppendFromTable(AribB24Tables.AlphanumericTable_Fullwidth, b1);
                    }

                    break;
                case GraphicSetKind.LatinExtension:
                    AppendFromTable(AribB24Tables.LatinExtensionTable, b1);
                    break;
                case GraphicSetKind.LatinSpecial:
                    AppendFromTable(AribB24Tables.LatinSpecialTable, b1);
                    break;
                case GraphicSetKind.Hiragana:
                    AppendFromTable(AribB24Tables.HiraganaTable, b1);
                    break;
                case GraphicSetKind.Katakana:
                    AppendFromTable(AribB24Tables.KatakanaTable, b1);
                    break;
                case GraphicSetKind.JisX0201Katakana:
                    AppendFromTable(AribB24Tables.JISX0201KatakanaTable, b1);
                    break;
                case GraphicSetKind.Macro:
                    if (b1 >= 0x60 && b1 <= 0x6f && recursionDepth < 3)
                    {
                        var macro = AribB24Tables.DefaultMacros[b1 & 0x0f];
                        DecodeInternal(macro, 0, macro.Length, recursionDepth + 1);
                    }

                    break;
                case GraphicSetKind.Drcs:
                    _text.Append(GetaMark);
                    break;
            }

            return 1;
        }

        private void AppendFromTable(string[] table, byte b1)
        {
            var index = b1 - 0x21;
            if (index < table.Length && table[index] != null)
            {
                _text.Append(table[index]);
            }
        }

        private void AppendKanji(byte b1, byte b2)
        {
            var row = b1 - 0x21; // 0-based ku
            var cell = b2 - 0x21; // 0-based ten
            string s = null;
            if (row < 84)
            {
                s = AribB24Tables.KanjiTable[row * 94 + cell];
            }
            else // rows 85-94: ARIB additional kanji and symbols (gaiji)
            {
                var index = (row - 84) * 94 + cell;
                if (index < AribB24Tables.AdditionalSymbolsTable_Unicode.Length)
                {
                    s = AribB24Tables.AdditionalSymbolsTable_Unicode[index];
                }
            }

            _text.Append(s ?? GetaMark);
        }
    }
}
