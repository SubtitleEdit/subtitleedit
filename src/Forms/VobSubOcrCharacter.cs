using System;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;
using System.Collections.Generic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class VobSubOcrCharacter : Form
    {

        private VobSubOcr _vobSubForm;
        private List<VobSubOcr.ImageCompareAddition> _additions;

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
            labelItalicOn.Text = language.Italic.Replace("&", string.Empty);
            labelItalicOn.Visible = false;
            buttonAbort.Text = language.Abort;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = language.Skip;
            nordicToolStripMenuItem.Text = language.Nordic;
            spanishToolStripMenuItem.Text = language.Spanish;
            germanToolStripMenuItem.Text = language.German;
            checkBoxAutoSubmitOfFirstChar.Text = language.AutoSubmitOnFirstChar;

            dataGridView1.Rows.Add("♪", "á", "é", "í", "ó", "ö", "ő", "ú", "ü", "ű");
            dataGridView1.Rows.Add("♫", "Á", "É", "Í", "Ó", "Ö", "Ő", "Ú", "Ü", "Ű");

            Utilities.FixLargeFonts(this, buttonCancel);
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

        internal void Initialize(Bitmap vobSubImage, ImageSplitterItem character, Point position, bool italicChecked, bool showShrink, VobSubOcr.CompareMatch bestGuess, List<VobSubOcr.ImageCompareAddition> additions, VobSubOcr vobSubForm)
        {
            ShrinkSelection = false;
            ExpandSelection = false;

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
            _additions = additions;

            buttonShrinkSelection.Visible = showShrink;

            checkBoxItalic.Checked = italicChecked;
            if (position.X != -1 && position.Y != -1)
            {
                StartPosition = FormStartPosition.Manual;
                Left = position.X;
                Top = position.Y;
            }

            pictureBoxSubtitleImage.Image = vobSubImage;
            pictureBoxCharacter.Image = character.NikseBitmap.GetBitmap();

            if (_additions.Count > 0)
            {
                var last = _additions[_additions.Count - 1];
                buttonLastEdit.Visible = true;
                if (last.Italic)
                    buttonLastEdit.Font = new Font(buttonLastEdit.Font.FontFamily, buttonLastEdit.Font.Size, FontStyle.Italic);
                else
                    buttonLastEdit.Font = new Font(buttonLastEdit.Font.FontFamily, buttonLastEdit.Font.Size);
                pictureBoxLastEdit.Visible = true;
                pictureBoxLastEdit.Image = last.Image.GetBitmap();
                buttonLastEdit.Text = string.Format(Configuration.Settings.Language.VobSubOcrCharacter.EditLastX, last.Text);
                pictureBoxLastEdit.Top = buttonLastEdit.Top - last.Image.Height + buttonLastEdit.Height;
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
            g.DrawRectangle(Pens.Red, character.X, character.Y, character.NikseBitmap.Width, character.NikseBitmap.Height - 1);
            g.Dispose();
            pictureBoxSubtitleImage.Image = bm;

            pictureBoxCharacter.Top = labelCharacters.Top + 16;
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
                labelCharactersAsText.Font = new Font(labelCharactersAsText.Font.FontFamily, labelCharactersAsText.Font.Size, FontStyle.Italic);
                textBoxCharacters.Font = new Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size, FontStyle.Italic);
                labelItalicOn.Visible = true;
            }
            else
            {
                labelCharactersAsText.Font = new Font(labelCharactersAsText.Font.FontFamily, labelCharactersAsText.Font.Size);
                textBoxCharacters.Font = new Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size);
                labelItalicOn.Visible = false;
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
            if (_additions.Count > 0)
            {
                var last = _additions[_additions.Count - 1];
                var result = _vobSubForm.EditImageCompareCharacters(last.Name, last.Text);
                if (result == DialogResult.OK)
                {
                    _additions.RemoveAt(_additions.Count - 1);
                    _vobSubForm.StartOcrFromDelayed();
                    DialogResult = DialogResult.Abort;
                }
            }
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

        private void VobSubOcrCharacter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Left && buttonShrinkSelection.Visible)
            {
                ButtonShrinkSelectionClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Alt && e.KeyCode == Keys.Right && buttonExpandSelection.Visible)
            {
                ButtonExpandSelectionClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Subtract && buttonShrinkSelection.Visible)
            {
                ButtonShrinkSelectionClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Shift && e.KeyCode == Keys.Add && buttonExpandSelection.Visible)
            {
                ButtonExpandSelectionClick(null, null);
                e.SuppressKeyPress = true;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            textBoxCharacters.Text = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
        }
    }
}
