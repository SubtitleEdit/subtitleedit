using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace SeConv.Core;

/// <summary>
/// No-op implementation of <see cref="Ebu.IEbuUiHelper"/> for headless EBU STL saves.
/// libse's <see cref="Ebu.Save(string, System.IO.Stream, Subtitle, bool, Ebu.EbuGeneralSubtitleInformation)"/>
/// requires a UI helper even in batch mode (used to read the active justification code).
/// </summary>
internal sealed class HeadlessEbuUiHelper : Ebu.IEbuUiHelper
{
    public byte JustificationCode { get; set; } = 2; // 2 = centered, the most common default

    public void Initialize(Ebu.EbuGeneralSubtitleInformation header, byte justificationCode, string fileName, Subtitle subtitle)
    {
        if (justificationCode != 0)
        {
            JustificationCode = justificationCode;
        }
    }

    public bool ShowDialogOk() => true;
}
