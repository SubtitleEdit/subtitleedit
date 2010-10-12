using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class VobSubOcrCharacter : Form
    {
        public VobSubOcrCharacter()
        {
            InitializeComponent();

            var language = Configuration.Settings.Language.VobSubOcrCharacter;
            Text = language.Title;
            labelSubtitleImage.Text = language.SubtitleImage;
            buttonExpandSelection.Text = language.ExpandSelection;
            buttonShrinkSelection.Text = language.ShrinkSelection;
            labelCharacters.Text = language.Characters;
            labelCharactersAsText.Text = language.CharactersAsText;
            checkBoxItalic.Text = language.Italic;
            buttonAbort.Text = language.Abort;
            buttonOK.Text = Configuration.Settings.Language.General.OK;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
        }

        public string ManualRecognizedCharacters
        {
            get
            {
                return textBoxCharacters.Text;
            }
        }

        public bool IsItalic
        {
            get
            {
                return checkBoxItalic.Checked;
            }
        }

        public Point FormPosition
        {
            get
            {
                return new Point(Left, Top);
            }
        }

        public bool ExpandSelection { get; private set; }

        public bool ShrinkSelection { get; private set; }

        public void Initialize(Bitmap vobSubImage, ImageSplitterItem character, Point position, bool italicChecked, bool showShrink)
        {
            buttonShrinkSelection.Visible = showShrink;

            checkBoxItalic.Checked = italicChecked;
            if (position.X != -1 && position.Y != -1)
            {
                StartPosition = FormStartPosition.Manual;
                Left = position.X;
                Top = position.Y;
            }
            
            pictureBoxSubtitleImage.Image = vobSubImage;
            pictureBoxCharacter.Image = character.Bitmap;


            Bitmap org = (Bitmap)vobSubImage.Clone();
            Bitmap bm = new Bitmap(org.Width, org.Height);
            Graphics g = Graphics.FromImage(bm);
            g.DrawImage(org, 0, 0, org.Width, org.Height);
//            g.FillRectangle(Brushes.Black, 5, 5, 100, 30);
            g.DrawRectangle(Pens.Red, character.X, character.Y, character.Bitmap.Width, character.Bitmap.Height - 1);
            g.Dispose();
            pictureBoxSubtitleImage.Image = bm;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void TextBoxCharactersKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                DialogResult = DialogResult.OK;
            else if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }

        private void CheckBoxItalicCheckedChanged(object sender, EventArgs e)
        {
            textBoxCharacters.Focus();
        }

        private void ButtonExpandSelectionClick(object sender, EventArgs e)
        {
            ExpandSelection = true;
            DialogResult = DialogResult.OK;
        }

        private void ButtonShrinkSelectionClick(object sender, EventArgs e)
        {
            ShrinkSelection = true;
            DialogResult = DialogResult.OK;
        }
    }
}
