using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Nikse.SubtitleEdit.Core.Forms
{
    public class CheckForUpdatesHelper
    {
        private static readonly Regex VersionNumberRegex = new Regex(@"\d\.\d", RegexOptions.Compiled); // 3.4.0 (xth June 2014)

        private const string ChangeLogUrl = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/master/Changelog.txt";

        private string _changeLog;
        private int _successCount;

        public string Error { get; set; }

        public bool Done => _successCount == 1;

        public string LatestVersionNumber { get; set; }
        public string LatestChangeLog { get; set; }

        private void StartDownloadString(string url, string contentType, AsyncCallback callback)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "SubtitleEdit";
                request.ContentType = contentType;
                request.Timeout = Timeout.Infinite;
                request.Method = "GET";
                request.AllowAutoRedirect = true;
                request.Accept = contentType;
                request.Proxy = Utilities.GetProxy();
                request.BeginGetResponse(callback, request);
            }
            catch (Exception exception)
            {
                if (Error == null)
                {
                    Error = exception.Message;
                }
            }
        }

        private void FinishWebRequestChangeLog(IAsyncResult result)
        {
            try
            {
                _changeLog = GetStringFromResponse(result);
                LatestChangeLog = GetLastestChangeLog(_changeLog);
                LatestVersionNumber = GetLastestVersionNumber(LatestChangeLog);
            }
            catch (Exception exception)
            {
                if (Error == null)
                {
                    Error = exception.Message;
                }
            }
        }

        private static string GetLastestVersionNumber(string latestChangeLog)
        {
            foreach (string line in latestChangeLog.Replace(Environment.NewLine, "\n").Split('\n'))
            {
                string s = line.Trim();
                if (!s.Contains("BETA", StringComparison.OrdinalIgnoreCase) && !s.Contains('x') && !s.Contains('*') && s.Contains('(') && s.Contains(')') && VersionNumberRegex.IsMatch(s))
                {
                    int indexOfSpace = s.IndexOf(' ');
                    if (indexOfSpace > 0)
                    {
                        return s.Substring(0, indexOfSpace).Trim();
                    }
                }
            }
            return null;
        }

        private static string GetLastestChangeLog(string changeLog)
        {
            bool releaseOn = false;
            var sb = new StringBuilder();
            foreach (string line in changeLog.Replace(Environment.NewLine, "\n").Split('\n'))
            {
                string s = line.Trim();
                if (s.Length == 0 && releaseOn)
                {
                    return sb.ToString();
                }

                if (!releaseOn)
                {
                    if (!s.Contains('x') && !s.Contains('*') && s.Contains('(') && s.Contains(')') && VersionNumberRegex.IsMatch(s))
                    {
                        releaseOn = true;
                    }
                }

                if (releaseOn)
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }

        private string GetStringFromResponse(IAsyncResult result)
        {
            HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
            System.IO.Stream responseStream = response.GetResponseStream();
            byte[] buffer = new byte[5000000];
            int count = 1;
            int index = 0;
            while (count > 0)
            {
                count = responseStream.Read(buffer, index, 2048);
                index += count;
            }
            if (index > 0)
            {
                _successCount++;
            }

            return Encoding.UTF8.GetString(buffer, 0, index);
        }

        public CheckForUpdatesHelper()
        {
            Error = null;
            _successCount = 0;
        }

        public void CheckForUpdates()
        {
            // load change log
            StartDownloadString(ChangeLogUrl, null, FinishWebRequestChangeLog);
        }

        public bool IsUpdateAvailable()
        {
            if (!string.IsNullOrEmpty(Error))
            {
                return false;
            }
            try
            {
                //string[] currentVersionInfo = "3.3.14".Split('.'); // for testing...
                return Version.Parse(LatestVersionNumber) > Version.Parse(Utilities.AssemblyVersion);
            }
            catch
            {
                return false;
            }
        }

    }
}
