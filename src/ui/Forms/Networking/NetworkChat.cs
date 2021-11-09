using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Networking;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Networking
{
    public sealed partial class NetworkChat : Form
    {
        private NikseWebServiceSession _networkSession;

        protected override bool ShowWithoutActivation => true;

        public NetworkChat()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            buttonSendChat.Text = LanguageSettings.Current.NetworkChat.Send;
            listViewUsers.Columns[0].Text = LanguageSettings.Current.General.UserName;
            listViewUsers.Columns[1].Text = LanguageSettings.Current.General.IP;
            listViewChat.Columns[0].Text = LanguageSettings.Current.General.UserName;
            listViewChat.Columns[1].Text = LanguageSettings.Current.General.Text;
        }

        internal void Initialize(NikseWebServiceSession networkSession)
        {
            _networkSession = networkSession;
            Text = LanguageSettings.Current.NetworkChat.Title + " - " + _networkSession.CurrentUser.UserName;

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
            ListViewItem item = new ListViewItem(user.UserName);
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
            else if (e.KeyData == (Keys.Control | Keys.A))
            {
                textBoxChat.SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyData == (Keys.Control | Keys.Back))
            {
                UiUtil.ApplyControlBackspace(textBoxChat);
                e.SuppressKeyPress = true;
            }
        }

        internal void AddUser(SeNetworkService.SeUser user)
        {
            ListViewItem item = new ListViewItem(user.UserName);
            item.Tag = user;
            item.ForeColor = Utilities.GetColorFromUserName(user.UserName);
            if (DateTime.Now.Month == 12 && DateTime.Now.Day >= 23 && DateTime.Now.Day <= 25)
            {
                item.ImageIndex = 7;
            }
            else
            {
                item.ImageIndex = Utilities.GetNumber0To7FromUserName(user.UserName);
            }

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
            {
                listViewUsers.Items.Remove(removeItem);
            }
        }

        private void NetworkChat_ResizeEnd(object sender, EventArgs e)
        {
            listViewChat.AutoSizeLastColumn();
            listViewUsers.AutoSizeLastColumn();
        }

        private void NetworkChat_Shown(object sender, EventArgs e)
        {
            NetworkChat_ResizeEnd(sender, e);
        }
    }
}
