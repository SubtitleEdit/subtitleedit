using System;
using System.Net;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.Forms
{
    public class CheckForUpdatesHelper
    {
        private const string ReleasesUrl = "https://api.github.com/repos/SubtitleEdit/subtitleedit/releases";
        private const string ChangeLogUrl = "https://raw.githubusercontent.com/SubtitleEdit/subtitleedit/master/src/Changelog.txt";

        private string _jsonReleases;
        private string _changeLog;
        private int _successCount;

        public string Error { get; set; }
        public bool Done 
        { 
            get 
            {
                return _successCount == 2;
            }
            private set
            {
                Done = value;
            }
        }
        public string LatestVersionNumber { get; set; }
        public string LatestChangeLog { get; set; }

        private void StartDownloadString(string url, string contentType, AsyncCallback callback)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.UserAgent = "SubtitleEdit";
                request.ContentType = contentType;
                request.Timeout = Timeout.Infinite;
                request.Method = "GET";
                request.AllowAutoRedirect = true;
                request.Accept = contentType;
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

        void FinishWebRequestReleases(IAsyncResult result)
        {
            try
            {
                _jsonReleases = GetStringFromResponse(result);
            }
            catch (Exception exception)
            {
                if (Error == null)
                {
                    Error = exception.Message;
                }
            }
        }

        void FinishWebRequestChangeLog(IAsyncResult result)
        {
            try
            {
                _changeLog = GetStringFromResponse(result);
                LatestChangeLog = _changeLog;
            }
            catch (Exception exception)
            {
                if (Error == null)
                {
                    Error = exception.Message;
                }
            }
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
                _successCount++;
            return System.Text.Encoding.UTF8.GetString(buffer, 0, index);
        }

        public CheckForUpdatesHelper()
        {
            Error = null;
            _successCount = 0;
        }

        public void CheckForUpdates()
        {
            // load github release json 
            StartDownloadString(ReleasesUrl, "application/json", new AsyncCallback(FinishWebRequestReleases));

            // load change log
            StartDownloadString(ChangeLogUrl, null, new AsyncCallback(FinishWebRequestChangeLog));
        }

    }
}
