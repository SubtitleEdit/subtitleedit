using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms.Networking
{
    public sealed partial class NetworkLogAndInfo : Form
    {
        public NetworkLogAndInfo()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = Configuration.Settings.Language.NetworkLogAndInfo.Title;
            labelSessionKey.Text = Configuration.Settings.Language.General.SessionKey;
            labelUserName.Text = Configuration.Settings.Language.General.UserName;
            labelWebServiceUrl.Text = Configuration.Settings.Language.General.WebServiceUrl;
            labelLog.Text = Configuration.Settings.Language.NetworkLogAndInfo.Log;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
        }

        internal void Initialize(Logic.Networking.NikseWebServiceSession networkSession)
        {
            textBoxSessionKey.Text = networkSession.SessionId;
            textBoxUserName.Text = networkSession.CurrentUser.UserName;
            textBoxWebServiceUrl.Text = networkSession.WebServiceUrl;
            textBoxLog.Text = networkSession.GetLog();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void NetworkLogAndInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
