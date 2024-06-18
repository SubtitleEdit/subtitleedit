using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Tts
{
    public sealed partial class ReviewAudioClips : Form
    {
        public List<int> SkipIndices { get; set; }

        private readonly Subtitle _subtitle;
        private readonly TextToSpeech _textToSpeech;
        private readonly List<TextToSpeech.FileNameAndSpeedFactor> _fileNames;
        private bool _abortPlay;
        private LibMpvDynamic _libMpv;
        private Timer _mpvDoneTimer;

        public ReviewAudioClips(TextToSpeech textToSpeech, Subtitle subtitle, List<TextToSpeech.FileNameAndSpeedFactor> fileNames)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            Text = LanguageSettings.Current.TextToSpeech.ReviewAudioClips;
            labelInfo.Text = LanguageSettings.Current.TextToSpeech.ReviewInfo;
            buttonEdit.Text = LanguageSettings.Current.ExportCustomText.Edit;
            buttonPlay.Text = LanguageSettings.Current.TextToSpeech.Play;
            checkBoxContinuePlay.Text = LanguageSettings.Current.TextToSpeech.AutoContinue;
            columnHeaderCps.Text = LanguageSettings.Current.General.CharsPerSec;
            columnHeaderInclude.Text = string.Empty; // include
            columnHeaderText.Text = LanguageSettings.Current.General.Text;
            columnHeaderVoice.Text = LanguageSettings.Current.TextToSpeech.Voice;
            columnHeaderAdjustSpeed.Text = LanguageSettings.Current.TextToSpeech.Speed;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);

            _textToSpeech = textToSpeech;
            _subtitle = subtitle;
            _fileNames = fileNames;

            SkipIndices = new List<int>();

            listViewAudioClips.BeginUpdate();
            foreach (var p in subtitle.Paragraphs)
            {
                var item = new ListViewItem { Tag = p, Checked = true };
                item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
                item.SubItems.Add(_textToSpeech.GetParagraphAudio(p));
                item.SubItems.Add(Utilities.GetCharactersPerSecond(p).ToString("0.#", CultureInfo.InvariantCulture));

                var pInfo = fileNames[subtitle.GetIndex(p)];
                if (pInfo.Factor == 1)
                {
                    item.SubItems.Add("-");
                }
                else
                {
                    item.SubItems.Add($"{(pInfo.Factor * 100.0m):0.#}%");
                }

                item.SubItems.Add(p.Text);
                listViewAudioClips.Items.Add(item);
            }
            listViewAudioClips.EndUpdate();

            if (listViewAudioClips.Items.Count > 0)
            {
                listViewAudioClips.Items[0].Selected = true;
            }

            labelParagraphInfo.Text = string.Empty;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _libMpv?.Stop();
            for (var index = 0; index < listViewAudioClips.Items.Count; index++)
            {
                if (!listViewAudioClips.Items[index].Checked)
                {
                    SkipIndices.Add(index);
                }
            }

            DialogResult = DialogResult.OK;
        }

        private void VoicePreviewList_Load(object sender, EventArgs e)
        {
            listViewAudioClips.AutoSizeLastColumn();
        }

        private void VoicePreviewList_ResizeEnd(object sender, EventArgs e)
        {
            listViewAudioClips.AutoSizeLastColumn();
        }

        private void VoicePreviewList_Resize(object sender, EventArgs e)
        {
            listViewAudioClips.AutoSizeLastColumn();
        }

        private void Play(bool noAutoContinue = false)
        {
            if (listViewAudioClips.SelectedItems.Count == 0)
            {
                return;
            }

            var idx = listViewAudioClips.SelectedItems[0].Index;
            var waveFileName = _fileNames[idx].Filename;

            if (_libMpv != null)
            {
                _libMpv.Initialize(
                    null,
                    waveFileName,
                    (sender, args) =>
                    {
                        _libMpv.PlayRate = 1;
                        _libMpv.Play();
                    },
                    null);
                if (checkBoxContinuePlay.Checked && !noAutoContinue)
                {
                    _mpvDoneTimer.Start();
                }
            }
            else
            {
                using (var soundPlayer = new System.Media.SoundPlayer(waveFileName))
                {
                    soundPlayer.PlaySync();
                }

                if (!noAutoContinue)
                {
                    PlayNext(idx);
                }
            }
        }

        private void PlayNext(int idx)
        {
            if (checkBoxContinuePlay.Checked && !_abortPlay && idx < listViewAudioClips.Items.Count - 1)
            {
                listViewAudioClips.Items[idx].Selected = false;
                listViewAudioClips.Items[idx + 1].Selected = true;
                listViewAudioClips.Items[idx + 1].Focused = true;
                listViewAudioClips.Items[idx + 1].EnsureVisible();
                TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(10), () => Play());
                Application.DoEvents();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewAudioClips.SelectedItems.Count == 0)
            {
                labelParagraphInfo.Text = string.Empty;
                return;
            }

            var idx = listViewAudioClips.SelectedItems[0].Index;
            var p = _subtitle.Paragraphs[idx];
            labelParagraphInfo.Text = p.StartTime.ToDisplayString() + " --> " + p.EndTime.ToDisplayString() + " : " + p.Duration.ToShortDisplayString();
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            _abortPlay = false;
            Play();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            _libMpv?.Stop();
            _abortPlay = true;
        }

        private void VoicePreviewList_Shown(object sender, EventArgs e)
        {
            if (LibMpvDynamic.IsInstalled)
            {
                _libMpv = new LibMpvDynamic();
                _mpvDoneTimer = new Timer();
                _mpvDoneTimer.Interval = 100;
                _mpvDoneTimer.Tick += (o, args) =>
                {
                    if (listViewAudioClips.SelectedItems.Count == 0)
                    {
                        return;
                    }

                    var idx = listViewAudioClips.SelectedItems[0].Index;
                    if (_libMpv.IsPaused && _libMpv.CurrentPosition > _libMpv.Duration - 1)
                    {
                        _mpvDoneTimer.Stop();
                        PlayNext(idx);
                    }
                };
            }

            listViewAudioClips.AutoSizeLastColumn();
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Space || e.KeyCode == Keys.P || e.KeyCode == Keys.Play)
            {
                e.SuppressKeyPress = true;
                TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(1), () => Play());
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (listViewAudioClips.SelectedItems.Count == 0)
            {
                return;
            }

            var idx = listViewAudioClips.SelectedItems[0].Index;
            using (var form = new RegenerateAudioClip(_textToSpeech, _subtitle, idx))
            {
                var dr = form.ShowDialog(this);
                if (dr == DialogResult.OK)
                {
                    _fileNames[idx].Filename = form.FileNameAndSpeedFactor.Filename;
                    _fileNames[idx].Factor = form.FileNameAndSpeedFactor.Factor;
                    listViewAudioClips.Items[idx].SubItems[4].Text = $"{(form.FileNameAndSpeedFactor.Factor * 100.0m):0.#}%";
                    listViewAudioClips.Items[idx].SubItems[5].Text = _subtitle.Paragraphs[idx].Text;
                    Play(true);
                }
            }
        }

        private void exportListAsCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog { FileName = string.Empty, Filter = "CSV|*.csv" })
            {
                if (saveDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var csvContent = new StringBuilder();

                // Header
                csvContent.AppendLine("LineNumber,Voice,Cps,Speed,FileName,Text");

                for (var i = 0; i < _subtitle.Paragraphs.Count && i < _fileNames.Count; i++)
                {
                    var p = _subtitle.Paragraphs[i];
                    var pInfo = _fileNames[i];

                    var number = p.Number.ToString(CultureInfo.InvariantCulture);
                    var voice = _textToSpeech.GetParagraphAudio(p);
                    var cps = Utilities.GetCharactersPerSecond(p).ToString("0.#", CultureInfo.InvariantCulture);
                    var factor = "-";
                    if (pInfo.Factor != 1)
                    {
                        factor = $"{(pInfo.Factor * 100.0m):0.#}%";
                    }

                    csvContent.AppendLine($"{number},{voice},{cps},{factor},{pInfo.Filename},{p.Text}");
                }

                File.WriteAllText(saveDialog.FileName, csvContent.ToString(), Encoding.UTF8);
                UiUtil.OpenFolderFromFileName(saveDialog.FileName);
            }
        }
    }
}
