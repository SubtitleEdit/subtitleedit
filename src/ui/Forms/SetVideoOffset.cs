using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        private void FillDefaultOffsets()
        {
            var secondsList = new List<long>();
            foreach (var ms in Configuration.Settings.General.DefaultVideoOffsetInSecondsList.Split(';'))
            {
                if (long.TryParse(ms, out var l))
                {
                    secondsList.Add(l);
                }
            }

            if (secondsList.Count == 0)
            {
                secondsList.Add(1 * 60 * 60);
                secondsList.Add(10 * 60 * 60);
            }

            foreach (var secs in secondsList.OrderBy(p => p))
            {
                var displayText = TimeCode.FromSeconds(secs).ToDisplayString();
                var item = new ToolStripMenuItem() { Text = displayText, Tag = secs };
                item.Click += Item_Click;
                contextMenuStripDefaultOffsets.Items.Add(item);
            }
        }

        private void Item_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                VideoOffset = TimeCode.FromSeconds((long)item.Tag);
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            VideoOffset = timeUpDownVideoPosition.TimeCode;
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
