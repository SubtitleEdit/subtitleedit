using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public partial class SetVideoOffset : PositionAndSizeForm
    {
        public bool FromCurrentVideoPosition { get; set; }

        private TimeCode _videoOffset = new TimeCode(0);
        public TimeCode VideoOffset
        {
            get
            {
                return _videoOffset;
            }
            set
            {
                _videoOffset.TotalMilliseconds = value.TotalMilliseconds;
                timeUpDownVideoPosition.SetTotalMilliseconds(value.TotalMilliseconds);
            }
        }

        public SetVideoOffset()
        {
            InitializeComponent();

            Text = Configuration.Settings.Language.SetVideoOffset.Title;
            labelDescription.Text = Configuration.Settings.Language.SetVideoOffset.Description;
            checkBoxFromCurrentPosition.Text = Configuration.Settings.Language.SetVideoOffset.RelativeToCurrentVideoPosition;
            buttonOK.Text = Configuration.Settings.Language.General.Ok;
            buttonCancel.Text = Configuration.Settings.Language.General.Cancel;
            Utilities.FixLargeFonts(this, buttonOK);
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            VideoOffset = timeUpDownVideoPosition.TimeCode;
            FromCurrentVideoPosition = checkBoxFromCurrentPosition.Checked;
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

    }
}
