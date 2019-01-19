using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class Interjections : Form
    {
        private List<string> _interjections;

        public Interjections()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
        }

        public string GetInterjectionsSemiColonSeperatedString()
        {
            var sb = new StringBuilder();
            foreach (string s in _interjections)
            {
                sb.Append(';');
                sb.Append(s.Trim());
            }
            return sb.ToString().Trim(';');
        }

        private void Interjections_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyCode == UiUtil.HelpKeys)
            {
                Utilities.ShowHelp("#remove_text_for_hi");
            }
        }

        public void Initialize(string semiColonSeperatedList)
        {
            _interjections = new List<string>();
            string[] arr = semiColonSeperatedList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in arr)
            {
                _interjections.Add(s.Trim());
            }
            FillListBox();
            Text = Configuration.Settings.Language.Interjections.Title;

            // Add to interjections (or general)
            buttonRemove.Text = Configuration.Settings.Language.Settings.Remove;
            buttonAdd.Text = Configuration.Settings.Language.MultipleReplace.Add;

            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            string text = textBoxInterjection.Text.Trim();
            if (text.Length == 0)
            {
                return;
            }

            if (!_interjections.Contains(text))
            {
                _interjections.Add(text);
                FillListBox();
                textBoxInterjection.Text = string.Empty;
                textBoxInterjection.Focus();
                for (int i = 0; i < listBoxInterjections.Items.Count; i++)
                {
                    if (listBoxInterjections.Items[i].ToString() == text)
                    {
                        listBoxInterjections.SelectedIndex = i;
                        int top = i - 5;
                        if (top < 0)
                        {
                            top = 0;
                        }

                        listBoxInterjections.TopIndex = top;
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show(Configuration.Settings.Language.Settings.WordAlreadyExists);
            }
        }

        private void FillListBox()
        {
            _interjections.Sort();
            listBoxInterjections.BeginUpdate();
            listBoxInterjections.Items.Clear();
            foreach (string s in _interjections)
            {
                listBoxInterjections.Items.Add(s);
            }
            listBoxInterjections.EndUpdate();
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            int index = listBoxInterjections.SelectedIndex;
            string text = listBoxInterjections.Items[index].ToString();
            if (index >= 0)
            {
                if (MessageBox.Show(string.Format(Configuration.Settings.Language.Settings.RemoveX, text), null, MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _interjections.Remove(text);
                    listBoxInterjections.Items.RemoveAt(index);
                    if (index < listBoxInterjections.Items.Count)
                    {
                        listBoxInterjections.SelectedIndex = index;
                    }
                    else if (listBoxInterjections.Items.Count > 0)
                    {
                        listBoxInterjections.SelectedIndex = index - 1;
                    }

                    listBoxInterjections.Focus();

                    return;
                }
                MessageBox.Show(Configuration.Settings.Language.Settings.WordNotFound);
            }
        }

        private void listBoxInterjections_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonRemove.Enabled = listBoxInterjections.SelectedIndex >= 0;
        }

        private void textBoxInterjection_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonAdd_Click(null, null);
            }
        }

        private void textBoxInterjection_TextChanged(object sender, EventArgs e)
        {
            buttonAdd.Enabled = textBoxInterjection.Text.Trim().Length > 0;
        }

    }
}
