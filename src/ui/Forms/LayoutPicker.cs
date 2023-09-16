using Nikse.SubtitleEdit.Logic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class LayoutPicker : Form
    {
        private int _layout;

        public LayoutPicker()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            CancelButton = buttonCancel;
        }


        public int GetLayout()
        {
            return _layout;
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            _layout = 1;
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            _layout = 2;
            DialogResult = DialogResult.OK;
        }

        private void button3_Click(object sender, System.EventArgs e)
        {
            _layout = 3;
            DialogResult = DialogResult.OK;
        }

        private void button4_Click(object sender, System.EventArgs e)
        {
            _layout = 4;
            DialogResult = DialogResult.OK;
        }

        private void button5_Click(object sender, System.EventArgs e)
        {
            _layout = 5;
            DialogResult = DialogResult.OK;
        }

        private void button6_Click(object sender, System.EventArgs e)
        {
            _layout = 6;
            DialogResult = DialogResult.OK;
        }

        private void button7_Click(object sender, System.EventArgs e)
        {
            _layout = 7;
            DialogResult = DialogResult.OK;
        }

        private void button8_Click(object sender, System.EventArgs e)
        {
            _layout = 8;
            DialogResult = DialogResult.OK;
        }

        private void button9_Click(object sender, System.EventArgs e)
        {
            _layout = 9;
            DialogResult = DialogResult.OK;
        }
    }
}
