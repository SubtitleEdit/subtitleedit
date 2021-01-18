using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Networking
{
    public sealed partial class NetworkLogAndInfo : Form
    {
        public NetworkLogAndInfo()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.NetworkLogAndInfo.Title;
            labelSessionKey.Text = LanguageSettings.Current.General.SessionKey;
            labelUserName.Text = LanguageSettings.Current.General.UserName;
            labelWebServiceUrl.Text = LanguageSettings.Current.General.WebServiceUrl;
            labelLog.Text = LanguageSettings.Current.NetworkLogAndInfo.Log;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
        }

        internal void Initialize(Logic.Networking.NikseWebServiceSession networkSession)
        {
            textBoxSessionKey.Text = networkSession.SessionId;
            textBoxUserName.Text = networkSession.CurrentUser.UserName;
            textBoxWebServiceUrl.Text = networkSession.BaseUrl;
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
