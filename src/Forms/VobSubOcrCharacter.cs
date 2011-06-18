using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class VobSubOcrCharacter : Form
    {

        VobSubOcr _vobSubForm;
        string _lastAdditionName;
        string _lastAdditionText;

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
            buttonCancel.Text = language.Skip;
            nordicToolStripMenuItem.Text = language.Nordic;
            spanishToolStripMenuItem.Text = language.Spanish;
            germanToolStripMenuItem.Text = language.German;
            checkBoxAutoSubmitOfFirstChar.Text = language.AutoSubmitOnFirstChar;

            FixLargeFonts();
        }

        private void FixLargeFonts()
        {
            Graphics graphics = this.CreateGraphics();
            SizeF textSize = graphics.MeasureString(buttonCancel.Text, this.Font);
            if (textSize.Height > buttonCancel.Height - 4)
            {
                int newButtonHeight = (int)(textSize.Height + 7 + 0.5);
                Utilities.SetButtonHeight(this, newButtonHeight, 1);
            }
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

        public void Initialize(Bitmap vobSubImage, ImageSplitterItem character, Point position, bool italicChecked, bool showShrink, Nikse.SubtitleEdit.Forms.VobSubOcr.CompareMatch bestGuess, string lastAdditionName, string lastAdditionText, Bitmap lastAdditionImage, bool lastAdditionItalic, VobSubOcr vobSubForm)
        {
            textBoxCharacters.Text = string.Empty;
            if (bestGuess != null)
            {
                buttonGuess.Visible = false; // hm... not too useful :(
                buttonGuess.Text = bestGuess.Text;
            }
            else
            {
                buttonGuess.Visible = false;
            }

            _vobSubForm = vobSubForm;
            _lastAdditionName = lastAdditionName;
            _lastAdditionText = lastAdditionText;

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

            if (lastAdditionImage != null && lastAdditionName != null && lastAdditionText != null)
            {
                buttonLastEdit.Visible = true;
                if (lastAdditionItalic)
                    buttonLastEdit.Font = new System.Drawing.Font(buttonLastEdit.Font.FontFamily, buttonLastEdit.Font.Size, FontStyle.Italic);
                else
                    buttonLastEdit.Font = new System.Drawing.Font(buttonLastEdit.Font.FontFamily, buttonLastEdit.Font.Size);                
                pictureBoxLastEdit.Visible = true;
                pictureBoxLastEdit.Image = lastAdditionImage;
                buttonLastEdit.Text = "Edit last: " + lastAdditionText;
            }
            else
            {
                buttonLastEdit.Visible = false;
                pictureBoxLastEdit.Visible = false;
            }            

            Bitmap org = (Bitmap)vobSubImage.Clone();
            Bitmap bm = new Bitmap(org.Width, org.Height);
            Graphics g = Graphics.FromImage(bm);
            g.DrawImage(org, 0, 0, org.Width, org.Height);
            g.DrawRectangle(Pens.Red, character.X, character.Y, character.Bitmap.Width, character.Bitmap.Height - 1);
            g.Dispose();
            pictureBoxSubtitleImage.Image = bm;

            pictureBoxCharacter.Top = labelCharacters.Top + 16;
            pictureBoxLastEdit.Top = buttonLastEdit.Top - 5;
            pictureBoxLastEdit.Left = buttonLastEdit.Left + buttonLastEdit.Width + 5;
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

            if (checkBoxItalic.Checked)
            {
                labelCharactersAsText.Font = new System.Drawing.Font(labelCharactersAsText.Font.FontFamily, labelCharactersAsText.Font.Size, FontStyle.Italic);
                textBoxCharacters.Font = new System.Drawing.Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size, FontStyle.Italic);
            }
            else
            {
                labelCharactersAsText.Font = new System.Drawing.Font(labelCharactersAsText.Font.FontFamily, labelCharactersAsText.Font.Size);
                textBoxCharacters.Font = new System.Drawing.Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size);
            }
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

        private void buttonLastEdit_Click(object sender, EventArgs e)
        {
            _vobSubForm.EditImageCompareCharacters(_lastAdditionName, _lastAdditionText);
            textBoxCharacters.Focus();
        }

        private void buttonGuess_Click(object sender, EventArgs e)
        {
            textBoxCharacters.Text = buttonGuess.Text;
            DialogResult = DialogResult.OK;
        }

        private void InsertLanguageCharacter(object sender, EventArgs e)
        {
            textBoxCharacters.Text = textBoxCharacters.Text.Insert(textBoxCharacters.SelectionStart, (sender as ToolStripMenuItem).Text);

        }

        private void textBoxCharacters_TextChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoSubmitOfFirstChar.Checked && textBoxCharacters.Text.Length > 0)
                DialogResult = DialogResult.OK;
        }

        private void VobSubOcrCharacter_Shown(object sender, EventArgs e)
        {
            textBoxCharacters.Focus();
        }

        private void checkBoxAutoSubmitOfFirstChar_CheckedChanged(object sender, EventArgs e)
        {
            Focus();
            textBoxCharacters.Focus();
            textBoxCharacters.Focus();
            Application.DoEvents();
            textBoxCharacters.Focus();
            textBoxCharacters.Focus();
        }
    }
}
