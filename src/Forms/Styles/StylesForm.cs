using System;
using System.ComponentModel;
using System.Windows.Forms;
using Nikse.SubtitleEdit.Core;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Forms.Styles
{
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<StylesForm, Form>))]
    public abstract class StylesForm : Form
    {
        private readonly Subtitle _subtitle;
        private readonly Timer _previewTimer = new Timer();

        protected StylesForm(Subtitle subtitle)
        {
            _subtitle = subtitle;

            _previewTimer.Interval = 200;
            _previewTimer.Tick += PreviewTimerTick;
        }

        public abstract string Header
        {
            get;
        }

        protected Subtitle Subtitle
        {
            get
            {
                return _subtitle;
            }
        }

        protected void GeneratePreview()
        {
            if (_previewTimer.Enabled)
            {
                _previewTimer.Stop();
                _previewTimer.Start();
            }
            else
            {
                _previewTimer.Start();
            }
        }

        protected abstract void GeneratePreviewReal();

        private void PreviewTimerTick(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            GeneratePreviewReal();
        }
    }
}
