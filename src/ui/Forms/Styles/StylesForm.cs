using Nikse.SubtitleEdit.Core;
using System;
using System.Windows.Forms;

namespace Nikse.SubtitleEdit.Forms.Styles
{
    public /* abstract */ class StylesForm : Form
    {
        private readonly Timer _previewTimer = new Timer();

        private StylesForm()
        {
            // Only used by the Visual Studio designer.
        }

        protected StylesForm(Subtitle subtitle)
        {
            Subtitle = subtitle;

            _previewTimer.Interval = 200;
            _previewTimer.Tick += PreviewTimerTick;
        }

        public virtual string Header => throw new NotImplementedException("This property getter has to be overridden.");

        protected Subtitle Subtitle { get; }

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

        protected virtual void GeneratePreviewReal()
        {
            throw new NotImplementedException("This method has to be overridden.");
        }

        private void PreviewTimerTick(object sender, EventArgs e)
        {
            _previewTimer.Stop();
            GeneratePreviewReal();
        }
    }
}
