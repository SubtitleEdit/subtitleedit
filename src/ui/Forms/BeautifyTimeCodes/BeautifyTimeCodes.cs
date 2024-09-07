﻿using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.Forms;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.ShotChanges;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;

namespace Nikse.SubtitleEdit.Forms.BeautifyTimeCodes
{
    public sealed partial class BeautifyTimeCodes : Form
    {
        private readonly Subtitle _subtitle;
        private readonly VideoInfo _videoInfo;
        private readonly double _frameRate;
        private readonly double _duration;
        private readonly string _videoFileName;

        private List<double> _timeCodes = new List<double>();
        private List<double> _shotChanges = new List<double>();

        public List<double> ShotChangesInSeconds = new List<double>(); // For storing imported/generated shot changes that will be returned to the main form
        public Subtitle FixedSubtitle { get; private set; }

        public List<UnfixableParagraphsPair> UnfixableParagraphs { get; private set; }
        public struct UnfixableParagraphsPair
        {
            public Paragraph leftParagraph;
            public Paragraph rightParagraph;
            public int gapFrames;
        }

        private bool _abortTimeCodes;
        private TimeCodesGenerator _timeCodesGenerator;

        public BeautifyTimeCodes(Subtitle subtitle, VideoInfo videoInfo, string videoFileName, List<double> shotChanges)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            if (videoInfo != null && videoInfo.FramesPerSecond > 1)
            {
                _frameRate = videoInfo.FramesPerSecond;
            }
            else
            {
                _frameRate = Configuration.Settings.General.CurrentFrameRate;
            }

            if (videoInfo != null && videoInfo.TotalMilliseconds > 0)
            {
                _duration = videoInfo.TotalSeconds;
            }

            _videoInfo = videoInfo;
            _videoFileName = videoFileName;
            _subtitle = subtitle;

            var language = LanguageSettings.Current.BeautifyTimeCodes;
            var settings = Configuration.Settings.BeautifyTimeCodes;
            Text = language.Title;
            groupBoxTimeCodes.Text = language.GroupTimeCodes;
            groupBoxShotChanges.Text = language.GroupShotChanges;
            checkBoxAlignTimeCodes.Text = language.AlignTimeCodes;
            checkBoxExtractExactTimeCodes.Text = language.ExtractExactTimeCodes;
            buttonExtractTimeCodes.Text = language.ExtractTimeCodes;
            buttonCancelTimeCodes.Text = language.CancelTimeCodes;
            checkBoxSnapToShotChanges.Text = language.SnapToShotChanges;
            buttonImportShotChanges.Text = language.ImportShotChanges;
            buttonEditProfile.Text = language.EditProfile;
            labelExtractTimeCodesProgress.Text = "";

            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            UiUtil.FixLargeFonts(this, buttonOK);

            labelTimeCodesStatus.Font = new Font(labelTimeCodesStatus.Font, FontStyle.Bold);
            labelShotChangesStatus.Font = new Font(labelShotChangesStatus.Font, FontStyle.Bold);

            checkBoxAlignTimeCodes.Checked = settings.AlignTimeCodes;
            checkBoxExtractExactTimeCodes.Checked = settings.ExtractExactTimeCodes;
            checkBoxSnapToShotChanges.Checked = settings.SnapToShotChanges;

            // Check if video is loaded
            if (_videoFileName != null)
            {
                // Load time codes
                _timeCodes = TimeCodesFileHelper.FromDisk(videoFileName);
                _shotChanges = shotChanges;

                // Check if ffprobe is available              
                if (!IsFfProbeAvailable(this))
                {
                    checkBoxExtractExactTimeCodes.Enabled = false;
                    checkBoxExtractExactTimeCodes.Checked = false;
                }
            }
            else
            {
                checkBoxExtractExactTimeCodes.Enabled = false;
                checkBoxExtractExactTimeCodes.Checked = false;
                groupBoxShotChanges.Enabled = false;
                checkBoxSnapToShotChanges.Enabled = false;
                checkBoxSnapToShotChanges.Checked = false;
            }

            RefreshControls();
        }

        private void RefreshControls()
        {
            panelTimeCodes.Enabled = checkBoxAlignTimeCodes.Checked;
            panelExtractTimeCodes.Enabled = checkBoxExtractExactTimeCodes.Checked;
            panelShotChanges.Enabled = checkBoxSnapToShotChanges.Checked;

            if (_timeCodes.Count > 0)
            {
                labelTimeCodesStatus.Text = string.Format(LanguageSettings.Current.BeautifyTimeCodes.XTimeCodesLoaded, _timeCodes.Count);
                buttonExtractTimeCodes.Enabled = false;
                progressBarExtractTimeCodes.Enabled = false;
            }
            else
            {
                labelTimeCodesStatus.Text = LanguageSettings.Current.BeautifyTimeCodes.NoTimeCodesLoaded;
                buttonExtractTimeCodes.Enabled = true;
                buttonExtractTimeCodes.BringToFront();
                progressBarExtractTimeCodes.Enabled = true;
                progressBarExtractTimeCodes.Value = 0;
            }

            if (_shotChanges.Count > 0)
            {
                labelShotChangesStatus.Text = string.Format(LanguageSettings.Current.BeautifyTimeCodes.XShotChangesLoaded, _shotChanges.Count);
                buttonImportShotChanges.Enabled = false;
            }
            else
            {
                labelShotChangesStatus.Text = LanguageSettings.Current.BeautifyTimeCodes.NoShotChangesLoaded;
                buttonImportShotChanges.Enabled = true;
            }
        }

        private void buttonExtractTimeCodes_Click(object sender, EventArgs e)
        {
            _timeCodesGenerator = new TimeCodesGenerator();
            _abortTimeCodes = false;
            checkBoxSnapToShotChanges.Enabled = false;
            panelShotChanges.Enabled = false;
            labelTimeCodesStatus.Enabled = false;
            checkBoxExtractExactTimeCodes.Enabled = false;
            buttonExtractTimeCodes.Enabled = false;
            buttonCancelTimeCodes.Visible = true;
            buttonCancelTimeCodes.BringToFront();
            progressBarExtractTimeCodes.Style = ProgressBarStyle.Marquee;
            labelExtractTimeCodesProgress.Visible = true;
            Cursor = Cursors.WaitCursor;
            buttonOK.Enabled = false;
            buttonCancel.Enabled = false;

            var success = false;
            using (var process = _timeCodesGenerator.GetProcess(_videoFileName))
            {
                while (!process.HasExited)
                {
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(100);

                    UpdateTimeCodeProgress();

                    if (_abortTimeCodes)
                    {
                        process.Kill();
                        break;
                    }
                }

                if (!_abortTimeCodes)
                {
                    success = process.ExitCode == 0;
                }
            }

            UpdateTimeCodeProgress();

            checkBoxSnapToShotChanges.Enabled = true;
            panelShotChanges.Enabled = true;
            labelTimeCodesStatus.Enabled = true;
            checkBoxExtractExactTimeCodes.Enabled = true;
            buttonExtractTimeCodes.Enabled = true;
            buttonExtractTimeCodes.BringToFront();
            buttonCancelTimeCodes.Visible = false;
            Cursor = Cursors.Default;
            buttonOK.Enabled = true;
            buttonCancel.Enabled = true;
            progressBarExtractTimeCodes.Style = ProgressBarStyle.Blocks;
            progressBarExtractTimeCodes.Value = 0;
            labelExtractTimeCodesProgress.Visible = false;

            if (!_abortTimeCodes && success)
            {
                _timeCodes = _timeCodesGenerator.GetTimeCodes();
                if (_timeCodes.Count > 0)
                {
                    TimeCodesFileHelper.SaveTimeCodes(_videoFileName, _timeCodes);
                }
            }

            RefreshControls();
        }

        private void UpdateTimeCodeProgress()
        {
            if (!(_duration > 0) || !(_timeCodesGenerator.LastSeconds > 0))
            {
                return;
            }

            if (progressBarExtractTimeCodes.Style != ProgressBarStyle.Blocks)
            {
                progressBarExtractTimeCodes.Style = ProgressBarStyle.Blocks;
                progressBarExtractTimeCodes.Maximum = Convert.ToInt32(_duration);
            }

            progressBarExtractTimeCodes.Value = Math.Min(Convert.ToInt32(_timeCodesGenerator.LastSeconds), progressBarExtractTimeCodes.Maximum);
            labelExtractTimeCodesProgress.Text = FormatSeconds(_timeCodesGenerator.LastSeconds) + @" / " + FormatSeconds(_duration);
        }

        private string FormatSeconds(double seconds)
        {
            var t = TimeSpan.FromSeconds(seconds);
            return t.ToString(@"hh\:mm\:ss");
        }

        private void buttonEditProfile_Click(object sender, EventArgs e)
        {
            using (var form = new BeautifyTimeCodesProfile(_frameRate))
            {
                form.ShowDialog(this);
            }
        }

        private void buttonImportShotChanges_Click(object sender, EventArgs e)
        {
            using (var form = new ImportShotChanges(_videoInfo, _videoFileName))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _shotChanges = form.ShotChangesInSeconds;
                    ShotChangesInSeconds = form.ShotChangesInSeconds;
                    ShotChangeHelper.SaveShotChanges(_videoFileName, form.ShotChangesInSeconds);
                    RefreshControls();
                }
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            // Validation
            if (checkBoxExtractExactTimeCodes.Checked && _timeCodes.Count == 0) // we check the extract checkbox here, otherwise it should just calculate the time codes based on frame rate
            {
                MessageBox.Show(this, string.Format(LanguageSettings.Current.BeautifyTimeCodes.NoTimeCodesLoadedError, LanguageSettings.Current.BeautifyTimeCodes.ExtractTimeCodes), LanguageSettings.Current.General.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }

            if (checkBoxSnapToShotChanges.Checked && _shotChanges.Count == 0)
            {
                MessageBox.Show(this, string.Format(LanguageSettings.Current.BeautifyTimeCodes.NoShotChangesLoadedError, LanguageSettings.Current.BeautifyTimeCodes.ImportShotChanges), LanguageSettings.Current.General.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }

            // Prepare
            groupBoxTimeCodes.Enabled = false;
            groupBoxShotChanges.Enabled = false;
            buttonOK.Enabled = false;

            // Actual processing
            FixedSubtitle = new Subtitle(_subtitle, false);

            var timeCodesBeautifier = new TimeCodesBeautifier(
                FixedSubtitle,
                _frameRate,
                checkBoxExtractExactTimeCodes.Checked ? _timeCodes : new List<double>(), // ditto
                checkBoxSnapToShotChanges.Checked ? _shotChanges : new List<double>()
            );
            timeCodesBeautifier.ProgressChanged += delegate (int progress)
            {
                progressBar.Value = progress;
                progressBar.Invalidate();
            };
            timeCodesBeautifier.Beautify();

            // Check for any paragraphs that still have a gap smaller than the smallest max. gap
            CheckUnfixableParagraphs();

            // Re-enable group boxes, otherwise child controls are also disabled, and we need their original state for the checks below
            groupBoxTimeCodes.Enabled = true;
            groupBoxShotChanges.Enabled = true;

            // Save settings
            Configuration.Settings.BeautifyTimeCodes.AlignTimeCodes = checkBoxAlignTimeCodes.Checked;

            if (checkBoxExtractExactTimeCodes.Enabled)
            {
                Configuration.Settings.BeautifyTimeCodes.ExtractExactTimeCodes = checkBoxExtractExactTimeCodes.Checked;
            }

            if (checkBoxSnapToShotChanges.Enabled)
            {
                Configuration.Settings.BeautifyTimeCodes.SnapToShotChanges = checkBoxSnapToShotChanges.Checked;
            }

            DialogResult = DialogResult.OK;
        }

        private void CheckUnfixableParagraphs()
        {
            var minGap = Configuration.Settings.BeautifyTimeCodes.Profile.Gap;
            var minGapInCueClosest = Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestLeftGap + Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesInCueClosestRightGap;
            var minGapOutCueClosest = Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestLeftGap + Configuration.Settings.BeautifyTimeCodes.Profile.ConnectedSubtitlesOutCueClosestRightGap;
            var smallestMaxGapFrames = TimeCodesBeautifierUtils.GetSmallestMaxGapFrames(_frameRate);

            UnfixableParagraphs = new List<UnfixableParagraphsPair>();

            for (var p = 0; p < FixedSubtitle.Paragraphs.Count - 2; p++)
            {
                var paragraph = FixedSubtitle.Paragraphs.ElementAtOrDefault(p);
                var nextParagraph = FixedSubtitle.Paragraphs.ElementAtOrDefault(p + 1);

                if (paragraph != null && nextParagraph != null)
                {
                    var gapFrames = SubtitleFormat.MillisecondsToFrames(nextParagraph.StartTime.TotalMilliseconds - paragraph.EndTime.TotalMilliseconds, _frameRate);
                    if (gapFrames < smallestMaxGapFrames && gapFrames != minGap && gapFrames != minGapInCueClosest && gapFrames != minGapOutCueClosest)
                    {
                        UnfixableParagraphs.Add(new UnfixableParagraphsPair()
                        {
                            leftParagraph = paragraph,
                            rightParagraph = nextParagraph,
                            gapFrames = gapFrames
                        });
                    }
                }
            }
        }

        private void checkBoxAlignTimeCodes_CheckedChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void checkBoxExtractExactTimeCodes_CheckedChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void checkBoxSnapToShotChanges_CheckedChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void buttonCancelTimeCodes_Click(object sender, EventArgs e)
        {
            _abortTimeCodes = true;
        }

        public static bool IsFfProbeAvailable(Form parentForm)
        {
            if (!Configuration.IsRunningOnWindows)
            {
                return true;
            }

            var ffprobeExists = !string.IsNullOrWhiteSpace(Configuration.Settings.General.FFmpegLocation)
                            && File.Exists(Path.Combine(Path.GetDirectoryName(Configuration.Settings.General.FFmpegLocation), "ffprobe.exe"));

            if (!ffprobeExists)
            {
                if (MessageBox.Show(string.Format(LanguageSettings.Current.Settings.DownloadX, "ffprobe"), "Subtitle Edit", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                {
                    return false;
                }

                using (var form = new DownloadFfmpeg("ffprobe"))
                {
                    if (form.ShowDialog(parentForm) == DialogResult.OK)
                    {
                        return !string.IsNullOrWhiteSpace(Configuration.Settings.General.FFmpegLocation)
                               && File.Exists(Path.Combine(Path.GetDirectoryName(Configuration.Settings.General.FFmpegLocation), "ffprobe.exe"));
                    }
                }
            }

            return ffprobeExists;
        }

        private void BeautifyTimeCodes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                UiUtil.ShowHelp("#beautify_time_codes");
            }
        }
    }
}