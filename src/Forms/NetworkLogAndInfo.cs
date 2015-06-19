using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class NetworkLogAndInfo : Form
    {
        public NetworkLogAndInfo()
        {
            InitializeComponent();
            Text = Configuration.Settings.Language.NetworkLogAndInfo.Title;
            labelSessionKey.Text = Configuration.Settings.Language.General.SessionKey;
            labelUserName.Text = Configuration.Settings.Language.General.UserName;
            labelWebServiceUrl.Text = Configuration.Settings.Language.General.WebServiceUrl;
            labelLog.Text = Configuration.Settings.Language.NetworkLogAndInfo.Log;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
        }

        internal void Initialize(Logic.Networking.NikseWebServiceSession _networkSession)
        {
            textBoxSessionKey.Text = _networkSession.SessionId;
            textBoxUserName.Text = _networkSession.CurrentUser.UserName;
            textBoxWebServiceUrl.Text = _networkSession.WebServiceUrl;
            textBoxLog.Text = _networkSession.GetLog();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void NetworkLogAndInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}