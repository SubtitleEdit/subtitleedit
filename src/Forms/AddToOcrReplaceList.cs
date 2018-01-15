using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Core.Dictionaries;
using Nikse.SubtitleEdit.Logic;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class AddToOcrReplaceList : Form
    {

        private OcrFixReplaceList _ocrFixReplaceList;

        public AddToOcrReplaceList(string input, string language, OcrFixReplaceList ocrFixReplaceList)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = Configuration.Settings.Language.AddToOcrReplaceList.Title;
            labelDescription.Text = Configuration.Settings.Language.AddToOcrReplaceList.Description;
            buttonAddOcrFix.Text = Configuration.Settings.Language.Settings.AddPair;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonCancel);

            textBoxOcrFixKey.Text = input;

            textBoxLanguage.Text = language;
            _ocrFixReplaceList = ocrFixReplaceList;
        }

        private void buttonAddOcrFix_Click(object sender, System.EventArgs e)
        {
            string key = textBoxOcrFixKey.Text.RemoveControlCharacters().Trim();
            string value = textBoxOcrFixValue.Text.RemoveControlCharacters().Trim();
            if (key.Length == 0 || value.Length == 0 || key == value || Utilities.IsInteger(key))
                return;

            var added = _ocrFixReplaceList.AddWordOrPartial(key, value);
            if (!added)
            {
                MessageBox.Show(Configuration.Settings.Language.Settings.WordAlreadyExists);
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void AddToOcrReplaceList_Shown(object sender, System.EventArgs e)
        {
            textBoxOcrFixValue.Focus();
        }
    }
}
