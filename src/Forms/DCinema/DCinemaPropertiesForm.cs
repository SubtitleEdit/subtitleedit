using System;
using System.ComponentModel;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms.DCinema
{
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DCinemaPropertiesForm, Form>))]
    public abstract class DCinemaPropertiesForm : Form
    {
        protected void FixLargeFonts(Button referenceButton)
        {
            using (var graphics = CreateGraphics())
            {
                var textSize = graphics.MeasureString(referenceButton.Text, Font);
                if (textSize.Height > referenceButton.Height - 4)
                {
                    var newButtonHeight = (int)(textSize.Height + 7.5f);
                    Utilities.SetButtonHeight(this, newButtonHeight, 1);
                }
            }
        }

        protected void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
