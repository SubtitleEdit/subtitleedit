using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
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
            UiUtil.FixLargeFonts(this, buttonOK);
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
    }
}
