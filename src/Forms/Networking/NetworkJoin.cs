using System;
using System.Net;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

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
            Text = Configuration.Settings.Language.NetworkJoin.Title;
            labelInfo.Text = Configuration.Settings.Language.NetworkJoin.Information;
            labelSessionKey.Text = Configuration.Settings.Language.General.SessionKey;
            labelUserName.Text = Configuration.Settings.Language.General.UserName;
            labelWebServiceUrl.Text = Configuration.Settings.Language.General.WebServiceUrl;
            buttonJoin.Text = Configuration.Settings.Language.NetworkJoin.Join;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
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

            comboBoxWebServiceUrl.Text = Configuration.Settings.NetworkSettings.WebServiceUrl;
            textBoxUserName.Text = Configuration.Settings.NetworkSettings.UserName;
            if (textBoxUserName.Text.Trim().Length < 2)
            {
                textBoxUserName.Text = Dns.GetHostName();
            }
        }

        private void buttonJoin_Click(object sender, EventArgs e)
        {
            Configuration.Settings.NetworkSettings.SessionKey = textBoxSessionKey.Text;
            Configuration.Settings.NetworkSettings.WebServiceUrl = comboBoxWebServiceUrl.Text;
            Configuration.Settings.NetworkSettings.UserName = textBoxUserName.Text;

            buttonJoin.Enabled = false;
            buttonCancel.Enabled = false;
            textBoxUserName.Enabled = false;
            comboBoxWebServiceUrl.Enabled = false;
            labelStatus.Text = string.Format(Configuration.Settings.Language.NetworkStart.ConnectionTo, comboBoxWebServiceUrl.Text);
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
                        MessageBox.Show(string.Format(Configuration.Settings.Language.Main.XNotFound, textBoxSessionKey.Text));
                    }
                    else if (message == "Username already in use!")
                    {
                        MessageBox.Show(string.Format(Configuration.Settings.Language.General.UserNameAlreadyInUse, textBoxSessionKey.Text));
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
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#networking");
                e.SuppressKeyPress = true;
            }
        }

    }
}
