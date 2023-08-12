using Nikse.SubtitleEdit.Logic;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core.Common;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public sealed partial class SetLayer : Form
    {
        private Subtitle _subtitle;
        private Paragraph _p;
        public int Layer { get; set; }

        public SetLayer(Subtitle subtitle, Paragraph p)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitle = subtitle;
            _p = p;
            Text = LanguageSettings.Current.Main.Menu.ContextMenu.SetLayer;

            numericUpDownLayer.Minimum = int.MinValue;
            numericUpDownLayer.Maximum = int.MaxValue;
            numericUpDownLayer.Value = p?.Layer ?? 0;
        }

        private void SetLayer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void numericUpDownLayer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonOK_Click(null, null);
            }
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            Layer = (int)numericUpDownLayer.Value;
            DialogResult = DialogResult.OK;
        }

        private void SetLayer_Shown(object sender, System.EventArgs e)
        {
            numericUpDownLayer.Focus();
        }
    }
}
