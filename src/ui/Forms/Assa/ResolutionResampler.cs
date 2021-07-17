using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using System;
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

            if (!string.IsNullOrEmpty(_subtitle.Header))
            {
                var oldPlayResX = AdvancedSubStationAlpha.GetTagFromHeader("PlayResX", "[Script Info]", _subtitle.Header);
                if (int.TryParse(oldPlayResX, out var w))
                {
                    numericUpDownSourceWidth.Value = w;
                }

                var oldPlayResY = AdvancedSubStationAlpha.GetTagFromHeader("PlayResY", "[Script Info]", _subtitle.Header);
                if (int.TryParse(oldPlayResY, out var h))
                {
                    numericUpDownSourceHeight.Value = h;
                }
            }

            if (_videoInfo != null && _videoInfo.Width > 0 && _videoInfo.Height > 0)
            {
                numericUpDownSourceWidth.Value = _videoInfo.Width;
                numericUpDownSourceHeight.Value = _videoInfo.Height;
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
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
