using Nikse.SubtitleEdit.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private SeNetworkService _seWs;
        private DateTime _seWsLastUpdate = DateTime.Now.AddYears(-1);
        public SeNetworkService.SeUser CurrentUser { get; set; }
        public Subtitle LastSubtitle { get; set; }
        public Subtitle Subtitle { get; private set; }
        public Subtitle OriginalSubtitle { get; private set; }
        public string SessionId { get; private set; }
        public string BaseUrl => _seWs != null ? _seWs.BaseUrl : string.Empty;
        private string _userName;
        private string _fileName;
        public List<SeNetworkService.SeUser> Users { get; private set; }
        public StringBuilder Log { get; }

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

        public void StartServer(string baseUrl, string sessionKey, string userName, string fileName, out string message)
        {
            SessionId = sessionKey;
            _userName = userName;
            _fileName = fileName;
            var list = new List<SeNetworkService.SeSequence>();
            foreach (var p in Subtitle.Paragraphs)
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
                foreach (var p in OriginalSubtitle.Paragraphs)
                {
                    originalSubtitle.Add(new SeNetworkService.SeSequence
                    {
                        StartMilliseconds = (int)p.StartTime.TotalMilliseconds,
                        EndMilliseconds = (int)p.EndTime.TotalMilliseconds,
                        Text = WebUtility.HtmlEncode(p.Text.Replace(Environment.NewLine, "<br />"))
                    });
                }
            }

            _seWs = new SeNetworkService(baseUrl);
            var request = new SeNetworkService.StartRequest()
            {
                SessionId = sessionKey,
                FileName = fileName,
                OriginalSubtitle = originalSubtitle,
                Subtitle = list,
                UserName = userName
            };

            var response = _seWs.Start(request);
            CurrentUser = response.User;
            message = response.Message;
            Users = new List<SeNetworkService.SeUser> { response.User };
            if (response.Message == "OK")
            {
                _timerWebService.Start();
            }
        }

        public bool Join(string webServiceUrl, string userName, string sessionKey, out string message)
        {
            SessionId = sessionKey;
            _seWs = new SeNetworkService(webServiceUrl);
            Users = new List<SeNetworkService.SeUser>();
            var joinRequest = new SeNetworkService.JoinRequest()
            {
                SessionId = sessionKey,
                UserName = userName
            };
            var joinResponse = _seWs.Join(joinRequest);
            message = joinResponse.Message;
            if (joinResponse.Message != "OK")
            {
                return false;
            }

            Subtitle = new Subtitle();
            var getSubtitleRequest = new SeNetworkService.GetSubtitleRequest
            {
                SessionId = sessionKey
            };
            var getSubtitleResponse = _seWs.GetSubtitle(getSubtitleRequest);
            foreach (var sequence in getSubtitleResponse.Subtitle)
            {
                Subtitle.Paragraphs.Add(new Paragraph(WebUtility.HtmlDecode(sequence.Text).Replace("<br />", Environment.NewLine), sequence.StartMilliseconds, sequence.EndMilliseconds));
            }

            _fileName = getSubtitleResponse.FileName;

            OriginalSubtitle = new Subtitle();
            var getOriginalSubtitleRequest = new SeNetworkService.GetOriginalSubtitleRequest
            {
                SessionId = sessionKey
            };
            var getOriginalSubtitleResponse = _seWs.GetOriginalSubtitle(getOriginalSubtitleRequest);
            if (getOriginalSubtitleResponse.Subtitle != null)
            {
                foreach (var sequence in getOriginalSubtitleResponse.Subtitle)
                {
                    OriginalSubtitle.Paragraphs.Add(new Paragraph(WebUtility.HtmlDecode(sequence.Text).Replace("<br />", Environment.NewLine), sequence.StartMilliseconds, sequence.EndMilliseconds));
                }
            }

            SessionId = sessionKey;
            CurrentUser = joinResponse.Users.Last();
            foreach (var user in joinResponse.Users)
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
            var list = new List<SeNetworkService.SeUpdate>();
            var request = new SeNetworkService.GetUpdatesRequest
            {
                SessionId = SessionId,
                UserName = CurrentUser.UserName,
                LastUpdateTime = _seWsLastUpdate
            };
            var response = _seWs.GetUpdates(request);
            if (response.Updates != null)
            {
                foreach (var update in response.Updates)
                {
                    list.Add(update);
                }
            }
            _seWsLastUpdate = response.NewUpdateTime;
            message = response.Message;
            numberOfLines = response.NumberOfLines;
            return list;
        }

        public Subtitle ReloadSubtitle()
        {
            Subtitle.Paragraphs.Clear();
            var request = new SeNetworkService.GetSubtitleRequest
            {
                SessionId = SessionId,
            };
            var response = _seWs.GetSubtitle(request);
            _fileName = response.FileName;
            _seWsLastUpdate = response.UpdateTime;
            if (response.Subtitle != null)
            {
                foreach (var sequence in response.Subtitle)
                {
                    Subtitle.Paragraphs.Add(new Paragraph(WebUtility.HtmlDecode(sequence.Text).Replace("<br />", Environment.NewLine), sequence.StartMilliseconds, sequence.EndMilliseconds));
                }
            }
            return Subtitle;
        }

        private void ReloadFromWs()
        {
            if (_seWs == null)
            {
                return;
            }

            Subtitle = new Subtitle();
            var request = new SeNetworkService.GetSubtitleRequest
            {
                SessionId = SessionId,
            };
            var response = _seWs.GetSubtitle(request);
            _fileName = response.FileName;
            _seWsLastUpdate = response.UpdateTime;
            foreach (var sequence in response.Subtitle)
            {
                var p = new Paragraph(WebUtility.HtmlDecode(sequence.Text).Replace("<br />", Environment.NewLine), sequence.StartMilliseconds, sequence.EndMilliseconds);
                Subtitle.Paragraphs.Add(p);
            }
            Subtitle.Renumber();
            LastSubtitle = new Subtitle(Subtitle);
        }

        public void AppendToLog(string text)
        {
            var timestamp = DateTime.Now.ToLongTimeString();
            Log.AppendLine(timestamp + ": " + UiUtil.GetListViewTextFromString(text.TrimEnd()));
        }

        public string GetLog()
        {
            return Log.ToString();
        }

        public void SendChatMessage(string message)
        {
            var request = new SeNetworkService.SendMessageRequest
            {
                SessionId = SessionId,
                User = CurrentUser,
                Text = WebUtility.HtmlEncode(message.Replace(Environment.NewLine, "<br />"))
            };
            _seWs.SendMessage(request);
        }

        internal void UpdateLine(int index, Paragraph paragraph)
        {
            var request = new SeNetworkService.UpdateLineRequest
            {
                SessionId = SessionId,
                User = CurrentUser,
                Index = index,
                Sequence = new SeNetworkService.SeSequence
                {
                    StartMilliseconds = (int)paragraph.StartTime.TotalMilliseconds,
                    EndMilliseconds = (int)paragraph.EndTime.TotalMilliseconds,
                    Text = WebUtility.HtmlEncode(paragraph.Text.Replace(Environment.NewLine, "<br />"))
                }
            };

            _seWs.UpdateLine(request);
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
                var request = new SeNetworkService.LeaveRequest { SessionId = SessionId, UserName = CurrentUser.UserName };
                _seWs.Leave(request);
            }
            catch
            {
                // ignored
            }
        }

        internal void DeleteLines(List<int> indices)
        {
            var request = new SeNetworkService.DeleteLinesRequest
            {
                SessionId = SessionId,
                User = CurrentUser,
                Indices = indices
            };
            _seWs.DeleteLines(request);
            foreach (int index in indices)
            {
                AdjustUpdateLogToDelete(index);
                AppendToLog(string.Format(LanguageSettings.Current.Main.NetworkDelete, CurrentUser.UserName, CurrentUser.Ip, index));
            }
        }

        internal void InsertLine(int index, Paragraph newParagraph)
        {
            var request = new SeNetworkService.InsertLineRequest
            {
                SessionId = SessionId,
                User = CurrentUser,
                Index = index,
                Text = newParagraph.Text,
                StartMilliseconds = (int)newParagraph.StartTime.TotalMilliseconds,
                EndMilliseconds = (int)newParagraph.EndTime.TotalMilliseconds
            };
            _seWs.InsertLine(request);
            AppendToLog(string.Format(LanguageSettings.Current.Main.NetworkInsert, CurrentUser.UserName, CurrentUser.Ip, index, UiUtil.GetListViewTextFromString(newParagraph.Text)));
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
                    StartServer(_seWs.BaseUrl, SessionId, _userName, _fileName, out message);
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
                    if (Join(_seWs.BaseUrl, _userName, SessionId, out message))
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
                _seWs = null;
            }
        }

    }
}
