using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Networking;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class NetworkChat : Form
    {
        private Logic.Networking.NikseWebServiceSession _networkSession;
        private string breakChars = "\".!?,)([]<>:;♪{}-/#*| ¿¡" + Environment.NewLine + "\t";

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        public NetworkChat()
        {
            InitializeComponent();
            buttonSendChat.Text = Configuration.Settings.Language.NetworkChat.Send;
            listViewUsers.Columns[0].Text = Configuration.Settings.Language.General.UserName;
            listViewUsers.Columns[1].Text = Configuration.Settings.Language.General.IP;
            listViewChat.Columns[0].Text = Configuration.Settings.Language.General.UserName;
            listViewChat.Columns[1].Text = Configuration.Settings.Language.General.Text;
        }

        internal void Initialize(NikseWebServiceSession networkSession)
        {
            _networkSession = networkSession;
            Text = Configuration.Settings.Language.NetworkChat.Title + " - " + _networkSession.CurrentUser.UserName;

            listViewUsers.Items.Clear();
            foreach (var user in _networkSession.Users)
            {
                AddUser(user);
            }

            listViewChat.Items.Clear();
            foreach (var message in _networkSession.ChatLog)
            {
                AddChatMessage(message.User, message.Message);
                listViewChat.EnsureVisible(listViewChat.Items.Count - 1);
            }
        }

        private void buttonSendChat_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxChat.Text))
            {
                _networkSession.SendChatMessage(textBoxChat.Text.Trim());
                AddChatMessage(_networkSession.CurrentUser, textBoxChat.Text.Trim());
                listViewChat.EnsureVisible(listViewChat.Items.Count - 1);
                _networkSession.ChatLog.Add(new NikseWebServiceSession.ChatEntry { User = _networkSession.CurrentUser, Message = textBoxChat.Text.Trim() });
            }
            textBoxChat.Text = string.Empty;
            textBoxChat.Focus();
        }

        public void AddChatMessage(SeNetworkService.SeUser user, string message)
        {
            var item = new ListViewItem(user.UserName);
            item.Tag = _networkSession.CurrentUser;
            item.ForeColor = Utilities.GetColorFromUserName(user.UserName);
            item.ImageIndex = Utilities.GetNumber0To7FromUserName(user.UserName);
            item.SubItems.Add(new ListViewItem.ListViewSubItem(item, message));
            listViewChat.Items.Add(item);
        }

        private void textBoxChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && e.Modifiers == Keys.None)
            {
                e.SuppressKeyPress = true;
                buttonSendChat_Click(null, null);
            }
            else
            {
                if (e.KeyData == (Keys.Control | Keys.A))
                {
                    textBoxChat.SelectAll();
                    e.SuppressKeyPress = true;
                }
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.Back)
                {
                    int index = textBoxChat.SelectionStart;
                    if (textBoxChat.SelectionLength == 0)
                    {
                        var s = textBoxChat.Text;
                        int deleteFrom = index - 1;

                        if (deleteFrom > 0 && deleteFrom < s.Length)
                        {
                            if (s[deleteFrom] == ' ')
                                deleteFrom--;
                            while (deleteFrom > 0 && !breakChars.Contains(s[deleteFrom]))
                            {
                                deleteFrom--;
                            }
                            if (deleteFrom == index - 1)
                            {
                                while (deleteFrom > 0 && breakChars.Replace(" ", string.Empty).Contains(s[deleteFrom - 1]))
                                {
                                    deleteFrom--;
                                }
                            }
                            if (s[deleteFrom] == ' ')
                                deleteFrom++;
                            textBoxChat.Text = s.Remove(deleteFrom, index - deleteFrom);
                            textBoxChat.SelectionStart = deleteFrom;
                        }
                    }
                    e.SuppressKeyPress = true;
                }
            }
        }

        internal void AddUser(SeNetworkService.SeUser user)
        {
            var item = new ListViewItem(user.UserName);
            item.Tag = user;
            item.ForeColor = Utilities.GetColorFromUserName(user.UserName);
            if (DateTime.Now.Month == 12 && DateTime.Now.Day >= 23 && DateTime.Now.Day <= 25)
                item.ImageIndex = 7;
            else
                item.ImageIndex = Utilities.GetNumber0To7FromUserName(user.UserName);
            item.SubItems.Add(new ListViewItem.ListViewSubItem(item, user.Ip));
            listViewUsers.Items.Add(item);
        }

        internal void RemoveUser(SeNetworkService.SeUser user)
        {
            ListViewItem removeItem = null;
            foreach (ListViewItem item in listViewUsers.Items)
            {
                if ((item.Tag as SeNetworkService.SeUser).UserName == user.UserName)
                {
                    removeItem = item;
                }
            }
            if (removeItem != null)
                listViewUsers.Items.Remove(removeItem);
        }
    }
}
