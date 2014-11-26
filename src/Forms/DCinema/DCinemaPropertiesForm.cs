using System;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms.DCinema
{
    public /* abstract */ class DCinemaPropertiesForm : PositionAndSizeForm
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
    }
}
