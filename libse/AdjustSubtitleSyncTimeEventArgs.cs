using Nikse.SubtitleEdit.Core;
using System;

namespace Nikse.SubtitleEdit.Forms
{
    public class AdjustSubtitleSyncTimeEventArgs : EventArgs
    {
        public TimeCode TimeCode { get; set; }
    }
}

