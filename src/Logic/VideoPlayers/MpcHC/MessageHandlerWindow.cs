using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Logic.VideoPlayers.MpcHC
{
    public partial class MessageHandlerWindow : Form
    {
        public event EventHandler OnCopyData;

        public MessageHandlerWindow()
        {
            InitializeComponent();
            Text = Guid.NewGuid().ToString();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WindowsMessageCopyData && OnCopyData != null)
            {
                OnCopyData.Invoke(m, new EventArgs());
            }
            base.WndProc(ref m);
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;
            base.OnLoad(e);
        }

    }
}
