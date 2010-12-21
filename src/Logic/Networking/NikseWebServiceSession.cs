using System;
using System.Collections.Generic;
using System.Text;

namespace Nikse.SubtitleEdit.Logic.Networking
{
    public class NikseWebServiceSession
    {
        public class ChatEntry
        {
            public SeNetworkService.SeUser User { get; set; }
            public string Message { get; set; }
        }

        public event EventHandler OnUpdateTimerTick;
        public event EventHandler OnUpdateUserLogEntries;

        System.Windows.Forms.Timer _timerWebService;
        public List<UpdateLogEntry> UpdateLog = new List<UpdateLogEntry>();
        public List<ChatEntry> ChatLog = new List<ChatEntry>();
        SeNetworkService.SeService _seWs;
        DateTime _seWsLastUpdate = DateTime.Now.AddYears(-1);
        public SeNetworkService.SeUser CurrentUser { get; set; }
        public Subtitle LastSubtitle = null;
        public Subtitle Subtitle;
        public Subtitle OriginalSubtitle;
        public string SessionId;
        public string FileName;
        public List<SeNetworkService.SeUser> Users;
        public StringBuilder _log;

        public string WebServiceUrl
        {
            get
            {
                return _seWs.Url;
            }
        }

        public NikseWebServiceSession(Subtitle subtitle, Subtitle originalSubtitle, EventHandler onUpdateTimerTick, EventHandler onUpdateUserLogEntries)
        {
            Subtitle = subtitle;
            OriginalSubtitle = originalSubtitle;
            _timerWebService = new System.Windows.Forms.Timer();
            _timerWebService.Interval = 5000;
            _timerWebService.Tick += new EventHandler(TimerWebServiceTick);
            _log = new StringBuilder();
            OnUpdateTimerTick = onUpdateTimerTick;
            OnUpdateUserLogEntries = onUpdateUserLogEntries;
        }

        public void StartServer(string webServiceUrl, string sessionKey, string userName, string fileName, out string message)
        {
            SessionId = sessionKey;
            var list = new List<SeNetworkService.SeSequence>();
            foreach (Paragraph p in Subtitle.Paragraphs)
            {
                list.Add(new SeNetworkService.SeSequence()
                {
                    StartMilliseconds = (int)p.StartTime.TotalMilliseconds,
                    EndMilliseconds = (int)p.EndTime.TotalMilliseconds,
                    Text = p.Text.Replace(Environment.NewLine, "<br />")
                });
            }

            var originalSubtitle = new List<SeNetworkService.SeSequence>();
            if (OriginalSubtitle != null)
            {
                foreach (Paragraph p in OriginalSubtitle.Paragraphs)
                {
                    originalSubtitle.Add(new SeNetworkService.SeSequence()
                    {
                        StartMilliseconds = (int)p.StartTime.TotalMilliseconds,
                        EndMilliseconds = (int)p.EndTime.TotalMilliseconds,
                        Text = p.Text.Replace(Environment.NewLine, "<br />")
                    });
                }
            }

            _seWs = new SeNetworkService.SeService();
            _seWs.Url = webServiceUrl;
            _seWs.Proxy = Utilities.GetProxy();
            SeNetworkService.SeUser user = _seWs.Start(sessionKey, userName, list.ToArray(), originalSubtitle.ToArray(), fileName, out message);
            CurrentUser = user;
            Users = new List<SeNetworkService.SeUser>();
            Users.Add(user);
            if (message == "OK")
                _timerWebService.Start();
        }

        public bool Join(string webServiceUrl, string userName, string sessionKey, out string message)
        {
            SessionId = sessionKey;
            _seWs = new SeNetworkService.SeService();
            _seWs.Url = webServiceUrl;
            _seWs.Proxy = Utilities.GetProxy();
            Users = new List<SeNetworkService.SeUser>();
            var users = _seWs.Join(sessionKey, userName, out message);
            if (message != "OK")
                return false;

            string tempFileName;
            DateTime updateTime;
            Subtitle = new Subtitle();
            foreach (var sequence in _seWs.GetSubtitle(sessionKey, out tempFileName, out updateTime))
                Subtitle.Paragraphs.Add(new Paragraph(sequence.Text.Replace("<br />", Environment.NewLine), sequence.StartMilliseconds, sequence.EndMilliseconds));
            FileName = tempFileName;

            OriginalSubtitle = new Subtitle();
            var sequences = _seWs.GetOriginalSubtitle(sessionKey);
            if (sequences != null)
            {
                foreach (var sequence in sequences)
                    OriginalSubtitle.Paragraphs.Add(new Paragraph(sequence.Text.Replace("<br />", Environment.NewLine), sequence.StartMilliseconds, sequence.EndMilliseconds));
            }
        
            SessionId = sessionKey;
            CurrentUser = users[users.Length - 1]; // me
            foreach (var user in users)
                Users.Add(user);
            ReloadFromWs();
            _timerWebService.Start();
            return true;
        }

        void TimerWebServiceTick(object sender, EventArgs e)
        {
            if (OnUpdateTimerTick != null)
                OnUpdateTimerTick.Invoke(sender, e);
        }

        public void TimerStop()
        {
            _timerWebService.Stop();
        }

        public void TimerStart()
        {
            _timerWebService.Start();
        }

        public IEnumerable<SeNetworkService.SeUpdate> GetUpdates(out string message)
        { 
            DateTime newUpdateTime;
            var updates = _seWs.GetUpdates(SessionId, CurrentUser.UserName, _seWsLastUpdate, out message, out newUpdateTime);
            _seWsLastUpdate = newUpdateTime;
            return updates;
        }

        private void ReloadFromWs()
        {
            if (_seWs != null)
            {
                Subtitle = new Logic.Subtitle();
                var sequences = _seWs.GetSubtitle(SessionId, out FileName, out _seWsLastUpdate);
                foreach (var sequence in sequences)
                {
                    Paragraph p = new Paragraph(sequence.Text.Replace("<br />", Environment.NewLine), sequence.StartMilliseconds, sequence.EndMilliseconds);
                    Subtitle.Paragraphs.Add(p);
                }
                Subtitle.Renumber(1);
                LastSubtitle = new Subtitle(Subtitle);
            }
        }

        public void Log(string text)
        {            
            string timestamp = DateTime.Now.ToLongTimeString();
            _log.AppendLine(timestamp + ": " + text.TrimEnd() .Replace(Environment.NewLine, Configuration.Settings.General.ListViewLineSeparatorString));
        }

        public string GetLog()
        {
            return _log.ToString();
        }

        public void SendChatMessage(string message)
        {
            _seWs.SendMessage(SessionId, message.Replace(Environment.NewLine, "<br />"), CurrentUser);
        }

        internal void UpdateLine(int index, Paragraph paragraph)
        {
            _seWs.UpdateLine(SessionId, index, new SeNetworkService.SeSequence()
            {
                StartMilliseconds = (int)paragraph.StartTime.TotalMilliseconds,
                EndMilliseconds = (int)paragraph.EndTime.TotalMilliseconds,
                Text = paragraph.Text.Replace(Environment.NewLine, "<br />")
            }, CurrentUser);
            AddToWsUserLog(CurrentUser, index, "UPD", true);
        }

        public void CheckForAndSubmitUpdates(IEnumerable<SeNetworkService.SeUpdate> updates)
        {
            // check for changes in text/time codes (not insert/delete)
            if (LastSubtitle != null)
            {
                for (int i = 0; i < Subtitle.Paragraphs.Count; i++)
                {
                    Paragraph last = LastSubtitle.GetParagraphOrDefault(i);
                    Paragraph current = Subtitle.GetParagraphOrDefault(i);
                    if (last != null && current != null)
                    {
                        if (last.StartTime.TotalMilliseconds != current.StartTime.TotalMilliseconds ||
                            last.EndTime.TotalMilliseconds != current.EndTime.TotalMilliseconds ||
                            last.Text != current.Text)
                        {
                            UpdateLine(i, current);
                        }
                    }
                }
            }
        }

        public void AddToWsUserLog(SeNetworkService.SeUser user, int pos, string action, bool updateUI)
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
            if (updateUI && OnUpdateUserLogEntries != null)
                OnUpdateUserLogEntries.Invoke(null, null);
        }


        internal void Leave()
        {
            try
            {
                _seWs.Leave(SessionId, CurrentUser.UserName);
            }
            catch
            { 
            }
        }

        internal void DeleteLines(List<int> indices)
        {
            _seWs.DeleteLines(SessionId, indices.ToArray(), CurrentUser);
            StringBuilder sb = new StringBuilder();
            foreach (int index in indices)
            {
                sb.Append(index + ", ");
                AdjustUpdateLogToDelete(index);
            }
            Log("Delete line: " + sb.ToString().Trim().TrimEnd(','));
        }

        internal void InsertLine(int index, Paragraph newParagraph)
        {
            _seWs.InsertLine(SessionId, index, (int)newParagraph.StartTime.TotalMilliseconds, (int)newParagraph.EndTime.TotalMilliseconds, newParagraph.Text, CurrentUser);
            Log("Insert line at " + index.ToString() + ": " + newParagraph.ToString());
        }

        internal void AdjustUpdateLogToInsert(int index)
        {
            foreach (var logEntry in UpdateLog)
            {
                if (logEntry.Index >= index)
                    logEntry.Index++;
            }
        }

        internal void AdjustUpdateLogToDelete(int index)
        {
            UpdateLogEntry removeThis = null;
            foreach (var logEntry in UpdateLog)
            {
                if (logEntry.Index == index)
                    removeThis = logEntry;
                else if (logEntry.Index > index)
                    logEntry.Index--;
            }
            if (removeThis != null)
                UpdateLog.Remove(removeThis);
        }

    }
}
