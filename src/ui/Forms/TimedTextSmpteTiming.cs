using Nikse.SubtitleEdit.Logic;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms
{
    public sealed partial class TimedTextSmpteTiming : Form
    {
        public bool Always { get; private set; }
        public bool Never { get; private set; }

        public TimedTextSmpteTiming()
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);

            Text = LanguageSettings.Current.TimedTextSmpteTiming.Title;
            labelUseSmpteTiming.Text = LanguageSettings.Current.TimedTextSmpteTiming.UseSmpteTiming;
            labelInfo.Text = LanguageSettings.Current.TimedTextSmpteTiming.SmpteTimingInfo;
            buttonAlways.Text = LanguageSettings.Current.TimedTextSmpteTiming.YesAlways;
            buttonNever.Text = LanguageSettings.Current.TimedTextSmpteTiming.NoNever;
            buttonCancel.Text = LanguageSettings.Current.General.Cancel;
            buttonOK.Text = LanguageSettings.Current.General.Ok;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void ButtonAlwaysClick(object sender, EventArgs e)
        {
            Always = true;
            DialogResult = DialogResult.OK;
        }

        private void ButtonNeverClick(object sender, EventArgs e)
        {
            Never = true;
            DialogResult = DialogResult.Cancel;
        }
    }
}
