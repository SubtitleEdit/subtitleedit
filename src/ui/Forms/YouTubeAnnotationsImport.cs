using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class YouTubeAnnotationsImport : Form
    {
        public YouTubeAnnotationsImport(Dictionary<string, int> stylesWithCount)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = "YouTube Annotations";
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            listViewFixes.Columns[1].Text = string.Empty; // style // TODO: Add better text + help text
            UiUtil.FixLargeFonts(this, buttonOK);

            foreach (KeyValuePair<string, int> kvp in stylesWithCount)
            {
                ListViewItem item = new ListViewItem();
                item.SubItems.Add(kvp.Key);
                item.SubItems.Add(kvp.Value.ToString());
                item.Checked = kvp.Key.Trim() == "speech";
                listViewFixes.Items.Add(item);
            }
        }

        public List<string> SelectedStyles
        {
            get
            {
                List<string> styles = new List<string>();
                foreach (ListViewItem item in listViewFixes.Items)
                {
                    if (item.Checked)
                    {
                        styles.Add(item.SubItems[1].Text);
                    }
                }
                return styles;
            }
        }

        private void YouTubeAnnotationsImport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void YouTubeAnnotationsImport_ResizeEnd(object sender, EventArgs e)
        {
            listViewFixes.AutoSizeLastColumn();
        }

        private void YouTubeAnnotationsImport_Shown(object sender, EventArgs e)
        {
            YouTubeAnnotationsImport_ResizeEnd(sender, e);
        }
    }
}
