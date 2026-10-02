using Nikse.SubtitleEdit.Core.Cea708.Commands;
using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Core.Cea708
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/CEA-708
    /// </summary>
    public static class Cea708
    {
        public static bool DebugMode => Configuration.Settings.SubtitleSettings.MccDebug;

        private static readonly Dictionary<byte, string> SingleCharLookupTable = new Dictionary<byte, string>
        {
            // G0 character table
            { 000, "" },
            { 001, "" },
            { 002, "" },
            { 003, "" },
            { 004, "" },
            { 005, "" },
            { 006, "" },
            { 007, "" },
            { 008, "" },
            { 009, "" },
            { 010, "" },
            { 011, "" },
            { 012, "" },
            { 013, "\r" },
            { 014, "" },
            { 015, "" },
            { 016, "" },
            { 017, "" },
            { 018, "" },
            { 019, "" },
            { 020, "" },
            { 021, "" },
            { 022, "" },
            { 023, "" },
            { 024, "" },
            { 025, "" },
            { 026, "" },
            { 027, "" },
            { 028, "" },
            { 029, "" },
            { 030, "" },
            { 031, "" },
            { 032, " " },
            { 033, "!" },
            { 034, "\"" },
            { 035, "#" },
            { 036, "$" },
            { 037, "%" },
            { 038, "&" },
            { 039, "'" },
            { 040, "(" },
            { 041, ")" },
            { 042, "*" },
            { 043, "+" },
            { 044, "," },
            { 045, "-" },
            { 046, "." },
            { 047, "/" },
            { 048, "0" },
            { 049, "1" },
            { 050, "2" },
            { 051, "3" },
            { 052, "4" },
            { 053, "5" },
            { 054, "6" },
            { 055, "7" },
            { 056, "8" },
            { 057, "9" },
            { 058, ":" },
            { 059, ";" },
            { 060, "<" },
            { 061, "=" },
            { 062, ">" },
            { 063, "?" },
            { 064, "@" },
            { 065, "A" },
            { 066, "B" },
            { 067, "C" },
            { 068, "D" },
            { 069, "E" },
            { 070, "F" },
            { 071, "G" },
            { 072, "H" },
            { 073, "I" },
            { 074, "J" },
            { 075, "K" },
            { 076, "L" },
            { 077, "M" },
            { 078, "N" },
            { 079, "O" },
            { 080, "P" },
            { 081, "Q" },
            { 082, "R" },
            { 083, "S" },
            { 084, "T" },
            { 085, "U" },
            { 086, "V" },
            { 087, "W" },
            { 088, "X" },
            { 089, "Y" },
            { 090, "Z" },
            { 091, "[" },
            { 092, "\\" },
            { 093, "]" },
            { 094, "^" },
            { 095, "_" },
            { 096, "`" },
            { 097, "a" },
            { 098, "b" },
            { 099, "c" },
            { 100, "d" },
            { 101, "e" },
            { 102, "f" },
            { 103, "g" },
            { 104, "h" },
            { 105, "i" },
            { 106, "j" },
            { 107, "k" },
            { 108, "l" },
            { 109, "m" },
            { 110, "n" },
            { 111, "o" },
            { 112, "p" },
            { 113, "q" },
            { 114, "r" },
            { 115, "s" },
            { 116, "t" },
            { 117, "u" },
            { 118, "v" },
            { 119, "w" },
            { 120, "x" },
            { 121, "y" },
            { 122, "z" },
            { 123, "{" },
            { 124, "|" },
            { 125, "}" },
            { 126, "~" },
            { 127, "♪" },
            { 128, "" },
            { 129, "" },
            { 130, "" },
            { 131, "" },
            { 132, "" },
            { 133, "" },
            { 134, "" },
            { 135, "" },
            { 136, "" },
            { 137, "" },
            { 138, "" },
            { 139, "" },
            { 140, "" },
            { 141, "" },
            { 142, "" },
            { 143, "" },
            { 144, "" },
            { 145, "" },
            { 146, "" },
            { 147, "" },
            { 148, "" },
            { 149, "" },
            { 150, "" },
            { 151, "" },
            { 152, "" },
            { 153, "" },
            { 154, "" },
            { 155, "" },
            { 156, "" },
            { 157, "" },
            { 158, "" },
            { 159, "" },

            // G1 character table
            { 160, " " }, // non breaking space
            { 161, "¡" },
            { 162, "¢" },
            { 163, "£" },
            { 164, "¤" },
            { 165, "¥" },
            { 166, "¦" },
            { 167, "§" },
            { 168, "¨" },
            { 169, "©" },
            { 170, "ª" },
            { 171, "«" },
            { 172, "¬" },
            { 173, "-" },
            { 174, "®" },
            { 175, "¯" },
            { 176, "°"},
            { 177, "±" },
            { 178, "²" },
            { 179, "³" },
            { 180, "´" },
            { 181, "µ" },
            { 182, "¶" },
            { 183, "·" },
            { 184, "¸" },
            { 185, "¹" },
            { 186, "º" },
            { 187, "»" },
            { 188, "¼" },
            { 189, "½" },
            { 190, "¾" },
            { 191, "¿" },
            { 192, "À" },
            { 193, "Á" },
            { 194, "Â" },
            { 195, "Ã" },
            { 196, "Ä" },
            { 197, "Å" },
            { 198, "Æ" },
            { 199, "Ç" },
            { 200, "È" },
            { 201, "É" },
            { 202, "Ê" },
            { 203, "Ë" },
            { 204, "Ì" },
            { 205, "Í" },
            { 206, "Î" },
            { 207, "Ï" },
            { 208, "Ð" },
            { 209, "Ñ" },
            { 210, "Ò" },
            { 211, "Ó" },
            { 212, "Ô" },
            { 213, "Õ" },
            { 214, "Ö" },
            { 215, "×" },
            { 216, "Ø" },
            { 217, "Ù" },
            { 218, "Ú" },
            { 219, "Û" },
            { 220, "Ü" },
            { 221, "Ý" },
            { 222, "Þ" },
            { 223, "ß" },
            { 224, "à" },
            { 225, "á" },
            { 226, "â" },
            { 227, "ã" },
            { 228, "ä" },
            { 229, "å" },
            { 230, "æ" },
            { 231, "ç" },
            { 232, "è" },
            { 233, "é" },
            { 234, "ê" },
            { 235, "ë" },
            { 236, "ì" },
            { 237, "í" },
            { 238, "î" },
            { 239, "ï" },
            { 240, "ð" },
            { 241, "ñ" },
            { 242, "ò" },
            { 243, "ó" },
            { 244, "ô" },
            { 245, "õ" },
            { 246, "ö" },
            { 247, "÷" },
            { 248, "ø" },
            { 249, "ù" },
            { 250, "ú" },
            { 251, "û" },
            { 252, "ü" },
            { 253, "ý" },
            { 254, "þ" },
            { 255, "ÿ" },
        };

        // G2: Extended Control Code Set 1 - reached via the EXT1 (0x10) prefix byte
        private static readonly Dictionary<byte, string> G2CharLookupTable = new Dictionary<byte, string>
        {
            { 0x20, " " }, // transparent space
            { 0x21, " " }, // non breaking transparent space
            { 0x25, "…" },
            { 0x2A, "Š" },
            { 0x2C, "Œ" },
            { 0x30, "█" },
            { 0x31, "‘" }, // '
            { 0x32, "’" }, // '
            { 0x33, "“" }, // "
            { 0x34, "”" }, // "
            { 0x35, "•" },
            { 0x39, "™" },
            { 0x3A, "š" },
            { 0x3C, "œ" },
            { 0x3D, "℠" },
            { 0x3F, "Ÿ" },
            { 0x76, "⅛" },
            { 0x77, "⅜" },
            { 0x78, "⅝" },
            { 0x79, "⅞" },
            { 0x7A, "│" },
            { 0x7B, "┐" },
            { 0x7C, "└" },
            { 0x7D, "─" },
            { 0x7E, "┘" },
            { 0x7F, "┌" },
        };

        private static Dictionary<char, byte[]> _textLookupTable;

        private const byte Backspace = 0x08;
        private const byte FormFeed = 0x0C;
        private const byte CarriageReturn = 0x0D;
        private const byte HorizontalCarriageReturn = 0x0E;

        private static Encoding _eucKr;
        private static bool _eucKrUnavailable;

        /// <summary>
        /// Decodes a P16 character. CEA-708 leaves its coding open; Korean broadcasters (TTA
        /// standard) send KS X 1001 as EUC-KR, i.e. both bytes 0xA1-0xFE - read as UTF-16 that is
        /// nonsense Hangul. Anything else is taken as UTF-16.
        /// </summary>
        internal static string DecodeP16(byte first, byte second)
        {
            if (first >= 0xA1 && first <= 0xFE && second >= 0xA1 && second <= 0xFE)
            {
                var eucKr = GetEucKrEncoding();
                if (eucKr != null)
                {
                    var s = eucKr.GetString(new[] { first, second });
                    if (s.Length == 1 && s[0] != '�' && s[0] != '?')
                    {
                        return s;
                    }
                }
            }

            return ((char)((first << 8) | second)).ToString();
        }

        private static Encoding GetEucKrEncoding()
        {
            if (_eucKr == null && !_eucKrUnavailable)
            {
                try
                {
                    _eucKr = Encoding.GetEncoding(949, EncoderFallback.ReplacementFallback, DecoderFallback.ReplacementFallback);
                }
                catch
                {
                    _eucKrUnavailable = true; // code pages provider not registered
                }
            }

            return _eucKr;
        }

        private static bool IsCurrentWindowVisible(CommandState state)
        {
            return state.CurrentWindow >= 0 && state.VisibleWindows[state.CurrentWindow];
        }

        private static void AddText(CommandState state, SetText text)
        {
            if (state.PendingWindow < 0)
            {
                state.PendingWindow = state.CurrentWindow;
            }

            state.Commands.Add(text);
        }

        /// <summary>
        /// Whether the buffered text is in one of these windows - also true when its window is
        /// unknown, so streams without window commands still flush.
        /// </summary>
        private static bool IsPendingTextIn(CommandState state, bool[] windows)
        {
            var w = state.PendingWindow;
            return w < 0 || w >= windows.Length || windows[w];
        }

        /// <summary>
        /// Whether the buffered text is on screen (roll-up/paint-on) in one of these windows - text
        /// buffered in a hidden window is a pop-on caption still being built.
        /// </summary>
        private static bool IsPendingTextVisibleIn(CommandState state, bool[] windows)
        {
            var w = state.PendingWindow;
            return IsPendingTextIn(state, windows) && (w < 0 || w >= state.VisibleWindows.Length || state.VisibleWindows[w]);
        }

        /// <summary>
        /// Removes the last not yet flushed character (BS).
        /// </summary>
        private static void RemoveLastPendingChar(CommandState state)
        {
            for (var index = state.Commands.Count - 1; index >= 0; index--)
            {
                var command = state.Commands[index];
                if (command is SetPenLocation)
                {
                    return; // the pen moved - nothing to erase on this row
                }

                if (command is SetText text && !string.IsNullOrEmpty(text.Content))
                {
                    if (text.Content == "\r")
                    {
                        return;
                    }

                    text.Content = text.Content.Substring(0, text.Content.Length - 1);
                    return;
                }
            }
        }

        /// <summary>
        /// Removes the not yet flushed text of the current row (HCR), or of all rows (FF).
        /// </summary>
        private static void RemovePendingText(CommandState state, bool currentRowOnly)
        {
            if (!currentRowOnly)
            {
                state.PendingWindow = -1;
            }

            for (var index = state.Commands.Count - 1; index >= 0; index--)
            {
                var command = state.Commands[index];
                if (currentRowOnly && (command is SetPenLocation || command is SetText { Content: "\r" }))
                {
                    return;
                }

                if (command is SetText)
                {
                    state.Commands.RemoveAt(index);
                }
            }
        }

        private static void SetWindowsVisible(CommandState state, bool[] windows, bool visible)
        {
            for (var w = 0; w < windows.Length && w < state.VisibleWindows.Length; w++)
            {
                if (windows[w])
                {
                    state.VisibleWindows[w] = visible;
                }
            }
        }

        public static string Decode(int lineIndex, byte[] bytes, CommandState state, bool flush)
        {
            var i = 0;
            var debugBuilder = new StringBuilder();
            var textBuilder = new StringBuilder();
            state.FlushedTexts.Clear();
            state.StillVisibleFlushes.Clear();
            state.ErasedAtFlushCounts.Clear();

            while (i < bytes.Length)
            {
                var b = bytes[i];

                // Commands
                if (b == EndOfText.Id)
                {
                    //The EndOfText command is a Null Command which can be used to flush any buffered text to the current window. All commands force a flush of any buffered text to the current window, so this command is only needed when no other command is pending.

                    if (DebugMode)
                    {
                        debugBuilder.Append("{EndOfText}");
                    }
                }
                else if (b >= SetCurrentWindow.IdStart && b <= SetCurrentWindow.IdEnd)
                {
                    //SetCurrentWindow tells the caption decoder which window the following commands describe: SetWindowAttributes, SetPenAttributes, SetPenColor, SetPenLocation. If the window specified has not already been created with a DefineWindow command then
                    //SetCurrentWindow and the window property commands can be safely ignored. 
                    var currentWindow = new SetCurrentWindow(lineIndex, b - 0x80);
                    state.Commands.Add(currentWindow);
                    state.CurrentWindow = currentWindow.WindowIndex;
                    if (DebugMode)
                    {
                        debugBuilder.Append("{SetCurrentWindow:" + currentWindow.WindowIndex + "}");
                    }
                }
                else if (b == ClearWindows.Id)
                {
                    if (bytes.Length - i < 2)
                    {
                        break;
                    }

                    // ClearWindows clears all the windows specified in the 8 bit window bitmap.
                    var clearWindows = new ClearWindows(lineIndex, bytes, i + 1);
                    state.Commands.Add(clearWindows);
                    FlushShownCaptions(debugBuilder, textBuilder, state, clearWindows.Flags, lineIndex);
                    state.ErasedAtFlushCounts.Add(state.FlushedTexts.Count);
                    if (DebugMode)
                    {
                        debugBuilder.Append("{ClearWindows:" + clearWindows.Flags[0] + "," + clearWindows.Flags[1] + "," + clearWindows.Flags[2] + "," + clearWindows.Flags[3] + "," + clearWindows.Flags[4] + "," + clearWindows.Flags[5] + "," + clearWindows.Flags[6] + "," + clearWindows.Flags[7] + "}");
                    }

                    i++;
                }
                else if (b == DisplayWindows.Id)
                {
                    if (bytes.Length - i < 2)
                    {
                        break;
                    }

                    // DisplayWindows displays all the windows specified in the 8 bit window bitmap.
                    var displayWindows = new DisplayWindows(lineIndex, bytes, i + 1);
                    state.Commands.Add(displayWindows);
                    ShowPendingText(debugBuilder, textBuilder, state, displayWindows.Flags, lineIndex);
                    SetWindowsVisible(state, displayWindows.Flags, true);
                    if (DebugMode)
                    {
                        debugBuilder.Append("{DisplayWindows:" + displayWindows.Flags[0] + "," + displayWindows.Flags[1] + "," + displayWindows.Flags[2] + "," + displayWindows.Flags[3] + "," + displayWindows.Flags[4] + "," + displayWindows.Flags[5] + "," + displayWindows.Flags[6] + "," + displayWindows.Flags[7] + "}");
                    }

                    i++;
                }
                else if (b == HideWindows.Id)
                {
                    if (bytes.Length - i < 2)
                    {
                        break;
                    }

                    // HideWindows hides all the windows specified in the 8 bit window bitmap.
                    var hideWindows = new HideWindows(lineIndex, bytes, i + 1);
                    FlushShownCaptions(debugBuilder, textBuilder, state, hideWindows.Flags, lineIndex);
                    if (IsPendingTextVisibleIn(state, hideWindows.Flags))
                    {
                        Flush(debugBuilder, textBuilder, state);
                    }

                    state.Commands.Add(hideWindows);
                    state.ErasedAtFlushCounts.Add(state.FlushedTexts.Count);
                    SetWindowsVisible(state, hideWindows.Flags, false);
                    if (DebugMode)
                    {
                        debugBuilder.Append("{HideWindows:" + hideWindows.Flags[0] + "," + hideWindows.Flags[1] + "," + hideWindows.Flags[2] + "," + hideWindows.Flags[3] + "," + hideWindows.Flags[4] + "," + hideWindows.Flags[5] + "," + hideWindows.Flags[6] + "," + hideWindows.Flags[7] + "}");
                    }

                    i++;
                }
                else if (b == ToggleWindows.Id)
                {
                    if (bytes.Length - i < 2)
                    {
                        break;
                    }

                    // ToggleWindows hides all displayed windows, and displays all hidden windows specified in the 8 bit window bitmap.
                    var toggleWindows = new ToggleWindows(lineIndex, bytes, i + 1);
                    var hiddenByToggle = new bool[toggleWindows.Flags.Length];
                    for (var w = 0; w < hiddenByToggle.Length && w < state.VisibleWindows.Length; w++)
                    {
                        hiddenByToggle[w] = toggleWindows.Flags[w] && state.VisibleWindows[w];
                    }

                    FlushShownCaptions(debugBuilder, textBuilder, state, hiddenByToggle, lineIndex);
                    if (IsPendingTextVisibleIn(state, hiddenByToggle))
                    {
                        Flush(debugBuilder, textBuilder, state);
                    }

                    ShowPendingText(debugBuilder, textBuilder, state, toggleWindows.Flags, lineIndex);
                    state.Commands.Add(toggleWindows);
                    state.ErasedAtFlushCounts.Add(state.FlushedTexts.Count);
                    for (var w = 0; w < toggleWindows.Flags.Length && w < state.VisibleWindows.Length; w++)
                    {
                        if (toggleWindows.Flags[w])
                        {
                            state.VisibleWindows[w] = !state.VisibleWindows[w];
                        }
                    }
                    if (DebugMode)
                    {
                        debugBuilder.Append("{ToggleWindows:" + toggleWindows.Flags[0] + "," + toggleWindows.Flags[1] + "," + toggleWindows.Flags[2] + "," + toggleWindows.Flags[3] + "," + toggleWindows.Flags[4] + "," + toggleWindows.Flags[5] + "," + toggleWindows.Flags[6] + "," + toggleWindows.Flags[7] + "}");
                    }

                    i++;
                }
                else if (b == DeleteWindows.Id)
                {
                    if (bytes.Length - i < 2)
                    {
                        break;
                    }

                    // DeleteWindows deletes all the windows specified in the 8 bit window bitmap.If the current window, as specified by the last SetCurrentWindow command, is deleted then the current window becomes undefined and the window attribute commands should have no effect until after the next SetCurrentWindow or DefineWindow command.
                    var deleteWindows = new DeleteWindows(lineIndex, bytes, i + 1);
                    FlushShownCaptions(debugBuilder, textBuilder, state, deleteWindows.Flags, lineIndex);
                    if (IsPendingTextIn(state, deleteWindows.Flags))
                    {
                        Flush(debugBuilder, textBuilder, state);
                    }

                    state.Commands.Add(deleteWindows);
                    state.ErasedAtFlushCounts.Add(state.FlushedTexts.Count);
                    SetWindowsVisible(state, deleteWindows.Flags, false);
                    if (state.CurrentWindow >= 0 && state.CurrentWindow < deleteWindows.Flags.Length && deleteWindows.Flags[state.CurrentWindow])
                    {
                        state.CurrentWindow = -1;
                    }
                    if (DebugMode)
                    {
                        debugBuilder.Append("{DeleteWindows:" + deleteWindows.Flags[0] + "," + deleteWindows.Flags[1] + "," + deleteWindows.Flags[2] + "," + deleteWindows.Flags[3] + "," + deleteWindows.Flags[4] + "," + deleteWindows.Flags[5] + "," + deleteWindows.Flags[6] + "," + deleteWindows.Flags[7] + "}");
                    }

                    i++;
                }
                else if (b == Delay.Id)
                {
                    if (bytes.Length - i < 2)
                    {
                        break;
                    }

                    // Delay suspends all processing of the current service, except for DelayCancel and Reset scanning.The period of suspension is set to by the one byte parameter.The parameter specifies the delay in tenths of a second, so the minimum delay is 0.1 seconds, and the maximum delay is 25.5 seconds.A zero second delay can safely be ignored in a decoder, but should not be emitted from an encoder.A delay should be cancelled if the caption decoder's input buffer becomes full, a DelayCancel or Reset is received, or the specified delay time elapses.
                    var delay = new Delay(lineIndex, bytes, i + 1);
                    state.Commands.Add(delay);
                    if (DebugMode)
                    {
                        debugBuilder.Append("{Delay:" + delay.Milliseconds + "ms}");
                    }

                    i++;
                }
                else if (b == DelayCancel.Id)
                {
                    // DelayCancel terminates any active delay and resumes normal command processing. DelayCancel should be scanned for during a Delay.
                    var delayCancel = new DelayCancel(lineIndex);
                    state.Commands.Add(delayCancel);
                    if (DebugMode)
                    {
                        debugBuilder.Append("{DelayCancel}");
                    }
                }
                else if (b == Reset.Id)
                {
                    // Reset deletes all windows, cancels any active delay, and clears the buffer before the Reset command. Reset should be scanned for during a Delay. 
                    var reset = new Reset(lineIndex);
                    FlushShownCaptions(debugBuilder, textBuilder, state, null, lineIndex);
                    state.ErasedAtFlushCounts.Add(state.FlushedTexts.Count);
                    state.Commands.Add(reset);
                    if (DebugMode)
                    {
                        debugBuilder.Append("{Reset}");
                    }
                }
                else if (b == SetPenAttributes.Id)
                {
                    if (bytes.Length - i < 3)
                    {
                        break;
                    }

                    // The SetPenAttributes command specifies how certain attributes of subsequent characters are to be rendered in the current window, until the next SetPenAttributes command.
                    var penAttributes = new SetPenAttributes(lineIndex, bytes, i + 1);
                    state.Commands.Add(penAttributes);
                    if (DebugMode)
                    {
                        debugBuilder.Append($"{{SetPenAttributes:PenSize={penAttributes.PenSize},Offset={penAttributes.Offset},TextTag={penAttributes.TextTag},FontTag={penAttributes.FontTag},EdgeType={penAttributes.EdgeType},Underline={penAttributes.Underline},Italic={penAttributes.Italics}}}");
                    }

                    i += 2;
                }
                else if (b == SetPenColor.Id)
                {
                    if (bytes.Length - i < 4)
                    {
                        break;
                    }

                    // SetPenColor sets the foreground, background, and edge color for the subsequent characters. Color is specified with 6 bits, 2 for each of blue, green and red. The lowest order bits are for blue, the next two for green and the highest order bits represent red. Opacity is represented by two bits, they represent SOLID=0, FLASH=1, TRANSLUCENT=2, and TRANSPARENT=3. The edge color is the color of the outlined edges of the text, but the outline shares its opacity with the foreground, so the highest order bits of the third parameter byte should both be cleared.
                    var penColor = new SetPenColor(lineIndex, bytes, i + 1);
                    state.Commands.Add(penColor);
                    if (DebugMode)
                    {
                        debugBuilder.Append($"{{SetPenColor:Foreground=r{penColor.ForegroundColorRed} g{penColor.ForegroundColorGreen} b{penColor.ForegroundColorBlue} op-{penColor.ForegroundOpacity}, Background=r{penColor.BackgroundColorRed} g{penColor.BackgroundColorGreen} b{penColor.BackgroundColorBlue} op-{penColor.BackgroundOpacity}, Edge=r{penColor.EdgeColorRed} g{penColor.EdgeColorGreen} b{penColor.EdgeColorBlue}}}");
                    }

                    i += 3;
                }
                else if (b == SetPenLocation.Id)
                {
                    if (bytes.Length - i < 3)
                    {
                        break;
                    }

                    // SetPenLocation sets the location of for the next bit of appended text in the current window. It has two parameters, row and column. If a window is not locked (see Define Window) and the SMALL font is in effect the location can be outside the otherwise valid addresses. 
                    var penLocation = new SetPenLocation(lineIndex, bytes, i + 1);
                    state.Commands.Add(penLocation);
                    if (DebugMode)
                    {
                        debugBuilder.Append("{SetPenLocation:" + penLocation.Column + "," + penLocation.Row + "}");
                    }

                    i += 2;
                }
                else if (b == SetWindowAttributes.Id)
                {
                    if (bytes.Length - i < 5)
                    {
                        break;
                    }

                    // SetWindowAttributes Sets the window attributes of the current window. Fill Color is specified with 6 bits, 2 for each of blue, green and red. The lowest order bits are for blue, the next two for green and the highest order bits represent red. Fill Opacity is represented by two bits, they represent SOLID=0, FLASH=1, TRANSLUCENT=2, and TRANSPARENT=3. The window's Border Color is specified the same way. However, the Border Type is split into two fields. They should be combined, with border type 01 representing the low order bits, and border type 2 the high order bit. Once combined the Border Type has 6 valid values: NONE=0, RAISED=1, DEPRESSED=2, UNIFORM=3, SHADOW_LEFT=4, and SHADOW_RIGHT=5. 
                    var windowAttributes = new SetWindowAttributes(lineIndex, bytes, i + 1);
                    state.Commands.Add(windowAttributes);
                    if (DebugMode)
                    {
                        debugBuilder.Append($"{{SetWindowAttributes:Justify={windowAttributes.Justify}, PrintDirection={windowAttributes.PrintDirection}, ScrollDirection={windowAttributes.ScrollDirection}, Wordwrap={windowAttributes.Wordwrap}, DisplayEffect={windowAttributes.DisplayEffect}, EffectDirection={windowAttributes.EffectDirection}, EffectSpeed={windowAttributes.EffectSpeed}, FillColorRed={windowAttributes.FillColorRed}, FillColorGreen={windowAttributes.FillColorGreen}, FillColorBlue={windowAttributes.FillColorBlue}, FillOpacity={windowAttributes.FillOpacity}, BorderType={windowAttributes.BorderType}, BorderColorRed={windowAttributes.BorderColorRed}, BorderColorGreen={windowAttributes.BorderColorGreen}, BorderColorBlue={windowAttributes.BorderColorBlue}}}");
                    }

                    i += 4;
                }
                else if (b >= DefineWindow.IdStart && b <= DefineWindow.IdEnd)
                {
                    if (bytes.Length - i < 7)
                    {
                        break;
                    }

                    //DefineWindow0-7 creates one of the eight windows used by a caption decoder. This command should be sent periodically by a caption encoder even for pre-existing windows so that a newly tuned in caption decoder can begin displaying captions. When issued on a pre-existing window the pen style and window style can be left null, this tells the decoder not to change the current styles if they exist, and initialize both to style 1 if the window does not exist in its context
                    var defineWindow = new DefineWindow(lineIndex, bytes, i);
                    state.Commands.Add(defineWindow);
                    state.CurrentWindow = defineWindow.Id - DefineWindow.IdStart;
                    var defined = new bool[state.VisibleWindows.Length];
                    defined[state.CurrentWindow] = true;
                    if (defineWindow.Visible)
                    {
                        // redefining a hidden window as visible displays it, like DisplayWindows
                        ShowPendingText(debugBuilder, textBuilder, state, defined, lineIndex);
                    }
                    else if (state.VisibleWindows[state.CurrentWindow])
                    {
                        // redefining a displayed window as hidden hides it, like HideWindows - the
                        // caption on screen there ends now (the next one is often built in it)
                        FlushShownCaptions(debugBuilder, textBuilder, state, defined, lineIndex);
                        if (IsPendingTextVisibleIn(state, defined))
                        {
                            Flush(debugBuilder, textBuilder, state);
                        }

                        state.ErasedAtFlushCounts.Add(state.FlushedTexts.Count);
                    }

                    state.VisibleWindows[state.CurrentWindow] = defineWindow.Visible;
                    if (DebugMode)
                    {
                        debugBuilder.Append($"{{DefineWindow:AnchorId={defineWindow.AnchorId}, AnchorV={defineWindow.AnchorVertical}, AnchorH={defineWindow.AnchorHorizontal}, Id={defineWindow.Id:X2}, Columns={defineWindow.ColumnCount}, Rows={defineWindow.RowCount}, RowLock={defineWindow.RowLock}, ColumnLock={defineWindow.ColumnLock}, PenStyleId={defineWindow.PenStyleId}, Priority={defineWindow.Priority}, RelativePositioning={defineWindow.RelativePositioning}, Visible={defineWindow.Visible}, WindowStyleId={defineWindow.WindowStyleId}}}");
                    }

                    i += 6;
                }

                // Lookups
                else if (b == 0x10 && i < bytes.Length - 1)
                {
                    // EXT1: the next byte selects from C2/G2/C3/G3
                    var b2 = bytes[i + 1];
                    i++;

                    if (b2 >= 0x20 && b2 <= 0x7F)
                    {
                        // GL Group: G2: Extended Control Code Set 1
                        if (G2CharLookupTable.TryGetValue(b2, out var g2Text))
                        {
                            var text = new SetText(lineIndex, g2Text);
                            AddText(state, text);
                            if (DebugMode)
                            {
                                debugBuilder.Append($"{{SetText G2:Text={text.Content}}}");
                            }
                        }
                    }

                    // C2 (0x00-0x1F), C3 (0x80-0x9F): no defined commands in use
                    // G3 (0xA0-0xFF): icons ([CC] etc.) - skipped
                }

                else if (b == 0x18 && i < bytes.Length - 2)
                {
                    // P16: a 16-bit character - Unicode, or (Korean broadcasts, TTA) EUC-KR
                    var text = new SetText(lineIndex, DecodeP16(bytes[i + 1], bytes[i + 2]));
                    AddText(state, text);
                    if (DebugMode)
                    {
                        debugBuilder.Append($"{{SetText CL Group Unicode:Text={text.Content}}}");
                    }
                    i += 2;
                }

                else if (b == Backspace)
                {
                    // BS erases the character before the pen.
                    RemoveLastPendingChar(state);
                    if (DebugMode)
                    {
                        debugBuilder.Append("{BS}");
                    }
                }
                else if (b == FormFeed)
                {
                    // FF erases the window and moves the pen to its top left corner. On screen
                    // that ends the caption shown; a caption being built in a hidden window is
                    // thrown away.
                    if (IsCurrentWindowVisible(state))
                    {
                        var erased = new bool[state.VisibleWindows.Length];
                        erased[state.CurrentWindow] = true;
                        FlushShownCaptions(debugBuilder, textBuilder, state, erased, lineIndex);
                        Flush(debugBuilder, textBuilder, state);
                        state.ErasedAtFlushCounts.Add(state.FlushedTexts.Count);
                    }
                    else
                    {
                        RemovePendingText(state, currentRowOnly: false);
                    }

                    if (DebugMode)
                    {
                        debugBuilder.Append("{FF}");
                    }
                }
                else if (b == HorizontalCarriageReturn)
                {
                    // HCR erases the current row and moves the pen to its start - the row is
                    // rewritten, so its text so far is dropped.
                    RemovePendingText(state, currentRowOnly: true);
                    if (DebugMode)
                    {
                        debugBuilder.Append("{HCR}");
                    }
                }
                else if (b == CarriageReturn)
                {
                    // CR moves the pen to the next row. In a visible window - roll-up and paint-on
                    // captions - that finishes the line, so it becomes a cue of its own; the line
                    // stays on screen (scrolling up) until a later line or an erase ends it.
                    // Otherwise (a pop-on caption being built in a hidden window) it is a line
                    // break inside the caption.
                    if (IsCurrentWindowVisible(state))
                    {
                        var flushCount = state.FlushedTexts.Count;
                        Flush(debugBuilder, textBuilder, state);
                        if (state.FlushedTexts.Count > flushCount)
                        {
                            state.StillVisibleFlushes.Add(state.FlushedTexts.Count - 1);
                        }
                    }
                    else
                    {
                        AddText(state, new SetText(lineIndex, "\r"));
                    }

                    if (DebugMode)
                    {
                        debugBuilder.Append("{CR}");
                    }
                }
                else if (b <= 0x1F)
                {
                    // CL Group: C0: Subset of ASCII Control Codes
                    var text = new SetText(lineIndex, SingleCharLookupTable[b]);
                    AddText(state, text);
                    if (DebugMode)
                    {
                        debugBuilder.Append($"{{SetText CL Group C0:Text={text.Content}}}");
                    }
                }
                else if (b >= 0x20 && b <= 0x7F)
                {
                    // Modified version of ANSI X3.4 Printable Character Set(ASCII)
                    var text = new SetText(lineIndex, SingleCharLookupTable[b]);
                    AddText(state, text);

                    if (DebugMode)
                    {
                        debugBuilder.Append($"{{SetText ANSI:Text={text.Content}}}");
                    }
                }
                else if (b >= 0x80 && b <= 0x9f)
                {
                    // CR Group: C1: Caption Control Codes
                    var text = new SetText(lineIndex, SingleCharLookupTable[b]);
                    AddText(state, text);

                    if (DebugMode)
                    {
                        debugBuilder.Append($"{{SetText:Text CR Group C1={text.Content}}}");
                    }
                }
                else if (b >= 0xA0 && b <= 0xFF)
                {
                    // ISO 8859 - 1 Latin 1 Characters
                    var text = new SetText(lineIndex, SingleCharLookupTable[b]);
                    AddText(state, text);

                    if (DebugMode)
                    {
                        debugBuilder.Append($"{{SetText:Text ISO 8859={text.Content}}}");
                    }
                }

                i++;
            }

            if (flush)
            {
                FlushShownCaptions(debugBuilder, textBuilder, state, null, lineIndex);
                Flush(debugBuilder, textBuilder, state);
            }

            if (state.FlushedTexts.Count > 0)
            {
                state.StartLineIndex = state.FlushedTexts[0].Key; // start of the returned text
            }

            return DebugMode ? debugBuilder.ToString() : textBuilder.ToString();
        }

        /// <summary>
        /// Flushes the buffered text as one caption - several flushes in one packet (e.g. roll-up
        /// lines ended by CR) each become a caption of their own in <see cref="CommandState.FlushedTexts"/>,
        /// and are separated by a line break in the returned text.
        /// </summary>
        private static void Flush(StringBuilder debugBuilder, StringBuilder textBuilder, CommandState state)
        {
            state.PendingWindow = -1;
            if (DebugMode)
            {
                FlushText(debugBuilder, state);
                return;
            }

            var text = new StringBuilder();
            FlushText(text, state);
            if (text.Length == 0)
            {
                return;
            }

            AddFlushedText(textBuilder, state, state.StartLineIndex, text.ToString());
        }

        private static void AddFlushedText(StringBuilder textBuilder, CommandState state, int startLineIndex, string text)
        {
            state.FlushedTexts.Add(new KeyValuePair<int, string>(startLineIndex, text));
            if (textBuilder.Length > 0)
            {
                textBuilder.AppendLine();
            }

            textBuilder.Append(text);
        }

        /// <summary>
        /// A pop-on caption - text buffered in a hidden window - is displayed: it is taken out of the
        /// buffer (the next caption is usually built in another window while this one shows) and
        /// kept as on screen in that window until the window is hidden, cleared or deleted.
        /// </summary>
        private static void ShowPendingText(StringBuilder debugBuilder, StringBuilder textBuilder, CommandState state, bool[] windows, int lineIndex)
        {
            var w = state.PendingWindow;
            if (w < 0 || w >= windows.Length || w >= state.VisibleWindows.Length || !windows[w] || state.VisibleWindows[w])
            {
                return;
            }

            state.PendingWindow = -1;
            var text = new StringBuilder();
            FlushText(text, state);
            if (text.Length == 0)
            {
                return;
            }

            if (DebugMode)
            {
                debugBuilder.Append(text);
                return;
            }

            if (state.ShownCaptions.ContainsKey(w))
            {
                // a caption still marked as shown there (its window was hidden some way not
                // tracked) ends now rather than being lost
                var replaced = new bool[windows.Length];
                replaced[w] = true;
                FlushShownCaptions(debugBuilder, textBuilder, state, replaced, lineIndex);
            }

            state.ShownCaptions[w] =new CommandState.ShownCaption
            {
                WrittenLineIndex = state.StartLineIndex,
                ShownLineIndex = lineIndex,
                Text = text.ToString(),
            };
        }

        /// <summary>
        /// Flushes the pop-on captions on screen in these windows (null = all): they start when
        /// displayed. One displayed and removed in the same packet was never really on screen - SE's
        /// own MCC writer (VancDataWriter) sends the text at the start time into a hidden window and
        /// toggles + hides it at the end time - so it keeps the time it was written.
        /// </summary>
        private static void FlushShownCaptions(StringBuilder debugBuilder, StringBuilder textBuilder, CommandState state, bool[] windows, int lineIndex)
        {
            if (state.ShownCaptions.Count == 0)
            {
                return;
            }

            var flushed = new List<int>();
            foreach (var shown in state.ShownCaptions)
            {
                if (windows != null && (shown.Key >= windows.Length || !windows[shown.Key]))
                {
                    continue;
                }

                flushed.Add(shown.Key);
                if (!DebugMode)
                {
                    var caption = shown.Value;
                    var startLineIndex = caption.ShownLineIndex == lineIndex ? caption.WrittenLineIndex : caption.ShownLineIndex;
                    AddFlushedText(textBuilder, state, startLineIndex, caption.Text);
                }
            }

            foreach (var w in flushed)
            {
                state.ShownCaptions.Remove(w);
            }
        }

        private static void FlushText(StringBuilder text, CommandState state)
        {
            var commands = new List<ICea708Command>();
            var y = 0;
            var x = 0; // pen column in the current row
            var italicOn = false;
            foreach (var command in state.Commands)
            {
                if (command is SetText textCommand)
                {
                    if (string.IsNullOrEmpty(textCommand.Content))
                    {
                        continue;
                    }

                    if (textCommand.Content == "\r")
                    {
                        if (text.Length > 0)
                        {
                            text.AppendLine();
                        }

                        x = 0;
                        continue;
                    }

                    if (text.Length == 0)
                    {
                        state.StartLineIndex = textCommand.LineIndex;
                    }

                    if (italicOn && !IsItalicOn(text.ToString()))
                    {
                        text.Append("<i>");
                    }
                    else if (!italicOn && IsItalicOn(text.ToString()))
                    {
                        AppendItalicEnd(text);
                    }
                    text.Append(textCommand.Content);
                    x += textCommand.Content.Length;
                }
                else
                {
                    if (command is SetPenLocation location)
                    {
                        if (text.Length > 0 && location.Row > y)
                        {
                            text.AppendLine();
                        }
                        else if (location.Row == y && location.Column > x && text.Length > 0 && !char.IsWhiteSpace(text[text.Length - 1]))
                        {
                            // the pen skips columns on the same row - blank cells on screen, e.g.
                            // where a CEA-608 upconvert had a mid-row code ("were smelling")
                            text.Append(' ');
                        }

                        y = location.Row;
                        x = location.Column;
                        continue; // consumed - retaining it would append line breaks again on the next flush
                    }

                    if (command is SetPenAttributes attributes)
                    {
                        italicOn = attributes.Italics;
                    }
                    else if (command is Reset || (command is DefineWindow defineWindow && defineWindow.PenStyleId > 0))
                    {
                        // Both (re)apply a predefined pen style, none of which is italic - without
                        // this, a retained italic SetPenAttributes leaked into every later caption.
                        italicOn = false;
                    }

                    commands.Add(command);
                }
            }

            if (IsItalicOn(text.ToString()))
            {
                AppendItalicEnd(text);
            }

            state.Commands = commands;
        }

        /// <summary>
        /// Closes an italic run right after its last visible char, so "&lt;i&gt;yo " + "ok" becomes "&lt;i&gt;yo&lt;/i&gt; ok".
        /// </summary>
        private static void AppendItalicEnd(StringBuilder text)
        {
            var index = text.Length;
            while (index > 0 && text[index - 1] == ' ')
            {
                index--;
            }

            text.Insert(index, "</i>");
        }

        private static bool IsItalicOn(string text)
        {
            if (!text.Contains("<i>"))
            {
                return false;
            }

            return text.LastIndexOf("<i>", StringComparison.Ordinal) >
                   text.LastIndexOf("</i>", StringComparison.Ordinal);
        }

        public static byte[] EncodeText(string input)
        {
            if (_textLookupTable == null)
            {
                // Keyed by char: the lookup below is per character, so multi-char table
                // values could never match, and the string key allocated per character.
                var dic = new Dictionary<char, byte[]>();
                foreach (var kvp in SingleCharLookupTable)
                {
                    if (kvp.Value?.Length == 1 && !dic.ContainsKey(kvp.Value[0]))
                    {
                        dic.Add(kvp.Value[0], new[] { kvp.Key });
                    }
                }

                foreach (var kvp in G2CharLookupTable)
                {
                    if (kvp.Value?.Length == 1 && !dic.ContainsKey(kvp.Value[0]))
                    {
                        dic.Add(kvp.Value[0], new byte[] { 0x10, kvp.Key }); // EXT1 + G2 code
                    }
                }

                _textLookupTable = dic;
            }

            var bytes = new List<byte>();
            foreach (var ch in input)
            {
                if (_textLookupTable.TryGetValue(ch, out var b))
                {
                    bytes.AddRange(b);
                }
            }

            return bytes.ToArray();
        }
    }
}
