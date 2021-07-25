using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public partial class ResolutionResampler : Form
    {
        private string _videoFileName;
        private VideoInfo _videoInfo;
        private Subtitle _subtitle;

        public ResolutionResampler(Subtitle subtitle, string videoFileName, VideoInfo videoInfo)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.AssaOverrideTags.History;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            _subtitle = subtitle;
            _videoFileName = videoFileName;
            _videoInfo = videoInfo;

            if (string.IsNullOrEmpty(_subtitle.Header))
            {
                _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
            }

            var oldPlayResX = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResX", "[Script Info]", _subtitle.Header);
            if (int.TryParse(oldPlayResX, out var w))
            {
                numericUpDownSourceWidth.Value = w;
            }

            var oldPlayResY = AdvancedSubStationAlpha.GetTagValueFromHeader("PlayResY", "[Script Info]", _subtitle.Header);
            if (int.TryParse(oldPlayResY, out var h))
            {
                numericUpDownSourceHeight.Value = h;
            }

            if (_videoInfo != null && _videoInfo.Width > 0 && _videoInfo.Height > 0)
            {
                numericUpDownTargetWidth.Value = _videoInfo.Width;
                numericUpDownTargetHeight.Value = _videoInfo.Height;
            }
        }

        private void buttonSourceRes_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.GetVideoFileFilter(false);
                openFileDialog1.FileName = string.Empty;
                if (string.IsNullOrEmpty(openFileDialog1.InitialDirectory) && !string.IsNullOrEmpty(_videoFileName))
                {
                    openFileDialog1.InitialDirectory = Path.GetDirectoryName(_videoFileName);
                    openFileDialog1.FileName = Path.GetFileName(_videoFileName);
                }

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    VideoInfo info = UiUtil.GetVideoInfo(openFileDialog1.FileName);
                    if (info != null && info.Success)
                    {
                        numericUpDownSourceWidth.Value = info.Width;
                        numericUpDownSourceHeight.Value = info.Height;
                    }
                }
            }
        }

        private void buttonGetResolutionFromVideo_Click(object sender, EventArgs e)
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Title = LanguageSettings.Current.General.OpenVideoFileTitle;
                openFileDialog1.FileName = string.Empty;
                openFileDialog1.Filter = UiUtil.GetVideoFileFilter(false);
                openFileDialog1.FileName = string.Empty;
                if (string.IsNullOrEmpty(openFileDialog1.InitialDirectory) && !string.IsNullOrEmpty(_videoFileName))
                {
                    openFileDialog1.InitialDirectory = Path.GetDirectoryName(_videoFileName);
                    openFileDialog1.FileName = Path.GetFileName(_videoFileName);
                }

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    VideoInfo info = UiUtil.GetVideoInfo(openFileDialog1.FileName);
                    if (info != null && info.Success)
                    {
                        numericUpDownTargetWidth.Value = info.Width;
                        numericUpDownTargetHeight.Value = info.Height;
                    }
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_subtitle.Header))
            {
                _subtitle.Header = AdvancedSubStationAlpha.DefaultHeader;
            }

            var sourceWidth = numericUpDownSourceWidth.Value;
            var sourceHeight = numericUpDownSourceHeight.Value;
            var targetWidth = numericUpDownTargetWidth.Value;
            var targetHeight = numericUpDownTargetHeight.Value;

            if (sourceWidth == 0 || sourceHeight == 0 || targetWidth == 0 || targetHeight == 0)
            {
                MessageBox.Show("Video width/height cannot be zero");
                return;
            }


            if (sourceWidth == targetWidth && sourceHeight == targetHeight)
            {
                MessageBox.Show("Source and target resolution is the same - nothing to do.");
                return;
            }

            var fixMargins = checkBoxMargins.Checked;
            var fixFonts = checkBoxFontSize.Checked;
            var fixPos = checkBoxPosition.Checked;
            var fixDraw = checkBoxDrawing.Checked;
            var styles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(_subtitle.Header);
            foreach (var style in styles)
            {
                if (fixMargins)
                {
                    style.MarginLeft = AssaResampler.Resample(sourceWidth, targetWidth, style.MarginLeft);
                    style.MarginRight = AssaResampler.Resample(sourceWidth, targetWidth, style.MarginLeft);
                    style.MarginVertical = AssaResampler.Resample(sourceHeight, targetHeight, style.MarginLeft);
                }
                style.FontSize = AssaResampler.Resample(sourceHeight, targetHeight, style.FontSize);
            }

            _subtitle.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(_subtitle.Header, styles);

            _subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResX", "PlayResX: " + targetWidth.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitle.Header);
            _subtitle.Header = AdvancedSubStationAlpha.AddTagToHeader("PlayResY", "PlayResY: " + targetHeight.ToString(CultureInfo.InvariantCulture), "[Script Info]", _subtitle.Header);

            foreach (var p in _subtitle.Paragraphs)
            {
                if (fixFonts)
                {
                    p.Text = AssaResampler.ResampleOverrideTagsFont(sourceWidth, targetWidth, sourceHeight, targetHeight, p.Text);
                }

                if (fixPos)
                {
                    p.Text = AssaResampler.ResampleOverrideTagsPosition(sourceWidth, targetWidth, sourceHeight, targetHeight, p.Text);
                }

                if (fixDraw)
                {
                    p.Text = AssaResampler.ResampleOverrideTagsDrawing(sourceWidth, targetWidth, sourceHeight, targetHeight, p.Text);
                }
            }

            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ResolutionResampler_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
