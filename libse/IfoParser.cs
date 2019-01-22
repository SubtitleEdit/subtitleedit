using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Core
{
    public class IfoParser : IDisposable
    {
        public class AudioStream
        {
            public int LanguageTypeSpecified { get; set; }
            public string Language { get; set; }
            public string LanguageCode { get; set; }
            public string CodingMode { get; set; }
            public int Channels { get; set; }
            public string Extension { get; set; }
        };

        public class VideoStream
        {
            public string Aspect { get; set; }
            public string Standard { get; set; }
            public string CodingMode { get; set; }
            public string Resolution { get; set; }
        }

        public class VtsVobs
        {
            public int NumberOfAudioStreams { get; set; }
            public int NumberOfSubtitles { get; set; }
            public VideoStream VideoStream { get; set; }
            public List<AudioStream> AudioStreams { get; set; }
            public List<string> Subtitles { get; set; }
            public List<string> SubtitleIDs { get; set; }
            public List<string> SubtitleTypes { get; set; }

            public List<string> GetAllLanguages()
            {
                var list = new List<string>();
                for (int i = 0; i < Subtitles.Count; i++)
                {
                    if (i < SubtitleIDs.Count && i < SubtitleTypes.Count)
                    {
                        var ids = SubtitleIDs[i].Split(',');
                        var types = SubtitleTypes[i].Split(',');
                        if (ids.Length == 2 && ids[0].Trim() == ids[1].Trim() || ids.Length == 3 && ids[0].Trim() == ids[1].Trim() && ids[1].Trim() == ids[2].Trim())
                        {
                            list.Add(Subtitles[i] + " (" + ids[0].Trim() + ")");
                        }
                        else
                        {
                            if (ids.Length >= 1 && types.Length >= 1)
                            {
                                list.Add(Subtitles[i] + ", " + types[0].Trim() + " (" + ids[0].Trim() + ")");
                            }
                            if (ids.Length >= 2 && types.Length >= 2)
                            {
                                list.Add(Subtitles[i] + ", " + types[1].Trim() + " (" + ids[1].Trim() + ")");
                            }
                            if (ids.Length >= 3 && types.Length >= 3)
                            {
                                list.Add(Subtitles[i] + ", " + types[2].Trim() + " (" + ids[2].Trim() + ")");
                            }
                            if (ids.Length >= 4 && types.Length >= 4)
                            {
                                list.Add(Subtitles[i] + ", " + types[3].Trim() + " (" + ids[3].Trim() + ")");
                            }
                        }
                    }
                }
                return list;
            }

            public VtsVobs()
            {
                VideoStream = new VideoStream();
                AudioStreams = new List<AudioStream>();
                Subtitles = new List<string>();
                SubtitleIDs = new List<string>();
                SubtitleTypes = new List<string>();
            }
        };

        public class ProgramChain
        {
            public int NumberOfPgc { get; set; }
            public int NumberOfCells { get; set; }
            public string PlaybackTime { get; set; }
            public List<byte> PgcEntryCells { get; set; }
            public List<string> PgcPlaybackTimes { get; set; }
            public List<string> PgcStartTimes { get; set; }
            public List<char> AudioStreamsAvailable { get; set; }
            public List<byte[]> SubtitlesAvailable { get; set; }
            public List<Color> ColorLookupTable { get; set; }

            public ProgramChain()
            {
                PgcEntryCells = new List<byte>();
                PgcPlaybackTimes = new List<string>();
                PgcStartTimes = new List<string>();
                AudioStreamsAvailable = new List<char>();
                SubtitlesAvailable = new List<byte[]>();
                ColorLookupTable = new List<Color>();
            }

            public bool Has43Subs { get; set; }
            public bool HasWideSubs { get; set; }
            public bool HasLetterSubs { get; set; }
            public bool HasPanSubs { get; set; }
            public bool HasNoSpecificSubs { get; set; }
        };

        public class VtsPgci
        {
            public int NumberOfProgramChains;
            public List<ProgramChain> ProgramChains;

            public VtsPgci()
            {
                ProgramChains = new List<ProgramChain>();
            }
        };

        private readonly List<string> _arrayOfAudioMode = new List<string> { "AC3", "...", "MPEG1", "MPEG2", "LPCM", "...", "DTS" };
        private readonly List<string> _arrayOfAudioExtension = new List<string> { "unspecified", "normal", "for visually impaired", "director's comments", "alternate director's comments" };
        private readonly List<string> _arrayOfAspect = new List<string> { "4:3", "...", "...", "16:9" };
        private readonly List<string> _arrayOfStandard = new List<string> { "NTSC", "PAL", "...", "..." };
        private readonly List<string> _arrayOfCodingMode = new List<string> { "MPEG1", "MPEG2" };
        private readonly List<string> _arrayOfNtscResolution = new List<string> { "720x480", "704x480", "352x480", "352x240" };
        private readonly List<string> _arrayOfPalResolution = new List<string> { "720x576", "704x576", "352x576", "352x288" };

        public VtsPgci VideoTitleSetProgramChainTable => _vtsPgci;
        public VtsVobs VideoTitleSetVobs => _vtsVobs;
        public string ErrorMessage { get; private set; }

        private readonly VtsVobs _vtsVobs = new VtsVobs();
        private readonly VtsPgci _vtsPgci = new VtsPgci();
        private FileStream _fs;

        public IfoParser(string fileName)
        {
            try
            {
                _fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                var buffer = new byte[12];
                _fs.Position = 0;
                _fs.Read(buffer, 0, 12);
                string id = Encoding.UTF8.GetString(buffer);
                if (id != "DVDVIDEO-VTS")
                {
                    ErrorMessage = string.Format(Configuration.Settings.Language.DvdSubRip.WrongIfoType, id, Environment.NewLine, fileName);
                    return;
                }
                ParseVtsVobs();
                ParseVtsPgci();
                _fs.Close();
            }
            catch (Exception exception)
            {
                ErrorMessage = exception.Message + Environment.NewLine + exception.StackTrace;
            }
        }

        private void ParseVtsVobs()
        {
            var buffer = new byte[16];

            //retrieve video info
            _fs.Position = 0x200;
            var data = IntToBin(GetEndian(2), 16);
            _vtsVobs.VideoStream.CodingMode = _arrayOfCodingMode[BinToInt(MidStr(data, 0, 2))];
            _vtsVobs.VideoStream.Standard = _arrayOfStandard[BinToInt(MidStr(data, 2, 2))];
            _vtsVobs.VideoStream.Aspect = _arrayOfAspect[BinToInt(MidStr(data, 4, 2))];
            if (_vtsVobs.VideoStream.Standard == "PAL")
            {
                _vtsVobs.VideoStream.Resolution = _arrayOfPalResolution[BinToInt(MidStr(data, 13, 2))];
            }
            else if (_vtsVobs.VideoStream.Standard == "NTSC")
            {
                _vtsVobs.VideoStream.Resolution = _arrayOfNtscResolution[BinToInt(MidStr(data, 13, 2))];
            }

            //retrieve audio info
            _fs.Position = 0x202; //useless but here for readability
            _vtsVobs.NumberOfAudioStreams = GetEndian(2);
            for (int i = 0; i < _vtsVobs.NumberOfAudioStreams; i++)
            {
                var audioStream = new AudioStream();
                data = IntToBin(GetEndian(2), 16);
                audioStream.LanguageTypeSpecified = Convert.ToInt32(MidStr(data, 4, 2));
                audioStream.CodingMode = _arrayOfAudioMode[(BinToInt(MidStr(data, 0, 3)))];
                audioStream.Channels = BinToInt(MidStr(data, 13, 3)) + 1;
                _fs.Read(buffer, 0, 2);
                audioStream.LanguageCode = new string(new[] { Convert.ToChar(buffer[0]), Convert.ToChar(buffer[1]) });
                var language = DvdSubtitleLanguage.GetLanguageOrNull(audioStream.LanguageCode);
                if (language != null)
                {
                    audioStream.Language = language.NativeName;
                }

                _fs.Seek(1, SeekOrigin.Current);
                audioStream.Extension = _arrayOfAudioExtension[_fs.ReadByte()];
                _fs.Seek(2, SeekOrigin.Current);
                _vtsVobs.AudioStreams.Add(audioStream);
            }

            //retrieve subs info (only name)
            _fs.Position = 0x254;
            _vtsVobs.NumberOfSubtitles = GetEndian(2);
            _fs.Position += 2;
            for (int i = 0; i < _vtsVobs.NumberOfSubtitles; i++)
            {
                _fs.Read(buffer, 0, 2);
                var languageTwoLetter = new string(new[] { Convert.ToChar(buffer[0]), Convert.ToChar(buffer[1]) });
                _vtsVobs.Subtitles.Add(DvdSubtitleLanguage.GetNativeLanguageName(languageTwoLetter));
                _fs.Read(buffer, 0, 2); // reserved for language code extension + code extension

                //switch (buffer[0])      // 4, 8, 10-12 unused
                //{
                //    // http://dvd.sourceforge.net/dvdinfo/sprm.html
                //    case 1: subtitleFormat = "(caption/normal size char)"; break; //0 = unspecified caption
                //    case 2: subtitleFormat = "(caption/large size char)"; break;
                //    case 3: subtitleFormat = "(caption for children)"; break;
                //    case 5: subtitleFormat = "(closed caption/normal size char)"; break;
                //    case 6: subtitleFormat = "(closed caption/large size char)"; break;
                //    case 7: subtitleFormat = "(closed caption for children)"; break;
                //    case 9: subtitleFormat = "(forced caption)"; break;
                //    case 13: subtitleFormat = "(director comments/normal size char)"; break;
                //    case 14: subtitleFormat = "(director comments/large size char)"; break;
                //    case 15: subtitleFormat = "(director comments for children)"; break;
                //}

                _fs.Position += 2;
            }
        }

        private static int BinToInt(string p)
        {
            return Convert.ToInt32(p, 2);
        }

        private static string MidStr(string data, int start, int count)
        {
            return data.Substring(start, count);
        }

        private static string IntToBin(int value, int digits)
        {
            string result = Convert.ToString(value, 2);
            while (result.Length < digits)
            {
                result = "0" + result;
            }

            return result;
        }

        private int GetEndian(int count)
        {
            int result = 0;
            for (int i = count; i > 0; i--)
            {
                int b = _fs.ReadByte();
                result = (result << 8) + b;
            }
            return result;
        }

        private void ParseVtsPgci()
        {
            const int sectorSize = 2048;

            _fs.Position = 0xCC; //Get VTS_PGCI adress
            int tableStart = sectorSize * GetEndian(4);

            _fs.Position = tableStart;
            _vtsPgci.NumberOfProgramChains = GetEndian(2);
            _vtsPgci.ProgramChains = new List<ProgramChain>();

            for (int i = 0; i < _vtsPgci.NumberOfProgramChains; i++)
            {
                //Parse PGC Header
                var programChain = new ProgramChain();
                _fs.Position = tableStart + 4 + 8 * (i + 1);  //Get PGC adress
                int programChainAdress = GetEndian(4);
                _fs.Position = tableStart + programChainAdress + 2;  //Move to PGC
                programChain.NumberOfPgc = _fs.ReadByte();
                programChain.NumberOfCells = _fs.ReadByte();
                programChain.PlaybackTime = InterpretTime(GetEndian(4));
                _fs.Seek(4, SeekOrigin.Current);

                // check if audio streams are available for this PGC
                _fs.Position = tableStart + programChainAdress + 0xC;
                for (int j = 0; j < _vtsVobs.NumberOfAudioStreams; j++)
                {
                    string temp = IntToBin(_fs.ReadByte(), 8);
                    programChain.AudioStreamsAvailable.Add(temp[0]);
                    _fs.Seek(1, SeekOrigin.Current);
                }

                // check if subtitles are available for this PGC
                _fs.Position = tableStart + programChainAdress + 0x1C;
                for (int j = 0; j < _vtsVobs.NumberOfSubtitles; j++)
                {
                    // read and save full subpicture stream info inside program chain
                    var subtitle = new byte[4];
                    _fs.Read(subtitle, 0, 4);
                    programChain.SubtitlesAvailable.Add(subtitle);
                }

                CalculateSubtitleTypes(programChain);

                //Parse Color LookUp Table (CLUT) - offset 00A4, 16*4 (0, Y, Cr, Cb)
                _fs.Position = tableStart + programChainAdress + 0xA4;
                for (int colorNumber = 0; colorNumber < 16; colorNumber++)
                {
                    var colors = new byte[4];
                    _fs.Read(colors, 0, 4);
                    int y = colors[1] - 16;
                    int cr = colors[2] - 128;
                    int cb = colors[3] - 128;
                    int r = (int)Math.Min(Math.Max(Math.Round(1.1644F * y + 1.596F * cr), 0), 255);
                    int g = (int)Math.Min(Math.Max(Math.Round(1.1644F * y - 0.813F * cr - 0.391F * cb), 0), 255);
                    int b = (int)Math.Min(Math.Max(Math.Round(1.1644F * y + 2.018F * cb), 0), 255);

                    programChain.ColorLookupTable.Add(Color.FromArgb(r, g, b));
                }

                //Parse Program Map
                _fs.Position = tableStart + programChainAdress + 0xE6;
                _fs.Position = tableStart + programChainAdress + GetEndian(2);
                for (int j = 0; j < programChain.NumberOfPgc; j++)
                {
                    programChain.PgcEntryCells.Add((byte)_fs.ReadByte());
                }

                // Cell Playback Info Table to retrieve duration
                _fs.Position = tableStart + programChainAdress + 0xE8;
                _fs.Position = tableStart + programChainAdress + GetEndian(2);
                var timeArray = new List<int>();
                for (int k = 0; k < programChain.NumberOfPgc; k++)
                {
                    int time = 0;
                    int max;
                    if (k == programChain.NumberOfPgc - 1)
                    {
                        max = programChain.NumberOfCells;
                    }
                    else
                    {
                        max = programChain.PgcEntryCells[k + 1] - 1;
                    }

                    for (int j = programChain.PgcEntryCells[k]; j <= max; j++)
                    {
                        _fs.Seek(4, SeekOrigin.Current);
                        time += TimeToMs(GetEndian(4));
                        _fs.Seek(16, SeekOrigin.Current);
                    }
                    programChain.PgcPlaybackTimes.Add(MsToTime(time));
                    timeArray.Add(time);

                    //convert to start time
                    time = 0;
                    for (int l = 1; l <= k; l++)
                    {
                        time += timeArray[l - 1];
                    }
                    if (k == 0)
                    {
                        programChain.PgcStartTimes.Add(MsToTime(0));
                    }

                    if (k > 0)
                    {
                        programChain.PgcStartTimes.Add(MsToTime(time));
                    }
                }
                _vtsPgci.ProgramChains.Add(programChain);
            }
        }

        private void CalculateSubtitleTypes(ProgramChain programChain)
        {
            // Additional Code to analyse stream bytes
            if (_vtsVobs.NumberOfSubtitles > 0)
            {
                // load the 'last' subpicture stream info,
                // because if we have more than one subtitle stream,
                // all subtitle positions > 0
                // lastSubtitle[0] is related to 4:3
                // lastSubtitle[1] is related to Wide
                // lastSubtitle[2] is related to letterboxed
                // lastSubtitle[3] is related to pan&scan
                byte[] lastSubtitle = programChain.SubtitlesAvailable[programChain.SubtitlesAvailable.Count - 1];

                int countSubs = 0;

                // set defaults for all possible subpicture types and positions
                programChain.Has43Subs = false;
                programChain.HasWideSubs = false;
                programChain.HasLetterSubs = false;
                programChain.HasPanSubs = false;
                programChain.HasNoSpecificSubs = true;

                int pos43Subs = -1;
                int posWideSubs = -1;
                int posLetterSubs = -1;
                int posPanSubs = -1;

                // parse different subtitle bytes
                if (lastSubtitle[0] > 0x80)
                {
                    programChain.Has43Subs = true;
                    countSubs++; // 4:3
                }
                if (lastSubtitle[1] > 0)
                {
                    programChain.HasWideSubs = true;
                    countSubs++; // wide
                }
                if (lastSubtitle[2] > 0)
                {
                    programChain.HasLetterSubs = true;
                    countSubs++; // letterboxed
                }
                if (lastSubtitle[3] > 0)
                {
                    programChain.HasPanSubs = true;
                    countSubs++; // pan&scan
                }

                if (countSubs == 0)
                {
                    // may be, only a 4:3 stream exists
                    // -> lastSubtitle[0] = 0x80
                }
                else
                {
                    if (_vtsVobs.NumberOfSubtitles == 1)
                    {
                        // only 1 stream exists, may be letterboxed
                        // if so we cound't find wide id, because lastSubtitle[1] = 0 !!
                        // corresponding wide stream byte is 0 => wide id = 0x20
                        // letterboxed = 0x21
                        if (programChain.HasLetterSubs && !programChain.HasWideSubs)
                        {
                            // repair it
                            programChain.HasWideSubs = true;
                        }
                    }
                    programChain.HasNoSpecificSubs = false;
                }

                // subpucture streams start with 0x20
                int subStream = 0x20;

                // Now we know all about available subpicture streams, including position type
                // And we can create whole complete definitions for all avalable streams
                foreach (byte[] subtitle in programChain.SubtitlesAvailable)
                {
                    if (programChain.HasNoSpecificSubs)
                    {
                        // only one unspezified subpicture stream exists
                        _vtsVobs.SubtitleIDs.Add($"0x{subStream++:x2}");
                        _vtsVobs.SubtitleTypes.Add("unspecific");
                    }
                    else
                    {
                        // read stream position for evey subtitle type from subtitle byte
                        if (programChain.Has43Subs)
                        {
                            pos43Subs = subtitle[0] - 0x80;
                        }
                        if (programChain.HasWideSubs)
                        {
                            posWideSubs = subtitle[1];
                        }
                        if (programChain.HasLetterSubs)
                        {
                            posLetterSubs = subtitle[2];
                        }
                        if (programChain.HasPanSubs)
                        {
                            posPanSubs = subtitle[3];
                        }

                        // Now we can create subpicture id's and types for every stream
                        // All used subpicture id's and types will beappended to string, separated by colon
                        // So it's possible to split it later
                        string sub = string.Empty;
                        string subType = string.Empty;
                        if (programChain.Has43Subs)
                        {
                            sub = $"0x{subStream + pos43Subs:x2}";
                            subType = "4:3";
                        }
                        if (programChain.HasWideSubs)
                        {
                            if (sub.Length > 0)
                            {
                                sub += ", ";
                                subType += ", ";
                            }
                            sub += $"0x{subStream + posWideSubs:x2}";
                            subType += "wide";
                        }
                        if (programChain.HasLetterSubs)
                        {
                            if (sub.Length > 0)
                            {
                                sub += ", ";
                                subType += ", ";
                            }
                            sub += $"0x{subStream + posLetterSubs:x2}";
                            subType += "letterboxed";
                        }
                        if (programChain.HasPanSubs)
                        {
                            if (sub.Length > 0)
                            {
                                sub += ", ";
                                subType += ", ";
                            }
                            sub += $"0x{subStream + posPanSubs:x2}";
                            subType += "pan&scan";
                        }

                        _vtsVobs.SubtitleIDs.Add(sub);
                        _vtsVobs.SubtitleTypes.Add(subType);
                    }
                }
            }
        }

        private static int TimeToMs(int time)
        {
            double fps;

            var temp = IntToBin(time, 32);
            var result = StrToInt(IntToHex(BinToInt(MidStr(temp, 0, 8)), 1)) * 3600000;
            result = result + StrToInt(IntToHex(BinToInt(MidStr(temp, 8, 8)), 2)) * 60000;
            result = result + StrToInt(IntToHex(BinToInt(MidStr(temp, 16, 8)), 2)) * 1000;
            if (temp.Substring(24, 2) == "11")
            {
                fps = 30;
            }
            else
            {
                fps = 25;
            }

            result += (int)Math.Round((TimeCode.BaseUnit / fps) * StrToFloat(IntToHex(BinToInt(MidStr(temp, 26, 6)), 3)));
            return result;
        }

        private static double StrToFloat(string p)
        {
            return Convert.ToDouble(p, System.Globalization.CultureInfo.InvariantCulture);
        }

        private static int StrToInt(string p)
        {
            return int.Parse(p);
        }

        private static string IntToHex(int value, int digits)
        {
            string hex = value.ToString("X");

            return hex.PadLeft(digits, '0');
        }

        private static string MsToTime(double milliseconds)
        {
            var ts = TimeSpan.FromMilliseconds(milliseconds);
            string s = $"{ts.Hours:#0}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
            return s;
        }

        private static string InterpretTime(int timeNumber)
        {
            string timeBytes = IntToBin(timeNumber, 32);
            int h = StrToInt(IntToHex(BinToInt(timeBytes.Substring(0, 8)), 1));
            int m = StrToInt(IntToHex(BinToInt(timeBytes.Substring(8, 8)), 2));
            int s = StrToInt(IntToHex(BinToInt(timeBytes.Substring(16, 8)), 2));
            int fps = 25;
            if (timeBytes.Substring(24, 2) == "11")
            {
                fps = 30;
            }

            int milliseconds = (int)Math.Round((TimeCode.BaseUnit / fps) * StrToFloat(IntToHex(BinToInt(timeBytes.Substring(26, 6)), 3)));
            var ts = new TimeSpan(0, h, m, s, milliseconds);
            return MsToTime(ts.TotalMilliseconds);
        }

        private void ReleaseManagedResources()
        {
            if (_fs != null)
            {
                _fs.Dispose();
                _fs = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseManagedResources();
            }
        }

    }
}
