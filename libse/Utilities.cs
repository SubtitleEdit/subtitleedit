using Nikse.SubtitleEdit.Core.ContainerFormats;
using Nikse.SubtitleEdit.Core.ContainerFormats.Matroska;
using Nikse.SubtitleEdit.Core.ContainerFormats.Mp4;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Nikse.SubtitleEdit.Core
{
    public static class Utilities
    {
        public const string WinXP2KUnicodeFontName = "Times New Roman";

        /// <summary>
        /// Cached environment new line characters for faster lookup.
        /// </summary>
        public static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();

        public static VideoInfo TryReadVideoInfoViaMatroskaHeader(string fileName)
        {
            var info = new VideoInfo { Success = false };

            MatroskaFile matroska = null;
            try
            {
                matroska = new MatroskaFile(fileName);
                if (matroska.IsValid)
                {
                    double frameRate;
                    int width;
                    int height;
                    double milliseconds;
                    string videoCodec;
                    matroska.GetInfo(out frameRate, out width, out height, out milliseconds, out videoCodec);

                    info.Width = width;
                    info.Height = height;
                    info.FramesPerSecond = frameRate;
                    info.Success = true;
                    info.TotalMilliseconds = milliseconds;
                    info.TotalSeconds = milliseconds / TimeCode.BaseUnit;
                    info.TotalFrames = info.TotalSeconds * frameRate;
                    info.VideoCodec = videoCodec;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (matroska != null)
                {
                    matroska.Dispose();
                }
            }

            return info;
        }

        public static VideoInfo TryReadVideoInfoViaAviHeader(string fileName)
        {
            var info = new VideoInfo { Success = false };

            try
            {
                using (var rp = new RiffParser())
                {
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
                        info.TotalSeconds = info.TotalMilliseconds / TimeCode.BaseUnit;
                        info.VideoCodec = dh.VideoHandler;
                        info.Success = true;
                    }
                }
            }
            catch
            {
            }
            return info;
        }

        public static VideoInfo TryReadVideoInfoViaMp4(string fileName)
        {
            var info = new VideoInfo { Success = false };

            try
            {
                var mp4Parser = new MP4Parser(fileName);
                if (mp4Parser.Moov != null && mp4Parser.VideoResolution.X > 0)
                {
                    info.Width = mp4Parser.VideoResolution.X;
                    info.Height = mp4Parser.VideoResolution.Y;
                    info.TotalMilliseconds = mp4Parser.Duration.TotalSeconds;
                    info.VideoCodec = "MP4";
                    info.FramesPerSecond = mp4Parser.FrameRate;
                    info.Success = true;
                }
            }
            catch
            {
            }
            return info;
        }

        public static List<string> GetMovieFileExtensions()
        {
            return new List<string> { ".avi", ".mkv", ".wmv", ".mpg", ".mpeg", ".divx", ".mp4", ".asf", ".flv", ".mov", ".m4v", ".vob", ".ogv", ".webm", ".ts", ".m2ts", ".avs", ".mxf" };
        }

        public static string GetVideoFileFilter(bool includeAudioFiles)
        {
            var sb = new StringBuilder();
            sb.Append(Configuration.Settings.Language.General.VideoFiles + "|");
            int i = 0;
            foreach (string extension in GetMovieFileExtensions())
            {
                if (i > 0)
                    sb.Append(';');
                sb.Append('*');
                sb.Append(extension);
                i++;
            }
            if (includeAudioFiles)
            {
                sb.Append('|');
                sb.Append(Configuration.Settings.Language.General.AudioFiles);
                sb.Append("|*.mp3;*.wav;*.wma;*.ogg;*.mpa;*.m4a;*.ape;*.aiff;*.flac;*.aac;*.mka");
            }
            sb.Append('|');
            sb.Append(Configuration.Settings.Language.General.AllFiles);
            sb.Append("|*.*");
            return sb.ToString();
        }

        public static bool IsInteger(string s)
        {
            int i;
            return int.TryParse(s, out i);
        }

        public static SubtitleFormat GetSubtitleFormatByFriendlyName(string friendlyName)
        {
            foreach (SubtitleFormat format in SubtitleFormat.AllSubtitleFormats)
            {
                if (format.FriendlyName == friendlyName || format.Name == friendlyName)
                    return format;
            }
            return null;
        }

        public static string FormatBytesToDisplayFileSize(long fileSize)
        {
            if (fileSize <= 1024)
                return string.Format("{0} bytes", fileSize);
            if (fileSize <= 1024 * 1024)
                return string.Format("{0} kb", fileSize / 1024);
            if (fileSize <= 1024 * 1024 * 1024)
                return string.Format("{0:0.0} mb", (float)fileSize / (1024 * 1024));
            return string.Format("{0:0.0} gb", (float)fileSize / (1024 * 1024 * 1024));
        }



        /// <summary>
        /// Downloads the requested resource as a <see cref="String"/> using the configured <see cref="WebProxy"/>.
        /// </summary>
        /// <param name="address">A <see cref="String"/> containing the URI to download.</param>
        /// <param name="encoding">Encoding for source text</param>
        /// <returns>A <see cref="String"/> containing the requested resource.</returns>
        public static string DownloadString(string address, Encoding encoding = null)
        {
            using (var wc = new WebClient())
            {
                wc.Proxy = GetProxy();
                if (encoding != null)
                    wc.Encoding = encoding;
                return wc.DownloadString(address).Trim();
            }
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
            if (string.IsNullOrWhiteSpace(s) || position + 1 >= s.Length)
                return false;

            if (position > 0 && @",.".Contains(s[position]))
            {
                return char.IsDigit(s[position - 1]) && char.IsDigit(s[position + 1]);
            }
            return false;
        }

        public static bool IsBetweenNumbers(string s, int position)
        {
            if (string.IsNullOrEmpty(s) || position < 1 || position + 2 > s.Length)
                return false;
            return char.IsDigit(s[position - 1]) && char.IsDigit(s[position + 1]);
        }

        public static string AutoBreakLine(string text, string language)
        {
            return AutoBreakLine(text, Configuration.Settings.General.SubtitleLineMaximumLength, Configuration.Settings.Tools.MergeLinesShorterThan, language);
        }

        public static string AutoBreakLine(string text)
        {
            return AutoBreakLine(text, string.Empty); // no language
        }

        private static bool CanBreak(string s, int index, string language)
        {
            char nextChar = ' ';
            if (index >= 0 && index < s.Length)
                nextChar = s[index];
            else
                return false;
            if (!"\r\n\t ".Contains(nextChar))
                return false;

            // Some words we don't like breaking after
            string s2 = s.Substring(0, index);
            if (Configuration.Settings.Tools.UseNoLineBreakAfter)
            {
                foreach (NoBreakAfterItem ending in NoBreakAfterList(language))
                {
                    if (ending.IsMatch(s2))
                        return false;
                }
            }

            if (s2.EndsWith("? -", StringComparison.Ordinal) || s2.EndsWith("! -", StringComparison.Ordinal) || s2.EndsWith(". -", StringComparison.Ordinal))
                return false;

            return true;
        }

        private static string _lastNoBreakAfterListLanguage;
        private static List<NoBreakAfterItem> _lastNoBreakAfterList = new List<NoBreakAfterItem>();
        private static IEnumerable<NoBreakAfterItem> NoBreakAfterList(string languageName)
        {
            if (string.IsNullOrEmpty(languageName))
                return new List<NoBreakAfterItem>();

            if (languageName == _lastNoBreakAfterListLanguage)
                return _lastNoBreakAfterList;

            _lastNoBreakAfterList = new List<NoBreakAfterItem>();

            //load words via xml
            string noBreakAfterFileName = DictionaryFolder + languageName + "_NoBreakAfterList.xml";
            var doc = new XmlDocument();
            if (File.Exists(noBreakAfterFileName))
            {
                doc.Load(noBreakAfterFileName);
                foreach (XmlNode node in doc.DocumentElement)
                {
                    if (!string.IsNullOrEmpty(node.InnerText))
                    {
                        if (node.Attributes["RegEx"] != null && node.Attributes["RegEx"].InnerText.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            Regex r = new Regex(node.InnerText, RegexOptions.Compiled);
                            _lastNoBreakAfterList.Add(new NoBreakAfterItem(r, node.InnerText));
                        }
                        else
                        {
                            _lastNoBreakAfterList.Add(new NoBreakAfterItem(node.InnerText));
                        }
                    }
                }
            }
            _lastNoBreakAfterListLanguage = languageName;

            return _lastNoBreakAfterList;
        }

        public static string AutoBreakLineMoreThanTwoLines(string text, int maximumLineLength, string language)
        {
            if (text == null || text.Length < 3)
                return text;

            string s = AutoBreakLine(text, 0, 0, language);

            var arr = s.SplitToLines();
            if ((arr.Length < 2 && arr[0].Length <= maximumLineLength) || (arr[0].Length <= maximumLineLength && arr[1].Length <= maximumLineLength))
                return s;

            s = RemoveLineBreaks(s);

            var htmlTags = new Dictionary<int, string>();
            var sb = new StringBuilder(s.Length);
            int six = 0;
            while (six < s.Length)
            {
                var letter = s[six];
                var tagFound = letter == '<' && (s.Substring(six).StartsWith("<font", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</font", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("<u", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</u", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("<b", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</b", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("<i", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</i", StringComparison.OrdinalIgnoreCase));
                int endIndex = -1;
                if (tagFound)
                    endIndex = s.IndexOf('>', six + 1);

                if (tagFound && endIndex > 0)
                {
                    string tag = s.Substring(six, endIndex - six + 1);
                    s = s.Remove(six, tag.Length);
                    if (htmlTags.ContainsKey(six))
                        htmlTags[six] = htmlTags[six] + tag;
                    else
                        htmlTags.Add(six, tag);
                }
                else
                {
                    sb.Append(letter);
                    six++;
                }
            }
            s = sb.ToString();

            var words = s.Split(' ');
            for (int numberOfLines = 3; numberOfLines < 9999; numberOfLines++)
            {
                int average = s.Length / numberOfLines + 1;
                for (int len = average; len < maximumLineLength; len++)
                {
                    List<int> list = SplitToX(words, numberOfLines, len);
                    bool allOk = true;
                    foreach (var lineLength in list)
                    {
                        if (lineLength > maximumLineLength)
                            allOk = false;
                    }
                    if (allOk)
                    {
                        int index = 0;
                        foreach (var item in list)
                        {
                            index += item;
                            htmlTags.Add(index, Environment.NewLine);
                        }
                        s = ReInsertHtmlTags(s, htmlTags);
                        s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
                        s = s.Replace(Environment.NewLine + " ", Environment.NewLine);
                        s = s.Replace(Environment.NewLine + "</i>", "</i>" + Environment.NewLine);
                        s = s.Replace(Environment.NewLine + "</b>", "</b>" + Environment.NewLine);
                        s = s.Replace(Environment.NewLine + "</u>", "</u>" + Environment.NewLine);
                        s = s.Replace(Environment.NewLine + "</font>", "</font>" + Environment.NewLine);
                        return s.TrimEnd();
                    }
                }
            }

            return text;
        }

        private static List<int> SplitToX(string[] words, int count, int average)
        {
            var list = new List<int>();
            int currentIdx = 0;
            int currentCount = 0;
            foreach (string word in words)
            {
                if (currentCount + word.Length + 3 > average && currentIdx < count)
                {
                    list.Add(currentCount);
                    currentIdx++;
                    currentCount = 0;
                }
                currentCount += word.Length + 1;
            }
            if (currentIdx < count)
                list.Add(currentCount);
            else
                list[list.Count - 1] += currentCount;
            return list;
        }

        public static string AutoBreakLine(string text, int maximumLength, int mergeLinesShorterThan, string language)
        {
            if (text == null || text.Length < 3)
                return text;

            // do not autobreak dialogs
            if (text.Contains('-') && text.Contains(Environment.NewLine))
            {
                var noTagLines = HtmlUtil.RemoveHtmlTags(text, true).SplitToLines();
                if (noTagLines.Length == 2)
                {
                    var arr0 = noTagLines[0].Trim().TrimEnd('"', '\'').TrimEnd();
                    if (arr0.StartsWith('-') && noTagLines[1].TrimStart().StartsWith('-') && arr0.Length > 1 && ".?!)]".Contains(arr0[arr0.Length - 1]) || arr0.EndsWith("--", StringComparison.Ordinal) || arr0.EndsWith('–'))
                        return text;
                }
            }

            string s = RemoveLineBreaks(text);
            string noTagText = HtmlUtil.RemoveHtmlTags(s, true);

            if (noTagText.Length < mergeLinesShorterThan)
            {
                var noTagLines = noTagText.SplitToLines();
                if (noTagLines.Length > 1)
                {
                    bool isDialog = true;
                    foreach (string line in noTagLines)
                    {
                        var noTagLine = line.TrimStart();
                        isDialog = isDialog && (noTagLine.StartsWith('-') || noTagLine.StartsWith('—'));
                    }
                    if (isDialog)
                    {
                        return text;
                    }
                }
                return s;
            }

            var htmlTags = new Dictionary<int, string>();
            var sb = new StringBuilder();
            int six = 0;
            while (six < s.Length)
            {
                var letter = s[six];
                var tagFound = letter == '<' && (s.Substring(six).StartsWith("<font", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</font", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("<u", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</u", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("<b", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</b", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("<i", StringComparison.OrdinalIgnoreCase)
                                                 || s.Substring(six).StartsWith("</i", StringComparison.OrdinalIgnoreCase));
                int endIndex = -1;
                if (tagFound)
                    endIndex = s.IndexOf('>', six + 1);

                if (tagFound && endIndex > 0)
                {
                    string tag = s.Substring(six, endIndex - six + 1);
                    s = s.Remove(six, tag.Length);
                    if (htmlTags.ContainsKey(six))
                        htmlTags[six] = htmlTags[six] + tag;
                    else
                        htmlTags.Add(six, tag);
                }
                else
                {
                    sb.Append(letter);
                    six++;
                }
            }
            s = sb.ToString();

            int splitPos = -1;
            int mid = s.Length / 2;

            // try to find " - " with uppercase letter after (dialog)
            if (s.Contains(" - "))
            {
                for (int j = 0; j <= (maximumLength / 2) + 5; j++)
                {
                    if (mid + j + 4 < s.Length)
                    {
                        if (s[mid + j] == '-' && s[mid + j + 1] == ' ' && s[mid + j - 1] == ' ')
                        {
                            string rest = s.Substring(mid + j + 1).TrimStart();
                            if (rest.Length > 0 && char.IsUpper(rest[0]))
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
                            if (rest.Length > 0 && char.IsUpper(rest[0]))
                            {
                                if (mid - j > 5 && s[mid - j - 1] == ' ' && @"!?.".Contains(s[mid - j - 2]))
                                {
                                    splitPos = mid - j;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (splitPos == maximumLength + 1 && s[maximumLength] != ' ') // only allow space for last char (as it does not count)
                splitPos = -1;

            if (splitPos < 0)
            {
                const string expectedChars1 = ".!?0123456789";
                const string expectedChars2 = ".!?";
                for (int j = 0; j < 15; j++)
                {
                    if (mid + j + 1 < s.Length && mid + j > 0)
                    {
                        if (expectedChars2.Contains(s[mid + j]) && !IsPartOfNumber(s, mid + j) && CanBreak(s, mid + j + 1, language))
                        {
                            splitPos = mid + j + 1;
                            if (expectedChars1.Contains(s[splitPos]))
                            { // do not break double/tripple end lines like "!!!" or "..."
                                splitPos++;
                                if (expectedChars1.Contains(s[mid + j + 1]))
                                    splitPos++;
                            }
                            break;
                        }
                        if (expectedChars2.Contains(s[mid - j]) && !IsPartOfNumber(s, mid - j) && CanBreak(s, mid - j, language))
                        {
                            splitPos = mid - j;
                            splitPos++;
                            break;
                        }
                    }
                }
            }

            if (splitPos > maximumLength) // too long first line
            {
                if (splitPos != maximumLength + 1 || s[maximumLength] != ' ') // allow for maxlength+1 char to be space (does not count)
                    splitPos = -1;
            }
            else if (splitPos >= 0 && s.Length - splitPos > maximumLength) // too long second line
            {
                splitPos = -1;
            }

            if (splitPos < 0)
            {
                const string expectedChars1 = ".!?, ";
                const string expectedChars2 = " .!?";
                const string expectedChars3 = ".!?";
                for (int j = 0; j < 25; j++)
                {
                    if (mid + j + 1 < s.Length && mid + j > 0)
                    {
                        if (expectedChars1.Contains(s[mid + j]) && !IsPartOfNumber(s, mid + j) && s.Length > mid + j + 2 && CanBreak(s, mid + j, language))
                        {
                            splitPos = mid + j;
                            if (expectedChars2.Contains(s[mid + j + 1]))
                            {
                                splitPos++;
                                if (expectedChars2.Contains(s[mid + j + 2]))
                                    splitPos++;
                            }
                            break;
                        }
                        if (expectedChars1.Contains(s[mid - j]) && !IsPartOfNumber(s, mid - j) && s.Length > mid + j + 2 && CanBreak(s, mid - j, language))
                        {
                            splitPos = mid - j;
                            if (expectedChars3.Contains(s[splitPos]))
                                splitPos--;
                            if (expectedChars3.Contains(s[splitPos]))
                                splitPos--;
                            if (expectedChars3.Contains(s[splitPos]))
                                splitPos--;
                            break;
                        }
                    }
                }
            }

            if (splitPos < 0)
            {
                splitPos = mid;
                s = s.Insert(mid - 1, Environment.NewLine);
                s = ReInsertHtmlTags(s, htmlTags);
                htmlTags = new Dictionary<int, string>();
                s = s.Replace(Environment.NewLine, "-");
            }
            if (splitPos < s.Length - 2)
                s = s.Substring(0, splitPos) + Environment.NewLine + s.Substring(splitPos);

            s = ReInsertHtmlTags(s, htmlTags);
            s = s.Replace(" " + Environment.NewLine, Environment.NewLine);
            s = s.Replace(Environment.NewLine + " ", Environment.NewLine);

            return s.TrimEnd();
        }

        public static string RemoveLineBreaks(string s)
        {
            s = HtmlUtil.FixUpperTags(s);
            s = s.Replace(Environment.NewLine + "</i>", "</i>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + "</b>", "</b>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + "</u>", "</u>" + Environment.NewLine);
            s = s.Replace(Environment.NewLine + "</font>", "</font>" + Environment.NewLine);
            s = s.Replace("</i> " + Environment.NewLine + "<i>", " ");
            s = s.Replace("</i>" + Environment.NewLine + " <i>", " ");
            s = s.Replace("</i>" + Environment.NewLine + "<i>", " ");
            s = s.Replace(Environment.NewLine, " ");
            s = s.Replace(" </i>", "</i> ");
            s = s.Replace(" </b>", "</b> ");
            s = s.Replace(" </u>", "</u> ");
            s = s.Replace(" </font>", "</font> ");
            s = s.FixExtraSpaces();
            return s.Trim();
        }

        private static string ReInsertHtmlTags(string s, Dictionary<int, string> htmlTags)
        {
            if (htmlTags.Count > 0)
            {
                var sb = new StringBuilder(s.Length);
                int six = 0;
                foreach (var letter in s)
                {
                    if (Environment.NewLine.Contains(letter))
                    {
                        sb.Append(letter);
                    }
                    else
                    {
                        if (htmlTags.ContainsKey(six))
                        {
                            sb.Append(htmlTags[six]);
                        }
                        sb.Append(letter);
                        six++;
                    }
                }
                if (htmlTags.ContainsKey(six))
                {
                    sb.Append(htmlTags[six]);
                }
                return sb.ToString();
            }
            return s;
        }

        public static string UnbreakLine(string text)
        {
            if (!text.Contains(Environment.NewLine))
                return text;

            var singleLine = text.Replace(Environment.NewLine, " ");
            while (singleLine.Contains("  "))
                singleLine = singleLine.Replace("  ", " ");

            if (singleLine.Contains("</")) // Fix tag
            {
                singleLine = singleLine.Replace("</i> <i>", " ");
                singleLine = singleLine.Replace("</i><i>", " ");

                singleLine = singleLine.Replace("</b> <b>", " ");
                singleLine = singleLine.Replace("</b><b>", " ");

                singleLine = singleLine.Replace("</u> <u>", " ");
                singleLine = singleLine.Replace("</u><u>", " ");
            }
            return singleLine;
        }

        public static void InitializeSubtitleFont(Control control)
        {
            var gs = Configuration.Settings.General;

            if (string.IsNullOrEmpty(gs.SubtitleFontName))
                gs.SubtitleFontName = "Tahoma";

            try
            {
                if (gs.SubtitleFontBold)
                    control.Font = new Font(gs.SubtitleFontName, gs.SubtitleFontSize, FontStyle.Bold);
                else
                    control.Font = new Font(gs.SubtitleFontName, gs.SubtitleFontSize);

                control.BackColor = gs.SubtitleBackgroundColor;
                control.ForeColor = gs.SubtitleFontColor;
            }
            catch
            {
            }
        }

        public static string RemoveSsaTags(string s)
        {
            int k = s.IndexOf('{');
            while (k >= 0)
            {
                int l = s.IndexOf('}', k);
                if (l > k)
                {
                    s = s.Remove(k, l - k + 1);
                    if (s.Length > 1 && s.Length > k)
                        k = s.IndexOf('{', k);
                    else
                        break;
                }
                else
                {
                    break;
                }
            }
            return s;
        }

        public static Encoding GetEncodingFromFile(string fileName)
        {
            Encoding encoding = Encoding.Default;

            try
            {
                foreach (EncodingInfo ei in Encoding.GetEncodings())
                {
                    if (ei.CodePage + ": " + ei.DisplayName == Configuration.Settings.General.DefaultEncoding &&
                        ei.Name != Encoding.UTF8.BodyName &&
                        ei.Name != Encoding.Unicode.BodyName)
                    {
                        encoding = ei.GetEncoding();
                        break;
                    }
                }

                using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
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
                    else if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76 && (bom[3] == 0x38 || bom[3] == 0x39 || bom[3] == 0x2b || bom[3] == 0x2f)) // utf-7
                        encoding = Encoding.UTF7;
                    else if (file.Length > 12)
                    {
                        long length = file.Length;
                        if (length > 500000)
                            length = 500000;

                        file.Position = 0;
                        var buffer = new byte[length];
                        file.Read(buffer, 0, (int)length);

                        bool couldBeUtf8;
                        if (IsUtf8(buffer, out couldBeUtf8))
                        {
                            encoding = Encoding.UTF8;
                        }
                        else if (couldBeUtf8 && Configuration.Settings.General.DefaultEncoding == Encoding.UTF8.BodyName)
                        { // keep utf-8 encoding if it's default
                            encoding = Encoding.UTF8;
                        }
                        else if (couldBeUtf8 && fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) && Encoding.Default.GetString(buffer).ToLower().Replace('\'', '"').Contains("encoding=\"utf-8\""))
                        { // keep utf-8 encoding for xml files with utf-8 in header (without any utf-8 encoded characters, but with only allowed utf-8 characters)
                            encoding = Encoding.UTF8;
                        }
                        else if (Configuration.Settings.General.AutoGuessAnsiEncoding)
                        {
                            encoding = DetectAnsiEncoding(buffer);

                            Encoding greekEncoding = Encoding.GetEncoding(1253); // Greek
                            if (GetCount(greekEncoding.GetString(buffer), AutoDetectWordsGreek) > 5)
                                return greekEncoding;

                            Encoding russianEncoding = Encoding.GetEncoding(1251); // Cyrillic
                            if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5) // Russian
                                return russianEncoding;
                            if (GetCount(russianEncoding.GetString(buffer), "Какво", "тук", "може", "Как", "Ваше", "какво") > 5) // Bulgarian
                                return russianEncoding;
                            russianEncoding = Encoding.GetEncoding(28595); // Russian
                            if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5)
                                return russianEncoding;

                            Encoding thaiEncoding = Encoding.GetEncoding(874); // Thai
                            if (GetCount(thaiEncoding.GetString(buffer), "โอ", "โรเบิร์ต", "วิตตอเรีย", "ดร", "คุณตำรวจ", "ราเชล") + GetCount(thaiEncoding.GetString(buffer), "ไม่", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์") > 5)
                                return thaiEncoding;

                            Encoding arabicEncoding = Encoding.GetEncoding(28596); // Arabic
                            Encoding hewbrewEncoding = Encoding.GetEncoding(28598); // Hebrew
                            if (GetCount(arabicEncoding.GetString(buffer), "من", "هل", "لا", "فى", "لقد", "ما") > 5)
                            {
                                if (GetCount(hewbrewEncoding.GetString(buffer), "אולי", "אולי", "אולי", "אולי", "טוב", "טוב") > 10)
                                    return hewbrewEncoding;
                                return arabicEncoding;
                            }
                            if (GetCount(hewbrewEncoding.GetString(buffer), AutoDetectWordsHebrew) > 5)
                                return hewbrewEncoding;

                            Encoding romanianEncoding = Encoding.GetEncoding(1250); // Romanian
                            if (GetCount(romanianEncoding.GetString(buffer), "să", "şi", "văzut", "regulă", "găsit", "viaţă") > 99)
                                return romanianEncoding;

                            Encoding koreanEncoding = Encoding.GetEncoding(949); // Korean
                            if (GetCount(koreanEncoding.GetString(buffer), "그리고", "아니야", "하지만", "말이야", "그들은", "우리가") > 5)
                                return koreanEncoding;
                        }
                    }
                }
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
                if (GetCount(greekEncoding.GetString(buffer), AutoDetectWordsGreek) > 5)
                    return greekEncoding;

                Encoding russianEncoding = Encoding.GetEncoding(1251); // Cyrillic
                if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5) // Russian
                    return russianEncoding;
                if (GetCount(russianEncoding.GetString(buffer), "Какво", "тук", "може", "Как", "Ваше", "какво") > 5) // Bulgarian
                    return russianEncoding;

                russianEncoding = Encoding.GetEncoding(28595); // Russian
                if (GetCount(russianEncoding.GetString(buffer), "что", "быть", "весь", "этот", "один", "такой") > 5) // Russian
                    return russianEncoding;

                Encoding thaiEncoding = Encoding.GetEncoding(874); // Thai
                if (GetCount(thaiEncoding.GetString(buffer), "โอ", "โรเบิร์ต", "วิตตอเรีย", "ดร", "คุณตำรวจ", "ราเชล") + GetCount(thaiEncoding.GetString(buffer), "ไม่", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์") > 5)
                    return thaiEncoding;

                Encoding arabicEncoding = Encoding.GetEncoding(28596); // Arabic
                Encoding hewbrewEncoding = Encoding.GetEncoding(28598); // Hebrew
                if (GetCount(arabicEncoding.GetString(buffer), "من", "هل", "لا", "فى", "لقد", "ما") > 5)
                {
                    if (GetCount(hewbrewEncoding.GetString(buffer), "אולי", "אולי", "אולי", "אולי", "טוב", "טוב") > 10)
                        return hewbrewEncoding;
                    return arabicEncoding;
                }
                if (GetCount(hewbrewEncoding.GetString(buffer), AutoDetectWordsHebrew) > 5)
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
            if (Directory.Exists(DictionaryFolder))
            {
                foreach (string dic in Directory.GetFiles(DictionaryFolder, "*.dic"))
                {
                    string name = Path.GetFileNameWithoutExtension(dic);
                    if (!name.StartsWith("hyph", StringComparison.Ordinal))
                    {
                        try
                        {
                            var ci = CultureInfo.GetCultureInfo(name.Replace('_', '-'));
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

        public static double GetOptimalDisplayMilliseconds(string text)
        {
            return GetOptimalDisplayMilliseconds(text, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds);
        }

        public static double GetOptimalDisplayMilliseconds(string text, double optimalCharactersPerSecond)
        {
            if (optimalCharactersPerSecond < 2 || optimalCharactersPerSecond > 100)
                optimalCharactersPerSecond = 14.7;
            double duration = (HtmlUtil.RemoveHtmlTags(text, true).Length / optimalCharactersPerSecond) * TimeCode.BaseUnit;

            if (duration < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                duration = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;

            if (duration > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                duration = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;

            return duration;
        }

        private static int GetCount(string text, params string[] words)
        {
            int count = 0;
            for (int i = 0; i < words.Length; i++)
            {
                count += Regex.Matches(text, "\\b" + words[i] + "\\b", (RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture)).Count;
            }
            return count;
        }

        private static int GetCountContains(string text, params string[] words)
        {
            int count = 0;
            for (int i = 0; i < words.Length; i++)
            {
                var regEx = new Regex(words[i]);
                count += regEx.Matches(text).Count;
            }
            return count;
        }

        public static string AutoDetectGoogleLanguage(Encoding encoding)
        {
            switch (encoding.CodePage)
            {
                case 860:
                    return "pt"; // Portuguese
                case 28599:
                case 1254:
                    return "tr"; // Turkish
                case 28598:
                case 1255:
                    return "he"; // Hebrew
                case 28596:
                case 1256:
                    return "ar"; // Arabic
                case 1258:
                    return "vi"; // Vietnamese
                case 949:
                case 1361:
                case 20949:
                case 51949:
                case 50225:
                    return "ko"; // Korean
                case 1253:
                case 28597:
                    return "el"; // Greek
                case 50220:
                case 50221:
                case 50222:
                case 51932:
                case 20932:
                case 10001:
                    return "ja"; // Japanese
                case 20000:
                case 20002:
                case 20936:
                case 950:
                case 52936:
                case 54936:
                case 51936:
                    return "zh"; // Chinese
                default:
                    return null;
            }
        }

        public static readonly string[] AutoDetectWordsEnglish = { "we", "are", "and", "you", "your", "what" };
        public static readonly string[] AutoDetectWordsDanish = { "vi", "han", "og", "jeg", "var", "men", "gider", "bliver", "virkelig", "kommer", "tilbage", "Hej" };
        public static readonly string[] AutoDetectWordsNorwegian = { "vi", "er", "og", "jeg", "var", "men" };
        public static readonly string[] AutoDetectWordsSwedish = { "vi", "är", "och", "Jag", "inte", "för" };
        public static readonly string[] AutoDetectWordsSpanish = { "el", "bien", "Vamos", "Hola", "casa", "con" };
        public static readonly string[] AutoDetectWordsFrench = { "un", "vous", "avec", "pas", "ce", "une" };
        public static readonly string[] AutoDetectWordsGerman = { "und", "auch", "sich", "bin", "hast", "möchte" };
        public static readonly string[] AutoDetectWordsDutch = { "van", "een", "[Hh]et", "m(ij|ĳ)", "z(ij|ĳ)n" };
        public static readonly string[] AutoDetectWordsPolish = { "Czy", "ale", "ty", "siê", "jest", "mnie" };
        public static readonly string[] AutoDetectWordsItalian = { "Cosa", "sono", "Grazie", "Buongiorno", "bene", "questo", "ragazzi", "propriamente", "numero", "hanno", "giorno", "faccio", "davvero", "negativo", "essere", "vuole", "sensitivo", "venire" };
        public static readonly string[] AutoDetectWordsPortuguese = { "não", "Não", "Estás", "Então", "isso", "com" };
        public static readonly string[] AutoDetectWordsGreek = { "μου", "είναι", "Είναι", "αυτό", "Τόμπυ", "καλά", "Ενταξει", "Ενταξει", "πρεπει", "Λοιπον", "τιποτα", "ξερεις" };
        public static readonly string[] AutoDetectWordsRussian = { "Это", "не", "ты", "что", "это", "Мы", "Да", "Нет", "Ты", "нет", "Он", "его", "тебя", "как", "Не", "вы", "меня", "Но", "то", "всё", "бы", "мы", "мне", "вас", "знаю", "ещё", "за", "нас", "чтобы", "был" };
        public static readonly string[] AutoDetectWordsBulgarian = { "Какво", "тук", "може", "Как", "Ваше", "какво" };
        public static readonly string[] AutoDetectWordsRomanian = { "Какво", "тук", "може", "Как", "Ваше", "какво" };
        public static readonly string[] AutoDetectWordsArabic = { "Какво", "тук", "може", "Как", "Ваше", "какво" };
        public static readonly string[] AutoDetectWordsHebrew = { "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב" };
        public static readonly string[] AutoDetectWordsVietnamese = { "không", "tôi", "anh", "đó", "Tôi", "ông" };
        public static readonly string[] AutoDetectWordsHungarian = { "hogy", "lesz", "tudom", "vagy", "mondtam", "még" };
        public static readonly string[] AutoDetectWordsTurkish = { "için", "Tamam", "Hayır", "benim", "daha", "deðil", "önce", "lazým", "benim", "çalýþýyor", "burada", "efendim" };
        public static readonly string[] AutoDetectWordsCroatianAndSerbian = { "sam", "ali", "nije", "samo", "ovo", "kako", "dobro", "sve", "tako", "će", "mogu", "ću", "zašto", "nešto", "za" };
        public static readonly string[] AutoDetectWordsCroatian = { "što", "ovdje", "gdje", "kamo", "tko", "prije", "uvijek", "vrijeme", "vidjeti", "netko",
                                                                    "vidio", "nitko", "bok", "lijepo", "oprosti", "htio", "mjesto", "oprostite", "čovjek", "dolje",
                                                                    "čovječe", "dvije", "dijete", "dio", "poslije", "događa", "vjerovati", "vjerojatno", "vjerujem", "točno",
                                                                    "razumijem", "vidjela", "cijeli", "svijet", "obitelj", "volio", "sretan", "dovraga", "svijetu", "htjela",
                                                                    "vidjeli", "negdje", "želio", "ponovno", "djevojka", "umrijeti", "čovjeka", "mjesta", "djeca", "osjećam",
                                                                    "uopće", "djecu", "naprijed", "obitelji", "doista", "mjestu", "lijepa", "također", "riječ", "tijelo" };
        public static readonly string[] AutoDetectWordsSerbian = { "šta", "ovde", "gde", "ko", "pre", "uvek", "vreme", "videti", "neko",
                                                                   "video", "niko", "ćao", "lepo", "izvini", "hteo", "mesto", "izvinite", "čovek", "dole",
                                                                   "čoveče", "dve", "dete", "deo", "posle", "dešava", "verovati", "verovatno", "verujem", "tačno",
                                                                   "razumem", "videla", "ceo", "svet", "porodica", "voleo", "srećan", "dođavola", "svetu", "htela",
                                                                   "videli", "negde", "želeo", "ponovo", "devojka", "umreti", "čoveka", "mesta", "deca", "osećam",
                                                                   "uopšte", "decu", "napred", "porodicu", "zaista", "mestu", "lepa", "takođe", "reč", "telo" };

        public static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            int count = GetCount(text, AutoDetectWordsEnglish);
            if (count > bestCount)
                return "en";

            count = GetCount(text, AutoDetectWordsDanish);
            if (count > bestCount)
            {
                int norwegianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                int dutchCount = GetCount(text, "van", "een", "[Hh]et", "m(ij|ĳ)", "z(ij|ĳ)n");
                if (norwegianCount < 2 && dutchCount < count)
                    return "da";
            }

            count = GetCount(text, AutoDetectWordsNorwegian);
            if (count > bestCount)
            {
                int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                int dutchCount = GetCount(text, "van", "een", "[Hh]et", "m(ij|ĳ)", "z(ij|ĳ)n");
                if (danishCount < 2 && dutchCount < count)
                    return "no";
            }

            count = GetCount(text, AutoDetectWordsSwedish);
            if (count > bestCount)
                return "sv";

            count = GetCount(text, AutoDetectWordsSpanish);
            if (count > bestCount)
            {
                int frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
                if (frenchCount < 2)
                    return "es";
            }

            count = GetCount(text, AutoDetectWordsFrench);
            if (count > bestCount)
            {
                int spanishCount = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                int italianCount = GetCount(text, AutoDetectWordsItalian);
                int romanianCount = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau");
                if (spanishCount < 2 && italianCount < 2 && romanianCount < 5)
                    return "fr";
            }

            count = GetCount(text, AutoDetectWordsGerman);
            if (count > bestCount)
                return "de";

            count = GetCount(text, AutoDetectWordsDutch);
            if (count > bestCount)
                return "nl";

            count = GetCount(text, AutoDetectWordsPolish);
            if (count > bestCount)
                return "pl";

            count = GetCount(text, AutoDetectWordsItalian);
            if (count > bestCount)
            {
                int frenchCount = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
                int spanishCount = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                if (frenchCount < 2 && spanishCount < 2)
                    return "it";
            }

            count = GetCount(text, AutoDetectWordsPortuguese);
            if (count > bestCount)
                return "pt"; // Portuguese

            count = GetCount(text, AutoDetectWordsGreek);
            if (count > bestCount)
                return "el"; // Greek

            count = GetCount(text, AutoDetectWordsRussian);
            if (count > bestCount)
                return "ru"; // Russian

            count = GetCount(text, AutoDetectWordsBulgarian);
            if (count > bestCount)
                return "bg"; // Bulgarian

            count = GetCount(text, AutoDetectWordsArabic);
            if (count > bestCount)
            {
                if (GetCount(text, "אולי", "אולי", "אולי", "אולי", "טוב", "טוב") > 10)
                    return "he";

                int romanianCount = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau", "trãiascã", "niciodatã", "înseamnã",
                                                   "vorbesti", "oamenii", "Asteaptã", "fãcut", "Fãrã", "spune", "decât", "pentru", "vreau");
                if (romanianCount > count)
                    return "ro"; // Romanian

                romanianCount = GetCount(text, "daca", "pentru", "acum", "soare", "trebuie", "Trebuie", "nevoie", "decat", "echilibrul",
                                               "vorbesti", "oamenii", "zeului", "vrea", "atunci", "Poate", "Acum", "memoria", "soarele");
                if (romanianCount > count)
                    return "ro"; // Romanian

                return "ar"; // Arabic
            }

            count = GetCount(text, AutoDetectWordsHebrew);
            if (count > bestCount)
                return "he"; // Hebrew

            count = GetCount(text, AutoDetectWordsCroatianAndSerbian);
            if (count > bestCount)
            {
                int croatianCount = GetCount(text, AutoDetectWordsCroatian);
                int serbianCount = GetCount(text, AutoDetectWordsSerbian);
                if (croatianCount > serbianCount)
                    return "hr"; // Croatian

                return "sr"; // Serbian
            }

            count = GetCount(text, AutoDetectWordsVietnamese);
            if (count > bestCount)
                return "vi"; // Vietnamese

            count = GetCount(text, AutoDetectWordsHungarian);
            if (count > bestCount)
                return "hu"; // Hungarian

            count = GetCount(text, AutoDetectWordsTurkish);
            if (count > bestCount)
                return "tr"; // Turkish

            count = GetCount(text, "yang", "tahu", "bisa", "akan", "tahun", "tapi", "dengan", "untuk", "rumah", "dalam", "sudah", "bertemu");
            if (count > bestCount)
                return "id"; // Indonesian

            count = GetCount(text, "โอ", "โรเบิร์ต", "วิตตอเรีย", "ดร", "คุณตำรวจ", "ราเชล", "ไม่", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์");
            if (count > 10 || count > bestCount)
                return "th"; // Thai

            count = GetCount(text, "그리고", "아니야", "하지만", "말이야", "그들은", "우리가");
            if (count > 10 || count > bestCount)
                return "ko"; // Korean

            count = GetCount(text, "että", "kuin", "minä", "mitään", "Mutta", "siitä", "täällä", "poika", "Kiitos", "enää", "vielä", "tässä");
            if (count > bestCount)
                return "fi"; // Finnish

            count = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau", "trãiascã", "niciodatã", "înseamnã", "vorbesti", "oamenii",
                                   "Asteaptã", "fãcut", "Fãrã", "spune", "decât", "pentru", "vreau");
            if (count > bestCount)
                return "ro"; // Romanian

            count = GetCount(text, "daca", "pentru", "acum", "soare", "trebuie", "Trebuie", "nevoie", "decat", "echilibrul", "vorbesti", "oamenii",
                                   "zeului", "vrea", "atunci", "Poate", "Acum", "memoria", "soarele");
            if (count > bestCount)
                return "ro"; // Romanian

            count = GetCountContains(text, "シ", "ュ", "シン", "シ", "ン", "ユ");
            count += GetCountContains(text, "イ", "ン", "チ", "ェ", "ク", "ハ");
            count += GetCountContains(text, "シ", "ュ", "う", "シ", "ン", "サ");
            count += GetCountContains(text, "シ", "ュ", "シ", "ン", "だ", "う");
            if (count > bestCount * 2)
                return "ja"; // Japanese - not tested...

            count = GetCountContains(text, "是", "是早", "吧", "的", "爱", "上好");
            count += GetCountContains(text, "的", "啊", "好", "好", "亲", "的");
            count += GetCountContains(text, "谢", "走", "吧", "晚", "上", "好");
            count += GetCountContains(text, "来", "卡", "拉", "吐", "滚", "他");
            if (count > bestCount * 2)
                return "zh"; // Chinese (simplified) - not tested...

            return string.Empty;
        }

        public static string AutoDetectGoogleLanguage(Subtitle subtitle)
        {
            string languageId = AutoDetectGoogleLanguageOrNull(subtitle);
            if (languageId == null)
                languageId = "en";

            return languageId;
        }

        public static string AutoDetectGoogleLanguageOrNull(Subtitle subtitle)
        {
            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
                sb.AppendLine(p.Text);

            string languageId = AutoDetectGoogleLanguage(sb.ToString(), subtitle.Paragraphs.Count / 14);
            if (string.IsNullOrEmpty(languageId))
                languageId = null;

            return languageId;
        }

        public static string AutoDetectLanguageName(string languageName, Subtitle subtitle)
        {
            if (string.IsNullOrEmpty(languageName))
                languageName = "en_US";
            int bestCount = subtitle.Paragraphs.Count / 14;

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
                sb.AppendLine(p.Text);
            string text = sb.ToString();

            List<string> dictionaryNames = GetDictionaryLanguages();

            bool containsEnGb = false;
            bool containsEnUs = false;
            bool containsHrHr = false;
            bool containsSrLatn = false;
            foreach (string name in dictionaryNames)
            {
                if (name.Contains("[en_GB]"))
                    containsEnGb = true;
                if (name.Contains("[en_US]"))
                    containsEnUs = true;
                if (name.Contains("[hr_HR]"))
                    containsHrHr = true;
                if (name.Contains("[sr-Latn]"))
                    containsSrLatn = true;
            }

            foreach (string name in dictionaryNames)
            {
                string shortName = string.Empty;
                int start = name.IndexOf('[');
                int end = name.IndexOf(']');
                if (start > 0 && end > start)
                {
                    start++;
                    shortName = name.Substring(start, end - start);
                }

                int count;
                switch (shortName)
                {
                    case "da_DK":
                        count = GetCount(text, "vi", "hun", "og", "jeg", "var", "men", "bliver", "meget", "spørger", "Hej", "utrolig", "dejligt");
                        if (count > bestCount)
                        {
                            int norweigianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                            if (norweigianCount < 2)
                                languageName = shortName;
                        }
                        break;
                    case "nb_NO":
                        count = GetCount(text, AutoDetectWordsNorwegian);
                        if (count > bestCount)
                        {
                            int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                            int dutchCount = GetCount(text, "van", "een", "[Hh]et", "m(ij|ĳ)", "z(ij|ĳ)n");
                            if (danishCount < 2 && dutchCount < count)
                                languageName = shortName;
                        }
                        break;
                    case "en_US":
                        count = GetCount(text, AutoDetectWordsEnglish);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            if (containsEnGb)
                            {
                                int usCount = GetCount(text, "color", "flavor", "honor", "humor", "neighbor", "honor");
                                int gbCount = GetCount(text, "colour", "flavour", "honour", "humour", "neighbour", "honour");
                                if (gbCount > usCount)
                                    languageName = "en_GB";
                            }
                        }
                        break;
                    case "en_GB":
                        count = GetCount(text, "we", "are", "and", "you", "your", "what");
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            if (containsEnUs)
                            {
                                int usCount = GetCount(text, "color", "flavor", "honor", "humor", "neighbor", "honor");
                                int gbCount = GetCount(text, "colour", "flavour", "honour", "humour", "neighbour", "honour");
                                if (gbCount < usCount)
                                    languageName = "en_US";
                            }
                        }
                        break;
                    case "sv_SE":
                        count = GetCount(text, "vi", "är", "och", "Jag", "inte", "för");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "es_ES":
                        count = GetCount(text, AutoDetectWordsSpanish);
                        if (count > bestCount)
                        {
                            int frenchWords = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
                            if (frenchWords < 2)
                                languageName = shortName;
                        }
                        break;
                    case "fr_FR":
                        count = GetCount(text, AutoDetectWordsFrench);
                        if (count > bestCount)
                        {
                            int spanishWords = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                            int italianWords = GetCount(text, AutoDetectWordsItalian); // not italian words
                            if (spanishWords < 2 && italianWords < 2)
                                languageName = shortName;
                        }
                        break;
                    case "it_IT":
                        count = GetCount(text, AutoDetectWordsItalian);
                        if (count > bestCount)
                        {
                            int frenchWords = GetCount(text, "[Cc]'est", "pas", "vous", "pour", "suis", "Pourquoi", "maison", "souviens", "quelque"); // not spanish words
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
                        count = GetCount(text, "van", "een", "[Hh]et", "m(ij|ĳ)", "z(ij|ĳ)n");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "pl_PL":
                        count = GetCount(text, "Czy", "ale", "ty", "siê", "jest", "mnie");
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "el_GR":
                        count = GetCount(text, AutoDetectWordsGreek);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "ru_RU":
                        count = GetCount(text, AutoDetectWordsRussian);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "ro_RO":
                        count = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau", "trãiascã", "niciodatã", "înseamnã", "vorbesti", "oamenii", "Asteaptã",
                                               "fãcut", "Fãrã", "spune", "decât", "pentru", "vreau");
                        if (count > bestCount)
                        {
                            languageName = shortName;
                        }
                        else
                        {
                            count = GetCount(text, "daca", "pentru", "acum", "soare", "trebuie", "Trebuie", "nevoie", "decat", "echilibrul", "vorbesti", "oamenii", "zeului",
                                                   "vrea", "atunci", "Poate", "Acum", "memoria", "soarele");

                            if (count > bestCount)
                                languageName = shortName;
                        }
                        break;
                    case "hr_HR": // Croatian
                        count = GetCount(text, AutoDetectWordsCroatianAndSerbian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            if (containsSrLatn)
                            {
                                int croatianCount = GetCount(text, AutoDetectWordsCroatian);
                                int serbianCount = GetCount(text, AutoDetectWordsSerbian);
                                if (serbianCount > croatianCount)
                                    languageName = "sr-Latn";
                            }
                        }
                        break;
                    case "sr-Latn": // Serbian (Latin)
                        count = GetCount(text, AutoDetectWordsCroatianAndSerbian);
                        if (count > bestCount)
                        {
                            languageName = shortName;
                            if (containsHrHr)
                            {
                                int croatianCount = GetCount(text, AutoDetectWordsCroatian);
                                int serbianCount = GetCount(text, AutoDetectWordsSerbian);
                                if (serbianCount < croatianCount)
                                    languageName = "hr_HR";
                            }
                        }
                        break;
                    case "pt_PT": // Portuguese
                        count = GetCount(text, AutoDetectWordsPortuguese);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "pt_BR": // Portuguese (Brasil)
                        count = GetCount(text, AutoDetectWordsPortuguese);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                    case "hu_HU": // Hungarian
                        count = GetCount(text, AutoDetectWordsHungarian);
                        if (count > bestCount)
                            languageName = shortName;
                        break;
                }
            }
            return languageName;
        }

        public static string ColorToHex(Color c)
        {
            return string.Format("#{0:x2}{1:x2}{2:x2}", c.R, c.G, c.B);
        }

        public static int GetMaxLineLength(string text)
        {
            int maxLength = 0;
            text = HtmlUtil.RemoveHtmlTags(text, true);
            foreach (string line in text.SplitToLines())
            {
                if (line.Length > maxLength)
                    maxLength = line.Length;
            }
            return maxLength;
        }

        public static double GetCharactersPerSecond(Paragraph paragraph)
        {
            if (paragraph.Duration.TotalMilliseconds < 1)
                return 999;

            const string zeroWidthSpace = "\u200B";
            const string zeroWidthNoBreakSpace = "\uFEFF";

            string s = HtmlUtil.RemoveHtmlTags(paragraph.Text, true).Replace(Environment.NewLine, string.Empty).Replace(zeroWidthSpace, string.Empty).Replace(zeroWidthNoBreakSpace, string.Empty);
            return s.Length / paragraph.Duration.TotalSeconds;
        }

        public static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
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
                return Assembly.GetEntryAssembly().GetName().Version.ToString();
            }
        }

        public static string AssemblyDescription
        {
            get
            {
                var assembly = Assembly.GetEntryAssembly();
                string assemblyName = assembly.GetName().Name;
                if (Attribute.IsDefined(assembly, typeof(AssemblyDescriptionAttribute)))
                {
                    Console.WriteLine(assemblyName);
                    var descriptionAttribute = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                    if (descriptionAttribute != null)
                        return descriptionAttribute.Description;
                }
                return null;
            }
        }

        private static void AddExtension(StringBuilder sb, string extension)
        {
            if (!sb.ToString().Contains("*" + extension + ";", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append('*');
                sb.Append(extension.TrimStart('*'));
                sb.Append(';');
            }
        }

        public static string GetOpenDialogFilter()
        {
            var sb = new StringBuilder();
            sb.Append(Configuration.Settings.Language.General.SubtitleFiles + "|");
            foreach (SubtitleFormat s in SubtitleFormat.AllSubtitleFormats)
            {
                AddExtension(sb, s.Extension);
                foreach (string ext in s.AlternateExtensions)
                    AddExtension(sb, ext);
            }
            AddExtension(sb, new Pac().Extension);
            AddExtension(sb, new Cavena890().Extension);
            AddExtension(sb, new Spt().Extension);
            AddExtension(sb, new Wsb().Extension);
            AddExtension(sb, new CheetahCaption().Extension);
            AddExtension(sb, ".chk");
            AddExtension(sb, new CaptionsInc().Extension);
            AddExtension(sb, new Ultech130().Extension);
            AddExtension(sb, new ELRStudioClosedCaption().Extension);
            AddExtension(sb, ".uld"); // Ultech drop frame
            AddExtension(sb, new SonicScenaristBitmaps().Extension);
            AddExtension(sb, ".mks");
            AddExtension(sb, ".mxf");
            AddExtension(sb, ".sup");
            AddExtension(sb, ".dost");
            AddExtension(sb, new Ayato().Extension);
            AddExtension(sb, new PacUnicode().Extension);

            if (!string.IsNullOrEmpty(Configuration.Settings.General.OpenSubtitleExtraExtensions))
            {
                var extraExtensions = Configuration.Settings.General.OpenSubtitleExtraExtensions.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string ext in extraExtensions)
                {
                    if (ext.StartsWith("*.", StringComparison.Ordinal) && !sb.ToString().Contains(ext, StringComparison.OrdinalIgnoreCase))
                        AddExtension(sb, ext);
                }
            }
            AddExtension(sb, ".son");

            sb.Append('|');
            sb.Append(Configuration.Settings.Language.General.AllFiles);
            sb.Append("|*.*");
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

        public static Color ColorDarkOrange = Color.FromArgb(220, 90, 10);

        public static void GetLineLengths(Label label, string text)
        {
            label.ForeColor = Color.Black;
            var lines = HtmlUtil.RemoveHtmlTags(text, true).SplitToLines();

            const int max = 3;

            var sb = new StringBuilder();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (i > 0)
                {
                    sb.Append('/');
                }

                if (i > max)
                {
                    label.ForeColor = Color.Red;
                    sb.Append("...");
                    label.Text = sb.ToString();
                    return;
                }

                sb.Append(line.Length);
                if (line.Length > Configuration.Settings.General.SubtitleLineMaximumLength)
                    label.ForeColor = Color.Red;
                else if (line.Length > Configuration.Settings.General.SubtitleLineMaximumLength - 5)
                    label.ForeColor = ColorDarkOrange;
            }
            label.Text = sb.ToString();
        }

        public static bool IsValidRegex(string testPattern)
        {
            if (string.IsNullOrEmpty(testPattern))
            {
                return false;
            }

            try
            {
                Regex.Match(string.Empty, testPattern);
            }
            catch (ArgumentException)
            {
                // BAD PATTERN: Syntax error
                return false;
            }
            return true;
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

        public static void RemoveFromUserDictionary(string word, string languageName)
        {
            word = word.Trim();
            if (word.Length > 0)
            {
                string userWordsXmlFileName = DictionaryFolder + languageName + "_user.xml";
                var userWords = new XmlDocument();
                if (File.Exists(userWordsXmlFileName))
                    userWords.Load(userWordsXmlFileName);
                else
                    userWords.LoadXml("<words />");

                var words = new List<string>();
                foreach (XmlNode node in userWords.DocumentElement.SelectNodes("word"))
                {
                    string w = node.InnerText.Trim();
                    if (w.Length > 0 && w != word)
                        words.Add(w);
                }
                words.Sort();

                userWords.DocumentElement.RemoveAll();
                foreach (string w in words)
                {
                    XmlNode node = userWords.CreateElement("word");
                    node.InnerText = w;
                    userWords.DocumentElement.AppendChild(node);
                }
                userWords.Save(userWordsXmlFileName);
            }
        }

        public static void AddToUserDictionary(string word, string languageName)
        {
            word = word.Trim();
            if (word.Length > 0)
            {
                string userWordsXmlFileName = DictionaryFolder + languageName + "_user.xml";
                var userWords = new XmlDocument();
                if (File.Exists(userWordsXmlFileName))
                    userWords.Load(userWordsXmlFileName);
                else
                    userWords.LoadXml("<words />");

                var words = new List<string>();
                foreach (XmlNode node in userWords.DocumentElement.SelectNodes("word"))
                {
                    string w = node.InnerText.Trim();
                    if (w.Length > 0)
                        words.Add(w);
                }
                words.Add(word);
                words.Sort();

                userWords.DocumentElement.RemoveAll();
                foreach (string w in words)
                {
                    XmlNode node = userWords.CreateElement("word");
                    node.InnerText = w;
                    userWords.DocumentElement.AppendChild(node);
                }
                userWords.Save(userWordsXmlFileName);
            }
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

        public static string LoadUserWordList(HashSet<string> userWordList, string languageName)
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

        public static readonly string UppercaseLetters = GetLetters(true, false, false);
        public static readonly string LowercaseLetters = GetLetters(false, true, false);
        public static readonly string LowercaseLettersWithNumbers = GetLetters(false, true, true);
        public static readonly string AllLetters = GetLetters(true, true, false);
        public static readonly string AllLettersAndNumbers = GetLetters(true, true, true);

        private static string GetLetters(bool uppercase, bool lowercase, bool numbers)
        {
            var sb = new StringBuilder();

            if (uppercase)
                sb.Append(Configuration.Settings.General.UppercaseLetters.ToUpper());

            if (lowercase)
                sb.Append(Configuration.Settings.General.UppercaseLetters.ToLower());

            if (numbers)
                sb.Append("0123456789");

            return sb.ToString();
        }

        public static Color GetColorFromUserName(string userName)
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
                default:
                    return Color.Black;
            }
        }

        public static int GetNumber0To7FromUserName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return 0;

            byte[] buffer = Encoding.UTF8.GetBytes(userName);
            long number = 0;
            foreach (byte b in buffer)
                number += b;

            return (int)(number % 8);
        }

        public static string GetRegExGroup(string regEx)
        {
            var start = regEx.IndexOf("(?<", StringComparison.Ordinal);
            if (start < 0)
                return null;
            start += 3;
            var end = regEx.IndexOf('>', start);
            if (end <= start)
                return null;
            return regEx.Substring(start, end - start);
        }

        public static string LowercaseVowels
        {
            get
            {
                return "aeiouyæøåéóáôèòæøåäöïɤəɛʊʉɨ";
            }
        }

        public static void SetButtonHeight(Control control, int newHeight, int level)
        {
            if (level > 6)
                return;

            if (control.HasChildren)
            {
                foreach (Control subControl in control.Controls)
                {
                    if (subControl.HasChildren)
                        SetButtonHeight(subControl, newHeight, level + 1);
                    else if (subControl is Button)
                        subControl.Height = newHeight;
                }
            }
            else if (control is Button)
                control.Height = newHeight;
        }

        public static int CountTagInText(string text, string tag)
        {
            int count = 0;
            int index = text.IndexOf(tag, StringComparison.Ordinal);
            while (index >= 0)
            {
                count++;
                index = index + tag.Length;
                if (index >= text.Length)
                    return count;
                index = text.IndexOf(tag, index, StringComparison.Ordinal);
            }
            return count;
        }

        public static int CountTagInText(string text, char tag)
        {
            int count = 0;
            int index = text.IndexOf(tag);
            while (index >= 0)
            {
                count++;
                if ((index + 1) == text.Length)
                    return count;
                index = text.IndexOf(tag, index + 1);
            }
            return count;
        }

        public static bool StartsAndEndsWithTag(string text, string startTag, string endTag)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            if (!text.Contains(startTag) || !text.Contains(endTag))
                return false;

            while (text.Contains("  "))
                text = text.Replace("  ", " ");

            var s1 = "- " + startTag;
            var s2 = "-" + startTag;
            var s3 = "- ..." + startTag;
            var s4 = "- " + startTag + "..."; // - <i>...

            var e1 = endTag + ".";
            var e2 = endTag + "!";
            var e3 = endTag + "?";
            var e4 = endTag + "...";
            var e5 = endTag + "-";

            bool isStart = false;
            bool isEnd = false;
            if (text.StartsWith(startTag, StringComparison.Ordinal) || text.StartsWith(s1, StringComparison.Ordinal) || text.StartsWith(s2, StringComparison.Ordinal) || text.StartsWith(s3, StringComparison.Ordinal) || text.StartsWith(s4, StringComparison.Ordinal))
                isStart = true;
            if (text.EndsWith(endTag, StringComparison.Ordinal) || text.EndsWith(e1, StringComparison.Ordinal) || text.EndsWith(e2, StringComparison.Ordinal) || text.EndsWith(e3, StringComparison.Ordinal) || text.EndsWith(e4, StringComparison.Ordinal) || text.EndsWith(e5, StringComparison.Ordinal))
                isEnd = true;
            return isStart && isEnd;
        }

        public static Paragraph GetOriginalParagraph(int index, Paragraph paragraph, List<Paragraph> originalParagraphs)
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
                    p.StartTime.TotalMilliseconds < paragraph.StartTime.TotalMilliseconds + TimeCode.BaseUnit)
                    return p;
            }
            return null;
        }

        /// <summary>
        /// UrlEncodes a string without the requirement for System.Web
        /// </summary>
        public static string UrlEncode(string text)
        {
            return Uri.EscapeDataString(text);
        }

        /// <summary>
        /// UrlDecodes a string without requiring System.Web
        /// </summary>
        public static string UrlDecode(string text)
        {
            // pre-process for + sign space formatting since System.Uri doesn't handle it
            // plus literals are encoded as %2b normally so this should be safe
            text = text.Replace('+', ' ');
            return Uri.UnescapeDataString(text);
        }

        public static void CheckAutoWrap(TextBox textBox, KeyEventArgs e, int numberOfNewLines)
        {
            int length = HtmlUtil.RemoveHtmlTags(textBox.Text).Length;
            if (e.Modifiers == Keys.None && e.KeyCode != Keys.Enter && numberOfNewLines < 1 && length > Configuration.Settings.General.SubtitleLineMaximumLength)
            {
                if (Configuration.Settings.General.AutoWrapLineWhileTyping) // only if auto-break-setting is true
                {
                    string newText;
                    if (length > Configuration.Settings.General.SubtitleLineMaximumLength + 30)
                    {
                        newText = AutoBreakLine(textBox.Text);
                    }
                    else
                    {
                        int lastSpace = textBox.Text.LastIndexOf(' ');
                        if (lastSpace > 0)
                            newText = textBox.Text.Remove(lastSpace, 1).Insert(lastSpace, Environment.NewLine);
                        else
                            newText = textBox.Text;
                    }

                    int autobreakIndex = newText.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    if (autobreakIndex > 0)
                    {
                        int selectionStart = textBox.SelectionStart;
                        textBox.Text = newText;
                        if (selectionStart > autobreakIndex)
                            selectionStart += Environment.NewLine.Length - 1;
                        if (selectionStart >= 0)
                            textBox.SelectionStart = selectionStart;
                    }
                }
            }
        }

        private static readonly Dictionary<string, Keys> AllKeys = new Dictionary<string, Keys>();
        public static Keys GetKeys(string keysInString)
        {
            if (string.IsNullOrEmpty(keysInString))
                return Keys.None;

            if (AllKeys.Count == 0)
            {
                foreach (Keys val in Enum.GetValues(typeof(Keys)))
                {
                    string k = val.ToString().ToLower();
                    if (!AllKeys.ContainsKey(k))
                        AllKeys.Add(k, val);
                }
                if (!AllKeys.ContainsKey("pagedown"))
                    AllKeys.Add("pagedown", Keys.RButton | Keys.Space);
                if (!AllKeys.ContainsKey("home"))
                    AllKeys.Add("home", Keys.MButton | Keys.Space);
                if (!AllKeys.ContainsKey("capslock"))
                    AllKeys.Add("capslock", Keys.CapsLock);
            }

            string[] parts = keysInString.ToLower().Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries);
            var resultKeys = Keys.None;
            foreach (string k in parts)
            {
                if (AllKeys.ContainsKey(k))
                    resultKeys = resultKeys | AllKeys[k];
            }
            return resultKeys;
        }

        public static string FixEnglishTextInRightToLeftLanguage(string text, string reverseChars)
        {
            var sb = new StringBuilder();
            var lines = text.SplitToLines();
            foreach (string line in lines)
            {
                string s = line.Trim();
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == ')')
                        s = s.Remove(i, 1).Insert(i, "(");
                    else if (s[i] == '(')
                        s = s.Remove(i, 1).Insert(i, ")");
                }

                bool numbersOn = false;
                string numbers = string.Empty;
                for (int i = 0; i < s.Length; i++)
                {
                    if (numbersOn && reverseChars.Contains(s[i]))
                    {
                        numbers = s[i] + numbers;
                    }
                    else if (numbersOn)
                    {
                        numbersOn = false;
                        s = s.Remove(i - numbers.Length, numbers.Length).Insert(i - numbers.Length, numbers);
                        numbers = string.Empty;
                    }
                    else if (reverseChars.Contains(s[i]))
                    {
                        numbers = s[i] + numbers;
                        numbersOn = true;
                    }
                }
                if (numbersOn)
                {
                    int i = s.Length;
                    s = s.Remove(i - numbers.Length, numbers.Length).Insert(i - numbers.Length, numbers);
                }

                sb.AppendLine(s);
            }
            return sb.ToString().Trim();
        }

        public static string ToSuperscript(string text)
        {
            var sb = new StringBuilder();
            var superscript = new List<char>{
                                              '⁰',
                                              '¹',
                                              '²',
                                              '³',
                                              '⁴',
                                              '⁵',
                                              '⁶',
                                              '⁷',
                                              '⁸',
                                              '⁹',
                                              '⁺',
                                              '⁻',
                                              '⁼',
                                              '⁽',
                                              '⁾',
                                              'ᵃ',
                                              'ᵇ',
                                              'ᶜ',
                                              'ᵈ',
                                              'ᵉ',
                                              'ᶠ',
                                              'ᵍ',
                                              'ʰ',
                                              'ⁱ',
                                              'ʲ',
                                              'ᵏ',
                                              'ˡ',
                                              'ᵐ',
                                              'ⁿ',
                                              'ᵒ',
                                              'ᵖ',
                                              'ʳ',
                                              'ˢ',
                                              'ᵗ',
                                              'ᵘ',
                                              'ᵛ',
                                              'ʷ',
                                              'ˣ',
                                              'ʸ',
                                              'ᶻ',
                                              'ᴬ',
                                              'ᴮ',
                                              'ᴰ',
                                              'ᴱ',
                                              'ᴳ',
                                              'ᴴ',
                                              'ᴵ',
                                              'ᴶ',
                                              'ᴷ',
                                              'ᴸ',
                                              'ᴹ',
                                              'ᴺ',
                                              'ᴼ',
                                              'ᴾ',
                                              'ᴿ',
                                              'ᵀ',
                                              'ᵁ',
                                              'ᵂ'
                                            };
            var normal = new List<char>{
                                         '0', // "⁰"
                                         '1', // "¹"
                                         '2', // "²"
                                         '3', // "³"
                                         '4', // "⁴"
                                         '5', // "⁵"
                                         '6', // "⁶"
                                         '7', // "⁷"
                                         '8', // "⁸"
                                         '9', // "⁹"
                                         '+', // "⁺"
                                         '-', // "⁻"
                                         '=', // "⁼"
                                         '(', // "⁽"
                                         ')', // "⁾"
                                         'a', // "ᵃ"
                                         'b', // "ᵇ"
                                         'c', // "ᶜ"
                                         'd', // "ᵈ"
                                         'e', // "ᵉ"
                                         'f', // "ᶠ"
                                         'g', // "ᵍ"
                                         'h', // "ʰ"
                                         'i', // "ⁱ"
                                         'j', // "ʲ"
                                         'k', // "ᵏ"
                                         'l', // "ˡ"
                                         'm', // "ᵐ"
                                         'n', // "ⁿ"
                                         'o', // "ᵒ"
                                         'p', // "ᵖ"
                                         'r', // "ʳ"
                                         's', // "ˢ"
                                         't', // "ᵗ"
                                         'u', // "ᵘ"
                                         'v', // "ᵛ"
                                         'w', // "ʷ"
                                         'x', // "ˣ"
                                         'y', // "ʸ"
                                         'z', // "ᶻ"
                                         'A', // "ᴬ"
                                         'B', // "ᴮ"
                                         'D', // "ᴰ"
                                         'E', // "ᴱ"
                                         'G', // "ᴳ"
                                         'H', // "ᴴ"
                                         'I', // "ᴵ"
                                         'J', // "ᴶ"
                                         'K', // "ᴷ"
                                         'L', // "ᴸ"
                                         'M', // "ᴹ"
                                         'N', // "ᴺ"
                                         'O', // "ᴼ"
                                         'P', // "ᴾ"
                                         'R', // "ᴿ"
                                         'T', // "ᵀ"
                                         'U', // "ᵁ"
                                         'W', // "ᵂ"
                                            };
            for (int i = 0; i < text.Length; i++)
            {
                char s = text[i];
                int index = normal.IndexOf(s);
                if (index >= 0)
                    sb.Append(superscript[index]);
                else
                    sb.Append(s);
            }
            return sb.ToString();
        }

        public static string ToSubscript(string text)
        {
            var sb = new StringBuilder();
            var subcript = new List<char>{
                                           '₀',
                                           '₁',
                                           '₂',
                                           '₃',
                                           '₄',
                                           '₅',
                                           '₆',
                                           '₇',
                                           '₈',
                                           '₉',
                                           '₊',
                                           '₋',
                                           '₌',
                                           '₍',
                                           '₎',
                                           'ₐ',
                                           'ₑ',
                                           'ᵢ',
                                           'ₒ',
                                           'ᵣ',
                                           'ᵤ',
                                           'ᵥ',
                                           'ₓ',
                                            };
            var normal = new List<char>
                             {
                               '0',  // "₀"
                               '1',  // "₁"
                               '2',  // "₂"
                               '3',  // "₃"
                               '4',  // "₄"
                               '5',  // "₅"
                               '6',  // "₆"
                               '7',  // "₇"
                               '8',  // "₈"
                               '9',  // "₉"
                               '+',  // "₊"
                               '-',  // "₋"
                               '=',  // "₌"
                               '(',  // "₍"
                               ')',  // "₎"
                               'a',  // "ₐ"
                               'e',  // "ₑ"
                               'i',  // "ᵢ"
                               'o',  // "ₒ"
                               'r',  // "ᵣ"
                               'u',  // "ᵤ"
                               'v',  // "ᵥ"
                               'x',  // "ₓ"
                             };
            for (int i = 0; i < text.Length; i++)
            {
                char s = text[i];
                int index = normal.IndexOf(s);
                if (index >= 0)
                    sb.Append(subcript[index]);
                else
                    sb.Append(s);
            }
            return sb.ToString();
        }

        public static string FixQuotes(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.StartsWith('"') && text.Length > 1)
                text = text.Substring(1);

            if (text.EndsWith('"') && text.Length >= 1)
                text = text.Substring(0, text.Length - 1);

            return text.Replace("\"\"", "\"");
        }

        public static Color GetColorFromFontString(string text, Color defaultColor)
        {
            string s = text.TrimEnd();
            int start = s.IndexOf("<font ", StringComparison.OrdinalIgnoreCase);
            if (start >= 0 && s.EndsWith("</font>", StringComparison.OrdinalIgnoreCase))
            {
                int end = s.IndexOf('>', start);
                if (end > 0)
                {
                    string f = s.Substring(start, end - start);
                    if (f.Contains(" color="))
                    {
                        int colorStart = f.IndexOf(" color=", StringComparison.OrdinalIgnoreCase);
                        if (s.IndexOf('"', colorStart + " color=".Length + 1) > 0)
                            end = s.IndexOf('"', colorStart + " color=".Length + 1);
                        s = s.Substring(colorStart, end - colorStart);
                        s = s.Replace(" color=", string.Empty);
                        s = s.Trim('\'').Trim('"').Trim('\'');
                        try
                        {
                            if (s.StartsWith("rgb(", StringComparison.Ordinal))
                            {
                                var arr = s.Remove(0, 4).TrimEnd(')').Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                return Color.FromArgb(int.Parse(arr[0]), int.Parse(arr[1]), int.Parse(arr[2]));
                            }
                            return ColorTranslator.FromHtml(s);
                        }
                        catch
                        {
                            return defaultColor;
                        }
                    }
                }
            }
            return defaultColor;
        }

        public static string[] SplitForChangedCalc(string s, bool ignoreLineBreaks, bool breakToLetters)
        {
            const string endChars = "!?.:;,#%$£";
            var list = new List<string>();

            if (breakToLetters)
            {
                foreach (char ch in s)
                    list.Add(ch.ToString());
            }
            else
            {
                var word = new StringBuilder();
                int i = 0;
                while (i < s.Length)
                {
                    if (ignoreLineBreaks && s.Substring(i).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                    {
                        if (word.Length > 0)
                            list.Add(word.ToString());
                        word.Clear();
                        i += Environment.NewLine.Length;
                    }
                    else if (s.Substring(i).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                    {
                        if (word.Length > 0)
                            list.Add(word.ToString());
                        word.Clear();
                        list.Add(Environment.NewLine);
                        i += Environment.NewLine.Length;
                    }
                    else if (s[i] == ' ')
                    {
                        if (word.Length > 0)
                            list.Add(word.ToString());
                        word.Clear();
                        i++;
                    }
                    else if (endChars.Contains(s[i]) && (word.Length == 0 || endChars.Contains(word[0])))
                    {
                        word.Append(s[i]);
                        i++;
                    }
                    else if (endChars.Contains(s[i]))
                    {
                        if (word.Length > 0)
                            list.Add(word.ToString());
                        word.Clear();
                        word.Append(s[i]);
                        i++;
                    }
                    else
                    {
                        word.Append(s[i]);
                        i++;
                    }
                }
                if (word.Length > 0)
                    list.Add(word.ToString());
            }
            return list.ToArray();
        }

        public static void GetTotalAndChangedWords(string s1, string s2, ref int total, ref int change, bool ignoreLineBreaks, bool breakToLetters)
        {
            var parts1 = SplitForChangedCalc(s1, ignoreLineBreaks, breakToLetters);
            var parts2 = SplitForChangedCalc(s2, ignoreLineBreaks, breakToLetters);
            total += Math.Max(parts1.Length, parts2.Length);
            change += GetChangesAdvanced(parts1, parts2);
        }

        private static int GetChangesAdvanced(string[] parts1, string[] parts2)
        {
            int i1 = 0;
            int i2 = 0;
            int i = 0;
            int c = 0;
            while (i < Math.Max(parts1.Length, parts2.Length) && i1 < parts1.Length && i2 < parts2.Length)
            {
                if (parts1[i1] == parts2[i2])
                {
                    i1++;
                    i2++;
                }
                else
                {
                    int i1Next = FindNext(parts2[i2], parts1, i1);
                    int i2Next = FindNext(parts1[i1], parts2, i2);
                    if (i1Next < i2Next)
                    {
                        c += i1Next - i1;
                        i1 = i1Next + 1;
                        i2++;
                    }
                    else if (i2Next < i1Next)
                    {
                        c += i2Next - i2;
                        i1++;
                        i2 = i2Next + 1;
                    }
                    else
                    {
                        i1++;
                        i2++;
                        c++;
                    }
                }
                i++;
            }
            if (i1 == parts1.Length && i2 == parts2.Length)
                return c;

            return c + Math.Abs(parts1.Length - parts2.Length);
        }

        private static int FindNext(string s, string[] parts, int startIndex)
        {
            for (int i = startIndex; i < parts.Length; i++)
            {
                if (s == parts[i])
                    return i;
            }
            return int.MaxValue;
        }

        public static string RemoveNonNumbers(string p)
        {
            if (string.IsNullOrEmpty(p))
                return p;

            var sb = new StringBuilder();
            foreach (var c in p)
            {
                if (char.IsDigit(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Remove unneeded spaces
        /// </summary>
        /// <param name="text">text string to remove unneeded spaces from</param>
        /// <param name="language">two letter language id string</param>
        /// <returns>text with unneeded spaces removed</returns>
        public static string RemoveUnneededSpaces(string text, string language)
        {
            const string zeroWidthSpace = "\u200B";
            const string zeroWidthNoBreakSpace = "\uFEFF";
            const string noBreakSpace = "\u00A0";
            const string operatingSystemCommand = "\u009D";

            text = text.Trim();

            text = text.Replace(zeroWidthSpace, string.Empty);
            text = text.Replace(zeroWidthNoBreakSpace, string.Empty);
            text = text.Replace(noBreakSpace, " ");
            text = text.Replace(operatingSystemCommand, string.Empty);
            text = text.Replace('\t', ' ');

            text = text.FixExtraSpaces();

            if (text.EndsWith(' '))
                text = text.Substring(0, text.Length - 1);

            const string ellipses = "...";
            text = text.Replace(". . ..", ellipses);
            text = text.Replace(". ...", ellipses);
            text = text.Replace(". .. .", ellipses);
            text = text.Replace(". . .", ellipses);
            text = text.Replace(". ..", ellipses);
            text = text.Replace(".. .", ellipses);

            // Fix recursive: ...
            while (text.Contains("...."))
                text = text.Replace("....", ellipses);

            text = text.Replace(" ..." + Environment.NewLine, "..." + Environment.NewLine);
            text = text.Replace(Environment.NewLine + "... ", Environment.NewLine + "...");
            text = text.Replace(Environment.NewLine + "<i>... ", Environment.NewLine + "<i>...");
            text = text.Replace(Environment.NewLine + "- ... ", Environment.NewLine + "- ...");
            text = text.Replace(Environment.NewLine + "<i>- ... ", Environment.NewLine + "<i>- ...");
            text = text.Replace(Environment.NewLine + "- ... ", Environment.NewLine + "- ...");

            if (text.StartsWith("... ", StringComparison.Ordinal))
                text = text.Remove(3, 1);
            if (text.EndsWith(" ...", StringComparison.Ordinal))
                text = text.Remove(text.Length - 4, 1);
            if (text.EndsWith(" ...</i>", StringComparison.Ordinal))
                text = text.Remove(text.Length - 8, 1);
            if (text.StartsWith("- ... ", StringComparison.Ordinal))
                text = text.Remove(5, 1);
            if (text.StartsWith("<i>... ", StringComparison.Ordinal))
                text = text.Remove(6, 1);

            if (language != "fr") // special rules for French
            {
                text = text.Replace("... ?", "...?");
                text = text.Replace("... !", "...!");

                text = text.Replace(" :", ":");
                text = text.Replace(" :", ":");
            }

            if (!text.Contains("- ..."))
                text = text.Replace(" ... ", "... ");

            while (text.Contains(" ,"))
                text = text.Replace(" ,", ",");

            if (text.EndsWith(" .", StringComparison.Ordinal))
                text = text.Remove(text.Length - 2, 1);

            if (text.EndsWith(" \"", StringComparison.Ordinal))
                text = text.Remove(text.Length - 2, 1);

            if (text.Contains(" \"" + Environment.NewLine))
                text = text.Replace(" \"" + Environment.NewLine, "\"" + Environment.NewLine);

            if (text.Contains(" ." + Environment.NewLine))
                text = text.Replace(" ." + Environment.NewLine, "." + Environment.NewLine);

            if (language != "fr") // special rules for French
            {
                if (text.Contains(" !"))
                    text = text.Replace(" !", "!");

                if (text.Contains(" ?"))
                    text = text.Replace(" ?", "?");
            }

            while (text.Contains("¿ "))
                text = text.Replace("¿ ", "¿");

            while (text.Contains("¡ "))
                text = text.Replace("¡ ", "¡");

            // Italic
            if (text.Contains("<i>", StringComparison.OrdinalIgnoreCase) && text.Contains("</i>", StringComparison.OrdinalIgnoreCase))
                text = RemoveSpaceBeforeAfterTag(text, "<i>");

            // Bold
            if (text.Contains("<b>", StringComparison.OrdinalIgnoreCase) && text.Contains("</b>", StringComparison.OrdinalIgnoreCase))
                text = RemoveSpaceBeforeAfterTag(text, "<b>");

            // Underline
            if (text.Contains("<u>", StringComparison.OrdinalIgnoreCase) && text.Contains("</u>", StringComparison.OrdinalIgnoreCase))
                text = RemoveSpaceBeforeAfterTag(text, "<u>");

            // Font
            if (text.Contains("<font ", StringComparison.OrdinalIgnoreCase))
            {
                var idx = text.IndexOf("<font ", StringComparison.OrdinalIgnoreCase);
                var endIdx = text.IndexOf('>', idx + 6);
                if (endIdx > idx && endIdx < text.Length - 8)
                {
                    var color = text.Substring(idx, (endIdx - idx) + 1).ToLower();
                    text = RemoveSpaceBeforeAfterTag(text, color);
                }
            }
            text = text.Trim();
            text = text.Replace(Environment.NewLine + " ", Environment.NewLine);

            if (text.Contains("- ") && text.Length > 5)
            {
                int idx = text.IndexOf("- ", 2, StringComparison.Ordinal);
                if (text.StartsWith("<i>", StringComparison.OrdinalIgnoreCase))
                    idx = text.IndexOf("- ", 5, StringComparison.Ordinal);
                while (idx > 0)
                {
                    if (idx > 0 && idx < text.Length - 2)
                    {
                        string before = string.Empty;
                        int k = idx - 1;
                        while (k >= 0 && AllLettersAndNumbers.Contains(text[k]))
                        {
                            before = text[k] + before;
                            k--;
                        }
                        string after = string.Empty;
                        k = idx + 2;
                        while (k < text.Length && AllLetters.Contains(text[k]))
                        {
                            after = after + text[k];
                            k++;
                        }
                        if (after.Length > 0 && after.Equals(before, StringComparison.OrdinalIgnoreCase))
                            text = text.Remove(idx + 1, 1);
                        else if (before.Length > 0)
                        {
                            if ((language == "en" && (after.Equals("and", StringComparison.OrdinalIgnoreCase) || after.Equals("or", StringComparison.OrdinalIgnoreCase))) ||
                                (language == "es" && (after.Equals("y", StringComparison.OrdinalIgnoreCase) || after.Equals("o", StringComparison.OrdinalIgnoreCase))) ||
                                (language == "da" && (after.Equals("og", StringComparison.OrdinalIgnoreCase) || after.Equals("eller", StringComparison.OrdinalIgnoreCase))) ||
                                (language == "de" && (after.Equals("und", StringComparison.OrdinalIgnoreCase) || after.Equals("oder", StringComparison.OrdinalIgnoreCase))) ||
                                (language == "fi" && (after.Equals("ja", StringComparison.OrdinalIgnoreCase) || after.Equals("tai", StringComparison.OrdinalIgnoreCase))) ||
                                (language == "fr" && (after.Equals("et", StringComparison.OrdinalIgnoreCase) || after.Equals("ou", StringComparison.OrdinalIgnoreCase))) ||
                                (language == "it" && (after.Equals("e", StringComparison.OrdinalIgnoreCase) || after.Equals("o", StringComparison.OrdinalIgnoreCase))) ||
                                (language == "nl" && (after.Equals("en", StringComparison.OrdinalIgnoreCase) || after.Equals("of", StringComparison.OrdinalIgnoreCase))) ||
                                (language == "pl" && (after.Equals("i", StringComparison.OrdinalIgnoreCase) || after.Equals("czy", StringComparison.OrdinalIgnoreCase))) ||
                                (language == "pt" && (after.Equals("e", StringComparison.OrdinalIgnoreCase) || after.Equals("ou", StringComparison.OrdinalIgnoreCase))))
                            {
                            }
                            else
                            {
                                text = text.Remove(idx + 1, 1);
                            }
                        }
                    }
                    if (idx + 1 < text.Length && idx != -1)
                        idx = text.IndexOf("- ", idx + 1, StringComparison.Ordinal);
                    else
                        break;
                }
            }

            if (CountTagInText(text, '"') == 2 && text.Contains(" \" "))
            {
                int idx = text.IndexOf(" \" ", StringComparison.Ordinal);
                int idxp = text.IndexOf('"');

                //"Foo " bar.
                if ((idxp >= 0 && idxp < idx) && AllLettersAndNumbers.Contains(text[idx - 1]) && !" \r\n".Contains(text[idxp + 1]))
                {
                    text = text.Remove(idx, 1);
                }

                //" Foo " bar.
                idx = text.IndexOf(" \" ", StringComparison.Ordinal);
                idxp = text.IndexOf('"');
                if (idxp >= 0 && idx > idxp)
                {
                    if (text[idxp + 1] == ' ' && AllLettersAndNumbers.Contains(text[idxp + 2]))
                    {
                        text = text.Remove(idxp + 1, 1);
                        idx--;
                    }
                    text = text.Remove(idx, 1);
                }
            }
            return text;
        }

        public static string RemoveSpaceBeforeAfterTag(string text, string openTag)
        {
            text = HtmlUtil.FixUpperTags(text);
            var closeTag = string.Empty;
            switch (openTag)
            {
                case "<i>":
                    closeTag = "</i>";
                    break;
                case "<b>":
                    closeTag = "</b>";
                    break;
                case "<u>":
                    closeTag = "</u>";
                    break;
            }

            if (closeTag.Length == 0 && openTag.Contains("<font ", StringComparison.Ordinal))
                closeTag = "</font>";

            // Open tags
            var open1 = openTag + " ";
            var open2 = Environment.NewLine + openTag + " ";

            // Closing tags
            var close1 = "! " + closeTag + Environment.NewLine;
            var close2 = "? " + closeTag + Environment.NewLine;
            var close3 = " " + closeTag;
            var close4 = " " + closeTag + Environment.NewLine;

            if (text.Contains(close1, StringComparison.Ordinal))
                text = text.Replace(close1, "!" + closeTag + Environment.NewLine);

            if (text.Contains(close2, StringComparison.Ordinal))
                text = text.Replace(close2, "?" + closeTag + Environment.NewLine);

            if (text.EndsWith(close3, StringComparison.Ordinal))
                text = text.Substring(0, text.Length - close3.Length) + closeTag;

            if (text.Contains(close4))
                text = text.Replace(close4, closeTag + Environment.NewLine);

            // e.g: ! </i><br>Foobar
            if (text.StartsWith(open1, StringComparison.Ordinal))
                text = openTag + text.Substring(open1.Length);

            if (text.Contains(open2, StringComparison.Ordinal))
                text = text.Replace(open2, Environment.NewLine + openTag);

            // Hi <i> bad</i> man! -> Hi <i>bad</i> man!
            text = text.Replace(" " + openTag + " ", " " + openTag);
            text = text.Replace(Environment.NewLine + openTag + " ", Environment.NewLine + openTag);

            // Hi <i>bad </i> man! -> Hi <i>bad</i> man!
            text = text.Replace(" " + closeTag + " ", closeTag + " ");
            text = text.Replace(" " + closeTag + Environment.NewLine, closeTag + Environment.NewLine);

            text = text.Trim();
            if (text.StartsWith(open1, StringComparison.Ordinal))
                text = openTag + text.Substring(open1.Length);

            return text;
        }

        /// <summary>
        /// Creates a task that will complete after a time delay.
        /// </summary>
        /// <param name="millisecondsDelay">The number of milliseconds to wait before completing the returned task.</param>
        /// <returns>A task that represents the time delay.</returns>
        public static Task TaskDelay(int millisecondsDelay)
        {
            var tcs = new TaskCompletionSource<object>();
            var t = new System.Threading.Timer(_ => tcs.SetResult(null));
            t.Change(millisecondsDelay, -1);
            return tcs.Task;
        }

        public static SubtitleFormat LoadMatroskaTextSubtitle(MatroskaTrackInfo matroskaSubtitleInfo, MatroskaFile matroska, List<MatroskaSubtitle> sub, Subtitle subtitle)
        {
            if (subtitle == null)
                throw new ArgumentNullException("subtitle");
            subtitle.Paragraphs.Clear();

            var isSsa = false;
            SubtitleFormat format = new SubRip();
            if (matroskaSubtitleInfo.CodecPrivate.Contains("[script info]", StringComparison.OrdinalIgnoreCase))
            {
                if (matroskaSubtitleInfo.CodecPrivate.Contains("[V4 Styles]", StringComparison.OrdinalIgnoreCase))
                    format = new SubStationAlpha();
                else
                    format = new AdvancedSubStationAlpha();
                isSsa = true;
            }

            if (isSsa)
            {
                foreach (var p in LoadMatroskaSSA(matroskaSubtitleInfo, matroska.Path, format, sub).Paragraphs)
                {
                    subtitle.Paragraphs.Add(p);
                }

                if (!string.IsNullOrEmpty(matroskaSubtitleInfo.CodecPrivate))
                {
                    bool eventsStarted = false;
                    bool fontsStarted = false;
                    bool graphicsStarted = false;
                    var header = new StringBuilder();
                    foreach (string line in matroskaSubtitleInfo.CodecPrivate.Replace(Environment.NewLine, "\n").Split('\n'))
                    {
                        if (!eventsStarted && !fontsStarted && !graphicsStarted)
                        {
                            header.AppendLine(line);
                        }

                        if (line.TrimStart().StartsWith("dialog:", StringComparison.OrdinalIgnoreCase))
                        {
                            eventsStarted = true;
                            fontsStarted = false;
                            graphicsStarted = false;
                        }
                        else if (line.Trim().Equals("[events]", StringComparison.OrdinalIgnoreCase))
                        {
                            eventsStarted = true;
                            fontsStarted = false;
                            graphicsStarted = false;
                        }
                        else if (line.Trim().Equals("[fonts]", StringComparison.OrdinalIgnoreCase))
                        {
                            eventsStarted = false;
                            fontsStarted = true;
                            graphicsStarted = false;
                        }
                        else if (line.Trim().Equals("[graphics]", StringComparison.OrdinalIgnoreCase))
                        {
                            eventsStarted = false;
                            fontsStarted = false;
                            graphicsStarted = true;
                        }
                    }
                    subtitle.Header = header.ToString().TrimEnd();
                    if (!subtitle.Header.Contains("[events]", StringComparison.OrdinalIgnoreCase))
                    {
                        subtitle.Header += Environment.NewLine + Environment.NewLine + "[Events]" + Environment.NewLine;
                    }
                }
            }
            else
            {
                foreach (var p in sub)
                {
                    subtitle.Paragraphs.Add(new Paragraph(p.Text, p.Start, p.End));
                }
            }
            subtitle.Renumber();
            return format;
        }

        public static Subtitle LoadMatroskaSSA(MatroskaTrackInfo matroskaSubtitleInfo, string fileName, SubtitleFormat format, List<MatroskaSubtitle> sub)
        {
            var subtitle = new Subtitle { Header = matroskaSubtitleInfo.CodecPrivate };
            var lines = new List<string>();
            foreach (string l in subtitle.Header.Trim().SplitToLines())
                lines.Add(l);
            var footer = new StringBuilder();
            var comments = new Subtitle();
            if (!string.IsNullOrEmpty(matroskaSubtitleInfo.CodecPrivate))
            {
                bool footerOn = false;
                foreach (string line in lines)
                {
                    if (footerOn)
                    {
                        footer.AppendLine(line);
                    }
                    else if (line.Trim() == "[Events]")
                    {
                    }
                    else if (line.Trim() == "[Fonts]" || line.Trim() == "[Graphics]")
                    {
                        footerOn = true;
                        footer.AppendLine();
                        footer.AppendLine();
                        footer.AppendLine(line);
                    }
                    else if (line.StartsWith("Comment:", StringComparison.Ordinal))
                    {
                        var arr = line.Split(',');
                        if (arr.Length > 3)
                        {
                            arr = arr[1].Split(new[] { ':', '.' });
                            if (arr.Length == 4)
                            {
                                int hour;
                                int min;
                                int sec;
                                int ms;
                                if (int.TryParse(arr[0], out hour) && int.TryParse(arr[1], out min) &&
                                    int.TryParse(arr[2], out sec) && int.TryParse(arr[3], out ms))
                                {
                                    comments.Paragraphs.Add(new Paragraph(new TimeCode(hour, min, sec, ms * 10), new TimeCode(0, 0, 0, 0), line));
                                }
                            }
                        }
                    }
                }
            }
            const string headerFormat = "Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text";
            if (!subtitle.Header.Contains("[Events]"))
            {
                subtitle.Header = subtitle.Header.Trim() + Environment.NewLine +
                                   Environment.NewLine +
                                   "[Events]" + Environment.NewLine +
                                   headerFormat + Environment.NewLine;
            }
            else
            {
                subtitle.Header = subtitle.Header.Remove(subtitle.Header.IndexOf("[Events]", StringComparison.Ordinal));
                subtitle.Header = subtitle.Header.Trim() + Environment.NewLine +
                                   Environment.NewLine +
                                   "[Events]" + Environment.NewLine +
                                   headerFormat + Environment.NewLine;
            }
            lines = new List<string>();
            foreach (string l in subtitle.Header.Trim().SplitToLines())
                lines.Add(l);

            const string timeCodeFormat = "{0}:{1:00}:{2:00}.{3:00}"; // h:mm:ss.cc
            foreach (var mp in sub)
            {
                var p = new Paragraph(string.Empty, mp.Start, mp.End);
                string start = string.Format(timeCodeFormat, p.StartTime.Hours, p.StartTime.Minutes, p.StartTime.Seconds, p.StartTime.Milliseconds / 10);
                string end = string.Format(timeCodeFormat, p.EndTime.Hours, p.EndTime.Minutes, p.EndTime.Seconds, p.EndTime.Milliseconds / 10);

                //MKS contains this: ReadOrder, Layer, Style, Name, MarginL, MarginR, MarginV, Effect, Text

                for (int commentIndex = 0; commentIndex < comments.Paragraphs.Count; commentIndex++)
                {
                    var cp = comments.Paragraphs[commentIndex];
                    if (cp.StartTime.TotalMilliseconds <= p.StartTime.TotalMilliseconds)
                        lines.Add(cp.Text);
                }
                for (int commentIndex = comments.Paragraphs.Count - 1; commentIndex >= 0; commentIndex--)
                {
                    var cp = comments.Paragraphs[commentIndex];
                    if (cp.StartTime.TotalMilliseconds <= p.StartTime.TotalMilliseconds)
                        comments.Paragraphs.RemoveAt(commentIndex);
                }

                string text = mp.Text.Replace(Environment.NewLine, "\\N");
                int idx = text.IndexOf(',') + 1;
                if (idx > 0 && idx < text.Length)
                {
                    text = text.Remove(0, idx); // remove ReadOrder
                    idx = text.IndexOf(',');
                    text = text.Insert(idx, "," + start + "," + end);
                    lines.Add("Dialogue: " + text);
                }
            }
            for (int commentIndex = 0; commentIndex < comments.Paragraphs.Count; commentIndex++)
            {
                var cp = comments.Paragraphs[commentIndex];
                lines.Add(cp.Text);
            }

            foreach (string l in footer.ToString().SplitToLines())
                lines.Add(l);

            format.LoadSubtitle(subtitle, lines, fileName);
            return subtitle;
        }

        public static int GetNumberOfLines(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            int lines = 1;
            int idx = text.IndexOf('\n');
            while (idx >= 0)
            {
                lines++;
                idx = text.IndexOf('\n', idx + 1);
            }
            return lines;
        }

        public static void FixLargeFonts(Control mainCtrl, Control ctrl)
        {
            using (Graphics graphics = mainCtrl.CreateGraphics())
            {
                SizeF textSize = graphics.MeasureString(ctrl.Text, mainCtrl.Font);
                if (textSize.Height > ctrl.Height - 4)
                {
                    int newButtonHeight = (int)(textSize.Height + 7.5);
                    SetButtonHeight(mainCtrl, newButtonHeight, 1);
                }
            }
        }

    }
}
