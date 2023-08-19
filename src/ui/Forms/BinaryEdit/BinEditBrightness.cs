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
            nikseBitmap.CropTransparentSidesAndBottom(2, true);
            nikseBitmap.CropTopTransparent(2);
            _bitmap = nikseBitmap.GetBitmap();

            trackBarBrightness_Scroll(null, null);
            Factor = 1.0m;
            Alignment = ContentAlignment.BottomCenter;

            Text = LanguageSettings.Current.BinEdit.ChangeBrightnessTitle;
            labelChangeBrightness.Text = string.Format(LanguageSettings.Current.BinEdit.BrightnessX, numericUpDownBrightness.Value);
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void trackBarBrightness_Scroll(object sender, EventArgs e)
        {
            Factor = trackBarBrightness.Value / 100.0m;
            numericUpDownBrightness.ValueChanged -= numericUpDownBrightness_ValueChanged;
            numericUpDownBrightness.Value = trackBarBrightness.Value;
            numericUpDownBrightness.ValueChanged += numericUpDownBrightness_ValueChanged;
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            labelChangeBrightness.Text = string.Format(LanguageSettings.Current.BinEdit.BrightnessX, numericUpDownBrightness.Value.ToString("0.##"));
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

        private void numericUpDownBrightness_ValueChanged(object sender, EventArgs e)
        {
            Factor = numericUpDownBrightness.Value / 100.0m;
            trackBarBrightness.Scroll -= trackBarBrightness_Scroll;
            trackBarBrightness.Value = (int)Math.Round(numericUpDownBrightness.Value, MidpointRounding.AwayFromZero);
            trackBarBrightness.Scroll += trackBarBrightness_Scroll;
            UpdatePreview();
        }
    }
}
