using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Forms;
using Nikse.SubtitleEdit.Logic.SubtitleFormats;
using Nikse.SubtitleEdit.Logic.VideoFormats;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
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

namespace Nikse.SubtitleEdit.Logic
{
    public static class Utilities
    {
        public const string WinXP2KUnicodeFontName = "Times New Roman";

        /// <summary>
        /// Cached environment new line characters for faster lookup.
        /// </summary>
        internal static readonly char[] NewLineChars = Environment.NewLine.ToCharArray();

        public static byte[] ReadAllBytes(String path)
        {
            byte[] bytes;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int index = 0;
                long fileLength = fs.Length;
                if (fileLength > Int32.MaxValue)
                    throw new IOException("File too long");
                int count = (int)fileLength;
                bytes = new byte[count];
                while (count > 0)
                {
                    int n = fs.Read(bytes, index, count);
                    if (n == 0)
                        throw new InvalidOperationException("End of file reached before expected");
                    index += n;
                    count -= n;
                }
            }
            return bytes;
        }

        public static VideoInfo GetVideoInfo(string fileName)
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
                    info.TotalSeconds = info.TotalMilliseconds / 1000.0;
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
                var mp4Parser = new Mp4.MP4Parser(fileName);
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
            if (includeAudioFiles && !string.IsNullOrEmpty(Configuration.Settings.Language.General.AudioFiles))
            {
                sb.Append('|');
                sb.Append(Configuration.Settings.Language.General.AudioFiles);
                sb.Append("|*.mp3;*.wav;*.wma;*.ogg;*.mpa;*.m4a;*.ape;*.aiff;*.flac;*.aac");
            }
            sb.Append('|');
            sb.Append(Configuration.Settings.Language.General.AllFiles);
            sb.Append("|*.*");
            return sb.ToString();
        }

        public static bool IsInteger(string s)
        {
            int i;
            if (int.TryParse(s, out i))
                return true;
            return false;
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

        public static int GetSubtitleIndex(List<Paragraph> paragraphs, VideoPlayerContainer videoPlayerContainer)
        {
            if (videoPlayerContainer.VideoPlayer != null)
            {
                double positionInMilliseconds = (videoPlayerContainer.VideoPlayer.CurrentPosition * 1000.0) + 5;
                for (int i = 0; i < paragraphs.Count; i++)
                {
                    var p = paragraphs[i];
                    if (p.StartTime.TotalMilliseconds <= positionInMilliseconds && p.EndTime.TotalMilliseconds > positionInMilliseconds)
                    {
                        bool isInfo = p == paragraphs[0] && (p.StartTime.TotalMilliseconds == 0 && p.Duration.TotalMilliseconds == 0 || p.StartTime.TotalMilliseconds == Pac.PacNullTime.TotalMilliseconds);
                        if (!isInfo)
                            return i;
                    }
                }
                if (!string.IsNullOrEmpty(videoPlayerContainer.SubtitleText))
                    videoPlayerContainer.SetSubtitleText(string.Empty, null);
            }
            return -1;
        }

        public static int ShowSubtitle(List<Paragraph> paragraphs, VideoPlayerContainer videoPlayerContainer)
        {
            if (videoPlayerContainer.VideoPlayer != null)
            {
                double positionInMilliseconds = (videoPlayerContainer.CurrentPosition * 1000.0) + 5;
                for (int i = 0; i < paragraphs.Count; i++)
                {
                    var p = paragraphs[i];
                    if (p.StartTime.TotalMilliseconds <= positionInMilliseconds &&
                        p.EndTime.TotalMilliseconds > positionInMilliseconds)
                    {
                        string text = p.Text.Replace("|", Environment.NewLine);
                        bool isInfo = p == paragraphs[0] && (p.StartTime.TotalMilliseconds == 0 && p.Duration.TotalMilliseconds == 0 || p.StartTime.TotalMilliseconds == Pac.PacNullTime.TotalMilliseconds);
                        if (!isInfo)
                        {
                            if (videoPlayerContainer.LastParagraph != p)
                                videoPlayerContainer.SetSubtitleText(text, p);
                            else if (videoPlayerContainer.SubtitleText != text)
                                videoPlayerContainer.SetSubtitleText(text, p);
                            return i;
                        }
                    }
                }
                if (!string.IsNullOrEmpty(videoPlayerContainer.SubtitleText))
                    videoPlayerContainer.SetSubtitleText(string.Empty, null);
            }
            return -1;
        }

        public static int ShowSubtitle(List<Paragraph> paragraphs, Subtitle original, VideoPlayerContainer videoPlayerContainer)
        {
            if (videoPlayerContainer.VideoPlayer != null)
            {
                double positionInMilliseconds = (videoPlayerContainer.VideoPlayer.CurrentPosition * 1000.0) + 15;
                for (int i = 0; i < paragraphs.Count; i++)
                {
                    var p = paragraphs[i];
                    if (p.StartTime.TotalMilliseconds <= positionInMilliseconds &&
                        p.EndTime.TotalMilliseconds > positionInMilliseconds)
                    {
                        var op = Utilities.GetOriginalParagraph(0, p, original.Paragraphs);

                        string text = p.Text.Replace("|", Environment.NewLine);
                        if (op != null)
                            text = text + Environment.NewLine + Environment.NewLine + op.Text.Replace("|", Environment.NewLine);

                        bool isInfo = p == paragraphs[0] && p.StartTime.TotalMilliseconds == 0 && positionInMilliseconds > 3000;
                        if (!isInfo)
                        {
                            if (videoPlayerContainer.LastParagraph != p)
                                videoPlayerContainer.SetSubtitleText(text, p);
                            else if (videoPlayerContainer.SubtitleText != text)
                                videoPlayerContainer.SetSubtitleText(text, p);
                            return i;
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(videoPlayerContainer.SubtitleText))
                videoPlayerContainer.SetSubtitleText(string.Empty, null);
            return -1;
        }

        /// <summary>
        /// Downloads the requested resource as a <see cref="String"/> using the configured <see cref="WebProxy"/>.
        /// </summary>
        /// <param name="address">A <see cref="String"/> containing the URI to download.</param>
        /// <returns>A <see cref="String"/> containing the requested resource.</returns>
        public static string DownloadString(string address)
        {
            using (var wc = new WebClient())
            {
                wc.Proxy = GetProxy();
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
            if (string.IsNullOrWhiteSpace(s))
                return false;

            if (position + 2 > s.Length)
                return false;

            if (@",.".Contains(s[position]))
            {
                if (position > 0 && position < s.Length - 1)
                {
                    return char.IsDigit(s[position - 1]) && char.IsDigit(s[position + 1]);
                }
            }
            return false;
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
            if (index < s.Length)
                nextChar = s[index];
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

        private static string _lastNoBreakAfterListLanguage = null;
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

            var arr = s.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
            if ((arr.Length < 2 && arr[0].Length <= maximumLineLength) || (arr[0].Length <= maximumLineLength && arr[1].Length <= maximumLineLength))
                return s;

            s = s.Replace("</i> " + Environment.NewLine + "<i>", " ");
            s = s.Replace("</i>" + Environment.NewLine + " <i>", " ");
            s = s.Replace("</i>" + Environment.NewLine + "<i>", " ");
            s = s.Replace(Environment.NewLine, " ");
            s = s.Replace("   ", " ");
            s = s.Replace("  ", " ");
            s = s.Replace("  ", " ");
            s = s.Replace("  ", " ");

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
                        s = s.Replace(Environment.NewLine + "</i>", "<i>" + Environment.NewLine);
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
                if (currentCount + word.Length + 3 > average)
                {
                    if (currentIdx < count)
                    {
                        list.Add(currentCount);
                        currentIdx++;
                        currentCount = 0;
                    }
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
                string dialogS = Utilities.RemoveHtmlTags(text);
                var arr = dialogS.Replace(Environment.NewLine, "\n").Split('\n');
                if (arr.Length == 2)
                {
                    string arr0 = arr[0].Trim().TrimEnd('"').TrimEnd('\'').TrimEnd();
                    if (arr0.StartsWith('-') && arr[1].TrimStart().StartsWith('-') && (arr0.EndsWith('.') || arr0.EndsWith('!') || arr0.EndsWith('?')))
                        return text;
                }
            }

            string s = text;
            s = s.Replace("</i> " + Environment.NewLine + "<i>", " ");
            s = s.Replace("</i>" + Environment.NewLine + " <i>", " ");
            s = s.Replace("</i>" + Environment.NewLine + "<i>", " ");
            s = s.Replace(Environment.NewLine, " ");
            s = s.Replace("   ", " ");
            s = s.Replace("  ", " ");
            s = s.Replace("  ", " ");
            s = s.Replace("  ", " ");

            string temp = RemoveHtmlTags(s, true);

            if (temp.Length < mergeLinesShorterThan)
            {
                string[] lines = text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1)
                {
                    bool isDialog = true;
                    foreach (string line in lines)
                    {
                        string cleanLine = RemoveHtmlTags(line).Trim();
                        isDialog = isDialog && (cleanLine.StartsWith('-') || cleanLine.StartsWith('—'));
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
            if (splitPos == -1 && s.Contains(" - "))
            {
                for (int j = 0; j < (maximumLength / 2) + 5; j++)
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

            if (splitPos == -1)
            {
                for (int j = 0; j < 15; j++)
                {
                    if (mid + j + 1 < s.Length && mid + j > 0)
                    {
                        if (@".!?".Contains(s[mid + j]) && !IsPartOfNumber(s, mid + j) && CanBreak(s, mid + j + 1, language))
                        {
                            splitPos = mid + j + 1;
                            if (@".!?0123456789".Contains(s[splitPos]))
                            { // do not break double/tripple end lines like "!!!" or "..."
                                splitPos++;
                                if (@".!?0123456789".Contains(s[mid + j + 1]))
                                    splitPos++;
                            }
                            break;
                        }
                        if (@".!?".Contains(s[mid - j]) && !IsPartOfNumber(s, mid - j) && CanBreak(s, mid - j, language))
                        {
                            splitPos = mid - j;
                            splitPos++;
                            break;
                        }
                    }
                }
            }

            if (splitPos > maximumLength) // too long first line
                splitPos = -1;
            else if (splitPos >= 0 && s.Length - splitPos > maximumLength) // too long second line
                splitPos = -1;

            if (splitPos == -1)
            {
                for (int j = 0; j < 25; j++)
                {
                    if (mid + j + 1 < s.Length && mid + j > 0)
                    {
                        if (@".!?, ".Contains(s[mid + j]) && !IsPartOfNumber(s, mid + j) && s.Length > mid + j + 2 && CanBreak(s, mid + j, language))
                        {
                            splitPos = mid + j;
                            if (@" .!?".Contains(s[mid + j + 1]))
                            {
                                splitPos++;
                                if (@" .!?".Contains(s[mid + j + 2]))
                                    splitPos++;
                            }
                            break;
                        }
                        if (@".!?, ".Contains(s[mid - j]) && !IsPartOfNumber(s, mid - j) && s.Length > mid + j + 2 && CanBreak(s, mid - j, language))
                        {
                            splitPos = mid - j;
                            if (@".!?".Contains(s[splitPos]))
                                splitPos--;
                            if (@".!?".Contains(s[splitPos]))
                                splitPos--;
                            if (@".!?".Contains(s[splitPos]))
                                splitPos--;
                            break;
                        }
                    }
                }
            }

            if (splitPos == -1)
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

        private static string ReInsertHtmlTags(string s, Dictionary<int, string> htmlTags)
        {
            if (htmlTags.Count > 0)
            {
                var sb = new StringBuilder();
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
                s = sb.ToString();
            }
            return s;
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

        public static string RemoveHtmlTags(string s)
        {
            if (s == null)
                return null;

            if (!s.Contains('<'))
                return s;

            if (s.Contains("< "))
                s = FixInvalidItalicTags(s);

            return HtmlUtils.RemoveOpenCloseTags(s, HtmlUtils.TagItalic, HtmlUtils.TagBold, HtmlUtils.TagUnderline, HtmlUtils.TagParagraph, HtmlUtils.TagFont, HtmlUtils.TagCyrillicI);
        }

        public static string RemoveHtmlTags(string s, bool alsoSsaTags)
        {
            if (s == null)
                return null;

            s = RemoveHtmlTags(s);

            if (alsoSsaTags)
                s = RemoveSsaTags(s);

            return s;
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
                        k = -1;
                }
                else
                {
                    k = -1;
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
                        else if (couldBeUtf8 && fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) && Encoding.Default.GetString(buffer).ToLower().Replace("'", "\"").Contains("encoding=\"utf-8\""))
                        { // keep utf-8 encoding for xml files with utf-8 in header (without any utf-8 encoded characters, but with only allowed utf-8 characters)
                            encoding = Encoding.UTF8;
                        }
                        else if (Configuration.Settings.General.AutoGuessAnsiEncoding)
                        {
                            encoding = DetectAnsiEncoding(buffer);

                            Encoding greekEncoding = Encoding.GetEncoding(1253); // Greek
                            if (GetCount(greekEncoding.GetString(buffer), "μου", "είναι", "Είναι", "αυτό", "Τόμπυ", "καλά") > 5)
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
                            if (GetCount(hewbrewEncoding.GetString(buffer), "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב") > 5)
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
                if (GetCount(greekEncoding.GetString(buffer), "μου", "είναι", "Είναι", "αυτό", "Τόμπυ", "καλά") > 5)
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
                if (GetCount(hewbrewEncoding.GetString(buffer), "אתה", "אולי", "הוא", "בסדר", "יודע", "טוב") > 5)
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
                    if (!name.StartsWith("hyph", StringComparison.Ordinal))
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

        public static double GetOptimalDisplayMilliseconds(string text)
        {
            return GetOptimalDisplayMilliseconds(text, Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds);
        }

        public static double GetOptimalDisplayMilliseconds(string text, double charactersPerSecond)
        {
            double optimalCharactersPerSecond = charactersPerSecond;
            if (optimalCharactersPerSecond < 2 || optimalCharactersPerSecond > 100)
                optimalCharactersPerSecond = 14.7;
            double duration = (RemoveHtmlTags(text, true).Length / optimalCharactersPerSecond) * 1000.0;

            if (duration < Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds)
                duration = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;

            if (duration > Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds)
                duration = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;

            return duration;
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

        private static int GetCountContains(string text,
                            string word1,
                            string word2,
                            string word3,
                            string word4,
                            string word5,
                            string word6)
        {
            var regEx1 = new Regex(word1);
            var regEx2 = new Regex(word2);
            var regEx3 = new Regex(word3);
            var regEx4 = new Regex(word4);
            var regEx5 = new Regex(word5);
            var regEx6 = new Regex(word6);
            int count = regEx1.Matches(text).Count;
            count += regEx2.Matches(text).Count;
            count += regEx3.Matches(text).Count;
            count += regEx4.Matches(text).Count;
            count += regEx5.Matches(text).Count;
            count += regEx6.Matches(text).Count;
            return count;
        }

        public static string AutoDetectGoogleLanguage(Encoding encoding)
        {
            if (encoding.CodePage == 860)
                return "pt"; // Portuguese
            if (encoding.CodePage == 949)
                return "ko"; // Korean
            if (encoding.CodePage == 950)
                return "zh"; // Chinese
            if (encoding.CodePage == 1253)
                return "el"; // Greek
            if (encoding.CodePage == 1254)
                return "tr"; // Turkish
            if (encoding.CodePage == 1255)
                return "he"; // Hebrew
            if (encoding.CodePage == 1256)
                return "ar"; // Arabic
            if (encoding.CodePage == 1258)
                return "vi"; // Vietnamese
            if (encoding.CodePage == 1361)
                return "ko"; // Korean
            if (encoding.CodePage == 10001)
                return "ja"; // Japanese
            if (encoding.CodePage == 20000)
                return "zh"; // Chinese
            if (encoding.CodePage == 20002)
                return "zh"; // Chinese
            if (encoding.CodePage == 20932)
                return "ja"; // Japanese
            if (encoding.CodePage == 20936)
                return "zh"; // Chinese
            if (encoding.CodePage == 20949)
                return "ko"; // Korean
            if (encoding.CodePage == 28596)
                return "ar"; // Arabic
            if (encoding.CodePage == 28597)
                return "el"; // Greek
            if (encoding.CodePage == 28598)
                return "he"; // Hebrew
            if (encoding.CodePage == 28599)
                return "tr"; // Turkish
            if (encoding.CodePage == 50220)
                return "ja"; // Japanese
            if (encoding.CodePage == 50221)
                return "ja"; // Japanese
            if (encoding.CodePage == 50222)
                return "ja"; // Japanese
            if (encoding.CodePage == 50225)
                return "ko"; // Korean
            if (encoding.CodePage == 51932)
                return "ja"; // Japanese
            if (encoding.CodePage == 51936)
                return "zh"; // Chinese
            if (encoding.CodePage == 51949)
                return "ko"; // Korean
            if (encoding.CodePage == 52936)
                return "zh"; // Chinese
            if (encoding.CodePage == 54936)
                return "zh"; // Chinese
            return null;
        }

        public static string AutoDetectGoogleLanguage(string text, int bestCount)
        {
            int count = GetCount(text, "we", "are", "and", "you", "your", "what");
            if (count > bestCount)
                return "en";

            count = GetCount(text, "vi", "han", "og", "jeg", "var", "men") + GetCount(text, "gider", "bliver", "virkelig", "kommer", "tilbage", "Hej");
            if (count > bestCount)
            {
                int norwegianCount = GetCount(text, "ut", "deg", "meg", "merkelig", "mye", "spørre");
                int dutchCount = GetCount(text, "van", "het", "een", "Het", "mij", "zijn");
                if (norwegianCount < 2 && dutchCount < count)
                    return "da";
            }

            count = GetCount(text, "vi", "er", "og", "jeg", "var", "men");
            if (count > bestCount)
            {
                int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                int dutchCount = GetCount(text, "van", "het", "een", "Het", "mij", "zijn");
                if (danishCount < 2 && dutchCount < count)
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
                int romanianWords = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau");
                if (spanishWords < 2 && italianWords < 2 && romanianWords < 5)
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

            count = GetCount(text, "Cosa", "sono", "Grazie", "Buongiorno", "bene", "questo") + GetCount(text, "ragazzi", "propriamente", "numero", "hanno", "giorno", "faccio") +
                    GetCount(text, "davvero", "negativo", "essere", "vuole", "sensitivo", "venire");

            if (count > bestCount)
            {
                int frenchWords = GetCount(text, "Cest", "cest", "pas", "vous", "pour", "suis"); // not spanish words
                int spanishWords = GetCount(text, "Hola", "nada", "Vamos", "pasa", "los", "como"); // not french words
                if (frenchWords < 2 && spanishWords < 2)
                    return "it";
            }

            count = GetCount(text, "não", "Não", "Estás", "Então", "isso", "com");
            if (count > bestCount)
                return "pt"; // Portuguese

            count = GetCount(text, "μου", "είναι", "Είναι", "αυτό", "Τόμπυ", "καλά") +
                    GetCount(text, "Ενταξει", "Ενταξει", "πρεπει", "Λοιπον", "τιποτα", "ξερεις");
            if (count > bestCount)
                return "el"; // Greek

            count = GetCount(text, "все", "это", "как", "Воробей", "сюда", "Давай");
            if (count > bestCount)
                return "ru"; // Russian

            count = GetCount(text, "Какво", "тук", "може", "Как", "Ваше", "какво");
            if (count > bestCount)
                return "bg"; // Bulgarian

            count = GetCount(text, "sam", "öto", "äto", "ovo", "vas", "što");
            if (count > bestCount && GetCount(text, "htjeti ", "htjeti ", "htjeti ", "htjeti ", "htjeti ", "htjeti ") > 0)
                return "hr"; // Croatia

            count = GetCount(text, "من", "هل", "لا", "فى", "لقد", "ما");
            if (count > bestCount)
            {
                if (GetCount(text, "אולי", "אולי", "אולי", "אולי", "טוב", "טוב") > 10)
                    return "he";

                int countRo = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau") +
                                   GetCount(text, "trãiascã", "niciodatã", "înseamnã", "vorbesti", "oamenii", "Asteaptã") +
                                   GetCount(text, "fãcut", "Fãrã", "spune", "decât", "pentru", "vreau");
                if (countRo > count)
                    return "ro"; // Romanian

                count = GetCount(text, "daca", "pentru", "acum", "soare", "trebuie", "Trebuie") +
                        GetCount(text, "nevoie", "decat", "echilibrul", "vorbesti", "oamenii", "zeului") +
                        GetCount(text, "vrea", "atunci", "Poate", "Acum", "memoria", "soarele");
                if (countRo > count)
                    return "ro"; // Romanian

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

            count = GetCount(text, "hogy", "lesz", "tudom", "vagy", "mondtam", "még");
            if (count > bestCount)
                return "hu"; // Hungarian

            count = GetCount(text, "için", "Tamam", "Hayır", "benim", "daha", "deðil") + GetCount(text, "önce", "lazým", "benim", "çalýþýyor", "burada", "efendim");
            if (count > bestCount)
                return "tr"; // Turkish

            count = GetCount(text, "yang", "tahu", "bisa", "akan", "tahun", "tapi") + GetCount(text, "dengan", "untuk", "rumah", "dalam", "sudah", "bertemu");
            if (count > bestCount)
                return "id"; // Indonesian

            count = GetCount(text, "โอ", "โรเบิร์ต", "วิตตอเรีย", "ดร", "คุณตำรวจ", "ราเชล") + GetCount(text, "ไม่", "เลดดิส", "พระเจ้า", "เท็ดดี้", "หัวหน้า", "แอนดรูว์");
            if (count > 10 || count > bestCount)
                return "th"; // Thai

            count = GetCount(text, "그리고", "아니야", "하지만", "말이야", "그들은", "우리가");
            if (count > 10 || count > bestCount)
                return "ko"; // Korean

            count = GetCount(text, "että", "kuin", "minä", "mitään", "Mutta", "siitä") + GetCount(text, "täällä", "poika", "Kiitos", "enää", "vielä", "tässä");
            if (count > bestCount)
                return "fi"; // Finnish

            count = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau") +
                    GetCount(text, "trãiascã", "niciodatã", "înseamnã", "vorbesti", "oamenii", "Asteaptã") +
                    GetCount(text, "fãcut", "Fãrã", "spune", "decât", "pentru", "vreau");
            if (count > bestCount)
                return "ro"; // Romanian

            count = GetCount(text, "daca", "pentru", "acum", "soare", "trebuie", "Trebuie") +
                    GetCount(text, "nevoie", "decat", "echilibrul", "vorbesti", "oamenii", "zeului") +
                    GetCount(text, "vrea", "atunci", "Poate", "Acum", "memoria", "soarele");
            if (count > bestCount)
                return "ro"; // Romanian

            count = GetCountContains(text, "シ", "ュ", "シン", "シ", "ン", "ユ");
            count += GetCountContains(text, "イ", "ン", "チ", "ェ", "ク", "ハ");
            count += GetCountContains(text, "シ", "ュ", "う", "シ", "ン", "サ");
            count += GetCountContains(text, "シ", "ュ", "シ", "ン", "だ", "う");
            if (count > bestCount * 2)
                return "jp"; // Japanese - not tested...

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
            int bestCount = subtitle.Paragraphs.Count / 14;

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
                sb.AppendLine(p.Text);
            string text = sb.ToString();

            string languageId = AutoDetectGoogleLanguage(text, bestCount);

            if (string.IsNullOrEmpty(languageId))
                return "en";

            return languageId;
        }

        public static string AutoDetectGoogleLanguageOrNull(Subtitle subtitle)
        {
            int bestCount = subtitle.Paragraphs.Count / 14;

            var sb = new StringBuilder();
            foreach (Paragraph p in subtitle.Paragraphs)
                sb.AppendLine(p.Text);
            string text = sb.ToString();

            string languageId = AutoDetectGoogleLanguage(text, bestCount);

            if (string.IsNullOrEmpty(languageId))
                return null;

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
            foreach (string name in dictionaryNames)
            {
                if (name.Contains("[en_GB]"))
                    containsEnGb = true;
                if (name.Contains("[en_US]"))
                    containsEnUs = true;
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
                        count = GetCount(text, "vi", "hun", "og", "jeg", "var", "men") + GetCount(text, "bliver", "meget", "spørger", "Hej", "utrolig", "dejligt");
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
                            int danishCount = GetCount(text, "siger", "dig", "mig", "mærkelig", "tilbage", "spørge");
                            int dutchCount = GetCount(text, "van", "het", "een", "Het", "mij", "zijn");
                            if (danishCount < 2 && dutchCount < count)
                                languageName = shortName;
                        }
                        break;
                    case "en_US":
                        count = GetCount(text, "we", "are", "and", "you", "your", "what");
                        if (count > bestCount)
                        {
                            if (containsEnGb)
                            {
                                int usCount = GetCount(text, "color", "flavor", "honor", "humor", "neighbor", "honor");
                                int gbCount = GetCount(text, "colour", "flavour", "honour", "humour", "neighbour", "honour");
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
                        break;
                    case "en_GB":
                        count = GetCount(text, "we", "are", "and", "you", "your", "what");
                        if (count > bestCount)
                        {
                            if (containsEnUs)
                            {
                                int usCount = GetCount(text, "color", "flavor", "honor", "humor", "neighbor", "honor");
                                int gbCount = GetCount(text, "colour", "flavour", "honour", "humour", "neighbour", "honour");
                                if (usCount >= gbCount)
                                    languageName = "en_US";
                                else
                                    languageName = "en_GB";
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
                        count = GetCount(text, "sînt", "aici", "Sînt", "domnule", "pentru", "Vreau") +
                                GetCount(text, "trãiascã", "niciodatã", "înseamnã", "vorbesti", "oamenii", "Asteaptã") +
                                GetCount(text, "fãcut", "Fãrã", "spune", "decât", "pentru", "vreau");
                        if (count > bestCount)
                        {
                            languageName = shortName;
                        }
                        else
                        {
                            count = GetCount(text, "daca", "pentru", "acum", "soare", "trebuie", "Trebuie") +
                                    GetCount(text, "nevoie", "decat", "echilibrul", "vorbesti", "oamenii", "zeului") +
                                    GetCount(text, "vrea", "atunci", "Poate", "Acum", "memoria", "soarele");

                            if (count > bestCount)
                                languageName = shortName;
                        }
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
            foreach (string line in text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries))
            {
                string s = RemoveHtmlTags(line, true);
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

            string s = Utilities.RemoveHtmlTags(paragraph.Text, true).Replace(Environment.NewLine, string.Empty).Replace(zeroWhiteSpace, string.Empty).Replace(zeroWidthNoBreakSpace, string.Empty);
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
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public static string AssemblyDescription
        {
            get
            {
                Assembly assy = Assembly.GetExecutingAssembly();
                String assyName = assy.GetName().Name;
                bool isdef = Attribute.IsDefined(assy, typeof(AssemblyDescriptionAttribute));
                if (isdef)
                {
                    Console.WriteLine(assyName);
                    AssemblyDescriptionAttribute adAttr = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assy, typeof(AssemblyDescriptionAttribute));
                    if (adAttr != null)
                        return adAttr.Description;
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
            StringBuilder sb = new StringBuilder();
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
            AddExtension(sb, ".sup");
            AddExtension(sb, ".dost");

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

        public static bool IsQuartsDllInstalled
        {
            get
            {
                if (IsRunningOnMono())
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
                if (Configuration.IsRunningOnLinux() || IsRunningOnMono())
                    return File.Exists(Path.Combine(Configuration.BaseDirectory, "mplayer"));

                return MPlayer.GetMPlayerFileName != null;
            }
        }

        public static bool IsMpcHcInstalled
        {
            get
            {
                if (IsRunningOnMono())
                    return false;

                try
                {
                    return VideoPlayers.MpcHC.MpcHc.GetMpcHcFileName() != null;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
            }
        }

        public static VideoPlayer GetVideoPlayer()
        {
            GeneralSettings gs = Configuration.Settings.General;

            if (Configuration.IsRunningOnLinux() || Configuration.IsRunningOnMac())
                return new MPlayer();

            //if (Utilities.IsRunningOnMac())
            //    return new LibVlcMono();

            if (gs.VideoPlayer == "VLC" && LibVlcDynamic.IsInstalled)
                return new LibVlcDynamic();

            //if (gs.VideoPlayer == "WindowsMediaPlayer" && IsWmpAvailable)
            //    return new WmpPlayer();
            //if (gs.VideoPlayer == "ManagedDirectX" && IsManagedDirectXInstalled)
            //    return new ManagedDirectXPlayer();

            if (gs.VideoPlayer == "MPlayer" && MPlayer.IsInstalled)
                return new MPlayer();

            if (gs.VideoPlayer == "MPC-HC" && VideoPlayers.MpcHC.MpcHc.IsInstalled)
                return new VideoPlayers.MpcHC.MpcHc();

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
                videoPlayerContainer.ShowFullscreenButton = false;
                videoPlayerContainer.ShowMuteButton = Configuration.Settings.General.VideoPlayerShowMuteButton;
                videoPlayerContainer.Volume = Configuration.Settings.General.VideoPlayerDefaultVolume;
                videoPlayerContainer.EnableMouseWheelStep();
                videoPlayerContainer.VideoWidth = videoInfo.Width;
                videoPlayerContainer.VideoHeight = videoInfo.Height;
                videoPlayerContainer.VideoPlayer.Resize(videoPlayerContainer.PanelPlayer.Width, videoPlayerContainer.PanelPlayer.Height);
            }
            catch (Exception exception)
            {
                videoPlayerContainer.VideoPlayer = null;
                var videoError = new VideoError();
                videoError.Initialize(fileName, videoInfo, exception);
                videoError.ShowDialog();
            }
        }

        public static void GetLineLengths(Label label, string text)
        {
            label.ForeColor = Color.Black;
            string cleanText = Utilities.RemoveHtmlTags(text, true).Replace(Environment.NewLine, "|");
            string[] lines = cleanText.Split('|');

            const int max = 3;

            StringBuilder sb = new StringBuilder();
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
                    label.ForeColor = Color.Orange;
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
                Regex.Match("", testPattern);
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
            int start = regEx.IndexOf("(?<", StringComparison.Ordinal);
            if (start >= 0 && regEx.IndexOf(')') > start)
            {
                int end = regEx.IndexOf('>');
                if (end > start)
                {
                    start += 3;
                    string group = regEx.Substring(start, end - start);
                    return group;
                }
            }
            return null;
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
                        SetButtonHeight(subControl, newHeight, level++);
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
                if (index == text.Length)
                    return count;
                index = text.IndexOf(tag, index + 1, StringComparison.Ordinal);
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
                if (index == text.Length)
                    return count;
                index = text.IndexOf(tag, index + 1);
            }
            return count;
        }

        public static string FixInvalidItalicTags(string text)
        {
            const string beginTag = "<i>";
            const string endTag = "</i>";

            text = text.Replace("< i >", beginTag);
            text = text.Replace("< i>", beginTag);
            text = text.Replace("<i >", beginTag);
            text = text.Replace("< I>", beginTag);
            text = text.Replace("<I >", beginTag);

            text = text.Replace("< / i >", endTag);
            text = text.Replace("< /i>", endTag);
            text = text.Replace("</ i>", endTag);
            text = text.Replace("< /i>", endTag);
            text = text.Replace("< /i >", endTag);
            text = text.Replace("</i >", endTag);
            text = text.Replace("</ i >", endTag);
            text = text.Replace("< / i>", endTag);
            text = text.Replace("< /I>", endTag);
            text = text.Replace("</ I>", endTag);
            text = text.Replace("< /I>", endTag);
            text = text.Replace("< / I >", endTag);

            text = text.Replace("</i> <i>", "_@_");
            text = text.Replace(" _@_", "_@_");
            text = text.Replace(" _@_ ", "_@_");
            text = text.Replace("_@_", " ");

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
                    if (text.IndexOf(beginTag, StringComparison.Ordinal) > text.IndexOf(endTag, StringComparison.Ordinal))
                    {
                        text = text.Replace(beginTag, "___________@");
                        text = text.Replace(endTag, beginTag);
                        text = text.Replace("___________@", endTag);
                    }
                }

                if (italicBeginTagCount == 2 && italicEndTagCount == 0)
                {
                    int firstIndex = text.IndexOf(beginTag, StringComparison.Ordinal);
                    int lastIndex = text.LastIndexOf(beginTag, StringComparison.Ordinal);
                    int lastIndexWithNewLine = text.LastIndexOf(Environment.NewLine + beginTag, StringComparison.Ordinal) + Environment.NewLine.Length;
                    if (noOfLines == 2 && lastIndex == lastIndexWithNewLine && firstIndex < 2)
                        text = text.Replace(Environment.NewLine, "</i>" + Environment.NewLine) + "</i>";
                    else if (text.Length > lastIndex + endTag.Length)
                        text = text.Substring(0, lastIndex) + endTag + text.Substring(lastIndex - 1 + endTag.Length);
                    else
                        text = text.Substring(0, lastIndex) + endTag;
                }

                if (italicBeginTagCount == 1 && italicEndTagCount == 2)
                {
                    int firstIndex = text.IndexOf(endTag, StringComparison.Ordinal);
                    if (text.StartsWith("</i>-<i>-", StringComparison.Ordinal))
                        text = text.Remove(0, 5);
                    else if (text.StartsWith("</i>- <i>-", StringComparison.Ordinal))
                        text = text.Remove(0, 5);
                    else if (text.StartsWith("</i>- <i> -", StringComparison.Ordinal))
                        text = text.Remove(0, 5);
                    else if (text.StartsWith("</i>-<i> -", StringComparison.Ordinal))
                        text = text.Remove(0, 5);
                    else if (firstIndex == 0)
                        text = text.Remove(0, 4);
                    else
                        text = text.Substring(0, firstIndex) + text.Substring(firstIndex + endTag.Length);
                }

                if (italicBeginTagCount == 2 && italicEndTagCount == 1)
                {
                    var lines = text.Replace(Environment.NewLine, "\n").Split('\n');
                    if (lines.Length == 2 && lines[0].StartsWith("<i>", StringComparison.Ordinal) && lines[0].EndsWith("</i>", StringComparison.Ordinal) &&
                        lines[1].StartsWith("<i>", StringComparison.Ordinal))
                    {
                        text = text.TrimEnd() + "</i>";
                    }
                    else
                    {
                        int lastIndex = text.LastIndexOf(beginTag, StringComparison.Ordinal);
                        if (text.Length > lastIndex + endTag.Length)
                            text = text.Substring(0, lastIndex) + text.Substring(lastIndex - 1 + endTag.Length);
                        else
                            text = text.Substring(0, lastIndex - 1) + endTag;
                    }
                    if (text.StartsWith("<i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal) && text.Contains("</i>" + Environment.NewLine + "<i>"))
                    {
                        text = text.Replace("</i>" + Environment.NewLine + "<i>", Environment.NewLine);
                    }
                }

                if (italicBeginTagCount == 1 && italicEndTagCount == 0)
                {
                    int lastIndexWithNewLine = text.LastIndexOf(Environment.NewLine + beginTag, StringComparison.Ordinal) + Environment.NewLine.Length;
                    int lastIndex = text.LastIndexOf(beginTag, StringComparison.Ordinal);

                    if (text.StartsWith(beginTag, StringComparison.Ordinal))
                        text += endTag;
                    else if (noOfLines == 2 && lastIndex == lastIndexWithNewLine)
                        text += endTag;
                    else
                        text = text.Replace(beginTag, string.Empty);
                }

                if (italicBeginTagCount == 0 && italicEndTagCount == 1)
                {
                    var cleanText = HtmlUtils.RemoveOpenCloseTags(text, HtmlUtils.TagItalic, HtmlUtils.TagBold, HtmlUtils.TagUnderline, HtmlUtils.TagCyrillicI);
                    bool isFixed = false;

                    // Foo.</i>
                    if (text.EndsWith(endTag, StringComparison.Ordinal) && !cleanText.StartsWith('-') && !cleanText.Contains(Environment.NewLine + "-"))
                    {
                        text = beginTag + text;
                        isFixed = true;
                    }

                    // - Foo</i> | - Foo.
                    // - Bar.    | - Foo.</i>
                    if (!isFixed && CountTagInText(cleanText, Environment.NewLine) == 1)
                    {
                        int newLineIndex = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                        if (newLineIndex > 0)
                        {
                            var firstLine = text.Substring(0, newLineIndex).Trim();
                            var secondLine = text.Substring(newLineIndex + 2).Trim();
                            if (firstLine.EndsWith(endTag, StringComparison.Ordinal))
                            {
                                firstLine = beginTag + firstLine;
                                isFixed = true;
                            }
                            if (secondLine.EndsWith(endTag, StringComparison.Ordinal))
                            {
                                secondLine = beginTag + secondLine;
                                isFixed = true;
                            }
                            text = firstLine + Environment.NewLine + secondLine;
                        }
                    }
                    if (!isFixed)
                        text = text.Replace(endTag, string.Empty);
                }

                // - foo.</i>
                // - bar.</i>
                if (italicBeginTagCount == 0 && italicEndTagCount == 2 && text.Contains(endTag + Environment.NewLine) && text.EndsWith(endTag, StringComparison.Ordinal))
                {
                    text = text.Replace(endTag, string.Empty);
                    text = beginTag + text + endTag;
                }

                if (italicBeginTagCount == 0 && italicEndTagCount == 2 && text.StartsWith("</i>", StringComparison.Ordinal) && text.EndsWith("</i>", StringComparison.Ordinal))
                {
                    int firstIndex = text.IndexOf(endTag, StringComparison.Ordinal);
                    text = text.Remove(firstIndex, endTag.Length).Insert(firstIndex, "<i>");
                }

                // <i>Foo</i>
                // <i>Bar</i>
                if (italicBeginTagCount == 2 && italicEndTagCount == 2 && CountTagInText(text, Environment.NewLine) == 1)
                {
                    int index = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                    if (index > 0 && text.Length > index + (beginTag.Length + endTag.Length))
                    {
                        var firstLine = text.Substring(0, index).Trim();
                        var secondLine = text.Substring(index + 2).Trim();

                        if (firstLine.Length > 10 && firstLine.StartsWith("- <i>") && firstLine.EndsWith(endTag))
                        {
                            text = "<i>- " + firstLine.Remove(0, 5) + Environment.NewLine + secondLine;
                            text = text.Replace("<i>-  ", "<i>- ");
                            index = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                            firstLine = text.Substring(0, index).Trim();
                            secondLine = text.Substring(index + 2).Trim();
                        }
                        if (secondLine.Length > 10 && secondLine.StartsWith("- <i>") && secondLine.EndsWith(endTag))
                        {
                            text = firstLine + Environment.NewLine + "<i>- " + secondLine.Remove(0, 5);
                            text = text.Replace("<i>-  ", "<i>- ");
                            index = text.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                            firstLine = text.Substring(0, index).Trim();
                            secondLine = text.Substring(index + 2).Trim();
                        }

                        if (StartsAndEndsWithTag(firstLine, beginTag, endTag) && StartsAndEndsWithTag(secondLine, beginTag, endTag))
                        {
                            text = text.Replace(beginTag, String.Empty).Replace(endTag, String.Empty);
                            text = beginTag + text + endTag;
                        }
                    }
                }

                text = text.Replace("<i></i>", string.Empty);
                text = text.Replace("<i> </i>", string.Empty);
                text = text.Replace("<i>  </i>", string.Empty);
            }
            return text;
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
                    p.StartTime.TotalMilliseconds < paragraph.StartTime.TotalMilliseconds + 1000)
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
            text = text.Replace("+", " ");
            return Uri.UnescapeDataString(text);
        }

        public static void CheckAutoWrap(TextBox textBox, KeyEventArgs e, int numberOfNewLines)
        {
            int length = Utilities.RemoveHtmlTags(textBox.Text).Length;
            if (e.Modifiers == Keys.None && e.KeyCode != Keys.Enter && numberOfNewLines < 1 && length > Configuration.Settings.General.SubtitleLineMaximumLength)
            {
                if (Configuration.Settings.General.AutoWrapLineWhileTyping) // only if auto-break-setting is true
                {
                    string newText;
                    if (length > Configuration.Settings.General.SubtitleLineMaximumLength + 30)
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
            Keys resultKeys = Keys.None;
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
            string[] lines = text.Split(Utilities.NewLineChars, StringSplitOptions.RemoveEmptyEntries);
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
                    numbers = string.Empty;
                }

                sb.AppendLine(s);
            }
            return sb.ToString().Trim();
        }

        public static string ToSuperscript(string text)
        {
            var sb = new StringBuilder();
            var superscript = new List<string>{
                                                "⁰",
                                                "¹",
                                                "²",
                                                "³",
                                                "⁴",
                                                "⁵",
                                                "⁶",
                                                "⁷",
                                                "⁸",
                                                "⁹",
                                                "⁺",
                                                "⁻",
                                                "⁼",
                                                "⁽",
                                                "⁾",
                                                "ᵃ",
                                                "ᵇ",
                                                "ᶜ",
                                                "ᵈ",
                                                "ᵉ",
                                                "ᶠ",
                                                "ᵍ",
                                                "ʰ",
                                                "ⁱ",
                                                "ʲ",
                                                "ᵏ",
                                                "ˡ",
                                                "ᵐ",
                                                "ⁿ",
                                                "ᵒ",
                                                "ᵖ",
                                                "ʳ",
                                                "ˢ",
                                                "ᵗ",
                                                "ᵘ",
                                                "ᵛ",
                                                "ʷ",
                                                "ˣ",
                                                "ʸ",
                                                "ᶻ",
                                                "ᴬ",
                                                "ᴮ",
                                                "ᴰ",
                                                "ᴱ",
                                                "ᴳ",
                                                "ᴴ",
                                                "ᴵ",
                                                "ᴶ",
                                                "ᴷ",
                                                "ᴸ",
                                                "ᴹ",
                                                "ᴺ",
                                                "ᴼ",
                                                "ᴾ",
                                                "ᴿ",
                                                "ᵀ",
                                                "ᵁ",
                                                "ᵂ",
                                            };
            var normal = new List<string>{
                                                "0", // "⁰"
                                                "1", // "¹"
                                                "2", // "²"
                                                "3", // "³"
                                                "4", // "⁴"
                                                "5", // "⁵"
                                                "6", // "⁶"
                                                "7", // "⁷"
                                                "8", // "⁸"
                                                "9", // "⁹"
                                                "+", // "⁺"
                                                "-", // "⁻"
                                                "=", // "⁼"
                                                "(", // "⁽"
                                                ")", // "⁾"
                                                "a", // "ᵃ"
                                                "b", // "ᵇ"
                                                "c", // "ᶜ"
                                                "d", // "ᵈ"
                                                "e", // "ᵉ"
                                                "f", // "ᶠ"
                                                "g", // "ᵍ"
                                                "h", // "ʰ"
                                                "i", // "ⁱ"
                                                "j", // "ʲ"
                                                "k", // "ᵏ"
                                                "l", // "ˡ"
                                                "m", // "ᵐ"
                                                "n", // "ⁿ"
                                                "o", // "ᵒ"
                                                "p", // "ᵖ"
                                                "r", // "ʳ"
                                                "s", // "ˢ"
                                                "t", // "ᵗ"
                                                "u", // "ᵘ"
                                                "v", // "ᵛ"
                                                "w", // "ʷ"
                                                "x", // "ˣ"
                                                "y", // "ʸ"
                                                "z", // "ᶻ"
                                                "A", // "ᴬ"
                                                "B", // "ᴮ"
                                                "D", // "ᴰ"
                                                "E", // "ᴱ"
                                                "G", // "ᴳ"
                                                "H", // "ᴴ"
                                                "I", // "ᴵ"
                                                "J", // "ᴶ"
                                                "K", // "ᴷ"
                                                "L", // "ᴸ"
                                                "M", // "ᴹ"
                                                "N", // "ᴺ"
                                                "O", // "ᴼ"
                                                "P", // "ᴾ"
                                                "R", // "ᴿ"
                                                "T", // "ᵀ"
                                                "U", // "ᵁ"
                                                "W", // "ᵂ"
                                            };
            for (int i = 0; i < text.Length; i++)
            {
                string s = text.Substring(i, 1);
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
            var subcript = new List<string>{
                                                "₀",
                                                "₁",
                                                "₂",
                                                "₃",
                                                "₄",
                                                "₅",
                                                "₆",
                                                "₇",
                                                "₈",
                                                "₉",
                                                "₊",
                                                "₋",
                                                "₌",
                                                "₍",
                                                "₎",
                                                "ₐ",
                                                "ₑ",
                                                "ᵢ",
                                                "ₒ",
                                                "ᵣ",
                                                "ᵤ",
                                                "ᵥ",
                                                "ₓ",
                                            };
            var normal = new List<string>
                             {
                                                "0",  // "₀"
                                                "1",  // "₁"
                                                "2",  // "₂"
                                                "3",  // "₃"
                                                "4",  // "₄"
                                                "5",  // "₅"
                                                "6",  // "₆"
                                                "7",  // "₇"
                                                "8",  // "₈"
                                                "9",  // "₉"
                                                "+",  // "₊"
                                                "-",  // "₋"
                                                "=",  // "₌"
                                                "(",  // "₍"
                                                ")",  // "₎"
                                                "a",  // "ₐ"
                                                "e",  // "ₑ"
                                                "i",  // "ᵢ"
                                                "o",  // "ₒ"
                                                "r",  // "ᵣ"
                                                "u",  // "ᵤ"
                                                "v",  // "ᵥ"
                                                "x",  // "ₓ"

                             };
            for (int i = 0; i < text.Length; i++)
            {
                string s = text.Substring(i, 1);
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
                            else
                            {
                                return ColorTranslator.FromHtml(s);
                            }
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
                        word = new StringBuilder();
                        i += Environment.NewLine.Length;
                    }
                    else if (s.Substring(i).StartsWith(Environment.NewLine, StringComparison.Ordinal))
                    {
                        if (word.Length > 0)
                            list.Add(word.ToString());
                        word = new StringBuilder();
                        list.Add(Environment.NewLine);
                        i += Environment.NewLine.Length;
                    }
                    else if (s[i] == ' ')
                    {
                        if (word.Length > 0)
                            list.Add(word.ToString());
                        word = new StringBuilder();
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
                        word = new StringBuilder();
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
            int t = Math.Max(parts1.Length, parts2.Length);
            total += t;
            int c = GetChangesAdvanced(parts1, parts2);
            change += c;
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

        internal static string RemoveNonNumbers(string p)
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
            const string zeroWhiteSpace = "\u200B";
            const string zeroWidthNoBreakSpace = "\uFEFF";
            const string noBreakSpace = "\u00A0";
            const string char160 = " "; // Convert.ToChar(160).ToString()

            text = text.Trim();

            text = text.Replace(zeroWhiteSpace, string.Empty);
            text = text.Replace(zeroWidthNoBreakSpace, string.Empty);
            text = text.Replace(noBreakSpace, string.Empty);
            text = text.Replace(char160, " ");

            text = text.Replace("", string.Empty); // some kind of hidden space!!!
            while (text.Contains("  "))
                text = text.Replace("  ", " ");

            if (text.Contains(" " + Environment.NewLine))
                text = text.Replace(" " + Environment.NewLine, Environment.NewLine);

            if (text.EndsWith(' '))
                text = text.TrimEnd(' ');

            text = text.Replace(". . ..", "...");
            text = text.Replace(". ...", "...");
            text = text.Replace(". .. .", "...");
            text = text.Replace(". . .", "...");
            text = text.Replace(". ..", "...");
            text = text.Replace(".. .", "...");
            text = text.Replace("....", "...");
            text = text.Replace("....", "...");
            text = text.Replace("....", "...");
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
                text = text.Substring(0, text.Length - " .".Length) + ".";

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

            if (text.Contains("! </i>" + Environment.NewLine))
                text = text.Replace("! </i>" + Environment.NewLine, "!</i>" + Environment.NewLine);

            if (text.Contains("? </i>" + Environment.NewLine))
                text = text.Replace("? </i>" + Environment.NewLine, "?</i>" + Environment.NewLine);

            if (text.EndsWith(" </i>", StringComparison.Ordinal))
                text = text.Substring(0, text.Length - " </i>".Length) + "</i>";

            if (text.Contains(" </i>" + Environment.NewLine))
                text = text.Replace(" </i>" + Environment.NewLine, "</i>" + Environment.NewLine);

            if (text.EndsWith(" </I>", StringComparison.Ordinal))
                text = text.Substring(0, text.Length - " </I>".Length) + "</I>";

            if (text.Contains(" </I>" + Environment.NewLine))
                text = text.Replace(" </I>" + Environment.NewLine, "</I>" + Environment.NewLine);

            if (text.StartsWith("<i> ", StringComparison.Ordinal))
                text = "<i>" + text.Substring("<i> ".Length);

            if (text.Contains(Environment.NewLine + "<i> "))

                text = text.Replace(Environment.NewLine + "<i> ", Environment.NewLine + "<i>");

            text = text.Trim();
            text = text.Replace(Environment.NewLine + " ", Environment.NewLine);
            if (text.StartsWith("<I> ", StringComparison.Ordinal))
                text = "<I>" + text.Substring("<I> ".Length);

            if (text.Contains(Environment.NewLine + "<I> "))
                text = text.Replace(Environment.NewLine + "<I> ", Environment.NewLine + "<I>");

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
                        while (k >= 0 && Utilities.AllLettersAndNumbers.Contains(text[k]))
                        {
                            before = text[k] + before;
                            k--;
                        }
                        string after = string.Empty;
                        k = idx + 2;
                        while (k < text.Length && Utilities.AllLetters.Contains(text[k]))
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
                        idx = -1;
                }
            }
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

        /// <summary>
        /// Retrieves the specified registry subkey value.
        /// </summary>
        /// <param name="keyName">The path of the subkey to open.</param>
        /// <param name="valueName">The name of the value to retrieve.</param>
        /// <returns>The value of the subkey requested, or <b>null</b> if the operation failed.</returns>
        public static string GetRegistryValue(string keyName, string valueName)
        {
            return Configuration.GetRegistryValue(keyName, valueName);
        }

    }
}