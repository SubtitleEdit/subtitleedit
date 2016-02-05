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
                    if (rp.TryOpenFile(fileName) && rp.FileType == RiffParser.ckidAVI)
                    {
                        var dh = new RiffDecodeHeader(rp);
                        dh.ProcessMainAVI();
                        info.FileType = RiffParser.FromFourCC(rp.FileType);
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
            char nextChar;
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
                            var r = new Regex(node.InnerText, RegexOptions.Compiled);
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
            var idx = s.IndexOf(Environment.NewLine + "</", StringComparison.Ordinal);
            if (idx > 2)
            {
                var endIdx = s.IndexOf('>', idx + 2);
                if (endIdx > idx)
                {
                    var tag = s.Substring(idx + Environment.NewLine.Length, endIdx - (idx + Environment.NewLine.Length) + 1);
                    s = s.Insert(idx, tag);
                    s = s.Remove(idx + tag.Length + Environment.NewLine.Length, tag.Length);
                }
            }
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
            var max = Math.Max(parts1.Length, parts2.Length);
            while (i < max && i1 < parts1.Length && i2 < parts2.Length)
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
            for (; startIndex < parts.Length; startIndex++)
            {
                if (s == parts[startIndex])
                    return startIndex;
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

        private static readonly Regex RemoveSpaceBetweenNumbersRegex = new Regex(@"(?<=\b\d+) \d(?!/\d)", RegexOptions.Compiled);

        public static string RemoveSpaceBetweenNumbers(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var match = RemoveSpaceBetweenNumbersRegex.Match(text);
                while (match.Success)
                {
                    text = text.Remove(match.Index, 1);
                    match = RemoveSpaceBetweenNumbersRegex.Match(text, match.Index);
                }
            }
            return text;
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

            if (text.Contains(" . "))
            {
                var regex = new Regex(@"[a-z] \. [A-Z]");
                var match = regex.Match(text);
                while (match.Success)
                {
                    text = text.Remove(match.Index + 1, 1);
                    match = regex.Match(text);
                }
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
            var open3 = openTag + Environment.NewLine;

            // Closing tags
            var close1 = "! " + closeTag + Environment.NewLine;
            var close2 = "? " + closeTag + Environment.NewLine;
            var close3 = " " + closeTag;
            var close4 = " " + closeTag + Environment.NewLine;
            var close5 = Environment.NewLine + closeTag;

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

            // e.g.: <i>\r\n
            if (text.StartsWith(open3, StringComparison.Ordinal))
                text = text.Remove(openTag.Length, Environment.NewLine.Length);

            // e.g.: \r\n</i>
            if (text.EndsWith(close5, StringComparison.Ordinal))
                text = text.Remove(text.Length - openTag.Length - Environment.NewLine.Length - 1, Environment.NewLine.Length);

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

        public static string Sha256Hash(string value)
        {
            using (var hasher = new System.Security.Cryptography.SHA256Managed())
            {
                var bytes = Encoding.UTF8.GetBytes(value);
                var hash = hasher.ComputeHash(bytes);
                return Convert.ToBase64String(hash, 0, hash.Length);
            }
        }

    }
}
