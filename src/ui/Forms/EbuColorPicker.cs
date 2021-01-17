using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class EbuColorPicker : Form
    {

        public string Color { get; private set; }

        public EbuColorPicker(bool showBlack)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.ColorChooser.Title;
            if (!showBlack)
            {
                buttonBlack.Visible = false;
                buttonBlue.Top -= buttonBlue.Height;
                buttonCyan .Top -= buttonBlue.Height;
                buttonGreen.Top -= buttonBlue.Height;
                buttonMagenta.Top -= buttonBlue.Height;
                buttonRed.Top -= buttonBlue.Height;
                buttonWhite.Top -= buttonBlue.Height;
                buttonYellow.Top -= buttonBlue.Height;
                Height -= buttonBlue.Height;
            }
        }

        private void EbuColorPicker_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonBlack_Click(object sender, EventArgs e)
        {
            Color = "Black";
            DialogResult = DialogResult.OK;
        }

        private void buttonRed_Click(object sender, EventArgs e)
        {
            Color = "Red";
            DialogResult = DialogResult.OK;
        }

        private void buttonGreen_Click(object sender, EventArgs e)
        {
            Color = "Green";
            DialogResult = DialogResult.OK;
        }

        private void buttonYellow_Click(object sender, EventArgs e)
        {
            Color = "Yellow";
            DialogResult = DialogResult.OK;
        }

        private void buttonBlue_Click(object sender, EventArgs e)
        {
            Color = "Blue";
            DialogResult = DialogResult.OK;
        }

        private void buttonMagenta_Click(object sender, EventArgs e)
        {
            Color = "Magenta";
            DialogResult = DialogResult.OK;
        }

        private void buttonCyan_Click(object sender, EventArgs e)
        {
            Color = "Cyan";
            DialogResult = DialogResult.OK;
        }

        private void buttonWhite_Click(object sender, EventArgs e)
        {
            Color = "White";
            DialogResult = DialogResult.OK;
        }

    }
}
