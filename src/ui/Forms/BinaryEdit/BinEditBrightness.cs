using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.BinaryEdit
{
    public sealed partial class BinEditBrightness : Form
    {
        private readonly Bitmap _bitmap;

        public decimal Factor { get; private set; }
        public ContentAlignment Alignment { get; }

        public BinEditBrightness(Bitmap bitmap)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var nikseBitmap = new NikseBitmap(bitmap);
            nikseBitmap.CropTransparentSidesAndBottom(99999, true);
            nikseBitmap.CropTopTransparent(2);
            _bitmap = nikseBitmap.GetBitmap();

            trackBarBrightness_Scroll(null, null);
            Factor = 1.0m;
            Alignment = ContentAlignment.BottomCenter;

            Text = LanguageSettings.Current.BinEdit.ChangeBrightnessTitle;
            labelChangeBrightness.Text = string.Format(LanguageSettings.Current.BinEdit.BrightnessX, trackBarBrightness.Value);
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void trackBarBrightness_Scroll(object sender, EventArgs e)
        {
            Factor = trackBarBrightness.Value / 100.0m;
            labelChangeBrightness.Text = string.Format(LanguageSettings.Current.BinEdit.BrightnessX, trackBarBrightness.Value);
            var bmp = ChangeBrightness(_bitmap, Factor);
            pictureBoxPreview.Image?.Dispose();
            pictureBoxPreview.Image = bmp;
        }

        public static Bitmap ChangeBrightness(Bitmap bitmap, decimal factor)
        {
            var n = new NikseBitmap(bitmap);
            n.ChangeBrightness(factor);
            return n.GetBitmap();
        }

        private void BinEditBrightness_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
