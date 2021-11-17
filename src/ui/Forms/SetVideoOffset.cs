using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class SetVideoOffset : PositionAndSizeForm
    {
        public bool FromCurrentVideoPosition { get; set; }
        public bool DoNotaddVideoOffsetToTimeCodes { get; set; }
        public bool Reset { get; set; }

        private readonly TimeCode _videoOffset;

        public TimeCode VideoOffset
        {
            get => _videoOffset;
            set
            {
                _videoOffset.TotalMilliseconds = value.TotalMilliseconds;
                timeUpDownVideoPosition.SetTotalMilliseconds(value.TotalMilliseconds);
            }
        }

        public SetVideoOffset()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            _videoOffset = new TimeCode();
            checkBoxKeepTimeCodes.Checked = Configuration.Settings.Tools.VideoOffsetKeepTimeCodes;
            Text = LanguageSettings.Current.SetVideoOffset.Title;
            labelDescription.Text = LanguageSettings.Current.SetVideoOffset.Description;
            checkBoxFromCurrentPosition.Text = LanguageSettings.Current.SetVideoOffset.RelativeToCurrentVideoPosition;
            checkBoxKeepTimeCodes.Text = LanguageSettings.Current.SetVideoOffset.KeepTimeCodes;
            buttonReset.Text = LanguageSettings.Current.SetVideoOffset.Reset;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            FillDefaultOffsets();
            UiUtil.FixLargeFonts(this, buttonOK);
            buttonPickOffset.Left = timeUpDownVideoPosition.Left + timeUpDownVideoPosition.Width + 6;
        }

        private void FillDefaultOffsets()
        {
            var msList = new List<long>();
            foreach (var ms in Configuration.Settings.General.DefaultVideoOffsetInMsList.Split(';'))
            {
                if (long.TryParse(ms, out var l))
                {
                    msList.Add(l);
                }
            }

            if (msList.Count == 0)
            {
                msList.Add(1 * 60 * 60);
                msList.Add(10 * 60 * 60);
            }

            foreach (var ms in msList.OrderBy(p => p))
            {
                var displayText = TimeCode.FromSeconds(ms / 1000.0).ToDisplayString();
                var item = new ToolStripMenuItem() { Text = displayText, Tag = ms };
                item.Click += Item_Click;
                contextMenuStripDefaultOffsets.Items.Add(item);
            }
        }

        private void AddToDefaultOffsets(TimeCode timeCode)
        {
            var msList = new List<long>();
            var ms = (long)Math.Round(timeCode.TotalMilliseconds);
            foreach (var item in Configuration.Settings.General.DefaultVideoOffsetInMsList.Split(';'))
            {
                if (long.TryParse(item, out var l))
                {
                    if (l != ms)
                    {
                        msList.Add(l);
                    }
                }
            }

            msList.Add(ms);
            while (msList.Count > 10)
            {
                msList.RemoveAt(0);
            }

            var sb = new StringBuilder();
            foreach (var item in msList)
            {
                sb.Append(item.ToString(CultureInfo.InvariantCulture));
                sb.Append(";");
            }

            Configuration.Settings.General.DefaultVideoOffsetInMsList = sb.ToString().TrimEnd(';');
        }

        private void Item_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                VideoOffset = TimeCode.FromSeconds((long)item.Tag / 1000.0);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            VideoOffset = timeUpDownVideoPosition.TimeCode;
            AddToDefaultOffsets(VideoOffset);
            FromCurrentVideoPosition = checkBoxFromCurrentPosition.Checked;
            DoNotaddVideoOffsetToTimeCodes = checkBoxKeepTimeCodes.Checked;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            VideoOffset = new TimeCode();
            DoNotaddVideoOffsetToTimeCodes = checkBoxKeepTimeCodes.Checked;
            Reset = true;
            DialogResult = DialogResult.OK;
        }

        private void SetVideoOffset_FormClosing(object sender, FormClosingEventArgs e)
        {
            Configuration.Settings.Tools.VideoOffsetKeepTimeCodes = checkBoxKeepTimeCodes.Checked;
        }

        private void buttonPickOffset_Click(object sender, EventArgs e)
        {
            var coordinates = buttonPickOffset.PointToClient(Cursor.Position);
            contextMenuStripDefaultOffsets.Show(buttonPickOffset, coordinates);
        }
    }
}
