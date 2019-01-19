using System.Text;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    /// <summary>
    /// ARIB B24 decoding
    /// See https://github.com/johnoneil/arib/blob/master/analysis/analysis.txt + https://github.com/johnoneil/arib/blob/master/docs/arib_std_b-24_v5.2-E1_Data_Coding_for_Digital_Broadcast.pdf
    /// </summary>
    public static class AribB24Decoder
    {

        private static readonly Encoding EncodingKanji = Encoding.GetEncoding("ISO-2022-JP");

        public static string AribToString(byte[] buffer, int index, int length)
        {
            int end = index + length;
            if (end > buffer.Length)
            {
                end = buffer.Length;
            }

            var sb = new StringBuilder();
            int pos = index;
            while (pos < end)
            {
                var b = buffer[pos++];

                if (b == 0x09) // Tab (09)
                {
                    sb.Append("\t");
                }
                else if (b == 0x0d && pos < end && buffer[pos] == 0x0a) // Carriage Return/Line Feed (0D0A)
                {
                    sb.AppendLine();
                    pos++;
                }
                else if (b <= 0x1f) // C0
                {
                    ParseC0ControlSet(sb, b, ref pos, buffer);
                }
                else if (b == 0x20) // SP - Space character
                {
                    sb.Append(" ");
                }
                else if (b >= 0x21 && b <= 0x7e) // GL - Graphic-set left
                {
                    ParseGlArea(sb, b, ref pos, buffer);
                }
                else if (b == 0x7f) // DEL - Delete character
                {
                }
                else if (b >= 0x80 && b <= 0x9f) // C1
                {
                    ParseC1ControlSet(sb, b, ref pos, buffer);
                }
                else if (b >= 0xc0) // GR - Graphic-set right
                {
                    ParseGrArea(sb, b, ref pos, buffer);
                }
            }
            return sb.ToString();
        }

        private static void ParseC0ControlSet(StringBuilder sb, byte b, ref int pos, byte[] buffer)
        {
            switch (b)
            {
                case 0x07: // BEL - Bell
                    break;
                case 0x08: // APB - Active position backward
                    break;
                case 0x09: // APF - Active position forward
                    break;
                case 0x0a: // APD - Active position down
                    break;
                case 0x0b: // APU - Active position up
                    break;
                case 0x0c: // CS  - Clear screen
                    break;
                case 0x0d: // APR - Active position return
                    break;
                case 0x0e: // LS1 - Locking shift 1
                    break;
                case 0x0f: // LS0 - Locking shift 0
                    break;

                case 0x16: // PAPF - Parameterized active position forward
                    break;

                case 0x18: // CAN - Cancel
                    break;
                case 0x19: // SS2 - Single shift 2
                    break;

                case 0x1b: // ESC - Escape
                    break;
                case 0x1c: // APS - Active position set
                    break;
                case 0x1d: // SS3 - Single shift 3
                    break;
                case 0x1e: // RS - Record separator
                    break;
                case 0x1f: // US - Unit separator
                    break;
            }
        }

        private static void ParseC1ControlSet(StringBuilder sb, byte b, ref int pos, byte[] buffer)
        {
            switch (b)
            {
                case 0x80: //  BKF - Black Foreground
                    break;
                case 0x81: // RDF - Red Foreground
                    break;
                case 0x82: // GRF - Green Foreground
                    break;
                case 0x83: // YLF - Yellow Foreground
                    break;
                case 0x84: // BLF - Blue Foreground
                    break;
                case 0x85: // MGF - Magenta Foreground
                    break;
                case 0x86: // CNF - Cyan Foreground
                    break;
                case 0x87: // WHF - White Foreground
                    break;
                case 0x88: // SSZ - Small Size
                    break;
                case 0x89: // MSZ - Middle Size
                    break;
                case 0x8a: // NSZ - Normal Size
                    break;
                case 0x8b: // SZX - Character Size Controls
                    b = buffer[pos++];
                    //SZX 06 / 0 : Tiny size (0x60)
                    //SZX 04 / 1 : Double height (0x41)
                    //SZX 04 / 4 : Double width (0x44)
                    //SZX 04 / 5 : Double height and width (0x45)
                    //SZX 06 / 11 : Special 1 (0x6b)
                    //SZX 06 / 4 : Special 2 (0x64)
                    break;
                case 0x90: // = COL - Colour Controls
                    b = buffer[pos++];
                    if (b == 0x20)
                    {
                        pos++;
                    }
                    break;
                case 0x91: // LC - Flashing control
                    break;
                case 0x92: // DC - Conceal Display Controls
                    break;
                case 0x93: // OL - Pattern Polarity Controls
                    break;
                case 0x94: // MM - Writing Mode Modification
                    break;
                case 0x95: // MACRO - Macro Command
                    break;

                case 0x97: // HLC - Highlight Character Block
                    break;
                case 0x98: // RPC - Repeat Character
                    break;
                case 0x99: // SPL - Stop Lining
                    break;
                case 0x9a: // STL - Start Lining
                    break;
                case 0x9b: // CSI - Control Sequence Introducer
                    break;

                case 0x9d: // TIME - Time Controls
                    break;
            }
        }

        private static void ParseGlArea(StringBuilder sb, byte b, ref int pos, byte[] buffer)
        {
            sb.Append(GetKanjiChar(b, buffer[pos++]));
        }

        private static void ParseGrArea(StringBuilder sb, byte b, ref int pos, byte[] buffer)
        {
            sb.Append(GetKanjiChar(b, buffer[pos++]));
        }

        private static string GetKanjiChar(byte b1, byte b2)
        {
            return EncodingKanji.GetString(new byte[] { 0x1B, 0x24, 0x40, b1, b2, 0x1B, 0x28, 0x4A });
        }

    }
}
