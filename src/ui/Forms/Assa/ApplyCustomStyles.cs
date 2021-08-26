using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.VideoPlayers;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Assa
{
    public sealed partial class ApplyCustomStyles : Form
    {
        public Subtitle UpdatedSubtitle { get; private set; }
        private readonly Subtitle _subtitle;
        private ListBox _intellisenseList;
        private readonly int[] _selectedIndices;
        private int[] _advancedIndices;
        private LibMpvDynamic _mpv;
        private string _mpvTextFileName;
        private const int PreviewHeight = 394;
        private readonly int _originalHeight;
        private Keys MainTextBoxAssaIntellisense { get; }
        private Keys MainTextBoxAssaRemoveTag { get; }
        private bool _closing;
        private bool _videoLoaded;

        public ApplyCustomStyles(Subtitle subtitle, int[] selectedIndices)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            _subtitle = subtitle;
            _selectedIndices = selectedIndices;
            _advancedIndices = new int[0];

            radioButtonSelectedLines.Checked = true;
            labelAdvancedSelection.Text = string.Empty;
            MainTextBoxAssaIntellisense = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxAssaIntellisense);
            MainTextBoxAssaRemoveTag = UiUtil.GetKeys(Configuration.Settings.Shortcuts.MainTextBoxAssaRemoveTag);

            radioButtonAdvancedSelection_CheckedChanged(null, null);
            Text = LanguageSettings.Current.AssaOverrideTags.ApplyCustomTags;
            buttonHistory.Text = LanguageSettings.Current.AssaOverrideTags.History;
            labelOverrideTags.Text = LanguageSettings.Current.AssaOverrideTags.TagsToApply;
            radioButtonAdvancedSelection.Text = LanguageSettings.Current.AssaOverrideTags.AdvancedSelection;
            radioButtonSelectedLines.Text = string.Format(LanguageSettings.Current.AssaOverrideTags.SelectedLinesX, _selectedIndices.Length);
            radioButtonAllLines.Text = LanguageSettings.Current.ShowEarlierLater.AllLines;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            groupBoxApplyTo.Text = LanguageSettings.Current.General.ApplyTo;
            buttonTogglePreview.Text = LanguageSettings.Current.General.ShowPreview;
            groupBoxPreview.Text = LanguageSettings.Current.General.Preview;
            UiUtil.FixLargeFonts(this, buttonOK);

            buttonHistory.Enabled = Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory.Count > 0;
            if (Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory.Count > 0)
            {
                seTextBox1.Text = Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory[0];
            }


            groupBoxPreview.Visible = false;
            _originalHeight = Height;
            MinimumSize = new Size(MinimumSize.Width, _originalHeight);

            buttonTogglePreview.Visible = LibMpvDynamic.IsInstalled;

            seTextBox1.TextChanged += (sender, args) =>
            {
                if (_videoLoaded)
                {
                    VideoLoaded(null, null);
                }
            };
        }

        private void ApplyCustomStyles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                if (_intellisenseList != null && _intellisenseList.Focused)
                {
                    return;
                }

                DialogResult = DialogResult.Cancel;
            }
            else if (e.KeyData == UiUtil.HelpKeys)
            {
                if (_intellisenseList != null && _intellisenseList.Focused)
                {
                    return;
                }

                UiUtil.OpenUrl("https://www.nikse.dk/SubtitleEdit/AssaOverrideTags");
                e.SuppressKeyPress = true;
            }
            else if (MainTextBoxAssaIntellisense == e.KeyData)
            {
                if (_intellisenseList != null && _intellisenseList.Focused)
                {
                    _intellisenseList.Hide();
                }
                else
                {
                    _intellisenseList = DoIntellisense(seTextBox1, _intellisenseList);
                }

                e.SuppressKeyPress = true;
            }
            else if (MainTextBoxAssaRemoveTag == e.KeyData)
            {
                AssaTagHelper.RemoveTagAtCursor(seTextBox1);
                e.SuppressKeyPress = true;
            }
        }

        private ListBox DoIntellisense(SETextBox tb, ListBox intellisenseListBox)
        {
            if (intellisenseListBox == null)
            {
                intellisenseListBox = new ListBox();
                Controls.Add(intellisenseListBox);
                intellisenseListBox.PreviewKeyDown += (o, args) =>
                {
                    if (args.KeyCode == Keys.Tab && intellisenseListBox.SelectedIndex >= 0)
                    {
                        if (intellisenseListBox.Items[intellisenseListBox.SelectedIndex] is AssaTagHelper.IntellisenseItem item)
                        {
                            AssaTagHelper.CompleteItem(tb, item);
                        }

                        intellisenseListBox.Hide();
                        System.Threading.SynchronizationContext.Current.Post(TimeSpan.FromMilliseconds(10), () => tb.Focus());
                    }
                };
                intellisenseListBox.KeyDown += (o, args) =>
                {
                    if (args.KeyCode == Keys.Enter && intellisenseListBox.SelectedIndex >= 0)
                    {
                        if (intellisenseListBox.Items[intellisenseListBox.SelectedIndex] is AssaTagHelper.IntellisenseItem item)
                        {
                            AssaTagHelper.CompleteItem(tb, item);
                        }

                        args.SuppressKeyPress = true;
                        args.Handled = true;
                        intellisenseListBox.Hide();
                        tb.Focus();
                    }
                    else if (args.KeyCode == Keys.Escape || MainTextBoxAssaIntellisense == args.KeyData)
                    {
                        args.SuppressKeyPress = true;
                        args.Handled = true;
                        intellisenseListBox.Hide();
                        tb.Focus();
                    }
                };
                intellisenseListBox.Click += (o, args) =>
                {
                    var index = intellisenseListBox.IndexFromPoint(((MouseEventArgs)args).Location);
                    if (index != ListBox.NoMatches)
                    {
                        if (intellisenseListBox.Items[index] is AssaTagHelper.IntellisenseItem item)
                        {
                            AssaTagHelper.CompleteItem(tb, item);
                        }

                        intellisenseListBox.Hide();
                        tb.Focus();
                    }
                };
                intellisenseListBox.KeyPress += (o, args) =>
                {
                    var x = args.KeyChar.ToString();
                    if (!string.IsNullOrEmpty(x) && x != "\r" && x != "\n" && x != "\u001b" && x != " ")
                    {
                        if (x == "{")
                        {
                            x = "{{}";
                        }
                        else if (x == "}")
                        {
                            x = "{}}";
                        }

                        tb.Focus();
                        SendKeys.SendWait(x);
                        args.Handled = true;
                        AssaTagHelper.AutoCompleteTextBox(tb, intellisenseListBox);
                        intellisenseListBox.Focus();
                    }
                };
                intellisenseListBox.LostFocus += (o, args) => intellisenseListBox.Hide();
            }

            if (AssaTagHelper.AutoCompleteTextBox(tb, intellisenseListBox))
            {
                var p = GetPositionInForm(tb);
                intellisenseListBox.Location = new Point(p.X + 10, p.Y + 30); //TODO: improve position
                intellisenseListBox.Height = 185;
                intellisenseListBox.Show();
                intellisenseListBox.BringToFront();
                intellisenseListBox.Focus();
            }

            return intellisenseListBox;
        }

        public Point GetPositionInForm(Control ctrl)
        {
            Point p = ctrl.Location;
            Control parent = ctrl.Parent;
            while (!(parent is Form))
            {
                p.Offset(parent.Location.X, parent.Location.Y);
                parent = parent.Parent;
            }
            return p;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(seTextBox1.Text))
            {
                return;
            }

            ApplyOverrideTags(_subtitle);
            DialogResult = DialogResult.OK;
        }

        private void ApplyOverrideTags(Subtitle subtitle)
        {
            var styleToApply = seTextBox1.Text;

            Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory = Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory
                .Where(p => p != styleToApply)
                .ToList();
            Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory.Insert(0, styleToApply);
            if (Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory.Count > 25)
            {
                Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory.RemoveAt(Configuration.Settings.SubtitleSettings.AssaOverrideTagHistory.Count - 1);
            }

            UpdatedSubtitle = new Subtitle(subtitle, false);
            var indices = GetIndices(subtitle);

            for (int i = 0; i < UpdatedSubtitle.Paragraphs.Count; i++)
            {
                if (!indices.Contains(i))
                {
                    continue;
                }

                Paragraph p = UpdatedSubtitle.Paragraphs[i];
                if (p.Text.StartsWith("{\\", StringComparison.Ordinal) && styleToApply.EndsWith('}'))
                {
                    p.Text = styleToApply.TrimEnd('}') + p.Text.Remove(0, 1);
                }
                else
                {
                    p.Text = styleToApply + p.Text;
                }
            }
        }

        private int[] GetIndices(Subtitle subtitle)
        {
            var indices = _selectedIndices;
            if (radioButtonAllLines.Checked)
            {
                indices = Enumerable.Range(0, subtitle.Paragraphs.Count).ToArray();
            }
            else if (radioButtonAdvancedSelection.Checked)
            {
                indices = _advancedIndices;
            }

            return indices;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonHistory_Click(object sender, EventArgs e)
        {
            using (var form = new TagHistory())
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                seTextBox1.Text = form.HistoryStyle;
                seTextBox1.Focus();
            }
        }

        private void buttonAdvancedSelection_Click(object sender, EventArgs e)
        {
            using (var form = new AdvancedSelectionHelper(_subtitle))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                _advancedIndices = form.Indices;
                labelAdvancedSelection.Text = string.Format(LanguageSettings.Current.Main.XLinesSelected, _advancedIndices.Length);
            }
        }

        private void radioButtonAdvancedSelection_CheckedChanged(object sender, EventArgs e)
        {
            buttonAdvancedSelection.Enabled = radioButtonAdvancedSelection.Checked;
            labelAdvancedSelection.Enabled = radioButtonAdvancedSelection.Checked;
        }

        private void buttonTogglePreview_Click(object sender, EventArgs e)
        {
            if (groupBoxPreview.Visible)
            {
                buttonTogglePreview.Text = LanguageSettings.Current.General.ShowPreview;
                MinimumSize = new Size(MinimumSize.Width, _originalHeight);
                Height = _originalHeight;
                groupBoxPreview.Visible = false;
            }
            else
            {
                MinimumSize = new Size(MinimumSize.Width, _originalHeight + PreviewHeight);
                buttonTogglePreview.Text = LanguageSettings.Current.General.HidePreview;
                Height = _originalHeight + PreviewHeight;
                groupBoxPreview.Height = buttonTogglePreview.Top - groupBoxPreview.Top - 10;
                groupBoxPreview.Visible = true;
                if (_mpv == null)
                {
                    GeneratePreviewViaMpv();
                }
            }
        }

        private bool GeneratePreviewViaMpv()
        {
            var fileName = VideoPreviewGenerator.GetVideoPreviewFileName();
            if (string.IsNullOrEmpty(fileName) || !LibMpvDynamic.IsInstalled)
            {
                return false;
            }

            if (_mpv == null)
            {
                _mpv = new LibMpvDynamic();
                _mpv.Initialize(pictureBoxPreview, fileName, VideoLoaded, null);
            }
            else
            {
                VideoLoaded(null, null);
            }

            return true;
        }

        private void VideoLoaded(object sender, EventArgs e)
        {
            var format = new AdvancedSubStationAlpha();
            var subtitle = new Subtitle();
            var indices = GetIndices(subtitle);
            var styleToApply = seTextBox1.Text;
            if (indices.Length > 0)
            {
                var p = new Paragraph(_subtitle.Paragraphs[indices[0]])
                {
                    StartTime = { TotalMilliseconds = 0 },
                    EndTime = { TotalMilliseconds = 2000 }
                };
                p.Text = styleToApply + p.Text;
                subtitle.Paragraphs.Add(p);
            }
            else
            {
                var p = new Paragraph(Configuration.Settings.General.PreviewAssaText, 0, 1000);
                p.Text = styleToApply + p.Text;
                subtitle.Paragraphs.Add(p);
            }

            subtitle.Header = _subtitle.Header ?? AdvancedSubStationAlpha.DefaultHeader;
            var text = subtitle.ToText(format);
            _mpvTextFileName = FileUtil.GetTempFileName(format.Extension);
            File.WriteAllText(_mpvTextFileName, text);
            _mpv.LoadSubtitle(_mpvTextFileName);
            _mpv.Play();

            if (!_videoLoaded)
            {
                timer1.Start();
                _videoLoaded = true;
            }
        }

        private void ApplyCustomStyles_FormClosing(object sender, FormClosingEventArgs e)
        {
            _closing = true;
            Application.DoEvents();
            _mpv?.Dispose();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_mpv == null || _closing)
            {
                return;
            }

            if (_mpv.IsPlaying)
            {
                return;
            }

            _mpv?.Stop();
            _mpv?.Play();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_intellisenseList != null && _intellisenseList.Focused)
            {
                _intellisenseList.Hide();
            }
            else
            {
                _intellisenseList = DoIntellisense(seTextBox1, _intellisenseList);
            }

            e.Cancel = true;
        }
    }
}
