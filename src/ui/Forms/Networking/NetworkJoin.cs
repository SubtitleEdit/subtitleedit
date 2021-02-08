using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Net;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Networking
{
    public sealed partial class NetworkJoin : Form
    {

        private Logic.Networking.NikseWebServiceSession _networkSession;
        public string FileName { get; set; }

        public NetworkJoin()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelStatus.Text = string.Empty;
            Text = LanguageSettings.Current.NetworkJoin.Title;
            labelInfo.Text = LanguageSettings.Current.NetworkJoin.Information;
            labelSessionKey.Text = LanguageSettings.Current.General.SessionKey;
            labelUserName.Text = LanguageSettings.Current.General.UserName;
            labelWebServiceUrl.Text = LanguageSettings.Current.General.WebServiceUrl;
            buttonJoin.Text = LanguageSettings.Current.NetworkJoin.Join;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonCancel);
        }

        internal void Initialize(Logic.Networking.NikseWebServiceSession networkSession)
        {
            _networkSession = networkSession;

            textBoxSessionKey.Text = Configuration.Settings.NetworkSettings.SessionKey;
            if (textBoxSessionKey.Text.Trim().Length < 2)
            {
                textBoxSessionKey.Text = Guid.NewGuid().ToString().RemoveChar('-');
            }

            comboBoxWebServiceUrl.Text = Configuration.Settings.NetworkSettings.WebApiUrl;
            textBoxUserName.Text = Configuration.Settings.NetworkSettings.UserName;
            if (textBoxUserName.Text.Trim().Length < 2)
            {
                textBoxUserName.Text = Dns.GetHostName();
            }
        }

        private void buttonJoin_Click(object sender, EventArgs e)
        {
            Configuration.Settings.NetworkSettings.SessionKey = textBoxSessionKey.Text;
            Configuration.Settings.NetworkSettings.WebApiUrl = comboBoxWebServiceUrl.Text;
            Configuration.Settings.NetworkSettings.UserName = textBoxUserName.Text;

            buttonJoin.Enabled = false;
            buttonCancel.Enabled = false;
            textBoxUserName.Enabled = false;
            comboBoxWebServiceUrl.Enabled = false;
            labelStatus.Text = string.Format(LanguageSettings.Current.NetworkStart.ConnectionTo, comboBoxWebServiceUrl.Text);
            Refresh();

            try
            {
                string message;
                _networkSession.Join(comboBoxWebServiceUrl.Text, textBoxUserName.Text, textBoxSessionKey.Text, out message);
                if (message == "OK")
                {
                    DialogResult = DialogResult.OK;
                    return;
                }
                else
                {
                    if (message == "Session not found!")
                    {
                        MessageBox.Show(string.Format(LanguageSettings.Current.Main.XNotFound, textBoxSessionKey.Text));
                    }
                    else if (message == "Username already in use!")
                    {
                        MessageBox.Show(string.Format(LanguageSettings.Current.General.UserNameAlreadyInUse, textBoxSessionKey.Text));
                    }
                    else
                    {
                        MessageBox.Show(message);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            buttonJoin.Enabled = true;
            buttonCancel.Enabled = true;
            textBoxUserName.Enabled = true;
            comboBoxWebServiceUrl.Enabled = true;
            labelStatus.Text = string.Empty;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void NetworkJoin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#networking");
                e.SuppressKeyPress = true;
            }
        }

    }
}
