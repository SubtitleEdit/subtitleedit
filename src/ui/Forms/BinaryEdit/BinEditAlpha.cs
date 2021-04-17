using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.BinaryEdit
{
    public sealed partial class BinEditAlpha : Form
    {
        private readonly Bitmap _bitmap;
        private Bitmap _backgroundImage;
        private bool _backgroundImageDark;

        public decimal Factor { get; private set; }
        public ContentAlignment Alignment { get; }

        public BinEditAlpha(Bitmap bitmap)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            var nikseBitmap = new NikseBitmap(bitmap);
            nikseBitmap.CropTransparentSidesAndBottom(99999, true);
            nikseBitmap.CropTopTransparent(2);
            _bitmap = nikseBitmap.GetBitmap();

            _backgroundImageDark = Configuration.Settings.General.UseDarkTheme;
            trackBarAlpha_Scroll(null, null);
            Factor = 1.0m;
            Alignment = ContentAlignment.BottomCenter;

            Text = LanguageSettings.Current.BinEdit.ChangeAlphaTitle;
            labelChangeAlpha.Text = string.Format(LanguageSettings.Current.BinEdit.AlphaX, trackBarAlpha.Value);
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void trackBarAlpha_Scroll(object sender, EventArgs e)
        {
            Factor = trackBarAlpha.Value / 100.0m;
            labelChangeAlpha.Text = string.Format(LanguageSettings.Current.BinEdit.AlphaX, trackBarAlpha.Value);
            var bmp = ChangeAlpha(_bitmap, Factor);

            if (_backgroundImage == null)
            {
                const int rectangleSize = 9;
                _backgroundImage = TextDesigner.MakeBackgroundImage(pictureBoxPreview.Width, pictureBoxPreview.Height, rectangleSize, _backgroundImageDark);
            }

            var finalBitmap = new Bitmap(bmp.Width, bmp.Height);
            using (var gfx = Graphics.FromImage(finalBitmap))
            {
                gfx.DrawImage(_backgroundImage, new Point(0, 0));
                gfx.DrawImage(bmp, new Point(0, 0));
            }

            bmp.Dispose();
            pictureBoxPreview.Image?.Dispose();
            pictureBoxPreview.Image = finalBitmap;
        }

        public static Bitmap ChangeAlpha(Bitmap bitmap, decimal factor)
        {
            var n = new NikseBitmap(bitmap);
            n.ChangeAlpha(factor);
            return n.GetBitmap();
        }

        private void BinEditAlpha_KeyDown(object sender, KeyEventArgs e)
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

        private void pictureBoxPreview_Click(object sender, EventArgs e)
        {
            _backgroundImageDark = !_backgroundImageDark;
            _backgroundImage?.Dispose();
            _backgroundImage = null;
            trackBarAlpha_Scroll(null, null);
        }
    }
}
