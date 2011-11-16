using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.VideoPlayers;

namespace Nikse.SubtitleEdit.Logic
{
    public static class Utilities
    {
        internal const string WinXp2kUnicodeFontName = "Times New Roman";

        public static VideoInfo GetVideoInfo(string fileName, EventHandler event1)
        {
            VideoInfo info = TryReadVideoInfoViaAviHeader(fileName);
            if (info.Success)
                return info;

            info = TryReadVideoInfoViaMatroskaHeader(fileName);
            if (info.Success)
                return info;

            info = TryReadVideoInfoViaDirectShow(fileName);
            if (info.Success)
                return info;

            info = TryReadVideoInfoViaMp4(fileName);
            if (info.Success)
                return info;

            return new VideoInfo { VideoCodec = "Unknown" };
        }

        private static VideoInfo TryReadVideoInfoViaDirectShow(string fileName)
        {
            return QuartsPlayer.GetVideoInfo(fileName);
        }

        private static VideoInfo TryReadVideoInfoViaMatroskaHeader(string fileName)
        {
            var info = new VideoInfo { Success = false };

            try
            {
                bool hasConstantFrameRate = false;
                bool success = false;
                double frameRate = 0;
                int width = 0;
                int height = 0;
                double milliseconds = 0;
                string videoCodec = string.Empty;

                var matroskaParser = new Matroska();
                matroskaParser.GetMatroskaInfo(fileName, ref success, ref hasConstantFrameRate, ref frameRate, ref width, ref height, ref milliseconds, ref videoCodec);
                if (success)
                {
                    info.Width = width;
                    info.Height = height;
                    info.FramesPerSecond = frameRate;
                    info.Success = true;
                    info.TotalMilliseconds = milliseconds;
                    info.TotalSeconds = milliseconds / 1000.0;
                    info.TotalFrames = info.TotalSeconds * frameRate;
                    info.VideoCodec = videoCodec;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            return info;
        }

        private static VideoInfo TryReadVideoInfoViaAviHeader(string fileName)
        {
            var info = new VideoInfo { Success = false };

            try
            {
                var rp = new RiffParser();
                var dh = new RiffDecodeHeader(rp);
                rp.OpenFile(fileName);
                info.FileType = RiffParser.FromFourCC(rp.FileType);
                if (RiffParser.ckidAVI == rp.FileType)
                {
                    dh.ProcessMainAVI();
                    info.Width = dh.Width;
                    info.Height = dh.Height;
                    info.FramesPerSecond = dh.FrameRate;
                    info.TotalFrames = dh.TotalFrames;
                    info.TotalMilliseconds = dh.TotalMilliseconds;
                    info.VideoCodec = dh.VideoHandler;
                    info.Success = true;
                }
            }
            catch
            {
            }
            return info;
        }

        private static VideoInfo TryReadVideoInfoViaMp4(string fileName)
        {
            var info = new VideoInfo { Success = false };

            try
            {
                var mp4Parser = new Mp4.Mp4Parser(fileName);
                if (mp4Parser.Moov != null && mp4Parser.VideoResolution.X > 0)
                {
                    info.Width = mp4Parser.VideoResolution.X;
                    info.Height = mp4Parser.VideoResolution.Y;
                    info.TotalMilliseconds = mp4Parser.Duration.TotalSeconds;
                    info.VideoCodec = "MP4";
                    info.Success = true;
                }
            }
            catch
            {
            }
            return info;
        }

        public static IEnumerable<string> GetMovieFileExtensions()
        {
            return new List<string> { ".avi", ".mkv", ".wmv", ".mpg", ".mpeg", ".divx", ".mp4", ".asf", ".flv", ".mov", ".m4v", ".vob" };
        }

        public static string GetVideoFileFilter()
        {
            var sb = new StringBuilder();
            sb.Append(Configuration.Settings.Language.General.VideoFiles + "|");
            int i = 0;
            foreach (string extension in GetMovieFileExtensions())
            {
                if (i > 0)
                    sb.Append(";");
                sb.Append("*" + extension);
                i++;
            }
            sb.Append("|" + Configuration.Settings.Language.General.AllFiles + "|*.*");
            return sb.ToString();
        }

        public static bool IsInteger(string s)
        {
            int i;
            if (int.TryParse(s, out i))
                return true;
            return false;
        }

        internal static SubtitleFormat GetSubtitleFormatByFriendlyName(string friendlyName)
        {
            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
            {
                if (format.FriendlyName == friendlyName)
                    return format;
            }
            return null;
        }

        public static string FormatBytesToDisplayFileSize(long fileSize)
        {
            {
                if (fileSize <= 1024)
                    return string.Format("{0} bytes", fileSize);
                if (fileSize <= 1024 * 1024)
                    return string.Format("{0} kb", fileSize / 1024);
                if (fileSize <= 1024 * 1024 * 1024)
                    return string.Format("{0:0.0} mb", (float)fileSize / (1024 * 1024));
                return string.Format("{0:0.0} gb", (float)fileSize / (1024 * 1024 * 1024));
            }
        }

        public static int ShowSubtitle(List<Paragraph> paragraphs, VideoPlayerContainer videoPlayerContainer)
        {
            int index = 0;
            if (videoPlayerContainer.VideoPlayer != null)
            {
                double positionInMilliseconds = (videoPlayerContainer.VideoPlayer.CurrentPosition * 1000.0) + 15;
                string text = string.Empty;
                foreach (Paragraph p in paragraphs)
                {
                    if (p.StartTime.TotalMilliseconds <= positionInMilliseconds &&
                        p.EndTime.TotalMilliseconds > positionInMilliseconds)
                    {
                        text = p.Text.Replace("|", Environment.NewLine);
                        break;
                    }
                    index++;
                }
                if (index == paragraphs.Count)
                    index = -1;

                if (videoPlayerContainer.SubtitleText != text)
                {
                    videoPlayerContainer.SubtitleText = text;
                    return index;
                }
            }
            return -1;
        }

        public static string ReadTextFileViaUrlAndProxyIfAvailable(string url)
        {
            var wc = new WebClient { Proxy = GetProxy() };
            var ms = new MemoryStream(wc.DownloadData(url));
            var reader = new StreamReader(ms);
            return reader.ReadToEnd().Trim();
        }

        public static WebProxy GetProxy()
        {
            if (!string.IsNullOrEmpty(Configuration.Settings.Proxy.ProxyAddress))
            {
                var proxy = new WebProxy(Configuration.Settings.Proxy.ProxyAddress);

                if (!string.IsNullOrEmpty(Configuration.Settings.Proxy.UserName))
                {
                    if (string.IsNullOrEmpty(Configuration.Settings.Proxy.Domain))
                        proxy.Credentials = new NetworkCredential(Configuration.Settings.Proxy.UserName, Configuration.Settings.Proxy.DecodePassword());
                    else
                        proxy.Credentials = new NetworkCredential(Configuration.Settings.Proxy.UserName, Configuration.Settings.Proxy.DecodePassword(), Configuration.Settings.Proxy.Domain);
                }
                else
                    proxy.UseDefaultCredentials = true;

                return proxy;
            }
            return null;
        }

        private static bool IsPartOfNumber(string s, int position)
        {
            if (",.".Contains(s[position].ToString()))
            {
                if (position > 0 && position < s.Length - 1)
                {
                    return "1234567890".Contains(s[position - 1].ToString()) && "1234567890".Contains(s[position + 1].ToString());
                }
            }
            return false;
        }

        public static string AutoBreakLine(string text)
        {
            return AutoBreakLine(text, 5, Configuration.Settings.General.SubtitleLineMaximumLength, Configuration.Settings.Tools.MergeLinesShorterThan);
        }

        public static string AutoBreakLine(string text, int mininumLength, int maximumLength, int mergeLinesShorterThan)
        {
            string s = text.Replace(Environment.NewLine, " ");
            s = s.Replace("  ", " ");
            s = s.Replace("  ", " ");
            string temp = RemoveHtmlTags(s);
            if (temp.Length < mergeLinesShorterThan)
            {
                string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 2 && Utilities.RemoveHtmlTags(lines[1]).Trim().StartsWith("-"))
                    return text;
                if (lines.Length == 3 && Utilities.RemoveHtmlTags(lines[1]).Trim().StartsWith("-") && Utilities.RemoveHtmlTags(lines[2]).Trim().StartsWith("-"))
                    return text;
                return s;
            }

            int splitPos = -1;
            int mid = s.Length / 2;

            // try to find " - " with uppercase letter after (dialogue)
            if (splitPos == -1 && s.Contains(" - "))
            {
                for (int j = 0; j < (maximumLength / 2)+5; j++)
                {
                    if (mid + j + 4 < s.Length)
                    {
                        if (s[mid + j] == '-' && s[mid + j + 1] == ' ' && s[mid + j - 1] == ' ')
                        {
                            string rest = s.Substring(mid + j + 1).TrimStart();
                            if (rest.Length > 0 && (rest.Substring(0, 1) == rest.Substring(0, 1).ToUpper()))
                            {
                                splitPos = mid + j;
                                break;
                            }
                        }
                    }
                    if (mid - (j + 1) > 4)
                    {
                        if (s[mid - j] == '-' && s[mid - j + 1] == ' ' && s[mid - j - 1] == ' ')
                        {
                            string rest = s.Substring(mid - j + 1).TrimStart();
                            if (rest.Length > 0 && (rest.Substring(0, 1) == rest.Substring(0, 1).ToUpper()))
                            {
                                splitPos = mid - j;
                                break;
                            }
                        }
                    }
                }
            }

            if (splitPos == -1)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (mid + j + 1 < s.Length && mid + j > 0)
                    {
                        if (".!?".Contains(s[mid + j].ToString()) && !IsPartOfNumber(s, mid + j))
                        {
                            splitPos = mid + j + 1;
                            if (".!?0123456789".Contains(s[splitPos].ToString()))
                            { // do not break double/tripple end lines like "!!!" or "..."
                                splitPos++;
                                if (".!?0123456789".Contains(s[mid + j + 1].ToString()))
                                    splitPos++;
                            }
                            break;
                        }
                        if (".!?".Contains(s[mid - j].ToString()) && !IsPartOfNumber(s, mid - j))
                        {
                            splitPos = mid - j;
                            splitPos++;
                            break;
                        }
                    }
                }
            }

            if (splitPos == -1)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (mid + j + 1 < s.Length && mid + j > 0)
                    {
                        if (".!?, ".Contains(s[mid + j].ToString()) && !IsPartOfNumber(s, mid + j))
                        {
                            splitPos = mid + j;
                            if (" .!?".Contains(s[mid + j + 1].ToString()))
                            {
                                splitPos++;
                                if (" .!?".Contains(s[mid + j + 2].ToString()))
                                    splitPos++;
                            }
                            break;
                        }
                        if (".!?, ".Contains(s[mid - j].ToString()) && !IsPartOfNumber(s, mid - j))
                        {
                            splitPos = mid - j;
                            if (".!?".Contains(s[mid - (j + 1)].ToString()))
                            {
                                splitPos--;
                                if (".!?".Contains(s[mid + (j + 2)].ToString()))
                                    splitPos--;
                            }
                            break;
                        }
                    }
                }
            }

            if (splitPos == -1)
            {
                splitPos = mid;
                s = s.Insert(mid - 1, "-");
            }
            if (splitPos < s.Length - 2)
                s = s.Substring(0, splitPos).TrimEnd() + Environment.NewLine + s.Substring(splitPos).Trim();
            return s.TrimEnd();
        }

        public static string UnbreakLine(string text)
        {
            if (text.Contains(Environment.NewLine))
            {
                string newText = text.Replace(Environment.NewLine, " ");
                while (newText.Contains("  "))
                    newText = newText.Replace("  ", " ");
                return newText;
            }
            return text;
        }

        public static void InitializeSubtitleFont(Control control)
        {
            var gs = Configuration.Settings.General;

            if (string.IsNullOrEmpty(gs.SubtitleFontName))
                gs.SubtitleFontName = "Tahoma";

            if (gs.SubtitleFontBold)
                control.Font = new Font(gs.SubtitleFontName, gs.SubtitleFontSize, FontStyle.Bold);
            else
                control.Font = new Font(gs.SubtitleFontName, gs.SubtitleFontSize);

            control.BackColor = gs.SubtitleBackgroundColor;
            control.ForeColor = gs.SubtitleFontColor;
        }

        public static string RemoveHtmlTags(string s)
        {
            if (s == null)
                return null;

            s = s.Replace("<i>", string.Empty);
            s = s.Replace("</i>", string.Empty);
            s = s.Replace("<b>", string.Empty);
            s = s.Replace("</b>", string.Empty);
            s = s.Replace("<u>", string.Empty);
            s = s.Replace("</u>", string.Empty);
            s = s.Replace("<I>", string.Empty);
            s = s.Replace("</I>", string.Empty);
            s = s.Replace("<B>", string.Empty);
            s = s.Replace("</B>", string.Empty);
            s = s.Replace("<U>", string.Empty);
            s = s.Replace("</U>", string.Empty);
            s = RemoveParagraphTag(s);
            return RemoveHtmlFontTag(s);
        }

        internal static string RemoveHtmlFontTag(string s)
        {
            s = s.Replace("</font>", string.Empty);
            s = s.Replace("</FONT>", string.Empty);
            s = s.Replace("</Font>", string.Empty);
            s = s.Replace("<font>", string.Empty);
            s = s.Replace("<FONT>", string.Empty);
            s = s.Replace("<Font>", string.Empty);
            while (s.ToLower().Contains("<font"))
            {
                int startIndex = s.ToLower().IndexOf("<font");
                int endIndex = Math.Max(s.IndexOf(">"), startIndex + 4);
                s = s.Remove(startIndex, (endIndex - startIndex) + 1);
            }
            return s;
        }

        internal static string RemoveParagraphTag(string s)
        {
            s = s.Replace("</p>", string.Empty);
            s = s.Replace("</P>", string.Empty);
            s = s.Replace("<P>", string.Empty);
            s = s.Replace("<P>", string.Empty);
            while (s.ToLower().Contains("<p "))
            {
                int startIndex = s.ToLower().IndexOf("<p ");
                int endIndex = Math.Max(s.IndexOf(">"), startIndex + 4);
                s = s.Remove(startIndex, (endIndex - startIndex) + 1);
            }
            return s;
        }

        public static Encoding GetEncodingFromFile(string fileName)
        {
            Encoding encoding = Encoding.Default;
            try
            {
                var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                var bom = new byte[12]; // Get the byte-order mark, if there is one
                file.Position = 0;
                file.Read(bom, 0, 12);
                if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf)
                    encoding = Encoding.UTF8;
                else if (bom[0] == 0xff && bom[1] == 0xfe)
                    encoding = Encoding.Unicode;
                else if (bom[0] == 0xfe && bom[1] == 0xff) // utf-16 and ucs-2
                    encoding = Encoding.BigEndianUnicode;
                else if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) // ucs-4
                    encoding = Encoding.UTF32;
                else if (encoding == Encoding.Default && file.Length > 12)
                {
                    int length = (int)file.Length;
                    if (length > 500000)
                        length = 500000;

                    file.Position = 0;
                    var buffer = new byte[length];
                    file.Read(buffer, 0, length);

                    bool couldBeUtf8;
                    if (IsUtf8(buffer, out couldBeUtf8))
                    {
                        encoding = Encoding.UTF8;
                    }
                    else if (couldBeUtf8 && fileName.ToLower().EndsWith(".xml") && Encoding.Default.GetString(buffer).ToLower().Replace("'", "\"").Contains("encoding=\"utf-8\""))
                    { // keep utf-8 encoding for xml files with utf-8 in header (without any utf-8 encoded characters, but with only allowed utf-8 characters)
                        encoding = Encoding.UTF8; 
                    }
                    else if (Configuration.Settings.General.AutoGuessAnsiEncoding)
                    {
                        encoding = DetectAnsiEncoding(buffer);

                        Encoding greekEncoding = Encoding.GetEncoding(1253); // Greek
                        if (GetCount(greekEncoding.GetString(buffer), "μου", "είναι", "Είναι", "αυτό", "Τόμπυ", "καλά") > 5)
                            return greekEncoding;

                        Encoding russianEncoding = Encoding.GetEncoding(1251); // Russian
                        if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5)
                            return russianEncoding;
                        russianEncoding = Encoding.GetEncoding(28595); // Russian
                        if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5)
                            return russianEncoding;

                        Encoding arabicEncoding = Encoding.GetEncoding(28596); // Arabic
                        Encoding hewbrewEncoding = Encoding.GetEncoding(28598); // Hebrew
                        if (GetCount(arabicEncoding.GetString(buffer), "من", "هل", "لا", "فى", "لقد", "ما") > 5)
                        {
                            if (GetCount(hewbrewEncoding.GetString(buffer), "אולי", "אולי", "אולי", "אולי", "טוב", "טוב") > 10)
                                return hewbrewEncoding;
                            return arabicEncoding;
                        }
                        else if (GetCount(hewbrewEncoding.GetString(buffer), "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב") > 5)
                            return hewbrewEncoding;
                    }
                }
                file.Close();
                file.Dispose();
            }
            catch
            {

            }
            return encoding;
        }

        /// <summary>
        /// Will try to determine if buffer is utf-8 encoded or not.
        /// If any non-utf8 sequences are found then false is returned, if no utf8 multibytes sequences are found then false is returned.
        /// </summary>
        private static bool IsUtf8(byte[] buffer, out bool couldBeUtf8)
        {
            couldBeUtf8 = false;
            int utf8Count = 0;
            int i = 0;
            while (i < buffer.Length - 3)
            {
                byte b = buffer[i];
                if (b > 127)
                {
                    if (b >= 194 && b <= 223 && buffer[i + 1] >= 128 && buffer[i + 1] <= 191)
                    { // 2-byte sequence
                        utf8Count++;
                        i++;
                    }
                    else if (b >= 224 && b <= 239 && buffer[i + 1] >= 128 && buffer[i + 1] <= 191 &&
                                                     buffer[i + 2] >= 128 && buffer[i + 2] <= 191)
                    { // 3-byte sequence
                        utf8Count++;
                        i += 2;
                    }
                    else if (b >= 240 && b <= 244 && buffer[i + 1] >= 128 && buffer[i + 1] <= 191 &&
                                                     buffer[i + 2] >= 128 && buffer[i + 2] <= 191 &&
                                                     buffer[i + 3] >= 128 && buffer[i + 3] <= 191)
                    { // 4-byte sequence
                        utf8Count++;
                        i += 3;
                    }
                    else
                    {
                        return false;
                    }
                }
                i++;
            }
            couldBeUtf8 = true;
            if (utf8Count == 0)
                return false; // not utf-8 (no characters utf-8 encoded...)

            return true;
        }

        public static Encoding DetectAnsiEncoding(byte[] buffer)
        {
            if (IsRunningOnMono())
                return Encoding.Default;

            try
            {
                Encoding encoding = DetectEncoding.EncodingTools.DetectInputCodepage(buffer);

                Encoding greekEncoding = Encoding.GetEncoding(1253); // Greek
                if (GetCount(greekEncoding.GetString(buffer), "μου", "είναι", "Είναι", "αυτό", "Τόμπυ", "καλά") > 5)
                    return greekEncoding;

                Encoding russianEncoding = Encoding.GetEncoding(1251); // Russian
                if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5)
                    return russianEncoding;
                russianEncoding = Encoding.GetEncoding(28595); // Russian
                if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5)
                    return russianEncoding;

                Encoding arabicEncoding = Encoding.GetEncoding(28596); // Arabic
                Encoding hewbrewEncoding = Encoding.GetEncoding(28598); // Hebrew
                if (GetCount(arabicEncoding.GetString(buffer), "من", "هل", "لا", "فى", "لقد", "ما") > 5)
                {
                    if (GetCount(hewbrewEncoding.GetString(buffer), "אולי", "אולי", "אולי", "אולי", "טוב", "טוב") > 10)
                        return hewbrewEncoding;
                    return arabicEncoding;
                }
                else if (GetCount(hewbrewEncoding.GetString(buffer), "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב") > 5)
                    return hewbrewEncoding;

                return encoding;
            }
            catch
            {
                return Encoding.Default;
            }
        }

        public static string DictionaryFolder
        {
            get
            {
                return Configuration.DictionariesFolder;
            }
        }

        public static List<string> GetDictionaryLanguages()
        {
            var list = new List<string>();
            string dictionaryFolder = DictionaryFolder;
            if (Directory.Exists(dictionaryFolder))
            {
                foreach (string dic in Directory.GetFiles(dictionaryFolder, "*.dic"))
                {
                    string name = Path.GetFileNameWithoutExtension(dic);
                    if (!name.StartsWith("hyph"))
                    {
                        try
                        {
                            var ci = new CultureInfo(name.Replace("_", "-"));
                            name = ci.DisplayName + " [" + name + "]";
                        }
                        catch (Exception exception)
                        {
                            System.Diagnostics.Debug.WriteLine(exception.Message);
                            name = "[" + name + "]";
                        }
                        list.Add(name);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Get suggested display time in milliseconds (a tiny bit low).
        /// </summary>
        /// <param name="text">Subtitle text paragraph</param>
        /// <returns>Suggested display time in milliseconds (a tiny bit low).</returns>
        public static double GetDisplayMillisecondsFromText(string text)
        {
            int l = text.Length;

            if (l < 8)
                return 800;
            if (l < 15)
                return 1000;
            if (l < 25)
                return 1300;
            if (l < 35)
                return 1500;
            if (l < 50)
                return 2000;
            if (l < 75)
                return 2500;
            if (l < 90)
                return 3000;
            return 3500;
        }

        private static int GetCount(string text,
                            string word1,
                            string word2,
                            string word3,
                            string word4,
                            string word5,
                            string word6)
        {
            var regEx1 = new Regex("\\b" + word1 + "\\b");
            var regEx2 = new Regex("\\b" + word2 + "\\b");
            var regEx3 = new Regex("\\b" + word3 + "\\b");
            var regEx4 = new Regex("\\b" + word4 + "\\b");
            var regEx5 = new Regex("\\b" + word5 + "\\b");
            var regEx6 = new Regex("\\b" + word6 + "\\b");
            int count = regEx1.Matches(text).Count;
            count += regEx2.Matches(text).Count;
            count += regEx3.Matches(text).Count;
            count += regEx4.Matches(text).Count;
            count += regEx5.Matches(text).Count;
            count += regEx6.Matches(text).Count;
            return count;
        }

        public static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            int count = GetCount(text, "we", "are", "and", "you", "your", "what");
            if (count > bestCount)
                return "en";

            count = GetCount(text, "vi", "er", "og", "jeg", "var", "men");
            if (count > bestCount)
            {
                int norwegianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                if (norwegianCount < 2)
                    return "da";
            }

            count = GetCount(text, "vi", "er", "og", "jeg", "var", "men");
            if (count > bestCount)
            {
                int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "meget", "spørge");
                if (danishCount < 2)
                    return "no";
            }

            count = GetCount(text, "vi", "är", "och", "Jag", "inte", "för");
            if (count > bestCount)
                return "sv";

            count = GetCount(text, "el", "bien", "Vamos", "Hola", "casa", "con");
            if (count > bestCount)
            {
                int frenchWords = GetCount(text, "Cest", "cest", "pas", "vous", "pour", "suis"); // not spanish words
                if (frenchWords < 2)
                    return "es";
            }

            count = GetCount(text, "un", "vous", "avec", "pas", "ce", "une");
            if (count > bestCount)
            {
                int spanishWords = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                int italianWords = GetCount(text, "Cosa", "sono", "Grazie", "Buongiorno", "bene", "questo");

                if (spanishWords < 2 && italianWords < 2)
                    return "fr";
            }

            count = GetCount(text, "und", "auch", "sich", "bin", "hast", "möchte");
            if (count > bestCount)
                return "de";

            count = GetCount(text, "van", "het", "een", "Het", "mij", "zijn");
            if (count > bestCount)
                return "nl";

            count = GetCount(text, "Czy", "ale", "ty", "siê", "jest", "mnie");
            if (count > bestCount)
                return "pl";

            count = GetCount(text, "Cosa", "sono", "Grazie", "Buongiorno", "bene", "questo");
            if (count > bestCount)
            {
                int frenchWords = GetCount(text, "Cest", "cest", "pas", "vous", "pour", "suis"); // not spanish words
                int spanishWords = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                if (frenchWords < 2 && spanishWords < 2)
                    return "it";
            }

            count = GetCount(text, "não", "Não", "Estás", "Então", "isso", "com");
            if (count > bestCount)
                return "pt-PT"; // PORTUGUESE

            count = GetCount(text, "μου", "είναι", "Είναι", "αυτό", "Τόμπυ", "καλά");
            if (count > bestCount)
                return "el"; // Greek

            count = GetCount(text, "все", "это", "как", "Воробей", "сюда", "Давай");
            if (count > bestCount)
                return "ru"; // Russian

            count = GetCount(text, "sam", "öto", "äto", "ovo", "vas", "što");
            if (count > bestCount && GetCount(text, "htjeti ", "htjeti ", "htjeti ", "htjeti ", "htjeti ", "htjeti ") > 0)
                return "hr"; // Croatia

            count = GetCount(text, "من", "هل", "لا", "فى", "لقد", "ما");
            if (count > bestCount)
            {
                if (GetCount(text, "אולי", "אולי", "אולי", "אולי", "טוב", "טוב") > 10)
                    return "he";
                return "ar"; // Arabic
            }

            count = GetCount(text, "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב");
            if (count > bestCount)
                return "he"; // Hebrew

            count = GetCount(text, "sam", "što", "nije", "Šta", "ovde", "za");
            if (count > bestCount)
                return "sr"; // Serbian

            count = GetCount(text, "không", "tôi", "anh", "đó", "Tôi", "ông");
            if (count > bestCount)
                return "vi"; // Vietnamese

            count = GetCount(text, "是的", "是啊", "好吧", "好的", "亲爱的", "早上好");
            if (count > bestCount)
                return "zh"; // Chinese (simplified) - not tested...

            count = GetCount(text, "hogy", "lesz", "tudom", "vagy", "mondtam", "még");
            if (count > bestCount)
                return "hu"; // Hungarian

            return string.Empty;
        }

        public static string AutoDetectGoogleLanguage(Subtitle subtitle)
        {
            int bestCount = subtitle.Paragraphs.Count / 14;

            StringBuilder sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
                sb.AppendLine(p.Text);
            string text = sb.ToString();

            string languageId = AutoDetectGoogleLanguage(text, bestCount);

            if (string.IsNullOrEmpty(languageId))
                return "en";

            return languageId;
        }

        public static string AutoDetectLanguageName(string languageName, Subtitle subtitle)
        {
            if (string.IsNullOrEmpty(languageName))
                languageName = "en_US";
            int bestCount = subtitle.Paragraphs.Count / 14;

            StringBuilder sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
                sb.AppendLine(p.Text);
            string text = sb.ToString();

            List<string> dictionaryNames = GetDictionaryLanguages();

            bool containsEnGB = false;
            bool containsEnUS = false;
            foreach (string name in dictionaryNames)
            {
                if (name.Contains("[en_GB]"))
                    containsEnGB = true;
                if (name.Contains("[en_US]"))
                    containsEnUS = true;
            }

            foreach (string name in dictionaryNames)
            {
                string shortName = string.Empty;
                int start = name.IndexOf("[");
                int end = name.IndexOf("]");
                if (start > 0 && end > start)
                {
                    start++;
                    shortName = name.Substring(start, end - start);
                }

                int count;
                switch (shortName)
                {
                    case "da_DK":
                        count = GetCount(text, "vi", "er", "og", "jeg", "var", "men");
                        if (count > bestCount)
                        {
                            int norweigianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                            if (norweigianCount < 2)
                                languageName = shortName;
                        }
                        break;
                    case "nb_NO":
                        count = GetCount(text, "vi", "er", "og", "jeg", "var", "men");
                        if (count > bestCount)
                        {
                            int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "meget", "spørge");
                            if (danishCount < 2)
                                languageName = shortName;
                        }
                        break;
                    case "en_US":
                        count = GetCount(text, "we", "are", "and", "you", "your", "what");
                        if (count > bestCount)
                        {
                            if (!string.IsNullOrEmpty(languageName) && languageName.StartsWith("en_"))
                            {
                                //keep existing english language
                            }
                            else
                            {
                                if (containsEnGB)
                                {
                                    int usCount = GetCount("color", "flavor", "ass", "humor", "neighbor", "honor", "airplane");
                                    int gbCount = GetCount("colour", "flavour", "arse", "humour", "neighbour", "honour", "aeroplane");
                                    if (usCount >= gbCount)
                                        languageName = shortName;
                                    else
                                        languageName = "en_GB";
                                }
                                else
                                {
                                    languageName = shortName;
                                }
                            }
                        }
                        break;
                    case "en_GB":
                        count = GetCount(text, "we", "are", "and", "you", "your", "what");
                        if (count > bestCount)
                        {
                            if (!string.IsNullOrEmpty(languageName) && languageName.StartsWith("en_"))
                            {
                                //keep existing english language
                            }
                            else
                            {
                                if (containsEnUS)
                                {
                                    int usCount = GetCount("color", "flavor", "ass", "humor", "neighbor", "honor", "airplane");
                                    int gbCount = GetCount("colour", "flavour", "arse", "humour", "neighbour", "honour", "aeroplane");
                                    if (usCount >= gbCount)
                                        languageName = "en_US";
                                    else
                                        languageName = "en_GB";
                                }
                                else
                                {
                                    languageName = shortName;
                                }
                            }
                        }
                        break;
                    case "sv_SE":
                        count = GetCount(text, "vi", "är", "och", "Jag", "inte", "för");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "es_ES":
                        count = GetCount(text, "el", "bien", "Vamos", "Hola", "casa", "con");
                        if (count > bestCount)
                        {
                            int frenchWords = GetCount(text, "Cest", "cest", "pas", "vous", "pour", "suis"); // not spanish words
                            if (frenchWords < 2)
                                languageName = shortName;
                        }
                        break;
                    case "fr_FR":
                        count = GetCount(text, "un", "vous", "avec", "pas", "ce", "une");
                        if (count > bestCount)
                        {
                            int spanishWords = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                            int italianWords = GetCount(text, "Cosa", "sono", "Grazie", "Buongiorno", "bene", "questo"); // not italian words
                            if (spanishWords < 2 && italianWords < 2)
                                languageName = shortName;
                        }
                        break;
                    case "it_IT":
                        count = GetCount(text, "Cosa", "sono", "Grazie", "Buongiorno", "bene", "questo");
                        if (count > bestCount)
                        {
                            int frenchWords = GetCount(text, "Cest", "cest", "pas", "vous", "pour", "suis"); // not spanish words
                            int spanishWords = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                            if (frenchWords < 2 && spanishWords < 2)
                                languageName = shortName;
                        }
                        break;
                    case "de_DE":
                        count = GetCount(text, "und", "auch", "sich", "bin", "hast", "möchte");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "nl_NL":
                        count = GetCount(text, "van", "het", "een", "Het", "mij", "zijn");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "pl_PL":
                        count = GetCount(text, "Czy", "ale", "ty", "siê", "jest", "mnie");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "el_GR":
                        count = GetCount(text, "μου", "είναι", "Είναι", "αυτό", "Τόμπυ", "καλά");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "ru_RU":
                        count = GetCount(text, "все", "это", "как", "Воробей", "сюда", "Давай");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "ro_RO":
                        count = GetCount(text, "sunt", "fost", "Bine", "Haide", "Trebuie", "trebuie");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "hr_HR": // Croatia
                        count = GetCount(text, "sam", "öto", "äto", "ovo", "vas", "što");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "pt_PT": // Portuguese
                        count = GetCount(text, "não", "Não", "Estás", "Então", "isso", "com");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "pt_BR": // Portuguese (Brasil)
                        count = GetCount(text, "não", "Não", "Estás", "Então", "isso", "com");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "hu_HU": // Hungarian
                        count = GetCount(text, "hogy", "lesz", "tudom", "vagy", "mondtam", "még");
                        if (count > bestCount)
                            languageName = shortName;
                        break;

                    default:
                        break;
                }
            }
            return languageName;
        }

        public static string ColorToHex(Color c)
        {
            string result = string.Format("#{0:x2}{1:x2}{2:x2}", c.R, c.G, c.B);
            return result;
        }

        public static int GetMaxLineLength(string text)
        {
            int maxLength = 0;
            foreach (string line in text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                string s = RemoveHtmlTags(line);
                if (s.Length > maxLength)
                    maxLength = s.Length;
            }
            return maxLength;
        }

        public static double GetCharactersPerSecond(Paragraph paragraph)
        {
            if (paragraph.Duration.TotalMilliseconds < 1)
                return 999;

            const string zeroWhiteSpace = "\u200B";
            const string zeroWidthNoBreakSpace = "\uFEFF";

            string s = Utilities.RemoveHtmlTags(paragraph.Text).Replace(Environment.NewLine, string.Empty).Replace(zeroWhiteSpace, string.Empty).Replace(zeroWidthNoBreakSpace, string.Empty);
            return s.Length / paragraph.Duration.TotalSeconds;
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        public static bool IsRunningOnLinux()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix;
        }

        public static bool IsRunningOnMac()
        {
            return Environment.OSVersion.Platform == PlatformID.MacOSX;
        }

        public static void ShowHelp(string parameter)
        {
            string helpFile = Configuration.Settings.Language.General.HelpFile;
            if (string.IsNullOrEmpty(helpFile))
                helpFile = "http://www.nikse.dk/SubtitleEdit/Help";
            System.Diagnostics.Process.Start(helpFile + parameter);
        }

        public static string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static string GetOpenDialogFilter()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Configuration.Settings.Language.General.SubtitleFiles + "|");
            foreach (SubtitleFormat s in SubtitleFormat.AllSubtitleFormats)
            {
                if (!sb.ToString().Contains("*" + s.Extension + ";"))
                    sb.Append("*" + s.Extension + ";");
                foreach (string ext in s.AlternateExtensions)
                {
                    if (!sb.ToString().Contains("*" + ext + ";"))
                        sb.Append("*" + ext + ";");
                }
            }
            sb.Append("*" + new Pac().Extension + ";");
            sb.Append("*" + new Cavena890().Extension + ";");
            sb.Append("*" + new Spt().Extension + ";");
            sb.Append("*" + new Wsb().Extension + ";");
            sb.Append("*.mp4;"); // mp4 video files (can contain subtitles)
            sb.Append("*.m4v;"); // mp4 video files (can contain subtitles)
            sb.Append("*.mkv;"); // matroska files (can contain subtitles)
            sb.Append("*.sup;"); // blu-ray sup
            sb.Append("*.son"); // SON text/tif
            sb.Append("|" + Configuration.Settings.Language.General.AllFiles + "|*.*");
            return sb.ToString();
        }

        public static void SetSaveDialogFilter(SaveFileDialog saveFileDialog, SubtitleFormat currentFormat)
        {
            var sb = new StringBuilder();
            int index = 0;
            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
            {
                sb.Append(format.Name + "|*" + format.Extension + "|");
                if (currentFormat.Name == format.Name)
                    saveFileDialog.FilterIndex = index + 1;
                index++;
            }
            saveFileDialog.Filter = sb.ToString().TrimEnd('|');
        }

        public static bool IsQuartsDllInstalled
        {
            get
            {
                if (IsRunningOnMono())
                    return false;

                string quartsInteropFileName = Path.GetDirectoryName(Application.ExecutablePath).TrimEnd('\\') + @"\Interop.QuartzTypeLib.dll";
                if (!File.Exists(quartsInteropFileName))
                    return false;

                string quartzFileName = Environment.GetFolderPath(Environment.SpecialFolder.System).TrimEnd('\\') + @"\quartz.dll";
                return File.Exists(quartzFileName);
            }
        }

        public static bool IsManagedDirectXInstalled
        {
            get
            {
                if (IsRunningOnMono())
                    return false;

                try
                {
                    //Check if this folder exists: C:\WINDOWS\Microsoft.NET\DirectX for Managed Code
                    string folderName = Environment.SystemDirectory.TrimEnd('\\');
                    folderName = folderName.Substring(0, folderName.LastIndexOf('\\'));
                    folderName = folderName + @"\\Microsoft.NET\DirectX for Managed Code";
                    return Directory.Exists(folderName);
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
            }
        }

        public static bool IsMPlayerAvailable
        {
            get
            {
                if (IsRunningOnMono() || IsRunningOnLinux())
                    return File.Exists(Path.Combine(Configuration.BaseDirectory, "mplayer"));

                return MPlayer.GetMPlayerFileName != null;
            }
        }

        public static VideoPlayer GetVideoPlayer()
        {
            GeneralSettings gs = Configuration.Settings.General;

            if (Utilities.IsRunningOnLinux() || Utilities.IsRunningOnMac())
                return new MPlayer();

            //if (Utilities.IsRunningOnMac())
            //    return new LibVlcMono();

            if (gs.VideoPlayer == "VLC" && LibVlc11xDynamic.IsInstalled)
                return new LibVlc11xDynamic();

            //if (gs.VideoPlayer == "WindowsMediaPlayer" && IsWmpAvailable)
            //    return new WmpPlayer();
            //if (gs.VideoPlayer == "ManagedDirectX" && IsManagedDirectXInstalled)
            //    return new ManagedDirectXPlayer();

            if (gs.VideoPlayer == "MPlayer" && MPlayer.IsInstalled)
                return new MPlayer();

            if (IsQuartsDllInstalled)
                return new QuartsPlayer();
            //if (IsWmpAvailable)
            //    return new WmpPlayer();

            throw new NotSupportedException("You need DirectX, VLC media player 1.1.x, or MPlayer2 installed as well as Subtitle Edit dll files in order to use the video player!");
        }

        public static void InitializeVideoPlayerAndContainer(string fileName, VideoInfo videoInfo, VideoPlayerContainer videoPlayerContainer, EventHandler onVideoLoaded, EventHandler onVideoEnded)
        {
            try
            {
                videoPlayerContainer.VideoPlayer = GetVideoPlayer();
                videoPlayerContainer.VideoPlayer.Initialize(videoPlayerContainer.PanelPlayer, fileName, onVideoLoaded, onVideoEnded);
                videoPlayerContainer.ShowStopButton = Configuration.Settings.General.VideoPlayerShowStopButton;
                videoPlayerContainer.Volume = Configuration.Settings.General.VideoPlayerDefaultVolume;
                videoPlayerContainer.EnableMouseWheelStep();
                videoPlayerContainer.VideoWidth = videoInfo.Width;
                videoPlayerContainer.VideoHeight = videoInfo.Height;
            }
            catch (Exception exception)
            {
                videoPlayerContainer.VideoPlayer = null;
                var videoError = new VideoError();
                videoError.Initialize(fileName, videoInfo, exception);
                videoError.ShowDialog();
            }
        }

        internal static void GetLineLengths(Label label, string text)
        {
            label.ForeColor = Color.Black;
            string cleanText = Utilities.RemoveHtmlTags(text).Replace(Environment.NewLine, "|");
            string[] lines = cleanText.Split('|');

            const int max = 3;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (i > 0)
                {
                    sb.Append("/");
                }

                if (i > max)
                {
                    label.ForeColor = Color.Red;
                    sb.Append("...");
                    label.Text = sb.ToString();
                    return;
                }

                sb.Append(line.Length.ToString());
                if (line.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                    label.ForeColor = Color.Red;
                else if (line.Length > Configuration.Settings.General.SubtitleLineMaximumLength - 5)
                    label.ForeColor = Color.Orange;
            }
            label.Text = sb.ToString();
        }

        //internal static void DisplayLineLengths(Panel panelSingleLine, string text)
        //{
        //    string cleanText = RemoveHtmlTags(text).Replace(Environment.NewLine, "|");
        //    string[] lines = cleanText.Split('|');

        //    int position = 0;

        //    // we must dispose before clearing controls (or this will occur: "Error creating window handle")
        //    foreach (Control ctrl in panelSingleLine.Controls)
        //        ctrl.Dispose();
        //    panelSingleLine.Controls.Clear();

        //    for (int i = 0; i < lines.Length; i++)
        //    {
        //        string line = lines[i];
        //        if (i > 0)
        //        {
        //            var labelSlash = new Label {AutoSize = true, Margin = new Padding(0)};
        //            panelSingleLine.Controls.Add(labelSlash);
        //            labelSlash.Text = "/";
        //            labelSlash.Top = 0;
        //            labelSlash.Left = position;
        //            position += labelSlash.Width - 4;

        //        }
        //        var labelLength = new Label();
        //        labelLength.AutoSize = true;
        //        labelLength.Margin = new Padding(0);
        //        panelSingleLine.Controls.Add(labelLength);
        //        labelLength.Text = line.Length.ToString();
        //        labelLength.Top = 0;
        //        labelLength.Left = position;
        //        position += labelLength.Width - 4;
        //        if (line.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
        //            labelLength.ForeColor = Color.Red;
        //        else if (line.Length > Configuration.Settings.General.SubtitleLineMaximumLength - 5)
        //            labelLength.ForeColor = Color.Orange;
        //    }
        //}

        public static bool IsValidRegex(string testPattern)
        {
            if (!string.IsNullOrEmpty(testPattern))
            {
                try
                {
                    Regex.Match("", testPattern);
                    return true;
                }
                catch (ArgumentException)
                { // BAD PATTERN: Syntax error
                }
            }
            return false;
        }

        public static Regex MakeWordSearchRegex(string word)
        {
            string s = word.Replace("\\", "\\\\");
            s = s.Replace("*", "\\*");
            s = s.Replace(".", "\\.");
            s = s.Replace("?", "\\?");
            return new Regex(@"\b" + s + @"\b", RegexOptions.Compiled);
        }

        public static Regex MakeWordSearchRegexWithNumbers(string word)
        {
            string s = word.Replace("\\", "\\\\");
            s = s.Replace("*", "\\*");
            s = s.Replace(".", "\\.");
            s = s.Replace("?", "\\?");
            return new Regex(@"[\b ,\.\?\!]" + s + @"[\b !\.,\r\n\?]", RegexOptions.Compiled);
        }

        public static void AddToUserDictionary(string word, string languageName)
        {
            word = word.Trim();
            if (word.Length > 1)
            {
                string userWordsXmlFileName = DictionaryFolder + languageName + "_user.xml";
                var userWords = new XmlDocument();
                if (File.Exists(userWordsXmlFileName))
                    userWords.Load(userWordsXmlFileName);
                else
                    userWords.LoadXml("<words />");

                XmlNode node = userWords.CreateElement("word");
                node.InnerText = word;
                userWords.DocumentElement.AppendChild(node);
                userWords.Save(userWordsXmlFileName);
            }
        }

        public static bool AddWordToLocalNamesEtcList(string word, string languageName)
        {
            word = word.Trim();
            if (word.Length > 1)
            {
                var localNamesEtc = new List<string>();
                string userNamesEtcXmlFileName = LoadLocalNamesEtc(localNamesEtc, localNamesEtc, languageName);

                if (localNamesEtc.Contains(word))
                    return false;
                localNamesEtc.Add(word);
                localNamesEtc.Sort();

                var namesEtcDoc = new XmlDocument();
                if (File.Exists(userNamesEtcXmlFileName))
                    namesEtcDoc.Load(userNamesEtcXmlFileName);
                else
                    namesEtcDoc.LoadXml("<ignore_words />");

                XmlNode de = namesEtcDoc.DocumentElement;
                if (de != null)
                {
                    de.RemoveAll();
                    foreach (var name in localNamesEtc)
                    {
                        XmlNode node = namesEtcDoc.CreateElement("name");
                        node.InnerText = name;
                        de.AppendChild(node);
                    }
                    namesEtcDoc.Save(userNamesEtcXmlFileName);
                }
                return true;
            }
            return false;
        }

        public static string LoadNamesEtcWordLists(List<string> namesEtcList, List<string> namesEtcMultiWordList, string languageName)
        {
            namesEtcList.Clear();
            namesEtcMultiWordList.Clear();

            LoadGlobalNamesEtc(namesEtcList, namesEtcMultiWordList);

            string userNamesEtcXmlFileName = LoadLocalNamesEtc(namesEtcList, namesEtcMultiWordList, languageName);
            return userNamesEtcXmlFileName;
        }

        internal static void LoadGlobalNamesEtc(List<string> namesEtcList, List<string> namesEtcMultiWordList)
        {
            // Load names etc list (names/noise words)
            var namesEtcDoc = new XmlDocument();
            bool loaded = false;
            if (Configuration.Settings.WordLists.UseOnlineNamesEtc && !string.IsNullOrEmpty(Configuration.Settings.WordLists.NamesEtcUrl))
            {
                try
                {
                    string xml = ReadTextFileViaUrlAndProxyIfAvailable(Configuration.Settings.WordLists.NamesEtcUrl);
                    namesEtcDoc.LoadXml(xml);
                    loaded = true;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
                }
            }
            if (!loaded && File.Exists(DictionaryFolder + "names_etc.xml"))
            {
                namesEtcDoc.Load(DictionaryFolder + "names_etc.xml");
            }
            if (namesEtcDoc.DocumentElement != null)
                foreach (XmlNode node in namesEtcDoc.DocumentElement.SelectNodes("name"))
                {
                    string s = node.InnerText.Trim();
                    if (s.Contains(" "))
                    {
                        if (!namesEtcMultiWordList.Contains(s))
                            namesEtcMultiWordList.Add(s);
                    }
                    else
                    {
                        if (!namesEtcList.Contains(s))
                            namesEtcList.Add(s);
                    }
                }
        }

        internal static string LoadLocalNamesEtc(List<string> namesEtcList, List<string> namesEtcMultiWordList, string languageName)
        {
            string userNamesEtcXmlFileName = DictionaryFolder + languageName + "_names_etc.xml";
            if (languageName.Length == 2)
            {
                string[] files = Directory.GetFiles(DictionaryFolder, languageName + "_??_names_etc.xml");
                if (files.Length > 0)
                    userNamesEtcXmlFileName = files[0];
            }

            if (File.Exists(userNamesEtcXmlFileName))
            {
                var namesEtcDoc = new XmlDocument();
                namesEtcDoc.Load(userNamesEtcXmlFileName);
                foreach (XmlNode node in namesEtcDoc.DocumentElement.SelectNodes("name"))
                {
                    string s = node.InnerText.Trim();
                    if (s.Contains(" "))
                    {
                        if (!namesEtcMultiWordList.Contains(s))
                            namesEtcMultiWordList.Add(s);
                    }
                    else
                    {
                        if (!namesEtcList.Contains(s))
                            namesEtcList.Add(s);
                    }
                }
            }
            return userNamesEtcXmlFileName;
        }

        internal static bool IsInNamesEtcMultiWordList(List<string> namesEtcMultiWordList, string line, string word)
        {
            string text = line.Replace(Environment.NewLine, " ");
            text = text.Replace("  ", " ");

            foreach (string s in namesEtcMultiWordList)
            {
                if (s.Contains(word) && text.Contains(s))
                {
                    if (s.StartsWith(word + " ") || s.EndsWith(" " + word) || s.Contains(" " + word + " "))
                        return true;
                }
            }
            return false;
        }

        public static string LoadUserWordList(List<string> userWordList, string languageName)
        {
            userWordList.Clear();
            var userWordDictionary = new XmlDocument();
            string userWordListXmlFileName = DictionaryFolder + languageName + "_user.xml";
            if (File.Exists(userWordListXmlFileName))
            {
                userWordDictionary.Load(userWordListXmlFileName);
                foreach (XmlNode node in userWordDictionary.DocumentElement.SelectNodes("word"))
                {
                    string s = node.InnerText.ToLower();
                    if (!userWordList.Contains(s))
                        userWordList.Add(s);
                }
            }
            return userWordListXmlFileName;
        }

        public static string GetLetters(bool uppercase, bool lowercase, bool numbers)
        {
            string s = string.Empty;

            if (uppercase)
                s += Configuration.Settings.General.UppercaseLetters;

            if (lowercase)
                s += Configuration.Settings.General.UppercaseLetters.ToLower();

            if (numbers)
                s += "0123456789";

            return s;
        }

        internal static Color GetColorFromUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return Color.Pink;

            byte[] buffer = Encoding.UTF8.GetBytes(userName);
            long number = 0;
            foreach (byte b in buffer)
                number += b;

            switch (number % 20)
            {
                case 0: return Color.Red;
                case 1: return Color.Blue;
                case 2: return Color.Green;
                case 3: return Color.DarkCyan;
                case 4: return Color.DarkGreen;
                case 5: return Color.DarkBlue;
                case 6: return Color.DarkTurquoise;
                case 7: return Color.DarkViolet;
                case 8: return Color.DeepPink;
                case 9: return Color.DodgerBlue;
                case 10: return Color.ForestGreen;
                case 11: return Color.Fuchsia;
                case 12: return Color.DarkOrange;
                case 13: return Color.GreenYellow;
                case 14: return Color.IndianRed;
                case 15: return Color.Indigo;
                case 16: return Color.LawnGreen;
                case 17: return Color.LightBlue;
                case 18: return Color.DarkGoldenrod;
                case 19: return Color.Magenta;
                case 20: return Color.Maroon;
                default:
                    return Color.Black;
            }
        }

        internal static int GetNumber0To7FromUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return 0;

            byte[] buffer = Encoding.UTF8.GetBytes(userName);
            long number = 0;
            foreach (byte b in buffer)
                number += b;

            return (int)(number % 8);
        }

        internal static string GetRegExGroup(string regEx)
        {
            int start = regEx.IndexOf("(?<");
            if (start >= 0 && regEx.IndexOf(")", start) > start)
            {
                int end = regEx.IndexOf(">", start);
                if (end > start)
                {
                    start += 3;
                    string group = regEx.Substring(start, end - start);
                    return group;
                }
            }
            return null;
        }

        internal static string LowerCaseVowels
        {
            get
            {
                return "aeiouyæøåéóáôèòæøåäöïɤəɛʊʉɨ";
            }
        }

        internal static void SetButtonHeight(Control control, int newHeight, int level)
        {
            if (level > 6)
                return;

            if (control.HasChildren)
            {
                foreach (Control subControl in control.Controls)
                {
                    if (subControl.HasChildren)
                        SetButtonHeight(subControl, newHeight, level++);
                    else if (subControl is Button)
                        subControl.Height = newHeight;
                }
            }
            else if (control is Button)
                control.Height = newHeight;
        }

        internal static int CountTagInText(string text, string tag)
        {
            int count = 0;
            int index = text.IndexOf(tag);
            while (index >= 0)
            {
                count++;
                index = text.IndexOf(tag, index + 1);
            }
            return count;
        }

        internal static string FixInvalidItalicTags(string text)
        {
            const string beginTag = "<i>";
            const string endTag = "</i>";

            text = text.Replace("< i>", beginTag);
            text = text.Replace("<i >", beginTag);
            text = text.Replace("< I>", beginTag);
            text = text.Replace("<I >", beginTag);

            text = text.Replace("< /i>", endTag);
            text = text.Replace("</ i>", endTag);
            text = text.Replace("< /i>", endTag);
            text = text.Replace("< /I>", endTag);
            text = text.Replace("</ I>", endTag);
            text = text.Replace("< /I>", endTag);

            if (text.Contains(beginTag))
                text = text.Replace("<i/>", endTag);
            else
                text = text.Replace("<i/>", string.Empty);

            text = text.Replace(beginTag + beginTag, beginTag);
            text = text.Replace(endTag + endTag, endTag);

            int italicBeginTagCount = CountTagInText(text, beginTag);
            int italicEndTagCount = CountTagInText(text, endTag);
            int noOfLines = CountTagInText(text, Environment.NewLine) + 1;
            if (italicBeginTagCount + italicEndTagCount > 0)
            {
                if (italicBeginTagCount == 1 && italicEndTagCount == 1)
                {
                    if (text.IndexOf(beginTag) > text.IndexOf(endTag))
                    {
                        text = text.Replace(beginTag, "___________@");
                        text = text.Replace(endTag, beginTag);
                        text = text.Replace("___________@", endTag);
                    }
                }

                if (italicBeginTagCount == 2 && italicEndTagCount == 0)
                {
                    int firstIndex = text.IndexOf(beginTag);
                    int lastIndex = text.LastIndexOf(beginTag);
                    int lastIndexWithNewLine = text.LastIndexOf(Environment.NewLine + beginTag) + Environment.NewLine.Length;
                    if (noOfLines == 2 && lastIndex == lastIndexWithNewLine && firstIndex < 2)
                        text = text.Replace(Environment.NewLine, "</i>" + Environment.NewLine) + "</i>";
                    else if (text.Length > lastIndex + endTag.Length)
                        text = text.Substring(0, lastIndex) + endTag + text.Substring(lastIndex - 1 + endTag.Length);
                    else
                        text = text.Substring(0, lastIndex) + endTag;
                }

                if (italicBeginTagCount == 1 && italicEndTagCount == 2)
                {
                    int firstIndex = text.IndexOf(endTag);
                    if (text.StartsWith("</i>-<i>-"))
                        text = text.Remove(0, 5);
                    else if (text.StartsWith("</i>- <i>-"))
                        text = text.Remove(0, 5);
                    else if (text.StartsWith("</i>- <i> -"))
                        text = text.Remove(0, 5);
                    else if (text.StartsWith("</i>-<i> -"))
                        text = text.Remove(0, 5);
                    else if (firstIndex == 0)
                        text = text.Remove(0, 4);
                    else
                        text = text.Substring(0, firstIndex - 1) + text.Substring(firstIndex + endTag.Length);
                }

                if (italicBeginTagCount == 2 && italicEndTagCount == 1)
                {
                    int lastIndex = text.LastIndexOf(beginTag);
                    if (text.Length > lastIndex + endTag.Length)
                        text = text.Substring(0, lastIndex) + text.Substring(lastIndex - 1 + endTag.Length);
                    else
                        text = text.Substring(0, lastIndex - 1) + endTag;
                }

                if (italicBeginTagCount == 1 && italicEndTagCount == 0)
                {
                    int lastIndexWithNewLine = text.LastIndexOf(Environment.NewLine + beginTag) + Environment.NewLine.Length;
                    int lastIndex = text.LastIndexOf(beginTag);

                    if (text.StartsWith(beginTag))
                        text += endTag;
                    else if (noOfLines == 2 && lastIndex == lastIndexWithNewLine)
                        text += endTag;
                    else
                        text = text.Replace(beginTag, string.Empty);
                }

                if (italicBeginTagCount == 0 && italicEndTagCount == 1)
                {
                    text = text.Replace(endTag, string.Empty);
                }

                text = text.Replace("<i></i>", string.Empty);
                text = text.Replace("<i> </i>", string.Empty);
                text = text.Replace("<i>  </i>", string.Empty);
            }
            return text;
        }

        internal static Paragraph GetOriginalParagraph(int index, Paragraph paragraph, List<Paragraph> originalParagraphs)
        {
            if (index < 0)
                return null;

            if (index < originalParagraphs.Count && Math.Abs(originalParagraphs[index].StartTime.TotalMilliseconds - paragraph.StartTime.TotalMilliseconds) < 50)
                return originalParagraphs[index];

            foreach (Paragraph p in originalParagraphs)
            {
                if (p.StartTime.TotalMilliseconds == paragraph.StartTime.TotalMilliseconds)
                    return p;
            }

            foreach (Paragraph p in originalParagraphs)
            {
                if (p.StartTime.TotalMilliseconds > paragraph.StartTime.TotalMilliseconds - 200 &&
                    p.StartTime.TotalMilliseconds < paragraph.StartTime.TotalMilliseconds + 1000)
                    return p;
            }
            return null;
        }

        /// <summary>
        /// HTML-encodes a string
        /// </summary>
        /// <param name="text">Text string to encode</param>
        /// <returns>HTML-encoded text</returns>
        internal static string HtmlEncode(string text)
        {
            if (text == null)
                return string.Empty;
            StringBuilder sb = new StringBuilder(text.Length);
            int len = text.Length;
            for (int i = 0; i < len; i++)
            {
                switch (text[i])
                {
                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    case '"':
                        sb.Append("&quot;");
                        break;
                    case '&':
                        sb.Append("&amp;");
                        break;
                    default:
                        if (text[i] > 127)
                          sb.Append("&#" + (int)text[i] + ";");
                        else
                            sb.Append(text[i]);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// HTML-decodes a string
        /// </summary>
        /// <param name="text">Text string to encode</param>
        /// <returns>HTML-decoded text</returns>
        internal static string HtmlDecode(string text)
        {
            if (text == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder(text.Length);

            int len = text.Length;
            int i = 0;
            while (i < len)
            {
                char c = text[i];
                int nextSemiColon = text.IndexOf(';', i+1);
                if (c == '&' && nextSemiColon > 0 && nextSemiColon <= i + 8)
                {
                    string code = text.Substring(i + 1, nextSemiColon - (i+1));
                    i += code.Length + 2;
                    switch (code) // http://www.html-entities.com/   + http://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references
                    {
                        case "lt":
                            sb.Append('<');
                            break;
                        case "gt":
                            sb.Append('>');
                            break;
                        case "quot":
                            sb.Append('"');
                            break;
                        case "amp":
                            sb.Append('&');
                            break;
                        case "apos":
                            sb.Append("'");
                            break;
                        case "nbsp":
                            sb.Append(" ");
                            break;
                        case "ndash":
                            sb.Append("–");
                            break;
                        case "mdash":
                            sb.Append("—");
                            break;
                        case "iexcl":
                            sb.Append("¡");
                            break;
                        case "iquest":
                            sb.Append("¿");
                            break;
                        case "ldquo":
                            sb.Append("“");
                            break;
                        case "rdquo":
                            sb.Append("”");
                            break;
                        case "&lsquo;":
                            sb.Append("‘");
                            break;
                        case "rsquo":
                            sb.Append("’");
                            break;
                        case "laquo":
                            sb.Append("«");
                            break;
                        case "raquo":
                            sb.Append("»");
                            break;
                        case "cent":
                            sb.Append("¢");
                            break;
                        case "copy":
                            sb.Append("©");
                            break;
                        case "divide":
                            sb.Append("÷");
                            break;
                        case "micro":
                            sb.Append("µ");
                            break;
                        case "middot":
                            sb.Append("·");
                            break;
                        case "para":
                            sb.Append("¶");
                            break;
                        case "plusmn":
                            sb.Append("±");
                            break;
                        case "euro":
                            sb.Append("€");
                            break;
                        case "pound":
                            sb.Append("£");
                            break;
                        case "reg":
                            sb.Append("®");
                            break;
                        case "sect":
                            sb.Append("§");
                            break;
                        case "trade":
                            sb.Append("™");
                            break;
                        case "yen":
                            sb.Append("¥");
                            break;
                        case "aacute":
                            sb.Append("á ");
                            break;
                        case "Aacute":
                            sb.Append("Á");
                            break;
                        case "agrave":
                            sb.Append("à");
                            break;
                        case "Agrave":
                            sb.Append("À");
                            break;
                        case "acirc":
                            sb.Append("â");
                            break;
                        case "Acirc":
                            sb.Append("Â");
                            break;
                        case "aring":
                            sb.Append("å ");
                            break;
                        case "Aring":
                            sb.Append("Å");
                            break;
                        case "atilde":
                            sb.Append("ã");
                            break;
                        case "Atilde":
                            sb.Append("Ã");
                            break;
                        case "auml":
                            sb.Append("ä");
                            break;
                        case "Auml":
                            sb.Append("Ä");
                            break;
                        case "aelig":
                            sb.Append("æ");
                            break;
                        case "AElig":
                            sb.Append("Æ");
                            break;
                        case "ccedil":
                            sb.Append("ç");
                            break;
                        case "Ccedil":
                            sb.Append("Ç");
                            break;
                        case "eacute":
                            sb.Append("é");
                            break;
                        case "Eacute":
                            sb.Append("É");
                            break;
                        case "egrave":
                            sb.Append("è");
                            break;
                        case "Egrave":
                            sb.Append("È");
                            break;
                        case "ecirc":
                            sb.Append("ê");
                            break;
                        case "Ecirc":
                            sb.Append("Ê");
                            break;
                        case "euml":
                            sb.Append("ë");
                            break;
                        case "Euml":
                            sb.Append("Ë");
                            break;
                        case "iacute":
                            sb.Append("í");
                            break;
                        case "Iacute":
                            sb.Append("Í");
                            break;
                        case "igrave":
                            sb.Append("ì");
                            break;
                        case "Igrave":
                            sb.Append("Ì");
                            break;
                        case "icirc":
                            sb.Append("î");
                            break;
                        case "Icirc":
                            sb.Append("Î");
                            break;
                        case "iuml":
                            sb.Append("iuml");
                            break;
                        case "Iuml":
                            sb.Append("Ï");
                            break;
                        case "ntilde":
                            sb.Append("ñ");
                            break;
                        case "Ntilde":
                            sb.Append("Ñ");
                            break;
                        case "oacute":
                            sb.Append("ó");
                            break;
                        case "Oacute":
                            sb.Append("Ó");
                            break;
                        case "ograve":
                            sb.Append("ò");
                            break;
                        case "Ograve":
                            sb.Append("Ò");
                            break;
                        case "ocirc":
                            sb.Append("ô");
                            break;
                        case "Ocirc":
                            sb.Append("Ô");
                            break;
                        case "oslash":
                            sb.Append("ø");
                            break;
                        case "Oslash":
                            sb.Append("Ø");
                            break;
                        case "otilde":
                            sb.Append("õ");
                            break;
                        case "Otilde":
                            sb.Append("Õ");
                            break;
                        case "ouml":
                            sb.Append("ö");
                            break;
                        case "Ouml":
                            sb.Append("Ö");
                            break;
                        case "szlig":
                            sb.Append("ß");
                            break;
                        case "uacute":
                            sb.Append("ú");
                            break;
                        case "Uacute":
                            sb.Append("Ú");
                            break;
                        case "ugrave":
                            sb.Append("ù");
                            break;
                        case "Ugrave":
                            sb.Append("Ù");
                            break;
                        case "ucirc":
                            sb.Append("û");
                            break;
                        case "Ucirc":
                            sb.Append("Û");
                            break;
                        case "uuml":
                            sb.Append("ü");
                            break;
                        case "Uuml":
                            sb.Append("Ü");
                            break;
                        case "yuml":
                            sb.Append("ÿ");
                            break;
                        case "":
                            sb.Append("");
                            break;
                        default:
                            code = code.TrimStart('#');
                            if (code.StartsWith("x") || code.StartsWith("X"))
                            {
                                code = code.TrimStart('x');
                                code = code.TrimStart('X');
                                try
                                {
                                    int value = Convert.ToInt32(code, 16);
                                    sb.Append(Convert.ToChar(value));
                                }
                                catch
                                {
                                }
                            }
                            else if (IsInteger(code))
                            {
                                sb.Append(Convert.ToChar(int.Parse(code)));
                            }
                            break;
                    }
                }
                else
                {
                    sb.Append(c);
                    i++;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// UrlEncodes a string without the requirement for System.Web
        /// </summary>
        public static string UrlEncode(string text)
        {
            return System.Uri.EscapeDataString(text);
        }

        /// <summary>
        /// UrlDecodes a string without requiring System.Web
        /// </summary>
        public static string UrlDecode(string text)
        {
            // pre-process for + sign space formatting since System.Uri doesn't handle it
            // plus literals are encoded as %2b normally so this should be safe
            text = text.Replace("+", " ");
            return System.Uri.UnescapeDataString(text);
        }

        internal static bool IsWordInUserPhrases(List<string> userPhraseList, int index, string[] words)
        {
            string current = words[index];
            string prev = "-";
            if (index > 0)
                prev = words[index-1];
            string next = "-";
            if (index < words.Length-1)
                next = words[index+1];
            foreach (string userPhrase in userPhraseList)
            {
                if (userPhrase == current + " " + next)
                    return true;
                if (userPhrase == prev + " " + current)
                    return true;
            }
            return false;
        }

        internal static void CheckAutoWrap(TextBox textBox, KeyEventArgs e, int numberOfNewLines)
        {
            if (e.Modifiers == Keys.None && e.KeyCode != Keys.Enter && numberOfNewLines < 1 && textBox.Text.Length >= Configuration.Settings.General.SubtitleLineMaximumLength)
            {
                if (Configuration.Settings.General.AutoWrapLineWhileTyping) // only if auto-break-setting is true
                {
                    string newText;
                    if (textBox.Text.Length > Configuration.Settings.General.SubtitleLineMaximumLength + 30)
                    {
                        newText = Utilities.AutoBreakLine(textBox.Text);
                    }
                    else
                    {
                        int lastSpace = textBox.Text.LastIndexOf(' ');
                        if (lastSpace > 0)
                            newText = textBox.Text.Remove(lastSpace, 1).Insert(lastSpace, Environment.NewLine);
                        else
                            newText = textBox.Text;
                    }

                    int autobreakIndex = newText.IndexOf(Environment.NewLine);
                    if (autobreakIndex > 0)
                    {
                        int selectionStart = textBox.SelectionStart;
                        textBox.Text = newText;
                        if (selectionStart > autobreakIndex)
                            selectionStart += Environment.NewLine.Length-1;
                        if (selectionStart >= 0)
                            textBox.SelectionStart = selectionStart;
                    }
                }
            }
        }

        public static Keys GetKeys(string keysInString)
        {
            if (string.IsNullOrEmpty(keysInString))
                return Keys.None;

            string[] parts = keysInString.ToLower().Split("+".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            Keys resultKeys = Keys.None;
            foreach (string k in parts)
            {
                foreach (Keys val in Enum.GetValues(typeof(Keys)))
                {
                    if (k == val.ToString().ToLower())
                        resultKeys = resultKeys | val;
                }
            }

            return resultKeys;
        }

        public static string FixEnglishTextInRightToLeftLanguage(string text, string reverseChars)
        {
            var sb = new StringBuilder();
            string[] lines = text.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string s = line.Trim();
                for (int i = 0; i < s.Length; i++)
                {
                    if (s.Substring(i, 1) == ")")
                        s = s.Remove(i, 1).Insert(i, "(");
                    else if (s.Substring(i, 1) == "(")
                        s = s.Remove(i, 1).Insert(i, ")");
                }

                bool numbersOn = false;
                string numbers = string.Empty;
                for (int i = 0; i < s.Length; i++)
                {
                    if (numbersOn && reverseChars.Contains(s.Substring(i, 1)))
                    {
                        numbers = s.Substring(i, 1) + numbers;
                    }
                    else if (numbersOn)
                    {
                        numbersOn = false;
                        s = s.Remove(i - numbers.Length, numbers.Length).Insert(i - numbers.Length, numbers.ToString());
                        numbers = string.Empty;
                    }
                    else if (reverseChars.Contains(s.Substring(i, 1)))
                    {
                        numbers = s.Substring(i, 1) + numbers;
                        numbersOn = true;
                    }
                }
                if (numbersOn)
                {
                    int i = s.Length;
                    s = s.Remove(i - numbers.Length, numbers.Length).Insert(i - numbers.Length, numbers.ToString());
                    numbers = string.Empty;
                }

                sb.AppendLine(s);
            }
            return sb.ToString().Trim();
        }

    }
}