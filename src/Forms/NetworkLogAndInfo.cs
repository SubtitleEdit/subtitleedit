using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class NetworkLogAndInfo : Form
    {
        public NetworkLogAndInfo()
        {
            InitializeComponent();
        }

        internal void Initialize(Logic.Networking.NikseWebServiceSession _networkSession)
        {
            textBoxSessionKey.Text = _networkSession.SessionId;
            textBoxUserName.Text = _networkSession.CurrentUser.UserName;
            textBoxWebServiceUrl.Text = _networkSession.WebServiceUrl;
            textBoxLog.Text = _networkSession.GetLog();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
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
