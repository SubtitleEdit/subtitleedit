using Nikse.SubtitleEdit.Logic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class GenerateVideoWithSoftSubsOutFileName : Form
    {
        public string Suffix { get; set; }
        public string ReplaceList { get; set; }

        public GenerateVideoWithSoftSubsOutFileName(string suffix, string replaceList)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.GenerateVideoWithEmbeddedSubs.OutputFileNameSettings.Trim('.');
            labelSuffix.Text = LanguageSettings.Current.Settings.Suffix;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            textBoxSuffix.Text = suffix;
            textBoxReplaceList.Text = replaceList;
        }

        private void GenerateVideoWithSoftSubsOutFileName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
