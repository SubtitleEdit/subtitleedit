using System;
using System.Text;
using Nikse.SubtitleEdit.Core.TransportStream;

namespace Nikse.SubtitleEdit.Core.SubtitleFormats
{

    public static class AribB24Decoder
    {

        public static string AribToString(byte[] buffer, int index, int length)
        {
            int end = index + length;
            if (end > buffer.Length)
                end = buffer.Length;

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
                }
                else if (b == 0x7f) // DEL - Delete character
                {

                }
                else if (b == 0x20) // SP - Space character
                {

                }

                else if (b <= 0x1f) // C0
                {
                    ParseC0ControlSet(sb, b, ref pos, buffer);
                }
                else if (b >= 0x21 && b <= 0x7e) // GL
                {
                    ParseGlArea(sb, ref pos, buffer);
                }
                else if (b >= 0x80 && b <= 0x9f) // C1
                {
                    ParseC1ControlSet(sb, b, ref pos, buffer);
                }
                else if (b >= 0xc0) // GR
                {
                    ParseGrArea(sb, ref pos, buffer);
                }
            }
            return sb.ToString();
        }

        private static void ParseC0ControlSet(StringBuilder sb, byte b, ref int pos, byte[] buffer)
        {
            switch (b)
            {
                case 0x07: // Bell
                    break;
                    //0x07 = BEL // Bell
                    //0x08 = APB // Active position backward
                    //0x09 = APF // Active position forward
                    //0x0a = APD // Active position down
                    //0x0b = APU // Active position up
                    //0x0c = CS  // Clear screen
                    //0x0d = APR // Active position return
                    //0x0e = LS1 // Locking shift 1
                    //0x0f = LS0 // Locking shift 0

                    //0x16 = PAPF // Parameterized active position forward

                    //0x18 = CAN // Cancel
                    //0x19 = SS2 // Single shift 2

                    //0x1b = ESC // Escape
                    //0x1c = APS // Active position set
                    //0x1d = SS3 // Single shift 3
                    //0x1e = RS // Record separator
                    //0x1f = US // Unit separator
            }
        }

        private static void ParseC1ControlSet(StringBuilder sb, byte b, ref int pos, byte[] buffer)
        {
            switch (b)
            {
                case 0x80: //  BKF - Black Foreground
                    break;
                //0x81 = RDF - Red Foreground
                //0x82 = GRF - Green Foreground
                case 0x83: // YLF - Yellow Foreground

                    break;
                //0x84 = BLF - Blue Foreground
                //0x85 = MGF - Magenta Foreground
                //0x86 = CNF - Cyan Foreground
                //0x87 = WHF - White Foreground
                //0x88 = SSZ - Small Size
                //0x89 = MSZ - Middle Size
                //0x8a = NSZ - Normal Size
                //0x8b = SZX - Character Size Controls

                case 0x90: // = COL - Colour Controls
                    b = buffer[pos];
                    if (b == 0x20)
                    {
                        pos++;
                    }
                    break;
                //0x91 = FLC - Flashing control
                //0x92 = CDC - Conceal Display Controls
                //0x93 = POL - Pattern Polarity Controls
                //0x94 = WMM - Writing Mode Modification
                //0x95 = MACRO - Macro Command

                //0x97 = HLC - Highlight Character Block
                //0x98 = RPC - Repeat Character
                //0x99 = SPL - Stop Lining
                //0x9a = STL - Start Lining
                //0x9b = CSI - Control Sequence Introducer

                //0x9d = TIME - Time Controls
            }
            pos++;
        }

        private static void ParseGlArea(StringBuilder sb, ref int pos, byte[] buffer)
        {
            // Single-byte (Halfwidth) Characters
            throw new NotImplementedException();
        }

        private static void ParseGrArea(StringBuilder sb, ref int pos, byte[] buffer)
        {
            throw new NotImplementedException();
        }

    }
}
