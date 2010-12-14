using System;
using System.Net;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class NetworkStart : Form
    {

        Logic.Networking.NikseWebServiceSession _networkSession;
        string _fileName;

        public NetworkStart()
        {
            InitializeComponent();
            labelStatus.Text = string.Empty;            
        }

        internal void Initialize(Logic.Networking.NikseWebServiceSession networkSession, string fileName)
        {
            _networkSession = networkSession;
            _fileName = fileName;

            textBoxSessionKey.Text = Configuration.Settings.NetworkSettings.SessionKey;
            if (textBoxSessionKey.Text.Trim().Length < 2)
                textBoxSessionKey.Text = Guid.NewGuid().ToString().Replace("-", string.Empty);

            comboBoxWebServiceUrl.Text = Configuration.Settings.NetworkSettings.WebServiceUrl;
            textBoxUserName.Text = Configuration.Settings.NetworkSettings.UserName;
            if (textBoxUserName.Text.Trim().Length < 2)
                textBoxUserName.Text = Dns.GetHostName();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Configuration.Settings.NetworkSettings.SessionKey = textBoxSessionKey.Text;
            Configuration.Settings.NetworkSettings.WebServiceUrl = comboBoxWebServiceUrl.Text;
            Configuration.Settings.NetworkSettings.UserName = textBoxUserName.Text;

            buttonConnect.Enabled = false;
            buttonCancel.Enabled = false;
            textBoxSessionKey.Enabled = false;
            textBoxUserName.Enabled = false;
            comboBoxWebServiceUrl.Enabled = false;
            labelStatus.Text = string.Format("Connecting to {0}...", comboBoxWebServiceUrl.Text);
            Refresh();

            try
            {
                string message;
                _networkSession.StartServer(comboBoxWebServiceUrl.Text, textBoxSessionKey.Text, textBoxUserName.Text, _fileName, out message);
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
            buttonConnect.Enabled = true;
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
                DialogResult = DialogResult.Cancel;
        }
    }
}
