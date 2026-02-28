using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic;
 
public class UiEbuSaveHelper : Ebu.IEbuUiHelper
{
    private byte _justificationCode = 2;

    public UiEbuSaveHelper()
    {
    }

    public void Initialize(Ebu.EbuGeneralSubtitleInformation header, byte justificationCode, string fileName, Subtitle subtitle)
    {
        _justificationCode = justificationCode;
    }

    public bool ShowDialogOk()
    {
        return true;
    }

    public byte JustificationCode
    {
        get => _justificationCode;
        set => _justificationCode = value;
    }
}
