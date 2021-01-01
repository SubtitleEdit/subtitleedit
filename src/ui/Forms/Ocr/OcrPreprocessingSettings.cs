using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Ocr;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Ocr
{
    public sealed partial class OcrPreprocessingSettings : Form
    {
        public PreprocessingSettings PreprocessingSettings { get; }
        private readonly bool _isBinaryImageCompare;
        private readonly NikseBitmap _source;
        private readonly bool _loading;

        public OcrPreprocessingSettings(Bitmap bitmap, bool isBinaryImageCompare, PreprocessingSettings preprocessingSettings)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _loading = true;
            _isBinaryImageCompare = isBinaryImageCompare;
            groupBoxBinaryImageCompareThreshold.Visible = isBinaryImageCompare;
            _source = new NikseBitmap(bitmap);
            pictureBoxSubtitleImage.Image = bitmap;
            if (preprocessingSettings != null)
            {
                PreprocessingSettings = preprocessingSettings;
            }
            else
            {
                PreprocessingSettings = new PreprocessingSettings
                {
                    BinaryImageCompareThreshold = Configuration.Settings.Tools.OcrBinaryImageCompareRgbThreshold
                };
            }

            checkBoxInvertColors.Checked = PreprocessingSettings.InvertColors;
            panelColorToWhite.BackColor = PreprocessingSettings.ColorToWhite;
            panelColorToRemove.BackColor = PreprocessingSettings.ColorToRemove;
            checkBoxCropTransparent.Checked = PreprocessingSettings.CropTransparentColors;
            checkBoxYellowToWhite.Checked = PreprocessingSettings.YellowToWhite;
            trackBarThresshold.Minimum = (int)numericUpDownThreshold.Minimum;
            trackBarThresshold.Maximum = (int)numericUpDownThreshold.Maximum;
            numericUpDownThreshold.Value = PreprocessingSettings.BinaryImageCompareThreshold;
            trackBarThresshold.Value = PreprocessingSettings.BinaryImageCompareThreshold;

            Text = LanguageSettings.Current.OcrPreprocessing.Title;
            groupBoxColors.Text = LanguageSettings.Current.OcrPreprocessing.Colors;
            checkBoxInvertColors.Text = LanguageSettings.Current.OcrPreprocessing.InvertColors;
            checkBoxYellowToWhite.Text = LanguageSettings.Current.OcrPreprocessing.YellowToWhite;
            buttonColorToWhite.Text = LanguageSettings.Current.OcrPreprocessing.ColorToWhite;
            buttonColorToRemove.Text = LanguageSettings.Current.OcrPreprocessing.ColorToRemove;
            buttonColorToRemove.Text = LanguageSettings.Current.OcrPreprocessing.ColorToRemove;
            groupBoxBinaryImageCompareThreshold.Text = LanguageSettings.Current.OcrPreprocessing.BinaryThreshold;
            labelThresholdDescription.Text = LanguageSettings.Current.OcrPreprocessing.AdjustAlpha;
            groupBoxCropping.Text = LanguageSettings.Current.OcrPreprocessing.Cropping;
            checkBoxCropTransparent.Text = LanguageSettings.Current.OcrPreprocessing.CropTransparentColors;
            labelOriginalImage.Text = LanguageSettings.Current.OcrPreprocessing.OriginalImage;
            labelPostImage.Text = LanguageSettings.Current.OcrPreprocessing.PostImage;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;

            _loading = false;
            RefreshImage();
            UiUtil.FixLargeFonts(this, buttonOK);
        }

        private void RefreshImage()
        {
            if (_loading)
            {
                return;
            }

            PreprocessingSettings.InvertColors = checkBoxInvertColors.Checked;
            PreprocessingSettings.YellowToWhite = checkBoxYellowToWhite.Enabled && checkBoxYellowToWhite.Checked;
            PreprocessingSettings.ColorToWhite = buttonColorToWhite.Enabled ? panelColorToWhite.BackColor : Color.Transparent;
            PreprocessingSettings.ColorToRemove = buttonColorToRemove.Enabled ? panelColorToRemove.BackColor : Color.Transparent;
            PreprocessingSettings.CropTransparentColors = checkBoxCropTransparent.Checked;
            PreprocessingSettings.BinaryImageCompareThreshold = (int)numericUpDownThreshold.Value;

            pictureBox1.Image?.Dispose();
            var n = new NikseBitmap(_source);

            if (PreprocessingSettings.CropTransparentColors)
            {
                n.CropSidesAndBottom(2, Color.Transparent, true);
                n.CropSidesAndBottom(2, Color.FromArgb(0, 0, 0, 0), true);
                n.CropTop(2, Color.Transparent);
                n.CropTop(2, Color.FromArgb(0, 0, 0, 0));
            }
            if (PreprocessingSettings.InvertColors)
            {
                n.InvertColors();
            }
            if (PreprocessingSettings.YellowToWhite)
            {
                n.ReplaceYellowWithWhite();
            }
            if (panelColorToWhite.BackColor != Color.Transparent)
            {
                n.ReplaceColor(panelColorToWhite.BackColor.A, panelColorToWhite.BackColor.R, panelColorToWhite.BackColor.G, panelColorToWhite.BackColor.B, 255, 255, 255, 255);
            }
            if (panelColorToRemove.BackColor != Color.Transparent)
            {
                n.ReplaceColor(panelColorToRemove.BackColor.A, panelColorToRemove.BackColor.R, panelColorToRemove.BackColor.G, panelColorToRemove.BackColor.B, Color.Transparent.A, Color.Transparent.R, Color.Transparent.G, Color.Transparent.B);
            }
            if (_isBinaryImageCompare)
            {
                n.MakeTwoColor((int)numericUpDownThreshold.Value);
            }

            pictureBox1.Image = n.GetBitmap();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void pictureBoxSubtitleImage_Click(object sender, EventArgs e)
        {
            var p = pictureBoxSubtitleImage.PointToClient(MousePosition);
            var bmp = pictureBoxSubtitleImage.Image as Bitmap;
            if (bmp == null || p.X >= bmp.Width || p.Y>= bmp.Height)
            {
                return;
            }

            var color = bmp.GetPixel(p.X, p.Y);
            labelOriginalImage.Text = LanguageSettings.Current.OcrPreprocessing.OriginalImage + 
                                      $"  {p.X},{p.Y} ARGB({color.A}, {color.R},{color.G},{color.B})";
        }

        private void ColorToWhite(object sender, EventArgs e)
        {
            colorDialog1.Color = panelColorToWhite.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelColorToWhite.BackColor = colorDialog1.Color;
                RefreshImage();
            }
        }

        private void buttonColorToRemove_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = panelColorToRemove.BackColor;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                panelColorToRemove.BackColor = colorDialog1.Color;
                RefreshImage();
            }
        }

        private void panelColorToRemove_Click(object sender, EventArgs e)
        {
            buttonColorToRemove_Click(sender, e);
        }

        private void SetForeColorThreshold_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void checkBoxInvertColors_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxYellowToWhite.Enabled = !checkBoxInvertColors.Checked;

            buttonColorToWhite.Enabled = !checkBoxInvertColors.Checked;
            panelColorToWhite.Enabled = !checkBoxInvertColors.Checked;

            buttonColorToRemove.Enabled = !checkBoxInvertColors.Checked;
            panelColorToRemove.Enabled = !checkBoxInvertColors.Checked;

            RefreshImage();
        }

        private void checkBoxCropTransparent_CheckedChanged(object sender, EventArgs e)
        {
            RefreshImage();
        }

        private void numericUpDownThreshold_ValueChanged(object sender, EventArgs e)
        {
            trackBarThresshold.ValueChanged -= trackBarThresshold_ValueChanged;
            trackBarThresshold.Value = (int)numericUpDownThreshold.Value;
            trackBarThresshold.ValueChanged += trackBarThresshold_ValueChanged;
            RefreshImage();
        }

        private void trackBarThresshold_ValueChanged(object sender, EventArgs e)
        {
            numericUpDownThreshold.ValueChanged -= numericUpDownThreshold_ValueChanged;
            numericUpDownThreshold.Value = trackBarThresshold.Value;
            numericUpDownThreshold.ValueChanged += numericUpDownThreshold_ValueChanged;
            RefreshImage();
        }
    }
}
