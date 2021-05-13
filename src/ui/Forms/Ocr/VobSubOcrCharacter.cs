using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class VobSubOcrCharacter : Form
    {

        private VobSubOcr _vobSubForm;
        private List<VobSubOcr.ImageCompareAddition> _additions;

        public VobSubOcrCharacter()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            VobSubEditCharacters.MakeToolStripLetters(contextMenuStripLetters, InsertLanguageCharacter);
            UiUtil.FixFonts(this);

            var language = LanguageSettings.Current.VobSubOcrCharacter;
            Text = language.Title;
            labelSubtitleImage.Text = language.SubtitleImage;
            buttonExpandSelection.Text = language.ExpandSelection;
            buttonShrinkSelection.Text = language.ShrinkSelection;
            labelCharacters.Text = language.Characters;
            labelCharactersAsText.Text = language.CharactersAsText;
            checkBoxItalic.Text = language.Italic;
            labelItalicOn.Text = language.Italic.RemoveChar('&');
            labelItalicOn2.Text = language.Italic.RemoveChar('&');
            labelItalicOn.Visible = false;
            labelItalicOn2.Visible = false;
            buttonAbort.Text = language.Abort;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonSkip.Text = language.Skip;
            checkBoxAutoSubmitOfFirstChar.Text = language.AutoSubmitOnFirstChar;

            dataGridView1.Columns.Clear();
            var row1 = Configuration.Settings.Tools.OcrAddLetterRow1.Split(';').ToArray<object>();
            var row2 = Configuration.Settings.Tools.OcrAddLetterRow2.Split(';').ToArray<object>();
            for (int i = 0; i < Math.Max(row1.Length, row2.Length); i++)
            {
                dataGridView1.Columns.Add(string.Empty, string.Empty);
                dataGridView1.Columns[dataGridView1.Columns.Count - 1].Width = 25;
                dataGridView1.Columns[dataGridView1.Columns.Count - 1].DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    ForeColor = UiUtil.ForeColor,
                    BackColor = UiUtil.BackColor
                };
            }
            dataGridView1.Rows.Add(row1);
            dataGridView1.Rows.Add(row2);
            dataGridView1.Rows[0].Height = dataGridView1.Height / 2;
            dataGridView1.Rows[1].Height = dataGridView1.Height / 2;
            UiUtil.FixLargeFonts(this, buttonSkip);
        }

        public string ManualRecognizedCharacters => textBoxCharacters.Text;

        public bool IsItalic => checkBoxItalic.Checked;

        public Point FormPosition => new Point(Left, Top);

        public bool ExpandSelection { get; private set; }

        public bool ShrinkSelection { get; private set; }

        public bool SkipImage { get; private set; }

        internal void Initialize(Bitmap vobSubImage, ImageSplitterItem character, Point position, bool italicChecked, bool showShrink, VobSubOcr.CompareMatch bestGuess, List<VobSubOcr.ImageCompareAddition> additions, VobSubOcr vobSubForm, bool allowExpand = true)
        {
            ShrinkSelection = false;
            ExpandSelection = false;
            SkipImage = false;

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
            labelItalicOn2.Left = Math.Max(40, pictureBoxCharacter.Left + pictureBoxCharacter.Width);

            if (_additions.Count > 0)
            {
                var last = _additions[_additions.Count - 1];
                buttonLastEdit.Visible = true;
                if (last.Italic)
                {
                    buttonLastEdit.Font = new Font(buttonLastEdit.Font.FontFamily, buttonLastEdit.Font.Size, FontStyle.Italic);
                }
                else
                {
                    buttonLastEdit.Font = new Font(buttonLastEdit.Font.FontFamily, buttonLastEdit.Font.Size);
                }

                pictureBoxLastEdit.Visible = true;
                pictureBoxLastEdit.Image = last.Image.GetBitmap();
                buttonLastEdit.Text = string.Format(LanguageSettings.Current.VobSubOcrCharacter.EditLastX, last.Text);
                pictureBoxLastEdit.Top = buttonLastEdit.Top - last.Image.Height + buttonLastEdit.Height;
            }
            else
            {
                buttonLastEdit.Visible = false;
                pictureBoxLastEdit.Visible = false;
            }

            using (var org = (Bitmap)vobSubImage.Clone())
            {
                var bm = new Bitmap(org.Width, org.Height);
                using (var g = Graphics.FromImage(bm))
                {
                    g.DrawImage(org, 0, 0, org.Width, org.Height);
                    g.DrawRectangle(Pens.Red, character.X - 1, character.Y - 1, character.NikseBitmap.Width + 1, character.NikseBitmap.Height + 1);
                }
                pictureBoxSubtitleImage.Image = bm;
            }

            pictureBoxCharacter.Top = labelCharacters.Top + 16;
            pictureBoxLastEdit.Left = buttonLastEdit.Left + buttonLastEdit.Width + 5;

            buttonExpandSelection.Visible = allowExpand;
        }

        private void ButtonOkClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void TextBoxCharactersKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                DialogResult = DialogResult.OK;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void CheckBoxItalicCheckedChanged(object sender, EventArgs e)
        {
            textBoxCharacters.Focus();

            if (checkBoxItalic.Checked)
            {
                labelCharactersAsText.Font = new Font(labelCharactersAsText.Font.FontFamily, labelCharactersAsText.Font.Size, FontStyle.Italic);
                textBoxCharacters.Font = new Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size, FontStyle.Italic);
                dataGridView1.Font = new Font(dataGridView1.Font.FontFamily, dataGridView1.Font.Size, FontStyle.Italic);
                labelItalicOn.Visible = true;
                labelItalicOn2.Visible = true;
                checkBoxItalic.Font = new Font(checkBoxItalic.Font.FontFamily, checkBoxItalic.Font.Size, FontStyle.Italic | FontStyle.Bold);
                checkBoxItalic.ForeColor = Color.Red;
            }
            else
            {
                labelCharactersAsText.Font = new Font(labelCharactersAsText.Font.FontFamily, labelCharactersAsText.Font.Size);
                textBoxCharacters.Font = new Font(textBoxCharacters.Font.FontFamily, textBoxCharacters.Font.Size);
                dataGridView1.Font = new Font(dataGridView1.Font.FontFamily, dataGridView1.Font.Size);
                labelItalicOn.Visible = false;
                labelItalicOn2.Visible = false;
                checkBoxItalic.Font = new Font(checkBoxItalic.Font.FontFamily, checkBoxItalic.Font.Size);
                checkBoxItalic.ForeColor = DefaultForeColor;
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
            if (sender is ToolStripMenuItem toolStripMenuItem)
            {
                var start = textBoxCharacters.SelectionStart;
                textBoxCharacters.SelectedText = toolStripMenuItem.Text;
                textBoxCharacters.SelectionLength = 0;
                textBoxCharacters.SelectionStart = start + toolStripMenuItem.Text.Length;
            }
        }

        private void textBoxCharacters_TextChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoSubmitOfFirstChar.Checked && textBoxCharacters.Text.Length > 0)
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void VobSubOcrCharacter_Shown(object sender, EventArgs e)
        {
            textBoxCharacters.Focus();

            if (ActiveForm == null)
            {
                TaskbarList.StartBlink(
                    _vobSubForm,
                    Configuration.Settings.VobSubOcr.UnfocusedAttentionBlinkCount,
                    Configuration.Settings.VobSubOcr.UnfocusedAttentionPlaySoundCount,
                    2);
            }
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
            if (buttonShrinkSelection.Visible &&
                e.Modifiers == Keys.Alt && e.KeyCode == Keys.Left ||
                e.Modifiers == Keys.Shift && e.KeyCode == Keys.Subtract ||
                e.Modifiers == Keys.Alt && e.KeyCode == Keys.D)
            {
                ButtonShrinkSelectionClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (buttonExpandSelection.Visible &&
                     e.Modifiers == Keys.Alt && e.KeyCode == Keys.Right ||
                     e.Modifiers == Keys.Shift && e.KeyCode == Keys.Add ||
                     e.Modifiers == Keys.Alt && e.KeyCode == Keys.E)
            {
                ButtonExpandSelectionClick(null, null);
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == Keys.Control && e.KeyCode == Keys.I)
            {
                checkBoxItalic.Checked = !checkBoxItalic.Checked;
                e.SuppressKeyPress = true;
            }
            else if (e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.S)
            {
                SkipImage = true;
                DialogResult = DialogResult.Cancel;
                e.SuppressKeyPress = true;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            textBoxCharacters.Text = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
        }

        private void VobSubOcrCharacter_FormClosing(object sender, FormClosingEventArgs e)
        {
            TaskbarList.StopBlink(_vobSubForm);
        }

        private void buttonSkip_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
