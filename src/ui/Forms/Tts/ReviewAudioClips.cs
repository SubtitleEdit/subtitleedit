using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Collections.Generic;
using System.Globalization;
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
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            UiUtil.FixLargeFonts(this, buttonOK);

            _textToSpeech = textToSpeech;
            _subtitle = subtitle;
            _fileNames = fileNames;

            SkipIndices = new List<int>();

            listView1.BeginUpdate();
            foreach (var p in subtitle.Paragraphs)
            {
                var item = new ListViewItem { Tag = p, Checked = true };
                item.SubItems.Add(p.Number.ToString(CultureInfo.InvariantCulture));
                item.SubItems.Add(_textToSpeech.GetParagraphAudio(p));
                item.SubItems.Add(Utilities.GetCharactersPerSecond(p).ToString("0.#", CultureInfo.InvariantCulture));
                item.SubItems.Add(p.Text);
                listView1.Items.Add(item);
            }
            listView1.EndUpdate();

            if (listView1.Items.Count > 0)
            {
                listView1.Items[0].Selected = true;
            }

            labelParagraphInfo.Text = string.Empty;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            _libMpv?.Stop();
            for (var index = 0; index < listView1.Items.Count; index++)
            {
                if (!listView1.Items[index].Checked)
                {
                    SkipIndices.Add(index);
                }
            }

            DialogResult = DialogResult.OK;
        }

        private void VoicePreviewList_Load(object sender, EventArgs e)
        {
            listView1.AutoSizeLastColumn();
        }

        private void VoicePreviewList_ResizeEnd(object sender, EventArgs e)
        {
            listView1.AutoSizeLastColumn();
        }

        private void VoicePreviewList_Resize(object sender, EventArgs e)
        {
            listView1.AutoSizeLastColumn();
        }

        private void Play(bool noAutoContinue = false)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }

            var idx = listView1.SelectedItems[0].Index;
            var waveFileName = _fileNames[idx].Filename;

            if (_libMpv != null)
            {
                _libMpv.Initialize(
                    null,
                    waveFileName,
                    (sender, args) =>
                    {
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
            if (checkBoxContinuePlay.Checked && !_abortPlay && idx < listView1.Items.Count - 1)
            {
                listView1.Items[idx].Selected = false;
                listView1.Items[idx + 1].Selected = true;
                listView1.Items[idx + 1].Focused = true;
                listView1.Items[idx + 1].EnsureVisible();
                TaskDelayHelper.RunDelayed(TimeSpan.FromMilliseconds(10), () => Play());
                Application.DoEvents();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                labelParagraphInfo.Text = string.Empty;
                return;
            }

            var idx = listView1.SelectedItems[0].Index;
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
                    if (listView1.SelectedItems.Count == 0)
                    {
                        return;
                    }

                    var idx = listView1.SelectedItems[0].Index;
                    if (_libMpv.IsPaused && _libMpv.CurrentPosition > _libMpv.Duration - 1)
                    {
                        _mpvDoneTimer.Stop();
                        PlayNext(idx);
                    }
                };
            }
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
            if (listView1.SelectedItems.Count == 0)
            {
                return;
            }

            var idx = listView1.SelectedItems[0].Index;
            using (var form = new RegenerateAudioClip(_textToSpeech, _subtitle, idx))
            {
                var dr = form.ShowDialog(this);
                if (dr == DialogResult.OK)
                {
                    _fileNames[idx].Filename = form.FileNameAndSpeedFactor.Filename;
                    _fileNames[idx].Factor = form.FileNameAndSpeedFactor.Factor;
                    listView1.Items[idx].SubItems[4].Text = _subtitle.Paragraphs[idx].Text;
                    Play(true);
                }
            }
        }
    }
}
