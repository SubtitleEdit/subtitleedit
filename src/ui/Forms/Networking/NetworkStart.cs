using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Net;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Networking
{
    public sealed partial class NetworkStart : PositionAndSizeForm
    {

        private Logic.Networking.NikseWebServiceSession _networkSession;
        private string _fileName;

        public NetworkStart()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            labelStatus.Text = string.Empty;
            Text = LanguageSettings.Current.NetworkStart.Title;
            labelInfo.Text = LanguageSettings.Current.NetworkStart.Information;
            labelSessionKey.Text = LanguageSettings.Current.General.SessionKey;
            buttonGenerateNewKey.Text = LanguageSettings.Current.General.SessionKeyGenerate;
            using (var graphics = CreateGraphics())
            {
                var textSize = graphics.MeasureString(buttonGenerateNewKey.Text, Font);
                buttonGenerateNewKey.Width = (int)textSize.Width + 15;
            }
            labelUserName.Text = LanguageSettings.Current.General.UserName;
            labelWebServiceUrl.Text = LanguageSettings.Current.General.WebServiceUrl;
            buttonStart.Text = LanguageSettings.Current.NetworkStart.Start;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonCancel);
        }

        internal void Initialize(Logic.Networking.NikseWebServiceSession networkSession, string fileName)
        {
            _networkSession = networkSession;
            _fileName = fileName;

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

        private void buttonStart_Click(object sender, EventArgs e)
        {
            Configuration.Settings.NetworkSettings.SessionKey = textBoxSessionKey.Text;
            Configuration.Settings.NetworkSettings.WebApiUrl = comboBoxWebServiceUrl.Text;
            Configuration.Settings.NetworkSettings.UserName = textBoxUserName.Text;

            buttonStart.Enabled = false;
            buttonCancel.Enabled = false;
            textBoxSessionKey.Enabled = false;
            textBoxUserName.Enabled = false;
            comboBoxWebServiceUrl.Enabled = false;
            labelStatus.Text = string.Format(LanguageSettings.Current.NetworkStart.ConnectionTo, comboBoxWebServiceUrl.Text);
            Refresh();

            try
            {
                _networkSession.StartServer(comboBoxWebServiceUrl.Text, textBoxSessionKey.Text, textBoxUserName.Text, _fileName, out var message);
                if (message != "OK")
                {
                    MessageBox.Show(message);
                }
                else
                {
                    DialogResult = DialogResult.OK;
                    return;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
            buttonStart.Enabled = true;
            buttonCancel.Enabled = true;
            textBoxSessionKey.Enabled = false;
            textBoxUserName.Enabled = true;
            comboBoxWebServiceUrl.Enabled = true;
            labelStatus.Text = string.Empty;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void NetworkNew_KeyDown(object sender, KeyEventArgs e)
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

        private void buttonGenerateNewKey_Click(object sender, EventArgs e)
        {
            textBoxSessionKey.Text = Guid.NewGuid().ToString();
            textBoxSessionKey.SelectAll();
            textBoxSessionKey.Focus();
        }
    }
}
