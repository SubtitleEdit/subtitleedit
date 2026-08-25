using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Logic;
 
/// <summary>
/// Carries the EBU STL save options that do not fit in the 1024-character GSI header from the
/// options dialog to <see cref="Ebu.Save(string,Subtitle)"/>. SE 4 could do without this because
/// the dialog itself was the save helper and wrote straight into the header object being saved;
/// here the dialog closes long before the save runs, so the leftovers are parked on the helper and
/// put back when the writer initializes.
/// </summary>
public class UiEbuSaveHelper : Ebu.IEbuUiHelper
{
    private byte _justificationCode = 2;
    private string? _frameRateHeader;
    private double _frameRate;

    public UiEbuSaveHelper()
    {
    }

    /// <summary>
    /// Remembers the frame rate picked in the save options dialog. It is tied to the header the
    /// dialog produced, so it is applied to that subtitle only - saving some other file must keep
    /// using the rate its own disk format code implies.
    /// </summary>
    public void SetFrameRate(string? header, double frameRate)
    {
        _frameRateHeader = header;
        _frameRate = frameRate;
    }

    /// <summary>
    /// The frame rate last picked for <paramref name="header"/>, if any - the options dialog uses it
    /// to show the rate the user chose rather than the one the disk format code implies.
    /// </summary>
    public bool TryGetFrameRate(string? header, out double frameRate)
    {
        frameRate = _frameRate;
        return _frameRate > 20 && header != null && header == _frameRateHeader;
    }

    public void Initialize(Ebu.EbuGeneralSubtitleInformation header, byte justificationCode, string fileName, Subtitle subtitle)
    {
        _justificationCode = justificationCode;

        // The frame rate is not part of the header bytes, so the writer has just lost it while
        // re-reading the header off the subtitle - put it back.
        if (header != null && TryGetFrameRate(subtitle?.Header, out var frameRate))
        {
            header.FrameRateFromSaveDialog = frameRate;
        }
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
