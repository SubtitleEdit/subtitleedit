using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace Nikse.SubtitleEdit.Logic
{
    public class IfoParser
    {
        public struct AudioStream
        {
            public int LangageTypeSpecified;
            public string Langage;
            public string LangageCode;
            public string CodingMode;
            public int Channels;
            public string Extension;
        };

        public struct VideoStream
        {
            public string Aspect;
            public string Standard;
            public string CodingMode;
            public string Resolution;
        }

        public class VtsVobs
        {
            public int NumberOfAudioStreams;
            public int NumberOfSubtitles;
            public VideoStream VideoStream;
            public List<AudioStream> AudioStreams;
            public List<string> Subtitles;

            public VtsVobs()
            {
                VideoStream = new VideoStream();
                AudioStreams = new List<AudioStream>();
                Subtitles = new List<string>();
            }
        };

        public class ProgramChain
        {
            public int NumberOfPGC;
            public int NumberOfCells;
            public string PlaybackTime;
            public List<byte> PGCEntryCells;
            public List<string> PGCPlaybackTimes;
            public List<string> PGCStartTimes;
            public List<char> AudioStreamsAvailable;
            public List<char> SubtitlesAvailable;
            public List<Color> ColorLookupTable;

            public ProgramChain()
            {
                PGCEntryCells = new List<byte>();
                PGCPlaybackTimes = new List<string>();
                PGCStartTimes = new List<string>();
                AudioStreamsAvailable = new List<char>();
                SubtitlesAvailable = new List<char>();
                ColorLookupTable = new List<Color>();
            }
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


        readonly List<string> ArrayOfAudioMode = new List<string> { "AC3", "...", "MPEG1", "MPEG2", "LPCM", "...", "DTS" };
        readonly List<string> ArrayOfAudioExtension = new List<string> { "unspecified", "normal", "for visually impaired", "director's comments", "alternate director's comments" };
        readonly List<string> ArrayOfAspect = new List<string> { "4:3", "...", "...", "16:9" };
        readonly List<string> ArrayOfStandard = new List<string> { "NTSC", "PAL", "...", "..." };
        readonly List<string> ArrayOfCodingMode = new List<string> { "MPEG1", "MPEG2" };
        readonly List<string> ArrayOfNTSCResolution = new List<string> { "720x480", "704x480", "352x480", "352x240" };
        readonly List<string> ArrayOfPALResolution = new List<string> { "720x576", "704x576", "352x576", "352x288" };
        readonly List<string> ArrayOfLangageCode = new List<string> { "  ", "aa", "ab", "af", "am", "ar", "as", "ay", "az", "ba", "be", "bg", "bh", "bi", "bn", "bo", "br", "ca", "co", "cs", "cy", "da", "de", "dz", "el",
           "en", "eo", "es", "et", "eu", "fa", "fi", "fj", "fo", "fr", "fy", "ga", "gd", "gl", "gn", "gu", "ha", "he", "hi", "hr", "hu", "hy", "ia", "id", "ie", "ik",
           "in", "is", "it", "iu", "iw", "ja", "ji", "jw", "ka", "kk", "kl", "km", "kn", "ko", "ks", "ku", "ky", "la", "ln", "lo", "lt", "lv", "mg", "mi", "mk", "ml",
           "mn", "mo", "mr", "ms", "mt", "my", "na", "ne", "nl", "no", "oc", "om", "or", "pa", "pl", "ps", "pt", "qu", "rm", "rn", "ro", "ru", "rw", "sa", "sd", "sg",
           "sh", "si", "sk", "sl", "sm", "sn", "so", "sq", "sr", "ss", "st", "su", "sv", "sw", "ta", "te", "tg", "th", "ti", "tk", "tl", "tn", "to", "tr", "ts", "tt",
           "tw", "ug", "uk", "ur", "uz", "vi", "vo", "wo", "xh", "yi", "yo", "za", "zh", "zu", ""};
        readonly List<string> ArrayOfLangage = new List<string> { "Not Specified", "Afar", "Abkhazian", "Afrikaans", "Amharic", "Arabic", "Assamese", "Aymara", "Azerbaijani", "Bashkir", "Byelorussian", "Bulgarian", "Bihari", "Bislama", "Bengali; Bangla", "Tibetan", "Breton", "Catalan", "Corsican", "Czech(Ceske)", "Welsh", "Dansk", "Deutsch", "Bhutani", "Greek",
           "English", "Esperanto", "Espanol", "Estonian", "Basque", "Persian", "Suomi", "Fiji", "Faroese", "Français", "Frisian", "Irish", "Scots Gaelic", "Galician", "Guarani", "Gujarati", "Hausa", "Hebrew", "Hindi", "Hrvatski", "Magyar", "Armenian", "Interlingua", "Indonesian", "Interlingue", "Inupiak",
           "Indonesian", "Islenska", "Italiano", "Inuktitut", "Hebrew", "Japanese", "Yiddish", "Javanese", "Georgian", "Kazakh", "Greenlandic", "Cambodian", "Kannada", "Korean", "Kashmiri", "Kurdish", "Kirghiz", "Latin", "Lingala", "Laothian", "Lithuanian", "Latvian, Lettish", "Malagasy", "Maori", "Macedonian", "Malayalam",
           "Mongolian", "Moldavian", "Marathi", "Malay", "Maltese", "Burmese", "Nauru", "Nepali", "Nederlands", "Norsk", "Occitan", "(Afan) Oromo", "Oriya", "Punjabi", "Polish", "Pashto, Pushto", "Portugues", "Quechua", "Rhaeto-Romance", "Kirundi", "Romanian", "Russian", "Kinyarwanda", "Sanskrit", "Sindhi", "Sangho",
           "Serbo-Croatian", "Sinhalese", "Slovak", "Slovenian", "Samoan", "Shona", "Somali", "Albanian", "Serbian", "Siswati", "Sesotho", "Sundanese", "Svenska", "Swahili", "Tamil", "Telugu", "Tajik", "Thai", "Tigrinya", "Turkmen", "Tagalog", "Setswana", "Tonga", "Turkish", "Tsonga", "Tatar",
           "Twi", "Uighur", "Ukrainian", "Urdu", "Uzbek", "Vietnamese", "Volapuk", "Wolof", "Xhosa", "Yiddish", "Yoruba", "Zhuang", "Chinese", "Zulu", "???"};

        public VtsPgci VideoTitleSetProgramChainTable { get { return _vtsPgci; } }
        public VtsVobs VideoTitleSetVobs { get { return _vtsVobs; } }
        public string ErrorMessage { get; private set; }

        private VtsVobs _vtsVobs = new VtsVobs();
        private VtsPgci _vtsPgci = new VtsPgci();
        FileStream _fs;

        public IfoParser(string fileName)
        {
            try
            {
                _fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

                byte[] buffer = new byte[12];
                _fs.Position = 0;
                _fs.Read(buffer, 0, 12);
                string id = Encoding.UTF8.GetString(buffer);
                if (id != "DVDVIDEO-VTS")
                {
                    ErrorMessage = string.Format(Configuration.Settings.Language.DvdSubrip.WrongIfoType,  id, Environment.NewLine, fileName);
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
            string data;
            byte[] buffer = new byte[16];

            //retrieve video info
            _fs.Position = 0x200;
            data = IntToBin(GetEndian(2), 16);
            _vtsVobs.VideoStream.CodingMode = ArrayOfCodingMode[BinToInt(MidStr(data, 0, 2))];
            _vtsVobs.VideoStream.Standard = ArrayOfStandard[BinToInt(MidStr(data, 2, 2))];
            _vtsVobs.VideoStream.Aspect = ArrayOfAspect[BinToInt(MidStr(data, 4, 2))];
            if (_vtsVobs.VideoStream.Standard == "PAL")
                _vtsVobs.VideoStream.Resolution = ArrayOfPALResolution[BinToInt(MidStr(data, 13, 2))];
            else if (_vtsVobs.VideoStream.Standard == "NTSC")
                _vtsVobs.VideoStream.Resolution = ArrayOfNTSCResolution[BinToInt(MidStr(data, 13, 2))];

            //retrieve audio info
            _fs.Position = 0x202; //useless but here for readability
            _vtsVobs.NumberOfAudioStreams = GetEndian(2);
            //            _ifo.VtsVobs.AudioStreams = new List<AudioStream>();
            for (int i = 0; i < _vtsVobs.NumberOfAudioStreams; i++)
            {
                AudioStream audioStream = new AudioStream();
                data = IntToBin(GetEndian(2), 16);
                audioStream.LangageTypeSpecified = Convert.ToInt32(MidStr(data, 4, 2));
                audioStream.CodingMode = ArrayOfAudioMode[(BinToInt(MidStr(data, 0, 3)))];
                audioStream.Channels = BinToInt(MidStr(data, 13, 3)) + 1;
                _fs.Read(buffer, 0, 2);
                audioStream.LangageCode = Convert.ToChar(buffer[0]).ToString() + Convert.ToChar(buffer[1]).ToString();
                if (ArrayOfLangageCode.Contains(audioStream.LangageCode))
                    audioStream.Langage = ArrayOfLangage[ArrayOfLangageCode.IndexOf(audioStream.LangageCode)];
                _fs.Seek(1, SeekOrigin.Current);
                audioStream.Extension = ArrayOfAudioExtension[_fs.ReadByte()];
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
                string languageTwoLetter = Convert.ToChar(buffer[0]).ToString() + Convert.ToChar(buffer[1]).ToString();
                _vtsVobs.Subtitles.Add(InterpretLangageCode(languageTwoLetter));

                string subtitleFormat = string.Empty;
                _fs.Read(buffer, 0, 2); // reserved for language code extension + code extension
                switch (buffer[0])      // 4, 8, 10-12 unused
                {
                    // http://dvd.sourceforge.net/dvdinfo/sprm.html
                    case 1: subtitleFormat = "(caption/normal size char)"; break; //0 = unspecified caption
                    case 2: subtitleFormat = "(caption/large size char)"; break;
                    case 3: subtitleFormat = "(caption for children)"; break;
                    case 5: subtitleFormat = "(closed caption/normal size char)"; break;
                    case 6: subtitleFormat = "(closed caption/large size char)"; break;
                    case 7: subtitleFormat = "(closed caption for children)"; break;
                    case 9: subtitleFormat = "(forced caption)"; break;
                    case 13: subtitleFormat = "(director comments/normal size char)"; break;
                    case 14: subtitleFormat = "(director comments/large size char)"; break;
                    case 15: subtitleFormat = "(director comments for children)"; break;
                }

////                int languageId = buffer[1] & Helper.B11111000;
//                int languageId1 = buffer[0] & Helper.B11111000;
//                int languageId2= buffer[1] & Helper.B11111000;
//                System.Diagnostics.Debug.WriteLine(languageTwoLetter + " " + languageId1.ToString() + " " + languageId2.ToString() + "  " + buffer[0].ToString() + " " + buffer[1].ToString());
                _fs.Position += 2;
            }
        }

        private int BinToInt(string p)
        {
            return Convert.ToInt32(p, 2);
        }

        private string MidStr(string data, int start, int count)
        {
            return data.Substring(start, count);
        }

        private string IntToBin(int value, int digits)
        {
            string result = string.Empty;
            result = Convert.ToString(value, 2);
            while (result.Length < digits)
                result = "0" + result;
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

        private string InterpretLangageCode(string code)
        {
            int i = 0;
            while (ArrayOfLangageCode[i] != code && i < 143)
            {
                i++;
            }
            return ArrayOfLangage[i];
        }

        private void ParseVtsPgci()
        {
            const int SectorSize = 2048;

            _fs.Position = 0xCC; //Get VTS_PGCI adress
            int tableStart = SectorSize * GetEndian(4);

            _fs.Position = tableStart;
            _vtsPgci.NumberOfProgramChains = GetEndian(2);
            _vtsPgci.ProgramChains = new List<ProgramChain>();

            for (int i = 0; i < _vtsPgci.NumberOfProgramChains; i++)
            {
                //Parse PGC Header
                ProgramChain programChain = new ProgramChain();
                _fs.Position = tableStart + 4 + 8 * (i + 1);  //Get PGC adress
                int programChainAdress = GetEndian(4);
                _fs.Position = tableStart + programChainAdress + 2;  //Move to PGC
                programChain.NumberOfPGC = _fs.ReadByte();
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
                    string temp = IntToBin(_fs.ReadByte(), 8);
                    programChain.SubtitlesAvailable.Add(temp[0]);
                    _fs.Seek(3, SeekOrigin.Current);
                }

                //Parse Color LookUp Table (CLUT) - offset 00A4, 16*4 (0, Y, Cr, Cb)
                _fs.Position = tableStart + programChainAdress + 0xA4;
                for (int colorNumber = 0; colorNumber < 16; colorNumber++)
                {
                    byte[] colors = new byte[4];
                    _fs.Read(colors, 0, 4);
                    int y = colors[1]-16;
                    int cr = colors[2]-128;
                    int cb = colors[3]-128;
                    int r = (int)Math.Min(Math.Max(Math.Round(1.1644F * y + 1.596F * cr), 0), 255);
                    int g = (int)Math.Min(Math.Max(Math.Round(1.1644F * y - 0.813F * cr - 0.391F * cb), 0), 255);
                    int b = (int)Math.Min(Math.Max(Math.Round(1.1644F * y + 2.018F * cb), 0), 255);

                    programChain.ColorLookupTable.Add(Color.FromArgb(r, g, b));
                }

                //Parse Program Map
                _fs.Position = tableStart + programChainAdress + 0xE6;
                _fs.Position = tableStart + programChainAdress + GetEndian(2);
                for (int j = 0; j < programChain.NumberOfPGC; j++)
                {
                    programChain.PGCEntryCells.Add((byte)_fs.ReadByte());
                }

                // Cell Playback Info Table to retrieve duration
                _fs.Position = tableStart + programChainAdress + 0xE8;
                _fs.Position = tableStart + programChainAdress + GetEndian(2);
                int max = 0;
                List<int> timeArray = new List<int>();
                for (int k = 0; k < programChain.NumberOfPGC; k++)
                {
                    int time = 0;
                    if (k == programChain.NumberOfPGC - 1)
                        max = programChain.NumberOfCells;
                    else
                        max = programChain.PGCEntryCells[k + 1] - 1;
                    for (int j = programChain.PGCEntryCells[k]; j <= max; j++)
                    {
                        _fs.Seek(4, SeekOrigin.Current);
                        time += TimeToMs(GetEndian(4));
                        _fs.Seek(16, SeekOrigin.Current);
                    }
                    programChain.PGCPlaybackTimes.Add(MsToTime(time));
                    timeArray.Add(time);

                    //convert to start time
                    time = 0;
                    for (int l = 1; l <= k; l++)
                    {
                        time += timeArray[l - 1];
                    }
                    if (k == 0)
                        programChain.PGCStartTimes.Add(MsToTime(0));
                    if (k > 0)
                        programChain.PGCStartTimes.Add(MsToTime(time));
                }
                _vtsPgci.ProgramChains.Add(programChain);
            }
        }

        private int TimeToMs(int time)
        {
            int result;
            string temp;
            double fps;

            temp = IntToBin(time, 32);
            result = StrToInt(IntToHex(BinToInt(MidStr(temp,0,8)),1))*3600000;
            result = result + StrToInt(IntToHex(BinToInt(MidStr(temp,8,8)),2))*60000;
            result = result + StrToInt(IntToHex(BinToInt(MidStr(temp,16,8)),2))*1000;
            if (temp.Substring(24,2) == "11")
                fps = 30;
            else
                fps = 25;
            result += (int) Math.Round((1000.0 / fps) * StrToFloat(IntToHex(BinToInt(MidStr(temp, 26, 6)), 3)));
            return result;
        }

        private double StrToFloat(string p)
        {
            return Convert.ToDouble(p);
        }

        private int StrToInt(string p)
        {
            return int.Parse(p);
        }

        private string IntToHex(int value, int digits)
        {
            string hex = value.ToString("X");

            return hex.PadLeft(digits, '0');
        }

        private string MsToTime(double milliseconds)
        {
            TimeSpan ts = TimeSpan.FromMilliseconds(milliseconds);
            string s = string.Format("{0:#0}:{1:00}:{2:00}.{3:000}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            return s;
        }

        private string InterpretTime(int timeNumber)
        {
            string timeBytes = IntToBin(timeNumber, 32);
            int h = StrToInt(IntToHex(BinToInt(timeBytes.Substring(0,8)),1));
            int m = StrToInt(IntToHex(BinToInt(timeBytes.Substring(8,8)),2));
            int s = StrToInt(IntToHex(BinToInt(timeBytes.Substring(16,8)),2));
            int fps = 25;
            if (timeBytes.Substring(24, 2) == "11")
                fps = 30;
            int milliseconds = (int)Math.Round((1000.0/fps)*StrToFloat(IntToHex(BinToInt(timeBytes.Substring(26,6)),3)));
            TimeSpan ts = new TimeSpan(0, h, m, s, milliseconds);
            return MsToTime(ts.TotalMilliseconds);
        }

    }
}