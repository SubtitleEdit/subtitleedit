using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Forms.Options;
using Nikse.SubtitleEdit.Logic.SeJob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.SeJobs
{
    public partial class SeJobExport : Form
    {
        private Subtitle _subtitle;
        private Subtitle _subtitleOriginal;
        private readonly SubtitleFormat _subtitleFormat;
        private readonly string _videoFileName;
        private SeJobWaveform _waveform;
        private List<double> _sceneChanges;

        public SeJobExport(
            Subtitle subtitle,
            Subtitle subtitleOriginal,
            SubtitleFormat subtitleFormat,
            string videoFileName,
            WavePeakData waveform,
            List<double> sceneChanges)
        {
            InitializeComponent();

            _subtitle = subtitle;
            _subtitleOriginal = subtitleOriginal;
            _subtitleFormat = subtitleFormat;
            _videoFileName = videoFileName;
            if (waveform != null)
            {
                _waveform = new SeJobWaveform
                {
                    SampleRate = waveform.SampleRate,
                    HighestPeak = waveform.HighestPeak,
                    PeakMins = waveform.Peaks.Select(p => p.Min).ToList(),
                    PeakMaxs = waveform.Peaks.Select(p => p.Max).ToList(),
                };
            }

            _sceneChanges = sceneChanges;
            textBoxJobId.Text = Guid.NewGuid().ToString();
            textBoxSubtitleFileName.Text = Path.GetFileName(_subtitle?.FileName);

            numericUpDownMaxNumberOfLines.Value = Configuration.Settings.General.MaxNumberOfLines;
            numericUpDownSubtitleLineMaximumLength.Value = Configuration.Settings.General.SubtitleLineMaximumLength;
            numericUpDownMaxCharsSec.Value = (decimal)Configuration.Settings.General.SubtitleMaximumCharactersPerSeconds;
            numericUpDownDurationMin.Value = Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds;
            numericUpDownDurationMax.Value = Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds;
            numericUpDownMinGapMs.Value = Configuration.Settings.General.MinimumMillisecondsBetweenLines;
            numericUpDownMaxWordsMin.Value = (decimal)Configuration.Settings.General.SubtitleMaximumWordsPerMinute;
            numericUpDownOptimalCharsSec.Value = (decimal)Configuration.Settings.General.SubtitleOptimalCharactersPerSeconds;

            UpdateUiAfterLoad();
        }

        private void UpdateUiAfterLoad()
        {
            checkBoxOriginal.Enabled = _subtitleOriginal?.Paragraphs.Count > 0;
            labelSubtitleTotalCount.Text = $"Number of subtitles: {_subtitle.Paragraphs.Count}";
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            var model = new SeJobModel
            {
                Version = "1.0",
                JobId = textBoxJobId.Text,
                JobName = textBoxJobName.Text.Trim(),
                Message = textBoxJobDescription.Text.Trim(),
                SubtitleFileFormat = _subtitleFormat.Name,
                SubtitleFileName = textBoxSubtitleFileName.Text.Trim(),
                SubtitleContent = _subtitle.ToText(_subtitleFormat),
                VideoStreamingUrl = textBoxVideoUrl.Text.Trim(),
            };

            if (!string.IsNullOrEmpty(_videoFileName))
            {
                model.VideoHash = Path.GetFileNameWithoutExtension(WavePeakGenerator.GetPeakWaveFileName(_videoFileName));
                if (!string.IsNullOrEmpty(model.VideoStreamingUrl))
                {
                    model.VideoHash = MovieHasher.GenerateHashFromString(model.VideoStreamingUrl);
                }
            }

            if (checkBoxOriginal.Checked && _subtitleOriginal?.Paragraphs.Count > 0)
            {
                model.SubtitleFileNameOriginal = _subtitleOriginal.FileName;
                model.SubtitleContentOriginal = _subtitleOriginal.ToText(_subtitleFormat);
            }

            if (checkBoxIncludeWaveform.Checked && _waveform?.PeakMins.Count > 0)
            {
                model.Waveform = _waveform;
            }

            if (checkBoxIncludeSceneChanges.Checked && _sceneChanges?.Count > 0)
            {
                model.SceneChanges = _sceneChanges;
            }

            if (checkBoxIncludeBookmarks.Checked)
            {
                model.Bookmarks = new List<SeJobBookmark>();
                foreach (var p in _subtitle?.Paragraphs.Where(p => !string.IsNullOrEmpty(p.Bookmark)))
                {
                    model.Bookmarks.Add(new SeJobBookmark
                    {
                        Idx = _subtitle.GetIndex(p),
                        Txt = p.Bookmark,
                    });
                }
            }

            if (checkBoxIncludeRules.Checked)
            {
                model.Rules = new SeJobRules
                {
                    MaxNumberOfLines = (int)numericUpDownMaxNumberOfLines.Value,
                    SubtitleLineMaximumLength = (int)numericUpDownSubtitleLineMaximumLength.Value,
                    SubtitleMaximumCharactersPerSeconds = numericUpDownMaxCharsSec.Value,
                    SubtitleMinimumDisplayMilliseconds = (int)numericUpDownDurationMin.Value,
                    SubtitleMaximumDisplayMilliseconds = (int)numericUpDownDurationMax.Value,
                    MinimumMillisecondsBetweenLines = (int)numericUpDownMinGapMs.Value,
                    SubtitleMaximumWordsPerMinute = numericUpDownMaxWordsMin.Value,
                    SubtitleOptimalCharactersPerSeconds = numericUpDownOptimalCharsSec.Value,
                };
            }

            using (var saveDialog = new SaveFileDialog { FileName = Path.GetFileNameWithoutExtension(model.SubtitleFileName), Filter = "se-job|*.se-job" })
            {
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(saveDialog.FileName, SeJobHandler.SaveSeJob(model));
                    DialogResult = DialogResult.OK;
                }
            }
        }

        private void buttonImport_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog { Filter = "se-job files|*.se-job" })
            {
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                var seJob = SeJobHandler.LoadSeJob(FileUtil.ReadAllBytesShared(openFileDialog.FileName));
                if (seJob == null)
                {
                    return;
                }

                textBoxJobId.Text = seJob.JobId;
                textBoxJobName.Text = seJob.JobName;
                textBoxJobDescription.Text = seJob.Message;
                textBoxSubtitleFileName.Text = seJob.SubtitleFileName;
                textBoxVideoUrl.Text = seJob.VideoStreamingUrl;
                checkBoxIncludeWaveform.Checked = seJob.Waveform?.PeakMins.Count > 0;
                checkBoxIncludeSceneChanges.Checked = seJob.SceneChanges?.Count > 0;
                _waveform = seJob.Waveform;
                _sceneChanges = seJob.SceneChanges;
                checkBoxIncludeRules.Checked = seJob.Rules != null;
                if (seJob.Rules != null)
                {
                    numericUpDownMaxNumberOfLines.Value = seJob.Rules.MaxNumberOfLines;
                    numericUpDownSubtitleLineMaximumLength.Value = seJob.Rules.SubtitleLineMaximumLength;
                    numericUpDownMaxCharsSec.Value = seJob.Rules.SubtitleMaximumCharactersPerSeconds;
                    numericUpDownDurationMin.Value = seJob.Rules.SubtitleMinimumDisplayMilliseconds;
                    numericUpDownDurationMax.Value = seJob.Rules.SubtitleMaximumDisplayMilliseconds;
                    numericUpDownMinGapMs.Value = seJob.Rules.MinimumMillisecondsBetweenLines;
                    numericUpDownMaxWordsMin.Value = seJob.Rules.SubtitleMaximumWordsPerMinute;
                    numericUpDownOptimalCharsSec.Value = seJob.Rules.SubtitleOptimalCharactersPerSeconds;
                }

                _subtitle = new Subtitle();
                if (!string.IsNullOrEmpty(seJob.SubtitleFileFormat) && !string.IsNullOrEmpty(seJob.SubtitleContent))
                {
                    var format = SubtitleFormat.AllSubtitleFormats.FirstOrDefault(p => p.Name == seJob.SubtitleFileFormat);
                    format?.LoadSubtitle(_subtitle, seJob.SubtitleContent.SplitToLines(), string.Empty);
                }

                if (!string.IsNullOrEmpty(seJob.SubtitleFileFormat) && !string.IsNullOrEmpty(seJob.SubtitleContentOriginal))
                {
                    _subtitleOriginal = new Subtitle();
                    var format = SubtitleFormat.AllSubtitleFormats.FirstOrDefault(p => p.Name == seJob.SubtitleFileFormat);
                    format?.LoadSubtitle(_subtitleOriginal, seJob.SubtitleContentOriginal.SplitToLines(), string.Empty);
                }

                UpdateUiAfterLoad();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SeJobExport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void buttonGapChoose_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsGapChoose())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    numericUpDownMinGapMs.Value = form.MinGapMs;
                }
            }
        }
    }
}
