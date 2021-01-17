/*
 * Teletext decoding
 *
 * Based on telxcc by Petr Kutalek (src from https://github.com/debackerl/telxcc) which is inspired by
 * https://github.com/nxmirrors/dvbtools/tree/master/dvbsubs and https://sourceforge.net/projects/project-x/
 *
 * Also, a fix applied from https://github.com/CCExtractor/ccextractor/commits/master/src/lib_ccx/telxcc.c
 * Related code here: https://github.com/orryverducci/TtxFromTS
 *
 * NOTE: Converted to C# and modified by nikse.dk@gmail.com
 */

using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nikse.SubtitleEdit.Core.ContainerFormats.TransportStream
{
    public class Teletext
    {
        public enum BoolT
        {
            No = 0x00,
            Yes = 0x01,
            Undef = 0xff
        }

        public enum DataUnitT
        {
            DataUnitEbuTeletextNonSubtitle = 0x02,
            DataUnitEbuTeletextSubtitle = 0x03,
        }

        public enum TransmissionMode
        {
            TransmissionModeParallel = 0,
            TransmissionModeSerial = 1
        }

        public class Config
        {
            public bool Verbose { get; set; } // should telxcc be verbose?
            public int Page { get; set; } // teletext page containing cc we want to filter
            public int Tid { get; set; } // 13-bit packet ID for teletext stream
            public bool Colors { get; set; } // output <font...></font> tags
            public StringBuilder Log { get; set; }

            public Config()
            {
                Colors = true;
                Log = new StringBuilder();
            }

            public void LogInfo(string message)
            {
                if (Verbose)
                {
                    Log.AppendLine(message);
                }
            }

            public void LogInfoNoNewLine(string message)
            {
                if (Verbose)
                {
                    Log.Append(message);
                }
            }

            public void LogError(string message)
            {
                Log.AppendLine("ERROR: " + message);
            }
        }

        // application states -- flags for notices that should be printed only once
        public class States
        {
            public bool ProgrammeInfoProcessed { get; set; }
        }

        public class TeletextPacketPayload
        {
            public int ClockIn { get; }
            public int FramingCode { get; }
            public byte[] Address { get; } = new byte[2];
            public byte[] Data { get; } = new byte[40];

            public TeletextPacketPayload(byte[] buffer, int index)
            {
                ClockIn = buffer[index];
                FramingCode = buffer[index + 1];
                Address[0] = buffer[index + 2];
                Address[1] = buffer[index + 3];
                Buffer.BlockCopy(buffer, index + 4, Data, 0, Data.Length);
            }
        }

        public class TeletextPage
        {
            public ulong ShowTimestamp { get; set; }
            public ulong HideTimestamp { get; set; }
            public int[,] Text { get; set; } = new int[25, 40];
            public bool Tainted { get; set; }
        }

        public class PrimaryCharset
        {
            public int Current { get; set; }
            public int G0M29 { get; set; }
            public int G0X28 { get; set; }

            public PrimaryCharset()
            {
                Current = 0x00;
                G0M29 = (int)BoolT.Undef;
                G0X28 = (int)BoolT.Undef;
            }
        }

        private static readonly string[] TeletextColors =
        {
            //black,   red,       green,     yellow,    blue,      magenta,   cyan,      white
            "#000000", "#ff0000", "#00ff00", "#ffff00", "#0000ff", "#ff00ff", "#00ffff", "#ffffff"
        };

        // subtitle type pages bitmap, 2048 bits = 2048 possible pages in teletext (excl. subpages)
        private static readonly byte[] CcMap = new byte[256];

        private static States _states = new States();

        private static Config _config = new Config();

        private static PrimaryCharset _primaryCharset = new PrimaryCharset();

        // teletext transmission mode
        private static TransmissionMode _transmissionMode = TransmissionMode.TransmissionModeSerial;

        // flag indicating if incoming data should be processed or ignored
        private static bool _receivingData;

        // working teletext page buffer
        private static TeletextPage _pageBuffer = new TeletextPage();

        public static void InitializeStaticFields(int packetId, int pageNumberBcd)
        {
            _config = new Config { Page = pageNumberBcd, Tid = packetId };
            _primaryCharset = new PrimaryCharset();
            _transmissionMode = TransmissionMode.TransmissionModeSerial;
            _receivingData = false;
            _states = new States();
            _pageBuffer = new TeletextPage();
        }

        private static void RemapG0Charset(int c)
        {
            if (c != _primaryCharset.Current)
            {
                var m = TeletextTables.G0LatinNationalSubsetsMap[c];
                if (m == 0xff)
                {
                    _config.LogError($"- G0 Latin National Subset ID {c >> 3:X2}.{c & 0x7:X2} is not implemented");
                }
                else
                {
                    for (int j = 0; j < 13; j++)
                    {
                        TeletextTables.G0[(int)TeletextTables.G0CharsetsT.Latin, TeletextTables.G0LatinNationalSubsetsPositions[j]] = TeletextTables.G0LatinNationalSubsets[m].Characters[j];
                    }

                    _config.LogInfo($"- Using G0 Latin National Subset ID {c >> 3:X2}.{c & 0x7:X2} ({TeletextTables.G0LatinNationalSubsets[m].Language})");

                    _primaryCharset.Current = c;
                }
            }
        }

        // UCS-2 (16 bits) to UTF-8 (Unicode Normalization Form C (NFC)) conversion
        private static string Ucs2ToUtf8(int ch)
        {
            var r = new byte[4];
            if (ch < 0x80)
            {
                r[0] = (byte)(ch & 0x7f);
                return Encoding.UTF8.GetString(r, 0, 1);
            }

            if (ch < 0x800)
            {
                r[0] = (byte)((ch >> 6) | 0xc0);
                r[1] = (byte)((ch & 0x3f) | 0x80);
                return Encoding.UTF8.GetString(r, 0, 2);
            }

            r[0] = (byte)((ch >> 12) | 0xe0);
            r[1] = (byte)(((ch >> 6) & 0x3f) | 0x80);
            r[2] = (byte)((ch & 0x3f) | 0x80);
            return Encoding.UTF8.GetString(r, 0, 3);
        }

        // check parity and translate any reasonable teletext character into ucs2
        private static int TelxToUcs2(byte c)
        {
            if (TeletextHamming.Parity8[c] == 0)
            {
                _config.LogError($"! Unrecoverable data error; PARITY({c:X2})");
                return 0x20;
            }

            var r = c & 0x7f;
            if (r >= 0x20)
            {
                r = TeletextTables.G0[(int)TeletextTables.G0CharsetsT.Latin, r - 0x20];
            }

            return r;
        }

        // extracts magazine number from teletext page
        private static int Magazine(int p)
        {
            return (p >> 8) & 0xf;
        }

        // extracts page number from teletext page
        public static int Page(int p)
        {
            return p & 0xff;
        }

        static void ProcessPage(TeletextPage page, TeletextRunSettings teletextRunSettings, int pageNumber)
        {
#if DEBUG
            for (int row = 1; row < 25; row++)
            {
                _config.LogInfoNoNewLine($"# DEBUG[{row}]: ");
                for (int col = 0; col < 40; col++)
                {
                    _config.LogInfo($"{(page.Text[row, col]):X2} ");
                }
                _config.LogInfo(string.Empty);
            }
            _config.LogInfo(string.Empty);
#endif

            // optimization: slicing column by column -- higher probability we could find boxed area start mark sooner
            bool pageIsEmpty = true;
            for (var col = 0; col < 40; col++)
            {
                for (var row = 1; row < 25; row++)
                {
                    if (page.Text[row, col] == 0x0b)
                    {
                        pageIsEmpty = false;
                        break;
                    }
                }
            }
            if (pageIsEmpty)
            {
                return;
            }

            var paragraph = new Paragraph();
            var usedLines = new List<int>();
            var sb = new StringBuilder();

            if (page.ShowTimestamp > page.HideTimestamp)
            {
                page.HideTimestamp = page.ShowTimestamp;
            }

            paragraph.StartTime = new TimeCode(page.ShowTimestamp);
            paragraph.EndTime = new TimeCode(page.HideTimestamp);

            // process data
            for (var row = 1; row < 25; row++)
            {
                // anchors for string trimming purpose
                var colStart = 40;
                var colStop = 40;
                bool boxOpen = false;
                for (var col = 0; col < 40; col++)
                {
                    // replace all 0/B and 0/A characters with 0/20, as specified in ETS 300 706:
                    // Unless operating in "Hold Mosaics" mode, each character space occupied by a
                    // spacing attribute is displayed as a SPACE
                    if (page.Text[row, col] == 0xb) // open the box
                    {
                        if (colStart == 40)
                        {
                            colStart = col;
                        }
                        else
                        {
                            page.Text[row, col] = 0x20;
                        }
                        boxOpen = true;
                    }
                    else if (page.Text[row, col] == 0xa) // close the box
                    {
                        page.Text[row, col] = 0x20;
                        boxOpen = false;
                    }
                    // characters between 0xA and 0xB shouldn't be displayed
                    // page->text[row][col] > 0x20 added to preserve color information
                    else if (!boxOpen && colStart < 40 && page.Text[row, col] > 0x20)
                    {
                        page.Text[row, col] = 0x20;
                    }
                }

                // line is empty
                if (colStart > 39)
                {
                    continue;
                }

                for (var col = colStart + 1; col <= 39; col++)
                {
                    if (page.Text[row, col] > 0x20)
                    {
                        if (colStop > 39)
                        {
                            colStart = col;
                        }
                        colStop = col;
                    }
                    if (page.Text[row, col] == 0xa)
                    {
                        break;
                    }
                }
                // line is empty
                if (colStop > 39)
                {
                    continue;
                }

                // ETS 300 706, chapter 12.2: Alpha White ("Set-After") - Start-of-row default condition.
                // used for color changes _before_ start box mark
                // white is default as stated in ETS 300 706, chapter 12.2
                // black(0), red(1), green(2), yellow(3), blue(4), magenta(5), cyan(6), white(7)
                var foregroundColor = 0x7;
                bool fontTagOpened = false;

                for (var col = 0; col <= colStop; col++)
                {
                    // v is just a shortcut
                    var v = page.Text[row, col];

                    if (col < colStart)
                    {
                        if (v <= 0x7)
                        {
                            foregroundColor = v;
                        }
                    }

                    if (col == colStart)
                    {
                        if (foregroundColor != 0x7 && _config.Colors)
                        {
                            sb.Append($"<font color=\"{TeletextColors[foregroundColor]}\">");
                            fontTagOpened = true;
                        }
                    }

                    if (col >= colStart)
                    {
                        if (v <= 0x7)
                        {
                            // ETS 300 706, chapter 12.2: Unless operating in "Hold Mosaics" mode,
                            // each character space occupied by a spacing attribute is displayed as a SPACE.
                            if (_config.Colors)
                            {
                                if (fontTagOpened)
                                {
                                    sb.Append("</font> ");
                                    fontTagOpened = false;
                                }

                                // black is considered as white for telxcc purpose
                                // telxcc writes <font/> tags only when needed
                                if (v > 0x0 && v < 0x7)
                                {
                                    sb.Append($"<font color=\"{TeletextColors[v]}\">");
                                    fontTagOpened = true;
                                }
                            }
                            else
                            {
                                v = 0x20;
                            }
                        }

                        if (v >= 0x20)
                        {
                            sb.Append(Ucs2ToUtf8(v));
                        }
                    }
                }

                // no tag will left opened!
                if (_config.Colors && fontTagOpened)
                {
                    sb.Append("</font>");
                }

                // line delimiter
                sb.Append(Environment.NewLine);
                usedLines.Add(row);
            }
            sb.AppendLine();
            var topAlign = usedLines.Count > 0 && usedLines.All(p => p < 6);
            paragraph.Text = (topAlign ? "{\\an8}" : "") + sb.ToString().TrimEnd();
            if (!teletextRunSettings.PageNumberAndParagraph.ContainsKey(pageNumber))
            {
                teletextRunSettings.PageNumberAndParagraph.Add(pageNumber, paragraph);
            }
            else
            {
                _config.LogError("New pageId in page: " + pageNumber);
            }
        }

        public static int GetPageNumber(TeletextPacketPayload packet)
        {
            var address = (TeletextHamming.UnHamming84(packet.Address[1]) << 4) | TeletextHamming.UnHamming84(packet.Address[0]);
            var m = address & 0x7;
            if (m == 0)
            {
                m = 8;
            }
            var y = (address >> 3) & 0x1f;
            if (y == 0)
            {
                var i = (TeletextHamming.UnHamming84(packet.Data[1]) << 4) | TeletextHamming.UnHamming84(packet.Data[0]);
                var flagSubtitle = (TeletextHamming.UnHamming84(packet.Data[5]) & 0x08) >> 3;
                if (flagSubtitle == 1 && i < 0xff)
                {
                    var bcdPage = (m << 8) | (TeletextHamming.UnHamming84(packet.Data[1]) << 4) | TeletextHamming.UnHamming84(packet.Data[0]);
                    return BcdToDec((ulong)bcdPage);
                }
            }
            return -1;
        }

        public static int BcdToDec(ulong bcd)
        {
            ulong mask = 0x000f;
            ulong pwr = 1;
            ulong i = bcd & mask;
            bcd = bcd >> 4;
            while (bcd > 0)
            {
                pwr *= 10;
                i += (bcd & mask) * pwr;
                bcd = bcd >> 4;
            }
            return (int)i;
        }

        public static int DecToBec(int dec)
        {
            return ((dec / 100) << 8) | ((dec / 10 % 10) << 4) | (dec % 10);
        }

        public static void ProcessTelxPacket(DataUnitT dataUnitId, TeletextPacketPayload packet, ulong timestamp, TeletextRunSettings teletextRunSettings, int targetPageNumberBcd, int targetPageNumberDec)
        {
            // variable names conform to ETS 300 706, chapter 7.1.2
            var address = (TeletextHamming.UnHamming84(packet.Address[1]) << 4) | TeletextHamming.UnHamming84(packet.Address[0]);
            var m = address & 0x7;
            if (m == 0)
            {
                m = 8;
            }

            var y = (address >> 3) & 0x1f;
            var designationCode = y > 25 ? TeletextHamming.UnHamming84(packet.Data[0]) : 0x00;

            if (y == 0) // PAGE HEADER
            {
                // CC map
                var i = (TeletextHamming.UnHamming84(packet.Data[1]) << 4) | TeletextHamming.UnHamming84(packet.Data[0]);
                var flagSubtitle = (TeletextHamming.UnHamming84(packet.Data[5]) & 0x08) >> 3;

                CcMap[i] |= (byte)(flagSubtitle << (m - 1));

                // Page number and control bits
                var pageNumber = (m << 8) | (TeletextHamming.UnHamming84(packet.Data[1]) << 4) | TeletextHamming.UnHamming84(packet.Data[0]);
                var charset = ((TeletextHamming.UnHamming84(packet.Data[7]) & 0x08) | (TeletextHamming.UnHamming84(packet.Data[7]) & 0x04) | (TeletextHamming.UnHamming84(packet.Data[7]) & 0x02)) >> 1;
                //uint8_t flag_suppress_header = unham_8_4(packet.data[6]) & 0x01;
                //uint8_t flag_inhibit_display = (unham_8_4(packet.data[6]) & 0x08) >> 3;

                // ETS 300 706, chapter 9.3.1.3:
                // When set to '1' the service is designated to be in Serial mode and the transmission of a page is terminated
                // by the next page header with a different page number.
                // When set to '0' the service is designated to be in Parallel mode and the transmission of a page is terminated
                // by the next page header with a different page number but the same magazine number.
                // The same setting shall be used for all page headers in the service.
                // ETS 300 706, chapter 7.2.1: Page is terminated by and excludes the next page header packet
                // having the same magazine address in parallel transmission mode, or any magazine address in serial transmission mode.
                _transmissionMode = (TransmissionMode)(TeletextHamming.UnHamming84(packet.Data[7]) & 0x01);

                // FIXME: Well, this is not ETS 300 706 kosher, however we are interested in DATA_UNIT_EBU_TELETEXT_SUBTITLE only
                if (_transmissionMode == TransmissionMode.TransmissionModeParallel && dataUnitId != DataUnitT.DataUnitEbuTeletextSubtitle)
                {
                    return;
                }

                if (_receivingData &&
                    (_transmissionMode == TransmissionMode.TransmissionModeSerial && Page(pageNumber) != Page(targetPageNumberBcd) ||
                    _transmissionMode == TransmissionMode.TransmissionModeParallel && Page(pageNumber) != Page(targetPageNumberBcd) && m == Magazine(targetPageNumberBcd))
                   )
                {
                    _receivingData = false;
                    return;
                }

                // Page transmission is terminated, however now we are waiting for our new page
                if (targetPageNumberBcd != pageNumber)
                {
                    return;
                }

                // Now we have the beginning of page transmission; if there is page_buffer pending, process it
                if (_pageBuffer.Tainted)
                {
                    // it would be nice, if subtitle hides on previous video frame, so we contract 40 ms (1 frame @25 fps)
                    _pageBuffer.HideTimestamp = timestamp - 40;
                    ProcessPage(_pageBuffer, teletextRunSettings, targetPageNumberDec);
                }

                _pageBuffer.ShowTimestamp = timestamp;
                _pageBuffer.HideTimestamp = 0;
                _pageBuffer.Text = new int[25, 40];
                _pageBuffer.Tainted = false;
                _receivingData = true;
                _primaryCharset.G0X28 = (int)BoolT.Undef;

                var c = _primaryCharset.G0M29 != (int)BoolT.Undef ? _primaryCharset.G0M29 : charset;
                RemapG0Charset(c);

                /*
                // I know -- not needed; in subtitles we will never need disturbing teletext page status bar
                // displaying tv station name, current time etc.
                if (flag_suppress_header == NO) {
                    for (uint8_t i = 14; i < 40; i++) page_buffer.text[y,i] = telx_to_ucs2(packet.data[i]);
                    //page_buffer.tainted = YES;
                }
                */
            }
            else if (m == Magazine(targetPageNumberBcd) && y >= 1 && y <= 23 && _receivingData)
            {
                // ETS 300 706, chapter 9.4.1: Packets X/26 at presentation Levels 1.5, 2.5, 3.5 are used for addressing
                // a character location and overwriting the existing character defined on the Level 1 page
                // ETS 300 706, annex B.2.2: Packets with Y = 26 shall be transmitted before any packets with Y = 1 to Y = 25;
                // so page_buffer.text[y,i] may already contain any character received
                // in frame number 26, skip original G0 character
                for (var i = 0; i < 40; i++)
                {
                    if (_pageBuffer.Text[y, i] == 0x00)
                    {
                        _pageBuffer.Text[y, i] = TelxToUcs2(packet.Data[i]);
                    }
                }

                _pageBuffer.Tainted = true;
            }
            else if (m == Magazine(targetPageNumberBcd) && y == 26 && _receivingData)
            {
                // ETS 300 706, chapter 12.3.2: X/26 definition
                var x26Row = 0;

                var triplets = new uint[13];
                var j = 0;
                for (var i = 1; i < 40; i += 3, j++)
                {
                    triplets[j] = TeletextHamming.UnHamming2418((packet.Data[i + 2] << 16) | (packet.Data[i + 1] << 8) | packet.Data[i]);
                }

                for (var j2 = 0; j2 < 13; j2++)
                {
                    if (triplets[j2] == 0xffffffff)
                    {
                        // invalid data (HAM24/18 uncorrectable error detected), skip group
                        _config.LogError($"! Unrecoverable data error; UNHAM24/18()={triplets[j2]}");
                        continue;
                    }

                    var data = (triplets[j2] & 0x3f800) >> 11;
                    var mode = (triplets[j2] & 0x7c0) >> 6;
                    var address2 = triplets[j2] & 0x3f;
                    var rowAddressGroup = address2 >= 40 && address2 <= 63;

                    // ETS 300 706, chapter 12.3.1, table 27: set active position
                    if (mode == 0x04 && rowAddressGroup)
                    {
                        x26Row = (int)(address2 - 40);
                        if (x26Row == 0)
                        {
                            x26Row = 24;
                        }
                    }

                    // ETS 300 706, chapter 12.3.1, table 27: termination marker
                    if (mode >= 0x11 && mode <= 0x1f && rowAddressGroup)
                    {
                        break;
                    }

                    // ETS 300 706, chapter 12.3.1, table 27: character from G2 set
                    int x26Col;
                    if (mode == 0x0f && !rowAddressGroup)
                    {
                        x26Col = (int)address2;
                        if (data > 31)
                        {
                            _pageBuffer.Text[x26Row, x26Col] = TeletextTables.G2[0, data - 0x20];
                        }
                    }

                    // ETS 300 706, chapter 12.3.1, table 27: G0 character with diacritical mark
                    if (mode >= 0x11 && mode <= 0x1f && !rowAddressGroup)
                    {
                        x26Col = (int)address2;
                        if (data >= 65 && data <= 90) // A-Z
                        {
                            _pageBuffer.Text[x26Row, x26Col] = TeletextTables.G2Accents[mode - 0x11, data - 65];
                        }
                        else if (data >= 97 && data <= 122) // a-z
                        {
                            _pageBuffer.Text[x26Row, x26Col] = TeletextTables.G2Accents[mode - 0x11, data - 71];
                        }
                        else // other
                        {
                            _pageBuffer.Text[x26Row, x26Col] = TelxToUcs2((byte)data);
                        }
                    }
                }
            }
            else if (m == Magazine(targetPageNumberBcd) && y == 28 && _receivingData)
            {
                // TODO:
                //   ETS 300 706, chapter 9.4.7: Packet X/28/4
                //   Where packets 28/0 and 28/4 are both transmitted as part of a page, packet 28/0 takes precedence over 28/4 for all but the color map entry coding.
                if (designationCode == 0 || designationCode == 4)
                {
                    // ETS 300 706, chapter 9.4.2: Packet X/28/0 Format 1
                    // ETS 300 706, chapter 9.4.7: Packet X/28/4
                    uint triplet0 = TeletextHamming.UnHamming2418((packet.Data[3] << 16) | (packet.Data[2] << 8) | packet.Data[1]);

                    if (triplet0 == 0xffffffff)
                    {
                        // invalid data (HAM24/18 uncorrectable error detected), skip group
                        _config.LogError($"! Unrecoverable data error; UNHAM24/18()={triplet0}");
                    }
                    else
                    {
                        // ETS 300 706, chapter 9.4.2: Packet X/28/0 Format 1 only
                        if ((triplet0 & 0x0f) == 0x00)
                        {
                            _primaryCharset.G0X28 = (int)((triplet0 & 0x3f80) >> 7);
                            RemapG0Charset(_primaryCharset.G0X28);
                        }
                    }
                }
            }
            else if (m == Magazine(targetPageNumberBcd) && y == 29)
            {
                // TODO:
                //   ETS 300 706, chapter 9.5.1 Packet M/29/0
                //   Where M/29/0 and M/29/4 are transmitted for the same magazine, M/29/0 takes precedence over M/29/4.
                if (designationCode == 0 || designationCode == 4)
                {
                    // ETS 300 706, chapter 9.5.1: Packet M/29/0
                    // ETS 300 706, chapter 9.5.3: Packet M/29/4
                    uint triplet0 = TeletextHamming.UnHamming2418((packet.Data[3] << 16) | (packet.Data[2] << 8) | packet.Data[1]);

                    if (triplet0 == 0xffffffff)
                    {
                        // invalid data (HAM24/18 uncorrectable error detected), skip group
                        _config.LogError($"! Unrecoverable data error; UNHAM24/18()={triplet0}");
                    }
                    else
                    {
                        // ETS 300 706, table 11: Coding of Packet M/29/0
                        // ETS 300 706, table 13: Coding of Packet M/29/4
                        if ((triplet0 & 0xff) == 0x00)
                        {
                            _primaryCharset.G0M29 = (int)((triplet0 & 0x3f80) >> 7);
                            // X/28 takes precedence over M/29
                            if (_primaryCharset.G0X28 == (int)BoolT.Undef)
                            {
                                RemapG0Charset(_primaryCharset.G0M29);
                            }
                        }
                    }
                }
            }
            else if (m == 8 && y == 30)
            {
                // ETS 300 706, chapter 9.8: Broadcast Service Data Packets
                if (!_states.ProgrammeInfoProcessed)
                {
                    // ETS 300 706, chapter 9.8.1: Packet 8/30 Format 1
                    if (TeletextHamming.UnHamming84(packet.Data[0]) < 2)
                    {
                        _config.LogInfoNoNewLine("- Programme Identification Data = ");
                        for (var i = 20; i < 40; i++)
                        {
                            var c = TelxToUcs2(packet.Data[i]);
                            // strip any control codes from PID, eg. TVP station
                            if (c < 0x20)
                            {
                                continue;
                            }
                            _config.LogInfoNoNewLine(Ucs2ToUtf8(c));
                        }
                        _config.LogInfo(string.Empty);

                        // OMG! ETS 300 706 stores timestamp in 7 bytes in Modified Julian Day in BCD format + HH:MM:SS in BCD format
                        // + timezone as 5-bit count of half-hours from GMT with 1-bit sign
                        // In addition all decimals are incremented by 1 before transmission.
                        long t = 0;
                        // 1st step: BCD to Modified Julian Day
                        t += (packet.Data[10] & 0x0f) * 10000;
                        t += ((packet.Data[11] & 0xf0) >> 4) * 1000;
                        t += (packet.Data[11] & 0x0f) * 100;
                        t += ((packet.Data[12] & 0xf0) >> 4) * 10;
                        t += packet.Data[12] & 0x0f;
                        t -= 11111;
                        // 2nd step: conversion Modified Julian Day to unix timestamp
                        t = (t - 40587) * 86400;
                        // 3rd step: add time
                        t += 3600 * (((packet.Data[13] & 0xf0) >> 4) * 10 + (packet.Data[13] & 0x0f));
                        t += 60 * (((packet.Data[14] & 0xf0) >> 4) * 10 + (packet.Data[14] & 0x0f));
                        t += ((packet.Data[15] & 0xf0) >> 4) * 10 + (packet.Data[15] & 0x0f);
                        t -= 40271;
                        // 4th step: conversion to time_t
                        var span = TimeSpan.FromTicks(t * TimeSpan.TicksPerSecond);
                        var t2 = new DateTime(1970, 1, 1).Add(span);
                        var localTime = TimeZoneInfo.ConvertTimeFromUtc(t2, TimeZoneInfo.Local);

                        _config.LogInfo($"- Programme Timestamp (UTC) = {localTime.ToLongDateString()} {localTime.ToLongTimeString()}");
                        _config.LogInfo($"- Transmission mode = {(_transmissionMode == TransmissionMode.TransmissionModeSerial ? "serial" : "parallel")}");
                        _states.ProgrammeInfoProcessed = true;
                    }
                }
            }
        }

        public static Dictionary<int, Paragraph> ProcessTelxPacketPendingLeftovers(TeletextRunSettings teletextRunSettings, int pageNumberDec)
        {
            if (_pageBuffer.Tainted)
            {
                // this time we do not subtract any frames, there will be no more frames
                _pageBuffer.HideTimestamp = teletextRunSettings.GetLastTimestamp(pageNumberDec);
                ProcessPage(_pageBuffer, teletextRunSettings, pageNumberDec);
                return teletextRunSettings.PageNumberAndParagraph;
            }

            return new Dictionary<int, Paragraph>();
        }
    }
}
