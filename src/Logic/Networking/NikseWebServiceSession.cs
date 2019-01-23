using Nikse.SubtitleEdit.Core;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Networking
{
    public class NikseWebServiceSession : IDisposable
    {
        public class ChatEntry
        {
            public SeNetworkService.SeUser User { get; set; }
            public string Message { get; set; }
        }

        public event EventHandler OnUpdateTimerTick;
        public event EventHandler OnUpdateUserLogEntries;

        private System.Windows.Forms.Timer _timerWebService;
        public List<UpdateLogEntry> UpdateLog { get; } = new List<UpdateLogEntry>();
        public List<ChatEntry> ChatLog { get; } = new List<ChatEntry>();
        private SeNetworkService.SeService _seWs;
        private DateTime _seWsLastUpdate = DateTime.Now.AddYears(-1);
        public SeNetworkService.SeUser CurrentUser { get; set; }
        public Subtitle LastSubtitle { get; set; }
        public Subtitle Subtitle { get; private set; }
        public Subtitle OriginalSubtitle { get; private set; }
        public string SessionId { get; private set; }
        private string _userName;
        private string _fileName;
        public List<SeNetworkService.SeUser> Users { get; private set; }
        public StringBuilder Log { get; }

        public string WebServiceUrl => _seWs.Url;

        public NikseWebServiceSession(Subtitle subtitle, Subtitle originalSubtitle, EventHandler onUpdateTimerTick, EventHandler onUpdateUserLogEntries)
        {
            Subtitle = subtitle;
            OriginalSubtitle = originalSubtitle;
            _timerWebService = new System.Windows.Forms.Timer();
            if (Configuration.Settings.NetworkSettings.PollIntervalSeconds < 1)
            {
                Configuration.Settings.NetworkSettings.PollIntervalSeconds = 1;
            }

            _timerWebService.Interval = Configuration.Settings.NetworkSettings.PollIntervalSeconds * 1000;
            _timerWebService.Tick += TimerWebServiceTick;
            Log = new StringBuilder();
            OnUpdateTimerTick = onUpdateTimerTick;
            OnUpdateUserLogEntries = onUpdateUserLogEntries;
        }

        public void StartServer(string webServiceUrl, string sessionKey, string userName, string fileName, out string message)
        {
            SessionId = sessionKey;
            _userName = userName;
            _fileName = fileName;
            var list = new List<SeNetworkService.SeSequence>();
            foreach (Paragraph p in Subtitle.Paragraphs)
            {
                list.Add(new SeNetworkService.SeSequence
                {
                    StartMilliseconds = (int)p.StartTime.TotalMilliseconds,
                    EndMilliseconds = (int)p.EndTime.TotalMilliseconds,
                    Text = WebUtility.HtmlEncode(p.Text.Replace(Environment.NewLine, "<br />"))
                });
            }

            var originalSubtitle = new List<SeNetworkService.SeSequence>();
            if (OriginalSubtitle != null)
            {
                foreach (Paragraph p in OriginalSubtitle.Paragraphs)
                {
                    originalSubtitle.Add(new SeNetworkService.SeSequence
                    {
                        StartMilliseconds = (int)p.StartTime.TotalMilliseconds,
                        EndMilliseconds = (int)p.EndTime.TotalMilliseconds,
                        Text = WebUtility.HtmlEncode(p.Text.Replace(Environment.NewLine, "<br />"))
                    });
                }
            }

            _seWs = new SeNetworkService.SeService
            {
                Url = webServiceUrl,
                Proxy = Utilities.GetProxy()
            };
            var user = _seWs.Start(sessionKey, userName, list.ToArray(), originalSubtitle.ToArray(), fileName, out message);
            CurrentUser = user;
            Users = new List<SeNetworkService.SeUser> { user };
            if (message == "OK")
            {
                _timerWebService.Start();
            }
        }

        public bool Join(string webServiceUrl, string userName, string sessionKey, out string message)
        {
            SessionId = sessionKey;
            _seWs = new SeNetworkService.SeService
            {
                Url = webServiceUrl,
                Proxy = Utilities.GetProxy()
            };
            Users = new List<SeNetworkService.SeUser>();
            var users = _seWs.Join(sessionKey, userName, out message);
            if (message != "OK")
            {
                return false;
            }

            string tempFileName;
            Subtitle = new Subtitle();
            foreach (var sequence in _seWs.GetSubtitle(sessionKey, out tempFileName, out _))
            {
                Subtitle.Paragraphs.Add(new Paragraph(WebUtility.HtmlDecode(sequence.Text).Replace("<br />", Environment.NewLine), sequence.StartMilliseconds, sequence.EndMilliseconds));
            }

            _fileName = tempFileName;

            OriginalSubtitle = new Subtitle();
            var sequences = _seWs.GetOriginalSubtitle(sessionKey);
            if (sequences != null)
            {
                foreach (var sequence in sequences)
                {
                    OriginalSubtitle.Paragraphs.Add(new Paragraph(WebUtility.HtmlDecode(sequence.Text).Replace("<br />", Environment.NewLine), sequence.StartMilliseconds, sequence.EndMilliseconds));
                }
            }

            SessionId = sessionKey;
            CurrentUser = users[users.Length - 1]; // me
            foreach (var user in users)
            {
                Users.Add(user);
            }

            ReloadFromWs();
            _timerWebService.Start();
            return true;
        }

        private void TimerWebServiceTick(object sender, EventArgs e)
        {
            OnUpdateTimerTick?.Invoke(sender, e);
        }

        public void TimerStop()
        {
            _timerWebService.Stop();
        }

        public void TimerStart()
        {
            _timerWebService.Start();
        }

        public List<SeNetworkService.SeUpdate> GetUpdates(out string message, out int numberOfLines)
        {
            List<SeNetworkService.SeUpdate> list = new List<SeNetworkService.SeUpdate>();
            var updates = _seWs.GetUpdates(SessionId, CurrentUser.UserName, _seWsLastUpdate, out message, out var newUpdateTime, out numberOfLines);
            if (updates != null)
            {
                foreach (var update in updates)
                {
                    list.Add(update);
                }
            }
            _seWsLastUpdate = newUpdateTime;
            return list;
        }

        public Subtitle ReloadSubtitle()
        {
            Subtitle.Paragraphs.Clear();
            var sequences = _seWs.GetSubtitle(SessionId, out var tempFileName, out var updateTime);
            _fileName = tempFileName;
            _seWsLastUpdate = updateTime;
            if (sequences != null)
            {
                foreach (var sequence in sequences)
                {
                    Subtitle.Paragraphs.Add(new Paragraph(WebUtility.HtmlDecode(sequence.Text).Replace("<br />", Environment.NewLine), sequence.StartMilliseconds, sequence.EndMilliseconds));
                }
            }
            return Subtitle;
        }

        private void ReloadFromWs()
        {
            if (_seWs != null)
            {
                Subtitle = new Subtitle();
                var sequences = _seWs.GetSubtitle(SessionId, out _fileName, out _seWsLastUpdate);
                foreach (var sequence in sequences)
                {
                    var p = new Paragraph(WebUtility.HtmlDecode(sequence.Text).Replace("<br />", Environment.NewLine), sequence.StartMilliseconds, sequence.EndMilliseconds);
                    Subtitle.Paragraphs.Add(p);
                }
                Subtitle.Renumber();
                LastSubtitle = new Subtitle(Subtitle);
            }
        }

        public void AppendToLog(string text)
        {
            string timestamp = DateTime.Now.ToLongTimeString();
            Log.AppendLine(timestamp + ": " + UiUtil.GetListViewTextFromString(text.TrimEnd()));
        }

        public string GetLog()
        {
            return Log.ToString();
        }

        public void SendChatMessage(string message)
        {
            _seWs.SendMessage(SessionId, WebUtility.HtmlEncode(message.Replace(Environment.NewLine, "<br />")), CurrentUser);
        }

        internal void UpdateLine(int index, Paragraph paragraph)
        {
            _seWs.UpdateLine(SessionId, index, new SeNetworkService.SeSequence
            {
                StartMilliseconds = (int)paragraph.StartTime.TotalMilliseconds,
                EndMilliseconds = (int)paragraph.EndTime.TotalMilliseconds,
                Text = WebUtility.HtmlEncode(paragraph.Text.Replace(Environment.NewLine, "<br />"))
            }, CurrentUser);
            AddToWsUserLog(CurrentUser, index, "UPD", true);
        }

        public void CheckForAndSubmitUpdates()
        {
            // check for changes in text/time codes (not insert/delete)
            if (LastSubtitle != null)
            {
                for (int i = 0; i < Subtitle.Paragraphs.Count; i++)
                {
                    var last = LastSubtitle.GetParagraphOrDefault(i);
                    var current = Subtitle.GetParagraphOrDefault(i);
                    if (last != null && current != null)
                    {
                        if (Math.Abs(last.StartTime.TotalMilliseconds - current.StartTime.TotalMilliseconds) > 0.01 ||
                            Math.Abs(last.EndTime.TotalMilliseconds - current.EndTime.TotalMilliseconds) > 0.01 ||
                            last.Text != current.Text)
                        {
                            UpdateLine(i, current);
                        }
                    }
                }
            }
        }

        public void AddToWsUserLog(SeNetworkService.SeUser user, int pos, string action, bool updateUi)
        {
            for (int i = 0; i < UpdateLog.Count; i++)
            {
                if (UpdateLog[i].Index == pos)
                {
                    UpdateLog.RemoveAt(i);
                    break;
                }
            }

            UpdateLog.Add(new UpdateLogEntry(0, user.UserName, pos, action));
            if (updateUi)
            {
                OnUpdateUserLogEntries?.Invoke(null, null);
            }
        }

        internal void Leave()
        {
            try
            {
                _seWs.Leave(SessionId, CurrentUser.UserName);
            }
            catch
            {
                // ignored
            }
        }

        internal void DeleteLines(List<int> indices)
        {
            _seWs.DeleteLines(SessionId, indices.ToArray(), CurrentUser);
            foreach (int index in indices)
            {
                AdjustUpdateLogToDelete(index);
                AppendToLog(string.Format(Configuration.Settings.Language.Main.NetworkDelete, CurrentUser.UserName, CurrentUser.Ip, index));
            }
        }

        internal void InsertLine(int index, Paragraph newParagraph)
        {
            _seWs.InsertLine(SessionId, index, (int)newParagraph.StartTime.TotalMilliseconds, (int)newParagraph.EndTime.TotalMilliseconds, newParagraph.Text, CurrentUser);
            AppendToLog(string.Format(Configuration.Settings.Language.Main.NetworkInsert, CurrentUser.UserName, CurrentUser.Ip, index, UiUtil.GetListViewTextFromString(newParagraph.Text)));
        }

        internal void AdjustUpdateLogToInsert(int index)
        {
            foreach (var logEntry in UpdateLog)
            {
                if (logEntry.Index >= index)
                {
                    logEntry.Index++;
                }
            }
        }

        internal void AdjustUpdateLogToDelete(int index)
        {
            UpdateLogEntry removeThis = null;
            foreach (var logEntry in UpdateLog)
            {
                if (logEntry.Index == index)
                {
                    removeThis = logEntry;
                }
                else if (logEntry.Index > index)
                {
                    logEntry.Index--;
                }
            }
            if (removeThis != null)
            {
                UpdateLog.Remove(removeThis);
            }
        }

        internal string Restart()
        {
            string message = string.Empty;
            int retries = 0;
            const int maxRetries = 10;
            while (retries < maxRetries)
            {
                try
                {
                    System.Threading.Thread.Sleep(200);
                    StartServer(_seWs.Url, SessionId, _userName, _fileName, out message);
                    retries = maxRetries;
                }
                catch
                {
                    System.Threading.Thread.Sleep(200);
                    retries++;
                }
            }

            if (message == "Session is already running")
            {
                return ReJoin();
            }
            return message;
        }

        internal string ReJoin()
        {
            string message = string.Empty;
            int retries = 0;
            const int maxRetries = 10;
            while (retries < maxRetries)
            {
                try
                {
                    System.Threading.Thread.Sleep(200);
                    if (Join(_seWs.Url, _userName, SessionId, out message))
                    {
                        message = "Reload";
                    }

                    retries = maxRetries;
                }
                catch
                {
                    System.Threading.Thread.Sleep(200);
                    retries++;
                }
            }

            return message;
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
                if (_timerWebService != null)
                {
                    _timerWebService.Dispose();
                    _timerWebService = null;
                }
                if (_seWs != null)
                {
                    _seWs.Dispose();
                    _seWs = null;
                }
            }
        }

    }
}
