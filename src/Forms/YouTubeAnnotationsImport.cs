using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class YouTubeAnnotationsImport : Form
    {
        public YouTubeAnnotationsImport(Dictionary<string, int> stylesWithCount)
        {
            InitializeComponent();

            Text = "YouTube Annotations";
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            //listViewFixes.Columns[0].Text = Configuration.Settings.Language.General.Apply;
            listViewFixes.Columns[1].Text = string.Empty; // style // TODO: Add better text + help text
            Utilities.FixLargeFonts(this, buttonOK);

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
                        styles.Add(item.SubItems[1].Text);
                }
                return styles;
            }
        }

        private void YouTubeAnnotationsImport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                DialogResult = DialogResult.Cancel;
        }
    }
}
