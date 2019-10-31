using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Nikse.SubtitleEdit.Core.TransportStream
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
            DataUnitEbuTeletextInverted = 0x0c,
            DataUnitVps = 0xc3,
            DataUnitClosedCaptions = 0xc5
        }

        public enum TransmissionMode
        {
            TransmissionModeParallel = 0,
            TransmissionModeSerial = 1
        }

        public class Config
        {
            public string InputName { get; set; }
            public string OutputName { get; set; }
            public bool Verbose { get; set; } // should telxcc be verbose?
            public int Page { get; set; } // teletext page containing cc we want to filter
            public int Tid { get; set; } // 13-bit packet ID for teletext stream
            public double Offset { get; set; } // time offset in seconds
            public bool Colors { get; set; } // output <font...></font> tags
            public bool Bom { get; set; } // print UTF-8 BOM characters at the beginning of output
            public bool NonEmpty { get; set; } // produce at least one (dummy) frame
            public ulong UtcRefValue { get; set; } // UTC referential value
            public bool SeMode { get; set; } // FIXME: move SE_MODE to output module
            public bool M2Ts { get; set; } // consider input stream is af s M2TS, instead of TS
            public int Count { get; set; } // consider input stream is af s M2TS, instead of TS

            public Config()
            {
                Bom = true;
            }
        }

        // application states -- flags for notices that should be printed only once
        public class States
        {
            public bool ProgrammeInfoProcessed { get; set; }
            public bool PtsInitialized { get; set; }
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

        // entities, used in colour mode, to replace unsafe HTML tag chars
        private static readonly Dictionary<char, string> Entities = new Dictionary<char, string>
        {
            //{ '<', "&lt;" },
            //{ '>', "&gt;" },
            //{ '&', "&amp;" }
        };

        public static readonly StringBuilder Fout = new StringBuilder();

        public static States states = new States();

        public static readonly Config config = new Config();

        private static readonly PrimaryCharset primaryCharset = new PrimaryCharset();

        // global TS PCR value
        public static ulong _globalTimestamp;

        // last timestamp computed
        public static ulong _lastTimestamp;

        private static long _delta;
        private static long _t0;

        private static BoolT _usingPts = BoolT.Undef;

        // SRT frames produced
        private static int _framesProduced;

        // teletext transmission mode
        private static TransmissionMode _transmissionMode = TransmissionMode.TransmissionModeSerial;

        // flag indicating if incoming data should be processed or ignored
        private static bool _receivingData;

        // working teletext page buffer
        private static readonly TeletextPage PageBuffer = new TeletextPage();

        private static void RemapG0Charset(int c)
        {
            if (c != primaryCharset.Current)
            {
                var m = TeletextTables.G0LatinNationalSubsetsMap[c];
                if (m == 0xff)
                {
                    Console.WriteLine($"- G0 Latin National Subset ID {(c >> 3):X2}.{(c & 0x7):X2} is not implemented");
                }
                else
                {
                    for (int j = 0; j < 13; j++) TeletextTables.G0[(int)TeletextTables.G0CharsetsT.Latin, TeletextTables.G0LatinNationalSubsetsPositions[j]] = TeletextTables.G0LatinNationalSubsets[m].Characters[j];
                    if (config.Verbose) Console.WriteLine($"- Using G0 Latin National Subset ID {c >> 3:X2}.{c & 0x7:X2} ({TeletextTables.G0LatinNationalSubsets[m].Language})");
                    primaryCharset.Current = c;
                }
            }
        }

        private static string TimestampToSrtTime(ulong timestamp)
        {
            var p = timestamp;
            var h = p / 3600000;
            var m = p / 60000 - 60 * h;
            var s = p / 1000 - 3600 * h - 60 * m;
            var u = p - 3600000 * h - 60000 * m - 1000 * s;
            return $"{h:00}:{m:00}:{s:00},{u:000}";
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
                if (config.Verbose) Console.WriteLine($"! Unrecoverable data error; PARITY({c:X2})");
                return 0x20;
            }

            var r = c & 0x7f;
            if (r >= 0x20) r = TeletextTables.G0[(int)TeletextTables.G0CharsetsT.Latin, r - 0x20];
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

        public static void ProcessPesPacket(byte[] buffer, int size)
        {
            if (size < 6) return;

            // Packetized Elementary Stream (PES) 32-bit start code
            ulong pesPrefix = (ulong)((buffer[0] << 16) | (buffer[1] << 8) | buffer[2]);
            var pesStreamId = buffer[3];

            // check for PES header
            if (pesPrefix != 0x000001) return;

            // stream_id is not "Private Stream 1" (0xbd)
            if (pesStreamId != 0xbd) return;

            // PES packet length
            // ETSI EN 301 775 V1.2.1 (2003-05) chapter 4.3: (N x 184) - 6 + 6 B header
            var pesPacketLength = 6 + ((buffer[4] << 8) | buffer[5]);
            // Can be zero. If the "PES packet length" is set to zero, the PES packet can be of any length.
            // A value of zero for the PES packet length can be used only when the PES packet payload is a video elementary stream.
            if (pesPacketLength == 6) return;

            // truncate incomplete PES packets
            if (pesPacketLength > size) pesPacketLength = size;

            bool optionalPesHeaderIncluded = false;
            var optionalPesHeaderLength = 0;
            // optional PES header marker bits (10.. ....)
            if ((buffer[6] & 0xc0) == 0x80)
            {
                optionalPesHeaderIncluded = true;
                optionalPesHeaderLength = buffer[8];
            }

            // should we use PTS or PCR?
            if (_usingPts == BoolT.Undef)
            {
                if (optionalPesHeaderIncluded && (buffer[7] & 0x80) > 0)
                {
                    _usingPts = BoolT.Yes;
                    if (config.Verbose) Console.WriteLine("- PID 0xbd PTS available");
                }
                else
                {
                    _usingPts = BoolT.No;
                    if (config.Verbose) Console.WriteLine(" - PID 0xbd PTS unavailable, using TS PCR");
                }
            }

            ulong t;
            // If there is no PTS available, use global PCR
            if (_usingPts == BoolT.No)
            {
                t = _globalTimestamp;
            }
            else
            {
                // PTS is 33 bits wide, however, timestamp in ms fits into 32 bits nicely (PTS/90)
                // presentation and decoder timestamps use the 90 KHz clock, hence PTS/90 = [ms]
                // __MUST__ assign value to uint64_t and __THEN__ rotate left by 29 bits
                // << is defined for signed int (as in "C" spec.) and overflow occures
                long pts = buffer[9] & 0x0e;
                pts <<= 29;
                pts |= (uint)(buffer[10] << 22);
                pts |= (uint)((buffer[11] & 0xfe) << 14);
                pts |= (uint)(buffer[12] << 7);
                pts |= (uint)((buffer[13] & 0xfe) >> 1);
                t = (ulong)pts / 90;
            }

            if (!states.PtsInitialized)
            {
                _delta = (long)(1000 * config.Offset + 1000 * config.UtcRefValue - t);
                states.PtsInitialized = true;

                if (_usingPts == BoolT.No && _globalTimestamp == 0)
                {
                    // We are using global PCR, nevertheless we still have not received valid PCR timestamp yet
                    states.PtsInitialized = false;
                }
            }
            if (t < (ulong)_t0) _delta = (long)_lastTimestamp;
            _lastTimestamp = t + (ulong)_delta;
            _t0 = (long)t;

            // skip optional PES header and process each 46 bytes long teletext packet
            var i = 7;
            if (optionalPesHeaderIncluded) i += 3 + optionalPesHeaderLength;
            while (i <= pesPacketLength - 6)
            {
                var dataUnitId = buffer[i++];
                var dataUnitLen = buffer[i++];

                if (dataUnitId == (int)DataUnitT.DataUnitEbuTeletextNonSubtitle || dataUnitId == (int)DataUnitT.DataUnitEbuTeletextSubtitle)
                {
                    // teletext payload has always size 44 bytes
                    if (dataUnitLen == 44)
                    {
                        // reverse endianess (via lookup table), ETS 300 706, chapter 7.1
                        for (var j = 0; j < dataUnitLen; j++) buffer[i + j] = TeletextHamming.Reverse8[buffer[i + j]];

                        // FIXME: This explicit type conversion could be a problem some day -- do not need to be platform independant
                        ProcessTelxPacket((DataUnitT)dataUnitId, new TeletextPacketPayload(buffer, i), _lastTimestamp, null, new StringBuilder());
                    }
                }

                i += dataUnitLen;
            }
        }


        static void ProcessPage(TeletextPage page)
        {
            //#if DEBUG
            //            for (int row = 1; row < 25; row++)
            //            {
            //                fout.Append($"# DEBUG[{row}]: ");
            //                for (int col = 0; col < 40; col++) fout.Append($"{(page.text[row, col]):X2} ");
            //                fout.AppendLine();
            //            }
            //            fout.AppendLine();
            //#endif

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
            if (pageIsEmpty) return;

            if (page.ShowTimestamp > page.HideTimestamp)
            {
                page.HideTimestamp = page.ShowTimestamp;
            }

            if (config.SeMode)
            {
                ++_framesProduced;
                Fout.Append($"{page.ShowTimestamp / 1000.0}|");
            }
            else
            {
                var timeCodeShow = TimestampToSrtTime(page.ShowTimestamp);
                var timeCodeHide = TimestampToSrtTime(page.HideTimestamp);
                Fout.AppendLine($"{++_framesProduced}{Environment.NewLine}{timeCodeShow} --> {timeCodeHide}");
            }

            // process data
            for (var row = 1; row < 25; row++)
            {
                // anchors for string trimming purpose
                var colStart = 40;
                var colStop = 40;

                for (var col = 39; col >= 0; col--)
                {
                    if (page.Text[row, col] == 0xb)
                    {
                        colStart = col;
                        break;
                    }
                }
                // line is empty
                if (colStart > 39) continue;

                for (var col = colStart + 1; col <= 39; col++)
                {
                    if (page.Text[row, col] > 0x20)
                    {
                        if (colStop > 39) colStart = col;
                        colStop = col;
                    }
                    if (page.Text[row, col] == 0xa) break;
                }
                // line is empty
                if (colStop > 39) continue;

                // ETS 300 706, chapter 12.2: Alpha White ("Set-After") - Start-of-row default condition.
                // used for colour changes _before_ start box mark
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
                        if (v <= 0x7) foregroundColor = v;
                    }

                    if (col == colStart)
                    {
                        if ((foregroundColor != 0x7) && (config.Colors))
                        {
                            Fout.Append($"<font color=\"{TeletextColors[foregroundColor]}\">");
                            fontTagOpened = true;
                        }
                    }

                    if (col >= colStart)
                    {
                        if (v <= 0x7)
                        {
                            // ETS 300 706, chapter 12.2: Unless operating in "Hold Mosaics" mode,
                            // each character space occupied by a spacing attribute is displayed as a SPACE.
                            if (config.Colors)
                            {
                                if (fontTagOpened)
                                {
                                    Fout.Append("</font> ");
                                    fontTagOpened = false;
                                }

                                // black is considered as white for telxcc purpose
                                // telxcc writes <font/> tags only when needed
                                if (v > 0x0 && v < 0x7)
                                {
                                    Fout.Append($"<font color=\"{TeletextColors[v]}\">");
                                    fontTagOpened = true;
                                }
                            }
                            else v = 0x20;
                        }

                        if (v >= 0x20)
                        {
                            // translate some chars into entities, if in colour mode
                            if (config.Colors)
                            {
                                if (Entities.ContainsKey(Convert.ToChar(v)))
                                {
                                    Fout.Append(Entities[Convert.ToChar(v)]);
                                    // v < 0x20 won't be printed in next block
                                    v = 0;
                                    break;
                                }
                            }
                        }

                        if (v >= 0x20)
                        {
                            Fout.Append(Ucs2ToUtf8(v));
                        }
                    }
                }

                // no tag will left opened!
                if (config.Colors && fontTagOpened)
                {
                    Fout.Append("</font>");
                    fontTagOpened = false;
                }

                // line delimiter
                Fout.Append(config.SeMode ? " " : Environment.NewLine);
            }
            Fout.AppendLine();
        }

        public static int GetPageNumber(TeletextPacketPayload packet)
        {
            var address = (TeletextHamming.UnHamming84(packet.Address[1]) << 4) | TeletextHamming.UnHamming84(packet.Address[0]);
            var m = address & 0x7;
            if (m == 0) m = 8;
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

            ulong i = (ulong)(bcd & mask);
            bcd = (bcd >> 4);
            while (bcd > 0)
            {
                pwr *= 10;
                i += (bcd & mask) * pwr;
                bcd = (bcd >> 4);
            }
            return (int)i;
        }

        public static void ProcessTelxPacket(DataUnitT dataUnitId, TeletextPacketPayload packet, ulong timestamp, TeletextRunSettings teletextRunSettings, StringBuilder sb)
        {
            // variable names conform to ETS 300 706, chapter 7.1.2
            var address = (TeletextHamming.UnHamming84(packet.Address[1]) << 4) | TeletextHamming.UnHamming84(packet.Address[0]);
            sb.AppendLine($"address: {address}");

            var m = address & 0x7;
            if (m == 0) m = 8;
            sb.AppendLine($"Magezine: {m}");
            var y = (address >> 3) & 0x1f;
            sb.AppendLine($"Y: {y}");
            var designationCode = y > 25 ? TeletextHamming.UnHamming84(packet.Data[0]) : 0x00;
            sb.AppendLine($"designationCode: {designationCode}");

            if (y == 0) // PAGE HEADER  
            {
                // CC map
                var i = (TeletextHamming.UnHamming84(packet.Data[1]) << 4) | TeletextHamming.UnHamming84(packet.Data[0]);
                var flagSubtitle = (TeletextHamming.UnHamming84(packet.Data[5]) & 0x08) >> 3;

                var bcdPage = (m << 8) | (TeletextHamming.UnHamming84(packet.Data[1]) << 4) | TeletextHamming.UnHamming84(packet.Data[0]);
                if (flagSubtitle == 1 && i < 0xff)
                {
                    var pageNumberInt = BcdToDec((ulong)bcdPage);
                    sb.AppendLine("page: " + pageNumberInt);
                    if (pageNumberInt == 398)
                    {
                        teletextRunSettings.PageNumber = pageNumberInt;
                        teletextRunSettings.PageNumberBcd = bcdPage;
                        if (!teletextRunSettings.PageNumbersInt.Contains(pageNumberInt))
                        {
                            teletextRunSettings.PageNumbersInt.Add(pageNumberInt);
                        }
                        if (!teletextRunSettings.PageNumbersBcd.Contains(bcdPage))
                        {
                            teletextRunSettings.PageNumbersBcd.Add(bcdPage);
                        }
                    }
                }
                else
                {
                    teletextRunSettings.PageNumber = -1;
                }

                
                CcMap[i] |= (byte)(flagSubtitle << (m - 1));

                if (config.Page == 0 && flagSubtitle == (int)BoolT.Yes && i < 0xff)
                {
                    config.Page = (m << 8) | (TeletextHamming.UnHamming84(packet.Data[1]) << 4) | TeletextHamming.UnHamming84(packet.Data[0]);
                    Console.WriteLine($"- No teletext page specified, first received suitable page is {config.Page}, not guaranteed");
                }

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
                sb.AppendLine($"_transmissionMode: {_transmissionMode}");
                teletextRunSettings.TransmissionMode = TeletextHamming.UnHamming84(packet.Data[7]) & 0x01;


                // FIXME: Well, this is not ETS 300 706 kosher, however we are interested in DATA_UNIT_EBU_TELETEXT_SUBTITLE only
                if (_transmissionMode == TransmissionMode.TransmissionModeParallel && dataUnitId != DataUnitT.DataUnitEbuTeletextSubtitle) return;

                if (_receivingData && (
                        _transmissionMode == TransmissionMode.TransmissionModeSerial && Page(pageNumber) != Page(config.Page) ||
                        _transmissionMode == TransmissionMode.TransmissionModeParallel && Page(pageNumber) != Page(config.Page) && m == Magazine(config.Page)
                    ))
                {
                    _receivingData = false;
                    return;
                }

                // Page transmission is terminated, however now we are waiting for our new page
               if (!teletextRunSettings.PageNumbersBcd.Contains(pageNumber))
               {
                   return;
               }

               // Now we have the beginning of page transmission; if there is page_buffer pending, process it
                if (PageBuffer.Tainted)
                {
                    // it would be nice, if subtitle hides on previous video frame, so we contract 40 ms (1 frame @25 fps)
                    PageBuffer.HideTimestamp = timestamp - 40;
                    ProcessPage(PageBuffer);
                }

                PageBuffer.ShowTimestamp = timestamp;
                PageBuffer.HideTimestamp = 0;
                PageBuffer.Text = new int[25, 40]; //memset(page_buffer.text, 0x00, sizeof(page_buffer.text));
                PageBuffer.Tainted = false;
                _receivingData = true;
                primaryCharset.G0X28 = (int)BoolT.Undef;

                var c = primaryCharset.G0M29 != (int)BoolT.Undef ? primaryCharset.G0M29 : charset;
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
            else if (m == Magazine(teletextRunSettings.PageNumberBcd) && y >= 1 && y <= 23 && _receivingData)
            {
                // ETS 300 706, chapter 9.4.1: Packets X/26 at presentation Levels 1.5, 2.5, 3.5 are used for addressing
                // a character location and overwriting the existing character defined on the Level 1 page
                // ETS 300 706, annex B.2.2: Packets with Y = 26 shall be transmitted before any packets with Y = 1 to Y = 25;
                // so page_buffer.text[y,i] may already contain any character received
                // in frame number 26, skip original G0 character
                for (var i = 0; i < 40; i++)
                {
                    if (PageBuffer.Text[y, i] == 0x00)
                    {
                        PageBuffer.Text[y, i] = TelxToUcs2(packet.Data[i]);
                    }
                }

                PageBuffer.Tainted = true;
            }
            else if  (m == Magazine(teletextRunSettings.PageNumberBcd) && y == 26 && _receivingData)
            {
                // ETS 300 706, chapter 12.3.2: X/26 definition
                var x26Row = 0;
                var x26Col = 0;

                var triplets = new uint[13];
                var j = 0;
                for (var i = 1; i < 40; i += 3, j++) triplets[j] = TeletextHamming.UnHamming2418((packet.Data[i + 2] << 16) | (packet.Data[i + 1] << 8) | packet.Data[i]);

                for (var j2 = 0; j2 < 13; j2++)
                {
                    if (triplets[j2] == 0xffffffff)
                    {
                        // invalid data (HAM24/18 uncorrectable error detected), skip group
                        if (config.Verbose) Console.WriteLine($"! Unrecoverable data error; UNHAM24/18()={triplets[j2]}");
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
                        if (x26Row == 0) x26Row = 24;
                        x26Col = 0;
                    }

                    // ETS 300 706, chapter 12.3.1, table 27: termination marker
                    if (mode >= 0x11 && mode <= 0x1f && rowAddressGroup) break;

                    // ETS 300 706, chapter 12.3.1, table 27: character from G2 set
                    if (mode == 0x0f && !rowAddressGroup)
                    {
                        x26Col = (int)address2;
                        if (data > 31) PageBuffer.Text[x26Row, x26Col] = TeletextTables.G2[0, data - 0x20];
                    }

                    // ETS 300 706, chapter 12.3.1, table 27: G0 character with diacritical mark
                    if (mode >= 0x11 && mode <= 0x1f && !rowAddressGroup)
                    {
                        x26Col = (int)address2;

                        // A - Z
                        if (data >= 65 && data <= 90) PageBuffer.Text[x26Row, x26Col] = TeletextTables.G2Accents[mode - 0x11, data - 65];
                        // a - z
                        else if (data >= 97 && data <= 122) PageBuffer.Text[x26Row, x26Col] = TeletextTables.G2Accents[mode - 0x11, data - 71];
                        // other
                        else PageBuffer.Text[x26Row, x26Col] = TelxToUcs2((byte)data);
                    }
                }
            }
            else if  (m == Magazine(teletextRunSettings.PageNumberBcd) && y == 28 && _receivingData)
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
                        if (config.Verbose) Console.WriteLine($"! Unrecoverable data error; UNHAM24/18()={triplet0}");
                    }
                    else
                    {
                        // ETS 300 706, chapter 9.4.2: Packet X/28/0 Format 1 only
                        if ((triplet0 & 0x0f) == 0x00)
                        {
                            primaryCharset.G0X28 = (int)((triplet0 & 0x3f80) >> 7);
                            RemapG0Charset(primaryCharset.G0X28);
                        }
                    }
                }
            }
            else if (m == Magazine(teletextRunSettings.PageNumberBcd) && y == 29)
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
                        if (config.Verbose) Console.WriteLine($"! Unrecoverable data error; UNHAM24/18()={triplet0}");
                    }
                    else
                    {
                        // ETS 300 706, table 11: Coding of Packet M/29/0
                        // ETS 300 706, table 13: Coding of Packet M/29/4
                        if ((triplet0 & 0xff) == 0x00)
                        {
                            primaryCharset.G0M29 = (int)((triplet0 & 0x3f80) >> 7);
                            // X/28 takes precedence over M/29
                            if (primaryCharset.G0X28 == (int)BoolT.Undef)
                            {
                                RemapG0Charset(primaryCharset.G0M29);
                            }
                        }
                    }
                }
            }
            else if (m == 8 && y == 30)
            {
                // ETS 300 706, chapter 9.8: Broadcast Service Data Packets
                if (!states.ProgrammeInfoProcessed)
                {
                    // ETS 300 706, chapter 9.8.1: Packet 8/30 Format 1
                    if (TeletextHamming.UnHamming84(packet.Data[0]) < 2)
                    {
                        Console.Write("- Programme Identification Data = ");
                        for (var i = 20; i < 40; i++)
                        {
                            var c = TelxToUcs2(packet.Data[i]);
                            // strip any control codes from PID, eg. TVP station
                            if (c < 0x20) continue;

                            Console.Write(Ucs2ToUtf8(c));
                        }
                        Console.WriteLine();

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

                        Console.WriteLine($"- Programme Timestamp (UTC) = {localTime.ToLongDateString()} {localTime.ToLongTimeString()}");

                        if (config.Verbose) Console.WriteLine($"- Transmission mode = {(_transmissionMode == TransmissionMode.TransmissionModeSerial ? "serial" : "parallel")}");

                        if (config.SeMode)
                        {
                            Console.WriteLine($"- Broadcast Service Data Packet received, resetting UTC referential value to {t} seconds");
                            config.UtcRefValue = (ulong)t;
                            states.PtsInitialized = false;
                        }

                        states.ProgrammeInfoProcessed = true;
                    }
                }
            }
        }

    }
}
